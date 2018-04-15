using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

namespace UGUIExtend
{
    [AddComponentMenu("UI/BakeMesh", 12)]
    public class UIBakeMesh : MaskableGraphic
    {
        [SerializeField]
        public UIBakeMeshAsset asset;

        public override Texture mainTexture
        {
            get
            {
                if (!Application.isPlaying)
                    return s_WhiteTexture;

                if (asset == null || asset.sprite == null)
                {
                    if (material != null && material.mainTexture != null)
                    {
                        return material.mainTexture;
                    }
                    return s_WhiteTexture;
                }
                return asset.sprite.texture;
            }
        }

        protected override void UpdateGeometry()
        {
            if (asset != null && asset.mesh != null)
                canvasRenderer.SetMesh(asset.mesh);
        }
    }
}