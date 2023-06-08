using Microsoft.AspNetCore.Mvc.Filters;

namespace DependencyChecker.App.Pages.Filters;

public class PageFilterExample : Attribute, IPageFilter
{
    public PageFilterExample(PageFiltersDependency dependency)
    { 
    }

    public void OnPageHandlerExecuted(PageHandlerExecutedContext context)
    {
    }

    public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
    }

    public void OnPageHandlerSelected(PageHandlerSelectedContext context)
    {
    }
}
