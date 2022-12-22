using App.Dapper.Core;

namespace App.Dapper.SqlAdapter.Provider
{
    public class ProviderFactory
    {
        public static IProvider GetProvider(SqlType type)
        {
            switch (type)
            {
                case SqlType.SqlServer: return SingleInstance<SqlServerProvider>.Instance;
                case SqlType.MySql: return SingleInstance<MySqlProvider>.Instance;
                case SqlType.Npgsql: return SingleInstance<PostgreSQLProvider>.Instance;
                default: return SingleInstance<SQLiteProvider>.Instance;
            }
        }
    }
}
