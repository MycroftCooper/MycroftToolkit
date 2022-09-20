using UnityEngine;

public static class PhysicsExtension {
    public static bool ContainsLayer(this LayerMask layerMask, int layer, bool isIndex = true) {
        int layVal = isIndex? 1 << layer:layer;
        return (layVal & layerMask.value) > 0;
    }
    
    public static bool ContainsLayer(this LayerMask layerMask, string layerName) {
        int layVal = LayerMask.NameToLayer(layerName);
        return (layVal & layerMask.value) > 0;
    }
    
}
 