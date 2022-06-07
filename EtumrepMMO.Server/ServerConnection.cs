using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Text;
using System.Diagnostics;
using System.Collections.Concurrent;
using Newtonsoft.Json;

namespace EtumrepMMO.Server
{
    public class ServerConnection
    {
        private ServerSettings Settings { get; }
        private TcpListener Listener { get; }
        private ConcurrentQueue<RemoteUser> UserQueue { get; set; } = new();
        private static IProgress<(string, int, bool)> UserProgress { get; set; } = default!;
        private static IProgress<(int, int, int)> Labels { get; set; } = default!;

        private string CurrentSeedChecker { get; set; } = "Waiting for users...";
        private bool IsStopped { get; set; }
        private int Progress { get; set; }
        private bool InQueue { get; set; }

        public ServerConnection(ServerSettings settings, IProgress<(string, int, bool)> progress, IProgress<(int, int, int)> labels)
        {
            Settings = settings;
            UserProgress = progress;
            Labels = labels;
            Listener = new(IPAddress.Any, settings.Port);
        }

        internal class RemoteUser
        {
            internal RemoteUser(TcpClient client, AuthenticatedStream stream)
            {
                Client = client;
                Stream = stream;
                Stream.ReadTimeout = 2_000;
                Stream.WriteTimeout = 2_000;
            }

            public TcpClient Client { get; }
            public AuthenticatedStream Stream { get; }
            public UserAuth? UserAuth { get; set; } = new();
            public bool IsAuthenticated { get; set; }
            public byte[] Buffer { get; } = new byte[1504];

            public static void Report(string name, int load, bool visible) => UserProgress.Report((name, load, visible));
        }

        internal class UserAuth
        {
            public string HostName { get; set; } = string.Empty;
            public ulong HostID { get; set; }
            public string Token { get; set; } = string.Empty;
            public string SeedCheckerName { get; set; } = string.Empty;
            public ulong SeedCheckerID { get; set; }
        }

        public async Task Stop()
        {
            if (!IsStopped)
            {
                LogUtil.Log("Stopping the TCP Listener...", "[TCP Listener]");
                Listener.Stop();
                IsStopped = true;
                await Task.Delay(0_050).ConfigureAwait(false);
            }
        }

        public async Task MainAsync(CancellationToken token)
        {
            Listener.Start(100);
            IsStopped = false;

            _ = Task.Run(async () => await RemoteUserQueue(token).ConfigureAwait(false), token);
            _ = Task.Run(async () => await ReportUserProgress(token).ConfigureAwait(false), token);
            _ = Task.Run(async () => await UpdateLabels(token).ConfigureAwait(false), token);
            LogUtil.Log("Server initialized, waiting for connections...", "[TCP Listener]");

            while (!token.IsCancellationRequested)
            {
                if (!Listener.Pending())
                {
                    await Task.Delay(0_250, token).ConfigureAwait(false);
                    continue;
                }

                LogUtil.Log("A user is attempting to connect, authenticating connection...", "[TCP Listener]");
                TcpClient remoteClient = await Listener.AcceptTcpClientAsync(token).ConfigureAwait(false);
                Settings.ConnectionsAccepted += 1;
                LogUtil.Log("A user has connected, authenticating the user...", "[TCP Listener]");

                RemoteUser? user = await AuthenticateConnection(remoteClient).ConfigureAwait(false);
                if (user is null || !user.IsAuthenticated)
                {
                    DisposeStream(user);
                    continue;
                }

                user.UserAuth = await AuthenticateUser(user, token).ConfigureAwait(false);
                if (user.UserAuth is null)
                {
                    DisposeStream(user);
                    continue;
                }
                Settings.UsersAuthenticated += 1;

                LogUtil.Log($"{user.UserAuth.HostName} {(user.UserAuth.HostID)} was successfully authenticated, enqueueing...", "[TCP Listener]");
                UserQueue.Enqueue(user);
            }
        }

        private async Task RemoteUserQueue(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                UserQueue.TryDequeue(out var user);
                if (user is not null && user.UserAuth is not null)
                {
                    CurrentSeedChecker = user.UserAuth.SeedCheckerName;
                    Progress = 10;
                    InQueue = true;

                    LogUtil.Log($"{user.UserAuth.HostName}: Attempting to read PKM data from {user.UserAuth.SeedCheckerName}.", "[User Queue]");
                    if (!user.Stream.CanRead)
                    {
                        LogUtil.Log($"{user.UserAuth.HostName}: Unable to read stream.", "[User Queue]");
                        DisposeStream(user);
                        continue;
                    }

                    int read = await user.Stream.ReadAsync(user.Buffer, token).ConfigureAwait(false);
                    int count = read / 376;

                    if (read is 0 || count is < 2)
                    {
                        LogUtil.Log($"{user.UserAuth.HostName}: Received an incorrect amount of data from {user.UserAuth.SeedCheckerName}.", "[User Queue]");
                        DisposeStream(user);
                        continue;
                    }

                    Progress = 30;
                    LogUtil.Log($"{user.UserAuth.HostName}: Beginning seed calculation for {user.UserAuth.SeedCheckerName}...", "[User Queue]");

                    var sw = new Stopwatch();
                    sw.Start();
                    var seed = EtumrepUtil.CalculateSeed(user.Buffer, count);
                    sw.Stop();

                    Progress = 80;
                    LogUtil.Log($"{user.UserAuth.HostName}: Seed ({seed}) calculation for {user.UserAuth.SeedCheckerName} complete ({sw.Elapsed}). Attempting to send the result...", "[User Queue]");

                    if (!user.Stream.CanWrite)
                    {
                        LogUtil.Log($"{user.UserAuth.HostName}: Unable to write to stream.", "[User Queue]");
                        DisposeStream(user);
                        continue;
                    }

                    var bytes = BitConverter.GetBytes(seed);
                    await user.Stream.WriteAsync(bytes, token).ConfigureAwait(false);
                    Settings.EtumrepsRun += 1;
                    LogUtil.Log($"{user.UserAuth.HostName}: Results were sent, removing from queue.", "[User Queue]");

                    DisposeStream(user);
                    Progress = 100;
                    await Task.Delay(0_250, token).ConfigureAwait(false);
                }
                else
                {
                    InQueue = false;
                    await Task.Delay(0_250, token).ConfigureAwait(false);
                }
            }
        }

        private static async Task<RemoteUser?> AuthenticateConnection(TcpClient client)
        {
            try
            {
                var stream = client.GetStream();
                var authStream = new NegotiateStream(stream, false);
                var user = new RemoteUser(client, authStream);

                await authStream.AuthenticateAsServerAsync().ConfigureAwait(false);
                user.IsAuthenticated = true;
                LogUtil.Log("Initial authentication complete.", "[Connection Authentication]");
                return user;
            }
            catch (Exception ex)
            {
                LogUtil.Log($"Failed to authenticate user.\n{ex.Message}", "[Connection Authentication]");
                return null;
            }
        }

        private async Task<UserAuth?> AuthenticateUser(RemoteUser user, CancellationToken token)
        {
            if (!user.Stream.CanRead)
            {
                LogUtil.Log("Cannot read from stream.", "[User Authentication]");
                return null;
            }

            byte[] authBytes = new byte[1024];
            await user.Stream.ReadAsync(authBytes, token).ConfigureAwait(false);
            var text = Encoding.Unicode.GetString(authBytes);
            var authObj = JsonConvert.DeserializeObject<UserAuth>(text);

            if (authObj is null)
            {
                LogUtil.Log("User did not send an authentication packet.", "[User Authentication]");
                return null;
            }
            else if (!Settings.HostWhitelist.Exists(x => x.ID == authObj.HostID))
            {
                LogUtil.Log($"{authObj.HostName} ({authObj.HostID}) is not a whitelisted bot host.", "[User Authentication]");
                return null;
            }
            else if (Settings.UserBlacklist.Exists(x => x.ID == authObj.SeedCheckerID))
            {
                LogUtil.Log($"{authObj.SeedCheckerName} ({authObj.SeedCheckerID}) is a blacklisted user.", "[User Authentication]");
                return null;
            }
            else if (Settings.Token != authObj.Token)
            {
                LogUtil.Log($"The provided token ({authObj.Token}) does not match the token defined by us.", "[User Authentication]");
                return null;
            }

            // Send confirmation to client.
            var bytes = BitConverter.GetBytes(true);
            await user.Stream.WriteAsync(bytes, token).ConfigureAwait(false);
            return authObj;
        }

        private async Task ReportUserProgress(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(0_100, token).ConfigureAwait(false);
                if (InQueue)
                {
                    var c = UserQueue.Count;
                    var msg = $"{CurrentSeedChecker} | {c} {(c is 1 ? "user" : "users")} waiting.";
                    RemoteUser.Report(msg, Progress, true);
                }
                else CurrentSeedChecker = "Waiting for users...";
            }
        }

        private async Task UpdateLabels(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(0_100, token).ConfigureAwait(false);
                Labels.Report((Settings.ConnectionsAccepted, Settings.UsersAuthenticated, Settings.EtumrepsRun));
            }
        }

        private static void DisposeStream(RemoteUser? user)
        {
            if (user is not null)
            {
                user.Client.Dispose();
                user.Stream.Dispose();
            }
        }
    }
}
