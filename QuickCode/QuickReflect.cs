using System;
using System.Reflection;

namespace MycroftToolkit.QuickCode {
    public static class QuickReflect {
        /// <summary>
        /// 利用反射创建新对象
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="namespacePath">反射类命名空间路径</param>
        /// <param name="parameters">构造函数参数</param>
        /// <returns>新对象</returns>
        public static T Create<T>(string namespacePath, object[] parameters = null) {
            Type type = Type.GetType(namespacePath);
            object obj;
            if (parameters == null) obj = Activator.CreateInstance(type);
            else obj = Activator.CreateInstance(type, parameters);
            return (T)obj;
        }
        /// <summary>
        /// 利用反射来判断对象是否包含某个属性
        /// </summary>
        /// <param name="instance">判断对象</param>
        /// <param name="propertyName">属性名称</param>
        /// <returns>是否包含</returns>
        public static bool ContainProperty(this object instance, string propertyName) {
            if (instance != null && !string.IsNullOrEmpty(propertyName)) {
                PropertyInfo _findedPropertyInfo = instance.GetType().GetProperty(propertyName);
                return (_findedPropertyInfo != null);
            }
            return false;
        }
        /// <summary>
        /// 利用反射设置对象的属性
        /// </summary>
        /// <param name="instance">对象</param>
        /// <param name="propertyName">属性名称</param>
        /// <param name="newVal">新值</param>
        public static void SetProperty(object instance, string propertyName, object newVal) {
            instance.GetType().GetProperty(propertyName).SetValue(instance, newVal);
        }
        /// <summary>
        /// 利用反射获取对象的属性
        /// </summary>
        /// <param name="instance">对象</param>
        /// <param name="propertyName">属性名称</param>
        /// <returns>属性</returns>
        public static dynamic GetProperty(object instance, string propertyName) {
            return instance.GetType().GetProperty(propertyName).GetValue(instance);
        }
    }
}
