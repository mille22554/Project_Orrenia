using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Random = UnityEngine.Random;

public static class EnemySetting
{
    private const float falloff = 10f; // 權重衰減因子，數值越大，距離中心越遠的數值權重衰減越快

    public static List<MobData> SetEnemy(string area, int deep)
    {
        List<MobData> enemies = new();
        Dictionary<string, int> enemyIDCounter = new();
        var enemyNum = GetEnemyNum(deep);
        for (int i = 0; i < enemyNum; i++)
            switch (area)
            {
                case GameArea.Floor1:
                    var enemyName = GetRandomMob(typeof(GameEnemy.Floor1));
                    if (!enemyIDCounter.ContainsKey(enemyName)) enemyIDCounter[enemyName] = 1;
                    else enemyIDCounter[enemyName] += 1;

                    var enemy = new MobData()
                    {
                        name = enemyName + enemyIDCounter[enemyName],
                        level = GetEnemyLevel(deep),
                        ability = new()
                    };
                    var ap = (enemy.level - 1) * 6;
                    switch (enemyName)
                    {
                        case GameEnemy.Floor1.mob1:
                            enemy.ability.STR = 1 + ap / 6;
                            enemy.ability.DEX = 1 + ap / 6;
                            enemy.ability.INT = 1 + ap / 6;
                            enemy.ability.VIT = 1 + ap / 6;
                            enemy.ability.AGI = 1 + ap / 6;
                            enemy.ability.LUK = 1 + ap / 6;
                            enemy.ability = SetAbility(enemy.ability);
                            enemy.currentHp = enemy.ability.HP;
                            enemy.currentMp = enemy.ability.MP;
                            enemy.currentTp = 0;

                            enemy.dropItems = new()
                            {
                                new()
                                {
                                    item=GameItem.Material.SlimeGel,
                                    prop=80
                                }
                            };

                            enemies.Add(enemy);
                            break;
                        case GameEnemy.Floor1.mob2:
                            enemy.ability.STR = 1;
                            enemy.ability.DEX = 1 + ap / 3;
                            enemy.ability.INT = 1;
                            enemy.ability.VIT = 1;
                            enemy.ability.AGI = 1 + ap / 6;
                            enemy.ability.LUK = 1 + ap / 2;
                            enemy.ability = SetAbility(enemy.ability);
                            enemy.currentHp = enemy.ability.HP;
                            enemy.currentMp = enemy.ability.MP;
                            enemy.currentTp = 0;

                            enemy.dropItems = new()
                            {
                                new()
                                {
                                    item=GameItem.Material.WhiteRabbitFur,
                                    prop=80
                                }
                            };

                            enemies.Add(enemy);
                            break;
                        case GameEnemy.Floor1.mob3:
                            enemy.ability.STR = 1;
                            enemy.ability.DEX = 1 + ap / 2;
                            enemy.ability.INT = 1;
                            enemy.ability.VIT = 1;
                            enemy.ability.AGI = 1 + ap / 2;
                            enemy.ability.LUK = 1;
                            enemy.ability = SetAbility(enemy.ability);
                            enemy.currentHp = enemy.ability.HP;
                            enemy.currentMp = enemy.ability.MP;
                            enemy.currentTp = 0;

                            enemy.dropItems = new()
                            {
                                new()
                                {
                                    item=GameItem.Material.SparrowFeather,
                                    prop=80
                                }
                            };

                            enemies.Add(enemy);
                            break;
                    }
                    break;
            }
        return enemies;
    }

    private static int GetEnemyNum(int deep)
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
            float t2 = (deep - 500) / 500f;
            p1 = Mathf.Lerp(0.72f, 0.44f, t2);
            p2 = Mathf.Lerp(0.18f, 0.36f, t2);
        }
        else
        {
            p1 = 0.72f; p2 = 0.18f;
        }

        // 隨機選擇
        float r = Random.value; // 0~1
        if (r < p1) return 1;
        else if (r < p1 + p2) return 2;
        else return 3;
    }

    private static string GetRandomMob(Type type)
    {
        var fields = type
                    .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                    .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
                    .ToArray();
        int index = Random.Range(0, fields.Length);
        return (string)fields[index].GetValue(null);
    }

    private static int GetEnemyLevel(int deep)
    {
        // 中心值
        float center = deep / 100f;

        // 範圍
        int min = Mathf.Max(1, Mathf.FloorToInt(center) - 2);
        int max = Mathf.Min(10, Mathf.CeilToInt(center) + 2);

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

        return min; // 保險回傳
    }

    public static AbilityBase SetAbility(AbilityBase data)
    {
        data.HP = (data.VIT * 10 + data.STR * 5 + 85) / 10;
        data.MP = data.INT * 10 + data.VIT * 5 + 35;
        data.ATK = data.STR * 2 + data.VIT;
        data.MATK = data.INT * 2 + data.VIT;
        data.DEF = data.VIT * 2 + data.STR;
        data.MDEF = data.VIT * 2 + data.INT;
        data.ACC = data.AGI * 3 + data.DEX * 2 + data.LUK;
        data.EVA = data.DEX * 3 + data.AGI * 2 + data.LUK;
        data.CRIT = data.AGI * 2 + data.LUK;
        data.SPD = data.DEX;

        return data;
    }

    public static void DropItem()
    {

    }
}