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
        private TcpListener Listener { get; set; }
        private BlockingCollection<RemoteUser> UserQueue { get; }
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
            Listener.Server.ReceiveTimeout = 60_000;
            Listener.Server.LingerState = new(true, 20);
            UserQueue = new(settings.MaxQueue);
        }

        internal class RemoteUser
        {
            internal RemoteUser(TcpClient client, AuthenticatedStream stream)
            {
                Client = client;
                Stream = stream;
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
            public string HostPassword { get; set; } = string.Empty;
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
                bool pending;
                try
                {
                    pending = Listener.Pending();
                }
                catch (Exception ex)
                {
                    LogUtil.Log($"TCP Listener has crashed, trying to restart the connection.\n{ex.Message}", "[TCP Listener]");
                    Listener = new(IPAddress.Any, Settings.Port);
                    Listener.Server.LingerState = new(true, 20);

                    pending = false;
                    LogUtil.Log("TCP Listener was restarted, waiting for connections...", "[TCP Listener]");
                }

                if (!pending)
                {
                    await Task.Delay(0_250, token).ConfigureAwait(false);
                    continue;
                }

                LogUtil.Log("A user is attempting to connect...", "[TCP Listener]");
                var remoteClient = await Listener.AcceptTcpClientAsync(token).ConfigureAwait(false);
                Settings.ConnectionsAccepted += 1;

                LogUtil.Log("A user has connected, authenticating the connection...", "[TCP Listener]");
                RemoteUser? user = await AuthenticateConnection(remoteClient).ConfigureAwait(false);
                if (user is null || !user.IsAuthenticated)
                {
                    DisposeStream(user);
                    continue;
                }

                LogUtil.Log("Connection authenticated, attempting to authenticate the user...", "[TCP Listener]");
                user.UserAuth = await AuthenticateUser(user, token).ConfigureAwait(false);
                if (user.UserAuth is null)
                {
                    await SendServerConfirmation(user, false, token).ConfigureAwait(false);
                    DisposeStream(user);
                    continue;
                }
                Settings.UsersAuthenticated += 1;

                bool enqueue = UserQueue.Count < UserQueue.BoundedCapacity;
                await SendServerConfirmation(user, enqueue, token).ConfigureAwait(false);

                if (enqueue)
                {
                    LogUtil.Log($"{user.UserAuth.HostName} ({user.UserAuth.HostID}) was successfully authenticated, enqueueing...", "[TCP Listener]");
                    UserQueue.Add(user, token);
                    continue;
                }

                LogUtil.Log($"{user.UserAuth.HostName} ({user.UserAuth.HostID}) was successfully authenticated but the queue is full, closing the connection...", "[TCP Listener]");
                DisposeStream(user);
            }
        }

        private async Task RemoteUserQueue(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var user = UserQueue.Take(token);
                if (user is not null && user.UserAuth is not null)
                {
                    CurrentSeedChecker = user.UserAuth.SeedCheckerName;
                    Progress = 10;
                    InQueue = true;

                    LogUtil.Log($"{user.UserAuth.HostName}: Attempting to read PKM data from {user.UserAuth.SeedCheckerName}.", "[User Queue]");
                    int read, count;
                    try
                    {
                        read = await user.Stream.ReadAsync(user.Buffer, token).ConfigureAwait(false);
                        count = read / 376;

                        if (read is 0 || count is < 2 || count is > 4)
                        {
                            LogUtil.Log($"{user.UserAuth.HostName}: Received an incorrect amount of data from {user.UserAuth.SeedCheckerName}.", "[User Queue]");
                            DisposeStream(user);
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogUtil.Log($"{user.UserAuth.HostName}: Error occurred while reading data from {user.UserAuth.SeedCheckerName}.\n{ex.Message}", "[User Queue]");
                        DisposeStream(user);
                        continue;
                    }

                    Progress = 30;
                    LogUtil.Log($"{user.UserAuth.HostName}: Beginning seed calculation for {user.UserAuth.SeedCheckerName}...", "[User Queue]");

                    var list = EtumrepUtil.GetPokeList(user.Buffer, count);
                    var sw = new Stopwatch();

                    sw.Start();
                    var seed = EtumrepUtil.CalculateSeed(list);
                    sw.Stop();

                    Progress = 80;
                    Settings.EtumrepsRun += 1;
                    LogUtil.Log($"{user.UserAuth.HostName}: Seed ({seed}) calculation for {user.UserAuth.SeedCheckerName} complete ({sw.Elapsed}). Attempting to send the result...", "[User Queue]");

                    var bytes = BitConverter.GetBytes(seed);
                    try
                    {
                        await user.Stream.WriteAsync(bytes, token).ConfigureAwait(false);
                        LogUtil.Log($"{user.UserAuth.HostName}: Results were sent, removing from queue.", "[User Queue]");
                        Progress = 100;
                    }
                    catch (Exception ex)
                    {
                        LogUtil.Log($"{user.UserAuth.HostName}: Error occurred while sending results to {user.UserAuth.SeedCheckerName}.\n{ex.Message}", "[User Queue]");
                    }

                    DisposeStream(user);
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
                stream.Socket.ReceiveTimeout = 60_000;
                stream.Socket.SendTimeout = 60_000;

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
            UserAuth? authObj = null;
            try
            {
                byte[] authBytes = new byte[688];
                await user.Stream.ReadAsync(authBytes, token).ConfigureAwait(false);
                var text = Encoding.Unicode.GetString(authBytes);
                authObj = JsonConvert.DeserializeObject<UserAuth>(text);
            }
            catch (Exception ex)
            {
                LogUtil.Log($"Failed to read user authentication data.\n{ex.Message}", "[User Authentication]");
                return null;
            }

            if (authObj is null)
            {
                LogUtil.Log("User did not send an authentication packet.", "[User Authentication]");
                return null;
            }
            else if (!Settings.HostWhitelist.Exists(x => x.ID == authObj.HostID && x.Password == authObj.HostPassword))
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

            await SendServerConfirmation(user, true, token).ConfigureAwait(false);
            return authObj;
        }

        private static async Task SendServerConfirmation(RemoteUser user, bool confirmed, CancellationToken token)
        {
            try
            {
                var bytes = BitConverter.GetBytes(confirmed);
                await user.Stream.WriteAsync(bytes.AsMemory(0, 1), token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogUtil.Log($"Failed to send response to user.\n{ex.Message}", "[SendServerConfirmation]");
            }
        }

        private async Task ReportUserProgress(CancellationToken token)
        {
            string msg;
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(0_100, token).ConfigureAwait(false);
                var count = UserQueue.Count;

                if (InQueue)
                {
                    msg = $"{CurrentSeedChecker} | {count} {(count is 1 ? "user" : "users")} waiting.";
                    RemoteUser.Report(msg, Progress, true);
                    continue;
                }

                msg = $"Waiting for users... | {count} {(count is 1 ? "user" : "users")} waiting.";
                RemoteUser.Report(msg, Progress, true);
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
                try
                {
                    user.Client.Close();
                    user.Stream.Dispose();
                }
                catch (Exception ex)
                {
                    string msg = string.Empty;
                    if (user.UserAuth is not null)
                        msg = $"{user.UserAuth.HostName}: ";

                    msg += $"Error occurred while disposing the connection stream.\n{ex.Message}";
                    LogUtil.Log(msg, "[DisposeStream]");
                }
            }
        }
    }
}
