using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

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
}