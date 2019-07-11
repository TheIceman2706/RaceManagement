using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strafrunden.Resources
{
    public static class TransponderLookup
    {
        public static Dictionary<string, int> Dict = new Dictionary<string, int>();
        public static int Find(string tc)
        {
            try
            {
                return Dict[tc];
            }
            catch(Exception e)
            {
                return 0;
            }
        }

        public static void Add(string tc, int stnr)
        {
            try
            {
                Dict.Add(tc, stnr);
            }
            catch (Exception e)
            {
            }
        }

        public static void Remove(string tc)
        {
            try
            {
                Dict.Remove(tc);
            }
            catch (Exception e)
            {
            }
        }

        public static void Remove(int stnr)
        {
            try
            {
                Dict.Remove(Dict.First(kv => kv.Value == stnr).Key);
            }
            catch (Exception e)
            {
            }
        }
    }
}
