using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Text;
using System.Diagnostics;
using System.Collections.Concurrent;
using Newtonsoft.Json;

namespace EtumrepMMO.Server;

public class ServerConnection
{
    private ServerSettings Settings { get; }
    private TcpListener Listener { get; set; }
    private BlockingCollection<RemoteUser> UserQueue { get; }
    private IProgress<ConnectionStatus> Status { get; }
    private IProgress<(string, bool)> ConcurrentQueue { get; }
    private IProgress<(int, int, int)> Labels { get; }
    private IProgress<(string, bool)> Queue { get; }
    private bool IsStopped { get; set; }

    private readonly SemaphoreSlim _semaphore;
    private int _entryID;

    private const int DefaultTimeout = 60_000; // 60 seconds

    public ServerConnection(ServerSettings settings, IProgress<ConnectionStatus> status, IProgress<(string, bool)> concurrent, IProgress<(int, int, int)> labels, IProgress<(string, bool)> queue)
    {
        Settings = settings;
        Status = status;
        ConcurrentQueue = concurrent;
        Labels = labels;
        Queue = queue;
        UserQueue = new(settings.MaxQueue);
        _semaphore = new(settings.MaxConcurrent, settings.MaxConcurrent);

        Listener = new(IPAddress.Any, settings.Port)
        {
            Server =
            {
                ReceiveTimeout = DefaultTimeout,
                SendTimeout = DefaultTimeout,
                LingerState = new(true, 20),
            },
        };
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
                Listener = new(IPAddress.Any, Settings.Port)
                {
                    Server =
                    {
                        LingerState = new(true, 20),
                    },
                };

                pending = false;
                LogUtil.Log("TCP Listener was restarted, waiting for connections...", "[TCP Listener]");
            }

            if (pending)
                _ = Task.Run(async () => await AcceptPendingConnection(token).ConfigureAwait(false), token);
            await Task.Delay(0_200, token).ConfigureAwait(false);
        }
    }

    private async Task AcceptPendingConnection(CancellationToken token)
    {
        LogUtil.Log("A user is attempting to connect...", "[TCP Listener]");
        var remoteClient = await Listener.AcceptTcpClientAsync(token).ConfigureAwait(false);
        Settings.AddConnectionsAccepted();

        LogUtil.Log("A user has connected, authenticating the connection...", "[TCP Listener]");
        RemoteUser? user = await AuthenticateConnection(remoteClient).ConfigureAwait(false);
        if (user is null)
        {
            remoteClient.Dispose();
            return;
        }
        if (!user.IsAuthenticated)
        {
            DisposeStream(user);
            return;
        }

        LogUtil.Log("Connection authenticated, attempting to authenticate the user...", "[TCP Listener]");
        var auth = await AuthenticateUser(user, token).ConfigureAwait(false);
        if (auth is null)
        {
            await SendServerConfirmation(user, false, token).ConfigureAwait(false);
            DisposeStream(user, auth);
            return;
        }
        Settings.AddUsersAuthenticated();

        bool enqueue = UserQueue.Count < UserQueue.BoundedCapacity;
        await SendServerConfirmation(user, enqueue, token).ConfigureAwait(false);

        if (enqueue)
        {
            LogUtil.Log($"{user.UserAuth.HostName} ({user.UserAuth.HostID}) was successfully authenticated, queueing...", "[TCP Listener]");

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
        var checker = $"{user.EntryID}. {user.UserAuth.SeedCheckerName} ({user.UserAuth.SeedCheckerID})";
        ReportCurrentlyProcessed(checker, true);
        LogUtil.Log($"{user.UserAuth.HostName}: Attempting to read PKM data from {user.UserAuth.SeedCheckerName}.", "[User Queue]");

        async Task EtumrepFunc()
        {
            int count;
            try
            {
                int read = await user.Stream.ReadAsync(user.Buffer, token).ConfigureAwait(false);
                count = read / EtumrepUtil.SIZE;

                if (count is not (2 or 3 or 4))
                {
                    LogUtil.Log($"{user.UserAuth.HostName}: Received an incorrect amount of data from {user.UserAuth.SeedCheckerName}.", "[User Queue]");
                    return;
                }
            }
            catch (Exception ex)
            {
                LogUtil.Log($"{user.UserAuth.HostName}: Error occurred while reading data from {user.UserAuth.SeedCheckerName}.\n{ex.Message}", "[User Queue]");
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
                await user.Stream.WriteAsync(bytes, token).ConfigureAwait(false);
                LogUtil.Log($"{user.UserAuth.HostName}: Results were sent, removing from queue.", "[User Queue]");
            }
            catch (Exception ex)
            {
                LogUtil.Log($"{user.UserAuth.HostName}: Error occurred while sending results to {user.UserAuth.SeedCheckerName}.\n{ex.Message}", "[User Queue]");
            }
        }

        try
        {
            await EtumrepFunc().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogUtil.Log($"{user.UserAuth.HostName}: Error occurred while processing {user.UserAuth.SeedCheckerName}.\n{ex.Message}", "[EtumrepFunc]");
        }

        ReportCurrentlyProcessed(checker, false);
        _semaphore.Release();
        DisposeStream(user);
    }

    private static async Task<RemoteUser?> AuthenticateConnection(TcpClient client)
    {
        try
        {
            var stream = client.GetStream();
            stream.Socket.ReceiveTimeout = DefaultTimeout;
            stream.Socket.SendTimeout = DefaultTimeout;

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
        UserAuth? authObj;
        try
        {
            byte[] authBytes = new byte[688];
            _ = await user.Stream.ReadAsync(authBytes, token).ConfigureAwait(false);
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
        if (!Settings.HostWhitelist.Exists(x => x.ID == authObj.HostID && x.Password == authObj.HostPassword))
        {
            LogUtil.Log($"{authObj.HostName} ({authObj.HostID}) is not a whitelisted bot host.", "[User Authentication]");
            return null;
        }
        if (Settings.UserBlacklist.Exists(x => x.ID == authObj.SeedCheckerID))
        {
            LogUtil.Log($"{authObj.SeedCheckerName} ({authObj.SeedCheckerID}) is a blacklisted user.", "[User Authentication]");
            return null;
        }
        if (Settings.Token != authObj.Token)
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

    private async Task UpdateLabels(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await Task.Delay(0_100, token).ConfigureAwait(false);
            Labels.Report((Settings.ConnectionsAccepted, Settings.UsersAuthenticated, Settings.EtumrepsRun));
        }
    }

    private void ReportUserQueue(string name, bool insert) => Queue.Report((name, insert));
    private void ReportCurrentlyProcessed(string name, bool insert) => ConcurrentQueue.Report((name, insert));

    private static void DisposeStream(RemoteUser user, UserAuth? auth = null)
    {
        try
        {
            user.Client.Close();
            user.Stream.Dispose();
        }
        catch (Exception ex)
        {
            string msg = string.Empty;
            if ((auth ?? user.UserAuth) is { } x)
                msg = $"{x.HostName}: ";

            msg += $"Error occurred while disposing the connection stream.\n{ex.Message}";
            LogUtil.Log(msg, "[DisposeStream]");
        }
    }
}
