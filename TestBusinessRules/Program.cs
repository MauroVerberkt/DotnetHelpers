using TestBusinessRules;

Console.WriteLine("Business Rules Analyzer - Test Cases");
Console.WriteLine("=====================================\n");

Console.WriteLine("Look at TestCases.cs in your IDE to see:");
Console.WriteLine("  🔴 RED squiggles (BR002 errors) - lines 68, 74, 83");
Console.WriteLine("  🟡 YELLOW squiggles (BR003 warnings) - lines 97, 103, 112");
Console.WriteLine("  ✅ No squiggles on valid cases - lines 38, 44, 51\n");

Console.WriteLine("VALID CASES (no diagnostics):");
var validCases = new ValidCases();
validCases.ValidMethod_HasMatchingValidator();
validCases.AnotherValidMethod();

var validClass = new ValidClassHasMatchingValidator();
validClass.SomeMethod();

Console.WriteLine("\nERROR CASES (should show BR002 errors in IDE):");
Console.WriteLine("  - ErrorCases.Error_NoValidator_DefaultEnforce");
Console.WriteLine("  - ErrorCases.Error_NoValidator_ExplicitEnforce");
Console.WriteLine("  - ErrorClass_NoValidator");

Console.WriteLine("\nWARNING CASES (should show BR003 warnings in IDE):");
Console.WriteLine("  - WarningCases.Warning_NoValidator_EnforceFalse");
Console.WriteLine("  - WarningCases.AnotherWarning");
Console.WriteLine("  - WarningClass_NoValidator");

Console.WriteLine("\n✓ Open the Error List window (View → Error List) to see all diagnostics!");
Console.WriteLine("✓ Open TestCases.cs and look for red/yellow underlines on [RequiresBusinessRule] attributes");
