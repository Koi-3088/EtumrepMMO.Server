using System.ComponentModel;

namespace Etumrep.Server
{
    public class HostSettings
    {
        public override string ToString() => "Host Settings";
        private const string Startup = nameof(Startup);

        [Category(Startup), Description("Host port.")]
        public int Port { get; set; } = 6969;

        [Category(Startup), Description("Token.")]
        public string Token { get; set; } = string.Empty;

        [Category(Startup), Description("Whitelisted bot hosts' numerical Discord user IDs.")]
        public List<string> HostWhitelist { get; set; } = new();

        [Category(Startup), Description("Blacklisted users' numerical Discord IDs.")]
        public List<string> UserBlacklist { get; set; } = new();
    }
}
