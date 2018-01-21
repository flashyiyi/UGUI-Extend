using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
#endif

namespace UGUIExtend
{
    /// <summary>
    /// 加载Prefab
    /// </summary>
    [ExecuteInEditMode]
    public class PrefabLoader : MonoBehaviour
    {
#if UNITY_EDITOR
        static Transform previewContainer;
        static void FindPreviewContainer()
        {
            if (previewContainer != null)
                return;

            Canvas canvas = GameObject.FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                Transform root = canvas.rootCanvas.transform;
                previewContainer = root.Find("_PrefabPreview");
                if (previewContainer == null)
                {
                    previewContainer = new GameObject("_PrefabPreview").transform;
                    previewContainer.SetParent(root.transform, false);
                }
            }
        }

        [PostProcessSceneAttribute(2)]
        public static void OnPostprocessScene()
        {
            Canvas canvas = GameObject.FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                Transform root = canvas.rootCanvas.transform;
                Transform previewContainer = root.Find("_PrefabPreview");
                if (previewContainer != null)
                {
                    GameObject.Destroy(previewContainer.gameObject);
                }
            }
        }
#endif

        // Prefab
        [SerializeField]
        public GameObject source;

        // Awake时自动加载
        [SerializeField]
        public bool autoLoad = true;

        // 是将预览预制的坐标复制到瞄点，还是相反。这是为了方便拖动。
        [SerializeField]
        public bool copyPositionFromPreview = false;

        Transform mChild;

        void Awake()
        {
            if (Application.isPlaying && autoLoad)
            {
                Load();
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (mChild == null)
                {
                    FindPreviewContainer();
                    if (previewContainer != null)
                    {
                        mChild = previewContainer.Find(source.name + GetInstanceID().ToString());
                    }
                }
            }
#endif
        }

        public void Load()
        {
            if (source != null)
            {
                if (mChild != null)
                {
                    GameObject.Destroy(mChild.gameObject);
                }
                mChild = GameObject.Instantiate(source).transform;
                mChild.name = source.name;
                mChild.SetParent(this.transform, false);
                mChild.localPosition = Vector3.zero;
            }
        }

#if UNITY_EDITOR
        void Update()
        {
            if (!Application.isPlaying)
            {
                if (source != null)
                {
                    if (mChild == null || PrefabUtility.GetPrefabParent(mChild.gameObject) != source)
                    {
                        FindPreviewContainer();
                        if (previewContainer != null)
                        {
                            if (mChild != null)
                            {
                                GameObject.DestroyImmediate(mChild.gameObject);
                                mChild = null;
                            }
                            GameObject prefab = PrefabUtility.InstantiatePrefab(source) as GameObject;
                            if (prefab != null)
                            {
                                prefab.name = source.name + GetInstanceID().ToString();
                                mChild = prefab.transform;
                                mChild.SetParent(previewContainer, false);
                                mChild.position = this.transform.position;
                            }
                        }
                    }
                    else
                    {
                        if (copyPositionFromPreview)
                            this.transform.position = mChild.position;
                        else
                            mChild.position = this.transform.position;
                    }
                }
                else
                {
                    if (mChild != null)
                    {
                        GameObject.DestroyImmediate(mChild.gameObject);
                        mChild = null;
                    }
                }
                if (previewContainer != null)
                {
                    if (previewContainer.GetSiblingIndex() < previewContainer.parent.childCount - 1)
                    {
                        previewContainer.SetAsLastSibling();
                    }
                }
            }
        }
#endif

    }
}