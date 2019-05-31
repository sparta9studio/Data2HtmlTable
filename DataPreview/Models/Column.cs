namespace DataPreview.Models
{
    public class Column
    {
        public string ColumnName { get; set; }

        public string ColumnType { get; set; }

        public bool IsNullable { get; set; }

        public int MaxLength { get; set; }

        public bool IsPrimaryKey { get; set; }
    }
}