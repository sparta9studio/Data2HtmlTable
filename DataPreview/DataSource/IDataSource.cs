using System.Data;
using System.Threading.Tasks;
using DataPreview.Models;

namespace DataPreview.DataSource
{
    public interface IDataSource
    {
        Task<Table> GetTableInfoAsync(string name);

        string GenerateSampleSql(Table table);

        Task<DataTable> QueryAsync(string sql, DataPreviewOptions options);

        Task<DataTable> GetSampleDataAsync(Table table, DataPreviewOptions options);
    }
}