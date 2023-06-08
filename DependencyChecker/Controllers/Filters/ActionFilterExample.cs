using Microsoft.AspNetCore.Mvc.Filters;

namespace DependencyChecker.App.Controllers.Filters;

public class ActionFilterExample : Attribute, IActionFilter
{
    public ActionFilterExample(ActionFiltersDependency dependency)
    {
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
    }
}
