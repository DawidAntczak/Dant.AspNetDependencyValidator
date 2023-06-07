using System;
using System.Reflection;
using Dant.AspNetDependencyValidator.Validation.Builder.AddAssembliesStage;

namespace Dant.AspNetDependencyValidator.Validation.Builder.Stages;

public interface ITypesPassedMethodStage
{
    ITypesPassedPositionStage To(MethodInfo methodWithGenericParameters);
}

public interface ITypesPassedPositionStage
{
    IValidationCollectionBuilder AtPosition(int parameterPosition);
}

internal sealed class TypesPassedBuilder : ITypesPassedMethodStage, ITypesPassedPositionStage
{
    private readonly ValidationCollectionBuilder _context;
    private readonly Action<TypesPassedBuilder> _onFinish;

    public MethodInfo MethodWithGenericParameters { get; private set; }
    public int ParameterPosition { get; private set; } = 0;

    public TypesPassedBuilder(ValidationCollectionBuilder context, Action<TypesPassedBuilder> onFinish)
    {
        _context = context;
        _onFinish = onFinish;
    }

    public ITypesPassedPositionStage To(MethodInfo methodWithGenericParameters)
    {
        MethodWithGenericParameters = methodWithGenericParameters;
        return this;
    }

    public IValidationCollectionBuilder AtPosition(int parameterPosition)
    {
        _onFinish(this);
        return _context;
    }
}
