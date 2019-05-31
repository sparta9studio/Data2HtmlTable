using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace DataPreview.Handlers
{
    public class PreviewTableHandler : IMiddlewareHandler
    {
        private readonly DataPreviewOptions options;

        public PreviewTableHandler(DataPreviewOptions options)
        {
            this.options = options;
        }

        public async Task ExecuteAsync(HttpContext context)
        {
            var name = context.Request.Query["tableName"].ToString().Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                await ToJsonAsync(context, new
                {
                    IsSuccessful = false,
                    Message = "Table name is required."
                });
                return;
            }

            var dataSource = options.DataSource;
            var table = await dataSource.GetTableInfoAsync(name);
            var sql = dataSource.GenerateSampleSql(table);
            var data = await dataSource.QueryAsync(sql, this.options);

            await ToJsonAsync(context, new
            {
                Columns = table.Columns,
                Data = data,
                Sql = sql
            });
        }

        private static Task ToJsonAsync<T>(HttpContext context, T value)
        {
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync(JsonConvert.SerializeObject(value));
        }
    }
}