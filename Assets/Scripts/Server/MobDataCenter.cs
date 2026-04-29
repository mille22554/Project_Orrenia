using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using Random = UnityEngine.Random;

public class MobDataBase
{
    public string Name;
    public int ID;
    public AbilityBase Ability = new();
    public List<ESkillID> Skills = new();
    public List<DropItem> DropItems = new();
    public List<int> Equips = new();
}

public static class MobDataCenter
{
    static Dictionary<int, MobDataBase> _datas;
    readonly static Dictionary<int, IMobHandler> _mobHandlers = new();

    static MobDataCenter()
    {
        LoadMobData();
        RegisterHandlers();
    }

    static void LoadMobData()
    {
        string path = GameData_Server.MobDataPath;
        Debug.Log($"從 {path} 讀取怪物資料");

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            _datas = JsonConvert.DeserializeObject<Dictionary<int, MobDataBase>>(json);

            foreach (var (id, mob) in _datas)
                mob.ID = id;
        }
        else
        {
            Debug.LogError("MobData檔案丟失!");
        }
    }

    static void RegisterHandlers()
    {
        var types = typeof(CharacterDataCenter).Assembly.GetTypes();

        foreach (var type in types)
        {
            if (type.IsInterface || type.IsAbstract)
                continue;

            if (!typeof(IMobHandler).IsAssignableFrom(type))
                continue;

            var instance = (IMobHandler)Activator.CreateInstance(type);

            Debug.Log($"Register effect handler: {instance.ID} -> {type.Name}");

            _mobHandlers[instance.ID] = instance;
        }
    }

    public static CharacterData GetRandomMob(int area, int deep, long partyUID)
    {
        var level = EnemySetting.GetEnemyLevel(area, deep);
        var mobList = AreaDataCenter.GetAreaData(area).MobList;
        var deepParam = Mathf.Max(0, deep - 300);
        var index = Random.Range(Mathf.Min(deepParam / 100, mobList.Count - 4), Mathf.Min(mobList.Count, deepParam / 100 + 3));
        var mobID = mobList.ElementAtOrDefault(index);

        if (_datas.TryGetValue(mobID, out var mob))
        {
            var mobData = MobData.CreateDefault(partyUID, mob.ID);
            SaveDataCenter.NewDataToDB(MobSave.Create(mobData));

            var characterData = CharacterData.CreateDefault(mobData.UID);
            characterData.Name = mob.Name;
            characterData.Level = level;
            characterData.Role = ECharacterRole.Mob;

            var mobAbility = AbilityBase.CreateDefault(mobData.UID);
            mobAbility.STR_Point += mob.Ability.STR_Point * level;
            mobAbility.DEX_Point += mob.Ability.DEX_Point * level;
            mobAbility.INT_Point += mob.Ability.INT_Point * level;
            mobAbility.VIT_Point += mob.Ability.VIT_Point * level;
            mobAbility.AGI_Point += mob.Ability.AGI_Point * level;
            mobAbility.LUK_Point += mob.Ability.LUK_Point * level;
            SaveDataCenter.NewDataToDB(CharaterAbilitySave.Create(mobAbility));

            CharacterDataCenter.InitCurrentData(characterData);

            foreach (var equipID in mob.Equips)
            {
                var equip = ItemDataCenter_Server.GetNewItemByItemID(equipID, mobData.UID);
                SaveDataCenter.NewDataToDB(BagItemSave.Create(equip));
                SaveDataCenter.NewDataToDB(EquipSave.Create(equip));
            }

            foreach (var skillID in mob.Skills)
                SaveDataCenter.NewDataToDB(SkillSave.Create(SkillDataCenter.GetSkillData(skillID, mobData.UID)));

            return characterData;
        }
        else
        {
            Debug.LogError("取得怪物資料出錯!");
            return null;
        }
    }

    public static SkillData SetMobAction(CharacterData mob, int mobID)
    {
        if (_mobHandlers.TryGetValue(mobID, out var mobHandler))
            return mobHandler.Handler(mob);

        return null;
    }
}

public static class EnemySetting
{
    const float falloff = 10f; // 權重衰減因子，數值越大，距離中心越遠的數值權重衰減越快

    public static void SetEnemy(int area, int deep, long partyUID)
    {
        var enemyIDCounter = new Dictionary<string, int>();
        var enemyNum = GetEnemyNum(deep);

        for (int i = 0; i < enemyNum; i++)
        {
            var mob = MobDataCenter.GetRandomMob(area, deep, partyUID);

            if (!enemyIDCounter.ContainsKey(mob.Name))
                enemyIDCounter[mob.Name] = 1;
            else
                enemyIDCounter[mob.Name] += 1;

            mob.Name += enemyIDCounter[mob.Name];
            SaveDataCenter.NewDataToDB(CharacterSave.Create(mob));
        }
    }

    static int GetEnemyNum(int deep)
    {
        // 先計算每個x的機率
        float p1; // x=1
        float p2; // x=2

        float t = (deep - 1) / 999f;
        if (deep < 500)
        {
            float t2 = (deep - 1) / 499f;
            p1 = Mathf.Lerp(1f, 0.72f, t2);
            p2 = Mathf.Lerp(0f, 0.18f, t2);
        }
        else if (deep > 500)
        {
            float t2 = Mathf.Clamp01((deep - 500) / 500f);
            p1 = Mathf.Lerp(0.72f, 0.44f, t2);
            p2 = Mathf.Lerp(0.18f, 0.36f, t2);
        }
        else
        {
            p1 = 0.72f; p2 = 0.18f;
        }

        // 隨機選擇
        float r = Random.value; // 0~1
        if (r < p1)
            return 1;
        else if (r < p1 + p2)
            return 2;
        else
            return 3;
    }

    public static int GetEnemyLevel(int area, int deep)
    {
        var areaData = AreaDataCenter.GetAreaData(area);

        // 中心值
        float center = deep / 100f;

        // 範圍
        int min = Mathf.Clamp(Mathf.FloorToInt(center) - 2, areaData.MinMobLevel, areaData.MaxMobLevel);
        int max = Mathf.Clamp(Mathf.CeilToInt(center) + 2, areaData.MinMobLevel, areaData.MaxMobLevel);

        if (min > max)
        {
            int safe = Mathf.Clamp(Mathf.RoundToInt(center), areaData.MinMobLevel, areaData.MaxMobLevel);
            return safe;
        }

        // 計算權重（距離中心越近越高）
        float[] weights = new float[max - min + 1];
        for (int i = 0; i < weights.Length; i++)
        {
            int xVal = min + i;
            int dist = Mathf.Abs(xVal - Mathf.RoundToInt(center));
            weights[i] = Mathf.Pow(1f / (dist + 1), falloff);
        }

        // 計算累積機率
        float total = weights.Sum();
        float r = Random.value * total;
        float sum = 0;
        for (int i = 0; i < weights.Length; i++)
        {
            sum += weights[i];
            if (r <= sum)
                return min + i;
        }

        return max;
    }
}

public interface IMobHandler
{
    int ID { get; }
    SkillData Handler(CharacterData mob);
}