using System.Linq;
using UnityEngine;

namespace MycroftToolkit.QuickCode {
    public static class AnimationExtension {
        public static AnimationClip GetClipByName(this Animator animator, string clipName) {
            if (animator == null || string.IsNullOrEmpty(clipName)) {
                return null;
            }

            RuntimeAnimatorController controller = animator.runtimeAnimatorController;
            return controller == null ? null : controller.animationClips.FirstOrDefault(clip => clip.name == clipName);
        }

        public static float GetLengthBySpeed(this AnimationClip clip, Animator animator) {
            if (clip == null || animator == null) {
                return 0f;
            }

            float speed = animator.speed;
            return clip.length / speed;
        }

        public static float GetRealLength(this AnimationClip clip, Animator animator) {
            if (clip == null || animator == null) {
                return 0f;
            }

            float speed = animator.speed;
            float timeScale = animator.updateMode == AnimatorUpdateMode.UnscaledTime ? 1f : Time.timeScale;

            return clip.length / (speed * timeScale);
        }
    }
}