using System.Reflection;

namespace BusinessRules.Utilities;

internal static class BusinessRuleResolver
{
    internal static BusinessRuleBase? FindBusinessRuleByKey(string key)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        return (assemblies
            .SelectMany(assembly => assembly.GetTypes().Where(t => t.IsClass),
                (assembly, type) => new { assembly, type })
            .SelectMany(t1 => t1.type.GetFields(BindingFlags.Public | BindingFlags.Static),
                (t1, field) => new { t1, field })
            .Where(t1 => t1.field.FieldType == typeof(BusinessRuleBase))
            .Select(t1 => (BusinessRuleBase)t1.field.GetValue(null)!)).FirstOrDefault(br => br.InternalKey == key);
    }
}
