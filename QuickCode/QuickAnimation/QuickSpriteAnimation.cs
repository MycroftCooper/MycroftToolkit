using System;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Sirenix.OdinInspector;
using Random = System.Random;

namespace MycroftToolkit.QuickCode.QuickAnimation {
    public partial class QuickSpriteAnimation : MonoBehaviour {
        public SpriteRenderer sr;
        public GameObject targetGameObject;
        
        private string DebugHead => $"{gameObject.name}.QuickSpriteAnimation>";

        #region 资源相关
        [BoxGroup("Resources")]
        public bool usePathLoadSprites;
        [BoxGroup("Resources")]
        public Sprite[] sprites;
        [BoxGroup("Resources"), ShowIf(nameof(usePathLoadSprites))]
        public string[] spritesPaths;
        public int SpritesNum => sprites.Length;
        
        private bool InitRes() {
            if (usePathLoadSprites) {
                if (spritesPaths == null || spritesPaths.Any(string.IsNullOrEmpty)) {
                    Debug.LogError($"{DebugHead}spritesPaths数组中存在空值!初始化失败!");
                    return false;
                }

                sprites = new Sprite[spritesPaths.Length];
                // todo: AB加载目标路径资源至sprites数组中
            }

            if (sprites != null && sprites.All(_ => _ != null)) return true;
            Debug.LogError($"{DebugHead}sprites数组中存在空值!初始化失败!");
            return false;
        }
        #endregion

        #region 参数相关
        [BoxGroup("StartSetting")]
        public float delayStartTime;
        [BoxGroup("StartSetting")]
        public bool isStartManual;
        [BoxGroup("StartSetting")]
        public bool isRandomStartIndex;
        [BoxGroup("StartSetting"),HideIf(nameof(isRandomStartIndex))]
        public int startFrameIndex;
        
        public enum EmPlayMode { Loop, Once, Yoyo, Random }
        [BoxGroup("PlaySetting")]
        public EmPlayMode playMode = EmPlayMode.Loop;
        [BoxGroup("PlaySetting")]
        public float frameRate = 8;
        [BoxGroup("PlaySetting")]
        public bool isReverse;
        [BoxGroup("PlaySetting")]
        public bool isPlaying;
        [BoxGroup("PlaySetting")]
        public bool isAlwaysUpdate;
        
        [BoxGroup("EndSetting")]
        public bool isAutoEndByTimer;
        [BoxGroup("EndSetting")]
        public float targetDuration;
        [BoxGroup("EndSetting")]
        public bool isNeedFadOutAtEnd;
        [BoxGroup("EndSetting")]
        public float fadOutAtEndDuration;
        [BoxGroup("EndSetting")]
        public bool isNeedDisableAtEnd;
        [BoxGroup("EndSetting")]
        public bool isNeedDestroyAtEnd;
        
        private bool _isStart;
        private float _currentFrameDuration;
        private int _currentIndex;
        private Random _random;
        private Tween _fadOutTween;
        [Button]
        public void SetCurrentSprite(int index) {
            if (index == _currentIndex) {
                return;
            }
            if (index < 0 || index >= SpritesNum) {
                Debug.LogError($"{DebugHead}目标下标{index}越界!，sprite数量为{sprites.Length}");
                return;
            }
            if (sr == null) {
                Debug.LogError($"{DebugHead}没有设置SpriteRenderer");
                return;
            }
            
            sr.sprite = sprites[index];
            _currentIndex = index;
        }
        #endregion

        #region 生命周期相关
        private void Awake() {
            if (frameRate <= 0) {
                Debug.LogError($"{DebugHead}配置错误!frameRate必须为正整数!");
                return;
            }
            if (sr == null) {
                sr = GetComponent<SpriteRenderer>();
                if (sr == null) {
                    Debug.LogError($"{DebugHead}未设置SpriteRenderer!");
                    return;
                }
            }
            sr.enabled = true;
            if (targetGameObject == null) {
                targetGameObject = sr.gameObject;
            }
            if (!InitRes()) return;
            _random = new Random((int)DateTime.Now.Ticks);
            
            int targetIndex = isRandomStartIndex ? _random.Next(SpritesNum) : startFrameIndex;
            targetIndex = Mathf.Clamp(targetIndex, 0, SpritesNum - 1);
            SetCurrentSprite(targetIndex);
            _isStart = false;
        }

        private void OnEnable() {
            if (isStartManual) {
                return;
            }

            if (delayStartTime == 0) {
                StartPlay();
            }else {
                Invoke(nameof(StartPlay), delayStartTime);
            }
        }
        
        public void StartPlay() {
            if (_isStart) {
                return;
            }
            _isStart = true;
            isPlaying = true;
            _currentFrameDuration = 0;
            
            if (!isAutoEndByTimer) return;
            if (targetDuration <= 0) {
                Debug.LogError($"{DebugHead}目标持续时间必须是正数!");
                return;
            }
            Invoke(nameof(End), targetDuration);
        }

        private void Update() {
            if (!_isStart || !isPlaying) {
                return;
            }
            
            float deltaTime = isAlwaysUpdate ? Time.unscaledDeltaTime : Time.deltaTime;
            _currentFrameDuration += deltaTime;
            if (_currentFrameDuration < 1 / frameRate) {
                return;
            }

            NextFrame();
        }
        
        public void Stop() {
            if (!_isStart) {
                return;
            }
            CancelInvoke();
            if (_fadOutTween != null) {
                _fadOutTween.Kill();
                _fadOutTween = null;
            }
            End();
        }
        
        private void End() {
            if (isNeedFadOutAtEnd) {
                if (fadOutAtEndDuration > 0) {
                    if (_fadOutTween != null) {
                        _fadOutTween.Kill();
                        _fadOutTween = null;
                    }
                    _fadOutTween = sr.DOColor(Color.clear, fadOutAtEndDuration).OnComplete(OnAnimationEnd);
                    return;
                }
                sr.color = Color.clear;
            }
            OnAnimationEnd();
        }

        [Button]
        public void Reset() {
            int targetIndex = isRandomStartIndex ? _random.Next(SpritesNum) : startFrameIndex;
            SetCurrentSprite(targetIndex);
            _isStart = false;
            isPlaying = false;
            _currentFrameDuration = 0;
            isReverse = false;
            
            CancelInvoke();
            if (_fadOutTween == null) return;
            _fadOutTween.Kill();
            _fadOutTween = null;
        }
        
        private void OnAnimationEnd() {
            Reset();
            if (isNeedDisableAtEnd) {
                targetGameObject.SetActive(false);
            }
            if (isNeedDestroyAtEnd) {
                Destroy(targetGameObject);
            }
        }

        private void NextFrame() {
            _currentFrameDuration = 0;
            int targetIndex = _currentIndex;
            targetIndex += isReverse ? -1 : 1;

            switch (playMode) {
                case EmPlayMode.Loop:
                    if (targetIndex >= SpritesNum) {
                        targetIndex = 0;
                    }else if (targetIndex < 0) {
                        targetIndex = SpritesNum - 1;
                    }
                    break;
                case EmPlayMode.Once:
                    if (targetIndex >= SpritesNum) {
                        targetIndex = SpritesNum - 1;
                        End();
                    }
                    break;
                case EmPlayMode.Yoyo:
                    if (targetIndex >= SpritesNum) {
                        targetIndex = SpritesNum - 2;
                        isReverse = true;
                    }else if (targetIndex < 0) {
                        targetIndex = 1;
                        isReverse = false;
                    }
                    break;
                case EmPlayMode.Random:
                    _random ??= new Random((int)DateTime.Now.Ticks);
                    targetIndex = _random.Next(SpritesNum);
                    if (targetIndex == _currentIndex) { // 防止随机得到的帧和上一帧相同
                        targetIndex = (targetIndex + _random.Next(SpritesNum)) % SpritesNum;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            SetCurrentSprite(targetIndex);
        }
        #endregion
    }
}