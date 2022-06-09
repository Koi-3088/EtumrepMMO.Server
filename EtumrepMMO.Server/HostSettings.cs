using System.ComponentModel;

namespace EtumrepMMO.Server
{
    public class ServerSettings
    {
        public override string ToString() => "Server Settings";
        private const string Startup = nameof(Startup);
        private const string User = nameof(User);

        [Category(Startup), Description("Port.")]
        public int Port { get; set; } = 80;

        [Category(Startup), Description("Token for client authorization.")]
        public string Token { get; set; } = string.Empty;

        [Category(Startup), Description("Whitelisted clients (bot hosts).")]
        public List<DiscordUser> HostWhitelist { get; set; } = new();

        [Category(Startup), Description("Blacklisted users.")]
        public List<DiscordUser> UserBlacklist { get; set; } = new();

        [Category(Startup), Description("Connections accepted.")]
        public int ConnectionsAccepted { get; set; }

        [Category(Startup), Description("Users authenticated.")]
        public int UsersAuthenticated { get; set; }

        [Category(Startup), Description("EtumrepMMOs successfully run.")]
        public int EtumrepsRun { get; set; }

        [Category(User), Description("Discord user object.")]
        public class DiscordUser
        {
            public override string ToString() => $"{Username}";

            [Category(User), Description("Discord user's username.")]
            public string Username { get; set; } = string.Empty;

            [Category(User), Description("Discord user's numerical ID.")]
            public ulong ID { get; set; }
        }
    }
}
