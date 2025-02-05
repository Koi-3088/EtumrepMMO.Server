using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Text;
using System.Diagnostics;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using PKHeX.Core;

namespace EtumrepMMO.Server;

public class ServerConnection
{
    private ServerSettings Settings { get; }
    private TcpListener Listener { get; set; }
    private BlockingCollection<RemoteUser> UserQueue_SeedFinder { get; }
    private BlockingCollection<RemoteUser> UserQueue_Z3 { get; }
    private IProgress<ConnectionStatus> Status { get; }
    private IProgress<(string, bool)> ConcurrentQueue { get; }
    private IProgress<(string, bool)> Queue { get; }
    private bool IsStopped { get; set; }

    private readonly SemaphoreSlim _semaphore_SeedFinder;
    private readonly SemaphoreSlim _semaphore_Z3;
    private int _entryID;

    private const int DefaultTimeout = 60_000; // 60 seconds

    public ServerConnection(ServerSettings settings, IProgress<ConnectionStatus> status, IProgress<(string, bool)> concurrent, IProgress<(string, bool)> queue)
    {
        Settings = settings;
        Status = status;
        ConcurrentQueue = concurrent;
        Queue = queue;
        UserQueue_SeedFinder = new(settings.MaxQueue);
        UserQueue_Z3 = [];
        _semaphore_SeedFinder = new(settings.MaxConcurrent, settings.MaxConcurrent);
        _semaphore_Z3 = new(1, 1);

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

    private async Task Reconnect()
    {
        await Stop().ConfigureAwait(false);
        Status.Report(ConnectionStatus.Connecting);
        Listener = new(IPAddress.Any, Settings.Port)
        {
            Server =
            {
                ReceiveTimeout = DefaultTimeout,
                SendTimeout = DefaultTimeout,
                LingerState = new(true, 20),
            },
        };

        Listener.Start(100);
        Status.Report(ConnectionStatus.Connected);
        IsStopped = false;
        LogUtil.Log("TCP Listener was restarted. Waiting for connections...", "[TCP Listener]");
    }

    public async Task MainAsync(CancellationToken token)
    {
        Status.Report(ConnectionStatus.Connecting);
        Listener.Start(100);
        IsStopped = false;

        _ = Task.Run(async () => await PlaSeedFinderQueue(token).ConfigureAwait(false), token);
        _ = Task.Run(async () => await Z3Queue(token).ConfigureAwait(false), token);
        Status.Report(ConnectionStatus.Connected);
        LogUtil.Log("Server initialized. Waiting for connections...", "[TCP Listener]");

        while (!token.IsCancellationRequested)
        {
            try
            {
                if (Listener.Pending())
                    _ = Task.Run(async () => await AcceptPendingConnection(token).ConfigureAwait(false), token);
                await Task.Delay(0_200, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (ex is not OperationCanceledException)
                {
                    LogUtil.Log($"TCP Listener has crashed. Trying to restart the connection.\n{ex.Message}\n{ex.InnerException}", "[TCP Listener]");
                    await Reconnect().ConfigureAwait(false);
                }
            }
        }
    }

    private async Task AcceptPendingConnection(CancellationToken token)
    {
        LogUtil.Log("A user is attempting to connect...", "[TCP Listener]");
        var remoteClient = await Listener.AcceptTcpClientAsync(token).ConfigureAwait(false);
        Settings.AddConnectionsAccepted();

        LogUtil.Log("A user has connected. Authenticating the connection...", "[TCP Listener]");
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

        LogUtil.Log("Connection authenticated. Attempting to authenticate the user...", "[TCP Listener]");
        var auth = await AuthenticateUser(user, token).ConfigureAwait(false);
        if (auth is null)
        {
            await SendServerConfirmation(user, false, token).ConfigureAwait(false);
            DisposeStream(user, auth);
            return;
        }
        Settings.AddUsersAuthenticated();

        bool enqueue = UserQueue_SeedFinder.Count < UserQueue_SeedFinder.BoundedCapacity;
        await SendServerConfirmation(user, enqueue, token).ConfigureAwait(false);

        if (enqueue)
        {
            LogUtil.Log($"{user.UserAuth.HostName} ({user.UserAuth.HostID}) was successfully authenticated. Queueing...", "[TCP Listener]");

            // Increment queue entry ID.
            user.EntryID = Interlocked.Increment(ref _entryID);
            ReportUserQueue(user.ToString(), true);
            UserQueue_SeedFinder.Add(user, token);
            return;
        }

        LogUtil.Log($"{user.UserAuth.HostName} ({user.UserAuth.HostID}) was successfully authenticated but the queue is full. Closing the connection...", "[TCP Listener]");
        DisposeStream(user);
    }

    private async Task PlaSeedFinderQueue(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                await _semaphore_SeedFinder.WaitAsync(token).ConfigureAwait(false);
                var user = UserQueue_SeedFinder.Take(token);
                _ = Task.Run(async () => await RunSeedFinderAsync(user, token).ConfigureAwait(false), token);
            }
            catch (Exception ex)
            {
                if (ex is not OperationCanceledException)
                {
                    LogUtil.Log($"Error occurred when queuing a user:\n{ex.Message}\n{ex.InnerException}", "[User Queue Z3]");
                }

                _semaphore_SeedFinder.Release();
            }
        }
    }

    private async Task Z3Queue(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                await _semaphore_Z3.WaitAsync(token).ConfigureAwait(false);
                var user = UserQueue_Z3.Take(token);
                await RunZ3Async(user, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (ex is not OperationCanceledException)
                {
                    LogUtil.Log($"Error occurred when queuing a user:\n{ex.Message}\n{ex.InnerException}", "[User Queue Z3]");
                }

                _semaphore_Z3.Release();
            }
        }
    }

    private async Task RunSeedFinderAsync(RemoteUser user, CancellationToken token)
    {
        var checker = $"{user.EntryID}. {user.UserAuth.SeedCheckerName} ({user.UserAuth.SeedCheckerID})";
        ReportCurrentlyProcessed(checker, true);
        LogUtil.Log($"{user.UserAuth.HostName}: Attempting to read PKM data from {user.UserAuth.SeedCheckerName}.", "[SeedFinder Queue]");

        async Task<bool> SeedFinderFunc()
        {
            int count;
            try
            {
                int read = await user.Stream.ReadAsync(user.Buffer, token).ConfigureAwait(false);
                count = read / EtumrepUtil.SIZE;

                if (read % EtumrepUtil.SIZE != 0 || count is < 2 or > 4)
                {
                    LogUtil.Log($"{user.UserAuth.HostName}: Received an incorrect amount of data from {user.UserAuth.SeedCheckerName}.", "[SeedFinder Queue]");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogUtil.Log($"{user.UserAuth.HostName}: Error occurred while reading data from {user.UserAuth.SeedCheckerName}.\n{ex.Message}\n{ex.InnerException}", "[SeedFinder Queue]");
                return false;
            }
            LogUtil.Log($"{user.UserAuth.HostName}: Beginning seed calculation for {user.UserAuth.SeedCheckerName}...", "[SeedFinder Queue]");

            var sw = new Stopwatch();
            IReadOnlyList<(PKM[], (ulong, byte)[])> seeds;
            try
            {
                var list = EtumrepUtil.GetPokeList(user.Buffer, count).ToArray();
                if (list.Length >= 1)
                {
                    sw.Start();
                    seeds = EtumrepUtil.GetSeeds(list);
                    sw.Stop();
                }
                else throw new Exception($"Too much malformed data received from {checker}. Dequeueing...");
            }
            catch (Exception ex)
            {
                LogUtil.Log($"{user.UserAuth.HostName}: Error occurred while calculating seed for {user.UserAuth.SeedCheckerName}.\n{ex.Message}\n{ex.InnerException}", "[SeedFinder Queue]");
                sw.Reset();
                return false;
            }

            if (seeds.Count is 0)
            {
                try
                {
                    var bytes = BitConverter.GetBytes(0);
                    LogUtil.Log($"{user.UserAuth.HostName}: No seeds were found, sending response.", "[SeedFinder Queue]");
                    await user.Stream.WriteAsync(bytes, token).ConfigureAwait(false);
                    return false;
                }
                catch (Exception ex)
                {
                    LogUtil.Log($"{user.UserAuth.HostName}: Error occurred while sending response to {user.UserAuth.SeedCheckerName}.\n{ex.Message}\n{ex.InnerException}", "[SeedFinder Queue]");
                    return false;
                }
            }

            LogUtil.Log($"{user.UserAuth.HostName}: PLA-SeedFinder calculation for {user.UserAuth.SeedCheckerName} complete ({sw.Elapsed}).", "[SeedFinder Queue]");
            user.SeedFinderResult = seeds;
            UserQueue_Z3.Add(user, token);
            return true;
        }

        try
        {
            if (!await SeedFinderFunc().ConfigureAwait(false))
            {
                ReportUserQueue(user.ToString(), false);
                ReportCurrentlyProcessed(checker, false);
                DisposeStream(user);
            }
        }
        catch (Exception ex)
        {
            LogUtil.Log($"{user.UserAuth.HostName}: Error occurred while processing {user.UserAuth.SeedCheckerName}.\n{ex.Message}\n{ex.InnerException}", "[SeedFinderFunc]");
            ReportUserQueue(user.ToString(), false);
            ReportCurrentlyProcessed(checker, false);
            DisposeStream(user);
        }

        _semaphore_SeedFinder.Release();
    }

    private async Task RunZ3Async(RemoteUser user, CancellationToken token)
    {
        var checker = $"{user.EntryID}. {user.UserAuth.SeedCheckerName} ({user.UserAuth.SeedCheckerID})";
        LogUtil.Log($"{user.UserAuth.HostName}: Attempting to run Z3 for final seed calculation for {user.UserAuth.SeedCheckerName}.", "[Z3 Queue]");

        async Task Z3Func()
        {
            var seeds = user.SeedFinderResult!;
            var sw = new Stopwatch();
            ulong seed;
            try
            {
                sw.Start();
                seed = EtumrepUtil.CalculateSeed(seeds);
                sw.Stop();
            }
            catch (Exception ex)
            {
                LogUtil.Log($"{user.UserAuth.HostName}: Error occurred while calculating seed for {user.UserAuth.SeedCheckerName}.\n{ex.Message}", "[Z3 Queue]");
                sw.Reset();
                return;
            }

            LogUtil.Log($"{user.UserAuth.HostName}: Seed ({seed}) calculation for {user.UserAuth.SeedCheckerName} complete ({sw.Elapsed}). Attempting to send the result...", "[Z3 Queue]");
            var bytes = BitConverter.GetBytes(seed);
            try
            {
                await user.Stream.WriteAsync(bytes, token).ConfigureAwait(false);
                LogUtil.Log($"{user.UserAuth.HostName}: Results were sent, removing from queue.", "[Z3 Queue]");
            }
            catch (Exception ex)
            {
                LogUtil.Log($"{user.UserAuth.HostName}: Error occurred while sending results to {user.UserAuth.SeedCheckerName}.\n{ex.Message}\n{ex.InnerException}", "[Z3 Queue]");
            }
        }

        try
        {
            await Z3Func().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogUtil.Log($"{user.UserAuth.HostName}: Error occurred while processing {user.UserAuth.SeedCheckerName}.\n{ex.Message}\n{ex.InnerException}", "[Z3 Queue]");
        }

        Settings.AddEtumrepsRun();
        ReportUserQueue(user.ToString(), false);
        ReportCurrentlyProcessed(checker, false);
        DisposeStream(user);
        _semaphore_Z3.Release();
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
            LogUtil.Log($"Failed to authenticate user.\n{ex.Message}\n{ex.InnerException}", "[Connection Authentication]");
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
            LogUtil.Log($"Failed to read user authentication data.\n{ex.Message}\n{ex.InnerException}", "[User Authentication]");
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
        user.UserAuth = authObj;
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
            LogUtil.Log($"Failed to send response to user.\n{ex.Message}\n{ex.InnerException}", "[SendServerConfirmation]");
        }
    }

    private void ReportUserQueue(string name, bool insert) => Queue.Report((name, insert));
    private void ReportCurrentlyProcessed(string name, bool insert) => ConcurrentQueue.Report((name, insert));

    private static void DisposeStream(RemoteUser user, UserAuth? auth = null)
    {
        try
        {
            user.Client.Close();
            user.Stream.Close();
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
