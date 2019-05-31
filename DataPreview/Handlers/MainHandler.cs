using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Reflection;

namespace DataPreview.Handlers
{
    public class MainHandler : IMiddlewareHandler
    {
        public Task ExecuteAsync(HttpContext context)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceStream = assembly.GetManifestResourceStream("DataPreview.Assets.dist.index.html");
            var response = context.Response;

            response.StatusCode = 200;
            response.ContentType = "text/html; charset=utf-8";

            return resourceStream.CopyToAsync(response.Body);
        }
    }
}