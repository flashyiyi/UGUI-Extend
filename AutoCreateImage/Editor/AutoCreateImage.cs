using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace UGUIExtend
{
    [InitializeOnLoad]
    public class AutoCreateImage
    {
        static AutoCreateImage()
        {
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemCallback;
            SceneView.onSceneGUIDelegate += HierarchyWindowItemCallback;
        }

        static void HierarchyWindowItemCallback(SceneView scene)
        {
            if (Event.current.type != EventType.DragPerform)
                return;

            Vector2 mousePos = Event.current.mousePosition;
            mousePos.y = scene.camera.pixelHeight - mousePos.y;
            HierarchyWindowItemCallback(null, scene.camera.ScreenToWorldPoint(mousePos));
        }

        static void HierarchyWindowItemCallback(int pID, Rect pRect)
        {
            if (Event.current.type != EventType.DragPerform)
                return;

            if (!pRect.Contains(Event.current.mousePosition))
                return;

            GameObject targetGo = EditorUtility.InstanceIDToObject(pID) as GameObject;
            if (targetGo == null || targetGo.GetComponentInParent<Canvas>() == null)
                return;

            HierarchyWindowItemCallback(targetGo.transform, Vector2.zero);
        }

        static void HierarchyWindowItemCallback(Transform target, Vector2 position)
        {
            List<Sprite> sprites = new List<Sprite>();
            foreach (Object obj in DragAndDrop.objectReferences)
            {
                string path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path))
                {
                    if (obj is Sprite)
                    {
                        sprites.Add(obj as Sprite);
                    }
                    else if (obj is Texture2D)
                    {
                        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                        if (sprite != null)
                            sprites.Add(sprite);
                    }
                }
            }

            if (sprites.Count == 0)
                return;

            Canvas canvas = GameObject.FindObjectOfType<Canvas>();
            if (canvas != null)
                canvas = canvas.rootCanvas;

            List<GameObject> selecteds = new List<GameObject>();
            foreach (Sprite sprite in sprites)
            {
                var gameObject = new GameObject(sprite.name);
                if (target != null)
                {
                    gameObject.transform.SetParent(target, false);
                }
                else if (canvas != null)
                {
                    gameObject.transform.SetParent(canvas.transform, false);
                    gameObject.transform.position = position;
                }
                var image = gameObject.AddComponent<UnityEngine.UI.Image>();
                image.sprite = sprite;
                image.SetNativeSize();
                selecteds.Add(gameObject);
            }
            Selection.objects = selecteds.ToArray();
            Event.current.Use();
        }
    }
}
