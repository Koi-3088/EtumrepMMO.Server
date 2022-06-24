using Newtonsoft.Json;

namespace EtumrepMMO.Server.WinForms
{
    public sealed partial class Main : Form
    {
        private readonly ServerConnection Connection;
        private readonly string ConfigPath = GetConfigPath();
        private readonly object _logLock = new();
        private readonly object _labelLock = new();
        private readonly object _queueLock = new();

        private static CancellationTokenSource Source { get; set; } = new();
        private ServerSettings Settings { get; set; }
        private static bool WasStarted { get; set; }

        public Main()
        {
            InitializeComponent();
            if (File.Exists(ConfigPath))
            {
                var text = File.ReadAllText(ConfigPath);
                Settings = JsonConvert.DeserializeObject<ServerSettings>(text, GetSettings()) ?? new ServerSettings();
                UpdateLabels(Settings.ConnectionsAccepted, Settings.UsersAuthenticated, Settings.EtumrepsRun);
            }
            else Settings = new();

            var status = new Progress<ConnectionStatus>(x =>
            {
                UpdateStatusLamp(x);
            });

            var conn = new Progress<string[]>(x =>
            {
                TB_ActiveConnections.Lines = x;
            });

            var labels = new Progress<(int, int, int)>(x =>
            {
                UpdateLabels(x.Item1, x.Item2, x.Item3);
            });

            var queue = new Progress<(string, bool)>(x =>
            {
                UpdateQueueList(x.Item1, x.Item2);
            });

            RTB_Logs.MaxLength = 32_767;
            Connection = new(Settings, status, conn, labels, queue);
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
                TB_ActiveConnections.Text = "Waiting for users...";
                LV_QueueList.Items.Clear();
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

        private static string GetConfigPath() => "config.json";

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
            _ => Color.WhiteSmoke
        };

        private void UpdateLabels(int connections, int authentications, int etumreps)
        {
            lock (_labelLock)
            {
                Label_Connections.Text = connections > 0 ? $"Connections accepted: {connections}" : string.Empty;
                Label_Authenticated.Text = authentications > 0 ? $"Users authenticated: {authentications}" : string.Empty;
                Label_Etumreps.Text = etumreps > 0 ? $"EtumrepMMOs run: {etumreps}" : string.Empty;
            }
        }

        private void UpdateQueueList(string text, bool insert)
        {
            lock (_queueLock)
            {
                if (insert)
                    LV_QueueList.Items.Add(text);
                else
                {
                    var item = LV_QueueList.FindItemWithText(text);
                    LV_QueueList.Items.Remove(item);
                }
            }
        }
    }
}
