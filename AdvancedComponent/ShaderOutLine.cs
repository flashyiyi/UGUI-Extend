using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/Effects/OutLineMaterial")]
    public class ShaderOutLine : BaseMeshEffect
    {
        private static List<UIVertex> output = new List<UIVertex>();
        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive())
                return;
            
            vh.GetUIVertexStream(output);
            
            int count = output.Count;
            for (int i = 0; i < count; i += 6)
            {
                if (i + 3 >= count)
                    break;

                UIVertex v1 = output[i];
                UIVertex v2 = output[i + 1];
                UIVertex v3 = output[i + 2];
                UIVertex v4 = output[i + 3];
                UIVertex v5 = output[i + 4];
                UIVertex v6 = output[i + 5];
                Vector2 bottomLeft = v1.uv0;
                Vector2 topRight = v4.uv0;
                if (bottomLeft.x > topRight.x)
                {
                    bottomLeft = v4.uv0;
                    topRight = v1.uv0;
                }
                Vector4 uvBounds = new Vector4(bottomLeft.x, bottomLeft.y, topRight.x, topRight.y);
                v1.tangent = uvBounds;
                v2.tangent = uvBounds;
                v3.tangent = uvBounds;
                v4.tangent = uvBounds;
                v5.tangent = uvBounds;
                v6.tangent = uvBounds;
                output[i] = v1;
                output[i + 1] = v2;
                output[i + 2] = v3;
                output[i + 3] = v4;
                output[i + 4] = v5;
                output[i + 5] = v6;
            }

            vh.Clear();
            vh.AddUIVertexTriangleStream(output);
        }
    }
}