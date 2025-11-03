using System;
using System.Linq;
using System.Reflection;

namespace BusinessRules.Utilities;

public static class BusinessRuleResolver
{
    public static BusinessRule? FindBusinessRuleByKey(string key)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        return (assemblies
            .SelectMany(assembly => assembly.GetTypes().Where(t => t.IsClass),
                (assembly, type) => new { assembly, type })
            .SelectMany(t1 => t1.type.GetFields(BindingFlags.Public | BindingFlags.Static),
                (t1, field) => new { t1, field })
            .Where(t1 => t1.field.FieldType == typeof(BusinessRule))
            .Select(t1 => (BusinessRule)t1.field.GetValue(null)!)).FirstOrDefault(br => br.InternalKey == key);
    }
}
