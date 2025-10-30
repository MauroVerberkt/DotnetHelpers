using BusinessRules.Attributes;

namespace TestBusinessRules;

// ============================================
// VALIDATORS (define these first)
// ============================================

public class GlobalValidators
{
    [ValidatesBusinessRule("USER_AUTH")]
    public void ValidateUserAuth()
    {
        Console.WriteLine("✓ Validates USER_AUTH");
    }

    [ValidatesBusinessRule("USER_ADMIN")]
    public void ValidateUserAdmin()
    {
        Console.WriteLine("✓ Validates USER_ADMIN");
    }

    [ValidatesBusinessRule("AGE_MIN")]
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
    [RequiresBusinessRule("USER_AUTH")]
    public void ValidMethod_HasMatchingValidator()
    {
        Console.WriteLine("Valid: USER_AUTH validator exists in GlobalValidators");
    }

    [RequiresBusinessRule("USER_ADMIN")]
    public void AnotherValidMethod()
    {
        Console.WriteLine("Valid: USER_ADMIN validator exists in GlobalValidators");
    }
}

[RequiresBusinessRule("AGE_MIN")]
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
    [ValidatesBusinessRule("USER_OWNER", ["ValidMethod_HasMatchingValidator"])]
    public void ValidateUserAuth()
    {
        Console.WriteLine("✓ Validates USER_OWNER");
    }
}

public class Br001ErrorCases
{
    [RequiresBusinessRule("USER_OWNER")]
    public void ValidMethod_HasMatchingValidator()
    {
        Console.WriteLine("Valid: USER_OWNER validator exists in GlobalValidators");
    }
}

[RequiresBusinessRule("USER_OWNER")]
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
    [RequiresBusinessRule("PAYMENT_VERIFIED")]
    public void Error_NoValidator_DefaultEnforce()
    {
        Console.WriteLine("ERROR: No ValidatesBusinessRule for PAYMENT_VERIFIED exists");
    }

    // ❌ BR002 ERROR - No validator exists for "DATA_ENCRYPTED"
    [RequiresBusinessRule("DATA_ENCRYPTED", enforceValidation: true)]
    public void Error_NoValidator_ExplicitEnforce()
    {
        Console.WriteLine("ERROR: No ValidatesBusinessRule for DATA_ENCRYPTED exists");
    }
}

// ❌ BR002 ERROR - No validator exists for "INVENTORY_AVAILABLE"
[RequiresBusinessRule("INVENTORY_AVAILABLE")]
public class Br002ErrorClassNoValidator
{
    public void SomeMethod()
    {
        Console.WriteLine("ERROR: No ValidatesBusinessRule for INVENTORY_AVAILABLE exists");
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
    [RequiresBusinessRule("EMAIL_VERIFIED", enforceValidation: false)]
    public void Warning_NoValidator_EnforceFalse()
    {
        Console.WriteLine("WARNING: No ValidatesBusinessRule for EMAIL_VERIFIED exists");
    }

    // ⚠️ BR003 WARNING - No validator exists for "TERMS_ACCEPTED"
    [RequiresBusinessRule("TERMS_ACCEPTED", enforceValidation: false)]
    public void AnotherWarning()
    {
        Console.WriteLine("WARNING: No ValidatesBusinessRule for TERMS_ACCEPTED exists");
    }
}

// ⚠️ BR003 WARNING - No validator exists for "SESSION_ACTIVE"
[RequiresBusinessRule("SESSION_ACTIVE", enforceValidation: false)]
public class WarningClassNoValidator
{
    public void SomeMethod()
    {
        Console.WriteLine("WARNING: No ValidatesBusinessRule for SESSION_ACTIVE exists");
    }
}
