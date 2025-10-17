using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;

public class GameSaveData
{
    public string version;
    public Datas datas;

    public GameSaveData()
    {
        version = GameData.version;
        datas = new();
    }
}

public class Datas
{
    public PlayerData playerData;
    public EnemyData enemyData;
    public BagData bagData;

    public Datas()
    {
        playerData = new();
        enemyData = new();
        bagData = new();
    }
}

public class PlayerData
{
    public string name;
    public int level;
    private int currentExp;
    public int CurrentExp
    {
        get { return currentExp; }
        set
        {
            currentExp = value;
            EventMng.EmitEvent(EventName.RefreshPlayerInfo);
        }
    }
    public int maxExp;
    private int currentHp;
    public int CurrentHp
    {
        get { return currentHp; }
        set
        {
            currentHp = value;
            if (currentHp > ability?.HP) currentHp = ability.HP;
            EventMng.EmitEvent(EventName.RefreshPlayerInfo);
        }
    }
    private int currentMp;
    public int CurrentMp
    {
        get { return currentMp; }
        set
        {
            currentMp = value;
            if (currentMp > ability?.MP) currentMp = ability.MP;
            EventMng.EmitEvent(EventName.RefreshPlayerInfo);
        }
    }
    private int currentSTA;
    public int CurrentSTA
    {
        get { return currentSTA; }
        set
        {
            currentSTA = value;
            if (currentSTA > ability?.STA) currentSTA = ability.STA;
            else if (currentSTA < 0) currentSTA = 0;
            else if (currentSTA == 0) PublicFunc.AddPlayerEffect(EffectType.Debuff.Exhausted, 10, 1);

            EventMng.EmitEvent(EventName.RefreshPlayerInfo);
        }
    }
    public int currentTp;

    public string area;
    public int deep;
    public int gold;

    private int abilityPoint;
    public int AbilityPoint
    {
        get { return abilityPoint; }
        set
        {
            abilityPoint = value;
            EventMng.EmitEvent(EventName.RefreshAbilityPoint);
        }
    }
    public AbilityBase ability;

    public EquipBase equips;

    public List<EffectData> effects;
    [JsonIgnore]
    public List<Action> effectActions;

    public int skillPoint;

    public bool isGetBasicDagger2;

    public PlayerData()
    {
        level = 1;
        CurrentExp = 0;
        maxExp = 100;
        CurrentHp = 100;
        CurrentMp = 50;
        CurrentSTA = 100;
        currentTp = 0;

        area = GameArea.Home;
        deep = 0;
        gold = 0;

        AbilityPoint = 0;
        ability = new()
        {
            STR_Point = 1,
            DEX_Point = 1,
            INT_Point = 1,
            VIT_Point = 1,
            AGI_Point = 1,
            LUK_Point = 1
        };
        equips = new();
        effects = new();
        effectActions = new();

        PublicFunc.SetPlayerAbility(ability, equips, effects, effectActions);

        skillPoint = 0;

        isGetBasicDagger2 = false;
    }
}

public class AbilityBase
{
    public int STR_Point;
    public int DEX_Point;
    public int INT_Point;
    public int VIT_Point;
    public int AGI_Point;
    public int LUK_Point;

    public int STR;
    public int DEX;
    public int INT;
    public int VIT;
    public int AGI;
    public int LUK;

    public int HP;
    public int MP;
    public int STA;
    public int ATK;
    public int MATK;
    public int DEF;
    public int MDEF;
    public int ACC;
    public int EVA;
    public int CRIT;
    public int SPD;
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
    public List<MobData> enemies;

    public EnemyData()
    {
        enemies = new();
    }
}

public class MobData
{
    public string name;
    public int level;
    public int currentHp;
    public int currentMp;
    public int currentTp;

    public AbilityBase ability;

    public List<DropItem> dropItems;

    public MobData()
    {
        ability = new();
        dropItems = new();
    }
}

public class DropItem
{
    public ItemData item;
    public int prop;
}

public class BagData
{
    public List<ItemData> items;

    public BagData()
    {
        items = new();
    }
}

public class ItemData
{
    public int id;
    public long uid;
    public string name;
    public string type;
    public string description;
    public int count;
    public AbilityBase ability;
    public int price;
    public int durability;

    public ItemData()
    {
        ability = new();
    }

    public string GetAbilityString()
    {
        if (ability == null) return "";

        List<string> parts = new();

        // 用反射抓 AbilityBase 的欄位
        foreach (var field in typeof(AbilityBase).GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            int value = (int)field.GetValue(ability);
            if (value != 0)
            {
                string sign = value > 0 ? "+" : "";
                parts.Add($"{field.Name}{sign}{value}");
            }
        }

        return string.Join(", ", parts);
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
    public Func<AbilityBase, int> damage;
    public string damageType;
    public string effect;
    public Action special;
    public string weaponType;
    public int cost;
    public int cooldown;
}