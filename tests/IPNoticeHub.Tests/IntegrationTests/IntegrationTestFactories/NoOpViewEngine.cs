using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace IPNoticeHub.Tests.IntegrationTests.IntegrationTestFactories
{
    public sealed class NoOpView : IView
    {
        public string Path => "NoOpView";
        public Task RenderAsync(ViewContext context) => Task.CompletedTask;
    }

    public sealed class NoOpViewEngine : IViewEngine
    {
        private static readonly ViewEngineResult viewEngine =
            ViewEngineResult.Found(
                "NoOpView", 
                new NoOpView());

        public ViewEngineResult GetView(
            string? executingFilePath, 
            string viewPath, 
            bool isMainPage) => viewEngine;
        
        public ViewEngineResult FindView(
            ActionContext context, 
            string viewName, 
            bool isMainPage) 
            => viewEngine;
    }
}
