using BusinessRules.Utilities;

namespace BusinessRules.Attributes;

/// <summary>
/// Marks a method or class as implementing validation for a specific business rule.
/// Satisfies the <see cref="BusinessRuleAttribute"/> enforcement check (BR002/BR003).
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class ImplementsBusinessRuleAttribute(string ruleKey) : Attribute
{
    /// <summary>
    /// The unique key identifying the business rule being implemented.
    /// </summary>
    public string RuleKey { get; } = ruleKey;

    /// <summary>
    /// The resolved business rule instance, looked up by <see cref="RuleKey"/> at runtime.
    /// </summary>
    public BusinessRuleBase Requirement { get; } = 
        BusinessRuleResolver.FindBusinessRuleByKey(ruleKey)
        ?? throw new ArgumentException($"BusinessRule with key '{ruleKey}' not found in any [BusinessRuleGroup] class.");
}
