using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SQLite;

public static class SaveDataCenter
{
    static SQLiteConnection _db;
    static readonly ConcurrentQueue<Action> _dbActionQueue = new();
    static readonly CancellationTokenSource _cts = new();

    public static void Init()
    {
        var path = GameData_Server.SaveDataBasePath();

        _db = new SQLiteConnection(path);

        _db.Execute("PRAGMA journal_mode = WAL;");
        _db.Execute("PRAGMA synchronous = NORMAL;");

        // 初始化時建立所有表 (若不存在則建立)
        _db.CreateTable<BagItemSave>();
        _db.CreateTable<CharacterSave>();
        _db.CreateTable<CharaterAbilitySave>();
        _db.CreateTable<EffectSave>();
        _db.CreateTable<EquipSave>();
        _db.CreateTable<MobSave>();
        _db.CreateTable<PartySave>();
        _db.CreateTable<PlayerSave>();
        _db.CreateTable<SkillSave>();

        Task.Run(ProcessQueueLoop, _cts.Token);
    }

    static async Task ProcessQueueLoop()
    {
        while (!_cts.Token.IsCancellationRequested)
        {
            if (_dbActionQueue.IsEmpty)
            {
                // 如果沒資料，就小睡一下（100ms），避免浪費 CPU
                await Task.Delay(100);
                continue;
            }

            try
            {
                _db.RunInTransaction(() =>
                {
                    while (_dbActionQueue.TryDequeue(out var action))
                    {
                        action.Invoke(); // 嚴格按照加入隊列的順序執行
                    }
                });
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[DB] 批次寫入失敗: {ex.Message}");
            }
        }
    }

    public static void CreateSaveData_New(string account)
    {
        _db.RunInTransaction(() =>
        {
            var player = PlayerSave.Create(PlayerContextData.CreateDefault());
            _db.Insert(player);
            player.Account = account;
            player.PartyUID = player.UID;
            _db.Update(player);

            _db.Insert(CharacterSave.Create(CharacterData.CreateDefault(player.UID)));
            _db.Insert(CharaterAbilitySave.Create(AbilityBase.CreateDefault(player.UID)));
            _db.Insert(PartySave.Create(PartyData.CreateDefault(player.UID)));

            var item = ItemDataCenter_Server.GetNewItemByItemID(1, player.UID);
            _db.Insert(BagItemSave.Create(item));
        });
    }

    public static void NewDataToDB<T>(T data) where T : class, IDBTable, new() => _dbActionQueue.Enqueue(() => _db.Insert(data));
    public static void SaveDataToDB<T>(T data) where T : class, IDBTable, new() => _dbActionQueue.Enqueue(() => _db.Update(data));
    public static void RemoveDataFromDB<T>(T data) where T : class, IDBTable, new() => _dbActionQueue.Enqueue(() => _db.Delete(data));

    public static PlayerContextData GetPlayerData(string account)
    {
        var player = _db.Table<PlayerSave>().Where(x => x.Account == account).FirstOrDefault();
        return PlayerSave.GetData(player);
    }

    public static CharacterData GetCharacterData(string account)
    {
        var player = _db.Table<PlayerSave>().Where(x => x.Account == account).FirstOrDefault();
        return GetCharacterData(player.UID);
    }
    public static CharacterData GetCharacterData(long UID)
    {
        return CharacterSave.GetData(_db.Find<CharacterSave>(UID));
    }

    public static List<CharacterData> GetPartyMembers(long partyUID)
    {
        var players = _db.Table<PlayerSave>().Where(x => x.PartyUID == partyUID).ToList();
        var members = new List<CharacterData>();
        foreach (var player in players)
        {
            members.Add(GetCharacterData(player.UID));
        }
        return members;
    }

    public static List<CharacterData> GetPartyEnemies(long partyUID)
    {
        var mobs = _db.Table<MobSave>().Where(x => x.PartyUID == partyUID).ToList();
        var members = new List<CharacterData>();
        foreach (var mob in mobs)
        {
            members.Add(GetCharacterData(mob.UID));
        }
        return members;
    }

    public static PartyData GetPartyData(string account)
    {
        var player = _db.Table<PlayerSave>().Where(x => x.Account == account).FirstOrDefault();
        return GetPartyData(player.PartyUID);
    }
    public static PartyData GetPartyData(long UID)
    {
        return PartySave.GetData(_db.Find<PartySave>(UID));
    }

    public static List<EffectData> GetEffects(string account)
    {
        var player = _db.Table<PlayerSave>().Where(x => x.Account == account).FirstOrDefault();
        return GetEffects(player.UID);
    }
    public static List<EffectData> GetEffects(long owner)
    {
        return _db.Table<EffectSave>().Where(x => x.Owner == owner).Select(x => EffectSave.GetData(x)).ToList();
    }

    public static List<SkillData> GetSkills(string account)
    {
        var player = _db.Table<PlayerSave>().Where(x => x.Account == account).FirstOrDefault();
        return GetSkills(player.UID);
    }
    public static List<SkillData> GetSkills(long owner)
    {
        return _db.Table<SkillSave>().Where(x => x.Owner == owner).Select(x => SkillSave.GetData(x)).ToList();
    }

    public static List<MobData> GetMobs(long partyUID)
    {
        return _db.Table<MobSave>().Where(x => x.PartyUID == partyUID).Select(x => MobSave.GetData(x)).ToList();
    }

    public static List<BagItemData> GetEquips(long owner)
    {
        var equips = _db.Table<EquipSave>().Where(x => x.Owner == owner).ToList();
        var items = new List<BagItemData>();
        foreach (var equip in equips)
        {
            items.Add(GetBagItemData(equip.UID));
        }
        return items;
    }

    public static BagItemData GetBagItemData(long UID)
    {
        return BagItemSave.GetData(_db.Find<BagItemSave>(UID));
    }

    public static PlayerSaveDataFormat CreateSaveData()
    {
        var saveData = new PlayerSaveDataFormat
        {
            version = GameData_Server.version,
            Datas = Datas.CreateDefault()
        };
        // saveData.Datas.CharacterData.BagItems.Add(ItemDataCenter_Server.GetNewItemByItemID(1));

        return saveData;
    }

    public static void SaveData(string account)
    {
        var path = GameData_Server.PlayerSaveDataPath(account);
        // Debug.Log($"儲存遊戲資料到 {path}");
        File.WriteAllText(path, JsonConvert.SerializeObject(GameData_Server.NowPlayers[account]));
    }
}

public interface IDBTable { }