using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace BusinessRules;

[DataContract]
public record BusinessRule(
    [property: DataMember, Required] string Key,
    [property: DataMember, Required] string Rule,
    [property: DataMember] string? Description = null,
    [property: DataMember] string? Category = null);
