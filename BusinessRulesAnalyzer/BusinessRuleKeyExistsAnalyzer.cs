using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace BusinessRulesAnalyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BusinessRuleKeyExistsAnalyzer : DiagnosticAnalyzer
{
    private const string DiagnosticId = "BR001";
    private const string Category = "Usage";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        "Business rule key not found",
        "Business rule with key '{0}' is not defined in any BusinessRule field",
        Category,
        DiagnosticSeverity.Error,
        true,
        "All business rule keys used in ValidatesBusinessRule or RequiresBusinessRule attributes must be defined as a BusinessRule field.",
        customTags: []
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
            var requiresAttrSymbol =
                compilationContext.Compilation.GetTypeByMetadataName(
                    "BusinessRules.Attributes.RequiresBusinessRuleAttribute");
            var businessRuleSymbol = compilationContext.Compilation.GetTypeByMetadataName("BusinessRules.BusinessRule");

            if (validatesAttrSymbol == null || requiresAttrSymbol == null || businessRuleSymbol == null)
                return;

            var definedRuleKeys = new ConcurrentDictionary<string, byte>(StringComparer.Ordinal);
            var reportedKeys = new ConcurrentDictionary<(FileLinePositionSpan, string), byte>();

            // --- PRE-SCAN ALL BUSINESS RULE FIELDS ACROSS THE COMPILATION ---
            foreach (var tree in compilationContext.Compilation.SyntaxTrees)
            {
                var model = compilationContext.Compilation.GetSemanticModel(tree);
                var fieldDeclarations = tree.GetRoot().DescendantNodes().OfType<FieldDeclarationSyntax>();

                foreach (var fieldDecl in fieldDeclarations)
                foreach (var variable in fieldDecl.Declaration.Variables)
                {
                    var symbol = model.GetDeclaredSymbol(variable) as IFieldSymbol;
                    if (symbol == null || !SymbolEqualityComparer.Default.Equals(symbol.Type, businessRuleSymbol))
                        continue;

                    // --- FIX: handle ObjectCreationExpressionSyntax and ImplicitObjectCreationExpressionSyntax separately ---
                    BaseObjectCreationExpressionSyntax? creation = null;
                    var value = variable.Initializer?.Value;

                    creation = value switch
                    {
                        ObjectCreationExpressionSyntax o => o,
                        ImplicitObjectCreationExpressionSyntax i => i,
                        _ => creation
                    };

                    var firstArg = creation?.ArgumentList?.Arguments.FirstOrDefault();
                    if (firstArg != null &&
                        (firstArg.NameColon == null ||
                         firstArg.NameColon.Name.Identifier.ValueText.Equals("key",
                             StringComparison.OrdinalIgnoreCase)) &&
                        firstArg.Expression is LiteralExpressionSyntax literal &&
                        literal.IsKind(SyntaxKind.StringLiteralExpression))
                        definedRuleKeys.TryAdd(literal.Token.ValueText, 0);
                }
            }

            // --- COLLECT ATTRIBUTES ---
            compilationContext.RegisterSyntaxNodeAction(nodeContext =>
            {
                if (nodeContext.Node is not AttributeSyntax attribute)
                    return;

                if (nodeContext.SemanticModel.GetTypeInfo(attribute).Type is not INamedTypeSymbol attrType)
                    return;

                if (!SymbolEqualityComparer.Default.Equals(attrType, validatesAttrSymbol) &&
                    !SymbolEqualityComparer.Default.Equals(attrType, requiresAttrSymbol))
                    return;

                if (attribute.ArgumentList?.Arguments.FirstOrDefault()?.Expression is not LiteralExpressionSyntax literalExpr)
                    return;

                var ruleKey = literalExpr.Token.ValueText;

                var locationSpan = literalExpr.GetLocation().GetLineSpan();
                if (!definedRuleKeys.ContainsKey(ruleKey) &&
                    reportedKeys.TryAdd((locationSpan, ruleKey), 0))
                    nodeContext.ReportDiagnostic(Diagnostic.Create(Rule, literalExpr.GetLocation(), ruleKey));
            }, SyntaxKind.Attribute);
        });
    }
}