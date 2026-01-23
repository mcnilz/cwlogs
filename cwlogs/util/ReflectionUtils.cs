using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace cwlogs.util;

public static class ReflectionUtils
{
    public static List<string> GetCommandOptions([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type commandType)
    {
        var optionList = new List<string>();
        var settingsType = GetSettingsType(commandType);

        if (settingsType != null)
        {
            var currentSettingsType = (Type?)settingsType;
            while (currentSettingsType != null && currentSettingsType != typeof(object))
            {
                foreach (var prop in currentSettingsType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
                {
                    foreach (var attr in prop.GetCustomAttributes(true))
                    {
                        var attrType = attr.GetType();
                        if (attrType.Name == "CommandOptionAttribute")
                        {
                            var shorts = GetPropertyValue(attr, "ShortNames") as System.Collections.IEnumerable;
                            var longs = GetPropertyValue(attr, "LongNames") as System.Collections.IEnumerable;
                            if (shorts != null)
                            {
                                foreach (var s in shorts) if (s != null) optionList.Add("-" + s);
                            }
                            if (longs != null)
                            {
                                foreach (var l in longs) if (l != null) optionList.Add("--" + l);
                            }
                        }
                    }
                }
                currentSettingsType = currentSettingsType.BaseType;
            }
        }

        return optionList.Distinct().ToList();
    }

    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2075:UnsatisfiedGetPropertyClassifier",
        Justification = "The attributes used for command options are known to have these properties.")]
    private static object? GetPropertyValue(object obj, string propertyName)
    {
        return obj.GetType().GetProperty(propertyName)?.GetValue(obj);
    }

    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2073:UnsatisfiedReturnClassifier",
        Justification = "We know that command settings types have public properties for options.")]
    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
    private static Type? GetSettingsType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type commandType)
    {
        var currentType = (Type?)commandType;
        while (currentType != null && currentType != typeof(object))
        {
            if (currentType.IsGenericType && (currentType.GetGenericTypeDefinition().Name.Contains("AsyncCommand") || currentType.GetGenericTypeDefinition().Name.Contains("Command")))
            {
                return currentType.GetGenericArguments().FirstOrDefault();
            }
            currentType = currentType.BaseType;
        }
        return null;
    }
}
