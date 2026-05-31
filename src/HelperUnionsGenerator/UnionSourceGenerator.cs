using Microsoft.CodeAnalysis;

namespace HelperUnionsGenerator;

/// <summary>
/// Generates HelperUnions source code for union declarations.
/// </summary>
[Generator]
public sealed class UnionSourceGenerator : IIncrementalGenerator
{
    /// <summary>
    /// Initializes the source generator pipeline.
    /// </summary>
    /// <param name="context">Generator initialization context.</param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
    }
}
