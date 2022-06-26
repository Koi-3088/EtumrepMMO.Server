namespace EtumrepMMO.Server;

[Serializable]
internal class UserAuth
{
    public override string ToString() => $"Host: {HostName} ({HostID}) | User: {SeedCheckerName} ({SeedCheckerID})";
    public string HostName { get; set; } = string.Empty;
    public ulong HostID { get; set; }
    public string HostPassword { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string SeedCheckerName { get; set; } = string.Empty;
    public ulong SeedCheckerID { get; set; }
}
