using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace MycroftToolkit.QuickResource.SpriteImportTool {
    public class SpriteImportPreSet:ScriptableObject {
        [BoxGroup("Sprite导入设置"), Title("模式")]
        [BoxGroup("Sprite导入设置"), LabelText("Sprite模式"), EnumPaging] 
        public SpriteImportMode  importMode;
        [BoxGroup("Sprite导入设置"), LabelText("网格类型"), EnumPaging] 
        public SpriteMeshType spriteMeshType;
        [BoxGroup("Sprite导入设置"), LabelText("拼接模式"), EnumPaging]
        public TextureWrapMode wrapMode;
        [BoxGroup("Sprite导入设置"), LabelText("过滤模式"), EnumPaging]
        public FilterMode filterMode;
        [BoxGroup("Sprite导入设置"), LabelText("压缩类型"), EnumPaging]
        public TextureImporterCompression textureImporterCompression;
    
        [BoxGroup("Sprite导入设置"), Space, Title("像素")]
        [BoxGroup("Sprite导入设置"), LabelText("每单位像素数")] 
        public int pixelsPerUnit;
        [BoxGroup("Sprite导入设置"), LabelText("挤出边缘"), Range(0,32)]
        public uint spriteExtrude;
    
        [BoxGroup("Sprite导入设置"), Space, Title("高级")]
        [BoxGroup("Sprite导入设置"), LabelText("生成物理形状")]
        public bool generatePhysicsShape;
        [BoxGroup("Sprite导入设置"), LabelText("Alpha透明")]
        public bool alphaIsTransparency;
        [BoxGroup("Sprite导入设置"), LabelText("启用读写")]
        public bool readWriteEnabled;
        [BoxGroup("Sprite导入设置"), LabelText("启用mipmap")]
        public bool mipmapEnabled;

        [BoxGroup("Sprite锚点设置"), Space, EnumPaging] 
        public SpriteAlignment pivotMode;
        [ShowIfGroup("Sprite锚点设置/pivotMode", Value = SpriteAlignment.Custom),  LabelText("使用像素坐标")]
        public bool isPixels;
        [ShowIfGroup("Sprite锚点设置/pivotMode", Value = SpriteAlignment.Custom), LabelText("锚点")]
        public Vector2 pivot;
    
        [ShowIfGroup("Sprite切片设置/importMode", Value = SpriteImportMode.Multiple)]
        [BoxGroup("Sprite切片设置"),  Space, LabelText("自动切片")]
        public bool autoSlicing = true;
        [HideIfGroup("Sprite切片设置/autoSlicing"), LabelText("按大小切片(否则是按行列切)")]
        public bool slicingUseSize;
        [HideIfGroup("Sprite切片设置/autoSlicing"), LabelText("切片设置")]
        public Vector2Int slicingInfo;
    }
}