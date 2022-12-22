using System.Data.Common;
using App.Dapper.SqlAdapter;
using System.Threading.Tasks;
using System;
using System.Text;
using System.Collections.Concurrent;
using App.Dapper.Core;
using System.Data;
using Dapper;

namespace App.Dapper.SqlAdapter.Provider
{
    public class SqlServerProvider : BaseProvider
    {
        private string GetInsertSql(TableInfo tableInfo)
        {
            return GetInsertSqlFromCache(tableInfo.TableName, () =>
            {
                var part = GetInsertSqlParts(tableInfo);
                if (tableInfo.AutoIncrement)
                    return string.Format("insert into {0} ({1})OUTPUT INSERTED.[{3}] values ({2})", tableInfo.TableName, part.Item1, part.Item2, tableInfo.PrimaryColumnName);
                else
                    return string.Format("insert into {0} ({1}) values ({2})", tableInfo.TableName, part.Item1, part.Item2);
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
        public override string BuildPageQuery(long skip, long take, SQLParts parts, ref object param)
        {
            var helper = (PagingHelper)PagingUtility;
            parts.SqlSelectRemoved = helper.RegexOrderBy.Replace(parts.SqlSelectRemoved, "", 1);
            if (helper.RegexDistinct.IsMatch(parts.SqlSelectRemoved))
            {
                parts.SqlSelectRemoved = "peta_inner.* FROM (SELECT " + parts.SqlSelectRemoved + ") peta_inner";
            }
            var sqlPage = string.Format("SELECT * FROM (SELECT ROW_NUMBER() OVER ({0}) peta_rn, {1}) peta_paged WHERE peta_rn>@min AND peta_rn<=@max",
                parts.SqlOrderBy == null ? "ORDER BY (SELECT NULL)" : parts.SqlOrderBy, parts.SqlSelectRemoved);
            DynamicParameters newParam = new DynamicParameters(param);
            newParam.Add("min", skip);
            newParam.Add("max", skip + take);
            param = newParam;
            return sqlPage;
        }
        public override string GetExistsSql(string table, string whereSql)
        {
            return string.Format("IF EXISTS (SELECT 1 FROM [{0}] WHERE {1}) SELECT 1 ELSE SELECT 0", table, whereSql);
        }
    }
}