using System.Text;
using DataPreview.Models;

namespace DataPreview.DataSource.SqlServer
{
    public static class QueryTemplate
    {
        public const string SelectTableSchema = @"select
            TABLE_NAME as [Name],
            TABLE_SCHEMA as [Schema],
            TABLE_TYPE as [Type]
        from INFORMATION_SCHEMA.TABLES
        where (@schema = '' or TABLE_SCHEMA = @schema) AND
            TABLE_NAME = @name AND
            TABLE_TYPE = 'BASE TABLE'";

        public const string SelectColumnInfo = @"select
            c.COLUMN_NAME as [ColumnName],
            c.DATA_TYPE as [ColumnType],
            cast((case when c.IS_NULLABLE='YES' then 1 else 0 end) as bit) as [IsNullable],
            case when c.DATA_TYPE in (
                'varchar', 'nvarchar',
                'text', 'ntext',
                'char', 'nchar',
                'binary', 'varbinary',
                'image', 'xml', 'json') then c.CHARACTER_MAXIMUM_LENGTH
            else 0 end as [MaxLength],
            cast((case when cu.COLUMN_NAME is null then 0 else 1 end) as bit) as [IsPrimaryKey]
        from INFORMATION_SCHEMA.COLUMNS c
        left join INFORMATION_SCHEMA.CONSTRAINT_TABLE_USAGE tu on
            tu.TABLE_SCHEMA=c.TABLE_SCHEMA and
            tu.TABLE_NAME=c.TABLE_NAME
        left join INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE cu on
            cu.TABLE_SCHEMA=c.TABLE_SCHEMA and
            cu.TABLE_NAME=c.TABLE_NAME and
            cu.CONSTRAINT_NAME=tu.CONSTRAINT_NAME and
            cu.COLUMN_NAME=c.COLUMN_NAME
        where c.TABLE_NAME = @name and c.TABLE_SCHEMA = @schema
        order by c.ORDINAL_POSITION";

        public static string GenerateSampleSelectStatement(Table table)
        {
            var statementBuilder = new StringBuilder("SELECT ");

            for (var i = 0; i < table.Columns.Count; i++)
            {
                if (i > 0)
                {
                    statementBuilder.Append(", ");
                }

                var column = table.Columns[i];
                var name = QuoteName(column.ColumnName);
                statementBuilder.Append(name);
            }

            statementBuilder
                .AppendLine()
                .AppendFormat("FROM {0}.{1}", QuoteName(table.Schema), QuoteName(table.Name));

            return statementBuilder.ToString();
        }

        public static string QuoteName(string name) => name;
    }
}