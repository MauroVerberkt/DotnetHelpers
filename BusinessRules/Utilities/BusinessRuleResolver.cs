using System;
using System.Linq;
using System.Reflection;

namespace BusinessRules.Utilities;

public static class BusinessRuleResolver
{
    public static BusinessRule? FindBusinessRuleByKey(string key)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes().Where(t => t.IsClass);

            foreach (var type in types)
            {
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);

                foreach (var field in fields)
                {
                    if (field.FieldType != typeof(BusinessRule))
                        continue;

                    var br = (BusinessRule)field.GetValue(null)!;
                    if (br.Key == key)
                        return br;
                }
            }
        }

        return null;
    }
}
