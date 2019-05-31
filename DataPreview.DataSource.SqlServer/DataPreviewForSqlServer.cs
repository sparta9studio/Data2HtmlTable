using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using DataPreview.Models;

namespace DataPreview.DataSource.SqlServer
{
    public class DataPreviewForSqlServer : IDataSource
    {
        private readonly string connectionString;
        private readonly static Type StringType = typeof(string);

        public DataPreviewForSqlServer(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            this.connectionString = connectionString;
        }

        public async Task<Table> GetTableInfoAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            var schemaIdicator = name.IndexOf('.');
            var table = await QueryTableInfoAsync(new QueryTableInfo
            {
                CommandText = QueryTemplate.SelectTableSchema,
                TableSchema = schemaIdicator > 0 ? name.Substring(0, schemaIdicator) : string.Empty,
                TableName = schemaIdicator > 0 ? name.Substring(schemaIdicator + 1) : name
            }, ReadTableInfoAsync);

            if (!string.IsNullOrEmpty(table.Name))
            {
                var columns = await QueryTableInfoAsync(new QueryTableInfo
                {
                    CommandText = QueryTemplate.SelectColumnInfo,
                    TableName = table.Name,
                    TableSchema = table.Schema
                }, ReadColumnsInfoAsync);

                table.Columns.AddRange(columns);
            }

            return table;
        }

        public Task<DataTable> GetSampleDataAsync(Table table, DataPreviewOptions options)
        {
            return QueryAsync(GenerateSampleSql(table), options);
        }

        public string GenerateSampleSql(Table table)
        {
            return QueryTemplate.GenerateSampleSelectStatement(table);
        }

        public async Task<DataTable> QueryAsync(string sql, DataPreviewOptions options)
        {
            var result = new DataTable();

            using (var connection = new SqlConnection(this.connectionString))
            {
                using (var command = new SqlCommand(sql, connection))
                {
                    await command.Connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            result.Columns.AddRange(GetDataColumns(reader));

                            do
                            {
                                var a = GetDataRow(reader, 50).ToArray();
                                result.Rows.Add(a);
                            } while (result.Rows.Count <= options.MaxRow && await reader.ReadAsync());
                        }
                    }
                }
            }

            return result;
        }

        private static IEnumerable<object> GetDataRow(IDataReader reader, int maxStringLength)
        {
            for (var i = 0; i < reader.FieldCount; i++)
            {
                if (!reader.IsDBNull(i) && reader.GetFieldType(i) == StringType)
                {
                    yield return Truncate(reader.GetString(i), maxStringLength);
                }
                else
                {
                    yield return reader.GetValue(i);
                }
            }
        }

        private static string Truncate(string value, int maxLength)
        {
            if (value.Length < maxLength)
            {
                return value;
            }
            
            return value.Substring(0, maxLength) + "...";
        }

        private static DataColumn[] GetDataColumns(IDataReader reader)
        {
            if (reader.FieldCount == 0)
            {
                return new DataColumn[0];
            }

            return Enumerable.Range(0, reader.FieldCount).Select(r => new DataColumn
            (
                reader.GetName(r),
                reader.GetFieldType(r)
            )).ToArray();
        }

        private static async Task<Table> ReadTableInfoAsync(SqlDataReader reader)
        {
            var table = new Table();

            if (await reader.ReadAsync())
            {
                var nameIndex = reader.GetOrdinal(nameof(Table.Name));
                var schemaIndex = reader.GetOrdinal(nameof(Table.Schema));

                table.Name = reader.GetString(nameIndex);
                table.Schema = reader.GetString(schemaIndex);
            }

            return table;
        }

        private static async Task<List<Column>> ReadColumnsInfoAsync(SqlDataReader reader)
        {
            var result = new List<Column>();

            var nameIndex = reader.GetOrdinal(nameof(Column.ColumnName));
            var typeIndex = reader.GetOrdinal(nameof(Column.ColumnType));
            var nullableIndex = reader.GetOrdinal(nameof(Column.IsNullable));
            var maxLengthIndex = reader.GetOrdinal(nameof(Column.MaxLength));
            var keyIndex = reader.GetOrdinal(nameof(Column.IsPrimaryKey));

            while (await reader.ReadAsync())
            {
                result.Add(new Column
                {
                    ColumnName = reader.GetString(nameIndex),
                    ColumnType = reader.GetString(typeIndex),
                    IsNullable = reader.GetBoolean(nullableIndex),
                    MaxLength = reader.GetInt32(maxLengthIndex),
                    IsPrimaryKey = reader.GetBoolean(keyIndex)
                });
            }

            return result;
        }

        private async Task<T> QueryTableInfoAsync<T>(QueryTableInfo query, Func<SqlDataReader, Task<T>> callbackAsync)
        {
            if (callbackAsync == null)
            {
                throw new ArgumentNullException(nameof(callbackAsync));
            }

            using (var connection = new SqlConnection(this.connectionString))
            {
                using (var command = new SqlCommand(query.CommandText, connection))
                {
                    command.Parameters.AddWithValue("@name", query.TableName);
                    command.Parameters.AddWithValue("@schema", query.TableSchema);

                    await command.Connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        return await callbackAsync(reader);
                    }
                }
            }
        }
    }
}