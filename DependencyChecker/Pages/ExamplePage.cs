using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DependencyChecker.App.Pages;

public class ExamplePage : PageModel
{
    public ExamplePage(PagesDependency dependency)
    {
    }
}
