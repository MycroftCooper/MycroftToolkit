using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 简单随机数工具包
/// @FAM 22.02.08
/// </summary>
public class QuickRandom {
    private int seed;
    /// <summary>
    /// 随机种子
    /// </summary>
    public int Seed {
        get => seed;
        set {
            seed = value;
            random = new System.Random(seed);
        }
    }

    private System.Random random;

    public QuickRandom(int seed) => Seed = seed;
    public QuickRandom() => Seed = (int)System.DateTime.Now.Ticks + new System.Random().Next(int.MinValue, int.MaxValue);

    public static QuickRandom SimpleRandom = new QuickRandom();

    #region 基础数据类型随机
    /// <summary>
    /// 获取随机Bool
    /// </summary>
    /// <param name="probability">为真的概率(默认0.5)</param>
    /// <returns>随机Bool</returns>
    public bool GetBool(float probability = 0.5f) => random.NextDouble() < probability;
    /// <summary>
    /// 获取随机Int
    /// </summary>
    /// <returns>[0,x)</returns>
    public int GetInt(int x) => random.Next(x);
    /// <summary>
    /// 获取随机Int
    /// </summary>
    /// <returns>[x,y)</returns>
    public int GetInt(int x, int y) => random.Next(x, y);
    /// <summary>
    /// 获取随机Float
    /// </summary>
    /// <returns>[x,y)</returns>
    public float GetFloat(float x = 0, float y = 1) => (float)random.NextDouble() * (y - x) + x;
    /// <summary>
    /// 获取随机Double
    /// </summary>
    /// <returns>[x,y)</returns>
    public double GetDouble(double x = 0, double y = 1) => random.NextDouble() * (y - x) + x;
    #endregion

    #region 字符串类型随机
    public string GetString_Encoding(System.Text.Encoding encoding, int length) {
        byte[] bytes = new byte[length];
        random.NextBytes(bytes);
        return encoding.GetString(bytes);
    }
    public string GetString_Num(int length) {
        const string nums = "0123456789";
        return nums.GetRandomSubString(length);
    }
    public string GetString_English(int length) {
        const string str = "ABCDEFGHIGKLMNOPQRSTUVWXYZabcdefghigklmnopqrstuvwxyz";
        return str.GetRandomSubString(length);
    }
    public string GetString_English_Upper(int length) {
        const string str = "ABCDEFGHIGKLMNOPQRSTUVWXYZ";
        return str.GetRandomSubString(length);
    }
    public string GetString_English_Lower(int length) {
        const string str = "abcdefghigklmnopqrstuvwxyz";
        return str.GetRandomSubString(length);
    }
    public string GetString_Chinese(int length) {
        byte[] bytes = new byte[length];
        for (int i = 0; i < length; i++) {
            bytes[i] = (byte)GetInt(0x4E00, 0x9FFF);
        }
        return System.Text.Encoding.Unicode.GetString(bytes);
    }
    #endregion
}
public static class RandomExtend {
    #region 非加权随机
    /// <summary>
    /// 获取随机对象
    /// </summary>
    /// <param name="qr">随机器</param>
    public static T GetRandomObject<T>(this IEnumerable<T> source, QuickRandom qr = null) {
        if (qr == null) qr = QuickRandom.SimpleRandom;
        T[] array = source.ToArray();
        return array[qr.GetInt(array.Length)];
    }

    /// <summary>
    /// 获取多个随机对象(可能重复)
    /// </summary>
    /// <param name="num">对象个数</param>
    /// <param name="qr">随机器</param>
    public static T[] GetRandomObject_Repeatable<T>(this IEnumerable<T> source, int num, QuickRandom qr = null) {
        if (qr == null) qr = QuickRandom.SimpleRandom;
        T[] array = source.ToArray();
        T[] output = Enumerable.Repeat(array, num).
                Select(s => s[qr.GetInt(s.Length)]).ToArray();
        return output;
    }

    /// <summary>
    /// 获取多个随机对象(不重复)
    /// </summary>
    /// <param name="num">对象个数</param>
    /// <param name="qr">随机器</param>
    public static T[] GetRandomObject_UnRepeatable<T>(this IEnumerable<T> source, int num, QuickRandom qr = null) {
        if (qr == null) qr = QuickRandom.SimpleRandom;
        List<T> list = source.ToList();
        list.Reorder(qr);
        T[] output = list.GetRange(0, num).ToArray();
        return output;
    }
    #endregion

    #region 加权随机
    /// <summary>
    /// 获取随机对象(加权)
    /// </summary>
    /// <param name="qr">随机器</param>
    public static T GetRandomWeightObject<T>(this IEnumerable<T> source, QuickRandom qr = null) where T : IWeightObject {
        if (qr == null) qr = QuickRandom.SimpleRandom;
        var totalWeight = source.Aggregate(0f, (total, current) => total + current.Weight);
        var targetWeight = qr.GetFloat(totalWeight);
        foreach (var item in source) {
            targetWeight -= item.Weight;
            if (targetWeight < 0) {
                return item;
            }
        }
        return source.FirstOrDefault();
    }

    /// <summary>
    /// 获取多个随机对象(加权)(可能重复)
    /// </summary>
    /// <param name="num">对象个数</param>
    /// <param name="qr">随机器</param>
    public static T[] GetRandomWeightObject_Repeatable<T>(this IEnumerable<T> source, int num, QuickRandom qr = null) where T : IWeightObject {
        if (qr == null) qr = QuickRandom.SimpleRandom;
        T[] output = new T[num];
        var totalWeight = source.Aggregate(0f, (total, current) => total + current.Weight);
        for (int i = 0; i < num; i++) {
            var targetWeight = qr.GetFloat(totalWeight);
            foreach (var item in source) {
                targetWeight -= item.Weight;
                if (targetWeight < 0) {
                    output[i] = item;
                    break;
                }
            }
        }
        return output;
    }

    /// <summary>
    /// 获取多个随机对象(加权)(不重复)
    /// </summary>
    /// <param name="num">对象个数</param>
    /// <param name="qr">随机器</param>
    public static T[] GetRandomWeightObject_UnRepeatable<T>(this IEnumerable<T> source, int num, QuickRandom qr = null) where T : IWeightObject {
        if (qr == null) qr = QuickRandom.SimpleRandom;
        List<T> list = source.ToList();
        T[] output = new T[num];
        var totalWeight = source.Aggregate(0f, (total, current) => total + current.Weight);
        for (int i = 0; i < num; i++) {
            var targetWeight = qr.GetFloat(totalWeight);
            for (int j = 0; j < list.Count; j++) {
                targetWeight -= list[j].Weight;
                if (targetWeight < 0) {
                    output[i] = list[j];
                    totalWeight -= list[j].Weight;
                    list.RemoveAt(j);
                    break;
                }
            }
        }
        return output;
    }
    #endregion

    /// <summary>
    /// 重排列
    /// </summary>
    /// <param name="qr">随机器</param>
    public static void Reorder<T>(this List<T> source, QuickRandom qr = null) {
        if (qr == null) qr = QuickRandom.SimpleRandom;
        for (var i = 0; i < source.Count; ++i) {
            int index = qr.GetInt(source.Count);
            T t = source[index];
            source[index] = source[i];
            source[i] = t;
        }
    }

    /// <summary>
    /// 获取随机子字符串
    /// </summary>
    /// <param name="sourceStr">源字符串</param>
    /// <param name="length">目标长度</param>
    /// <param name="qr">随机器</param>
    public static string GetRandomSubString(this string sourceStr, int length, QuickRandom qr = null) {
        if (qr == null) qr = QuickRandom.SimpleRandom;
        return new string(
                Enumerable.Repeat(sourceStr, length).
                Select(s => s[qr.GetInt(s.Length)]).ToArray()
            );
    }
}

public interface IWeightObject {
    public float Weight { get; }
}