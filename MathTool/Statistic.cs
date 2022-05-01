using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MycroftToolkit.MathTool {
    public static class Statistic {
        public static float Average(this List<int> input) {
            int add = 0;
            input.ForEach(x => add += x);
            return (float)add / (float)input.Count;
        }
        public static float Average(this List<float> input) {
            float add = 0;
            input.ForEach(x => add += x);
            return (float)add / (float)input.Count;
        }
        public static int Median(this List<int> input) {
            List<int> sort = new List<int>(input);
            sort.Sort();
            return sort[sort.Count / 2];
        }
        public static float Median(this List<float> input) {
            List<float> sort = new List<float>(input);
            sort.Sort();
            return sort[sort.Count / 2];
        }
    }
}
