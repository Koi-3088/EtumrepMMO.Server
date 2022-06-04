using PKHeX.Core;
using EtumrepMMO.Lib;

namespace EtumrepMMO.Server
{
    public class EtumrepUtil
    {
        public static ulong CalculateSeed(byte[] data, int count)
        {
            var list = GetPokeList(data, count);
            var seed = GroupSeedFinder.FindSeed(list).Seed;
            return seed;
        }

        private static List<PKM> GetPokeList(byte[] data, int count)
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
}
