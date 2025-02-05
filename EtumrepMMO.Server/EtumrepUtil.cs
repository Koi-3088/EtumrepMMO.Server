using PKHeX.Core;
using EtumrepMMO.Lib;

namespace EtumrepMMO.Server;

public static class EtumrepUtil
{
    public const int MAXCOUNT = 4;
    public const int SIZE = 376;

    public static IReadOnlyList<(PKM[], (ulong, byte)[])> GetSeeds(IEnumerable<PKM> pkms, byte maxRolls = 32) => GroupSeedFinder.GetSeeds(pkms, maxRolls);
    public static ulong CalculateSeed(IReadOnlyList<(PKM[], (ulong, byte)[])> list, SpawnerType mode = SpawnerType.All) => GroupSeedFinder.FindSeed(list, mode).Seed;

    public static IEnumerable<PKM> GetPokeList(ReadOnlySpan<byte> data, int count)
    {
        //Crashes for some reason?
        //System.Diagnostics.Debug.Assert(data.Length % SIZE == 0 && data.Length / SIZE == count);
        var result = new List<PA8>();
        for (int i = 0; i < count; i++)
        {
            try
            {
                byte[] buf = data.Slice(i * SIZE, SIZE).ToArray();
                PA8? pa8 = new(buf);

                if (pa8 is not null)
                {
                    var la = new LegalityAnalysis(pa8);
                    var res = la.Results.ToList().FindAll(x => !x.Valid);
                    if (la.Valid || res.Count is 1)
                    {
                        result.Add(pa8);
                        continue;
                    }
                }

                var msg = "Likely malformed data received, skipping file...";
                LogUtil.Log(msg, "[GetPokeList]");

            }
            catch (Exception ex)
            {
                var msg = $"Error ocurred when casting byte array into PA8: {ex.Message}\nStack Trace: {ex.StackTrace}";
                LogUtil.Log(msg, "[GetPokeList]");
            }
        }
        return result;
    }
}
