# BusinessRules.UnitTests

Fast, isolated unit tests for BusinessRules analyzers and code fixes using the Roslyn testing framework.

## What's Tested

- **BR001** - BusinessRuleKeyExistsAnalyzer (validates rule keys exist in JSON)
- **BR002** - RequiresValidationAnalyzer (error when validator missing)
- **BR003** - RequiresValidationAnalyzer (warning for optional rules)
- **BR004** - ThrowWithoutValidationAnalyzer (warns when throwing without attribute)
- **Code Fixes** - ThrowWithoutValidationCodeFixProvider

## Running Tests

```bash
dotnet test
```

## Test Structure

- `Analyzers/` - Tests for diagnostic analyzers
- `CodeFixes/` - Tests for code fix providers
- `Verifiers/` - Helper classes for testing framework

## Example Test

```csharp
[Test]
public async Task InvalidRuleKey_ReportsDiagnostic()
{
    var test = """
        [BusinessRule({|#0:"INVALID_KEY"|})]
        public void TestMethod() { }
        """;

    var expected = Diagnostic("BR001")
        .WithLocation(0)
        .WithArguments("INVALID_KEY");

    await VerifyAnalyzerAsync(test, expected);
}
```

## Benefits

- ✅ **Fast** - Tests run in < 2 seconds
- ✅ **Isolated** - No build pollution
- ✅ **Comprehensive** - Covers all diagnostic rules
- ✅ **Industry Standard** - Uses Microsoft.CodeAnalysis.Testing
