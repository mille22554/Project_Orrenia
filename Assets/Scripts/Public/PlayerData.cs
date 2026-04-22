

public class PlayerContextData
{
    public string Password;
    public int Gold;

    public string NowPartyLeader;

    public int SkillPoint;

    public int ForgeLevel;
    public int CurrentForgeExp;

    public static PlayerContextData CreateDefault()
    {
        var data = new PlayerContextData
        {
            Gold = 0,
            SkillPoint = 0,
            ForgeLevel = 1,
            CurrentForgeExp = 0,
        };

        return data;
    }
}