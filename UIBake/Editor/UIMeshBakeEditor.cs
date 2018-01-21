using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Callbacks;

namespace UGUIExtend
{
    [CustomEditor(typeof(UIBakeMeshAsset), false)]
    public class UIBakeMeshAssetEditor : Editor
    {
        [PostProcessBuildAttribute(1)]
        static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            string[] guids = AssetDatabase.FindAssets("t:GameObject", new string[] { "Assets/MeskBakePrefab" });
            foreach (string guid in guids)
            {
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
                if (go != null)
                {
                    UIBakeMeshCreater[] bakes = go.GetComponentsInChildren<UIBakeMeshCreater>(true);
                    foreach (UIBakeMeshCreater bake in bakes)
                    {
                        bake.BatchAll();
                        Debug.Log(string.Format("Combine Mesh Asset \"{0}\" in \"{1}\"", bake.name, go.name));
                    }
                }
            }
        }

        private SerializedProperty meshProperty;
        private PreviewRenderUtility m_PreviewUtility;

        protected virtual void OnEnable()
        {
            meshProperty = this.serializedObject.FindProperty("mesh");
            if (m_PreviewUtility == null)
            {
                m_PreviewUtility = new PreviewRenderUtility(true);
            }
        }

        protected virtual void OnDisable()
        {
            if (m_PreviewUtility != null)
            {
                m_PreviewUtility.Cleanup();
                m_PreviewUtility = null;
            }
        }

        public override bool HasPreviewGUI()
        {
            return true;
        }

        public override GUIContent GetPreviewTitle()
        {
            return new GUIContent("Mesh");
        }

        protected virtual Mesh GetMesh()
        {
            if (meshProperty == null)
                return null;

            return meshProperty.objectReferenceValue as Mesh;
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            Mesh mesh = GetMesh();
            if (mesh == null)
                return;

            m_PreviewUtility.BeginPreview(r, background);
            Graphics.DrawMeshNow(mesh, r.size, Quaternion.identity);
            m_PreviewUtility.EndAndDrawPreview(r);
        }
    }

    [CustomEditor(typeof(UIBakeMeshCreater), false)]
    public class UIBakeMeshCreaterEditor : UIBakeMeshAssetEditor
    {
        SerializedProperty asset;

        protected override void OnEnable()
        {
            base.OnEnable();
            asset = serializedObject.FindProperty("asset");
        }

        protected override Mesh GetMesh()
        {
            if (asset != null && asset.objectReferenceValue != null)
                return (asset.objectReferenceValue as UIBakeMeshAsset).mesh;

            return null;
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(asset);

            if (asset.objectReferenceValue == null)
            {
                if (GUILayout.Button("CreateMesh"))
                {
                    string assetPath = EditorUtility.SaveFilePanelInProject("CreateMesh", serializedObject.targetObject.name + "_Mesh", "asset", "");
                    if (assetPath != null)
                    {
                        UIBakeMeshAsset assetData = new UIBakeMeshAsset();
                        assetData.mesh = new Mesh();
                        assetData.mesh.name = "mesh";
                        AssetDatabase.CreateAsset(assetData, assetPath);
                        AssetDatabase.AddObjectToAsset(assetData.mesh, assetData);
                        asset.objectReferenceValue = assetData;
                        serializedObject.ApplyModifiedProperties();
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                }
            }
            else
            {

                if (EditorSettings.spritePackerMode != SpritePackerMode.AlwaysOn)
                {
                    EditorGUILayout.LabelField("You must set SpritePackerMode to AlwaysOn, \nand in the Play state, make the Mesh data correctly.",GUILayout.Height(30));
                    if (GUILayout.Button("Set SpritePackerMode To AlwaysOn"))
                    {
                        EditorSettings.spritePackerMode = SpritePackerMode.AlwaysOn;
                    }
                }
                else
                {
                    if (!Application.isPlaying)
                    {
                        EditorGUILayout.LabelField("You must in the Play state, \nmake the Mesh data correctly.", GUILayout.Height(30));
                        if (GUILayout.Button("Press 'Play' To Combine"))
                        {
                            EditorApplication.ExecuteMenuItem("Edit/Play");
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("Combine"))
                        {
                            (serializedObject.targetObject as UIBakeMeshCreater).BatchAll();
                            AssetDatabase.SaveAssets();
                        }
                    }
                }
            }

            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(UIBakeMesh), false)]
    public class UIBakeMeshEditor : UIBakeMeshAssetEditor
    {
        SerializedProperty asset;
        SerializedProperty m_Mat;
        SerializedProperty m_RaycastTarget;

        protected override void OnEnable()
        {
            base.OnEnable();
            asset = serializedObject.FindProperty("asset");
            m_Mat = serializedObject.FindProperty("m_Material");
            m_RaycastTarget = serializedObject.FindProperty("m_RaycastTarget");
        }

        protected override Mesh GetMesh()
        {
            if (asset != null && asset.objectReferenceValue != null)
                return (asset.objectReferenceValue as UIBakeMeshAsset).mesh;

            return null;
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(asset);
            EditorGUILayout.PropertyField(m_Mat);
            EditorGUILayout.PropertyField(m_RaycastTarget);

            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        }
    }
}
