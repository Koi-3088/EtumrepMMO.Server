using System.ComponentModel;

namespace EtumrepMMO.Server
{
    public class ServerSettings
    {
        public override string ToString() => "Server Settings";
        private const string Startup = nameof(Startup);
        private const string User = nameof(User);
        private const string Counts = nameof(Counts);

        [Category(Startup), Description("Port.")]
        public int Port { get; set; } = 80;

        [Category(Startup), Description("Token for client authorization.")]
        public string Token { get; set; } = string.Empty;

        [Category(Startup), Description("Maximum EtumrepMMO queue size. Server will deny connections until the queue is below this value.")]
        public int MaxQueue { get; set; } = 10;

        [Category(Startup), Description("Maximum concurrent EtumrepMMO instances to run.")]
        public int MaxConcurrent { get; set; } = 2;

        [Category(Startup), Description("Whitelisted clients (bot hosts).")]
        public List<DiscordUser> HostWhitelist { get; set; } = new();

        [Category(Startup), Description("Blacklisted users.")]
        public List<DiscordUser> UserBlacklist { get; set; } = new();

        [Category(User), Description("Discord user object.")]
        public class DiscordUser
        {
            public override string ToString() => $"{Username}";

            [Category(User), Description("Discord user's username.")]
            public string Username { get; set; } = string.Empty;

            [Category(User), Description("Discord user's numerical ID.")]
            public ulong ID { get; set; }

            [Category(User), Description("Discord user's password.")]
            public string Password { get; set; } = string.Empty;
        }

        private int _connectionsAccepted;
        private int _usersAuthenticated;
        private int _etumrepsRun;

        [Category(Counts), Description("Connections accepted.")]
        public int ConnectionsAccepted
        {
            get => _connectionsAccepted;
            set => _connectionsAccepted = value;
        }

        [Category(Counts), Description("Users authenticated.")]
        public int UsersAuthenticated
        {
            get => _usersAuthenticated;
            set => _usersAuthenticated = value;
        }

        [Category(Counts), Description("EtumrepMMOs successfully run.")]
        public int EtumrepsRun
        {
            get => _etumrepsRun;
            set => _etumrepsRun = value;
        }

        public void AddConnectionsAccepted() => Interlocked.Increment(ref _connectionsAccepted);
        public void AddUsersAuthenticated() => Interlocked.Increment(ref _usersAuthenticated);
        public void AddEtumrepsRun() => Interlocked.Increment(ref _etumrepsRun);
    }
}
