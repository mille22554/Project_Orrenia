using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class SaveDataFormat
{
    public string version;
    public Datas Datas;
}

public class Datas
{
    public PlayerContextData PlayerData;
    public CharacterData CharacterData;
    public EnemyData EnemyData;
    public BagData BagData;

    public static Datas CreateDefault()
    {
        var datas = new Datas
        {
            PlayerData = PlayerContextData.CreateDefault(),
            CharacterData = CharacterData.CreateDefault(),
            EnemyData = new(),
            BagData = new()
        };

        return datas;
    }
}

public class EnemyData
{
    public List<MobData> Enemies;

    public EnemyData()
    {
        Enemies = new();
    }
}

public class BagData
{
    public List<BagItemData> Items;

    public BagData()
    {
        Items = new();
    }
}

public class SkillData
{
    public string name;
    public string description;
    public Func<FullAbilityBase, int> damage;
    public string damageType;
    public string effect;
    public Action special;
    public string weaponType;
    public int cost;
    public int cooldown;
}