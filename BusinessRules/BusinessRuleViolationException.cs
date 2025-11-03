using System;

namespace BusinessRules;

public class BusinessRuleViolationException : Exception
{
    public BusinessRule BusinessRule { get; }

    public BusinessRuleViolationException(BusinessRule businessRule)
        : base(businessRule.InternalRule)
    {
        BusinessRule = businessRule;
    }

    public BusinessRuleViolationException(BusinessRule businessRule, string message)
        : base(message)
    {
        BusinessRule = businessRule;
    }

    public BusinessRuleViolationException(BusinessRule businessRule, string message, Exception innerException)
        : base(message, innerException)
    {
        BusinessRule = businessRule;
    }

    public string Key => BusinessRule.InternalKey;
    public string Rule => BusinessRule.InternalRule;
    public string Description => BusinessRule.InternalDescription;
    public string Category => BusinessRule.InternalCategory;
}
