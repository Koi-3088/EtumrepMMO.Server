using EtumrepMMO.Server;
using Newtonsoft.Json;

namespace EtumrepMMO.WinForms
{
    public sealed partial class Main : Form
    {
        private readonly ServerConnection Connection;
        private readonly string ConfigPath = GetConfigPath();

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

            Connection = new(Settings, prg, labels);
            Grid_Settings.SelectedObject = Settings;
            LogUtil.Forwarders.Add(PostLog);
        }

        private void Main_Closing(object sender, FormClosingEventArgs e)
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

        private void UpdateLog(string line)
        {
            if (RTB_Logs.Lines.Length > 50_000)
                RTB_Logs.Lines = Array.Empty<string>();

            RTB_Logs.AppendText(line);
            RTB_Logs.ScrollToCaret();
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
