using Microsoft.AspNetCore.Mvc.Filters;

namespace DependencyChecker.App.Controllers.Filters;

public class AsyncActionFilterExample : Attribute, IAsyncActionFilter
{
    public AsyncActionFilterExample(ActionFiltersDependency dependency)
    {
    }

    public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        return Task.CompletedTask;
    }
}
