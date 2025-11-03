using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace BusinessRules;

[DataContract]
public record BusinessRuleFault(
    [property: DataMember]
    [property: Required]
    BusinessRule BusinessRule);
