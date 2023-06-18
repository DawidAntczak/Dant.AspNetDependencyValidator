using System.Collections.Generic;
using System.Reflection;

namespace ServiceCollectionDIValidator.Validation.Builder.AddAssembliesStage
{
    public interface IAssemblyCollectionBuilder
    {
        IAssemblyCollectionBuilder Including(Assembly assembly);
        IAssemblyCollectionBuilder Including<TFromAssembly>();
    }

    internal sealed class AssemblyCollectionBuilder : IAssemblyCollectionBuilder
    {
        public List<Assembly> Assemblies { get; } = new List<Assembly>();

        public IAssemblyCollectionBuilder Including(Assembly assembly)
        {
            Assemblies.Add(assembly);
            return this;
        }

        public IAssemblyCollectionBuilder Including<TFromAssembly>()
        {
            Assemblies.Add(typeof(TFromAssembly).Assembly);
            return this;
        }
    }
}
