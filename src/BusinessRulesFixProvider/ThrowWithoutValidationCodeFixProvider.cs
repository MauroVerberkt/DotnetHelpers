using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BusinessRulesFixProvider;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ThrowWithoutValidationCodeFixProvider)), Shared]
public class ThrowWithoutValidationCodeFixProvider : CodeFixProvider
{
    private const string Title = "Add [ImplementsBusinessRule] attribute";

    public sealed override ImmutableArray<string> FixableDiagnosticIds => ["BR004"];

    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null)
            return;

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var throwStatement = root.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf()
            .OfType<ThrowStatementSyntax>().FirstOrDefault();
        if (throwStatement == null)
            return;

        var methodDeclaration = throwStatement.FirstAncestorOrSelf<MethodDeclarationSyntax>();
        if (methodDeclaration == null)
            return;

        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
        if (semanticModel == null)
            return;

        var (ruleKey, className) = TryExtractRuleInfo(throwStatement, semanticModel);

        context.RegisterCodeFix(
            CodeAction.Create(
                title: className != null ? $"Add [ImplementsBusinessRule({className}.Key)]" : 
                       ruleKey != null ? $"Add [ImplementsBusinessRule(\"{ruleKey}\")]" : Title,
                createChangedDocument: c => AddValidatesAttributeAsync(context.Document, methodDeclaration, ruleKey, className, c),
                equivalenceKey: Title),
            diagnostic);
    }

    private (string? ruleKey, string? className) TryExtractRuleInfo(ThrowStatementSyntax throwStatement, SemanticModel semanticModel)
    {
        // Handle: throw SomeRule.ToException() or throw SomeRule.ToFaultException()
        if (throwStatement.Expression is InvocationExpressionSyntax invocation &&
            invocation.Expression is MemberAccessExpressionSyntax { Name.Identifier.Text: "ToException" or "ToFaultException" } memberAccess)
        {
            var typeSymbol = semanticModel.GetTypeInfo(memberAccess.Expression).Type;
            if (typeSymbol != null)
            {
                var keyField = typeSymbol.GetMembers("Key").OfType<IFieldSymbol>().FirstOrDefault();
                if (keyField?.ConstantValue is string key)
                    return (key, typeSymbol.Name);
            }
        }

        // Handle: throw new BusinessRuleException(...)
        if (throwStatement.Expression is not ObjectCreationExpressionSyntax objectCreation)
            return (null, null);

        var firstArg = objectCreation.ArgumentList?.Arguments.FirstOrDefault();
        if (firstArg == null)
            return (null, null);

        var constantValue = semanticModel.GetConstantValue(firstArg.Expression);
        if (constantValue.HasValue && constantValue.Value is string ruleKey)
            return (ruleKey, null);

        if (firstArg.Expression is MemberAccessExpressionSyntax memberAccessArg &&
            memberAccessArg.Name.Identifier.Text == "Key")
        {
            var symbol = semanticModel.GetSymbolInfo(memberAccessArg).Symbol;
            if (symbol is IFieldSymbol fieldSymbol)
            {
                var fieldValue = fieldSymbol.ConstantValue;
                if (fieldValue is string key)
                    return (key, fieldSymbol.ContainingType?.Name);
            }
        }

        return (null, null);
    }

    private async Task<Document> AddValidatesAttributeAsync(
        Document document,
        MethodDeclarationSyntax methodDeclaration,
        string? ruleKey,
        string? className,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null)
            return document;

        AttributeListSyntax attributeList;
        
        if (className != null)
        {
            // Generate: [ImplementsBusinessRule(ClassName.Key)]
            var memberAccess = SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.IdentifierName(className),
                SyntaxFactory.IdentifierName("Key"));
                
            attributeList = SyntaxFactory.AttributeList(
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Attribute(
                        SyntaxFactory.IdentifierName("ImplementsBusinessRule"),
                        SyntaxFactory.AttributeArgumentList(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.AttributeArgument(memberAccess))))));
        }
        else if (ruleKey != null)
        {
            // Generate: [ImplementsBusinessRule("RULE_KEY")]
            attributeList = SyntaxFactory.AttributeList(
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Attribute(
                        SyntaxFactory.IdentifierName("ImplementsBusinessRule"),
                        SyntaxFactory.AttributeArgumentList(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.AttributeArgument(
                                    SyntaxFactory.LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        SyntaxFactory.Literal(ruleKey))))))));
        }
        else
        {
            // Generate: [ImplementsBusinessRule]
            attributeList = SyntaxFactory.AttributeList(
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Attribute(
                        SyntaxFactory.IdentifierName("ImplementsBusinessRule"))));
        }

        var leadingTrivia = methodDeclaration.GetLeadingTrivia();
        var attributeWithTrivia = attributeList.WithLeadingTrivia(leadingTrivia).WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);
        var newMethodDeclaration = methodDeclaration.WithLeadingTrivia(SyntaxFactory.TriviaList()).AddAttributeLists(attributeWithTrivia);
        var newRoot = root.ReplaceNode(methodDeclaration, newMethodDeclaration);

        return document.WithSyntaxRoot(newRoot);
    }
}
