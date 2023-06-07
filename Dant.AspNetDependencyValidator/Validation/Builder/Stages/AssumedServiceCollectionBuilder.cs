using System;
using System.Collections.Generic;

namespace Dant.AspNetDependencyValidator.Validation.Builder.AddAssembliesStage
{
    public interface IAssumedServiceCollectionBuilder
    {
        IAssumedServiceCollectionBuilder Including<T>();
        IAssumedServiceCollectionBuilder Including(Type type);
    }
    
    internal sealed class AssumedServiceCollectionBuilder : IAssumedServiceCollectionBuilder
    {
        public HashSet<Type> AssumedExistingServices { get; } = new HashSet<Type>();

        public IAssumedServiceCollectionBuilder Including<T>()
        {
            AssumedExistingServices.Add(typeof(T));
            return this;
        }

        public IAssumedServiceCollectionBuilder Including(Type type)
        {
            AssumedExistingServices.Add(type);
            return this;
        }
    }
}
