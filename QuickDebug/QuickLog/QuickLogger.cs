using System;
using UnityEngine;
using System.Text;
using System.IO;
using UnityEditor;
using Object = UnityEngine.Object;

public static class QuickLogger {
    // 普通调试日志开关
    public static bool DebugLogEnable = true;

    // 警告日志开关
    public static bool WarningLogEnable = true;

    // 错误日志开关
    public static bool ErrorLogEnable = true;

    // 使用StringBuilder来优化字符串的重复构造
    private static StringBuilder _logStr = new StringBuilder();

    // 日志文件存储位置
    private static string _logFileSavePath;

    /// <summary>
    /// 初始化，在游戏启动的入口脚本的Awake函数中调用GameLogger.Init
    /// </summary>
    public static void Init() {
        // 日期
        var t = DateTime.Now.ToString("yyyyMMddhhmmss");

#if UNITY_STANDALONE || UNITY_EDITOR
        var logDir = $"{Application.dataPath}/../gameLog/";
#else
        var logDir = string.Format("{0}/gamelog/", Application.persistentDataPath);
#endif
        if (!Directory.Exists(logDir))
            Directory.CreateDirectory(logDir);
        _logFileSavePath = $"{logDir}/output_{t}.txt";
        Application.logMessageReceived += OnLogCallBack;
    }
    
    public static void PrintSystemInfo() {
        string systemInfo =
            $"OS:{SystemInfo.operatingSystem}-{SystemInfo.processorType}-{SystemInfo.processorCount}\n" +
            $"MemorySize:{SystemInfo.systemMemorySize}\n" +
            $"Graphics:{SystemInfo.graphicsDeviceName} " +
            $"-vendor:{SystemInfo.graphicsDeviceVendor} " +
            $"-memorySize:{SystemInfo.graphicsMemorySize} -deviceVersion:{SystemInfo.graphicsDeviceVersion}";
        Log(systemInfo);
    }

    /// <summary>
    /// 打印日志回调
    /// </summary>
    /// <param name="condition">日志文本</param>
    /// <param name="stackTrace">调用堆栈</param>
    /// <param name="type">日志类型</param>
    private static void OnLogCallBack(string condition, string stackTrace, LogType type) {
        _logStr.Append(condition);
        _logStr.Append("\n");
        if (type == LogType.Error || type == LogType.Exception) {
            _logStr.Append(stackTrace);
            _logStr.Append("\n");
        }

        if (_logStr.Length <= 0) return;
        if (!File.Exists(_logFileSavePath)) {
            var fs = File.Create(_logFileSavePath);
            fs.Close();
        }

        using (var sw = File.AppendText(_logFileSavePath)) {
            sw.WriteLine(_logStr.ToString());
        }

        _logStr.Remove(0, _logStr.Length);
    }

    /// <summary>
    /// 普通调试日志
    /// </summary>
    public static void Log(object message, Object context = null) {
        if (!DebugLogEnable) return;
        Debug.Log(message, context);
    }

    /// <summary>
    /// 格式化打印日志
    /// </summary>
    /// <param name="format">例："a is {0}, b is {1}"</param>
    /// <param name="args">可变参数，根据format的格式传入匹配的参数，例：a, b</param>
    public static void LogFormat(string format, params object[] args) {
        if (!DebugLogEnable) return;
        Debug.LogFormat(format, args);
    }

    /// <summary>
    /// 带颜色的日志
    /// </summary>
    /// <param name="message"></param>
    /// <param name="color">颜色值，例：green, yellow，#ff0000</param>
    /// <param name="context">上下文对象</param>
    public static void LogWithColor(object message, string color, Object context = null) {
        if (!DebugLogEnable) return;
        Debug.Log(FmtColor(color, message), context);
    }

    /// <summary>
    /// 红色日志
    /// </summary>
    public static void LogRed(object message, Object context = null) {
        if (!DebugLogEnable) return;
        Debug.Log(FmtColor("red", message), context);
    }

    /// <summary>
    /// 绿色日志
    /// </summary>
    public static void LogGreen(object message, Object context = null) {
        if (!DebugLogEnable) return;
        Debug.Log(FmtColor("#00ff00", message), context);
    }

    /// <summary>
    /// 黄色日志
    /// </summary>
    public static void LogYellow(object message, Object context = null) {
        if (!DebugLogEnable) return;
        Debug.Log(FmtColor("yellow", message), context);
    }

    /// <summary>
    /// 青蓝色日志
    /// </summary>
    public static void LogCyan(object message, Object context = null) {
        if (!DebugLogEnable) return;
        Debug.Log(FmtColor("#00ffff", message), context);
    }

    /// <summary>
    /// 带颜色的格式化日志打印
    /// </summary>
    public static void LogFormatWithColor(string format, string color, params object[] args) {
        if (!DebugLogEnable) return;
        Debug.LogFormat((string)FmtColor(color, format), args);
    }

    /// <summary>
    /// 警告日志
    /// </summary>
    public static void LogWarning(object message, Object context = null) {
        if (!WarningLogEnable) return;
        Debug.LogWarning(message, context);
    }

    /// <summary>
    /// 错误日志
    /// </summary>
    public static void LogError(object message, Object context = null) {
        if (!ErrorLogEnable) return;
        Debug.LogError(message, context);
    }

    /// <summary>
    /// 格式化颜色日志
    /// </summary>
    private static object FmtColor(string color, object obj) {
        if (obj is string s) {
#if !UNITY_EDITOR
            return obj;
#else
            return FmtColor(color, s);
#endif
        }
        else {
#if !UNITY_EDITOR
            return obj;
#else
            return $"<color={color}>{obj}</color>";
#endif
        }
    }

    /// <summary>
    /// 格式化颜色日志
    /// </summary>
    private static object FmtColor(string color, string msg) {
#if !UNITY_EDITOR
        return msg;
#else
        int p = msg.IndexOf('\n');
        if (p >= 0) p = msg.IndexOf('\n', p + 1); // 可以同时显示两行
        if (p < 0 || p >= msg.Length - 1) return $"<color={color}>{msg}</color>";
        if (p > 2 && msg[p - 1] == '\r') p--;
        return $"<color={color}>{msg.Substring(0, p)}</color>{msg.Substring(p)}";
#endif
    }

    public static bool Assert(bool condition, string errorMsg) {
        if (!condition)
            LogError(errorMsg);
        return condition;
    }

    #region 解决日志双击溯源问题

#if UNITY_EDITOR
    [UnityEditor.Callbacks.OnOpenAssetAttribute(0)]
    static bool OnOpenAsset(int instanceID, int line) {
        string stackTrace = GetStackTrace();
        if (string.IsNullOrEmpty(stackTrace) || !stackTrace.Contains("GameLogger:Log")) return false;
        // 使用正则表达式匹配at的哪个脚本的哪一行
        var matches = System.Text.RegularExpressions.Regex.Match(stackTrace, @"\(at (.+)\)",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        while (matches.Success) {
            var pathLine = matches.Groups[1].Value;

            if (!pathLine.Contains("GameLogger.cs")) {
                int splitIndex = pathLine.LastIndexOf(":", StringComparison.Ordinal);
                // 脚本路径
                string path = pathLine.Substring(0, splitIndex);
                // 行号
                line = Convert.ToInt32(pathLine.Substring(splitIndex + 1));
                string fullPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("Assets", StringComparison.Ordinal));
                fullPath = fullPath + path;
                // 跳转到目标代码的特定行
                UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(fullPath.Replace('/', '\\'), line);
                break;
            }

            matches = matches.NextMatch();
        }

        return true;

    }

    /// <summary>
    /// 获取当前日志窗口选中的日志的堆栈信息
    /// </summary>
    /// <returns></returns>
    static string GetStackTrace() {
        // 通过反射获取ConsoleWindow类
        var consoleWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.ConsoleWindow");
        // 获取窗口实例
        var fieldInfo = consoleWindowType.GetField("ms_ConsoleWindow",
            System.Reflection.BindingFlags.Static |
            System.Reflection.BindingFlags.NonPublic);
        var consoleInstance = fieldInfo.GetValue(null);
        if (consoleInstance == null) return null;
        if (EditorWindow.focusedWindow != (EditorWindow)consoleInstance) return null;
        // 获取m_ActiveText成员
        fieldInfo = consoleWindowType.GetField("m_ActiveText",
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic);
        // 获取m_ActiveText的值
        string activeText = fieldInfo.GetValue(consoleInstance).ToString();
        return activeText;

    }
#endif

    #endregion 解决日志双击溯源问题
}
