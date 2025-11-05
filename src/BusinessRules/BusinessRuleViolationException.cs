using System;

namespace BusinessRules;

public class BusinessRuleViolationException : Exception
{
    public BusinessRuleBase BusinessRule { get; }

    public BusinessRuleViolationException(BusinessRuleBase businessRule)
        : base(businessRule.InternalRule)
    {
        BusinessRule = businessRule;
    }

    public BusinessRuleViolationException(BusinessRuleBase businessRule, string message)
        : base(message)
    {
        BusinessRule = businessRule;
    }

    public BusinessRuleViolationException(BusinessRuleBase businessRule, string message, Exception innerException)
        : base(message, innerException)
    {
        BusinessRule = businessRule;
    }

    public string Key => BusinessRule.InternalKey;
    public string Rule => BusinessRule.InternalRule;
    public string Description => BusinessRule.InternalDescription;
    public string Category => BusinessRule.InternalCategory;
}
