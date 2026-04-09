using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public static class SkillDataCenter
{
    static Dictionary<ESkillID, SkillData> _skillDatas;

    static SkillDataCenter()
    {
        string path = GameData_Server.SkillDataPath;
        Debug.Log($"從 {path} 讀取技能資料");

        if (File.Exists(path))
        {
            var json = File.ReadAllText(path);
            _skillDatas = JsonConvert.DeserializeObject<Dictionary<ESkillID, SkillData>>(json);
            foreach (var effectData in _skillDatas)
            {
                effectData.Value.ID = effectData.Key;
            }
        }
        else
        {
            Debug.LogError("SkillData檔案丟失!");
        }
    }

    public static SkillData GetSkillData(ESkillID skillID)
    {
        _skillDatas.TryGetValue(skillID, out var skillData);
        return skillData;
    }
}