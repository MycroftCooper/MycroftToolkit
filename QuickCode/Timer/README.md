# Unity Timer

原版[发布页](https://github.com/akbiggs/UnityTimer/releases)

`https://github.com/akbiggs/UnityTimer.git`

# 基础使用

## 创建定时器

Unity Timer包提供了以下方法来创建定时器。

**使用委托(Action):**
`public static Timer Register(float duration, Action onComplete);`

- **duration:** 计时器持续时间
- **onComplete：**计时器完成时要触发的事件
- **Timer：** 返回该计时器

**使用Lamda表达式：**
`Timer.Register(5f, () => Debug.Log("Hello World"));`

**创建与Mono同步销毁的计时器：**
使用`this.AttachTimer()`创建定时器，这样当MonoBehaviour被销毁时，定时器就会被销毁

```c#
public class CoolMonoBehaviour : MonoBehaviour {

   void Start() {
      // 使用AttachTimer扩展方法来创建一个定时器，当MonoBehaviour被检测到为null时，它将自动取消该定时器。
      this.AttachTimer(5f, () => {
         // 如果这段代码在对象被销毁后运行，将抛出一个空引用。这可能会破坏游戏状态。
         this.gameObject.transform.position = Vector3.zero;
      });
   }
   
   void Update() {
      if (Input.GetKeyDown(KeyCode.X)) {
         GameObject.Destroy(this.gameObject);
      }
   }
}
```

## 定时器控制

**设置定时器循环：**
设置`isLooped` 为True，则计时器可循环
`Timer.Register(2f, player.Jump, isLooped: true);`

**暂停定时器：**`timer.Pause()`

**恢复定时器：**`timer.Resume()`

**获取剩余时间：**
按时间`timer.GetTimeRemaining()`
按比例`timer.GetRatioComplete()`
是否完成`timer.isDone`

**设置定时器取消：**
使用`Timer.Cancel(timer);`来取消已经启用的计时器

```c#
Timer timer;

void Start() {
   timer = Timer.Register(2f, () => Debug.Log("You won't see this text if you press X."));
}

void Update() {
   if (Input.GetKeyDown(KeyCode.X)) {
      Timer.Cancel(timer);
   }
}
```

**设置定时器使用真实时间：**
案例如下：

```c#
// 假设你通过设置时间刻度为0来暂停你的游戏。
Time.timeScale = 0f;

// 然后设置useRealTime，这样即使游戏时间没有进展，这个定时器仍然会启动。
Timer.Register(1f, this.HandlePausedGameState, useRealTime: true);
```

**利用定时器进行插值：**
使用`onUpdate`回调，随着时间的推移逐渐更新一个值。

```c#
// 在五秒钟内将一种颜色从白色变为红色。
Color color = Color.white;
float transitionDuration = 5f;

Timer.Register(transitionDuration,
   onUpdate: secondsElapsed => color.r = 255 * (secondsElapsed / transitionDuration),
   onComplete: () => Debug.Log("Color is now red"));
```

## 注意事项

当改变场景时，所有的定时器都被销毁。
这种行为通常是人们所希望的，它的发生是因为计时器是由TimerController更新的，而TimerController在场景改变时也会被销毁。

请注意，由于这个原因，在场景被关闭时创建一个定时器，例如在一个对象的OnDestroy方法中，将[导致TimerController被生成时出现Unity错误](http://i.imgur.com/ESFmFDO.png)。
