using BusinessRules.Attributes;
using BusinessRules.Rules.Authentication;
using BusinessRules.Rules.Authorization;
using BusinessRules.Rules.General;
using BusinessRules.Rules.Validation;

namespace BusinessRules.IntegrationTests;

// ============================================
// VALIDATORS (define these first)
// ============================================

public class GlobalValidators
{
    [ImplementsBusinessRule(UserMustBeAuthenticated.Key)]
    public void ValidateUserAuth()
    {
        Console.WriteLine("✓ Validates USER_AUTH");
    }

    [ImplementsBusinessRule(UserMustBeAdmin.Key)]
    public void ValidateUserAdmin()
    {
        Console.WriteLine("✓ Validates USER_ADMIN");
    }

    [ImplementsBusinessRule(AgeMinimum.Key)]
    public void ValidateAgeMinimum()
    {
        Console.WriteLine("✓ Validates AGE_MIN");
    }
}

// ============================================
// VALID CASES (should have NO errors/warnings)
// ============================================

public class ValidCases
{
    [BusinessRule(UserMustBeAuthenticated.Key)]
    public void ValidMethod_HasMatchingValidator()
    {
        Console.WriteLine("Valid: USER_AUTH validator exists in GlobalValidators");
    }

    [BusinessRule(UserMustBeAdmin.Key)]
    public void AnotherValidMethod()
    {
        Console.WriteLine("Valid: USER_ADMIN validator exists in GlobalValidators");
    }
}

[BusinessRule(AgeMinimum.Key)]
public class ValidClassHasMatchingValidator
{
    public void SomeMethod()
    {
        Console.WriteLine("Valid: AGE_MIN validator exists in GlobalValidators");
    }
}

// ============================================
// INVALID CASES - ERROR (BR001)
// COMMENTED OUT so solution builds cleanly
// Uncomment these to test analyzer BR002 errors
// ============================================

public class Br001ErrorValidatorCases
{
    [ImplementsBusinessRule("USER_OWNER")]
    public void ValidateUserAuth()
    {
        Console.WriteLine("✓ Validates USER_OWNER");
    }
}

public class Br001ErrorCases
{
    [BusinessRule("USER_OWNER")]
    public void ValidMethod_HasMatchingValidator()
    {
        Console.WriteLine("Valid: USER_OWNER validator exists in GlobalValidators");
    }
}

[BusinessRule("USER_OWNER")]
public class Br001ErrorClassHasMatchingValidator
{
    public void SomeMethod()
    {
        Console.WriteLine("Valid: USER_OWNER validator exists in GlobalValidators");
    }
}

// ============================================
// INVALID CASES - ERROR (BR002)
// COMMENTED OUT so solution builds cleanly
// Uncomment these to test analyzer BR002 errors
// ============================================

public class Br002ErrorCases
{
    // ❌ BR002 ERROR - No validator exists for "PAYMENT_VERIFIED"
    [BusinessRule("PAYMENT_VERIFIED")]
    public void Error_NoValidator_DefaultEnforce()
    {
        Console.WriteLine("ERROR: No ImplementsBusinessRule for PAYMENT_VERIFIED exists");
    }

    // ❌ BR002 ERROR - No validator exists for "DATA_ENCRYPTED"
    [BusinessRule("DATA_ENCRYPTED", enforceValidation: true)]
    public void Error_NoValidator_ExplicitEnforce()
    {
        Console.WriteLine("ERROR: No ImplementsBusinessRule for DATA_ENCRYPTED exists");
    }
}

// ❌ BR002 ERROR - No validator exists for "INVENTORY_AVAILABLE"
[BusinessRule("INVENTORY_AVAILABLE")]
public class Br002ErrorClassNoValidator
{
    public void SomeMethod()
    {
        Console.WriteLine("ERROR: No ImplementsBusinessRule for INVENTORY_AVAILABLE exists");
    }
}

// ============================================
// INVALID CASES - WARNING (BR003)
// COMMENTED OUT to allow clean build
// Uncomment to test BR003 warnings
// ============================================

public class WarningCases
{
    // ⚠️ BR003 WARNING - No validator exists for "EMAIL_VERIFIED"
    [BusinessRule(EmailVerified.Key, enforceValidation: false)]
    public void Warning_NoValidator_EnforceFalse()
    {
        Console.WriteLine("WARNING: No ImplementsBusinessRule for EMAIL_VERIFIED exists");
    }

    // ⚠️ BR003 WARNING - No validator exists for "TERMS_ACCEPTED"
    [BusinessRule(TermsAccepted.Key, enforceValidation: false)]
    public void AnotherWarning()
    {
        Console.WriteLine("WARNING: No ImplementsBusinessRule for TERMS_ACCEPTED exists");
    }
}

// ⚠️ BR003 WARNING - No validator exists for "SESSION_ACTIVE"
[BusinessRule(SessionActive.Key, enforceValidation: false)]
public class WarningClassNoValidator
{
    public void SomeMethod()
    {
        Console.WriteLine("WARNING: No ImplementsBusinessRule for SESSION_ACTIVE exists");
    }
}

// ============================================
// BR004 TEST CASES - Throw without ImplementsBusinessRule
// ============================================

public class ThrowWithoutValidationTests
{
    // VALID: Has [ImplementsBusinessRule] attribute
    [ImplementsBusinessRule(UserMustBeAuthenticated.Key)]
    public void ValidCase_ThrowsWithAttribute_SOAP()
    {
        throw UserMustBeAuthenticated.ToFaultException();
    }

    [ImplementsBusinessRule(UserMustBeAdmin.Key)]
    public void ValidCase_ThrowsWithAttribute_REST()
    {
        throw UserMustBeAdmin.ToException();
    }

    // ⚠️ BR004 WARNING - Missing [ImplementsBusinessRule] attribute
    public void InvalidCase_ThrowsWithoutAttribute_SOAP()
    {
        throw UserMustBeAuthenticated.ToFaultException();
    }

    // ⚠️ BR004 WARNING - Missing [ImplementsBusinessRule] attribute
    public void InvalidCase_ThrowsWithoutAttribute_REST()
    {
        throw UserMustBeAdmin.ToException();
    }
}
