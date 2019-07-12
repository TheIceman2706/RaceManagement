using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strafrunden.Resources
{
    public static class TransponderLookup
    {
        public static SqlConnection Sql;
        public static int Find(string tc)
        {
            try
            {
                using (var com = Sql.CreateCommand())
                {
                    com.CommandText = "SELECT startnummer FROM transponder WHERE code = @tc;";
                    com.Parameters.AddWithValue("@tc", tc);
                    return (int)com.ExecuteScalar();
                }

            }
            catch(Exception e)
            {
                Logging.Log.Instance.Error("[TransponderLookup] " + e.ToString());
                return 0;
            }
        }

        public static void Add(string tc, int stnr)
        {
            try
            {
                using (var com = Sql.CreateCommand())
                {
                    com.CommandText = "INSERT INTO transponder (code,startnummer) VALUES (@tc,@stnr);";
                    com.Parameters.AddWithValue("@tc", tc);
                    com.Parameters.AddWithValue("@stnr", stnr);
                    com.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                Logging.Log.Instance.Error("[TransponderLookup] " + e.ToString());
            }
        }

        public static void Remove(string tc)
        {
            try
            {
                using (var com = Sql.CreateCommand())
                {
                    com.CommandText = "DELETE FROM transponder WHERE code = @tc;";
                    com.Parameters.AddWithValue("@tc", tc);
                    com.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                Logging.Log.Instance.Error("[TransponderLookup] " + e.ToString());
            }
        }

        public static void Remove(int stnr)
        {
            try
            {
                using (var com = Sql.CreateCommand())
                {
                    com.CommandText = "DELETE FROM transponder WHERE startnummer = @stnr;";
                    com.Parameters.AddWithValue("@stnr", stnr);
                    com.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                Logging.Log.Instance.Error("[TransponderLookup] " + e.ToString());
            }
        }

        internal static void Clear()
        {
            try
            {
                using (var com = Sql.CreateCommand())
                {
                    com.CommandText = "DELETE FROM transponder WHERE 1=1;";
                    com.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                Logging.Log.Instance.Error("[TransponderLookup] " + e.ToString());
            }
        }

        public static IReadOnlyDictionary<string,int> List()
        {
            Dictionary<string, int> dict = new Dictionary<string, int>();
            try
            {
                using (var com = Sql.CreateCommand())
                {
                    com.CommandText = "SELECT code,startnummer FROM transponder;";
                    using (var rd = com.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            if (!rd.IsDBNull(0) && !rd.IsDBNull(1))
                                dict.Add(rd.GetString(0), rd.GetInt32(1));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logging.Log.Instance.Error("[TransponderLookup] " + e.ToString());
            }
            return new ReadOnlyDictionary<string, int>(dict);
        }
    }
}
