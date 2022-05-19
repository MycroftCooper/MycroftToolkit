using System;
using System.Collections.Generic;

public static class EnumExtension {

    public static T ToEnum<T>(this int value)
        => (T)Enum.ToObject(typeof(T), value);
    public static List<Enum> GetFlags(this Enum enumValue) {
        List<Enum> output = new List<Enum>();
        foreach (Enum t in Enum.GetValues(enumValue.GetType())) {
            if (enumValue.HasFlag(t)) output.Add(t);
        }
        return output;
    }
}
