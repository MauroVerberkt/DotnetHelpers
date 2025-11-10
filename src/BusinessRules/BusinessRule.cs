using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace BusinessRules;

[DataContract]
public abstract class BusinessRule<T>(string key, string rule, string description = "", string category = "") : BusinessRuleBase
    where T : BusinessRule<T>, new()
{
    [DataMember, Required]
    internal override string InternalKey { get; } = key;

    [DataMember, Required]
    internal override string InternalRule { get; } = rule;

    [DataMember]
    internal override string InternalDescription { get; } = description;

    [DataMember]
    internal override string InternalCategory { get; } = category;

    public static BusinessRuleViolationException ToException()
    {
        return new BusinessRuleViolationException(new T());
    }

    public static BusinessRuleViolationException ToException(string message)
    {
        return new BusinessRuleViolationException(new T(), message);
    }
}