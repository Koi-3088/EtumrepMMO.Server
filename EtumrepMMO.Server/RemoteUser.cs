using System.Net.Security;
using System.Net.Sockets;
using PKHeX.Core;

namespace EtumrepMMO.Server;

internal class RemoteUser
{
    public override string ToString() => $"{EntryID}. {UserAuth}";

    internal RemoteUser(TcpClient client, NetworkStream stream)
    {
        Client = client;
        Stream = stream;
    }

    public TcpClient Client { get; }
    public NetworkStream Stream { get; }
    public UserAuth UserAuth { get; set; } = new();
    public byte[] Buffer { get; } = new byte[EtumrepUtil.MAXCOUNT * EtumrepUtil.SIZE];

    public bool IsAuthenticated { get; set; }
    public int EntryID { get; set; }

    public IReadOnlyList<(PKM[], (ulong, byte)[])>? SeedFinderResult { get; set; }
}
