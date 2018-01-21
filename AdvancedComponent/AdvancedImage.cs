using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIExtend
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasRenderer))]
    [AddComponentMenu("UI/AdvancedImage", 12)]
    public class AdvancedImage : Image
    {
        /// <summary>
        /// 可见度
        /// </summary>
        [SerializeField]
        private bool m_Visible = true;
        public bool visible
        {
            get
            {
                return m_Visible;
                
            }

            set
            {
                if (m_Visible != value)
                {
                    m_Visible = value;
                    UpdateVisible();
                    if (m_Visible)
                        CanvasUpdateRegistry.RegisterCanvasElementForGraphicRebuild(this);
                }
            }
        }

        /// <summary>
        /// 是否绘制
        /// </summary>
        [SerializeField]
        private bool m_EnabledPopulateMesh = true;
        public bool enabledPopulateMesh
        {
            get
            {
                return m_EnabledPopulateMesh;
            }

            set
            {
                if (m_EnabledPopulateMesh != value)
                {
                    SetVerticesDirty();
                }
                m_EnabledPopulateMesh = value;
            }
        }

        /// <summary>
        /// 是否使用Sprite的网格
        /// </summary>
        [SerializeField]
        private bool m_UseSpriteMesh;
        public bool useSpriteMesh
        {
            get
            {
                return m_UseSpriteMesh;
            }

            set
            {
                if (m_UseSpriteMesh != value)
                {
                    SetVerticesDirty();
                }
                m_UseSpriteMesh = value;
            }
        }

        /// <summary>
        /// 水平镜像
        /// </summary>
        [SerializeField]
        private bool m_HorizontalMirror;
        public bool horizontalMirror
        {
            get
            {
                return m_HorizontalMirror;
            }

            set
            {
                if (m_UseSpriteMesh != value)
                {
                    SetVerticesDirty();
                }
                m_HorizontalMirror = value;
            }
        }

        /// <summary>
        /// 垂直镜像
        /// </summary>
        [SerializeField]
        private bool m_VerticalMirror;
        public bool verticalMirror
        {
            get
            {
                return m_VerticalMirror;
            }

            set
            {
                if (m_VerticalMirror != value)
                {
                    SetVerticesDirty();
                }
                m_VerticalMirror = value;
            }
        }

        /// <summary>
        /// 设置中间挖空部分的边缘，FillCenter激活时无效
        /// </summary>
        [SerializeField]
        private Vector4 m_FillBorders = Vector4.zero;
        public Vector4 fillBorders
        {
            get
            {
                return m_FillBorders;
            }

            set
            {
                if (m_FillBorders != value)
                {
                    SetVerticesDirty();
                } 
                m_FillBorders = value;
            }
            
            
        }
        

        /// <summary>
        /// 碰撞箱
        /// </summary>
        [SerializeField]
        public Collider2D hitArea;

        /// <summary>
        /// 使用Sprite网格作为碰撞箱
        /// </summary>
        [SerializeField]
        public bool useSpriteHitArea;
        
        /// <summary>
        /// 碰撞箱缩放
        /// </summary>
        [SerializeField]
        public Vector2 hitScale = Vector2.one;

        protected override void OnEnable()
        {
            base.OnEnable();
            UpdateVisible();
        }

        protected virtual void UpdateVisible()
        {
            this.canvasRenderer.cull = !m_Visible;
        }

        public override bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        {
            if (!useSpriteHitArea && hitArea == null && hitScale == Vector2.one)
            {
                return base.IsRaycastLocationValid(screenPoint, eventCamera);
            }

            if (eventCamera != null)
            {
                screenPoint = eventCamera.ScreenToWorldPoint(screenPoint);
            }
            if (hitScale != Vector2.one)
            {
                Vector2 centerInScreen = transform.position;
                screenPoint = centerInScreen + Vector2.Scale(screenPoint - centerInScreen, new Vector2(1f / hitScale.x, 1f / hitScale.y));
            }
            if (useSpriteHitArea && IsRaycastSprite(rectTransform.InverseTransformPoint(screenPoint)) ||
                hitArea != null && hitArea.OverlapPoint(screenPoint) ||
                !useSpriteHitArea && hitArea == null && rectTransform.rect.Contains(rectTransform.InverseTransformPoint(screenPoint)))
            {
                return true;
            }
            return false;
        }

        bool IsRaycastSprite(Vector2 point)
        {
            Rect r = GetPixelAdjustedRect();
            var size = new Vector2(overrideSprite.rect.width, overrideSprite.rect.height);
            Bounds bounds = overrideSprite.bounds;
            
            if (preserveAspect)
            {
                PreserveAspect(ref r, size);
            }

            float w = r.width / bounds.size.x;
            float h = r.height / bounds.size.y;
            point.x = (point.x + w * bounds.center.x) / w;
            point.y = (point.y + h * bounds.center.y) / h;

            var vertices = overrideSprite.vertices;
            var triangles = overrideSprite.triangles;
            int count = triangles.Length;
            for (int i = 0;i < count;i+= 3)
            {
                Vector2 v1 = vertices[triangles[i]];
                Vector2 v2 = vertices[triangles[i + 1]];
                Vector2 v3 = vertices[triangles[i + 2]];
                if (PointinTriangle(v1, v2, v3, point))
                    return true;
            }
            return false;
        }

        // 三角形碰撞检测
        static bool PointinTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
        {
            Vector2 v0 = C - A;
            Vector2 v1 = B - A;
            Vector2 v2 = P - A;

            float dot00 = Vector2.Dot(v0, v0);
            float dot01 = Vector2.Dot(v0, v1);
            float dot02 = Vector2.Dot(v0, v2);
            float dot11 = Vector2.Dot(v1, v1);
            float dot12 = Vector2.Dot(v1, v2);

            float inverDeno = 1 / (dot00 * dot11 - dot01 * dot01);

            float u = (dot11 * dot02 - dot01 * dot12) * inverDeno;
            if (u < 0 || u > 1) 
            {
                return false;
            }

            float v = (dot00 * dot12 - dot01 * dot02) * inverDeno;
            if (v < 0 || v > 1)
            {
                return false;
            }

            return u + v <= 1;
        }

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            if (!m_EnabledPopulateMesh)
            {
                toFill.Clear();
                return;
            }

            if (overrideSprite == null)
            {
                base.OnPopulateMesh(toFill);
                return;
            }

            switch (type)
            {
                case Type.Simple:
                    if (m_UseSpriteMesh)
                        GenerateSpriteSprite(toFill, preserveAspect);
                    else
                        GenerateSimpleSprite(toFill, preserveAspect);
                    break;
                case Type.Sliced:
                    GenerateSlicedSprite(toFill);
                    break;
                default:
                    base.OnPopulateMesh(toFill);
                    break;
            }
        }

        public override void SetNativeSize()
        {
            if (overrideSprite != null && (type == Type.Simple || type == Type.Sliced) && (m_HorizontalMirror || m_VerticalMirror))
            {
                float w = overrideSprite.rect.width / pixelsPerUnit;
                float h = overrideSprite.rect.height / pixelsPerUnit;
                rectTransform.anchorMax = rectTransform.anchorMin;

                if (m_HorizontalMirror && m_VerticalMirror)
                {
                    rectTransform.sizeDelta = new Vector2(w * 2, h * 2);
                }
                else if (m_HorizontalMirror)
                {
                    rectTransform.sizeDelta = new Vector2(w * 2, h);
                }
                else
                {
                    rectTransform.sizeDelta = new Vector2(w, h * 2);
                }
            }
            else
            {
                base.SetNativeSize();
            }
        }

        /// <summary>
        /// 用Sprite的网格数据直接创建顶点
        /// </summary>
        void GenerateSpriteSprite(VertexHelper vh, bool shouldPreserveAspect)
        {
            Rect r = GetPixelAdjustedRect();
            var size = new Vector2(overrideSprite.rect.width, overrideSprite.rect.height);
            Bounds bounds = overrideSprite.bounds;

            if (shouldPreserveAspect)
            {
                PreserveAspect(ref r, size);
            }

            float w = r.width / bounds.size.x;
            float h = r.height / bounds.size.y;
            Vector4 v = new Vector4(-w * bounds.center.x,
                -h * bounds.center.y,
                w,
                h);

            Color32 color32 = color;
            vh.Clear();
            var vertices = overrideSprite.vertices;
            var uv = overrideSprite.uv;
            int count = vertices.Length;
            for (int i = 0; i < count; i++)
            {
                Vector2 vert = vertices[i];
                vh.AddVert(new Vector3(v.x + vert.x * v.z, v.y + vert.y * v.w, 0), color32, uv[i]);
            }
            var triangles = overrideSprite.triangles;
            count = triangles.Length;
            for (int i = 0; i < count; i += 3)
            {
                vh.AddTriangle(triangles[i], triangles[i + 1], triangles[i + 2]);
            }
        }

        

        /// <summary>
        /// Generate vertices for a simple Image.
        /// </summary>
        void GenerateSimpleSprite(VertexHelper vh, bool shouldPreserveAspect)
        {
            Vector4 v = GetDrawingDimensions(shouldPreserveAspect);
            var uv = (overrideSprite != null) ? UnityEngine.Sprites.DataUtility.GetOuterUV(overrideSprite) : Vector4.zero;

            var color32 = color;
            vh.Clear();

            if (!fillCenter)
            {
                if (m_FillBorders != Vector4.zero)
                {
                    AddBorders(vh,
                        v,
                        m_FillBorders, color,
                        uv);
                }
            }
            else
            {
                if (!m_HorizontalMirror && !m_VerticalMirror)
                {
                    vh.AddVert(new Vector3(v.x, v.y), color32, new Vector2(uv.x, uv.y));
                    vh.AddVert(new Vector3(v.x, v.w), color32, new Vector2(uv.x, uv.w));
                    vh.AddVert(new Vector3(v.z, v.w), color32, new Vector2(uv.z, uv.w));
                    vh.AddVert(new Vector3(v.z, v.y), color32, new Vector2(uv.z, uv.y));

                    vh.AddTriangle(0, 1, 2);
                    vh.AddTriangle(2, 3, 0);
                }
                else
                {
                    AddMirror(vh, v, color32, uv, m_HorizontalMirror, m_VerticalMirror);
                }
            }
        }

        /// <summary>
        /// Generate vertices for a 9-sliced Image.
        /// </summary>

        static readonly Vector2[] s_VertScratch = new Vector2[4];
        static readonly Vector2[] s_UVScratch = new Vector2[4];

        private void GenerateSlicedSprite(VertexHelper toFill)
        {
            if (!hasBorder)
            {
                GenerateSimpleSprite(toFill, false);
                return;
            }
            
            Vector4 outer, inner, padding, border;

            if (overrideSprite != null)
            {
                outer = UnityEngine.Sprites.DataUtility.GetOuterUV(overrideSprite);
                inner = UnityEngine.Sprites.DataUtility.GetInnerUV(overrideSprite);
                padding = UnityEngine.Sprites.DataUtility.GetPadding(overrideSprite);
                border = overrideSprite.border;
            }
            else
            {
                outer = Vector4.zero;
                inner = Vector4.zero;
                padding = Vector4.zero;
                border = Vector4.zero;
            }

            Rect rect = GetPixelAdjustedRect();
            Vector4 adjustedBorders = GetAdjustedBorders(border / pixelsPerUnit, rect);
            padding = padding / pixelsPerUnit;
            if (m_HorizontalMirror)
            {
                adjustedBorders.z = adjustedBorders.x;
            }
            if (m_VerticalMirror)
            {
                adjustedBorders.y = adjustedBorders.w;
            }

            s_VertScratch[0] = new Vector2(padding.x, padding.y);
            s_VertScratch[3] = new Vector2(rect.width - padding.z, rect.height - padding.w);

            s_VertScratch[1].x = adjustedBorders.x;
            s_VertScratch[1].y = adjustedBorders.y;

            s_VertScratch[2].x = rect.width - adjustedBorders.z;
            s_VertScratch[2].y = rect.height - adjustedBorders.w;

            for (int i = 0; i < 4; ++i)
            {
                s_VertScratch[i].x += rect.x;
                s_VertScratch[i].y += rect.y;
            }

            s_UVScratch[0] = new Vector2(outer.x, outer.y);
            s_UVScratch[1] = new Vector2(inner.x, inner.y);
            s_UVScratch[2] = new Vector2(inner.z, inner.w);
            s_UVScratch[3] = new Vector2(outer.z, outer.w);

            toFill.Clear();

            for (int x = 0; x < 3; ++x)
            {
                int x2 = x + 1;
           
                for (int y = 0; y < 3; ++y)
                {
                    int y2 = y + 1;

                    if (!fillCenter && x == 1 && y == 1)
                    {
                        if (m_FillBorders != Vector4.zero)
                        {
                            AddBorders(toFill, 
                                new Vector4(s_VertScratch[x].x, s_VertScratch[y].y, s_VertScratch[x2].x, s_VertScratch[y2].y),
                                m_FillBorders, color, 
                                new Vector4(s_UVScratch[x].x, s_UVScratch[y].y, s_UVScratch[x2].x, s_UVScratch[y2].y));
                        }
                        continue;
                    }
                    
                    Vector4 uv;
                    if (x == 2 && y == 0 && m_HorizontalMirror && m_VerticalMirror)
                    {
                        uv = new Vector4(s_UVScratch[1].x, s_UVScratch[3].y, s_UVScratch[0].x, s_UVScratch[2].y);
                    }
                    else if (x == 2 && m_HorizontalMirror)
                    {
                        uv = new Vector4(s_UVScratch[1].x, s_UVScratch[y].y, s_UVScratch[0].x, s_UVScratch[y2].y);
                    }
                    else if (y == 0 && m_VerticalMirror)
                    {
                        uv = new Vector4(s_UVScratch[x].x, s_UVScratch[3].y, s_UVScratch[x2].x, s_UVScratch[2].y);
                    }
                    else
                    {
                        uv = new Vector4(s_UVScratch[x].x, s_UVScratch[y].y, s_UVScratch[x2].x, s_UVScratch[y2].y);
                    }

                    AddMirror(toFill,
                        new Vector4(s_VertScratch[x].x, s_VertScratch[y].y,s_VertScratch[x2].x, s_VertScratch[y2].y),
                        color,
                        uv,
                        m_HorizontalMirror && x == 1, m_VerticalMirror && y == 1);
                }
            }
        }

        static void AddMirror(VertexHelper vh, Vector4 v, Color32 color32, Vector4 uv, bool hMirror, bool vMirror)
        {
            if (v.x >= v.z || v.y >= v.w)
            {
                return;
            }
            int si = vh.currentVertCount;
            
            if (hMirror && vMirror)
            {
                float d = (v.z - v.x) / 2f;
                v.z -= d;
                float d2 = (v.w - v.y) / 2f;
                v.y += d2;
                vh.AddVert(new Vector3(v.x, v.y), color32, new Vector2(uv.x, uv.y));
                vh.AddVert(new Vector3(v.x, v.w), color32, new Vector2(uv.x, uv.w));
                vh.AddVert(new Vector3(v.z, v.w), color32, new Vector2(uv.z, uv.w));
                vh.AddVert(new Vector3(v.z, v.y), color32, new Vector2(uv.z, uv.y));

                vh.AddVert(new Vector3(v.z + d, v.w), color32, new Vector2(uv.x, uv.w));
                vh.AddVert(new Vector3(v.z + d, v.y), color32, new Vector2(uv.x, uv.y));

                vh.AddVert(new Vector3(v.x, v.y - d2), color32, new Vector2(uv.x, uv.w));
                vh.AddVert(new Vector3(v.z, v.y - d2), color32, new Vector2(uv.z, uv.w));
                vh.AddVert(new Vector3(v.z + d, v.y - d2), color32, new Vector2(uv.x, uv.w));

                vh.AddTriangle(si, si + 1, si + 2);
                vh.AddTriangle(si + 2, si + 3, si);
                vh.AddTriangle(si + 3, si + 2, si + 5);
                vh.AddTriangle(si + 5, si + 2, si + 4);
                vh.AddTriangle(si, si + 3, si + 7);
                vh.AddTriangle(si, si + 7, si + 6);
                vh.AddTriangle(si + 3, si + 5, si + 7);
                vh.AddTriangle(si + 5, si + 8, si + 7);
            }
            else if (hMirror)
            {
                float d = (v.z - v.x) / 2f;
                v.z -= d;
                vh.AddVert(new Vector3(v.x, v.y), color32, new Vector2(uv.x, uv.y));
                vh.AddVert(new Vector3(v.x, v.w), color32, new Vector2(uv.x, uv.w));
                vh.AddVert(new Vector3(v.z, v.w), color32, new Vector2(uv.z, uv.w));
                vh.AddVert(new Vector3(v.z, v.y), color32, new Vector2(uv.z, uv.y));

                vh.AddVert(new Vector3(v.z + d, v.w), color32, new Vector2(uv.x, uv.w));
                vh.AddVert(new Vector3(v.z + d, v.y), color32, new Vector2(uv.x, uv.y));

                vh.AddTriangle(si, si + 1, si + 2);
                vh.AddTriangle(si + 2, si + 3, si);
                vh.AddTriangle(si + 3, si + 2, si + 5);
                vh.AddTriangle(si + 5, si + 2, si + 4);
            }
            else if (vMirror)
            {
                float d = (v.w - v.y) / 2f;
                v.y += d;
                vh.AddVert(new Vector3(v.x, v.y), color32, new Vector2(uv.x, uv.y));
                vh.AddVert(new Vector3(v.x, v.w), color32, new Vector2(uv.x, uv.w));
                vh.AddVert(new Vector3(v.z, v.w), color32, new Vector2(uv.z, uv.w));
                vh.AddVert(new Vector3(v.z, v.y), color32, new Vector2(uv.z, uv.y));

                vh.AddVert(new Vector3(v.x, v.y - d), color32, new Vector2(uv.x, uv.w));
                vh.AddVert(new Vector3(v.z, v.y - d), color32, new Vector2(uv.z, uv.w));

                vh.AddTriangle(si, si + 1, si + 2);
                vh.AddTriangle(si + 2, si + 3, si);
                vh.AddTriangle(si, si + 3, si + 5);
                vh.AddTriangle(si, si + 5, si + 4);
            }
            else
            {
                vh.AddVert(new Vector3(v.x, v.y), color32, new Vector2(uv.x, uv.y));
                vh.AddVert(new Vector3(v.x, v.w), color32, new Vector2(uv.x, uv.w));
                vh.AddVert(new Vector3(v.z, v.w), color32, new Vector2(uv.z, uv.w));
                vh.AddVert(new Vector3(v.z, v.y), color32, new Vector2(uv.z, uv.y));
                
                vh.AddTriangle(si, si + 1, si + 2);
                vh.AddTriangle(si + 2, si + 3, si);
            }
        }

        static void AddBorders(VertexHelper vertexHelper, Vector4 pos, Vector4 border,Color32 color, Vector4 uv)
        {
            float w = pos.z - pos.x;
            float h = pos.w - pos.y;
            float uvW = uv.z - uv.x;
            float uvH = uv.w - uv.y;
            Vector4 inner = new Vector4(pos.x + w * border.x,
                pos.y + h * border.y,
                pos.z - w * border.z,
                pos.w - h * border.w);

            Vector4 innerUv = new Vector4(uv.x + uvW * border.x,
                uv.y + uvH * border.y,
                uv.z - uvW * border.z,
                uv.w - uvH * border.w);

            if (border.y > 0)
                AddQuad(vertexHelper, new Vector4(pos.x, pos.y, pos.z, inner.y), color, new Vector4(uv.x, uv.y, uv.z, innerUv.y));
            if (border.w > 0)
                AddQuad(vertexHelper, new Vector4(pos.x, inner.w, pos.z, pos.w), color, new Vector4(uv.x, innerUv.w, uv.z, uv.w));
            if (border.x > 0)
                AddQuad(vertexHelper, new Vector4(pos.x, inner.y, inner.x, inner.w), color, new Vector4(uv.x, innerUv.y, innerUv.x, innerUv.w));
            if (border.z > 0)
                AddQuad(vertexHelper, new Vector4(inner.z, inner.y, pos.z, inner.w), color, new Vector4(innerUv.z, innerUv.y, uv.z, innerUv.w));
        }

        static void AddQuad(VertexHelper vertexHelper, Vector4 pos, Color32 color, Vector4 uv)
        {
            int startIndex = vertexHelper.currentVertCount;

            vertexHelper.AddVert(new Vector3(pos.x, pos.y, 0), color, new Vector2(uv.x, uv.y));
            vertexHelper.AddVert(new Vector3(pos.x, pos.w, 0), color, new Vector2(uv.x, uv.w));
            vertexHelper.AddVert(new Vector3(pos.z, pos.w, 0), color, new Vector2(uv.z, uv.w));
            vertexHelper.AddVert(new Vector3(pos.z, pos.y, 0), color, new Vector2(uv.z, uv.y));

            vertexHelper.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
            vertexHelper.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
        }

        Vector4 GetAdjustedBorders(Vector4 border, Rect rect)
        {
            for (int axis = 0; axis <= 1; axis++)
            {
                // If the rect is smaller than the combined borders, then there's not room for the borders at their normal size.
                // In order to avoid artefacts with overlapping borders, we scale the borders down to fit.
                float combinedBorders = border[axis] + border[axis + 2];
                if (rect.size[axis] < combinedBorders && combinedBorders != 0)
                {
                    float borderScaleRatio = rect.size[axis] / combinedBorders;
                    border[axis] *= borderScaleRatio;
                    border[axis + 2] *= borderScaleRatio;
                }
            }
            return border;
        }

        
        /// Image's dimensions used for drawing. X = left, Y = bottom, Z = right, W = top.
        private Vector4 GetDrawingDimensions(bool shouldPreserveAspect)
        {
            Rect r = GetPixelAdjustedRect();
            
            var size = overrideSprite == null ? Vector2.zero : new Vector2(overrideSprite.rect.width, overrideSprite.rect.height);
            var padding = overrideSprite == null ? Vector4.zero : UnityEngine.Sprites.DataUtility.GetPadding(overrideSprite);
            int spriteW = Mathf.RoundToInt(size.x);
            int spriteH = Mathf.RoundToInt(size.y);

            Vector4 v = new Vector4(
                        padding.x / spriteW,
                        padding.y / spriteH,
                        (spriteW - padding.z) / spriteW,
                        (spriteH - padding.w) / spriteH);

            if (shouldPreserveAspect)
            {
                PreserveAspect(ref r, size);
            }

            v = new Vector4(
                    r.x + r.width * v.x,
                    r.y + r.height * v.y,
                    r.x + r.width * v.z,
                    r.y + r.height * v.w
                    );

            return v;
        }

        private void PreserveAspect(ref Rect r,Vector2 size)
        {
            if (size.sqrMagnitude == 0.0f)
                return;

            var spriteRatio = size.x / size.y;
            var rectRatio = r.width / r.height;

            if (spriteRatio > rectRatio)
            {
                var oldHeight = r.height;
                r.height = r.width * (1.0f / spriteRatio);
                r.y += (oldHeight - r.height) * rectTransform.pivot.y;
            }
            else
            {
                var oldWidth = r.width;
                r.width = r.height * spriteRatio;
                r.x += (oldWidth - r.width) * rectTransform.pivot.x;
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            UpdateVisible();
        }

        private readonly static Vector3[] fourCorners = new Vector3[4];
        private void OnDrawGizmosSelected()
        {
            if (this.raycastTarget)
            {
                Gizmos.color = Color.green;

                if (this.useSpriteHitArea)
                {
                    Rect r = GetPixelAdjustedRect();
                    var size = new Vector2(overrideSprite.rect.width, overrideSprite.rect.height);
                    Bounds bounds = overrideSprite.bounds;

                    if (preserveAspect)
                    {
                        PreserveAspect(ref r, size);
                    }

                    float w = r.width / bounds.size.x;
                    float h = r.height / bounds.size.y;
                    Vector4 v = new Vector4(-w * bounds.center.x,
                        -h * bounds.center.y,
                        w,
                        h);
                    int count = overrideSprite.triangles.Length;
                    var vertices = overrideSprite.vertices;
                    var triangles = overrideSprite.triangles;
                    Vector2 center = transform.position;
                    for (int i = 0; i < count; i += 3)
                    {
                        Vector2 v1 = vertices[triangles[i]];
                        Vector2 v2 = vertices[triangles[i + 1]];
                        Vector2 v3 = vertices[triangles[i + 2]];
                        v1 = transform.TransformPoint(new Vector2(v.x + v1.x * v.z, v.y + v1.y * v.w));
                        v2 = transform.TransformPoint(new Vector2(v.x + v2.x * v.z, v.y + v2.y * v.w));
                        v3 = transform.TransformPoint(new Vector2(v.x + v3.x * v.z, v.y + v3.y * v.w));
                        v1 = transform.position + Vector3.Scale(v1 - center, hitScale);
                        v2 = transform.position + Vector3.Scale(v2 - center, hitScale);
                        v3 = transform.position + Vector3.Scale(v3 - center, hitScale);
                        Gizmos.DrawLine(v1, v2);
                        Gizmos.DrawLine(v2, v3);
                        Gizmos.DrawLine(v3, v1);
                    }
                }
                else
                {
                    rectTransform.GetWorldCorners(fourCorners);
                    for (int i = 0; i < 4; i++)
                    {
                        fourCorners[i] = transform.position + Vector3.Scale(fourCorners[i] - transform.position,hitScale);
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        Gizmos.DrawLine(fourCorners[i], fourCorners[(i + 1) % 4]);
                    }
                }
            }
        }
#endif
    }
}



