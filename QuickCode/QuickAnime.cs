using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace MycroftToolkit.QuickCode {
    public static class QuickAnime {

        /// <summary>
        /// 只设置颜色不改变alpha
        /// </summary>
        public static void SetImageColor(Image image, Color color) {
            Color tempColor = color;
            tempColor.a = image.color.a;
            image.color = tempColor;
        }

        /// <summary>
        /// 只设置alpha不改变颜色
        /// </summary>
        public static void SetImageAlpha(Image image, float alpha) {
            Color tempColor = image.color;
            tempColor.a = alpha;
            image.color = tempColor;
        }

        /// <summary>
        /// 只设置alpha不改变颜色
        /// </summary>
        public static void SetSpriteAlpha(SpriteRenderer spriteRenderer, float alpha) {
            Color tempColor = spriteRenderer.color;
            tempColor.a = alpha;
            spriteRenderer.color = tempColor;
        }

        /// <param name="animeTime">
        /// 动画时间
        /// </param>
        /// <param name="percentageHandler">
        /// 根据当前时间进度设置的回调
        /// </param>
        public static IEnumerator Animation(float animeTime, Action<float> percentageHandler) {
            float startTime = 0;
            while (startTime < animeTime) {
                yield return null;
                startTime += Time.deltaTime;
                float percentage = startTime / animeTime;
                percentageHandler(percentage);
            }
        }

        public static IEnumerator AnimationRealTime(float animeTime, Action<float> percentageHandler) {
            float startTime = 0;
            while (startTime < animeTime) {
                yield return new WaitForFixedUpdate();
                startTime += Time.deltaTime;
                float percentage = startTime / animeTime;
                percentageHandler(percentage);
            }
        }

        public static void AlphaTween(SpriteRenderer sr, float targerAlphaVal, float durTime, Ease easeType = Ease.Linear) {
            if (null != sr) {
                Color targetColor = sr.color;
                targetColor.a = targerAlphaVal;
                sr.DOColor(targetColor, durTime).SetEase(easeType);
            }
        }

        public static bool HasParam(this Animator anim, string name) {
            foreach (AnimatorControllerParameter param in anim.parameters) {
                if (param.name == name)
                    return true;
            }
            return false;
        }

        public static bool HasParam(this Animator anim, int nameHash) {
            foreach (AnimatorControllerParameter param in anim.parameters) {
                if (param.nameHash == nameHash)
                    return true;
            }
            return false;
        }
    }
}