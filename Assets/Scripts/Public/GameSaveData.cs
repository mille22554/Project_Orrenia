using System;
using System.Collections.Generic;
using System.Reflection;

public class GameSaveData
{
    public string version;
    public Datas Datas;

    public static GameSaveData CreateDefault()
    {
        var saveData = new GameSaveData
        {
            version = GameData_Server.version,
            Datas = Datas.CreateDefault()
        };

        return saveData;
    }
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

public class EquipBase
{
    public long Right_Hand;
    public long Left_Hand;
    public long Helmet;
    public long Armor;
    public long Greaves;
    public long Shoes;
    public long Gloves;
    public long Cape;
    public long Ring;
    public long Pendant;
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
    public List<ItemData> Items;

    public BagData()
    {
        Items = new();
    }
}

public class EffectData
{
    public string type;
    public int value;
    public int times;
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