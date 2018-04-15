using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Reflection;

namespace UGUIExtend
{
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

    [ExecuteInEditMode]
    public class UIPackage : MonoBehaviour, ISerializationCallbackReceiver
    {
        [SerializeField]
        List<string> names;

        [SerializeField]
        List<Component> components;

        [SerializeField]
        public List<Component> customComponents;

        [NonSerialized]
        public Dictionary<string, object> objects;

        #region Get Method
        public T Get<T>(string name) where T : Component
        {
            return Get(name, typeof(T)) as T;
        }

        public List<T> GetAll<T>(string name) where T : Component
        {
            return GetAll(name, typeof(T)) as List<T>;
        }

        public Component Get(string name, Type type = null)
        {
            object result = RawGet(name, type);
            if (result is Component[])
                return (result as Component[])[0];
            else
                return result as Component;
        }

        public List<Component> GetAll(string name, Type type)
        {
            object result = RawGet(name, type);
            if (result is List<Component>)
                return result as List<Component>;
            else if (result is Component)
                return new List<Component> { result as Component };
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

            customComponentSet = new HashSet<Component>(customComponents);

            foreach (var item in customComponents)
            {
                AddData(item.name, item);
            }
            AddDataFromChildren(transform);
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

#if UNITY_EDITOR
        //确保RefreshDatas在保存时可以被执行
        [UnityEditor.InitializeOnLoadMethod]
        static void StartInitializeOnLoadMethod()
        {
            UnityEditor.PrefabUtility.prefabInstanceUpdated += ApplyHandler;
        }

        static void ApplyHandler(GameObject go)
        {
            UIPackage pack = go.GetComponent<UIPackage>();
            if (pack != null)
                pack.RefreshDatas();
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
                if (value != null)
                {
                    fi.SetValue(target, value);
                }
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
                    if (info.DeclaringType is IEnumerable)
                    {
                        result = GetAll(name, GetGenericType(info.DeclaringType));
                    }
                    else if (attr.index != -1)
                    {
                        List<Component> components = GetAll(name, GetGenericType(info.DeclaringType));
                        result = attr.index < components.Count ? components[attr.index] : null;
                    }
                    else
                    {
                        result = Get(name, info.DeclaringType);
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
}

