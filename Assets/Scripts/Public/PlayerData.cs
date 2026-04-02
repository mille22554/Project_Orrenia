

public class PlayerContextData
{
    public int Area;
    public int Deep;
    public int Gold;

    public int SkillPoint;

    public int ForgeLevel;
    public int CurrentForgeExp;

    public bool IsGetBasicDagger;

    public static PlayerContextData CreateDefault()
    {
        var data = new PlayerContextData
        {
            Area = 1,
            Deep = 0,
            Gold = 0,
            SkillPoint = 0,

            ForgeLevel = 1,
            CurrentForgeExp = 0,

            IsGetBasicDagger = false
        };

        return data;
    }
}