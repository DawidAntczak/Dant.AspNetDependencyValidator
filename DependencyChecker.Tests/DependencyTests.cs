using Dant.AspNetDependencyValidator;
using DependencyChecker.App;

namespace DependencyChecker.Tests;

[TestFixture]
public class DependencyTests
{
    [Test]
    public void ValidateDependencies()
    {
        AspNetDependenciesValidator.Validate<IApiMarker>();
    }
}
