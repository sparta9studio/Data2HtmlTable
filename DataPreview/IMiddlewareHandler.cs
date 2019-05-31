using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace DataPreview
{
    public interface IMiddlewareHandler
    {
        Task ExecuteAsync(HttpContext context);
    }
}