using System;
using System.Reflection;

namespace MycroftToolkit.QuickCode {
 public class ReflectObject {
        private readonly object _obj;
        public Type ObjectType { get; }

        public ReflectObject(string fullName, params object[] parameters) {
            _obj = QuickReflect.CreateInstance<object>(fullName, parameters);
            ObjectType = _obj.GetType();
        }
        
        public ReflectObject(string fullName,AssemblyName assemblyName, params object[] parameters) {
            _obj = QuickReflect.CreateInstance<object>(fullName,assemblyName, parameters);
            ObjectType = _obj.GetType();
        }
        
        public T GetObject<T>() => (T) _obj;

        #region 字段
        public bool HasField(string fieldName) {
            if (_obj == null || string.IsNullOrEmpty(fieldName)) return false;
            return ObjectType.HasField(fieldName);
        }
        public dynamic GetField(string fieldName) {
            if (_obj == null || string.IsNullOrEmpty(fieldName)) return null;
            return ObjectType.GetField(fieldName)?.GetValue(_obj);
        }

        public T GetField<T>(string fieldName) {
            if (_obj == null || string.IsNullOrEmpty(fieldName)) return default;
            return (T)ObjectType.GetField(fieldName)?.GetValue(_obj);
        }
        
        public void SetField<T>(string fieldName, T newVal) {
            ObjectType.SetField(fieldName, newVal);
        }
        #endregion

        
        #region 属性
        public bool HasProperty(string propertyName) {
            if (_obj == null || string.IsNullOrEmpty(propertyName)) return false;
            return ObjectType.HasProperty(propertyName);
        }
        
        public dynamic GetProperty(string propertyName) {
            if (_obj == null || string.IsNullOrEmpty(propertyName)) return null;
            return ObjectType.GetProperty(propertyName)?.GetValue(_obj);
        }

        public T GetProperty<T>(string propertyName) {
            if (_obj == null || string.IsNullOrEmpty(propertyName)) return default;
            return (T)ObjectType.GetProperty(propertyName)?.GetValue(_obj);
        }
        
        public void SetProperty<T>(string propertyName, T newVal) {
            ObjectType.SetProperty(propertyName, newVal);
        }
        #endregion
        
        
        #region 方法
        public bool HasMethod(string methodName) {
            if (_obj == null || string.IsNullOrEmpty(methodName)) return false;
            return ObjectType.HasMethod(methodName);
        }
        
        public bool HasMethod(string methodName,Type[] argsTypes) {
            if (_obj == null || string.IsNullOrEmpty(methodName)) return false;
            return ObjectType.FindMethod(methodName, argsTypes) != null;
        }
        
        public T InvokeMethod<T>(string methodName, object[] parameters = null) {
            if (_obj == null || string.IsNullOrEmpty(methodName)) return default;
            return (T)QuickReflect.RawCall(ObjectType, _obj, methodName, parameters, false);
        }
        
        #endregion

    }
}