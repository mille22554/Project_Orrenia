using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;

public class PublicFunc
{
    public static void SaveData()
    {
        var path = Path.Combine(Application.persistentDataPath, "savedata.json");
        Debug.Log($"儲存遊戲資料到 {path}");
        File.WriteAllText(path, JsonConvert.SerializeObject(GameData.gameData));
    }

    public static GameSaveData UpdateSaveData(GameSaveData oldData)
    {
        Debug.Log("更新存檔資料結構");
        var newData = new GameSaveData
        {
            version = GameData.version,
            datas = new()
            {
                playerData = new()
                {
                    name = oldData.datas.playerData.name
                }
            }
        };

        CopyNonDefaultValues(oldData.datas, newData.datas);
        return newData;
    }

    public static T CopyNonDefaultValues<T>(T oldData, T newData)
    {
        if (oldData == null || newData == null) return newData;

        foreach (var field in oldData.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            object oldValue = field.GetValue(oldData);
            if (oldValue == null) continue;

            FieldInfo newField = newData.GetType().GetField(field.Name, BindingFlags.Public | BindingFlags.Instance);
            if (newField == null) continue; // 新物件沒有這個欄位就跳過

            Type fieldType = field.FieldType;

            // 判斷是否為自訂 class
            if (!fieldType.IsPrimitive && fieldType != typeof(string))
            {
                // 自訂類別就遞迴處理
                object newChild = newField.GetValue(newData);
                if (newChild == null)
                {
                    newChild = Activator.CreateInstance(fieldType);
                    newField.SetValue(newData, newChild);
                }
                CopyNonDefaultValues(oldValue, newChild);
            }
            else
            {
                // 基本型別直接賦值
                newField.SetValue(newData, oldValue);
            }
        }

        return newData;
    }
}