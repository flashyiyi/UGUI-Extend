using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SubjectNerd.PsdImporter.PsdParser;

[ExecuteInEditMode]
public class UIPSDViewer : MonoBehaviour
{
    public Object asset;

    PsdDocument psd;
    RectTransform rectTransform;
    List<Sprite> sprites;

    public bool createTexture = true;
    public bool drawGimzos = true;
    public bool replaceByName = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
            rectTransform = gameObject.AddComponent<RectTransform>();
    }

    public void LoadDocument()
    {
        UnLoadDocument();
#if UNITY_EDITOR
        if (asset != null)
        {
            string path = UnityEditor.AssetDatabase.GetAssetPath(asset);
            if (path.EndsWith(".psd"))
                psd = PsdDocument.Create(path);
        }
#endif
    }

    public void CreateTextures()
    {
        DestoryTextures();
        if (psd == null)
            return;

        rectTransform.sizeDelta = new Vector2(psd.Width, psd.Height);
        sprites = new List<Sprite>();
        CreateLayers(rectTransform, psd.Childs);
    }

    public void UnLoadDocument()
    {
        if (psd != null)
        {
            psd.Dispose();
            psd = null;
        }
    }

    public void DestoryTextures()
    {
        if (sprites != null)
        {
            foreach (var sprite in sprites)
            {
                if (sprite != null)
                {
                    GameObject.DestroyImmediate(sprite.texture);
                    GameObject.DestroyImmediate(sprite);
                }
            }
            sprites = null;
        }

        if (rectTransform != null)
        {
            int count = rectTransform.childCount;
            for (int i = count - 1; i >= 0; i--)
            {
                GameObject.DestroyImmediate(rectTransform.GetChild(i).gameObject);
            }
        }
    }

    private void OnDestroy()
    {
        UnLoadDocument();
        DestoryTextures();
    }

    private void CreateLayers(RectTransform parent,IPsdLayer[] layers)
    {
        if (layers == null)
            return;

        Vector2 rootSize = rectTransform.sizeDelta;
        Vector2 rootOffest = (Vector2)(rectTransform.position) - rootSize / 2f;
        foreach (var layer in layers)
        {
            GameObject go = new GameObject(layer.Name);
            RectTransform t = go.AddComponent<RectTransform>();
            t.anchoredPosition = new Vector2(layer.Left + layer.Width / 2f, rootSize.y - (layer.Top + layer.Height / 2f)) + rootOffest;
            t.sizeDelta = new Vector2(layer.Width,layer.Height);
            t.SetParent(parent, true);

            if (layer.HasImage && createTexture)
            {
                Image image = t.gameObject.AddComponent<Image>();
                Sprite sprite = null;
#if UNITY_EDITOR
                if (replaceByName)
                {
                    string[] findedSprites = UnityEditor.AssetDatabase.FindAssets(layer.Name + " t:sprite");
                    if (findedSprites.Length > 0)
                    {
                        sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(UnityEditor.AssetDatabase.GUIDToAssetPath(findedSprites[0]));
                    }
                }
#endif
                if (sprite == null)
                {
                    Texture2D tex = GetTexture2D(layer);
                    sprite = Sprite.Create(tex, new Rect(0, 0, layer.Width, layer.Height), Vector2.zero);
                    sprites.Add(sprite);
                }
                image.sprite = sprite;
            }

            CreateLayers(t,layer.Childs);
            go.SetActive(layer is PsdLayer ? (layer as PsdLayer).IsVisible : true);
        }
    }

    public Texture2D GetTexture2D(IPsdLayer layer)
    {
        byte[] data = layer.MergeChannels();
        var channelCount = layer.Channels.Length;
        var pitch = layer.Width * layer.Channels.Length;
        var w = layer.Width;
        var h = layer.Height;

        var format = channelCount == 3 ? TextureFormat.RGB24 : TextureFormat.ARGB32;
        var tex = new Texture2D(w, h, format, false);
        var colors = new Color32[data.Length / channelCount];
        
        var k = 0;
        for (var y = h - 1; y >= 0; --y)
        {
            for (var x = 0; x < pitch; x += channelCount)
            {
                var n = x + y * pitch;
                var c = new Color32();
                if (channelCount == 5)
                {
                    c.b = data[n++];
                    c.g = data[n++];
                    c.r = data[n++];
                    n++;
                    c.a = (byte)Mathf.RoundToInt((float)(data[n++])  * layer.Opacity);
                }
                else if (channelCount == 4)
                {
                    c.b = data[n++];
                    c.g = data[n++];
                    c.r = data[n++];
                    c.a = (byte)Mathf.RoundToInt((float)data[n++] * layer.Opacity);
                }
                else
                {
                    c.b = data[n++];
                    c.g = data[n++];
                    c.r = data[n++];
                    c.a = (byte)Mathf.RoundToInt(layer.Opacity * 255f);
                }
                colors[k++] = c;
            }
        }
        tex.SetPixels32(colors);
        tex.Apply(false, true);
        return tex;
    }

    private void OnDrawGizmos()
    {
        if (!drawGimzos)
            return;
        Vector3[] vectors = new Vector3[4];
        foreach (var t in GetComponentsInChildren<RectTransform>())
        {
            t.GetWorldCorners(vectors);
            Gizmos.DrawLine(vectors[0], vectors[1]);
            Gizmos.DrawLine(vectors[1], vectors[2]);
            Gizmos.DrawLine(vectors[2], vectors[3]);
            Gizmos.DrawLine(vectors[3], vectors[0]);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!gameObject.activeInHierarchy)
            return;
        
        UnityEditor.EditorApplication.delayCall += () =>
        {
            LoadDocument();
            CreateTextures();
        };
    }
    [UnityEditor.InitializeOnLoadMethod]
    static void AutoCreateMethod()
    {
        UnityEditor.EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemCallback;
    }

    static void HierarchyWindowItemCallback(int pID, Rect pRect)
    {
        if (!pRect.Contains(Event.current.mousePosition))
            return;

        GameObject targetGo = UnityEditor.EditorUtility.InstanceIDToObject(pID) as GameObject;
        if (targetGo == null || targetGo.GetComponentInParent<Canvas>() == null)
            return;

        if (Event.current.type == EventType.DragUpdated)
        {
            foreach (string path in UnityEditor.DragAndDrop.paths)
            {
                if (!string.IsNullOrEmpty(path) && path.EndsWith(".psd"))
                {
                    UnityEditor.DragAndDrop.visualMode = UnityEditor.DragAndDropVisualMode.Link;
                    UnityEditor.DragAndDrop.AcceptDrag();
                    Event.current.Use();
                }
            }
        }
        else if (Event.current.type == EventType.DragPerform)
        {
            foreach (string path in UnityEditor.DragAndDrop.paths)
            {
                if (!string.IsNullOrEmpty(path) && path.EndsWith(".psd"))
                {
                    Object asset = UnityEditor.AssetDatabase.LoadMainAssetAtPath(path);
                    GameObject go = new GameObject(asset.name);
                    go.transform.SetParent(targetGo.transform, false);
                    go.AddComponent<UIPSDViewer>().asset = asset;
                    Event.current.Use();
                }
            }
        }
    }
    [UnityEditor.MenuItem("GameObject/UI/Create Text By Name", false)]
    public static void CreateTextByName()
    {
        GameObject[] gameObjects = UnityEditor.Selection.gameObjects;
        foreach (GameObject go in gameObjects)
        {
            Graphic g = go.GetComponent<Graphic>();
            if (g != null)
                GameObject.DestroyImmediate(g);
            Text t = go.AddComponent<Text>();
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.text = go.name;
        }
    }

#endif
}
