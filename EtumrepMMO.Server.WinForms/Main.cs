using Newtonsoft.Json;

namespace EtumrepMMO.Server.WinForms
{
    public sealed partial class Main : Form
    {
        private readonly ServerConnection Connection;
        private readonly string ConfigPath = GetConfigPath();
        private readonly object _logLock = new();

        private static CancellationTokenSource Source { get; set; } = new();
        private ServerSettings Settings { get; set; }
        private static bool WasStarted { get; set; }

        public Main()
        {
            InitializeComponent();
            var prg = new Progress<(string, int, bool)>(x =>
            {
                ActiveConnection.Text = x.Item1;
                Bar_Load.Value = x.Item2;
                ActiveConnection.Visible = x.Item3;
                Bar_Load.Visible = x.Item3;
            });

            if (File.Exists(ConfigPath))
            {
                var text = File.ReadAllText(ConfigPath);
                Settings = JsonConvert.DeserializeObject<ServerSettings>(text, GetSettings()) ?? new ServerSettings();
                UpdateLabels(Settings.ConnectionsAccepted, Settings.UsersAuthenticated, Settings.EtumrepsRun);
            }
            else Settings = new();

            var labels = new Progress<(int, int, int)>(x =>
            {
                UpdateLabels(x.Item1, x.Item2, x.Item3);
            });

            RTB_Logs.MaxLength = 32_767;
            Connection = new(Settings, prg, labels);
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

        private void UpdateLabels(int connections, int authentications, int etumreps)
        {
            Connections.Text = connections > 0 ? $"Connections accepted: {connections}" : string.Empty;
            Authenticated.Text = authentications > 0 ? $"Users authenticated: {authentications}" : string.Empty;
            Etumreps.Text = etumreps > 0 ? $"EtumrepMMOs run: {etumreps}" : string.Empty;
        }
    }
}
