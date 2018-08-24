﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Database.Adapters
{
    /// <summary>
    /// The SQL Server database adapter.
    /// </summary>
    public partial class SqlServerAdapter : SqlAdapter, ISqlAdapter
    {

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
                cmd.Append($"; select {EscapeColumnListWithAliases(tableInfo.GeneratedColumns, tableInfo.TableName)} from {EscapeTableName(tableInfo)} ");

                if (tableInfo.KeyColumns.Any(k => k.IsIdentity))
                {
                    cmd.Append($"where {EscapeColumnn(tableInfo.KeyColumns.First(k => k.IsIdentity).ColumnName)} = SCOPE_IDENTITY();");
                }
                else
                {
                    cmd.Append($"where {EscapeWhereList(tableInfo.KeyColumns)};");
                }

                var multi = connection.QueryMultiple(cmd.ToString(), entityToInsert, transaction, commandTimeout);

                var vals = multi.Read().ToList();

                if (!vals.Any()) return false;

                var rvals = ((IDictionary<string, object>)vals[0]);

                foreach (var key in rvals.Keys)
                {
                    var rval = rvals[key];
                    var p = tableInfo.GeneratedColumns.Single(gp => gp.ColumnName == key).Property;
                    p.SetValue(entityToInsert, Convert.ChangeType(rval, p.PropertyType), null);
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
                cmd.Append($"; select {EscapeColumnListWithAliases(tableInfo.GeneratedColumns, tableInfo.TableName)} from {EscapeTableName(tableInfo)} ");
                cmd.Append($"where {EscapeWhereList(tableInfo.KeyColumns)};");

                var multi = connection.QueryMultiple(cmd.ToString(), entityToUpdate, transaction, commandTimeout);

                var vals = multi.Read().ToList();

                if (!vals.Any()) return false;

                var rvals = ((IDictionary<string, object>)vals[0]);

                foreach (var key in rvals.Keys)
                {
                    var rval = rvals[key];
                    var p = tableInfo.GeneratedColumns.Single(gp => gp.ColumnName == key).Property;
                    p.SetValue(entityToUpdate, Convert.ChangeType(rval, p.PropertyType), null);
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
                cmd.Append($"; select {EscapeColumnListWithAliases(tableInfo.GeneratedColumns, tableInfo.TableName)} from {EscapeTableName(tableInfo)} ");

                if (tableInfo.KeyColumns.Any(k => k.IsIdentity))
                {
                    cmd.Append($"where {EscapeColumnn(tableInfo.KeyColumns.First(k => k.IsIdentity).ColumnName)} = SCOPE_IDENTITY();");
                }
                else
                {
                    cmd.Append($"where {EscapeWhereList(tableInfo.KeyColumns)};");
                }

                var multi = await connection.QueryMultipleAsync(cmd.ToString(), entityToInsert, transaction, commandTimeout);

                var vals = multi.Read().ToList();

                if (!vals.Any()) return false;

                var rvals = ((IDictionary<string, object>)vals[0]);

                foreach (var key in rvals.Keys)
                {
                    var rval = rvals[key];
                    var p = tableInfo.GeneratedColumns.Single(gp => gp.ColumnName == key).Property;
                    p.SetValue(entityToInsert, Convert.ChangeType(rval, p.PropertyType), null);
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
                cmd.Append($"; select {EscapeColumnListWithAliases(tableInfo.GeneratedColumns, tableInfo.TableName)} from {EscapeTableName(tableInfo)} ");
                cmd.Append($"where {EscapeWhereList(tableInfo.KeyColumns)};");

                var multi = await connection.QueryMultipleAsync(cmd.ToString(), entityToUpdate, transaction, commandTimeout);

                var vals = multi.Read().ToList();

                if (!vals.Any()) return false;

                var rvals = ((IDictionary<string, object>)vals[0]);

                foreach (var key in rvals.Keys)
                {
                    var rval = rvals[key];
                    var p = tableInfo.GeneratedColumns.Single(gp => gp.ColumnName == key).Property;
                    p.SetValue(entityToUpdate, Convert.ChangeType(rval, p.PropertyType), null);
                }

                return true;
            }
            else
            {
                return await connection.ExecuteAsync(cmd.ToString(), entityToUpdate, transaction, commandTimeout) > 0;
            }
        }

        /// <summary>
        /// Constructs a paged sql statement
        /// </summary>
        /// <param name="tableInfo">table information about the entity</param>
        /// <param name="page">the page to request</param>
        /// <param name="pageSize">the size of the page to request</param>
        /// <param name="sql">a sql statement or partial statement</param>
        /// <param name="parameters">the dynamic parameters for the query</param>
        /// <returns>A paginated sql statement</returns>
        public override string GetPageListQuery(TableInfo tableInfo, long page, long pageSize, string sql, DynamicParameters parameters)
        {
            var q = new SqlParser(GetListQuery(tableInfo, sql));
            var pageSkip = (page - 1) * pageSize;
            var pageTake = pageSkip + pageSize;
            var sqlOrderBy = "order by (select null)";
            var sqlOrderByRemoved = q.Sql;

            if (string.IsNullOrEmpty(q.OrderByClause))
            {
                if (tableInfo.KeyColumns.Any())
                {
                    sqlOrderBy = $"order by {EscapeColumnn(tableInfo.KeyColumns.First().ColumnName)}";
                }
            }
            else
            {
                sqlOrderBy = q.OrderByClause;
                sqlOrderByRemoved = sqlOrderByRemoved.Replace(q.OrderByClause, "");
            }

            var columnsOnly = $"page_inner.* FROM ({sqlOrderByRemoved}) page_inner";

            // TODO: can you bind these as bind variables in Sql Server CE?
            return $"select * from (select row_number() over ({sqlOrderBy}) page_rn, {columnsOnly}) page_outer where page_rn > {pageSkip} and page_rn <= {pageTake}";

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableInfo">table information about the entity</param>
        /// <returns></returns>
        public override string EscapeTableName(TableInfo tableInfo) =>
            (!string.IsNullOrEmpty(tableInfo.SchemaName) ? EscapeTableName(tableInfo.SchemaName) + "." : null) + EscapeTableName(tableInfo.TableName);

    }
}
