using System;
using DataPreview.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace DataPreview
{
    public static class DataPreviewExtensions
    {
        public static void UseDataPreview(this IApplicationBuilder appBuilder, DataPreviewOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            appBuilder.Map(options.MapPath, app =>
            {
                app.Map("/getTable", a => a.Run(new PreviewTableHandler(options)));
                app.Map("/doQuery", a => a.Run(new QueryHandler(options)));
                app.Run(new MainHandler());
            });
        }

        public static void Run(this IApplicationBuilder app, IMiddlewareHandler handler) => app.Run(handler.ExecuteAsync);
    }
}