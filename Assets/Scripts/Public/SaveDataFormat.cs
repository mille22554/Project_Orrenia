using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class PlayerSaveDataFormat
{
    public string version;
    public Datas Datas;
}

public class Datas
{
    public PlayerContextData PlayerData;
    public CharacterData CharacterData;
    public PartyData PartyData;

    public static Datas CreateDefault()
    {
        var datas = new Datas
        {
            PlayerData = PlayerContextData.CreateDefault(),
            CharacterData = CharacterData.CreateDefault(),
            PartyData = PartyData.CreateDefault(),
        };

        return datas;
    }
}

public class PartySaveDataFormat
{
    public long PartyID;
    public string Leader;
    public List<string> Members = new();
    public int Area;
    public int Deep;
    public List<MobData> Enemies = new();
}

public class EnemyData
{
    public List<MobData> Enemies;

    public EnemyData()
    {
        Enemies = new();
    }
}