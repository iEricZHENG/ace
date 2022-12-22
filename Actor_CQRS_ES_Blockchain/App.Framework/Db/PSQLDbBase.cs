using App.Dapper;
using System.Collections.Concurrent;

namespace App.Framework.Db
{
    public class PSQLDbBase
    {
        static DbFactory dbFactory;
        static PSQLDbBase()
        {
            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["CoreDb"].ConnectionString;
            dbFactory = new DbFactory(Dapper.SqlAdapter.SqlType.Npgsql, connStr);            
        }
        static object getLock = new object();
        static ConcurrentDictionary<string, DbFactory> factoryDict = new ConcurrentDictionary<string, DbFactory>();
        public static DapperConnection GetConnection(string connName = null)
        {
            DapperConnection conn = null;
            if (!string.IsNullOrEmpty(connName))
            {
                var configConnectionStrings = System.Configuration.ConfigurationManager.ConnectionStrings[connName];
                if (configConnectionStrings != null)
                {
                    if (!string.IsNullOrEmpty(configConnectionStrings.ConnectionString))
                    {
                        conn = CreateConnection(configConnectionStrings.ConnectionString);
                    }
                }
            }
            if (conn == null)
                conn = dbFactory.GetConnection();
            return conn;
        }
        private static DapperConnection CreateConnection(string conn = null)
        {
            DbFactory factory = null;
            if (!string.IsNullOrEmpty(conn))
            {
                if (!factoryDict.TryGetValue(conn, out factory))
                {
                    lock (getLock)
                    {
                        if (!factoryDict.TryGetValue(conn, out factory))
                        {
                            factory = new DbFactory(Dapper.SqlAdapter.SqlType.Npgsql, conn);
                            factoryDict.TryAdd(conn, factory);
                        }
                    }
                }
            }
            if (factory == null)
                return null;
            return factory.GetConnection();
        }
    }
}