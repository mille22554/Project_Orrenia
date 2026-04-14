using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using Random = UnityEngine.Random;

public class MobDataBase
{
    public string Name;
    public AbilityBase Ability = new();
    public List<ESkillID> Skills = new();
    public List<DropItem> DropItems = new();
    public List<int> Equips = new();
}

public static class MobDataCenter
{
    static readonly Dictionary<int, MobDataBase> _datas = new();

    static MobDataCenter()
    {
        string path = GameData_Server.MobDataPath;
        Debug.Log($"從 {path} 讀取怪物資料");

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            _datas = JsonConvert.DeserializeObject<Dictionary<int, MobDataBase>>(json);
        }
        else
        {
            Debug.LogError("MobData檔案丟失!");
        }
    }

    public static MobData GetRandomMob(int area, int deep)
    {
        var level = EnemySetting.GetEnemyLevel(area, deep);
        var mobList = AreaDataCenter.GetAreaData(area).MobList;
        var index = Random.Range(Mathf.Min(0 + deep / 300, mobList.Count - 4), Mathf.Min(mobList.Count, deep / 300 + 3));
        var mobID = mobList.ElementAtOrDefault(index);

        var mobData = MobData.CreateDefault();
        if (_datas.TryGetValue(mobID, out var mob))
        {
            mobData.CharacterData.Name = mob.Name;
            mobData.CharacterData.Level = level;
            mobData.CharacterData.Role = ECharacterRole.Mob;

            mobData.CharacterData.Ability.STR_Point += mob.Ability.STR_Point * level;
            mobData.CharacterData.Ability.DEX_Point += mob.Ability.DEX_Point * level;
            mobData.CharacterData.Ability.INT_Point += mob.Ability.INT_Point * level;
            mobData.CharacterData.Ability.VIT_Point += mob.Ability.VIT_Point * level;
            mobData.CharacterData.Ability.AGI_Point += mob.Ability.AGI_Point * level;
            mobData.CharacterData.Ability.LUK_Point += mob.Ability.LUK_Point * level;

            CharacterDataCenter.InitCurrentData(mobData.CharacterData);

            mobData.DropItems = mob.DropItems;

            foreach (var equipID in mob.Equips)
            {
                var equip = ItemDataCenter_Server.GetNewItemByItemID(equipID);
                mobData.CharacterData.BagItems.Add(equip);
                mobData.CharacterData.Equips.Add(equip.UID);
            }

            foreach (var skillID in mob.Skills)
                mobData.CharacterData.Skills.Add(skillID, SkillDataCenter.GetSkillData(skillID));
        }
        else
        {
            Debug.LogError("取得怪物資料出錯!");
        }

        return mobData;
    }
}

public static class EnemySetting
{
    const float falloff = 10f; // 權重衰減因子，數值越大，距離中心越遠的數值權重衰減越快

    public static List<MobData> SetEnemy(int area, int deep)
    {
        var enemies = new List<MobData>();
        var enemyIDCounter = new Dictionary<string, int>();
        var enemyNum = GetEnemyNum(deep);

        for (int i = 0; i < enemyNum; i++)
        {
            var mob = MobDataCenter.GetRandomMob(area, deep);

            if (!enemyIDCounter.ContainsKey(mob.CharacterData.Name))
                enemyIDCounter[mob.CharacterData.Name] = 1;
            else
                enemyIDCounter[mob.CharacterData.Name] += 1;

            mob.CharacterData.Name += enemyIDCounter[mob.CharacterData.Name];

            enemies.Add(mob);
        }

        return enemies;
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