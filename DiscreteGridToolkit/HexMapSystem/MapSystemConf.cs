using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EBlockType {
    Desert,
    Gobi,
    Oasis,
}
public static class MapSystemConf {
    public static Dictionary<EBlockType, Vector2> BlockTypeRange_VC = new Dictionary<EBlockType, Vector2> {
        {EBlockType.Desert, new Vector2(0f, 0.3f)},
        {EBlockType.Gobi, new Vector2(0.3f, 0.8f)},
        {EBlockType.Oasis, new Vector2(0.8f, 1f)},
    };

    public static Dictionary<EBlockType, float> BlockTypeRange_Generate = new Dictionary<EBlockType, float> {
        {EBlockType.Desert, 0.2f},
        {EBlockType.Gobi, 0.3f},
        {EBlockType.Oasis, 0.5f},
    };
    public static EBlockType GetBlockTypeByVC(float vc) {
        if (vc < BlockTypeRange_VC[EBlockType.Desert].y) return EBlockType.Desert;
        if (vc < BlockTypeRange_VC[EBlockType.Gobi].y) return EBlockType.Desert;
        return EBlockType.Oasis;
    }
}
