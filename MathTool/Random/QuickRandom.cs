using System.Collections.Generic;
using System.Linq;
using MycroftToolkit.DiscreteGridToolkit;
using UnityEngine;

namespace MycroftToolkit.MathTool {
    public interface IWeightObject {
        public int Weight { get; }
    }
    
    /// <summary>
    /// 简单随机数工具包
    /// @FAM 22.02.08
    /// </summary>
    public class QuickRandom {
        private int _seed;
        /// <summary>
        /// 随机种子
        /// </summary>
        public int Seed {
            get => _seed;
            set {
                _seed = value;
                _random = new System.Random(_seed);
                if (_noise != null) _noise.SetSeed(_seed);
            }
        }

        private System.Random _random;

        public QuickRandom(int seed) 
            => Seed = seed;
        
        public QuickRandom() => 
            Seed = (int)System.DateTime.Now.Ticks + new System.Random().Next(int.MinValue, int.MaxValue);

        private static QuickRandom _simple;
        public static QuickRandom Simple => _simple ??= new QuickRandom();

        #region 基础数据类型随机
        /// <summary>
        /// 获取随机Bool
        /// </summary>
        /// <param name="probability">为真的概率(默认0.5)</param>
        /// <returns>随机Bool</returns>
        public bool GetBool(float probability = 0.5f) => _random.NextDouble() < probability;

        /// <summary>
        /// 获取随机Int
        /// </summary>
        /// <returns>[0,x)</returns>
        public int GetInt(int x) => _random.Next(x);

        /// <summary>
        /// 获取随机Int
        /// </summary>
        /// <returns>[x,y)</returns>
        public int GetInt(int x, int y) => _random.Next(x, y);

        /// <summary>
        /// 获取随机Float
        /// </summary>
        /// <returns>[x,y)</returns>
        public float GetFloat(float x = 0, float y = 1) => (float)_random.NextDouble() * (y - x) + x;

        /// <summary>
        /// 获取随机Double
        /// </summary>
        /// <returns>[x,y)</returns>
        public double GetDouble(double x = 0, double y = 1) => _random.NextDouble() * (y - x) + x;
        #endregion


        #region 字符串类型随机
        public string GetString_Encoding(System.Text.Encoding encoding, int length) {
            byte[] bytes = new byte[length];
            _random.NextBytes(bytes);
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

        private QuickNoise _noise;
        public QuickNoise Noise => _noise ??= new QuickNoise(Seed); 
}
    
    public static class RandomExtend {
        #region 非加权随机
        /// <summary>
        /// 获取随机对象
        /// </summary>
        /// <param name="source">目标</param>
        /// <param name="qr">随机器</param>
        public static T GetRandomObject<T>(this IEnumerable<T> source, QuickRandom qr = null) {
            if(source == null)throw new System.NullReferenceException();
            T[] target = source as T[] ?? source.ToArray();
            if (source == null || !target.Any()) throw new System.IndexOutOfRangeException();
            qr ??= QuickRandom.Simple;
            return target[qr.GetInt(target.Length)];
        }

        /// <summary>
        /// 获取多个随机对象(可能重复)
        /// </summary>
        /// <param name="source">目标</param>
        /// <param name="num">对象个数</param>
        /// <param name="qr">随机器</param>
        public static T[] GetRandomObject_Repeatable<T>(this IEnumerable<T> source, int num, QuickRandom qr = null) {
            if(source == null)throw new System.NullReferenceException();
            T[] target = source as T[] ?? source.ToArray();
            if (!target.Any()) throw new System.IndexOutOfRangeException();
            qr ??= QuickRandom.Simple;
            T[] output = Enumerable.Repeat(target, num).
                    Select(s => s[qr.GetInt(s.Length)]).ToArray();
            return output;
        }

        /// <summary>
        /// 获取多个随机对象(不重复)
        /// </summary>
        /// <param name="source">目标</param>
        /// <param name="num">对象个数</param>
        /// <param name="qr">随机器</param>
        public static T[] GetRandomObject_UnRepeatable<T>(this IEnumerable<T> source, int num, QuickRandom qr = null) {
            if(source == null)throw new System.NullReferenceException();
            T[] target = source as T[] ?? source.ToArray();
            if (!target.Any()) throw new System.IndexOutOfRangeException();
            qr ??= QuickRandom.Simple;
            if (target.Length < num) {
                Debug.LogError("QuickRandom>Error>要求对象个数大于枚举器内对象个数!");
                return null;
            }
            target.Shuffle(qr);
            T[] output = target.Take(num).ToArray();
            return output;
        }
        #endregion

        
        #region 加权随机
        /// <summary>
        /// 获取随机对象(加权)
        /// </summary>
        /// <param name="source">目标</param>
        /// <param name="qr">随机器</param>
        public static T GetRandomWeightObject<T>(this IEnumerable<T> source, QuickRandom qr = null) where T : IWeightObject {
            if(source == null)throw new System.NullReferenceException();
            T[] target = source as T[] ?? source.ToArray();
            if (!target.Any()) throw new System.IndexOutOfRangeException();
            qr ??= QuickRandom.Simple;
            
            var totalWeight = target.Aggregate(0f, (total, current) => total + current.Weight);
            var targetWeight = qr.GetFloat(totalWeight);
            foreach (var item in target) {
                targetWeight -= item.Weight;
                if (targetWeight < 0) {
                    return item;
                }
            }
            return target.FirstOrDefault();
        }

        /// <summary>
        /// 获取多个随机对象(加权)(可能重复)
        /// </summary>
        /// <param name="source">目标</param>
        /// <param name="num">对象个数</param>
        /// <param name="qr">随机器</param>
        public static T[] GetRandomWeightObject_Repeatable<T>(this IEnumerable<T> source, int num, QuickRandom qr = null) where T : IWeightObject {
            if(source == null)throw new System.NullReferenceException();
            T[] target = source as T[] ?? source.ToArray();
            if (!target.Any()) throw new System.IndexOutOfRangeException();
            qr ??= QuickRandom.Simple;
            
            T[] output = new T[num];
            var totalWeight = target.Aggregate(0f, (total, current) => total + current.Weight);
            for (int i = 0; i < num; i++) {
                var targetWeight = qr.GetFloat(totalWeight);
                foreach (var item in target) {
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
        /// <param name="source">目标</param>
        /// <param name="num">对象个数</param>
        /// <param name="qr">随机器</param>
        public static T[] GetRandomWeightObject_UnRepeatable<T>(this IEnumerable<T> source, int num, QuickRandom qr = null) where T : IWeightObject {
            if(source == null)throw new System.NullReferenceException();
            List<T> target = source as List<T> ?? source.ToList();
            if (target.Count < num) {
                Debug.LogError("QuickRandom>Error>要求对象个数大于枚举器内对象个数!");
                return null;
            }
            qr ??= QuickRandom.Simple;

            T[] output = new T[num];
            var totalWeight = target.Aggregate(0f, (total, current) => total + current.Weight);
            for (int i = 0; i < num; i++) {
                var targetWeight = qr.GetFloat(totalWeight);
                for (int j = 0; j < target.Count; j++) {
                    targetWeight -= target[j].Weight;
                    if (!(targetWeight < 0)) continue;
                    
                    output[i] = target[j];
                    totalWeight -= target[j].Weight;
                    target.RemoveAt(j);
                    break;
                }
            }
            return output;
        }

        public static T GetRandomWeightObject<T>(this IDictionary<T, int> source, QuickRandom qr = null) {
            if(source == null)throw new System.NullReferenceException();
            if (!source.Any()) throw new System.IndexOutOfRangeException();
            qr ??= QuickRandom.Simple;
            
            var totalWeight = source.Aggregate(0f, (total, current) => total + current.Value);
            var targetWeight = qr.GetFloat(totalWeight);
            
            foreach (var item in source) {
                targetWeight -= item.Value;
                if (targetWeight < 0) {
                    return item.Key;
                }
            }
            return source.Keys.FirstOrDefault();
        }
        
        #endregion

        
        /// <summary>
        /// 重排列
        /// </summary>
        /// <param name="source">重排列目标</param>
        /// <param name="qr">随机器</param>
        public static void Shuffle<T>(this List<T> source, QuickRandom qr = null) {
            if (source.Count == 0) throw new System.IndexOutOfRangeException();
            qr ??= QuickRandom.Simple;
            for (var i = 0; i < source.Count; ++i) {
                int index = qr.GetInt(source.Count);
                (source[index], source[i]) = (source[i], source[index]);
            }
        }
        
        /// <summary>
        /// 重排列
        /// </summary>
        /// <param name="source">重排列目标</param>
        /// <param name="qr">随机器</param>
        public static void Shuffle<T>(this T[] source, QuickRandom qr = null) {
            if (source.Length == 0) throw new System.IndexOutOfRangeException();
            qr ??= QuickRandom.Simple;
            for (var i = 0; i < source.Length; ++i) {
                int index = qr.GetInt(source.Length);
                (source[index], source[i]) = (source[i], source[index]);
            }
        }

        /// <summary>
        /// 获取随机子字符串
        /// </summary>
        /// <param name="sourceStr">源字符串</param>
        /// <param name="length">目标长度</param>
        /// <param name="qr">随机器</param>
        public static string GetRandomSubString(this string sourceStr, int length, QuickRandom qr = null) {
            qr ??= QuickRandom.Simple;
            return new string(
                    Enumerable.Repeat(sourceStr, length).
                    Select(s => s[qr.GetInt(s.Length)]).ToArray()
                );
        }
    }

    public static class QuickRandomInArea {
        public static Vector2 GetRandomPoint_Circular(Vector2 pos, float radius, QuickRandom random = null) {
            random ??= QuickRandom.Simple;
            float l = random.GetFloat(0, radius);
            float a = random.GetFloat(0, 360);
            return new Polar2(l, a).ToVector2() + pos;
        }
    }
}
