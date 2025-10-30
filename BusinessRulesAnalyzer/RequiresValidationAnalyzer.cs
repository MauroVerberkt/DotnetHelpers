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
public class RequiresValidationAnalyzer : DiagnosticAnalyzer
{
    private const string DiagnosticIdError = "BR002";
    private const string DiagnosticIdWarning = "BR003";
    private const string Category = "Usage";

    private static readonly DiagnosticDescriptor ErrorRule = new(
        DiagnosticIdError,
        "Missing ValidatesBusinessRule attribute",
        "Business rule '{0}' is required but not validated anywhere in the compilation",
        Category,
        DiagnosticSeverity.Error,
        true,
        "When RequiresBusinessRule has enforceValidation=true, a ValidatesBusinessRule with the same ruleKey must exist in the compilation.",
        customTags: [WellKnownDiagnosticTags.CompilationEnd]
    );

    private static readonly DiagnosticDescriptor WarningRule = new(
        DiagnosticIdWarning,
        "Missing ValidatesBusinessRule attribute",
        "Business rule '{0}' is required but not validated anywhere in the compilation",
        Category,
        DiagnosticSeverity.Warning,
        true,
        "When RequiresBusinessRule has enforceValidation=false, a ValidatesBusinessRule with the same ruleKey should exist in the compilation.",
        customTags: [WellKnownDiagnosticTags.CompilationEnd]
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [ErrorRule, WarningRule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(compilationContext =>
        {
            var requiresAttrSymbol =
                compilationContext.Compilation.GetTypeByMetadataName(
                    "BusinessRules.Attributes.RequiresBusinessRuleAttribute");
            var validatesAttrSymbol =
                compilationContext.Compilation.GetTypeByMetadataName(
                    "BusinessRules.Attributes.ValidatesBusinessRuleAttribute");

            if (requiresAttrSymbol == null || validatesAttrSymbol == null)
                return;

            var validatedKeys = new ConcurrentDictionary<string, byte>(StringComparer.Ordinal);
            var requiredRules = new ConcurrentBag<(string RuleKey, bool EnforceValidation, Location Location)>();
            var reportedKeys = new ConcurrentDictionary<(FileLinePositionSpan, string), byte>();

            // --- PRE-SCAN ALL VALIDATESBUSINESSRULE ATTRIBUTES ---
            foreach (var tree in compilationContext.Compilation.SyntaxTrees)
            {
                var model = compilationContext.Compilation.GetSemanticModel(tree);
                var attributes = tree.GetRoot().DescendantNodes().OfType<AttributeSyntax>();

                foreach (var attr in attributes)
                {
                    var type = model.GetTypeInfo(attr).Type as INamedTypeSymbol;
                    if (type == null || !SymbolEqualityComparer.Default.Equals(type, validatesAttrSymbol))
                        continue;

                    if (attr.ArgumentList?.Arguments.FirstOrDefault()
                            ?.Expression is LiteralExpressionSyntax literalExpr)
                        validatedKeys.TryAdd(literalExpr.Token.ValueText, 0);
                }
            }

            // --- COLLECT REQUIRED RULES ---
            compilationContext.RegisterSymbolAction(symbolContext =>
            {
                var symbol = symbolContext.Symbol;
                foreach (var attr in symbol.GetAttributes().Where(attr =>
                             SymbolEqualityComparer.Default.Equals(attr.AttributeClass, requiresAttrSymbol)))
                {
                    if (attr.ConstructorArguments.Length == 0 ||
                        !(attr.ConstructorArguments[0].Value is string ruleKey))
                        continue;

                    var enforceValidation = true;
                    if (attr.ConstructorArguments.Length > 1 && attr.ConstructorArguments[1].Value is bool enforce)
                    {
                        enforceValidation = enforce;
                    }
                    else
                    {
                        var namedArg = attr.NamedArguments.FirstOrDefault(kvp =>
                            kvp.Key == "enforceValidation" || kvp.Key == "EnforceValidation");
                        if (namedArg.Key != null && namedArg.Value.Value is bool namedEnforce)
                            enforceValidation = namedEnforce;
                    }

                    var location = attr.ApplicationSyntaxReference?.GetSyntax().GetLocation() ??
                                   symbol.Locations.FirstOrDefault();
                    if (location == null) continue;
                    requiredRules.Add((ruleKey, enforceValidation, location));

                    // Partial/live diagnostic
                    var locationSpan = location.GetLineSpan();
                    if (!validatedKeys.ContainsKey(ruleKey) &&
                        reportedKeys.TryAdd((locationSpan, ruleKey), 0))
                        symbolContext.ReportDiagnostic(Diagnostic.Create(
                            enforceValidation ? ErrorRule : WarningRule,
                            location,
                            ruleKey));
                }
            }, SymbolKind.Method, SymbolKind.NamedType);

            // --- FULL COMPILATION-END DIAGNOSTICS ---
            compilationContext.RegisterCompilationEndAction(endContext =>
            {
                foreach (var (ruleKey, enforceValidation, location) in requiredRules)
                {
                    var locationSpan = location.GetLineSpan();
                    if (!validatedKeys.ContainsKey(ruleKey) &&
                        reportedKeys.TryAdd((locationSpan, ruleKey), 0))
                        endContext.ReportDiagnostic(Diagnostic.Create(
                            enforceValidation ? ErrorRule : WarningRule,
                            location,
                            ruleKey));
                }
            });
        });
    }
}