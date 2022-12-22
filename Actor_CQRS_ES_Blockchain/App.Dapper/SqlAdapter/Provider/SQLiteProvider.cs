using System;
using System.Data.Common;
using App.Dapper.SqlAdapter;
using System.Threading.Tasks;
using System.Text;
using App.Dapper.Core;
using System.Data;
using Dapper;

namespace App.Dapper.SqlAdapter.Provider
{
    public class SQLiteProvider : BaseProvider
    {
        private string GetInsertSql(TableInfo tableInfo)
        {
            return GetInsertSqlFromCache(tableInfo.TableName, () =>
            {
                var part = GetInsertSqlParts(tableInfo);
                if (tableInfo.AutoIncrement)
                    return string.Format("insert into {0} ({1}) values ({2}); SELECT last_insert_rowid()", tableInfo.TableName, part.Item1, part.Item2);
                else
                    return string.Format("insert into {0} ({1}) values ({2});", tableInfo.TableName, part.Item1, part.Item2);
            });
        }
        public override object Insert<T>(IDbConnection connection, TableInfo tableInfo, T data, IDbTransaction transaction = null)
        {
            return connection.ExecuteScalar(GetInsertSql(tableInfo), data, transaction);
        }
        public override async Task<object> InsertAsync<T>(IDbConnection connection, TableInfo tableInfo, T data, IDbTransaction transaction = null)
        {
            return await connection.ExecuteScalarAsync(GetInsertSql(tableInfo), data, transaction);
        }
        public override void AppendColumnName(StringBuilder sb, string columnName)
        {
            sb.AppendFormat("\"{0}\"", columnName);
        }
        public override string GetColumnName(string columnName)
        {
            return string.Format("\"{0}\"", columnName);
        }
        public override void AppendColumnNameEqualsValue(StringBuilder sb, string columnName)
        {
            sb.AppendFormat("\"{0}\" = @{1}", columnName, columnName);
        }
        public override string GetColumnNameEqualsValue(string columnName)
        {
            return string.Format("\"{0}\" = @{1}", columnName, columnName);
        }
        public override string GetExistsSql(string table, string whereSql)
        {
            return string.Format("SELECT EXISTS (SELECT 1 FROM [{0}] WHERE {1})", table, whereSql);
        }
    }
}