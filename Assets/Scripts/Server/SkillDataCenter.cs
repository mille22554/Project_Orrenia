using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public static class SkillDataCenter
{
    public static Dictionary<ESkillType, string> SkillTypes => _skillTypes;

    static Dictionary<ESkillID, SkillData> _skillDatas;
    static Dictionary<ESkillType, string> _skillTypes;

    static SkillDataCenter()
    {
        LoadSkillData();
        LoadSkillType();
    }

    static void LoadSkillData()
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

    static void LoadSkillType()
    {
        string path = GameData_Server.SkillTypePath;
        Debug.Log($"從 {path} 讀取傷害類型資料");

        if (File.Exists(path))
        {
            var json = File.ReadAllText(path);
            _skillTypes = JsonConvert.DeserializeObject<Dictionary<ESkillType, string>>(json);
        }
        else
        {
            Debug.LogError("SkillType檔案丟失!");
        }
    }

    public static SkillData GetSkillData(ESkillID skillID)
    {
        _skillDatas.TryGetValue(skillID, out var skillData);
        return new SkillData
        {
            Name = skillData.Name,
            ID = skillData.ID,
            Description = skillData.Description,
            SkillType = skillData.SkillType,
            Damage = skillData.Damage,
            Buffs = skillData.Buffs,
            DeBuffs = skillData.DeBuffs,
            WeaponType = skillData.WeaponType,
            Cost = skillData.Cost,
            CoolDown = skillData.CoolDown,
        };
    }
}