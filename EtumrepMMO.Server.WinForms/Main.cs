using Newtonsoft.Json;

namespace EtumrepMMO.Server.WinForms;

public sealed partial class Main : Form
{
    private readonly ServerConnection Connection;
    private readonly ServerSettings Settings;
    private const string ConfigPath = "config.json";
    private readonly object _logLock = new();
    private readonly object _queueLock = new();
    private readonly object _concurrentLock = new();

    private CancellationTokenSource Source { get; set; } = new();
    private static bool WasStarted { get; set; }

    private const string _waiting = "Waiting for users...";
    private const string _noQueue = "No users in queue...";
    private const string _connectionsText = "Connections accepted: {0}";
    private const string _authText = "Users authenticated: {0}";
    private const string _etumrepText = "EtumrepMMOs run: {0}";

    public Main()
    {
        InitializeComponent();
        if (File.Exists(ConfigPath))
        {
            var text = File.ReadAllText(ConfigPath);
            Settings = JsonConvert.DeserializeObject<ServerSettings>(text, GetSettings()) ?? new ServerSettings();
        }
        else
        {
            Settings = new();
        }

        var status = new Progress<ConnectionStatus>(UpdateStatusLamp);
        var concurrent = new Progress<(string, bool)>(x => UpdateCurrentlyProcessed(x.Item1, x.Item2));
        var queue = new Progress<(string, bool)>(x => UpdateQueue(x.Item1, x.Item2));

        UpdateCurrentlyProcessed(_waiting, false);
        UpdateQueue(_noQueue, false);

        RTB_Logs.MaxLength = 32_767;
        Connection = new(Settings, status, concurrent, queue);
        Grid_Settings.SelectedObject = Settings;
        LogUtil.Forwarders.Add(PostLog);
    }

    private void Main_FormClosing(object sender, FormClosingEventArgs e)
    {
        WindowState = FormWindowState.Minimized;
        Stop();
    }

    private void Button_Start_Click(object sender, EventArgs e)
    {
        if (WasStarted)
            return;

        WasStarted = true;
        Tab_Logs.Select();
        RunServer();
    }

    private void Button_Stop_Click(object sender, EventArgs e)
    {
        if (WasStarted)
            Stop();
    }

    private void Stop()
    {
        SaveSettings();
        Source.Cancel();

        async Task WaitUntilDone()
        {
            await Connection.Stop().ConfigureAwait(false);
            Source = new();
            WasStarted = false;
            lock (_concurrentLock)
            {
                LV_Concurrent.Items.Clear();
                LV_Concurrent.Items.Add("Waiting for users...");
                LV_QueueList.Items.Clear();
            }
        }
        Task.WhenAny(WaitUntilDone(), Task.Delay(1_000)).ConfigureAwait(true).GetAwaiter().GetResult();
        LogUtil.Log("Server has been shut down.", "[Stop Button Event]");
    }

    private void PostLog(string message, string identity)
    {
        var line = $"[{DateTime.Now:HH:mm:ss}] - {identity}: {message}{Environment.NewLine}";
        if (InvokeRequired)
            Invoke((MethodInvoker)(() => UpdateLog(line)));
        else UpdateLog(line);
    }

    // Taken from kwsch's SysBot
    // https://github.com/kwsch/SysBot.NET/commit/27455c4d88f1f9df7dc94dd0e76f3a9bb44b6242
    private void UpdateLog(string line)
    {
        lock (_logLock)
        {
            // ghetto truncate
            var rtb = RTB_Logs;
            var text = rtb.Text;
            var max = rtb.MaxLength;
            if (text.Length + line.Length + 2 >= max)
                rtb.Text = text[(max / 4)..];

            rtb.AppendText(line);
            rtb.ScrollToCaret();
        }
    }

    private void RunServer()
    {
        var token = Source.Token;
        _ = Task.Run(async () => await Connection.MainAsync(token), token);
    }

    private static JsonSerializerSettings GetSettings() => new()
    {
        Formatting = Formatting.Indented,
        DefaultValueHandling = DefaultValueHandling.Include,
        NullValueHandling = NullValueHandling.Ignore,
    };

    private void SaveSettings()
    {
        var lines = JsonConvert.SerializeObject(Settings, GetSettings());
        File.WriteAllText(ConfigPath, lines);
    }

    private void UpdateStatusLamp(ConnectionStatus status) => PB_Ready.BackColor = status switch
    {
        ConnectionStatus.Connecting => Color.Wheat,
        ConnectionStatus.Connected => Color.LawnGreen,
        _ => Color.WhiteSmoke,
    };

    private void UpdateQueue(string text, bool insert)
    {
        lock (_queueLock)
        {
            var item = LV_QueueList.FindItemWithText(_noQueue);
            LV_QueueList.Items.Remove(item);

            if (insert)
            {
                LV_QueueList.Items.Add(text);
            }
            else
            {
                item = LV_QueueList.FindItemWithText(text);
                LV_QueueList.Items.Remove(item);

                if (LV_QueueList.Items.Count is 0)
                    LV_QueueList.Items.Add(_noQueue);
            }
        }
    }

    private void UpdateCurrentlyProcessed(string text, bool insert)
    {
        lock (_concurrentLock)
        {
            var item = LV_Concurrent.FindItemWithText(_waiting);
            LV_Concurrent.Items.Remove(item);

            if (insert)
            {
                LV_Concurrent.Items.Add(text);
            }
            else
            {
                item = LV_Concurrent.FindItemWithText(text);
                LV_Concurrent.Items.Remove(item);

                if (LV_Concurrent.Items.Count is 0)
                    LV_Concurrent.Items.Add(_waiting);
            }
        }
    }
}
