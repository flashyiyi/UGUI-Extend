using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using UnityEngine.Serialization;
using System;

namespace UGUIExtend
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasRenderer))]
    [AddComponentMenu("UI/AdvancedText", 12)]
    public class AdvancedText : Text
    {
        /// <summary>
        /// 加载图片的方法
        /// </summary>
        protected virtual void LoadSprite(Image image, string path)
        {
            if (image.sprite == null || image.sprite.name != path)
            {
                image.sprite = Resources.Load<Sprite>(imagePathRoot + path); ;
            }
        }

        [Serializable]
        public class CharOffest
        {
            public Vector2 position = Vector2.zero;
            public float rotation = 0f;
            public Vector2 scale = Vector2.one;
        }

        [Serializable]
        public class InLineImage
        {
            public bool cull;
            public Image image;
            public InLineImage(Image image)
            {
                this.image = image;
                this.cull = false;
            }
        }

        private static readonly Regex s_Regex = new Regex(@"<image src=(\S+?)(?: width=(\d*\.?\d+))?(?: height=(\d*\.?\d+))?/>", RegexOptions.Singleline);
        /// <summary>
        /// 可见度
        /// </summary>
        [SerializeField]
        private bool m_Visible = true;
        public bool visible
        {
            get { return m_Visible; }
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
        /// 外部图片加载目录
        /// </summary>
        [SerializeField]
        public string imagePathRoot = "";

        public enum TextEffectType
        {
            NONE,
            SHADOW,
            OUTLINE4,
            OUTLINE8,
            MATERIAL
        }

        /// <summary>
        /// 文字描边类型
        /// </summary>
        [SerializeField]
        private TextEffectType m_EffectType = TextEffectType.NONE;
        public TextEffectType effectType
        {
            get { return m_EffectType; }
            set
            {
                if (m_EffectType != value)
                {
                    m_EffectType = value;
                    SetVerticesDirty();
                }
            }
        }

        /// <summary>
        /// 文字描边颜色
        /// </summary>
        [SerializeField]
        private Color32 m_EffectColor = new Color(0, 0, 0, 128);
        public Color32 effectColor
        {
            get { return m_EffectColor; }
            set
            {
                if (!Equals(m_EffectColor, value))
                {
                    m_EffectColor = value;
                    SetVerticesDirty();
                }
            }
        }

        /// <summary>
        /// 文字描边宽度
        /// </summary>
        [SerializeField]
        private Vector2 m_EffectDistance = new Vector2(1f, -1f);
        public Vector2 effectDistance
        {
            get { return m_EffectDistance; }
            set
            {
                if (m_EffectDistance != value)
                {
                    m_EffectDistance = value;
                    SetVerticesDirty();
                }
            }
        }

        /// <summary>
        /// 是否混合描边透明度（取消可增加性能）
        /// </summary>
        [SerializeField]
        private bool m_UseGraphicAlpha = false;
        public bool useGraphicAlpha
        {
            get { return m_UseGraphicAlpha; }
            set
            {
                if (m_UseGraphicAlpha != value)
                {
                    m_UseGraphicAlpha = value;
                    SetVerticesDirty();
                }
            }
        }
        [SerializeField]
        private bool m_EnabledGradient = false;
        public bool enabledGradient
        {
            get { return m_EnabledGradient; }
            set
            {
                if (m_EnabledGradient != value)
                {
                    m_EnabledGradient = value;
                    SetVerticesDirty();
                }
            }
        }
        [SerializeField]
        private Color m_GradientColor = Color.white;
        public Color gradientColor
        {
            get { return m_GradientColor; }
            set
            {
                if (m_GradientColor != value)
                {
                    m_GradientColor = value;
                    SetVerticesDirty();
                }
            }
        }

        /// <summary>
        /// 单字位移
        /// </summary>
        [SerializeField]
        public List<CharOffest> charOffests;

        /// <summary>
        /// 设置单字位移
        /// </summary>
        public void SetCharOffest(int index, Vector3 position)
        {
            SeekToCharOffestIndex(index);
            this.charOffests[index].position = position;
            SetVerticesDirty();
        }

        public void SetCharOffest(int index, Vector3 position, float rotation)
        {
            SetCharOffest(index, position);
            this.charOffests[index].rotation = rotation;
            SetVerticesDirty();
        }

        public void SetCharOffest(int index, Vector3 position, float rotation, Vector3 scale)
        {
            SetCharOffest(index, position, rotation);
            this.charOffests[index].scale = scale;
            SetVerticesDirty();
        }

        private void SeekToCharOffestIndex(int index)
        {
            for (int i = charOffests.Count; i <= index; i++)
                this.charOffests.Add(null);

            if (this.charOffests[index] == null)
                this.charOffests[index] = new CharOffest();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (inlineImages != null)
            {
                int count = inlineImages.Count;
                for (int i = 0; i < count; i++)
                {
                    if (inlineImages[i].image != null)
                        inlineImages[i].image.enabled = true;
                }
            }
            UpdateVisible();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (inlineImages != null)
            {
                int count = inlineImages.Count;
                for (int i = 0; i < count; i++)
                {
                    if (inlineImages[i].image != null)
                        inlineImages[i].image.enabled = false;
                }
            }
        }

        protected virtual void UpdateVisible()
        {
            this.canvasRenderer.cull = !m_Visible;
            if (inlineImages != null)
            {
                int count = inlineImages.Count;
                for (int i = 0; i < count; i++)
                {
                    InLineImage item = inlineImages[i];
                    if (item.image != null)
                    {
                        bool cull = !m_Visible || item.cull;
                        if (cull != item.image.canvasRenderer.cull)
                        {
                            item.image.canvasRenderer.cull = cull;
                            if (!cull)
                            {
                                CanvasUpdateRegistry.RegisterCanvasElementForGraphicRebuild(item.image);//不加这个在隐藏状态时有变化不会更新
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 清除不使用的图片，释放内存
        /// </summary>
        public void ClearUnUsedInLineImage()
        {
            if (inlineImages != null)
            {
                int needCount = inlineCharIndex.Count;
                int count = inlineImages.Count;
                for (int i = count - 1; i > 0; i--)
                {
                    InLineImage item = inlineImages[i];
                    if (item.image != null && i >= needCount)
                    {
                        if (Application.isPlaying)
                            GameObject.Destroy(item.image.gameObject);
#if UNITY_EDITOR
                        else
                            GameObject.DestroyImmediate(item.image.gameObject);
#endif
                        item.image = null;
                    }
                    if (item.image == null)
                    {
                        inlineImages.RemoveAt(i);
                    }
                }
            }
            SetVerticesDirty();
            m_FilterText = FilterRichText(m_Text);
        }

        private string m_OldNoFilterText;
        private string m_FilterText;
        private bool m_OldSupportRichText;
        public override void SetVerticesDirty()
        {
            base.SetVerticesDirty();
            if (m_OldNoFilterText != m_Text || m_OldSupportRichText != supportRichText)
            {
                m_OldNoFilterText = m_Text;
                m_OldSupportRichText = supportRichText;
                m_FilterText = FilterRichText(m_Text);
            }
        }

        [SerializeField] private List<InLineImage> inlineImages = new List<InLineImage>();
        private List<int> inlineCharIndex = new List<int>();
        private void SetInLineImageCull(int index, bool cull)
        {
            InLineImage item = inlineImages[index];
            if (item.image == null)
                return;

            item.cull = cull;
            if (cull != item.image.canvasRenderer.cull)
            {
                item.image.canvasRenderer.cull = cull;
                if (!cull)
                {
                    item.image.Rebuild(CanvasUpdate.PreRender);//不加这个在隐藏状态时有变化不会更新
                }
            }
        }

        private string FilterRichText(string text)
        {
            inlineCharIndex.Clear();
            int i = 0;
            if (supportRichText)
            {
                Match match;
                do
                {
                    match = s_Regex.Match(text);
                    if (match.Success)
                    {
                        inlineCharIndex.Add(match.Index);

                        string src = match.Groups[1].Value;
                        float width = string.IsNullOrEmpty(match.Groups[2].Value) ? float.NaN : float.Parse(match.Groups[2].Value);
                        float height = string.IsNullOrEmpty(match.Groups[3].Value) ? float.NaN : float.Parse(match.Groups[3].Value);
                        string newText = "";
                        Image img = null;
                        if (i >= inlineImages.Count || inlineImages[i].image == null)
                        {
                            img = new GameObject("InlineImage" + (i + 1).ToString()).AddComponent<Image>();
                            img.transform.SetParent(this.transform, false);
                            img.raycastTarget = false;
                            if (i < inlineImages.Count)
                            {
                                inlineImages[i].image = img;
                                inlineImages[i].cull = false;
                            }
                            else
                                inlineImages.Add(new InLineImage(img));
                        }
                        else
                        {
                            img = inlineImages[i].image;
                        }

                        LoadSprite(img, match.Groups[1].Value);
                        Sprite spr = img.sprite;
                        if (spr != null)
                        {
                            if (float.IsNaN(height) && float.IsNaN(width))
                            {
                                height = fontSize;
                                width = height / spr.rect.height * spr.rect.width;
                            }
                            else if (float.IsNaN(width))
                            {
                                width = height / spr.rect.height * spr.rect.width;
                            }
                            else if (float.IsNaN(height))
                            {
                                height = width / spr.rect.width * spr.rect.height;
                            }
                            img.rectTransform.sizeDelta = new Vector2(width, height);
                            newText = "<quad size=" + height.ToString() +
                            " width=" + width.ToString() +
                            " height=" + height.ToString() + "/>";
                        }
                        text = text.Substring(0, match.Index) + newText + text.Substring(match.Index + match.Length);
                        i++;
                    }
                } while (match.Success);
            }
            int c = inlineImages.Count - 1;
            while (i <= c)
            {
                if (inlineImages[c].image != null)
                {
                    inlineImages[c].image.sprite = null;
                    SetInLineImageCull(c, true);
                }
#if UNITY_EDITOR
                else
                {
                    inlineImages.RemoveAt(c);
                }
#endif
                c--;
            }
            return text;
        }

        readonly UIVertex[] m_TempVerts = new UIVertex[4];
        readonly UIVertex[] m_TempEffectVerts = new UIVertex[4];
        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            if (font == null)
                return;

            // We don't care if we the font Texture changes while we are doing our Update.
            // The end result of cachedTextGenerator will be valid for this instance.
            // Otherwise we can get issues like Case 619238.
            m_DisableFontTextureRebuiltCallback = true;

            Vector2 extents = rectTransform.rect.size;

            var settings = GetGenerationSettings(extents);
            cachedTextGenerator.PopulateWithErrors(m_FilterText, settings, gameObject);//这里有缓存，不需要处理
            OnPopulateVertsPosition(toFill);

            m_DisableFontTextureRebuiltCallback = false;
        }

        protected virtual void OnPopulateVertsPosition(VertexHelper toFill)
        {
            // Apply the offset to the vertices
            IList<UIVertex> verts = cachedTextGenerator.verts;
            float unitsPerPixel = 1 / pixelsPerUnit;
            //Last 4 verts are always a new line... (\n)
            int vertCount = verts.Count - 4;

            Vector2 roundingOffset = new Vector2(verts[0].position.x, verts[0].position.y) * unitsPerPixel;
            roundingOffset = PixelAdjustPoint(roundingOffset) - roundingOffset;
            toFill.Clear();
            bool needOffest = roundingOffset != Vector2.zero;

            //处理图文混排的图片
            int count = this.inlineCharIndex.Count;
            for (int i = 0; i < count; i++)
            {
                int index = inlineCharIndex[i];
                if (index * 4 + 3 < vertCount)
                {
                    if (i < inlineImages.Count && inlineImages[i].image != null)
                    {
                        Vector2 topLeft = verts[index * 4 + 1].position;
                        Vector2 bottomRight = verts[index * 4 + 3].position;
                        Vector2 center = new Vector2((topLeft.x + bottomRight.x) * 0.5f, topLeft.y + (bottomRight.y - topLeft.y) * 0.6f);
                        topLeft.y += (bottomRight.y - topLeft.y) * 0.5f;
                        var image = inlineImages[i].image;
                        image.transform.localPosition = center * unitsPerPixel;
                        SetInLineImageCull(i, false);
                        UIVertex newVertex = UIVertex.simpleVert;
                        newVertex.position = center;
                        verts[index * 4] = newVertex;
                        verts[index * 4 + 1] = newVertex;
                        verts[index * 4 + 2] = newVertex;
                        verts[index * 4 + 3] = newVertex;
                    }
                }
                else
                {
                    if (i < inlineImages.Count)
                    {
                        SetInLineImageCull(i, true);
                    }
                }
            }

            int charIndex = 0;
            bool hasCharOffests = charOffests != null && charOffests.Count > 0;

            float bottomY = float.MaxValue;
            float topY = float.MinValue;
            if (m_EnabledGradient)
            {
                for (int i = 0; i < vertCount; i += 2)
                {
                    float y = verts[i].position.y;
                    if (y > topY)
                    {
                        topY = y;
                    }
                    else if (y < bottomY)
                    {
                        bottomY = y;
                    }
                }
            }

            for (int i = 0; i < vertCount; i += 4)
            {
                if (verts[i].position == verts[i + 1].position)
                    continue;

                for (int j = 0; j < 4; ++j)
                {
                    m_TempVerts[j] = verts[i + j];
                    m_TempVerts[j].position *= unitsPerPixel;
                    if (needOffest)
                    {
                        m_TempVerts[j].position.x += roundingOffset.x;
                        m_TempVerts[j].position.y += roundingOffset.y;
                    }
                }

                //文字位移
                float cosRotate = 0f;
                float sinRotate = 0f;
                if (hasCharOffests && charIndex < charOffests.Count)
                {
                    CharOffest charOffest = charOffests[charIndex];
                    bool needScale = charOffest.scale != Vector2.one;
                    if (charOffest.rotation != 0f)
                    {
                        cosRotate = Mathf.Cos(charOffest.rotation);
                        sinRotate = Mathf.Sin(charOffest.rotation);
                    }
                    if (charOffest != null)
                    {
                        Vector2 center = (m_TempVerts[0].position + m_TempVerts[2].position) / 2f;
                        for (int j = 0; j < 4; ++j)
                        {
                            if (needScale)
                            {
                                m_TempVerts[j].position.x = center.x + (m_TempVerts[j].position.x - center.x) * charOffest.scale.x;
                                m_TempVerts[j].position.y = center.y + (m_TempVerts[j].position.y - center.y) * charOffest.scale.y;
                            }
                            if (charOffest.rotation != 0f)
                            {
                                float dx = m_TempVerts[j].position.x - center.x;
                                float dy = m_TempVerts[j].position.y - center.y;
                                m_TempVerts[j].position.x = center.x + dx * cosRotate - dy * sinRotate;
                                m_TempVerts[j].position.y = center.y + dx * sinRotate + dy * cosRotate;
                            }
                            m_TempVerts[j].position.x += charOffest.position.x;
                            m_TempVerts[j].position.y += charOffest.position.y;
                        }
                    }
                }

                //渐变
                if (m_EnabledGradient)
                {
                    ApplyGradientColor(m_TempVerts, color, m_GradientColor, topY, bottomY);
                }

                //阴影与描边
                if (m_EffectType != TextEffectType.NONE)
                {
                    if (m_EffectType == TextEffectType.MATERIAL)
                    {
                        Vector2 bottomLeft = m_TempVerts[0].uv0;
                        Vector2 topRight = m_TempVerts[2].uv0;
                        if (bottomLeft.x > topRight.x)
                        {
                            bottomLeft = m_TempVerts[2].uv0;
                            topRight = m_TempVerts[0].uv0;
                        }
                        Vector4 uvBounds = new Vector4(bottomLeft.x, bottomLeft.y, topRight.x, topRight.y);
                        m_TempVerts[0].tangent = uvBounds;
                        m_TempVerts[1].tangent = uvBounds;
                        m_TempVerts[2].tangent = uvBounds;
                        m_TempVerts[3].tangent = uvBounds;
                    }
                    else
                    {
                        m_TempVerts.CopyTo(m_TempEffectVerts, 0);
                        ApplyColor(m_TempEffectVerts, m_EffectColor);
                        ApplyOffestX(m_TempEffectVerts, m_EffectDistance.x);
                        ApplyOffestY(m_TempEffectVerts, m_EffectDistance.y);
                        toFill.AddUIVertexQuad(m_TempEffectVerts);
                        if (m_EffectType != TextEffectType.SHADOW)
                        {
                            ApplyOffestY(m_TempEffectVerts, -m_EffectDistance.y - m_EffectDistance.y);
                            toFill.AddUIVertexQuad(m_TempEffectVerts);
                            ApplyOffestX(m_TempEffectVerts, -m_EffectDistance.x - m_EffectDistance.x);
                            toFill.AddUIVertexQuad(m_TempEffectVerts);
                            ApplyOffestY(m_TempEffectVerts, m_EffectDistance.y + m_EffectDistance.y);
                            toFill.AddUIVertexQuad(m_TempEffectVerts);
                            if (m_EffectType != TextEffectType.OUTLINE4)
                            {
                                const float sqrt2 = 1.414214f;
                                ApplyOffestX(m_TempEffectVerts, m_EffectDistance.x);
                                ApplyOffestY(m_TempEffectVerts, (sqrt2 - 1) * m_EffectDistance.y);
                                toFill.AddUIVertexQuad(m_TempEffectVerts);
                                ApplyOffestY(m_TempEffectVerts, -sqrt2 * 2 * m_EffectDistance.y);
                                toFill.AddUIVertexQuad(m_TempEffectVerts);
                                ApplyOffestY(m_TempEffectVerts, sqrt2 * m_EffectDistance.y);
                                ApplyOffestX(m_TempEffectVerts, sqrt2 * m_EffectDistance.x);
                                toFill.AddUIVertexQuad(m_TempEffectVerts);
                                ApplyOffestX(m_TempEffectVerts, -sqrt2 * 2 * m_EffectDistance.x);
                                toFill.AddUIVertexQuad(m_TempEffectVerts);
                            }
                        }
                    }
                };
                toFill.AddUIVertexQuad(m_TempVerts);
                charIndex++;
            };
        }

        void ApplyColor(UIVertex[] vertexs, Color32 effectColor)
        {
            if (m_UseGraphicAlpha)
                effectColor.a = (byte)(effectColor.a * vertexs[0].color.a / 255);
            vertexs[0].color = effectColor;
            vertexs[1].color = effectColor;
            vertexs[2].color = effectColor;
            vertexs[3].color = effectColor;
        }

        void ApplyGradientColor(UIVertex[] vertexs, Color32 topColor, Color32 bottomColor, float topY, float bottomY)
        {
            if (m_UseGraphicAlpha)
            {
                topColor.a = (byte)(topColor.a * vertexs[0].color.a / 255);
                bottomColor.a = (byte)(bottomColor.a * vertexs[0].color.a / 255);
            }
            float uiElementHeight = topY - bottomY;
            vertexs[0].color = vertexs[1].color = Color32.Lerp(bottomColor, topColor, (vertexs[0].position.y - bottomY) / uiElementHeight);
            vertexs[2].color = vertexs[3].color = Color32.Lerp(bottomColor, topColor, (vertexs[2].position.y - bottomY) / uiElementHeight);
        }

        void ApplyOffestX(UIVertex[] vertexs, float v)
        {
            vertexs[0].position.x += v;
            vertexs[1].position.x += v;
            vertexs[2].position.x += v;
            vertexs[3].position.x += v;
        }

        void ApplyOffestY(UIVertex[] vertexs, float v)
        {
            vertexs[0].position.y += v;
            vertexs[1].position.y += v;
            vertexs[2].position.y += v;
            vertexs[3].position.y += v;
        }


#if UNITY_EDITOR
        private readonly static Vector3[] fourCorners = new Vector3[4];
        private void OnDrawGizmosSelected()
        {
            if (this.raycastTarget)
            {
                Gizmos.color = Color.green;
                rectTransform.GetWorldCorners(fourCorners);
                for (int i = 0; i < 4; i++)
                {
                    Gizmos.DrawLine(fourCorners[i], fourCorners[(i + 1) % 4]);
                }
            }
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            UpdateVisible();
           
            
        }
#endif
    }
}
