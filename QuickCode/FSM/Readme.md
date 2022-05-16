# Unity的简单有限状态机（C#）

状态机是管理游戏状态的一种非常有效的方法，无论是在游戏的主要对象（游戏结束、重新启动、继续等）或UI（按钮结束、按钮按下等）上，还是在单个演员和NPC（AI行为、动画等）上。下面是一个简单的状态机，它应该在任何Unity上下文中都能很好地工作。

## 特点

教科书中的状态机实现，以及通过扩展其他C#状态机库，都倾向于复杂的配置或过多的样板文件。然而，状态机非常有用——管理开销永远不会阻止我们提高可读性、修复bug，或者编写好代码。

* 只需添加枚举字段
* 用Unity的方式做事可以避免意想不到的怪异和副作用
* 反射可以避免你写冗长的样板文件

* 广泛的单元测试覆盖范围
* 初始化后无垃圾分配
* 支持 iOS/Android/IL2CPP


## 用法

```c#
using MonsterLove.StateMachine; //1. Remember the using statement

public class MyGameplayScript : MonoBehaviour
{
    public enum States
    {
        Init, 
        Play, 
        Win, 
        Lose
    }
    
    StateMachine<States> fsm;
    
    void Awake(){
        fsm = new StateMachine<States>(this); //2. The main bit of "magic". 

        fsm.ChangeState(States.Init); //3. Easily trigger state transitions
    }

    void Init_Enter()
    {
        Debug.Log("Ready");
    }

    void Play_Enter()
    {      
        Debug.Log("Spawning Player");    
    }

    void Play_FixedUpdate()
    {
        Debug.Log("Doing Physics stuff");
    }

    void Play_Update()
    {
        if(player.health <= 0)
        {
            fsm.ChangeState(States.Lose); //3. Easily trigger state transitions
        }
    }

    void Play_Exit()
    {
        Debug.Log("Despawning Player");    
    }

    void Win_Enter()
    {
        Debug.Log("Game Over - you won!");
    }

    void Lose_Enter()
    {
        Debug.Log("Game Over - you lost!");
    }

}
```



\### 状态方法由下划线约定（“StateName_Method”）定义



与单行为方法（“Awake”、“Updates”等）一样，状态方法由约定定义。以“StateName_method”格式声明一个方法，该方法将与提供的枚举中的任何匹配名称相关联。

Like MonoBehavior methods (`Awake`, `Updates`, etc), state methods are defined by convention. Declare a method in the format `StateName_Method`, and this will be associated with any matching names in the provided enum.

```C#
void enum States
{
    Play, 
}


//Coroutines are supported, simply return IEnumerator
IEnumerator Play_Enter()
{
    yield return new WaitForSeconds(1);
    
    Debug.Log("Start");    
}


IEnumerator Play_Exit()
{
    yield return new WaitForSeconds(1);
}

void Play_Finally()
{
    Debug.Log("GameOver");
}
```

These built-in methods are always available, triggered automatically by `ChangeState(States newState)` calls: 

- `Enter`
- `Exit`
- `Finally`

Both `Enter` and `Exit` support co-routines, simply return `IEnumerator`. However, return `void`, and they will be called immediately with no overhead. `Finally` is always called after `Exit` and provides an opportunity to perform clean-up and hygiene in special cases where the `Exit` routine might be interrupted before completing (see the Transitions heading).

## Data-Driven State Events

To define additional events, we need to specify a `Driver`.

```C#
public class Driver
{
    StateEvent Update;
    StateEvent<Collision> OnCollisionEnter; 
    StateEvent<int> OnHealthPickup;
}
```

This is a very simple class. It doesn't have to be called `Driver`; the only constraint is that it must contain `StateEvent` fields. When we pass this to our state machine definition, it will take care of everything needed to set up new State event hooks.

```C#
StateMachine<States, Driver> fsm;
    
void Awake(){
    fsm = new StateMachine<States, Driver>(this); 
}

void Play_Enter()
{
    Debug.Log("Started");
}

void Play_Update()
{
    Debug.Log("Ticked");
}

void Play_OnHealthPickup(int health)
{
    //Add to player health
}

```

As these are custom events, the final step is to tell the state machine when these should be fired.

```C#
void Update()
{
    fsm.Driver.Update.Invoke();
}

void OnCollisionEnter(Collision collision)
{
    fsm.Driver.OnCollisionEnter.Invoke(collision);
}

void OnHealthPickup(int health)
{
    fsm.Driver.OnHealthPickup.Invoke();
}
```

##### Driver Deep-Dive

Compared to the rest of the StateMachine, the `Driver` might elicit a reaction of: *"Hey! You said there wasn't going to be any funny business here!"*

Indeed, there aren't many analogues in either C# or Unity. Before `v4.0`, the state machine would dynamically assign a `StateMachineRunner` component that would call `FixedUpdate`,`Update` & `LateUpate` hooks. (For backwards compatibility this is still the default behaviour when omitting a `Driver`). This worked, but additional hooks meant forking the `StateMachineRunner` class. Also, as a separate MonoBehaviour, it has it's own script execution order which could sometimes lead to oddities.

But with the user responsible for invoking events - eg `fsm.Drive.Update.Invoke()`, it becomes much easier to reason about the lifecycle of the fsm. No more having to guess whether the StateMachine will update before or after the rest of the class, because the trigger is right there. It can be moved to right spot in the main `Update()` call. 

```C#
void Update()
{
    //Do Stuff

    fsm.Driver.Update.Invoke();

    //Do Other Stuff
}

void Play_Update()
{
    //No guessing when this happens
}
```

The real power shines when we consider another anti-pattern. Calling a state change from outside the state machine can lead to unintended side-effects. Imagine the following scenario where a global call causes a state transition. However without 

```C#
public void EndGame()
{
    fsm.ChangeState(States.GameOver);
}

void Idle_Update()
{
    //Changing to GameOver would cause unintended things to happen
}

void Play_Update()
{
    //GameOver is legal
}
```

Some libraries deal with this by defining transitons tables. However, it's possible to achieve a similar outcome using state events:  

```C#
public class Driver()
{
    public StateEvent OnEndGame;
}

public void EndGame()
{
    fsm.Driver.OnEndGame.Invoke();
}

void Idle_Update()
{
    //Changing to GameOver would cause unintended things to happen
}

void Play_Update()
{
    //GameOver is legal
}

void Play_OnEndGame()
{
    fsm.ChangeState(State.GameOver);
}
```

Now the `Play` state is only state that can respond to EndGame calls. This creates an implicit transition table as sort of "free" side-effect.

\##异步转换

通过长时间的进入或退出协程，可以简单地管理异步状态更改。

```C#
fsm.ChangeState(States.MyNextState, StateTransition.Safe);
```

默认值为`StateTransition.Safe`. 这将始终允许当前状态在转换到任何新状态之前完成其进入和退出功能。

```C#
fsm.ChangeState(States.MyNextState, StateTransition.Overwrite);
```

`StateMahcine.Overwrite` 将取消所有当前转换，并立即调用下一个状态。这意味着任何尚未在进入和退出例程中运行的代码都将被跳过。如果需要确保以特定配置结束，则始终会调用finally函数：

```C#
void MyCurrentState_Finally()
{
    //Reset object to desired configuration
}
```

\#####设计理念

状态机旨在最大限度地简化最终用户的操作。为了实现这一点，引擎盖下隐藏着一些复杂的反射“魔法”。反射是一个有争议的选择，因为它很慢——在这里也不例外。然而，我们试图通过在初始化状态机时将所有反射限制为单个调用来平衡这种权衡。这确实会降低实例化性能，但是，实例化已经很慢了。预计诸如对象池（回收在启动时而不是运行时生成的对象）之类的策略已经生效，这会将此成本转移到用户不太可能注意到它的时候。

\##### 内存分配空闲？

这是针对移动设备设计的，因此应该是无内存分配的。然而，在使用“IEnumerator”和协同程序时，同样的规则也适用于Unity的其他部分。