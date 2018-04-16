using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System;
using System.Reflection;
using System.Text;

namespace UGUIExtend
{
    [CustomEditor(typeof(UIPackage), false)]
    public class UIPackageEditor : Editor
    {
        UIPackage package
        {
            get { return target as UIPackage; }
        }

        public override void OnInspectorGUI()
        {
            if (Event.current.type == EventType.DragPerform)
            {
                foreach (object item in DragAndDrop.objectReferences)
                {
                    if (item is GameObject)
                    {
                        AddCustomComponent((item as GameObject).transform);
                    }
                    else if (item is Component)
                    {
                        AddCustomComponent(item as Component);
                    }
                }
                Event.current.Use();
            }
            else if (Event.current.type == EventType.DragUpdated)
            {
                DragAndDrop.AcceptDrag();
                DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                Event.current.Use();
            }

            if (package.objects != null && package.objects.Count > 0)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Generate Code"))
                {
                    EditorUtility.DisplayDialog("", GreateCode(), "ok");
                }
                if (GUILayout.Button("Copy To Clipboard"))
                {
                    TextEditor t = new TextEditor();
                    t.text = GreateCode();
                    t.OnFocus();
                    t.Copy();
                }
                EditorGUILayout.EndHorizontal();

                foreach (var pair in package.objects)
                {
                    if (pair.Value == null)
                        continue;

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
                            if (obj2 == null)
                                continue;
                            if (obj2 is Component)
                            {
                                DrawCompent(obj2 as Component);
                            }
                            else
                            {
                                EditorGUILayout.BeginVertical(GUI.skin.box);
                                foreach (var item in obj2 as IEnumerable<Component>)
                                {
                                    if (item == null)
                                        continue;
                                    DrawCompent(item);
                                }
                                EditorGUILayout.EndVertical();
                            }
                        }
                    }
                }
            }
            else
            {
                EditorGUILayout.LabelField("Children Empty");
            }

            if (Event.current.type == EventType.Repaint)
            {
                package.RefreshDatas();
                EditorUtility.SetDirty(target);
            }
        }

        string GreateCode()
        {
            StringBuilder result = new StringBuilder();
            foreach (var pair in package.objects)
            {
                if (pair.Value is Component)
                {
                    result.AppendLine("[UI]" + pair.Value.GetType().Name + " " + pair.Key + ";");
                }
                else
                {
                    Dictionary<Type, object> typeObj = pair.Value as Dictionary<Type, object>;
                    foreach (var pair2 in typeObj)
                    {
                        object obj2 = pair2.Value;
                        if (obj2 is Component)
                        {
                            result.AppendLine("[UI]" + obj2.GetType().Name + " " + pair.Key + ";");
                        }
                        else
                        {
                            foreach (var item in obj2 as IEnumerable<Component>)
                            {
                                result.AppendLine("[UI]" + item.GetType().Name + "[] " + pair.Key + ";");
                                break;
                            }
                        }
                    }
                }
            }
            return result.ToString();
        }

        string GetCompentFullName(Transform trans, bool isEnd = true)
        {
            if (trans.parent == null || trans.parent.parent == null)
                return trans.name;
            else
                return GetCompentFullName(trans.parent,false) + "/" + (isEnd ? "<color=yellow>" + trans.name + "</color>" : trans.name);
        }

        string GetCompentValue(Component asset)
        {
            if (asset is Text)
            {
                return "(" + (asset as Text).text + ")";
            }
            else if (asset is Image && ((asset as Image).sprite != null))
            {
                return "(" + (asset as Image).sprite.name + ")";
            }
            return "";
        }

        void DrawCompent(Component asset)
        {
            if (asset == null)
                return;

            GUIContent content = EditorGUIUtility.ObjectContent(asset, asset.GetType());
            content.text = GetCompentFullName(asset.transform) + " " + GetCompentValue(asset);
            GUIStyle style = new GUIStyle(GUI.skin.label) { richText = true };
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(content,style);
            if (package.customComponents != null && package.customComponents.Contains(asset))
            {
                Component[] components = asset.gameObject.GetComponents(typeof(Component));
                string[] componentNames = new string[components.Length];
                int c = components.Length;
                int selectIndex = 0;
                for (int i = 0;i < c;i++)
                {
                    componentNames[i] = components[i].GetType().Name;
                    if (components[i] == asset)
                        selectIndex = i;
                }
                EditorGUI.BeginChangeCheck();
                selectIndex = EditorGUILayout.Popup(selectIndex, componentNames);
                if (EditorGUI.EndChangeCheck())
                {
                    package.customComponents.Remove(asset);
                    AddCustomComponent(asset.GetComponent(componentNames[selectIndex]));
                }

                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    package.customComponents.Remove(asset);
                }
            }
            else
            {
                EditorGUILayout.LabelField(asset.GetType().Name);
            }
            EditorGUILayout.EndHorizontal();
            if (Event.current.type == EventType.MouseDown && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
            {
                if (Event.current.clickCount >= 2)
                    AssetDatabase.OpenAsset(asset);
                else if (Event.current.clickCount == 1)
                    EditorGUIUtility.PingObject(asset);
            }
        }

        void AddCustomComponent(Component asset)
        {
            if (!package.customComponents.Contains(asset))
                package.customComponents.Add(asset);
        }
    }

}

