using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexBlockData {
    public EBlockType BlockType;

    private float vegetationCoverage;
    public float VegetationCoverage {
        get => vegetationCoverage;
        set {
            vegetationCoverage = value;
            BlockType = MapSystemConf.GetBlockTypeByVC(value);
        }
    }

    public Vector3 WorldPos;
    public Vector3Int LogicalPos;

}
