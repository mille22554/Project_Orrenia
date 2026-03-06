using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool
{
    // 假設有一個字典來儲存所有的類別實例
    static readonly Dictionary<Type, List<MonoBehaviour>> classesDic = new();

    // 泛型方法，接受一個類型並返回該類型的第一個活躍的物件
    public static T Get<T>(T prefab = null, Transform parent = null) where T : MonoBehaviour
    {
        var type = prefab.GetType();
        // 檢查字典中是否包含這個類型的實例
        if (classesDic.TryGetValue(type, out List<MonoBehaviour> classes))
        {
            foreach (var objClass in classes)
            {
                if (objClass == null) continue;
                // 檢查物件是否為活躍狀態
                if (!objClass.gameObject.activeSelf)
                {
                    classes.RemoveAt(classes.IndexOf(objClass));
                    objClass.gameObject.SetActive(true);
                    objClass.transform.SetParent(parent);
                    objClass.transform.SetAsLastSibling();
                    return objClass as T;
                }
            }
        }

        // 如果沒有找到符合條件的物件，返回null
        if (prefab != null && parent != null)
        {
            var result = GameObject.Instantiate(prefab, parent).GetComponent<T>();
            result.transform.SetAsLastSibling();
            return result;
        }
        else
        {
            Debug.LogWarning(prefab);
            Debug.LogWarning(parent);
            return null;
        }
    }

    // 添加物件到字典中
    public static void Put<T>(T objClass) where T : MonoBehaviour
    {
        var type = objClass.GetType();
        // 如果字典中沒有這個類型的列表，則新增一個
        if (!classesDic.ContainsKey(type))
        {
            classesDic[type] = new List<MonoBehaviour>();
        }

        // 將物件加入到列表中
        classesDic[type].Add(objClass);
        // objClass.transform.SetParent(null, false);
        objClass.gameObject.SetActive(false);

        // Debug.Log("ObjectPool Put:" + objClass.name);
    }

    public static void Clean<T>() where T : MonoBehaviour
    {
        // 檢查字典中是否包含這個類型的實例
        if (classesDic.TryGetValue(typeof(T), out List<MonoBehaviour> classes))
        {
            foreach (var objClass in classes)
            {
                GameObject.Destroy(objClass.gameObject);
            }
        }
    }

    public static void CleanAll()
    {
        Debug.Log("ObjectPool CleanAll");
        foreach (var classes in classesDic)
        {
            foreach (var objClass in classes.Value)
            {
                if (objClass == null)
                    Debug.LogWarning("ObjectPool objClass null in cleanAll:" + classes.Key);

                if (objClass != null && objClass.gameObject != null)
                    GameObject.Destroy(objClass.gameObject);
            }
        }

        classesDic.Clear();
        Debug.Log("ObjectPool CleanAll:" + classesDic.Count);
    }
}