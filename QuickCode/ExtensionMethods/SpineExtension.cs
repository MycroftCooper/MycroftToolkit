using Spine.Unity;
using UnityEngine;

namespace MycroftToolkit.QuickCode {
    public static class SpineExtension {
        public static Vector3 GetBoneUnityWorldPos(this SkeletonRenderer skeletonRenderer, string boneName) {
            if (skeletonRenderer == null || string.IsNullOrEmpty(boneName)) {
                Debug.LogError("boneName为空!或skeletonRenderer为空！");
                return Vector3.zero;
            }

            Spine.Bone bone = skeletonRenderer.skeleton.FindBone(boneName);
            if (bone == null) {
                Debug.LogError($"找不到{boneName}!");
                return Vector3.zero;
            }

            Vector3 localBonePosition = new Vector3(bone.WorldX, bone.WorldY, 0);
            Vector3 worldBonePosition = skeletonRenderer.transform.TransformPoint(localBonePosition);
            return worldBonePosition;
        }

        public static Vector3 GetBoneUnityWorldPos(this Spine.Bone bone, Transform spineGameObjectTransform) {
            if (bone == null || spineGameObjectTransform == null) {
                Debug.LogError("bone为空!或spineGameObjectTransform为空！");
                return Vector3.zero;
            }

            Vector3 localBonePosition = new Vector3(bone.WorldX, bone.WorldY, 0);
            Vector3 worldBonePosition = spineGameObjectTransform.TransformPoint(localBonePosition);
            return worldBonePosition;
        }
    }
}
