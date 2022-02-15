using System;
namespace MycroftToolkit.QuickCode {
    public static class QuickReflect {
        public static T Reflect<T>(string namespacePath, object[] parameters = null) {
            Type type = Type.GetType(namespacePath);
            object obj;
            if (parameters == null) obj = Activator.CreateInstance(type);
            else obj = Activator.CreateInstance(type, parameters);
            return (T)obj;
        }
    }
}