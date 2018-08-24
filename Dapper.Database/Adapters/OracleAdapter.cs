﻿using System;
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

                var vals = connection.Query(cmd.ToString(), entityToInsert, transaction, commandTimeout: commandTimeout).ToList();

                if (!vals.Any()) return false;

                var rvals = ((IDictionary<string, object>)vals[0]);

                foreach (var key in rvals.Keys)
                {
                    var rval = rvals[key];
                    var p = tableInfo.GeneratedColumns.Single(gp => gp.ColumnName.Equals(key, StringComparison.OrdinalIgnoreCase)).Property;
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
                cmd.Append($" RETURNING  {EscapeColumnList(tableInfo.GeneratedColumns)} INTO {EscapeParameters(tableInfo.GeneratedColumns)}");

                var vals = connection.Query(cmd.ToString(), entityToUpdate, transaction, commandTimeout: commandTimeout).ToList();

                if (!vals.Any()) return false;

                var rvals = ((IDictionary<string, object>)vals[0]);

                foreach (var key in rvals.Keys)
                {
                    var rval = rvals[key];
                    var p = tableInfo.GeneratedColumns.Single(gp => gp.ColumnName.Equals(key, StringComparison.OrdinalIgnoreCase)).Property;
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
                cmd.Append($" RETURNING  {EscapeColumnList(tableInfo.GeneratedColumns)} INTO {EscapeParameters(tableInfo.GeneratedColumns)}");

                var rslt = await connection.QueryAsync(cmd.ToString(), entityToInsert, transaction, commandTimeout: commandTimeout);

                var vals = rslt.ToList();

                if (!vals.Any()) return false;

                var rvals = ((IDictionary<string, object>)vals[0]);

                foreach (var key in rvals.Keys)
                {
                    var rval = rvals[key];
                    var p = tableInfo.GeneratedColumns.Single(gp => gp.ColumnName.Equals(key, StringComparison.OrdinalIgnoreCase)).Property;
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
                cmd.Append($" RETURNING  {EscapeColumnList(tableInfo.GeneratedColumns)} INTO {EscapeParameters(tableInfo.GeneratedColumns)}");

                var rslt = await connection.QueryAsync(cmd.ToString(), entityToUpdate, transaction, commandTimeout: commandTimeout);

                var vals = rslt.ToList();

                if (!vals.Any()) return false;

                var rvals = ((IDictionary<string, object>)vals[0]);

                foreach (var key in rvals.Keys)
                {
                    var rval = rvals[key];
                    var p = tableInfo.GeneratedColumns.Single(gp => gp.ColumnName.Equals(key, StringComparison.OrdinalIgnoreCase)).Property;
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
        /// Applies a schema name is one is specified
        /// </summary>
        /// <param name="tableInfo"></param>
        /// <returns></returns>
        public override string EscapeTableName(TableInfo tableInfo) =>
            (!string.IsNullOrEmpty(tableInfo.SchemaName) ? EscapeTableName(tableInfo.SchemaName) + "." : null) + EscapeTableName(tableInfo.TableName);

        /// <summary>
        /// Returns the format for table name
        /// </summary>
        public override string EscapeTableName(string value) => $"{value}";

        /// <summary>
        /// Returns the format for column
        /// </summary>
        public override string EscapeColumnn(string value) => $"{value}";

        /// <summary>
        /// Returns the format for parameter
        /// </summary>
        public override string EscapeParameter(string value) => $":{value}";

        public override string DeleteQuery(TableInfo tableInfo, string sql)
        {
            var x = base.DeleteQuery(tableInfo, sql);
            return x;
        }
    }
}
