using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Database.Adapters
{
    /// <summary>
    /// The Oracle database adapter.
    /// </summary>
    public partial class OracleAdapter : SqlAdapter, ISqlAdapter
    {
        /// <summary>
        /// Oracle-specific implementation of an Exists query.
        /// </summary>
        /// <param name="tableInfo">table information about the entity</param>
        /// <param name="sql">a sql statement or partial statement</param>
        /// <returns>A sql statement that selects true if a record matches</returns>
        public override string ExistsQuery(TableInfo tableInfo, string sql)
        {
            var q = new SqlParser(sql ?? "");

            if (q.Sql.StartsWith(";", StringComparison.Ordinal))
                return q.Sql.Substring(1);

            if (!q.IsSelect)
            {
                var wc = string.IsNullOrWhiteSpace(q.Sql) ? $"where {EscapeWhereList(tableInfo.KeyColumns)}" : q.Sql;

                if (string.IsNullOrEmpty(q.FromClause))
                    return $"select 1 from dual where exists (select 1 from { EscapeTableName(tableInfo)} {wc})";
                else
                    return $"select 1 from dual where exists (select 1 {wc})";
            }

            return $"select 1 from dual where exists ({q.Sql})";
        }

        /// <summary>
        /// Oracle implementation of a a paged sql statement
        /// </summary>
        /// <param name="tableInfo">table information about the entity</param>
        /// <param name="page">the page to request</param>
        /// <param name="pageSize">the size of the page to request</param>
        /// <param name="sql">a sql statement or partial statement</param>
        /// <param name="parameters">the dynamic parameters for the query</param>
        /// <returns>A paginated sql statement</returns>
        /// <remarks>
        /// Oracle supports binding <c>offset</c> and <paramref name="pageSize"/> as <paramref name="parameters"/>.
        /// </remarks>
        public override string GetPageListQuery(TableInfo tableInfo, long page, long pageSize, string sql, DynamicParameters parameters)
        {
            var q = new SqlParser(GetListQuery(tableInfo, sql));
            var pageSkip = (page - 1) * pageSize;

            var sqlOrderBy = string.Empty;

            if (string.IsNullOrEmpty(q.OrderByClause) && tableInfo.KeyColumns.Any())
            {
                sqlOrderBy = $"order by {EscapeColumnn(tableInfo.KeyColumns.First().ColumnName)}";
            }

            // Oracle supports binding the offset and page size.
            // Use variable names that are unlikely to be used as parameters, and that are safe to use with ODP.NET and Dapper.
            parameters.Add("PAGE_SKIP$$", pageSkip, DbType.Int64);
            parameters.Add("PAGE_SIZE$$", pageSize, DbType.Int64);

            return $"{q.Sql} {sqlOrderBy} offset :PAGE_SKIP$$ rows fetch next :PAGE_SIZE$$ rows only";
        }

        /// <summary>
        /// Inserts an entity into table "Ts"
        /// </summary>
        /// <param name="connection">Open SqlConnection</param>
        /// <param name="transaction">The transaction to run under, null (the default) if none</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
        /// <param name="tableInfo">table information about the entity</param>
        /// <param name="entityToInsert">Entity to insert</param>
        /// <returns>true if the entity was inserted</returns>
        public override bool Insert(IDbConnection connection, IDbTransaction transaction, int? commandTimeout, TableInfo tableInfo, object entityToInsert)
        {
            var cmd = new StringBuilder(InsertQuery(tableInfo));

            if (tableInfo.GeneratedColumns.Any() && tableInfo.KeyColumns.Any())
            {
                cmd.Append($" RETURNING  {EscapeColumnList(tableInfo.GeneratedColumns)} INTO {EscapeParameters(tableInfo.GeneratedColumns)}");

                // Oracle does not return RETURNING values in a result set; rather, it returns them as InputOutput parameters.
                // We need to wrap the incoming object in a DynamicParameters collection to get at the values.
                var parameters = new DynamicParameters(entityToInsert);
                var count = connection.Execute(cmd.ToString(), parameters, transaction, commandTimeout: commandTimeout);

                if (count == 0) return false;

                foreach (var column in tableInfo.GeneratedColumns)
                {
                    var property = column.Property;
                    var paramName = parameters.ParameterNames.Single(p => column.ColumnName.Equals(p, StringComparison.OrdinalIgnoreCase));

                    property.SetValue(entityToInsert, Convert.ChangeType(parameters.Get<object>(paramName), property.PropertyType), null);
                }

                return true;
            }
            else
            {
                return connection.Execute(cmd.ToString(), entityToInsert, transaction, commandTimeout) > 0;
            }

        }

        /// <summary>
        /// updates an entity into table "Ts"
        /// </summary>
        /// <param name="connection">Open SqlConnection</param>
        /// <param name="transaction">The transaction to run under, null (the default) if none</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
        /// <param name="tableInfo">table information about the entity</param>
        /// <param name="entityToUpdate">Entity to update</param>
        /// <param name="columnsToUpdate">A list of columns to update</param>
        /// <returns>true if the entity was updated</returns>
        public override bool Update(IDbConnection connection, IDbTransaction transaction, int? commandTimeout, TableInfo tableInfo, object entityToUpdate, IEnumerable<string> columnsToUpdate)
        {
            var cmd = new StringBuilder(UpdateQuery(tableInfo, columnsToUpdate));

            if (tableInfo.GeneratedColumns.Any() && tableInfo.KeyColumns.Any())
            {
                cmd.Append($" RETURNING  {EscapeColumnList(tableInfo.GeneratedColumns)} INTO {EscapeParameters(tableInfo.GeneratedColumns)}");

                // Oracle does not return RETURNING values in a result set; rather, it returns them as InputOutput parameters.
                // We need to wrap the incoming object in a DynamicParameters collection to get at the values.
                var parameters = new DynamicParameters(entityToUpdate);
                var count = connection.Execute(cmd.ToString(), parameters, transaction, commandTimeout: commandTimeout);

                if (count == 0) return false;

                foreach (var column in tableInfo.GeneratedColumns)
                {
                    var property = column.Property;
                    var paramName = parameters.ParameterNames.Single(p => column.ColumnName.Equals(p, StringComparison.OrdinalIgnoreCase));

                    property.SetValue(entityToUpdate, Convert.ChangeType(parameters.Get<object>(paramName), property.PropertyType), null);
                }

                return true;
            }
            else
            {
                return connection.Execute(cmd.ToString(), entityToUpdate, transaction, commandTimeout) > 0;
            }
        }

        /// <summary>
        /// Inserts an entity into table "Ts"
        /// </summary>
        /// <param name="connection">Open SqlConnection</param>
        /// <param name="transaction">The transaction to run under, null (the default) if none</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
        /// <param name="tableInfo">table information about the entity</param>
        /// <param name="entityToInsert">Entity to insert</param>
        /// <returns>true if the entity was inserted</returns>
        public override async Task<bool> InsertAsync(IDbConnection connection, IDbTransaction transaction, int? commandTimeout, TableInfo tableInfo, object entityToInsert)
        {
            var cmd = new StringBuilder(InsertQuery(tableInfo));

            if (tableInfo.GeneratedColumns.Any() && tableInfo.KeyColumns.Any())
            {
                cmd.Append($" RETURNING  {EscapeColumnList(tableInfo.GeneratedColumns)} INTO {EscapeParameters(tableInfo.GeneratedColumns)}");

                // Oracle does not return RETURNING values in a result set; rather, it returns them as InputOutput parameters.
                // We need to wrap the incoming object in a DynamicParameters collection to get at the values.
                var parameters = new DynamicParameters(entityToInsert);
                var count = await connection.ExecuteAsync(cmd.ToString(), parameters, transaction, commandTimeout: commandTimeout);

                if (count == 0) return false;

                foreach (var column in tableInfo.GeneratedColumns)
                {
                    var property = column.Property;
                    var paramName = parameters.ParameterNames.Single(p => column.ColumnName.Equals(p, StringComparison.OrdinalIgnoreCase));

                    property.SetValue(entityToInsert, Convert.ChangeType(parameters.Get<object>(paramName), property.PropertyType), null);
                }

                return true;
            }
            else
            {
                return await connection.ExecuteAsync(cmd.ToString(), entityToInsert, transaction, commandTimeout) > 0;
            }

        }

        /// <summary>
        /// updates an entity into table "Ts"
        /// </summary>
        /// <param name="connection">Open SqlConnection</param>
        /// <param name="transaction">The transaction to run under, null (the default) if none</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
        /// <param name="tableInfo">table information about the entity</param>
        /// <param name="entityToUpdate">Entity to update</param>
        /// <param name="columnsToUpdate">A list of columns to update</param>
        /// <returns>true if the entity was updated</returns>
        public override async Task<bool> UpdateAsync(IDbConnection connection, IDbTransaction transaction, int? commandTimeout, TableInfo tableInfo, object entityToUpdate, IEnumerable<string> columnsToUpdate)
        {
            var cmd = new StringBuilder(UpdateQuery(tableInfo, columnsToUpdate));

            if (tableInfo.GeneratedColumns.Any() && tableInfo.KeyColumns.Any())
            {
                cmd.Append($" RETURNING  {EscapeColumnList(tableInfo.GeneratedColumns)} INTO {EscapeParameters(tableInfo.GeneratedColumns)}");

                // Oracle does not return RETURNING values in a result set; rather, it returns them as InputOutput parameters.
                // We need to wrap the incoming object in a DynamicParameters collection to get at the values.
                var parameters = new DynamicParameters(entityToUpdate);
                var count = await connection.ExecuteAsync(cmd.ToString(), parameters, transaction, commandTimeout: commandTimeout);

                if (count == 0) return false;

                foreach (var column in tableInfo.GeneratedColumns)
                {
                    var property = column.Property;
                    var paramName = parameters.ParameterNames.Single(p => column.ColumnName.Equals(p, StringComparison.OrdinalIgnoreCase));

                    property.SetValue(entityToUpdate, Convert.ChangeType(parameters.Get<object>(paramName), property.PropertyType), null);
                }

                return true;
            }
            else
            {
                return await connection.ExecuteAsync(cmd.ToString(), entityToUpdate, transaction, commandTimeout) > 0;
            }
        }

        /// <summary>
        /// Applies a schema name is one is specified
        /// </summary>
        /// <param name="tableInfo"></param>
        /// <returns></returns>
        public override string EscapeTableName(TableInfo tableInfo) =>
            (!string.IsNullOrEmpty(tableInfo.SchemaName) ? EscapeTableName(tableInfo.SchemaName) + "." : null) + EscapeTableName(tableInfo.TableName);

        /// <summary>
        /// Returns the format for table name
        /// </summary>
        public override string EscapeTableName(string value) => $"\"{value.ToUpperInvariant()}\"";

        /// <summary>
        /// Returns the format for column
        /// </summary>
        public override string EscapeColumnn(string value) => $"\"{value.ToUpperInvariant()}\"";

        /// <summary>
        /// Returns the format for parameter
        /// </summary>
        public override string EscapeParameter(string value) => $":{value}";
    }
}
