using BusinessRules.UnitTests.Verifiers;
using BusinessRulesAnalyzer;
using BusinessRulesFixProvider;
using Microsoft.CodeAnalysis;

namespace BusinessRules.UnitTests.CodeFixes;

[TestFixture]
public class ThrowWithoutValidationCodeFixProviderTests
{
    [Test]
    public async Task CodeFix_AddsAttributeWithClassName()
    {
        var source = """
                     using BusinessRules;
                     using BusinessRules.Rules.Authentication;

                     public class TestClass
                     {
                         public void SomeMethod()
                         {
                             {|#0:throw UserMustBeAuthenticated.ToException();|}
                         }
                     }
                     """;

        var fixedSource = $$"""
                          using BusinessRules;
                          using BusinessRules.Rules.Authentication;
                          using BusinessRules.Attributes;

                          public class TestClass
                          {
                              [ImplementsBusinessRule(UserMustBeAuthenticated.Key)]
                              public void SomeMethod()
                              {
                                  throw UserMustBeAuthenticated.ToException();
                              }
                          }
                          """;

        var expected = CSharpCodeFixVerifier<ThrowWithoutValidationAnalyzer, ThrowWithoutValidationCodeFixProvider>
            .Diagnostic("BR004")
            .WithLocation(0)
            .WithArguments("SomeMethod")
            .WithSeverity(DiagnosticSeverity.Warning);

        await CSharpCodeFixVerifier<ThrowWithoutValidationAnalyzer, ThrowWithoutValidationCodeFixProvider>
            .VerifyCodeFixWithGeneratedCodeAsync(source, fixedSource, expected);
    }

    [Test]
    public async Task CodeFix_AddsAttributeWithClassName_ToException()
    {
        var source = """
                     using BusinessRules;
                     using BusinessRules.Rules.Authentication;

                     public class TestClass
                     {
                         public void SomeMethod()
                         {
                             {|#0:throw UserMustBeAuthenticated.ToException();|}
                         }
                     }
                     """;

        var fixedSource = """
                          using BusinessRules;
                          using BusinessRules.Rules.Authentication;
                          using BusinessRules.Attributes;

                          public class TestClass
                          {
                              [ImplementsBusinessRule(UserMustBeAuthenticated.Key)]
                              public void SomeMethod()
                              {
                                  throw UserMustBeAuthenticated.ToException();
                              }
                          }
                          """;

        var expected = CSharpCodeFixVerifier<ThrowWithoutValidationAnalyzer, ThrowWithoutValidationCodeFixProvider>
            .Diagnostic("BR004")
            .WithLocation(0)
            .WithArguments("SomeMethod")
            .WithSeverity(DiagnosticSeverity.Warning);

        await CSharpCodeFixVerifier<ThrowWithoutValidationAnalyzer, ThrowWithoutValidationCodeFixProvider>
            .VerifyCodeFixWithGeneratedCodeAsync(source, fixedSource, expected);
    }

    [Test]
    public async Task CodeFix_AddsAttributeWithClassName_ToFaultException()
    {
        var source = """
                     using BusinessRules;
                     using BusinessRules.Rules.Authentication;

                     public class TestClass
                     {
                         public void SomeMethod()
                         {
                             {|#0:throw UserMustBeAuthenticated.ToFaultException();|}
                         }
                     }
                     """;

        var fixedSource = """
                          using BusinessRules;
                          using BusinessRules.Rules.Authentication;
                          using BusinessRules.Attributes;

                          public class TestClass
                          {
                              [ImplementsBusinessRule(UserMustBeAuthenticated.Key)]
                              public void SomeMethod()
                              {
                                  throw UserMustBeAuthenticated.ToFaultException();
                              }
                          }
                          """;

        var expected = CSharpCodeFixVerifier<ThrowWithoutValidationAnalyzer, ThrowWithoutValidationCodeFixProvider>
            .Diagnostic("BR004")
            .WithLocation(0)
            .WithArguments("SomeMethod")
            .WithSeverity(DiagnosticSeverity.Warning);

        await CSharpCodeFixVerifier<ThrowWithoutValidationAnalyzer, ThrowWithoutValidationCodeFixProvider>
            .VerifyCodeFixWithGeneratedCodeAsync(source, fixedSource, expected);
    }

    [Test]
    public async Task CodeFix_PreservesExistingAttributes()
    {
        var source = """
                     using BusinessRules;
                     using BusinessRules.Rules.Authentication;
                     using System;

                     public class TestClass
                     {
                         [Obsolete]
                         public void SomeMethod()
                         {
                             {|#0:throw UserMustBeAuthenticated.ToException();|}
                         }
                     }
                     """;

        var fixedSource = """
                          using BusinessRules;
                          using BusinessRules.Rules.Authentication;
                          using System;
                          using BusinessRules.Attributes;

                          public class TestClass
                          {
                              [ImplementsBusinessRule(UserMustBeAuthenticated.Key)]
                              [Obsolete]
                              public void SomeMethod()
                              {
                                  throw UserMustBeAuthenticated.ToException();
                              }
                          }
                          """;

        var expected = CSharpCodeFixVerifier<ThrowWithoutValidationAnalyzer, ThrowWithoutValidationCodeFixProvider>
            .Diagnostic("BR004")
            .WithLocation(0)
            .WithArguments("SomeMethod")
            .WithSeverity(DiagnosticSeverity.Warning);

        await CSharpCodeFixVerifier<ThrowWithoutValidationAnalyzer, ThrowWithoutValidationCodeFixProvider>
            .VerifyCodeFixWithGeneratedCodeAsync(source, fixedSource, expected);
    }
}