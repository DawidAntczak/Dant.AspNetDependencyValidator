using System;
using System.Collections.Generic;
using System.Linq;

namespace Dant.AspNetDependencyValidator.Extensions;
internal static class IEnumerableExtensions
{
    public static DisposableArray<T> ToDisposableArray<T>(this IEnumerable<T> disposables) where T : IDisposable
    {
        return new DisposableArray<T>(disposables.ToArray());
    }
}
