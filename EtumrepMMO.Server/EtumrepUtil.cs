using PKHeX.Core;
using EtumrepMMO.Lib;

namespace EtumrepMMO.Server;

public class EtumrepUtil
{
    public static ulong CalculateSeed(List<PKM> pkms)
    {
        var seed = GroupSeedFinder.FindSeed(pkms).Seed;
        return seed;
    }

    public static List<PKM> GetPokeList(byte[] data, int count)
    {
        List<PKM> pks = new();
        for (int i = 0; i < count; i++)
        {
            int ofs = 376 * i;
            byte[] buf = data.Slice(ofs, 376);
            var pk = EntityFormat.GetFromBytes(buf)!;
            pks.Add(pk);
        }
        return pks;
    }
}
