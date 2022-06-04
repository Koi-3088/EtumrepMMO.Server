using System.ComponentModel;

namespace EtumrepMMO.Server
{
    public class HostSettings
    {
        public override string ToString() => "Host Settings";
        private const string Startup = nameof(Startup);

        [Category(Startup), Description("Host port.")]
        public int Port { get; set; } = 80;

        [Category(Startup), Description("Token for client authorization.")]
        public string Token { get; set; } = string.Empty;

        [Category(Startup), Description("Whitelisted clients' numerical Discord user IDs.")]
        public List<string> HostWhitelist { get; set; } = new();

        [Category(Startup), Description("Blacklisted users' numerical Discord IDs.")]
        public List<string> UserBlacklist { get; set; } = new();
    }
}
