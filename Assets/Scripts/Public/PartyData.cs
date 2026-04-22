using System.Collections.Generic;

public class PartyData
{
    public string Leader;
    public List<string> Members = new();
    public int Area;
    public int Deep;
    public List<MobData> Enemies = new();

    public static PartyData CreateDefault()
    {
        return new PartyData
        {
            Area = 1,
            Deep = 0,
        };
    }
}