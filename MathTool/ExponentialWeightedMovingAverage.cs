using System;

namespace MycroftToolkit.MathTool {
    public class ExponentialWeightedMovingAverage {
        private readonly float _alpha;
        public float? EWMA { get; private set; }

        public ExponentialWeightedMovingAverage(float alpha) {
            if (alpha is <= 0 or > 1) {
                throw new ArgumentOutOfRangeException(nameof(alpha), "Alpha must be between 0 and 1 (exclusive).");
            }
            _alpha = alpha;
            EWMA = null;
        }

        public float AddValue(float value) {
            if (EWMA == null) {
                EWMA = value;
            } else {
                EWMA = _alpha * value + (1 - _alpha) * EWMA;
            }
            return EWMA.Value;
        }

        public void Reset() {
            EWMA = null;
        }
    }
}