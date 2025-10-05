public class GameSaveData
{
    public string version;
    public Datas datas;
}

public class Datas
{
    public PlayerData playerData;
}

public class PlayerData
{
    public string name;
    public int level;
    public int currentExp;
    public int maxExp;
    public int currentHp;
    public int maxHp;
    public int currentMp;
    public int maxMp;

    public int gold;

    public int abilityPoint;
    public int STR;
    public int DEX;
    public int INT;
    public int VIT;
    public int AGI;
    public int LUK;

    public int skillPoint;

    public PlayerData()
    {
        level = 1;
        currentExp = 0;
        maxExp = 100;
        currentHp = 100;
        maxHp = 100;
        currentMp = 50;
        maxMp = 50;

        gold = 0;

        abilityPoint = 0;
        STR = 1;
        DEX = 1;
        INT = 1;
        VIT = 1;
        AGI = 1;
        LUK = 1;

        skillPoint = 0;
    }
}