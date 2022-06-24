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
        private static IProgress<ConnectionStatus> Status { get; set; } = default!;
        private static IProgress<string[]> ActiveConnections { get; set; } = default!;
        private static IProgress<(int, int, int)> Labels { get; set; } = default!;
        private static IProgress<(string, bool)> Queue { get; set; } = default!;

        private List<string> UsersInQueue { get; set; } = new();
        private List<string> CurrentlyProcessed { get; set; } = new();
        private bool IsStopped { get; set; }

        private readonly SemaphoreSlim _semaphore;
        private int _entryID;

        public ServerConnection(ServerSettings settings, IProgress<ConnectionStatus> status, IProgress<string[]> connections, IProgress<(int, int, int)> labels, IProgress<(string, bool)> queue)
        {
            Settings = settings;
            Status = status;
            ActiveConnections = connections;
            Labels = labels;
            Queue = queue;
            UserQueue = new(settings.MaxQueue);
            _semaphore = new(settings.MaxConcurrent, settings.MaxConcurrent);

            Listener = new(IPAddress.Any, settings.Port);
            Listener.Server.ReceiveTimeout = 60_000;
            Listener.Server.SendTimeout = 60_000;
            Listener.Server.LingerState = new(true, 20);
        }

        internal class RemoteUser
        {
            public override string ToString() => $"{EntryID}. {(UserAuth is not null ? UserAuth.ToString() : "Unknown user")}";

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
            public int EntryID { get; set; }
        }

        internal class UserAuth
        {
            public override string ToString() => $"Host: {HostName} ({HostID}) | User: {SeedCheckerName} ({SeedCheckerID})";
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
                Status.Report(ConnectionStatus.NotConnected);
                await Task.Delay(0_050).ConfigureAwait(false);
            }
        }

        public async Task MainAsync(CancellationToken token)
        {
            Status.Report(ConnectionStatus.Connecting);
            Listener.Start(100);
            IsStopped = false;

            _ = Task.Run(async () => await RemoteUserQueue(token).ConfigureAwait(false), token);
            _ = Task.Run(async () => await ReportCurrentlyProcessed(token).ConfigureAwait(false), token);
            _ = Task.Run(async () => await UpdateLabels(token).ConfigureAwait(false), token);

            Status.Report(ConnectionStatus.Connected);
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

                if (pending)
                {
                    LogUtil.Log("A user is attempting to connect...", "[TCP Listener]");
                    _ = Task.Run(async () => await AcceptPendingConnection(token).ConfigureAwait(false), token);
                }
                await Task.Delay(0_200, token).ConfigureAwait(false);
            }
        }

        private async Task AcceptPendingConnection(CancellationToken token)
        {
            var remoteClient = await Listener.AcceptTcpClientAsync(token).ConfigureAwait(false);
            Settings.AddConnectionsAccepted();

            LogUtil.Log("A user has connected, authenticating the connection...", "[TCP Listener]");
            RemoteUser? user = await AuthenticateConnection(remoteClient).ConfigureAwait(false);
            if (user is null || !user.IsAuthenticated)
            {
                DisposeStream(user);
                return;
            }

            LogUtil.Log("Connection authenticated, attempting to authenticate the user...", "[TCP Listener]");
            user.UserAuth = await AuthenticateUser(user, token).ConfigureAwait(false);
            if (user.UserAuth is null)
            {
                await SendServerConfirmation(user, false, token).ConfigureAwait(false);
                DisposeStream(user);
                return;
            }
            Settings.AddUsersAuthenticated();

            bool enqueue = UserQueue.Count < UserQueue.BoundedCapacity;
            await SendServerConfirmation(user, enqueue, token).ConfigureAwait(false);

            if (enqueue)
            {
                LogUtil.Log($"{user.UserAuth.HostName} ({user.UserAuth.HostID}) was successfully authenticated, enqueueing...", "[TCP Listener]");

                // Increment queue entry ID.
                user.EntryID = Interlocked.Increment(ref _entryID);
                ReportUserQueue(user.ToString(), true);
                UserQueue.Add(user, token);
                return;
            }

            LogUtil.Log($"{user.UserAuth.HostName} ({user.UserAuth.HostID}) was successfully authenticated but the queue is full, closing the connection...", "[TCP Listener]");
            DisposeStream(user);
        }

        private async Task RemoteUserQueue(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await _semaphore.WaitAsync(token).ConfigureAwait(false);
                    var user = UserQueue.Take(token);
                    _ = Task.Run(async () => await RunEtumrepAsync(user, token).ConfigureAwait(false), token);
                }
                catch (Exception ex)
                {
                    LogUtil.Log($"Error occurred when queuing a user:\n{ex.Message}", "[User Queue]");
                    _semaphore.Release();
                }
            }
        }

        private async Task RunEtumrepAsync(RemoteUser user, CancellationToken token)
        {
            if (user.UserAuth is null)
            {
                LogUtil.Log("UserAuth is null: Something went very, very wrong.", "[User Queue]");
                DisposeStream(user);
                return;
            }

            var checker = $"{user.UserAuth.SeedCheckerName} ({user.UserAuth.SeedCheckerID})";
            CurrentlyProcessed.Add(checker);
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
                    return;
                }
            }
            catch (Exception ex)
            {
                LogUtil.Log($"{user.UserAuth.HostName}: Error occurred while reading data from {user.UserAuth.SeedCheckerName}.\n{ex.Message}", "[User Queue]");
                DisposeStream(user);
                return;
            }
            LogUtil.Log($"{user.UserAuth.HostName}: Beginning seed calculation for {user.UserAuth.SeedCheckerName}...", "[User Queue]");

            var list = EtumrepUtil.GetPokeList(user.Buffer, count);
            var sw = new Stopwatch();

            sw.Start();
            var seed = EtumrepUtil.CalculateSeed(list);
            sw.Stop();

            Settings.AddEtumrepsRun();
            LogUtil.Log($"{user.UserAuth.HostName}: Seed ({seed}) calculation for {user.UserAuth.SeedCheckerName} complete ({sw.Elapsed}). Attempting to send the result...", "[User Queue]");

            var bytes = BitConverter.GetBytes(seed);
            try
            {
                ReportUserQueue(user.ToString(), false);
                _semaphore.Release();
                await user.Stream.WriteAsync(bytes, token).ConfigureAwait(false);
                LogUtil.Log($"{user.UserAuth.HostName}: Results were sent, removing from queue.", "[User Queue]");
            }
            catch (Exception ex)
            {
                LogUtil.Log($"{user.UserAuth.HostName}: Error occurred while sending results to {user.UserAuth.SeedCheckerName}.\n{ex.Message}", "[User Queue]");
            }

            CurrentlyProcessed.Remove(checker);
            DisposeStream(user);
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

        private async Task ReportCurrentlyProcessed(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(0_100, token).ConfigureAwait(false);
                var msg = CurrentlyProcessed.Count is 0 ? new string[] { "Waiting for users..." } : CurrentlyProcessed.ToArray();
                ActiveConnections.Report(msg);
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

        private void ReportUserQueue(string name, bool insert)
        {
            if (!insert)
                UsersInQueue.Remove(name);
            else UsersInQueue.Add(name);

            Queue.Report((name, insert));
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
