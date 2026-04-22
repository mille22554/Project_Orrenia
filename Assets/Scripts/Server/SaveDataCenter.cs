using System.IO;
using Newtonsoft.Json;

public static class SaveDataCenter
{
    public static PlayerSaveDataFormat CreateSaveData()
    {
        var saveData = new PlayerSaveDataFormat
        {
            version = GameData_Server.version,
            Datas = Datas.CreateDefault()
        };
        saveData.Datas.CharacterData.BagItems.Add(ItemDataCenter_Server.GetNewItemByItemID(1));

        return saveData;
    }

    public static void SaveData(string account)
    {
        var path = GameData_Server.PlayerSaveDataPath(account);
        // Debug.Log($"儲存遊戲資料到 {path}");
        File.WriteAllText(path, JsonConvert.SerializeObject(GameData_Server.NowPlayers[account]));
    }
}