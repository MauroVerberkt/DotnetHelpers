using BusinessRules;

namespace TestBusinessRules;

public class MyBusinessRules
{
    // Rules with validators
    public static readonly BusinessRule UserMustBeAuthenticated = new(
        Key: "USER_AUTH",
        Rule: "User must be authenticated"
    );

    public static readonly BusinessRule UserMustBeAdmin = new(
        Key: "USER_ADMIN",
        Rule: "User must have admin privileges"
    );

    public static readonly BusinessRule AgeMinimum = new(
        Key: "AGE_MIN",
        Rule: "User must be at least 18 years old"
    );
    
    // Rules without validators
    public static readonly BusinessRule EmailVerified = new(
        Key: "EMAIL_VERIFIED",
        Rule: "Email must be verified"
    );

    public static readonly BusinessRule TermsAccepted = new(
        Key: "TERMS_ACCEPTED",
        Rule: "Terms must de accepted"
    );

    public static readonly BusinessRule SessionActive = new(
        Key: "SESSION_ACTIVE",
        Rule: "Session must be active"
    );
    
    public static readonly BusinessRule PaymentVerified = new(
        Key: "PAYMENT_VERIFIED",
        Rule: "Payment must be verified"
    );
    
    public static readonly BusinessRule DataEncrypted = new(
        Key: "DATA_ENCRYPTED",
        Rule: "Data must be encrypted"
    );
    
    public static readonly BusinessRule InventoryAvailable = new(
        Key: "INVENTORY_AVAILABLE",
        Rule: "Inventory must be available"
    );
}
