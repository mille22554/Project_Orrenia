using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;
using Random = UnityEngine.Random;

public class MobDataBase
{
    public string Name;
    public AbilityBase Ability;
    public List<DropItem> DropItems;
}

public static class MobDataCenter
{
    static readonly Dictionary<int, MobDataBase> mobBaseDatas = new();

    static MobDataCenter()
    {
        string path = GameData_Server.MobDataPath;
        Debug.Log($"從 {path} 讀取怪物資料");

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            mobBaseDatas = JsonConvert.DeserializeObject<Dictionary<int, MobDataBase>>(json);
        }
        else
        {
            Debug.LogError("MobData檔案丟失!");
        }
    }

    public static MobData GetRandomMob(int area, int level)
    {
        var mobList = AreaDataCenter.GetAreaData(area).MobList;
        var index = Random.Range(0, mobList.Count);
        var mobID = mobList.ElementAtOrDefault(index);

        var mobData = MobData.CreateDefault();
        if (mobBaseDatas.TryGetValue(mobID, out var mob))
        {
            mobData.CharacterData.Name = mob.Name;
            mobData.CharacterData.Level = level;
            mobData.CharacterData.Role = CharacterRole.Mob;

            mobData.CharacterData.Ability.STR_Point += mob.Ability.STR_Point * level;
            mobData.CharacterData.Ability.DEX_Point += mob.Ability.DEX_Point * level;
            mobData.CharacterData.Ability.INT_Point += mob.Ability.INT_Point * level;
            mobData.CharacterData.Ability.VIT_Point += mob.Ability.VIT_Point * level;
            mobData.CharacterData.Ability.AGI_Point += mob.Ability.AGI_Point * level;
            mobData.CharacterData.Ability.LUK_Point += mob.Ability.LUK_Point * level;

            PublicFunc.InitCurrentData(mobData.CharacterData);

            mobData.DropItems = mob.DropItems;
        }
        else
        {
            Debug.LogError("取得怪物資料出錯!");
        }

        return mobData;
    }
}