using Microsoft.AspNetCore.Mvc.Filters;

namespace DependencyChecker.App.Controllers.Filters;

public class ActionFilterAttributeExample : ActionFilterAttribute
{
    public ActionFilterAttributeExample(ActionFiltersDependency dependency)
    {
    }
}
