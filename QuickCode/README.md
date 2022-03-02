# 快速编码工具包

# 0. 工具包信息

创建时间：22-01-20																							
维护人：MycroftCooper
应用项目：


| 版本号 | 日期     | 更新内容                                       | 更新人        |
| ------ | -------- | ---------------------------------------------- | ------------- |
| v.0.1  | 22-01-20 | 创建工具包，实现四舍五入                       | MycroftCooper |
| v.0.2  | 22-02-8  | 加入对象池，单例，快速重启，快捷移除组件等功能 | MycroftCooper |
| v.0.3  | 22-02-15 | 加入计时器，快速反射等功能                     | MycroftCooper |
| v.0.4  | 22-02-28 | 加入通用字典                                   | MycroftCooper |

# 1. 工具包简介

该工具包可以让编码更加快捷迅速

**命名空间：**MycroftToolkit.QuickCode

**使用环境:**

- C# 

- Unity

**主要功能：**

- QuickRestart .cs
  项目快速重启
- MathTools.cs
  数学工具包
- ExtensionMethods .cs
  Unity扩展方法包
- ObjectPool .cs
  通用对象池(含游戏对象)
- Singleton.cs
  通用单例
- QuickReflect
  快速反射
- GeneralDictionary
  通用字典

# 2. 功能列表

## 2.1 QuickRestart 快速重启

放入Unity项目中，菜单栏 **Tools** 下就会出现快速重启按钮，点击后Unity项目将快速重启

## 2.2 MathTools 数学工具包

### 2.2.1 中国式四舍五入

https://www.cnblogs.com/WalkingSnail/p/8125780.html

C#与Unity中没有“四舍五入”，它采用的是“四舍六入五成双”。使用本工具包，可以快速实现中国式四舍五入。

| 函数名 | 参数列表              | 返回值 | 功能                        | 案例                   |
| ------ | --------------------- | ------ | --------------------------- | ---------------------- |
| Round  | (float input)         | int    | 中式四舍五入(取整)-负数支持 | 输入-1.2<br>输出-1     |
| Round  | (float input, int dp) | float  | 中式四舍五入(精确)-负数支持 | 输入2.45,1<br/>输出2.5 |

**未来拓展方向:**

- 添加更多便捷简化的常用数学工具

### 2.2.2 RandomEx 简单随机数工具包

Unity的随机数是根据系统时钟取的种子，特别短时间内取随机会导致随机结果相同，这个工具包可以解决这类问题。

| 函数名    | 参数列表                     | 返回值 | 功能                            | 案例                             |
| --------- | ---------------------------- | ------ | ------------------------------- | -------------------------------- |
| GetBool   | (float probability = 0.5f)   | bool   | 获取随机Bool值(默认为真概率0.5) | MathLF.RandomEx.GetBool()        |
| GetInt    | (int x, int y)               | int    | [x,y)的随机数                   | MathLF.RandomEx.GetInt(0,10)     |
| GetFloat  | (float x = 0, float y = 1)   | float  | [x,y)的随机数                   | MathLF.RandomEx.GetFloat(0f,1f)  |
| GetDouble | (double x = 0, double y = 1) | double | [x,y)的随机数                   | MathLF.RandomEx.GetDouble(0f,1f) |

**未来拓展方向:**

- 添加更多科学化的随机方式：https://www.jianshu.com/p/d683ee23362e

## 2.3 ExtensionMethods 扩展方法包

一些扩展Unity原本功能的代码

| 函数名          | 参数列表                                      | 返回值 | 功能                 | 案例                                                         |
| --------------- | --------------------------------------------- | ------ | -------------------- | ------------------------------------------------------------ |
| RemoveComponent | (this GameObject obj, bool immediate = false) | void   | 移除游戏实体上的组件 | gameobject.RemoveComponent\<SpriteRander>(true);<br/>立刻移除游戏实体上的精灵渲染器组件 |

**未来拓展方向:**

- 添加更多Unity原本功能的扩展代码

## 2.4 ObjectPool 通用对象池

有三种种对象池：

1. 针对普通对象的**ObjectPool\<T>**
2. 针对GameObject的**GameObjectPool** 游戏实体对象池
3. 针对Component的 **ComponentPool\<T>** 组件对象池

### ObjectPool\<T>

| 函数名    | 参数列表                 | 返回值 | 功能                          | 案例                                                         |
| --------- | ------------------------ | ------ | ----------------------------- | ------------------------------------------------------------ |
| InitPool  | (int size = 10)          | bool   | 初始化对象池<br/>成功返回True | ObjectPool OP = new ObjectPool\<DamageEffect>();<br/>OP.InitPool(20)<br/>创建大小为20，装载DamageEffect的对象池 |
| GetObject | (bool createIfPoolEmpty) | T      | 取对象                        | OP.GetObject(true);<br/>获取对象，若池子空了，创建新对象     |
| Recycle   | (T obj)                  | bool   | 回收对象<br/>成功返回True     | OP.Recycle(obj);                                             |
| CleanPool | ()                       | void   | 清空对象池                    | 略                                                           |

### GameObjectPool

| 函数名    | 参数列表                                                     | 返回值     | 功能                          | 案例                                                         |
| --------- | ------------------------------------------------------------ | ---------- | ----------------------------- | ------------------------------------------------------------ |
| InitPool  | (GameObject prefab, int size = 10, Transform parent = null, bool setActive = false) | bool       | 初始化对象池<br/>成功返回True | GameObjectPool OP = new GameObjectPool();<br/>OP.InitPool(Bat, 20)<br/>创建大小为20，装载Bat游戏对象的对象池 |
| GetObject | (bool createIfPoolEmpty, bool setActive = true)              | GameObject | 取对象                        | OP.GetObject(true);<br/>获取对象，若池子空了，创建新对象     |
| Recycle   | (GameObject obj, bool setActive = false)                     | bool       | 回收对象<br/>成功返回True     | OP.Recycle(obj);                                             |
| CleanPool | ()                                                           | void       | 清空对象池                    | OP.CleanPool()                                               |

### ComponentPool\<T>

| 函数名    | 参数列表                                                    | 返回值     | 功能                          | 案例                                                         |
| --------- | ----------------------------------------------------------- | ---------- | ----------------------------- | ------------------------------------------------------------ |
| InitPool  | (GameObject parent, int size = 10, bool setEnabled = false) | bool       | 初始化对象池<br/>成功返回True | ComponentPool\<AudioSource> asPool = new ComponentPool\<AudioSource>();<br/>asPool.InitPool(gameObject);<br/>创建大小为20，装载音源组件的对象池 |
| GetObject | (bool createIfPoolEmpty, bool setEnabled = true)            | GameObject | 取对象                        | OP.GetObject(true);<br/>获取对象，若池子空了，创建新对象     |
| Recycle   | (T obj, bool setEnabled = false)                            | bool       | 回收对象<br/>成功返回True     | OP.Recycle(obj);                                             |
| CleanPool | ()                                                          | void       | 清空对象池                    | OP.CleanPool()                                               |

## 2.5 Singleton 通用单例

有两种通用单例：

**普通代码单例用法：**

```c#
// 声明单例类
public class MyClass:Singleton<MyClass>{}

// 获取单例
MyClass mc = MyClass.Instance;
```

这样，MyClass就是一个单例了

**继承MonoBehaviour的单例用法：**

```c#
// 声明单例类
public class MyClass:MonoSingleton<MyClass>{}

// 获取单例
MyClass mc = MyClass.Instance;

// 设置加载时是否销毁
MyClass.DontDestry = true;	//不销毁
MyClass.DontDestry = false;	//销毁
```

这样，MyClass就是一个继承MonoBehaviour的单例了

## 2.6 QuickReflect 快速反射

### API表

| 函数名          | 参数列表                                                     | 返回值  | 功能                   |
| --------------- | ------------------------------------------------------------ | ------- | ---------------------- |
| Create\<T>      | (string 命名空间路径, object[] 反射目标类型构造函数参数数组 = null) | T       | 反射                   |
| ContainProperty | (this object 判断对象, string 属性名称)                      | bool    | 判断对象是否包含属性   |
| SetProperty     | (object 对象, string 属性名称, object 新值)                  | void    | 利用反射设置对象的属性 |
| GetProperty     | (object 对象, string 属性名称)                               | dynamic | 利用反射获取对象的属性 |

### 案例

**T Create\<T> (string namespacePath, object[] parameters = null)**

```csharp
string namespacePath = "AshCastle.BattleSystem.Logic." + dataRow.ClassName + "_Effect";
BEffect buffEffect = QuickReflect.Create<BEffect>(namespacePath);
```

> 反射相关教程：https://mycroftcooper.github.io/2021/10/28/C%E4%BA%95%E5%8F%8D%E5%B0%84/
>
> dynamic相关教程：https://www.cnblogs.com/yayazi/p/8998610.html

## 2.7 GeneralDictionary 通用字典

这是一个可以装不同数据类型值的字典

**属性：**

| 属性名                         | 作用                             |
| ------------------------------ | -------------------------------- |
| Dictionary<Tkey, object> _dict | 内部封装的字典，可以进行更多操作 |

**方法：**

| 函数名        | 参数列表                 | 返回值 | 功能                         | 案例 |
| ------------- | ------------------------ | ------ | ---------------------------- | ---- |
| Count         | ()                       | int    | 字典大小                     | 略   |
| Add           | (Tkey key, object value) | void   | 增加键值对                   | 略   |
| Get\<Tvalue>  | (Tkey key)               | Tvalue | 通过键获取值                 | 如下 |
| Set           | (Tkey key, object value) | bool   | 通过键修改值<br>成功返回True | 略   |
| ContainsKey   | (Tkey key)               | bool   | 是否存在键                   | 略   |
| ContainsValue | (object value)           | bool   | 是否存在值                   | 略   |
| Remove        | (Tkey key)               | bool   | 根据键移除键值对             | 略   |
| Clear         | ()                       | void   | 清空字典                     | 略   |

**案例：**

```c#
public class Test {
    public GeneralDictionary<int> myDict;
    public Test() {
        myDict = new GeneralDictionary<int>();
        myDict.Add(0, 1);
        myDict.Add(1, "2");
        myDict.Add(2, 3.1415);
        myDict.Add(3, DateTime.Now);
    }
    public void show() {
        foreach (var k in myDict._dict.Keys) {
            Console.WriteLine(k + ":" + myDict.Get<dynamic>(k).ToString());
        }
    }
    public static void Main(string[] args) {
        Test t = new Test();
        t.show();
    }
}
```

输出结果：

```
0:1
1:2
2:3.1415
3:2022/2/23 16:26:31
```

