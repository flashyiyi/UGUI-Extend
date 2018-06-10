using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
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

        [MenuItem("Tools/Apply All Canvas Prefab")]
        public static void ApplyAllCanvasPrefab()
        {
            Canvas[] canvases = GameObject.FindObjectsOfType<Canvas>();
            foreach (Canvas canvas in canvases)
            {
                ApplyAllPrefab(canvas.transform);
            }
        }

        static void ApplyAllPrefab(Transform root)
        {
            int count = root.childCount;
            for (int i = 0; i < count; i++)
            {
                Transform trans = root.GetChild(i);
                ApplyAllPrefab(trans);
                PrefabLoader loader = trans.GetComponent<PrefabLoader>();
                if (loader != null && loader.mChild != null)
                {
                    GameObject loaderGo = loader.mChild.gameObject;
                    Object childPrefab = PrefabUtility.GetPrefabParent(loaderGo);
                    if (childPrefab != null)
                    {
                        PrefabUtility.ReplacePrefab(loaderGo, childPrefab, ReplacePrefabOptions.ConnectToPrefab);
                    }
                }
            }
        }

        [InitializeOnLoadMethod]
        static void StartInitializeOnLoadMethod()
        {
            PrefabUtility.prefabInstanceUpdated += RemoveAllTempPrefab;
            SceneView.onSceneGUIDelegate += OnSceneGUI;
        }

        static bool isDraging;
        static void OnSceneGUI(SceneView sceneview)
        {
            Event e = Event.current;
            if (!isDraging && e.type == EventType.MouseDrag)
            {
                isDraging = true;
            }
            if (isDraging && e.type == EventType.MouseUp)
            {
                isDraging = false;
            }
        }

        static void FindPrefabLoader(Transform root, ref List<PrefabLoader> loaders)
        {
            int count = root.childCount;
            for (int i = 0; i < count; i++)
            {
                Transform trans = root.GetChild(i);
                PrefabLoader loader = trans.GetComponent<PrefabLoader>();
                if (loader != null)
                {
                    if (loader.transform.childCount > 0)
                        loaders.Add(loader);
                }
                else
                {
                    FindPrefabLoader(trans, ref loaders);
                }
            }
        }

        static void RemoveAllTempPrefab(GameObject go)
        {
            List<PrefabLoader> loaders = new List<PrefabLoader>();
            FindPrefabLoader(go.transform, ref loaders);

            if (loaders.Count > 0)
            {
                //foreach (PrefabLoader loader in loaders)
                //{
                //    GameObject loaderGo = loader.mChild != null ? loader.mChild.gameObject : null;
                //    if (loaderGo != null && PrefabUtility.GetPrefabType(loaderGo) == PrefabType.PrefabInstance)
                //    {
                //        Object childPrefab = PrefabUtility.GetPrefabParent(go);
                //        PrefabUtility.ReplacePrefab(loaderGo, childPrefab, ReplacePrefabOptions.ConnectToPrefab);
                //    }
                //}
                foreach (PrefabLoader loader in loaders)
                {
                    if (loader != null)
                    {
                        RemoveAllChildren(loader.transform);
                    }
                }
                loaders = null;

                Object prefab = PrefabUtility.GetPrefabParent(go);
                PrefabUtility.ReplacePrefab(go, prefab, ReplacePrefabOptions.ConnectToPrefab);

                EditorApplication.CallbackFunction oldCallBack = EditorApplication.delayCall;
                EditorApplication.delayCall = () =>
                {
                    Selection.activeGameObject = go;
                    if (oldCallBack != null)
                        oldCallBack.Invoke();
                };
            }
        }

        void Update()
        {
            if (!Application.isPlaying)
            {
                if (source != null)
                {
                    Object selfPrefab = PrefabUtility.GetPrefabParent(PrefabUtility.FindPrefabRoot(gameObject));
                    if (selfPrefab == source)
                    {
                        source = null;
                        return;
                    }

                    if (mChild == null || PrefabUtility.GetPrefabParent(mChild.gameObject) != source)
                    {
                        RemoveAllChildren(transform);
                        mChild = null;
                        GameObject prefab = PrefabUtility.InstantiatePrefab(source) as GameObject;
                        if (prefab != null)
                        {
                            mChild = prefab.transform;
                            mChild.SetParent(transform, false);
                            mChild.localPosition = Vector3.zero;
                        }
                    }

                    if (!isDraging && mChild != null)
                    {
                        transform.position = mChild.position;
                        mChild.localPosition = Vector3.zero;
                    }
                }
                else
                {
                    if (mChild != null)
                    {
                        RemoveAllChildren(transform);
                        mChild = null;
                    }
                }
            }
        }
#endif

        public static void RemoveAllChildren(Transform trans)
        {
            int count = trans.childCount;
            for (int i = count - 1; i >= 0; i--)
            {
                if (Application.isPlaying)
                    GameObject.Destroy(trans.GetChild(i).gameObject);
                else
                    GameObject.DestroyImmediate(trans.GetChild(i).gameObject);
            }
        }

        // Prefab
        [SerializeField]
        public GameObject source;

        // Awake时自动加载
        [SerializeField]
        public bool autoLoad = true;

        Transform mChild;

        void Awake()
        {
            if (Application.isPlaying && autoLoad)
            {
                Load();
            }
        }

        public void Load()
        {
            if (source != null)
            {
                RemoveAllChildren(transform);

                mChild = GameObject.Instantiate(source).transform;
                mChild.name = source.name;
                mChild.SetParent(transform, false);
                mChild.localPosition = Vector3.zero;
            }
        }
    }


    //public class RespectReadOnly : UnityEditor.AssetModificationProcessor
    //{
    //    public static string[] OnWillSaveAssets(string[] paths)
    //    {
    //        PrefabLoader.ApplyAllCanvasPrefab();
    //        return paths;
    //    }
    //}
}