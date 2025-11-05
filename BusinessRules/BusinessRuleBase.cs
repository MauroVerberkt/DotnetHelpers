using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace BusinessRules;

[DataContract]
public abstract class BusinessRuleBase
{
    [DataMember, Required]
    internal abstract string InternalKey { get; }

    [DataMember, Required]
    internal abstract string InternalRule { get; }

    [DataMember]
    internal abstract string InternalDescription { get; }

    [DataMember]
    internal abstract string InternalCategory { get; }
}