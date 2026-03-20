using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public static class AreaDataCenter
{
    static readonly Dictionary<int, AreaData> areaData = new();

    static AreaDataCenter()
    {
        string path = GameData_Server.AreaDataPath;
        Debug.Log($"從 {path} 讀取區域資料");

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            areaData = JsonConvert.DeserializeObject<Dictionary<int, AreaData>>(json);
        }
        else
        {
            Debug.LogError("AreaData檔案丟失!");
        }
    }

    public static AreaData GetAreaData(int areaID)
    {
        if (areaData.TryGetValue(areaID, out var area))
        {
            return area;
        }
        else
        {
            Debug.LogError("不存在的地區!");
            return null;
        }
    }

    public static Dictionary<int, AreaData> GetAllAreaData()
    {
        return areaData;
    }
}

public class AreaData
{
    public string Name;
    public List<int> MobList;
}