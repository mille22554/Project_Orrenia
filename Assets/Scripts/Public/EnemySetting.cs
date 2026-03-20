using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
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
            var level = GetEnemyLevel(deep);
            var mob = MobDataCenter.GetRandomMob(area, level);

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

    static int GetEnemyLevel(int deep)
    {
        // 中心值
        float center = deep / 100f;

        // 範圍
        int min = Mathf.Clamp(Mathf.FloorToInt(center) - 2, 1, 10);
        int max = Mathf.Clamp(Mathf.CeilToInt(center) + 2, 1, 10);

        if (min > max)
        {
            int safe = Mathf.Clamp(Mathf.RoundToInt(center), 1, 10);
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