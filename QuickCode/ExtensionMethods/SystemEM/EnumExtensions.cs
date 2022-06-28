using System;
using System.Collections.Generic;
using System.Linq;

public static class EnumExtension {

    public static T ToEnum<T>(this int value)
        => (T)Enum.ToObject(typeof(T), value);
    public static List<Enum> GetFlags(this Enum enumValue) {
        if (enumValue == null) return null;
        List<Enum> output = new List<Enum>();
        foreach (Enum t in Enum.GetValues(enumValue.GetType())) {
            if (enumValue.HasFlag(t)) output.Add(t);
        }
        return output;
    }

    public static List<T> GetEnums<T>(Type enumType) where T : Enum {
        if (enumType == null) return null;
        var enumArray = Enum.GetValues(enumType);
        List<T> output = enumArray.OfType<T>().ToList();
        return output;
    }
}
