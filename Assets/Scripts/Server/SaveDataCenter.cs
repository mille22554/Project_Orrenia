using System.IO;
using Newtonsoft.Json;
using SQLite;

public static class SaveDataCenter
{
    static SQLiteConnection _db;

    public static void Init(long UID)
    {
        var path = GameData_Server.SaveDataBasePath();

        _db = new SQLiteConnection(path);

        // 初始化時建立所有表 (若不存在則建立)
        _db.CreateTable<BagItemSave>();
        _db.CreateTable<CharaterSave>();
        _db.CreateTable<CharaterAbilitySave>();
        _db.CreateTable<EffectSave>();
        _db.CreateTable<EquipSave>();
        _db.CreateTable<MobSave>();
        _db.CreateTable<PartySave>();
        _db.CreateTable<PlayerSave>();
        _db.CreateTable<SkillSave>();
    }

    public static void CreateSaveData_New()
    {
        var saveData = Datas.CreateDefault();

        _db.RunInTransaction(() =>
        {
            var player = PlayerSave.Create(saveData.PlayerData);
            _db.Insert(player);
            player.PartyUID = player.UID;
            _db.Update(player);

            _db.Insert(CharaterSave.Create(player.UID, saveData.CharacterData));
            _db.Insert(CharaterAbilitySave.Create(player.UID, saveData.CharacterData.Ability));
            _db.Insert(PartySave.Create(player.UID, saveData.PartyData));

            var item = ItemDataCenter_Server.GetNewItemByItemID(1);
            saveData.CharacterData.BagItems.Add(item);
            _db.Insert(BagItemSave.Create(player.UID, item));
        });
    }

    public static void SaveDataToDB(long UID)
    {

    }

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