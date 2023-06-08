using Microsoft.AspNetCore.Mvc.Filters;

namespace DependencyChecker.App.Pages.Filters;

public class AsyncPageFilterExample : Attribute, IAsyncPageFilter
{
    public AsyncPageFilterExample(PageFiltersDependency dependency)
    { 
    }

    public Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        return Task.CompletedTask;
    }

    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
    {
        return Task.CompletedTask;
    }
}
