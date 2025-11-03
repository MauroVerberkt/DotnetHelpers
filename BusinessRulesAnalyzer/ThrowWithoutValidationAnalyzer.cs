using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace BusinessRulesAnalyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ThrowWithoutValidationAnalyzer : DiagnosticAnalyzer
{
    private const string DiagnosticId = "BR004";
    private const string Category = "Usage";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        "Throwing BusinessRule without validation",
        "Throwing a BusinessRule exception without [ValidatesBusinessRule] attribute on method '{0}'",
        Category,
        DiagnosticSeverity.Warning,
        true,
        "Methods that throw BusinessRule exceptions should have the [ValidatesBusinessRule] attribute to document what rule is being validated."
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(compilationContext =>
        {
            var validatesAttrSymbol =
                compilationContext.Compilation.GetTypeByMetadataName(
                    "BusinessRules.Attributes.ValidatesBusinessRuleAttribute");
            var businessRuleFaultSymbol = 
                compilationContext.Compilation.GetTypeByMetadataName("BusinessRules.BusinessRuleFault");
            var businessRuleViolationSymbol = 
                compilationContext.Compilation.GetTypeByMetadataName("BusinessRules.BusinessRuleViolationException");

            if (validatesAttrSymbol == null || (businessRuleFaultSymbol == null && businessRuleViolationSymbol == null))
                return;

            compilationContext.RegisterSyntaxNodeAction(nodeContext =>
            {
                var throwStatement = (ThrowStatementSyntax)nodeContext.Node;
                
                // Check if we're throwing a BusinessRuleFault or derived type
                if (throwStatement.Expression == null)
                    return;

                var typeInfo = nodeContext.SemanticModel.GetTypeInfo(throwStatement.Expression);
                if (typeInfo.Type == null)
                    return;

                // Check if the thrown type is BusinessRuleFault, BusinessRuleViolationException, or FaultException<BusinessRuleFault>
                var isBusinessRuleException = false;
                var currentType = typeInfo.Type;
                
                while (currentType != null)
                {
                    if ((businessRuleFaultSymbol != null && SymbolEqualityComparer.Default.Equals(currentType, businessRuleFaultSymbol)) ||
                        (businessRuleViolationSymbol != null && SymbolEqualityComparer.Default.Equals(currentType, businessRuleViolationSymbol)))
                    {
                        isBusinessRuleException = true;
                        break;
                    }
                    
                    // Check for FaultException<BusinessRuleFault>
                    if (currentType is INamedTypeSymbol namedType && namedType.IsGenericType)
                    {
                        var typeArgs = namedType.TypeArguments;
                        if (typeArgs.Length > 0 && businessRuleFaultSymbol != null && 
                            SymbolEqualityComparer.Default.Equals(typeArgs[0], businessRuleFaultSymbol))
                        {
                            isBusinessRuleException = true;
                            break;
                        }
                    }
                    
                    currentType = currentType.BaseType;
                }

                if (!isBusinessRuleException)
                    return;

                // Find the containing method
                var methodDeclaration = throwStatement.FirstAncestorOrSelf<MethodDeclarationSyntax>();
                if (methodDeclaration == null)
                    return;

                var methodSymbol = nodeContext.SemanticModel.GetDeclaredSymbol(methodDeclaration);
                if (methodSymbol == null)
                    return;

                // Check if method has ValidatesBusinessRule attribute
                var hasValidatesAttribute = methodSymbol.GetAttributes()
                    .Any(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, validatesAttrSymbol));

                if (!hasValidatesAttribute)
                {
                    nodeContext.ReportDiagnostic(Diagnostic.Create(
                        Rule,
                        throwStatement.GetLocation(),
                        methodSymbol.Name));
                }
            }, SyntaxKind.ThrowStatement);
        });
    }
}
