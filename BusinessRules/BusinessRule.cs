using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace BusinessRules;

[DataContract]
public class BusinessRule(string key, string rule, string description = "", string category = "")
{
    [DataMember] 
    [Required] 
    internal string InternalKey { get; } = key;

    [DataMember] 
    [Required] 
    internal string InternalRule { get; } = rule;
    
    [DataMember] 
    internal string InternalDescription { get; } = description;
    
    [DataMember]  
    internal string InternalCategory { get; } = category;

    public static FaultException<BusinessRuleFault> ToFaultException(BusinessRule businessRule)
    {
        return new FaultException<BusinessRuleFault>(
            new BusinessRuleFault(businessRule),
            new FaultReason(businessRule.InternalRule),
            new FaultCode(businessRule.InternalKey));
    }

    public static BusinessRuleViolationException ToException(BusinessRule businessRule)
    {
        return new BusinessRuleViolationException(businessRule);
    }

    public static BusinessRuleViolationException ToException(BusinessRule businessRule, string message)
    {
        return new BusinessRuleViolationException(businessRule, message);
    }
}
