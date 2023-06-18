using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ServiceCollectionDIValidator.Extensions;

internal sealed record DisposableArray<T>(T[] Items) : IDisposable, IEnumerable<T> where T : IDisposable
{
    public void Dispose()
    {
        foreach (var item in Items)
        {
            item.Dispose();
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        return Items.AsEnumerable().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Items.GetEnumerator();
    }
}
