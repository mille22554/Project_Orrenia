using System.Collections.Generic;

public class GameSaveData
{
    public string version;
    public Datas datas;
}

public class Datas
{
    public PlayerData playerData;
    public EnemyData enemyData;
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

        abilityPoint = 0;
        ability = new()
        {
            STR = 1,
            DEX = 1,
            INT = 1,
            VIT = 1,
            AGI = 1,
            LUK = 1
        };
        ability = PublicFunc.SetAbility(ability);

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
}

public class MobData
{
    public string name;
    public int level;
    public int currentHp;
    public int currentMp;
    public int currentTp;

    public AbilityBase ability;
}