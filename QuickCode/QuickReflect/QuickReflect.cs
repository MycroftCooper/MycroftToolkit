using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace MycroftToolkit.QuickCode {
    public static class QuickReflect { 
         public const BindingFlags FullBinding = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic; 
         public const BindingFlags InstanceBinding = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public; 
         public const BindingFlags StaticBinding = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;

         public static T CreateInstance<T>(string fullName,params object[] parameters) {
             if (String.IsNullOrEmpty(fullName)) {
                 Debug.LogError($"QuickReflect>Error>类名称非法!");
                 return default;
             }
             
             Type type = Type.GetType(fullName);
             if (type == null) {
                 Debug.LogError($"QuickReflect>Error>当前程序集下未找到:{fullName}");
                 return default;
             }

             try {
                 object obj = parameters == null ? 
                     Activator.CreateInstance(type) : Activator.CreateInstance(type, parameters);
                 return (T)obj;
             }
             catch (Exception ex){
                 Debug.LogError($"QuickReflect>Error>构造函数参数异常:{ex.Message}:{fullName}");
                 return default;
             }
         }
         
         public static T CreateInstance<T>(string fullName,AssemblyName assemblyName, params object[] parameters) {
             if (String.IsNullOrEmpty(fullName) || assemblyName == null) {
                 Debug.LogError($"QuickReflect>Error>类名称或程序集名称非法!");
                 return default;
             }
             Assembly targetAssembly = Assembly.Load(assemblyName);
             Type objectType =  targetAssembly.GetType(fullName);
             object obj = parameters == null ? Activator.CreateInstance(objectType) : Activator.CreateInstance(objectType, parameters);
             return (T)obj;
         }
         

        #region Fields
        public static bool HasField(this Type type, string fieldName) {
            return type.FindField(fieldName) != null;
        }

        public static bool HasField<T>(this T obj, string fieldName) {
            return obj.GetType().HasField(fieldName);
        }
        
        public static FieldInfo FindField(this Type type, string fieldName, BindingFlags flags = FullBinding) 
            => type.GetField(fieldName, flags);
        
        public static T GetField<T>(this Type type, string fieldName) {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            return (T)type.FindField(fieldName).GetValue(null);
        }
        public static T GetStaticField<T>(this Type type, string fieldName) {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            return (T)type.FindField(fieldName, StaticBinding).GetValue(null);
        }

        public static T GetInstanceField<T>(this object obj, string fieldName) {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            return (T)obj.GetType().FindField(fieldName, InstanceBinding).GetValue(obj);
        }

        public static void SetField<TValue>(this Type type, string fieldName, TValue value) {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            type.FindField(fieldName).SetValue(null, value);
        }
        
        public static void SetStaticField<TValue>(this Type type, string fieldName, TValue value) {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            type.FindField(fieldName, StaticBinding).SetValue(null, value);
        }

        public static void SetInstanceField<TObj, TValue>(this TObj obj, string fieldName, TValue value) {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            obj.GetType().FindField(fieldName, InstanceBinding).SetValue(obj, value);
        }
        #endregion
        

        #region Props
        public static bool HasProperty(this Type type, string propertyName) {
            return type.FindProperty(propertyName) != null;
        }

        public static bool HasProperty<T>(this T obj, string propertyName) {
            return obj.GetType().HasProperty(propertyName);
        }
        
        public static PropertyInfo FindProperty(this Type type, string propertyName, BindingFlags flags = FullBinding) 
            => type.GetProperty(propertyName, flags);
        
        public static T GetProperty<T>(this Type type, string propertyName) {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            return (T)type.FindProperty(propertyName).GetValue(null, null);
        }
        
        public static T GetStaticProperty<T>(this Type type, string propertyName) {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            return (T)type.FindProperty(propertyName, StaticBinding).GetValue(null, null);
        }

        public static T GetInstanceProperty<T>(this object obj, string propertyName) {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            return (T)obj.GetType().FindProperty(propertyName, InstanceBinding).GetValue(obj, null);
        }
        
        public static void SetProperty<TValue>(this Type type, string propertyName, TValue value) {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            type.FindProperty(propertyName).SetValue(null, value, null);
        }


        public static void SetStaticProperty<TValue>(this Type type, string propertyName, TValue value) {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            type.FindProperty(propertyName, StaticBinding).SetValue(null, value, null);
        }

        public static void SetInstanceProperty<TObj, TValue>(this TObj obj, string propertyName, TValue value) {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            obj.GetType().FindProperty(propertyName, InstanceBinding).SetValue(obj, value, null);
        }
        #endregion

        #region FieldsAndProps
        /// <summary>
        /// 获取指定名字的属性或字段
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemberInfo GetDataMember(Type objType, string name, bool isStatic) {
            if (objType == null || name == null)
                return null;

            var flags = BindingFlags.Public | BindingFlags.NonPublic |
                        (isStatic ? BindingFlags.Static : BindingFlags.Instance);

            var prop = objType.GetProperty(name, flags);
            if (prop != null)
                return prop;

            var field = objType.GetField(name, flags);
            return field != null ? field : null;
        }

        /// <summary>
        /// 通过反射为数据变量赋值
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValue(Type objType, string name, object value, object instanceObj) {
            if (objType == null || name == null)
                return;

            bool isStatic = (instanceObj == null);

            var member = GetDataMember(objType, name, isStatic);
            if (member == null) return;
            switch (member.MemberType) {
                case MemberTypes.Property:
                    var prop = (PropertyInfo)member;
                    prop.SetValue(instanceObj, ChangeType(value, prop.PropertyType), null);
                    break;
                case MemberTypes.Field:
                    var field = (FieldInfo)member;
                    field.SetValue(instanceObj, ChangeType(value, field.FieldType));
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static object ChangeType(object value, Type conversionType)
        {
            if (value != null && value.GetType() == conversionType)
                return value;

            return Convert.ChangeType(value, conversionType, CultureInfo.InvariantCulture);
        }
        #endregion

        #region Has Method
        public static MethodInfo FindMethod(this Type type, string methodName, Type[] argsTypes = null, BindingFlags flags = FullBinding) =>
            argsTypes == null ?
                type.GetMethod(methodName, flags) :
                type.GetMethod(methodName, flags, null, argsTypes, null);
        
        public static bool HasMethod(this object obj, string methodName) {
            var argsTypes = new Type[] { };
            return obj.GetType().FindMethod(methodName, argsTypes) != null;
        }

        public static bool HasMethod<TArg1>(this object obj, string methodName) {
            var argsTypes = new [] { typeof(TArg1) };
            return obj.GetType().FindMethod(methodName, argsTypes) != null;
        }

        public static bool HasMethod<TArg1, TArg2>(this object obj, string methodName) {
            var argsTypes = new [] { typeof(TArg1), typeof(TArg2) };
            return obj.GetType().FindMethod(methodName, argsTypes) != null;
        }

        public static bool HasMethod<TArg1, TArg2, TArg3>(this object obj, string methodName) {
            var argsTypes = new [] { typeof(TArg1), typeof(TArg2), typeof(TArg3) };
            return obj.GetType().FindMethod(methodName, argsTypes) != null;
        }

        public static bool HasMethod<TArg1, TArg2, TArg3, TArg4>(this object obj, string methodName) {
            var argsTypes = new [] { typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4) };
            return obj.GetType().FindMethod(methodName, argsTypes) != null;
        }

        public static bool HasMethod(this Type type, string methodName) {
            var argsTypes = new Type[] { };
            return type.FindMethod(methodName, argsTypes) != null;
        }

        public static bool HasMethod<TArg1>(this Type type, string methodName) {
            var argsTypes = new [] { typeof(TArg1) };
            return type.FindMethod(methodName, argsTypes) != null;
        }

        public static bool HasMethod<TArg1, TArg2>(this Type type, string methodName) {
            var argsTypes = new [] { typeof(TArg1), typeof(TArg2) };
            return type.FindMethod(methodName, argsTypes) != null;
        }

        public static bool HasMethod<TArg1, TArg2, TArg3>(this Type type, string methodName) {
            var argsTypes = new [] { typeof(TArg1), typeof(TArg2), typeof(TArg3) };
            return type.FindMethod(methodName, argsTypes) != null;
        }

        public static bool HasMethod<TArg1, TArg2, TArg3, TArg4>(this Type type, string methodName) {
            var argsTypes = new [] { typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4) };
            return type.FindMethod(methodName, argsTypes) != null;
        }
        #endregion
        
        public static object RawCall(Type type, object obj, string methodName, object[] args, bool isStatic) {

            if (obj == null && !isStatic)
                throw new ArgumentNullException(nameof(obj), "obj cannot be null for instance methods");
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            Type[] argsTypes = new Type[args.Length];
            for (var i = 0; i < argsTypes.Length; i++)
                if (argsTypes[i] == typeof(object))
                    argsTypes[i] = args[i].GetType();

            var method = type.FindMethod(methodName, argsTypes, isStatic ? StaticBinding : InstanceBinding);

            if (method == null)
                throw new MissingMethodException(type.FullName, methodName);

            return method.Invoke(obj, args);
        }
        
        public static object RawCall(Type type, object obj, string methodName, object[] args, Type[] argsTypes, bool isStatic) {
            if (obj == null && !isStatic)
                throw new ArgumentNullException(nameof(obj), "obj cannot be null for instance methods");
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            for (var i = 0; i < argsTypes.Length; i++)
                if (argsTypes[i] == typeof(object))
                    argsTypes[i] = args[i].GetType();

            var method = type.FindMethod(methodName, argsTypes, isStatic ? StaticBinding : InstanceBinding);

            if (method == null)
                throw new MissingMethodException(type.FullName, methodName);

            return method.Invoke(obj, args);
        }

        #region Invoke Instance
        public static void InvokeMethod(this object obj, string methodName) {
            var args = new object[] { };
            var argsTypes = new Type[] { };
            RawCall(obj.GetType(), obj, methodName, args, argsTypes, false);
        }

        public static void InvokeMethod<TArg1>(this object obj, string methodName, TArg1 arg1) {
            var args = new object[] { arg1 };
            var argsTypes = new [] { typeof(TArg1) };
            RawCall(obj.GetType(), obj, methodName, args, argsTypes, false);
        }

        public static void InvokeMethod<TArg1, TArg2>(this object obj, string methodName, TArg1 arg1, TArg2 arg2) {
            var args = new object[] { arg1, arg2 };
            var argsTypes = new [] { typeof(TArg1), typeof(TArg2) };
            RawCall(obj.GetType(), obj, methodName, args, argsTypes, false);
        }

        public static void InvokeMethod<TArg1, TArg2, TArg3>(this object obj, string methodName, TArg1 arg1, TArg2 arg2, TArg3 arg3) {
            var args = new object[] { arg1, arg2, arg3 };
            var argsTypes = new [] { typeof(TArg1), typeof(TArg2), typeof(TArg3) };
            RawCall(obj.GetType(), obj, methodName, args, argsTypes, false);
        }

        public static void InvokeMethod<TArg1, TArg2, TArg3, TArg4>(this object obj, string methodName, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4) {
            var args = new object[] { arg1, arg2, arg3, arg4 };
            var argsTypes = new [] { typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4) };
            RawCall(obj.GetType(), obj, methodName, args, argsTypes, false);
        }

        public static TResult InvokeMethod<TResult>(this object obj, string methodName) {
            var args = new object[] { };
            var argsTypes = new Type[] { };
            return (TResult)RawCall(obj.GetType(), obj, methodName, args, argsTypes, false);
        }

        public static TResult InvokeMethod<TResult, TArg1>(this object obj, string methodName, TArg1 arg1) {
            var args = new object[] { arg1 };
            var argsTypes = new [] { typeof(TArg1) };
            return (TResult)RawCall(obj.GetType(), obj, methodName, args, argsTypes, false);
        }

        public static TResult InvokeMethod<TResult, TArg1, TArg2>(this object obj, string methodName, TArg1 arg1, TArg2 arg2) {
            var args = new object[] { arg1, arg2 };
            var argsTypes = new [] { typeof(TArg1), typeof(TArg2) };
            return (TResult)RawCall(obj.GetType(), obj, methodName, args, argsTypes, false);
        }

        public static TResult InvokeMethod<TResult, TArg1, TArg2, TArg3>(this object obj, string methodName, TArg1 arg1, TArg2 arg2, TArg3 arg3) {
            var args = new object[] { arg1, arg2, arg3 };
            var argsTypes = new [] { typeof(TArg1), typeof(TArg2), typeof(TArg3) };
            return (TResult)RawCall(obj.GetType(), obj, methodName, args, argsTypes, false);
        }

        public static TResult InvokeMethod<TResult, TArg1, TArg2, TArg3, TArg4>(this object obj, string methodName, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4) {
            var args = new object[] { arg1, arg2, arg3, arg4 };
            var argsTypes = new [] { typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4) };
            return (TResult)RawCall(obj.GetType(), obj, methodName, args, argsTypes, false);
        }
        #endregion
        

        #region Invoke Static
        public static void InvokeStaticMethod(this Type type, string methodName) {
            var args = new object[] { };
            var argsTypes = new Type[] { };
            RawCall(type, null, methodName, args, argsTypes, true);
        }

        public static void InvokeStaticMethod<TArg1>(this Type type, string methodName, TArg1 arg1) {
            var args = new object[] { arg1 };
            var argsTypes = new [] { typeof(TArg1) };
            RawCall(type, null, methodName, args, argsTypes, true);
        }

        public static void InvokeStaticMethod<TArg1, TArg2>(this Type type, string methodName, TArg1 arg1, TArg2 arg2) {
            var args = new object[] { arg1, arg2 };
            var argsTypes = new [] { typeof(TArg1), typeof(TArg2) };
            RawCall(type, null, methodName, args, argsTypes, true);
        }

        public static void InvokeStaticMethod<TArg1, TArg2, TArg3>(this Type type, string methodName, TArg1 arg1, TArg2 arg2, TArg3 arg3) {
            var args = new object[] { arg1, arg2, arg3 };
            var argsTypes = new [] { typeof(TArg1), typeof(TArg2), typeof(TArg3) };
            RawCall(type, null, methodName, args, argsTypes, true);
        }

        public static void InvokeStaticMethod<TArg1, TArg2, TArg3, TArg4>(this Type type, string methodName, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4) {
            var args = new object[] { arg1, arg2, arg3, arg4 };
            var argsTypes = new [] { typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4) };
            RawCall(type, null, methodName, args, argsTypes, true);
        }

        public static TResult InvokeStaticMethod<TResult>(this Type type, string methodName) {
            var args = new object[] { };
            var argsTypes = new Type[] { };
            return (TResult)RawCall(type, null, methodName, args, argsTypes, true);
        }

        public static TResult InvokeStaticMethod<TResult, TArg1>(this Type type, string methodName, TArg1 arg1) {
            var args = new object[] { arg1 };
            var argsTypes = new [] { typeof(TArg1) };
            return (TResult)RawCall(type, null, methodName, args, argsTypes, true);
        }

        public static TResult InvokeStaticMethod<TResult, TArg1, TArg2>(this Type type, string methodName, TArg1 arg1, TArg2 arg2) {
            var args = new object[] { arg1, arg2 };
            var argsTypes = new [] { typeof(TArg1), typeof(TArg2) };
            return (TResult)RawCall(type, null, methodName, args, argsTypes, true);
        }

        public static TResult InvokeStaticMethod<TResult, TArg1, TArg2, TArg3>(this Type type, string methodName, TArg1 arg1, TArg2 arg2, TArg3 arg3) {
            var args = new object[] { arg1, arg2, arg3 };
            var argsTypes = new [] { typeof(TArg1), typeof(TArg2), typeof(TArg3) };
            return (TResult)RawCall(type, null, methodName, args, argsTypes, true);
        }

        public static TResult InvokeStaticMethod<TResult, TArg1, TArg2, TArg3, TArg4>(this Type type, string methodName, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4) {
            var args = new object[] { arg1, arg2, arg3, arg4 };
            var argsTypes = new [] { typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4) };
            return (TResult)RawCall(type, null, methodName, args, argsTypes, true);
        }
        #endregion
        
        public static T DeepCopy<T>(this T obj) {
            if (obj is string || obj.GetType().IsValueType){ //如果是字符串或值类型则直接返回
                return obj;
            }

            object output = Activator.CreateInstance(obj.GetType());
            FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            foreach (FieldInfo field in fields) {
                field.SetValue(output, DeepCopy(field.GetValue(obj)));
            }
            return (T)output;
        }

        #region Assembly
        public static readonly string[] SystemAssemblyPrefixList = new string[] {
            "mscorlib","netstandard","System.",	"Mono.","Microsoft.", "Unity.","UnityEngine.","UnityEditor.",
        };

        /// <summary>
        /// 检查是否用户程序集
        /// (简单过滤是否在SystemAssemblyPrefixList内)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsUserAssembly(Assembly assembly) {
            var name = assembly.FullName;
            foreach (var prefix in SystemAssemblyPrefixList) {
                if (name.StartsWith(prefix))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 获取可加载类型
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly) {
            try {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e) {
                // Gets the array of classes that were defined in the module and loaded.
                return e.Types.Where(t => t != null);
            }
        }

        /// <summary>
        /// 获取所有类型定义，允许指定只收集用户程序级
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<Type> GetAllTypes(bool userAssemblyOnly = true) {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(asm => !userAssemblyOnly || IsUserAssembly(asm))
                .SelectMany(asm => asm.GetLoadableTypes());
        }
        #endregion
    }
}
