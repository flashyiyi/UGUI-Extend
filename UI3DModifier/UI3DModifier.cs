using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace UGUIExtend
{
    public class UI3DModifier : MonoBehaviour
    {
        static readonly int propertyNameHash = Shader.PropertyToID("_TransformModifier");
        static Shader m_defaultModifierShader;
        static Shader defaultModifierShader
        {
            get
            {
                if (m_defaultModifierShader == null)
                    m_defaultModifierShader = Shader.Find("UI/3DModifier");
                return m_defaultModifierShader;
            }
        }

        public Vector3 position = new Vector3(0, 0, 0);
        public Vector3 rotation = new Vector3(0, 0, 0);
        public bool containMaskComponent = false;

        List<Graphic> graphics = new List<Graphic>();

        Material modifierMat;
        HashSet<Material> modifierMats = new HashSet<Material>();
        Dictionary<Material, Material> customMatDict = new Dictionary<Material, Material>();


        void OnEnable()
        {
            modifierMat = new Material(defaultModifierShader);
            RefreshGraphics();
        }

        static Dictionary<Material, Material> customMatInvDict = new Dictionary<Material, Material>();
        void OnDisable()
        {
            foreach (var pair in customMatDict)
            {
                customMatInvDict.Add(pair.Value, pair.Key);
            }

            foreach (Graphic g in graphics)
            {
                Material curMat = g.material;
                if (curMat == modifierMat)
                {
                    g.material = null;
                }
                else
                {
                    Material m;
                    customMatInvDict.TryGetValue(curMat, out m);
                    if (m != null)
                    {
                        g.material = m;
                    }
                }
            }
            modifierMat = null;
            modifierMats.Clear();
            customMatDict.Clear();
            customMatInvDict.Clear();
        }

        void FindGraphics(Transform mTrans)
        {
            Graphic g = mTrans.GetComponent<Graphic>();
            if (g != null)
            {
                graphics.Add(g);
            }
            int count = mTrans.childCount;
            for (int i = 0;i < count;i++)
            {
                Transform t = mTrans.GetChild(i);
                if (t.GetComponent<UI3DModifier>() != null)
                    continue;
                
                FindGraphics(t);
            }
        }

        public void RefreshGraphics()
        {
            graphics.Clear();
            FindGraphics(transform);

            modifierMats.Clear();
            modifierMats.Add(modifierMat);

            foreach (Graphic g in graphics)
            {
                Material curMat = g.material;
                if (curMat == g.defaultMaterial)
                {
                    g.material = modifierMat;
                }
                else
                {
                    Material m;
                    customMatDict.TryGetValue(curMat, out m);
                    if (m == null)
                    {
                        m = GameObject.Instantiate(curMat);
                        customMatDict.Add(curMat, m);
                    }
                    g.material = m;
                }

                modifierMats.Add(containMaskComponent ? g.materialForRendering : g.material);
            }
        }

        public void Refresh()
        {
            Matrix4x4 matrix = Matrix4x4.TRS(position, Quaternion.Euler(rotation), Vector3.one);
            foreach (var m in modifierMats)
            {
                if (m != null)
                    m.SetMatrix(propertyNameHash, matrix);
            }
        }

        void Update()
        {
            Refresh();
        }
    }
}


