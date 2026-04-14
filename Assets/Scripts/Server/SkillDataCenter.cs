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
            Effect = skillData.Effect,
            WeaponType = skillData.WeaponType,
            Cost = skillData.Cost,
            CoolDown = skillData.CoolDown,
        };
    }

    public static int SkillDamage(BattleData battleData, SkillData skillData)
    {
        var damage = 0m;
        foreach (var format in skillData.Damage)
        {
            damage += format.Constant *
            (
                format.Ability.STR * battleData.STR +
                format.Ability.DEX * battleData.DEX +
                format.Ability.INT * battleData.INT +
                format.Ability.VIT * battleData.VIT +
                format.Ability.AGI * battleData.AGI +
                format.Ability.LUK * battleData.LUK +
                format.Ability.HP * battleData.HP +
                format.Ability.MP * battleData.MP +
                format.Ability.STA * battleData.STA +
                format.Ability.ATK * battleData.ATK +
                format.Ability.MATK * battleData.MATK +
                format.Ability.DEF * battleData.DEF +
                format.Ability.MDEF * battleData.MDEF +
                format.Ability.ACC * battleData.ACC +
                format.Ability.EVA * battleData.EVA +
                format.Ability.CRIT * battleData.CRIT +
                format.Ability.SPD * battleData.SPD
            );
        }

        return (int)damage;
    }
}