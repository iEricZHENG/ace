using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Data;
using System.Threading.Tasks;
using System.Text;
using Dapper;
using App.Dapper.SqlAdapter;
using App.Dapper.Core;

namespace App.Dapper
{
    public static class DapperExtensions
    {
        #region All
        public static IEnumerable<T> GetAll<T>(this DapperConnection connection, bool keyAsc = true)
        {
            var sql = BuildAllSql<T>(connection, keyAsc);
            return connection.Base.Query<T>(sql);
        }
        public static async Task<IEnumerable<T>> GetAllAsync<T>(this DapperConnection connection, bool keyAsc = true)
        {
            var sql = BuildAllSql<T>(connection, keyAsc);
            return await connection.Base.QueryAsync<T>(sql);
        }
        public static Page<T> GetAllPage<T>(this DapperConnection connection, long page, long pageSize, bool keyAsc = true)
        {
            var sql = BuildAllSql<T>(connection, keyAsc);
            return connection.QueryPage<T>(page, pageSize, sql);
        }
        public static async Task<Page<T>> GetAllPageAsync<T>(this DapperConnection connection, long page, long pageSize, bool keyAsc = true)
        {
            var sql = BuildAllSql<T>(connection, keyAsc);
            return await connection.QueryPageAsync<T>(page, pageSize, sql);
        }
        private static string BuildAllSql<T>(DapperConnection connection, bool keyAsc)
        {
            var tType = typeof(T);
            var tb = TableInfo.FromType(tType);
            return string.Format("SELECT * FROM {0} ORDER BY {1} {2}", connection.SqlProvider.EscapeTableName(tb.TableName), connection.SqlProvider.GetColumnName(tb.PrimaryColumnName), keyAsc ? "ASC" : "DESC");
        }
        #endregion
        #region Update
        public static int Update<T>(this DapperConnection connection, T data, List<string> columns = null, List<string> noColumns = null, IDbTransaction transaction = null)
        {
            return connection.Base.Execute(BuildUpdateSql(connection, data, columns, noColumns), data, transaction);
        }
        public static async Task<int> UpdateAsync<T>(this DapperConnection connection, T data, List<string> columns = null, List<string> noColumns = null, IDbTransaction transaction = null)
        {
            return await connection.Base.ExecuteAsync(BuildUpdateSql(connection, data, columns, noColumns), data, transaction);
        }
        private static ConcurrentDictionary<string, string> updateSqlDict = new ConcurrentDictionary<string, string>();
        private static string BuildUpdateSql<T>(DapperConnection connection, T data, List<string> columns = null, List<string> noColumns = null)
        {
            var t = typeof(T);
            string sql;
            if (columns == null && noColumns == null)
            {
                if (updateSqlDict.TryGetValue(t.FullName, out sql)) return sql;
            }
            var tb = TableInfo.FromType(t);
            IProvider _provider = connection.SqlProvider;
            object primaryValue;
            var sb = new StringBuilder("UPDATE ");
            sb.Append(_provider.EscapeTableName(tb.TableName)).Append(" SET ");
            if (columns == null)
            {
                int alreadyCount = 0;
                for (int i = 0; i < tb.Columns.Count; i++)
                {
                    var column = tb.Columns[i];
                    if (noColumns != null && noColumns.Contains(column.Name)) continue;
                    if (column.Name.Equals(tb.PrimaryColumnName))
                    {
                        primaryValue = column.GetValue(data);
                        if (tb.AutoIncrement) continue;
                    }
                    if (column.IsResult) continue;
                    if (alreadyCount > 0)
                        sb.Append(", ");
                    _provider.AppendColumnNameEqualsValue(sb, column.Name);
                    alreadyCount++;
                }
            }
            else
            {
                for (int i = 0; i < columns.Count; i++)
                {
                    var column = columns[i];
                    if (i > 0)
                        sb.Append(", ");
                    _provider.AppendColumnNameEqualsValue(sb, column);
                }
            }
            sb.Append(" WHERE ");
            _provider.AppendColumnNameEqualsValue(sb, tb.PrimaryColumnName);
            sql = sb.ToString();
            if (columns == null && noColumns == null)
            {
                updateSqlDict.TryAdd(t.FullName, sql);
            }
            return sql;
        }
        #endregion
        #region QuerySingle
        public static T Single<T>(this DapperConnection connection, object primaryKey)
        {
            var sqlPara = BuildSingleSql<T>(connection, primaryKey);
            return connection.Base.QuerySingle<T>(sqlPara.Item1, sqlPara.Item2);
        }
        public static async Task<T> SingleAsync<T>(this DapperConnection connection, object primaryKey)
        {
            var sqlPara = BuildSingleSql<T>(connection, primaryKey);
            return await connection.Base.QuerySingleAsync<T>(sqlPara.Item1, sqlPara.Item2);
        }
        public static T SingleOrDefault<T>(this DapperConnection connection, object primaryKey)
        {
            var sqlPara = BuildSingleSql<T>(connection, primaryKey);
            return connection.Base.QuerySingleOrDefault<T>(sqlPara.Item1, sqlPara.Item2);
        }
        public static Task<T> SingleOrDefaultAsync<T>(this DapperConnection connection, object primaryKey)
        {
            var sqlPara = BuildSingleSql<T>(connection, primaryKey);
            return connection.Base.QuerySingleOrDefaultAsync<T>(sqlPara.Item1, sqlPara.Item2);
        }
        private static Tuple<string, DynamicParameters> BuildSingleSql<T>(DapperConnection connection, object primaryKey)
        {
            var tb = TableInfo.FromType(typeof(T));
            IProvider provider = connection.SqlProvider;
            var sql = string.Format("SELECT * FROM {0} WHERE {1}", provider.EscapeTableName(tb.TableName), provider.GetColumnNameEqualsValue(tb.PrimaryColumnName));
            DynamicParameters paras = new DynamicParameters();
            paras.Add(tb.PrimaryColumnName, primaryKey);
            return Tuple.Create(sql, paras);
        }
        #endregion
        #region QueryPage
        public static Page<T> QueryPage<T>(this DapperConnection connection, long page, long pageSize, string sql, object param = null, CommandType? commandType = null)
        {
            SQLParts parts;
            if (!connection.SqlProvider.PagingUtility.SplitSQL(sql, out parts))
                throw new Exception("Unable to parse SQL statement for paged query");
            string pageSql = connection.SqlProvider.BuildPageQuery((page - 1) * pageSize, pageSize, parts, ref param);
            string sqlCount = parts.SqlCount;
            var result = new Page<T>();
            var list = connection.Base.Query<T>(pageSql, param);
            using (var multi = connection.Base.QueryMultiple(pageSql + ";" + sqlCount, param))
            {
                result.Items = multi.Read<T>().ToList();
                result.TotalItems = multi.ReadSingle<long>();
                result.CurrentPage = page;
                result.PageSize = pageSize;
                result.TotalPage = (long)Math.Ceiling(result.TotalItems / (pageSize * 1.0));
            }
            return result;
        }
        public static async Task<Page<T>> QueryPageAsync<T>(this DapperConnection connection, long page, long pageSize, string sql, object param = null, CommandType? commandType = null)
        {
            SQLParts parts;
            if (!connection.SqlProvider.PagingUtility.SplitSQL(sql, out parts))
                throw new Exception("Unable to parse SQL statement for paged query");
            string pageSql = connection.SqlProvider.BuildPageQuery((page - 1) * pageSize, pageSize, parts, ref param);
            string sqlCount = parts.SqlCount;
            var result = new Page<T>();
            using (var multi = await connection.Base.QueryMultipleAsync(pageSql + ";" + sqlCount, param))
            {
                var items = await multi.ReadAsync<T>();
                result.Items = items.ToList();
                result.TotalItems = await multi.ReadSingleAsync<long>();
                result.CurrentPage = page;
                result.PageSize = pageSize;
                result.TotalPage = (long)Math.Ceiling(result.TotalItems / (pageSize * 1.0));
            }
            return result;
        }
        public static async Task<Page<T>> QueryPageAsync<T>(this DapperConnection connection, long pageIndex, long pageSize, string countSql, string pagingSql, object param = null, CommandType? commandType = null)
        {
            var result = new Page<T>();
            using (var multi = await connection.Base.QueryMultipleAsync(pagingSql + ";" + countSql, param))
            {
                var items = await multi.ReadAsync<T>();
                result.Items = items.ToList();
                result.TotalItems = await multi.ReadSingleAsync<long>();
                result.CurrentPage = pageIndex;
                result.PageSize = pageSize;
                result.TotalPage = (long)Math.Ceiling(result.TotalItems / (pageSize * 1.0));
            }
            return result;
        }
        #endregion
        #region Delete
        public static bool Delete<T>(this DapperConnection connection, object primaryKey, IDbTransaction transaction = null)
        {
            var sqlPara = BuildDeleteSql<T>(connection, primaryKey);
            return connection.Base.Execute(sqlPara.Item1, sqlPara.Item2, transaction) > 0;
        }
        public static async Task<bool> DeleteAsync<T>(this DapperConnection connection, object primaryKey, IDbTransaction transaction = null)
        {
            var sqlPara = BuildDeleteSql<T>(connection, primaryKey);
            var count = await connection.Base.ExecuteAsync(sqlPara.Item1, sqlPara.Item2, transaction);
            return count > 0;
        }
        private static Tuple<string, DynamicParameters> BuildDeleteSql<T>(DapperConnection connection, object primaryKey)
        {
            var tb = TableInfo.FromType(typeof(T));
            IProvider provider = connection.SqlProvider;
            var sql = string.Format("DELETE FROM {0} WHERE {1}", provider.EscapeTableName(tb.TableName), provider.GetColumnNameEqualsValue(tb.PrimaryColumnName));
            DynamicParameters paras = new DynamicParameters();
            paras.Add(tb.PrimaryColumnName, primaryKey);
            return Tuple.Create(sql, paras);
        }
        #endregion
        #region Insert
        public static object Insert<T>(this DapperConnection connection, T data, IDbTransaction transaction = null)
        {
            var tType = typeof(T);
            var tb = TableInfo.FromType(tType);
            return connection.SqlProvider.Insert<T>(connection.Base, tb, data, transaction);
        }
        public static async Task<object> InsertAsync<T>(this DapperConnection connection, T data, IDbTransaction transaction = null)
        {
            var tType = typeof(T);
            var tb = TableInfo.FromType(tType);
            return await connection.SqlProvider.InsertAsync<T>(connection.Base, tb, data, transaction);
        }
        public static object Insert<T>(this DapperConnection connection, T[] data, IDbTransaction transaction = null)
        {
            var tType = typeof(T);
            var tb = TableInfo.FromType(tType);
            return connection.SqlProvider.Insert<T[]>(connection.Base, tb, data, transaction);
        }
        public static async Task<object> InsertAsync<T>(this DapperConnection connection, T[] data, IDbTransaction transaction = null)
        {
            var tType = typeof(T);
            var tb = TableInfo.FromType(tType);
            return await connection.SqlProvider.InsertAsync<T[]>(connection.Base, tb, data, transaction);
        }
        //自定义表名称
        public static object Insert<T>(this DapperConnection connection, string tableName, T data, IDbTransaction transaction = null)
        {
            var tType = typeof(T);
            var tb = TableInfo.FromType(tType);
            tb.TableName = tableName;
            return connection.SqlProvider.Insert<T>(connection.Base, tb, data, transaction);
        }
        public static async Task<object> InsertAsync<T>(this DapperConnection connection, string tableName, T data, IDbTransaction transaction = null)
        {
            var tType = typeof(T);
            var tb = TableInfo.FromType(tType);
            tb.TableName = tableName;
            return await connection.SqlProvider.InsertAsync<T>(connection.Base, tb, data, transaction);
        }
        public static object Insert<T>(this DapperConnection connection, string tableName, T[] data, IDbTransaction transaction = null)
        {
            var tType = typeof(T);
            var tb = TableInfo.FromType(tType);
            tb.TableName = tableName;
            return connection.SqlProvider.Insert<T[]>(connection.Base, tb, data, transaction);
        }
        public static async Task<object> InsertAsync<T>(this DapperConnection connection, string tableName, T[] data, IDbTransaction transaction = null)
        {
            var tType = typeof(T);
            var tb = TableInfo.FromType(tType);
            tb.TableName = tableName;
            return await connection.SqlProvider.InsertAsync<T[]>(connection.Base, tb, data, transaction);
        }
        #endregion Insert
        #region exits
        public static bool ExistsByKey<T>(this DapperConnection connection, object primaryKey)
        {
            var tType = typeof(T);
            var tb = TableInfo.FromType(tType);
            var sql = connection.SqlProvider.GetExistsSql(tb.TableName, string.Format(connection.SqlProvider.GetColumnNameEqualsValue(tb.PrimaryColumnName)));
            DynamicParameters paras = new DynamicParameters();
            paras.Add(tb.PrimaryColumnName, primaryKey);
            return connection.Base.ExecuteScalar<int>(sql, paras) != 0;
        }
        public static async Task<bool> ExistsByKeyAsync<T>(this DapperConnection connection, object primaryKey)
        {
            var tType = typeof(T);
            var tb = TableInfo.FromType(tType);
            var sql = connection.SqlProvider.GetExistsSql(tb.TableName, string.Format(connection.SqlProvider.GetColumnNameEqualsValue(tb.PrimaryColumnName)));
            DynamicParameters paras = new DynamicParameters();
            paras.Add(tb.PrimaryColumnName, primaryKey);
            return await connection.Base.ExecuteScalarAsync<int>(sql, paras) != 0;
        }
        public static bool Exists<T>(this DapperConnection connection, string whereSql, object param = null)
        {
            var tType = typeof(T);
            var tb = TableInfo.FromType(tType);
            var sql = connection.SqlProvider.GetExistsSql(tb.TableName, whereSql);
            return connection.Base.ExecuteScalar<int>(sql, param) != 0;
        }
        public static async Task<bool> ExistsAsync<T>(this DapperConnection connection, string whereSql, object param = null)
        {
            var tType = typeof(T);
            var tb = TableInfo.FromType(tType);
            var sql = connection.SqlProvider.GetExistsSql(tb.TableName, whereSql);
            return await connection.Base.ExecuteScalarAsync<int>(sql, param) != 0;
        }
        #endregion
    }
}
