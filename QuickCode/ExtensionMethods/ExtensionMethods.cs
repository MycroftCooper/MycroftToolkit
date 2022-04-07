using UnityEngine;
using UnityEngine.UI;

public static class ExtensionMethods {
    public static void RemoveComponent<Component>(this GameObject obj, bool immediate = false) {
        Component component = obj.GetComponent<Component>();
        if (component != null) {
            if (immediate) {
                Object.DestroyImmediate(component as Object, true);
            } else {
                Object.Destroy(component as Object);
            }
        }
    }

    public static void LocalRest(this Transform tr) {
        tr.localPosition = Vector3.zero;
        tr.localRotation = Quaternion.identity;
        tr.localScale = Vector3.one;
    }
    public static void Reset(this Transform tr) {
        tr.position = Vector3.zero;
        tr.rotation = Quaternion.identity;
        tr.localScale = Vector3.zero;
    }

    /// <summary>
    /// 只设置颜色不改变alpha
    /// </summary>
    public static void SetColor(this Image image, Color color) {
        Color tempColor = color;
        tempColor.a = image.color.a;
        image.color = tempColor;
    }

    /// <summary>
    /// 只设置alpha不改变颜色
    /// </summary>
    public static void SetAlpha(this Image image, float alpha) {
        Color tempColor = image.color;
        tempColor.a = alpha;
        image.color = tempColor;
    }

    /// <summary>
    /// 只设置alpha不改变颜色
    /// </summary>
    public static void SetAlpha(this SpriteRenderer spriteRenderer, float alpha) {
        Color tempColor = spriteRenderer.color;
        tempColor.a = alpha;
        spriteRenderer.color = tempColor;
    }
}
