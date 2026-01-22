using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace cwlogs.util;

public static class ReflectionUtils
{
    public static List<string> GetCommandOptions(Type commandType)
    {
        var optionList = new List<string>();
        var settingsType = GetSettingsType(commandType);

        if (settingsType != null)
        {
            var currentSettingsType = settingsType;
            while (currentSettingsType != null && currentSettingsType != typeof(object))
            {
                foreach (var prop in currentSettingsType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
                {
                    foreach (var attr in prop.GetCustomAttributes(true))
                    {
                        if (attr.GetType().Name == "CommandOptionAttribute")
                        {
                            var shorts = attr.GetType().GetProperty("ShortNames")?.GetValue(attr) as System.Collections.IEnumerable;
                            var longs = attr.GetType().GetProperty("LongNames")?.GetValue(attr) as System.Collections.IEnumerable;
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

    private static Type? GetSettingsType(Type commandType)
    {
        var currentType = commandType;
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
