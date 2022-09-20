using UnityEngine;

public class QuickLogCat : MonoBehaviour {
    private bool _showLog;
    private string _logStr = "";

    private Rect _scrollViewRect;
    private GUIStyle _lblStyle;
    private Vector2 _scrollViewPos;
    
    public static void Init() {
        GameObject go = new GameObject("LogCat");
        go.AddComponent<QuickLogCat>();
    }

    void Start() {
        Application.logMessageReceivedThreaded += LogCallBack;

        _scrollViewRect = new Rect(0, 0, Screen.width, Screen.height * 0.9f);
        _lblStyle = new GUIStyle();
        _lblStyle.normal.textColor = Color.white;
        _lblStyle.wordWrap = true;
        _lblStyle.fontSize = 25;
    }


    void Update() {
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetKeyDown(KeyCode.F4))
#else
        if (Input.GetMouseButtonDown(0) && Input.touchCount == 4)
#endif
        {
            _showLog = !_showLog;
        }
    }

    public void OnGUI() {
        if (!_showLog) return;
        GUILayout.BeginArea(_scrollViewRect);
        {
            GUILayout.Box("", GUILayout.Width(_scrollViewRect.width), GUILayout.Height(_scrollViewRect.height));
            GUILayout.EndArea();
        }

        GUILayout.BeginArea(_scrollViewRect);
        {
            _scrollViewPos = GUILayout.BeginScrollView(_scrollViewPos);
            {

                GUILayout.Label(_logStr, _lblStyle);
                GUILayout.EndScrollView();
            }
            if (GUILayout.Button("clear", GUILayout.Height(80))) {
                _logStr = "";
            }

            GUILayout.EndArea();
        }
    }

    private void LogCallBack(string condition, string stackTrace, LogType type) {
        //if(type ==LogType.Error || type == LogType.Exception || type == LogType.Assert)
        {
            _logStr += condition + "\n";
        }
    }
}
