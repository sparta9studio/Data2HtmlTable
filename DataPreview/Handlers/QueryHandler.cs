using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace DataPreview.Handlers
{
    public class QueryHandler : IMiddlewareHandler
    {
        private readonly DataPreviewOptions options;

        public QueryHandler(DataPreviewOptions options)
        {
            this.options = options;
        }

        public async Task ExecuteAsync(HttpContext context)
        {
            var query = context.Request.Query["query"].ToString().Trim();
            if (string.IsNullOrWhiteSpace(query))
            {
                await ToJsonAsync(context, new
                {
                    IsSuccessful = false,
                    Message = "Invalid query"
                });
                return;
            }

            var dataSource = options.DataSource;
            var data = await dataSource.QueryAsync(query, this.options);

            await ToJsonAsync(context, new
            {
                Columns = data.Columns.OfType<System.Data.DataColumn>().Select(r => new { r.ColumnName }),
                Data = data,
                Sql = query
            });
        }

        private static Task ToJsonAsync<T>(HttpContext context, T value)
        {
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync(JsonConvert.SerializeObject(value));
        }
    }
}