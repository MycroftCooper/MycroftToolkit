using UnityEngine;
using UnityEngine.SceneManagement;

namespace MycroftToolkit.QuickCode {
    public class Singleton<T> where T : Singleton<T>, new() {
        private static T _instance;
        private static readonly object Obj = new object();
        public static T Instance {
            get {
                lock (Obj)
                {
                    return _instance ??= new T();
                }
            }
        }
    }

    public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour {
        private static T _instance;
        private static readonly object _lock = new object();
        /// <summary>
        /// 程序是否正在退出
        /// </summary>
        protected static bool ApplicationIsQuitting { get; private set; }
        /// <summary>
        /// 是否为全局单例
        /// </summary>
        private static bool dontDestry = true;
        protected static bool IsGlobal {
            get => dontDestry;
            set {
                if (value == dontDestry || !Application.isPlaying) return;
                if (value)
                    DontDestroyOnLoad(_instance.gameObject);
                else
                    SceneManager.MoveGameObjectToScene(_instance.gameObject, SceneManager.GetActiveScene());
                dontDestry = value;
            }
        }

        static MonoSingleton() {
            ApplicationIsQuitting = false;
        }

        public static T Instance {
            get {
                if (ApplicationIsQuitting) {
                    if (Debug.isDebugBuild) {
                        Debug.LogWarning("[Singleton] " + typeof(T) +
                                                " already destroyed on application quit." +
                                                " Won't create again - returning null.");
                    }

                    return null;
                }

                lock (_lock) {
                    if (_instance != null) return _instance;
                    
                    // 先在场景中找寻
                    _instance = (T)FindObjectOfType(typeof(T));

                    if (FindObjectsOfType(typeof(T)).Length > 1) {
                        if (Debug.isDebugBuild) {
                            Debug.LogWarning("[Singleton] " + typeof(T).Name + " should never be more than 1 in scene!");
                        }

                        return _instance;
                    }

                    
                    // 场景中找不到就创建新物体挂载
                    if (_instance != null) return _instance;
                    
                    GameObject singletonObj = new GameObject();
                    _instance = singletonObj.AddComponent<T>();
                    singletonObj.name = "(singleton) " + typeof(T);

                    if (IsGlobal && Application.isPlaying) {
                        DontDestroyOnLoad(singletonObj);
                    }

                    return _instance;

                }
            }
        }

        /// <summary>
        /// 当工程运行结束，在退出时，不允许访问单例
        /// </summary>
        public void OnApplicationQuit() {
            ApplicationIsQuitting = true;
        }
    }
}
