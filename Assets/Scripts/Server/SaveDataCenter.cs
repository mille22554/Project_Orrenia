using System.IO;
using Newtonsoft.Json;

public static class SaveDataCenter
{

    public static SaveDataFormat CreateSaveData()
    {
        var saveData = new SaveDataFormat
        {
            version = GameData_Server.version,
            Datas = Datas.CreateDefault()
        };

        return saveData;
    }
    
    public static void SaveData()
    {
        var path = GameData_Server.SaveDataPath;
        // Debug.Log($"儲存遊戲資料到 {path}");
        File.WriteAllText(path, JsonConvert.SerializeObject(GameData_Server.SaveData));
    }
}