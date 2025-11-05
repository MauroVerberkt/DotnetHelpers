using System;
using BusinessRules.Utilities;

namespace BusinessRules.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class BusinessRuleAttribute(string ruleKey, bool enforceValidation = true) : Attribute
{
    public string RuleKey { get; } = ruleKey;
    public BusinessRuleBase Rule { get; } = 
        BusinessRuleResolver.FindBusinessRuleByKey(ruleKey) 
        ?? throw new ArgumentException($"BusinessRule with key '{ruleKey}' not found in any [BusinessRuleGroup] class.");

    public bool EnforceValidation { get; } = enforceValidation;
}
