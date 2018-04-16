using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Reflection;

namespace UGUIExtend
{
    [ExecuteInEditMode]
    public class UIPackage : MonoBehaviour, ISerializationCallbackReceiver
    {
        [SerializeField]
        string[] names;

        [SerializeField]
        Component[] components;

        [SerializeField]
        public List<Component> customComponents = new List<Component>();

        [NonSerialized]
        public Dictionary<string, object> objects;

        #region Get Method
        public T Get<T>(string name) where T : Component
        {
            return Get(name, typeof(T)) as T;
        }

        public T[] GetAll<T>(string name) where T : Component
        {
            return GetAll(name, typeof(T)) as T[];
        }

        public Component Get(string name, Type type = null)
        {
            object result = RawGet(name, type);
            if (result is Component[])
                return (result as Component[])[0];
            else
                return result as Component;
        }

        public Component[] GetAll(string name, Type type)
        {
            object result = RawGet(name, type);
            if (result is Component[])
                return result as Component[];
            else if (result is Component)
                return new Component[1] { result as Component };
            else
                return null;
        }

        public object this[string name]
        {
            get
            {
                return Get(name);
            }
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

        /// <summary>
        /// 不通过缓存直接查找组件
        /// </summary>
        /// <param name="includeInactive">是否查找禁用状态的组件</param>
        /// <returns></returns>
        public Component FindByPath(string name, Type type, bool includeInactive = false)
        {
            string[] parts = name.Split('/');
            Transform cur = transform;
            foreach (string part in parts)
            {
                cur = FindByName(cur, part, includeInactive);
                if (cur == null)
                    return null;
            }
            return cur.GetComponent(type);
        }
        public T FindByPath<T>(string name, bool includeInactive = false) where T : Component
        {
            return FindByPath(name, typeof(T), includeInactive) as T;
        }

        Transform FindByName(Transform trans, string name, bool includeactive = false)
        {
            if (!includeactive)
            {
                return trans.Find(name);
            }
            else
            {
                int c = trans.childCount;
                for (int i = 0; i < c; i++)
                {
                    Transform child = trans.GetChild(i);
                    if (child.name == name)
                        return child;
                }
                return null;
            }
        }

        #endregion
        #region Serialize
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (objects == null)
                return;

            var names = new List<string>();
            var components = new List<Component>();

            foreach (var pair in objects)
            {
                if (pair.Value == null)
                    continue;

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
                            if (pair2.Value == null)
                                continue;

                            if (pair2.Value is Component)
                            {
                                names.Add(pair.Key);
                                components.Add(pair2.Value as Component);
                            }
                            else if (pair2.Value is Component[])
                            {
                                foreach (Component item in pair2.Value as Component[])
                                {
                                    if (item == null)
                                        continue;

                                    names.Add(pair.Key);
                                    components.Add(item);
                                }
                            }
                        }
                    }
                }
            }
            this.names = names.ToArray();
            this.components = components.ToArray();
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            objects = new Dictionary<string, object>();
            int count = names.Length;
            for (int i = 0; i < count; i++)
            {
                string name = names[i];
                Component component = components[i];
                AddData(name, component);
            }
        }
        #endregion
        #region Create Datas
        HashSet<Component> customComponentSet;

        /// <summary>
        /// 临时创建时可以用此方法手动初始化数据
        /// </summary>
        public void RefreshDatas()
        {
            if (objects == null)
                objects = new Dictionary<string, object>();
            else
                objects.Clear();

            ChangeGameObjectName();

            customComponentSet = new HashSet<Component>(customComponents);
            foreach (var item in customComponents)
            {
                AddData(item.name, item);
            }
            AddDataFromChildren(transform);
        }

        //删除空格，括号等非法命名
        void ChangeGameObjectName()
        {
#if UNITY_EDITOR
            foreach (Transform t in GetComponentsInChildren<Transform>())
            {
                t.name = t.name.Replace(" ", "").Replace("(", "").Replace(")", "");
            }
#endif
        }

        void AddDataFromChildren(Transform trans)
        {
            foreach (var item in trans.GetComponents<Button>())
            {
                if (!customComponentSet.Contains(item))
                    AddData(item.name, item);
            }
            foreach (var item in trans.GetComponents<Text>())
            {
                if (!customComponentSet.Contains(item))
                    AddData(item.name, item);
            }

            int count = trans.childCount;
            for (int i = 0; i < count; i++)
            {
                Transform child = trans.GetChild(i);
                UIPackage pack = child.GetComponent<UIPackage>();
                if (pack != null)
                {
                    AddData(pack.name, pack);
                }
                else
                {
                    AddDataFromChildren(child);
                }
            }
        }

        void AddData(string name, Component component)
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
                        if (obj2 is Component[])
                        {
                            Component[] arr = obj2 as Component[];
                            Array.Resize<Component>(ref arr, arr.Length + 1);
                            arr[arr.Length - 1] = component;
                            typeObj[t] = arr;
                        }
                        else if (obj2 is Component)
                        {
                            typeObj[t] = new Component[] { obj2 as Component, component };
                        }
                    }
                }
            }
        }

#if UNITY_EDITOR
        //void OnTransformChildrenChanged()
        //{
        //    RefreshDatas();
        //    UnityEditor.EditorUtility.SetDirty(this);
        //}
        //确保RefreshDatas在保存时一定被执行
        [UnityEditor.InitializeOnLoadMethod]
        static void StartInitializeOnLoadMethod()
        {
            UnityEditor.PrefabUtility.prefabInstanceUpdated += ApplyHandler;
        }

        static void ApplyHandler(GameObject go)
        {
            foreach (var pack in go.GetComponentsInChildren<UIPackage>())
            {
                pack.RefreshDatas();
            }
        }
#endif
        #endregion
        #region Inject

        /// <summary>
        /// 根据UIAttribute自动注入UI控件的依赖
        /// </summary>
        public void InjectAll(object target)
        {
            Type T = target.GetType();
            foreach (FieldInfo fi in T.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                object value = GetUI(fi);
                fi.SetValue(target, value);
            }
            foreach (MethodInfo fi in T.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                object value = GetUI(fi);
                if (value is Button)
                {
                    UnityEngine.Events.UnityAction fun = Delegate.CreateDelegate(typeof(UnityEngine.Events.UnityAction), target, fi) as UnityEngine.Events.UnityAction;
                    (value as Button).onClick.AddListener(fun);
                }
            }
        }

        object GetUI(MemberInfo info)
        {
            object result = null;
            object[] attrs = info.GetCustomAttributes(typeof(UIAttribute), false);
            if (attrs != null && attrs.Length > 0)
            {
                UIAttribute attr = attrs[0] as UIAttribute;
                if (attr.findByName)
                {
                    result = FindByPath(attr.name, info.DeclaringType, attr.findInactive);
                }
                else
                {
                    string name = attr.name != null ? attr.name : info.Name;
                    Type fieldType = info is FieldInfo ? (info as FieldInfo).FieldType : null;
                    if (fieldType.IsArray)
                    {
                        Type elementType = fieldType.GetElementType();
                        Component[] components = GetAll(name, elementType);
                        if (components != null)
                        {
                            Array resultList = Array.CreateInstance(elementType, components.Length);
                            for (int i = 0, c = components.Length; i < c; i++)
                            {
                                resultList.SetValue(components[i], i);
                            }
                            result = resultList;
                        }
                    }
                    else if (attr.index != -1)
                    {
                        Component[] components = GetAll(name, fieldType);
                        result = attr.index < components.Length ? components[attr.index] : null;
                    }
                    else
                    {
                        result = Get(name, fieldType);
                    }
                }

                if (result == null && attr.warn)
                {
                    Debug.LogWarning(this.name + "/" + name + " No Find!");
                }
            }
            return result;
        }

        Type GetGenericType(Type t)
        {
            Type[] types = t.GetGenericArguments();
            return types != null && types.Length > 0 ? types[0] : null;
        }

        #endregion
    }

    public sealed class UIAttribute : Attribute
    {
        public string name;
        public int index = -1;
        public bool warn = true;

        public bool findByName;
        public bool findInactive;

        public UIAttribute(string name = null)
        {
            this.name = name;
        }
    }
}

