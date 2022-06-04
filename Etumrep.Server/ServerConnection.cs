using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Text;
using System.Diagnostics;
using System.Collections.Concurrent;
using Newtonsoft.Json;

namespace Etumrep.Server
{
    public class ServerConnection
    {
        private int Port { get; }
        private string Token { get; }
        private TcpListener Listener { get; }
        private bool IsStopped { get; set; }
        private List<string> HostWhitelist { get; }
        private List<string> UserBlacklist { get; }
        private string CurrentSeedChecker { get; set; } = "Waiting for users...";
        private int Progress { get; set; }
        private bool InQueue { get; set; }
        private static IProgress<(string, int, bool)> UserProgress { get; set; } = default!;
        private ConcurrentQueue<RemoteUser> UserQueue { get; set; } = new();

        public ServerConnection(HostSettings settings, IProgress<(string, int, bool)> progress)
        {
            Port = settings.Port;
            Token = settings.Token;
            HostWhitelist = settings.HostWhitelist;
            UserBlacklist = settings.UserBlacklist;
            UserProgress = progress;
            Listener = new(IPAddress.Any, Port);
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
            public string Name { get; set; } = string.Empty;
            public string SeedCheckerName { get; set; } = string.Empty;
            public bool IsAuthenticated { get; set; }
            public byte[] Buffer { get; } = new byte[1504];

            public static void Report(string name, int load, bool visible) => UserProgress.Report((name, load, visible));
        }

        internal class UserAuth
        {
            public string HostName { get; set; } = string.Empty;
            public string HostID { get; set; } = string.Empty;
            public string Token { get; set; } = string.Empty;
            public string SeedCheckerName { get; set; } = string.Empty;
            public string SeedCheckerID { get; set; } = string.Empty;
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
            LogUtil.Log("Server initialized, waiting for connections...", "[TCP Listener]");

            while (!token.IsCancellationRequested)
            {
                if (!Listener.Pending() || UserQueue.Count >= 100)
                {
                    await Task.Delay(0_250, token).ConfigureAwait(false);
                    continue;
                }

                LogUtil.Log("A user is attempting to connect, authenticating connection...", "[TCP Listener]");
                TcpClient remoteClient = await Listener.AcceptTcpClientAsync(token).ConfigureAwait(false);
                LogUtil.Log("A user has connected, authenticating the user...", "[TCP Listener]");

                RemoteUser user = await AuthenticateConnection(remoteClient).ConfigureAwait(false);
                if (!user.IsAuthenticated)
                {
                    DisposeStream(user);
                    continue;
                }

                var userAuth = await AuthenticateUser(user, token).ConfigureAwait(false);
                if (userAuth is null)
                {
                    DisposeStream(user);
                    continue;
                }

                LogUtil.Log($"{user.Name} was successfully authenticated, enqueueing...", "[TCP Listener]");
                UserQueue.Enqueue(user);
            }
        }

        private async Task RemoteUserQueue(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                UserQueue.TryDequeue(out var user);
                if (user is not null)
                {
                    CurrentSeedChecker = user.SeedCheckerName;
                    Progress = 10;
                    InQueue = true;

                    LogUtil.Log($"{user.Name}: Attempting to read PKM data from {user.SeedCheckerName}.", "[UserQueue]");
                    if (!user.Stream.CanRead)
                    {
                        LogUtil.Log($"{user.Name}: Unable to read stream.", "[UserQueue]");
                        DisposeStream(user);
                        continue;
                    }

                    int read = await user.Stream.ReadAsync(user.Buffer, token).ConfigureAwait(false);
                    int count = read / 376;

                    if (read is 0 || count is < 2)
                    {
                        LogUtil.Log($"{user.Name}: Received an incorrect amount of data from {user.SeedCheckerName}.", "[UserQueue]");
                        DisposeStream(user);
                        continue;
                    }

                    Progress = 30;
                    LogUtil.Log($"{user.Name}: Beginning seed calculation for {user.SeedCheckerName}...", "[UserQueue]");

                    var sw = new Stopwatch();
                    sw.Start();
                    var seed = EtumrepUtil.CalculateSeed(user.Buffer, count);
                    sw.Stop();

                    Progress = 80;
                    LogUtil.Log($"{user.Name}: Seed ({seed}) calculation for {user.SeedCheckerName} complete ({sw.Elapsed}). Attempting to send the result...", "[UserQueue]");

                    if (!user.Stream.CanWrite)
                    {
                        LogUtil.Log($"{user.Name}: Unable to write to stream.", "[UserQueue]");
                        DisposeStream(user);
                        continue;
                    }

                    var bytes = BitConverter.GetBytes(seed);
                    await user.Stream.WriteAsync(bytes, token).ConfigureAwait(false);

                    LogUtil.Log($"{user.Name}: Sent results to {user.Name}, removing from queue.", "[UserQueue]");

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

        private static async Task<RemoteUser> AuthenticateConnection(TcpClient client)
        {
            var stream = client.GetStream();
            var authStream = new NegotiateStream(stream, false);
            var user = new RemoteUser(client, authStream);

            try
            {
                await authStream.AuthenticateAsServerAsync().ConfigureAwait(false);
                user.IsAuthenticated = true;
                LogUtil.Log("Initial authentication complete.", "[Connection Authentication]");
                return user;
            }
            catch (Exception ex)
            {
                LogUtil.Log($"Failed to authenticate user.\n{ex.Message}", "[Connection Authentication]");
                return user;
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
            else if (!HostWhitelist.Contains(authObj.HostID))
            {
                LogUtil.Log($"{authObj.HostName} ({authObj.HostID}) is not a whitelisted bot host.", "[User Authentication]");
                return null;
            }
            else if (UserBlacklist.Contains(authObj.SeedCheckerID))
            {
                LogUtil.Log($"{authObj.SeedCheckerName} ({authObj.SeedCheckerID}) is a blacklisted user.", "[User Authentication]");
                return null;
            }
            else if (authObj.Token != Token)
            {
                LogUtil.Log($"The provided token ({authObj.Token}) does not match the token defined by us.", "[User Authentication]");
                return null;
            }

            user.Name = authObj.HostName;
            user.SeedCheckerName = authObj.SeedCheckerName;

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

        private static void DisposeStream(RemoteUser user)
        {
            user.Client.Dispose();
            user.Stream.Dispose();
        }
    }
}
