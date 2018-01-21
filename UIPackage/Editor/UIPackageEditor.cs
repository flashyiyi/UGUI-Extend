using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEditor;

namespace UGUIExtend
{
    [CustomEditor(typeof(UIPackage), false)]
    public class UIPackageEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var objects = (target as UIPackage).objects;
            if (objects != null && objects.Count > 0)
            {
                foreach (var pair in objects)
                {
                    if (pair.Value is Component)
                    {
                        DrawCompent(pair.Value as Component);
                    }
                    else
                    {
                        Dictionary<Type, object> typeObj = pair.Value as Dictionary<Type, object>;
                        foreach (var pair2 in typeObj)
                        {
                            object obj2 = pair2.Value;
                            if (obj2 is Component)
                            {
                                DrawCompent(obj2 as Component);
                            }
                            else
                            {
                                foreach (var item in obj2 as List<Component>)
                                {
                                    DrawCompent(item);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                EditorGUILayout.LabelField("Children Empty");
            }
        }

        private void DrawCompent(Component item)
        {
            EditorGUILayout.ObjectField(item, typeof(Component), true);
        }
    }

}

