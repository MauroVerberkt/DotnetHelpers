using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace BusinessRules.Wcf;

[DataContract]
public record BusinessRuleFault(
    [property: DataMember]
    [property: Required]
    BusinessRuleBase BusinessRule);
