using UnityEngine;
namespace UGUIExtend
{
    /// <summary>
    /// 按RectTransform裁切Renderer
    /// </summary>
    [ExecuteInEditMode]
    public class UIClipAble : MonoBehaviour
    {
        static readonly int clipRectId = Shader.PropertyToID("_ClipRect");

        [SerializeField] public RectTransform mask;

        MaterialPropertyBlock materialBlock;
        Renderer[] renderers;
        Vector4 clipRect;

        private void OnEnable()
        {
            materialBlock = new MaterialPropertyBlock();
            renderers = GetComponentsInChildren<Renderer>();
        }

        static readonly Vector3[] corners = new Vector3[4];
        private void Update()
        {
            if (mask != null)
            {
                mask.GetWorldCorners(corners);
                Vector4 r = new Vector4(corners[0].x, corners[0].y, corners[2].x, corners[2].y);
                if (r != clipRect)
                {
                    clipRect = r;
                    materialBlock.SetVector(clipRectId, clipRect);
                    foreach (Renderer renderer in renderers)
                    {
                        renderer.SetPropertyBlock(materialBlock);
                    }
                }
            }
        }
    }
}
