using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.AnimatedValues;

namespace UGUIExtend
{
    [CustomEditor(typeof(AdvancedImage), false)]
    [CanEditMultipleObjects]
    
    public class AdvancedImageEditor : UnityEditor.UI.ImageEditor
    {
        [MenuItem("GameObject/UI/Advanced Image", false, 3)]
        static void CreatePanel()
        {
            GameObject spriteObject = new GameObject("Sprite");
            if (Selection.activeGameObject != null)
            {
                spriteObject.transform.parent = Selection.activeGameObject.transform;
                spriteObject.layer = Selection.activeGameObject.layer;
            }
            else
            {
                Canvas mainCanvas = GameObject.FindObjectOfType<Canvas>();
                if (mainCanvas != null)
                {
                    spriteObject.transform.parent = mainCanvas.transform;
                    spriteObject.layer = mainCanvas.gameObject.layer;
                }
            }
            spriteObject.AddComponent<AdvancedImage>();
            Selection.objects = new Object[] { spriteObject };
        }

        SerializedProperty m_Type;
        SerializedProperty m_UseSpriteMesh;
        SerializedProperty m_EnabledPopulateMesh;
        SerializedProperty m_Visible;
        SerializedProperty m_HorizontalMirror;
        SerializedProperty m_VerticalMirror;
        SerializedProperty m_FillCenter;
        SerializedProperty m_FillBorders;
        SerializedProperty m_HitArea;
        SerializedProperty m_UseSpriteHitArea;
        SerializedProperty m_HitScale;
        AnimBool m_ShowFillBorders;
        AnimBool m_ShowFillMirror;
        protected override void OnEnable()
        {
            base.OnEnable();
            
            m_Type = serializedObject.FindProperty("m_Type");

            m_UseSpriteMesh = serializedObject.FindProperty("m_UseSpriteMesh");
            m_EnabledPopulateMesh = serializedObject.FindProperty("m_EnabledPopulateMesh");
            m_Visible = serializedObject.FindProperty("m_Visible");
            m_HorizontalMirror = serializedObject.FindProperty("m_HorizontalMirror");
            m_VerticalMirror = serializedObject.FindProperty("m_VerticalMirror");
            m_FillCenter = serializedObject.FindProperty("m_FillCenter");
            m_FillBorders = serializedObject.FindProperty("m_FillBorders");
            m_HitArea = serializedObject.FindProperty("hitArea");
            m_UseSpriteHitArea = serializedObject.FindProperty("useSpriteHitArea");
            m_HitScale = serializedObject.FindProperty("hitScale");


            m_ShowFillBorders = new AnimBool(!m_FillCenter.hasMultipleDifferentValues && m_FillCenter.boolValue == false);
            m_ShowFillBorders.valueChanged.AddListener(Repaint);
            m_ShowFillMirror = new AnimBool(!m_FillCenter.hasMultipleDifferentValues && m_FillCenter.boolValue == false);
            m_ShowFillMirror.valueChanged.AddListener(Repaint);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            m_ShowFillBorders.valueChanged.AddListener(Repaint);
            m_ShowFillMirror.valueChanged.AddListener(Repaint);
        }
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            Image.Type typeEnum = (Image.Type)m_Type.enumValueIndex;

            if (!m_Type.hasMultipleDifferentValues && typeEnum == Image.Type.Simple)
            {
                m_ShowFillBorders.target = !m_FillCenter.hasMultipleDifferentValues && m_FillCenter.boolValue == false;
                m_ShowFillMirror.target = !m_UseSpriteMesh.hasMultipleDifferentValues && m_UseSpriteMesh.boolValue == false;
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(m_UseSpriteMesh);
                if (EditorGUILayout.BeginFadeGroup(m_ShowFillMirror.faded))
                {
                    EditorGUILayout.PropertyField(m_FillCenter);
                    if (m_ShowFillBorders.target)
                    {
                        ++EditorGUI.indentLevel;
                        m_FillBorders.vector4Value = EditorGUILayout.Vector4Field("Fill Borders", m_FillBorders.vector4Value); m_FillBorders.vector4Value = new Vector4(Mathf.Clamp01(m_FillBorders.vector4Value.x),
                        Mathf.Clamp01(m_FillBorders.vector4Value.y),
                        Mathf.Clamp01(m_FillBorders.vector4Value.z),
                        Mathf.Clamp01(m_FillBorders.vector4Value.w));
                        --EditorGUI.indentLevel;
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(m_HorizontalMirror);
                        EditorGUILayout.PropertyField(m_VerticalMirror);
                    }
                }
                EditorGUILayout.EndFadeGroup();
                --EditorGUI.indentLevel;
            }
            else if (!m_Type.hasMultipleDifferentValues && typeEnum == Image.Type.Sliced)
            {
                m_ShowFillBorders.target = !m_FillCenter.hasMultipleDifferentValues && m_FillCenter.boolValue == false;
                m_ShowFillMirror.target = !m_UseSpriteMesh.hasMultipleDifferentValues && m_UseSpriteMesh.boolValue == false;
                ++EditorGUI.indentLevel;
                if (EditorGUILayout.BeginFadeGroup(m_ShowFillBorders.faded))
                {
                    ++EditorGUI.indentLevel;
                    m_FillBorders.vector4Value = EditorGUILayout.Vector4Field("Fill Borders", m_FillBorders.vector4Value);
                    m_FillBorders.vector4Value = new Vector4(Mathf.Clamp01(m_FillBorders.vector4Value.x),
                        Mathf.Clamp01(m_FillBorders.vector4Value.y),
                        Mathf.Clamp01(m_FillBorders.vector4Value.z),
                        Mathf.Clamp01(m_FillBorders.vector4Value.w));
                    --EditorGUI.indentLevel;
                }
                EditorGUILayout.EndFadeGroup();
                EditorGUILayout.PropertyField(m_HorizontalMirror);
                EditorGUILayout.PropertyField(m_VerticalMirror);
                --EditorGUI.indentLevel;
            }
            EditorGUILayout.PropertyField(m_EnabledPopulateMesh);
            EditorGUILayout.PropertyField(m_Visible);
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(m_HitArea);
            EditorGUILayout.PropertyField(m_UseSpriteHitArea);
            EditorGUILayout.PropertyField(m_HitScale);
            m_HitScale.vector2Value = new Vector2(Mathf.Clamp01(m_HitScale.vector2Value.x), Mathf.Clamp01(m_HitScale.vector2Value.y));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
