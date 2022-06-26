using PKHeX.Core;
using EtumrepMMO.Lib;

namespace EtumrepMMO.Server;

public static class EtumrepUtil
{
    public const int MAXCOUNT = 4;
    public const int SIZE = 376;
    public static ulong CalculateSeed(IEnumerable<PKM> pkms) => GroupSeedFinder.FindSeed(pkms).Seed;

    public static IEnumerable<PKM> GetPokeList(ReadOnlySpan<byte> data, int count)
    {
        System.Diagnostics.Debug.Assert(data.Length % SIZE == 0 && data.Length / SIZE == count);
        var result = new PA8[count];
        for (int i = 0; i < result.Length; i++)
        {
            byte[] buf = data.Slice(i * SIZE, SIZE).ToArray();
            result[i] = new PA8(buf);
        }
        return result;
    }
}
