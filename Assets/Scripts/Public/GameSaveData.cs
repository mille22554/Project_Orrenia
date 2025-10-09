using System.Collections.Generic;
using System.Reflection;

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
    public int currentExp;
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
    public int currentHp;
    public int CurrentHp
    {
        get { return currentHp; }
        set
        {
            currentHp = value;
            EventMng.EmitEvent(EventName.RefreshPlayerInfo);
        }
    }
    public int currentMp;
    public int CurrentMp
    {
        get { return currentMp; }
        set
        {
            currentMp = value;
            EventMng.EmitEvent(EventName.RefreshPlayerInfo);
        }
    }
    public int currentTp;

    public string area;
    public int deep;
    public int gold;

    public int abilityPoint;
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

    public int skillPoint;

    public PlayerData()
    {
        level = 1;
        CurrentExp = 0;
        maxExp = 100;
        CurrentHp = 100;
        CurrentMp = 50;
        currentTp = 0;

        area = GameArea.Home;
        deep = 0;
        gold = 0;

        AbilityPoint = 0;
        ability = new()
        {
            STR = 1,
            DEX = 1,
            INT = 1,
            VIT = 1,
            AGI = 1,
            LUK = 1
        };
        ability = PublicFunc.SetPlayerAbility(ability);

        skillPoint = 0;
    }
}
public class AbilityBase
{
    public int STR;
    public int DEX;
    public int INT;
    public int VIT;
    public int AGI;
    public int LUK;

    public int HP;
    public int MP;
    public int ATK;
    public int MATK;
    public int DEF;
    public int MDEF;
    public int ACC;
    public int EVA;
    public int CRIT;
    public int SPD;
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

