using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Runtime.InteropServices;

namespace UnityEngine.UI
{
    [ExecuteInEditMode]
    public class UIPackage : MonoBehaviour, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<string> names;

        [SerializeField]
        private List<Component> components;

        [NonSerialized]
        public Dictionary<string, object> objects;
 

        public T Get<T>(string name) where T : Component
        {
            return Get(name, typeof(T)) as T;
        }

        public List<T> GetAll<T>(string name) where T : Component
        {
            return GetAll(name, typeof(T)) as List<T>;
        }

        Component Get(string name, Type type = null)
        {
            object result = RawGet(name, type);
            if (result is Component[])
                return (result as Component[])[0];
            else
                return result as Component;
        }

        List<Component> GetAll(string name, Type type)
        {
            object result = RawGet(name, type);
            if (result is List<Component>)
                return result as List<Component>;
            else if (result is Component)
                return new List<Component> { result as Component };
            else
                return null;
        }

        object RawGet(string name, Type type = null)
        {
            object obj;
            if (!objects.TryGetValue(name, out obj))
                return null;

            if (obj is Component)
                return obj;

            Dictionary<Type, object> typeObj = obj as Dictionary<Type, object>;
            if (typeObj != null)
            {
                object obj2 = null;
                if (type == null)
                {
                    foreach (var pair in typeObj)
                    {
                        obj2 = pair.Value;
                        break;
                    }
                }
                else
                {
                    typeObj.TryGetValue(type, out obj2);
                }
                return obj2;
            }

            return null;
        }

        public object this[string name]
        {
            get
            {
                return Get(name);
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            names = new List<string>();
            components = new List<Component>();

            if (objects == null)
                return;

            foreach (var pair in objects)
            {
                if (pair.Value is Component)
                {
                    names.Add(pair.Key);
                    components.Add(pair.Value as Component);
                }
                else
                {
                    Dictionary<Type, object> typeObj = pair.Value as Dictionary<Type, object>;
                    if (typeObj != null)
                    {
                        foreach (var pair2 in typeObj)
                        {
                            if (pair2.Value is Component)
                            {
                                names.Add(pair.Key);
                                components.Add(pair2.Value as Component);
                            }
                            else if (pair2.Value is List<Component>)
                            {
                                foreach (Component item in pair2.Value as List<Component>)
                                {
                                    names.Add(pair.Key);
                                    components.Add(item);
                                }
                            }
                        }
                    }
                }
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            List<Vector3> v = new List<Vector3>();
            objects = new Dictionary<string, object>();
            int count = names.Count;
            for (int i = 0; i < count; i++)
            {
                string name = names[i];
                Component component = components[i];
                AddData(name, component);
            }
        }
        
        void AddData(string name,Component component)
        {
            if (!objects.ContainsKey(name))
            {
                objects.Add(name, component);
            }
            else
            {
                object oldValue = objects[name];
                Dictionary<Type, object> typeObj = null;
                if (oldValue is Component)
                {
                    typeObj = new Dictionary<Type, object> { { oldValue.GetType(), oldValue as Component } };
                    objects[name] = typeObj;
                }
                else
                {
                    typeObj = oldValue as Dictionary<Type, object>;
                }

                if (typeObj != null)
                {
                    Type t = component.GetType();
                    if (!typeObj.ContainsKey(t))
                    {
                        typeObj.Add(t, component);
                    }
                    else
                    {
                        object obj2 = typeObj[t];
                        if (obj2 is List<Component>)
                        {
                            (obj2 as List<Component>).Add(component);
                        }
                        else if (obj2 is Component)
                        {
                            typeObj[t] = new List<Component>() { obj2 as Component, component };
                        }
                    }
                }
            }
        }

        public void GetData()
        {
            if (objects == null)
                objects = new Dictionary<string, object>();
            else
                objects.Clear();

            GetDataFromChildren(transform);
        }

        void GetDataFromChildren(Transform trans)
        {
            foreach (var item in trans.GetComponents<Button>())
            {
                AddData(item.name,item);
            }
            foreach (var item in trans.GetComponents<Text>())
            {
                AddData(item.name, item);
            }

            int count = trans.childCount;
            for (int i = 0;i < count;i++)
            {
                Transform child = trans.GetChild(i);
                UIPackage pack = child.GetComponent<UIPackage>();
                if (pack != null)
                {
                    AddData(pack.name, pack);
                }
                else
                {
                    GetDataFromChildren(child);
                }
            }
        }


#if UNITY_EDITOR
        void Update()
        {
            if (!Application.isPlaying)
            {
                GetData();
                EditorUtility.SetDirty(this);
            }
        }
#endif
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = false)]
    [ComVisible(true)]
    public sealed class UIProperty : Attribute
    {
        public string Name { get; private set; }
        public UIProperty(string name = null)
        {
            Name = name;
        }
    }
}

