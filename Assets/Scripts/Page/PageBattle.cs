using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Random = UnityEngine.Random;
using System;
using System.IO;
using System.Reflection;

public class PageBattle : MonoBehaviour
{
    const string resourcePath = "Prefabs/PageBattle";
    [SerializeField] Text _area;
    [SerializeField] Text deep;
    [SerializeField] Button btnInto;
    [SerializeField] Button btnGoAhead;
    [SerializeField] Button btnAttack;
    [SerializeField] Button btnRest;
    [SerializeField] Button btnLeave;
    [SerializeField] Button btnShop;
    [SerializeField] ToggleGroup _enemies;
    [SerializeField] ScrollRect log;
    [SerializeField] ItemEnemy itemEnemy;

    [SerializeField] PanelShop panelShop;
    [SerializeField] PanelLog panelLog;

    readonly List<ItemEnemy> enemyList = new();
    ItemEnemy selectedEnemy;
    readonly List<string> LogList = new();
    enum BattleState
    {
        Continue,
        DefenderDead,
        AttackerDead,
    }

    public static void Create()
    {
        var page = ObjectPool.Get(Resources.Load<GameObject>(resourcePath).GetComponent<PageBattle>(), MainController.Instance.PageContent);

        MainController.Instance.SwitchPage(page);
    }

    void Awake()
    {
        btnInto.onClick.AddListener(OnInto);
        btnGoAhead.onClick.AddListener(OnGoAhead);
        btnAttack.onClick.AddListener(OnAttack);
        btnRest.onClick.AddListener(OnRest);
        btnLeave.onClick.AddListener(OnLeave);
        btnShop.onClick.AddListener(panelShop.OnShop);

        foreach (Transform enemy in _enemies.transform)
            Destroy(enemy.gameObject);
    }

    void OnEnable()
    {
        var requestData = new GetSaveDataRequest();
        ApiBridge.Send(requestData, OnGetSaveData);

        void OnGetSaveData(GetSaveDataResponse response)
        {
            var playerData = response.SaveData.Datas.PlayerData;
            var enemyData = response.SaveData.Datas.EnemyData;

            panelShop.gameObject.SetActive(false);
            var area = GameData.AreaData[playerData.Area];
            _area.text = area.Name;
            if (playerData.Deep == 0)
            {
                deep.text = "";
                btnInto.gameObject.SetActive(true);
                btnShop.gameObject.SetActive(true);
                btnGoAhead.gameObject.SetActive(false);
                btnRest.gameObject.SetActive(false);
                btnLeave.gameObject.SetActive(false);
                btnAttack.gameObject.SetActive(false);
            }
            else
            {
                deep.text = "深度 " + playerData.Deep;
                btnRest.gameObject.SetActive(true);
                btnLeave.gameObject.SetActive(true);
                btnInto.gameObject.SetActive(false);
                btnShop.gameObject.SetActive(false);

                if (enemyData.enemies != null && enemyData.enemies.Count > 0)
                {
                    foreach (var enemy in enemyData.enemies)
                    {
                        var obj = Instantiate(itemEnemy, _enemies.transform);
                        obj.Toggle.group = _enemies;
                        obj.Toggle.isOn = true;
                        obj.SetData(enemy);
                        enemyList.Add(obj);
                    }
                    var firstEnemy = enemyList.FirstOrDefault();
                    if (firstEnemy != null)
                        firstEnemy.Toggle.isOn = true;

                    btnGoAhead.gameObject.SetActive(false);
                    btnRest.gameObject.SetActive(false);
                    btnAttack.gameObject.SetActive(true);

                    var requestData = new GetBattleStatusRequest();
                    ApiBridge.Send(requestData, OnGetBattleState);
                }
                else
                {
                    btnGoAhead.gameObject.SetActive(true);
                    btnRest.gameObject.SetActive(true);
                    btnAttack.gameObject.SetActive(false);
                }
            }
        }
    }

    void OnDisable()
    {
        foreach (var enemy in enemyList)
            Destroy(enemy.gameObject);

        enemyList.Clear();
    }

    void OnInto()
    {
        var requestData = new SetAdventureActionRequest
        {
            AdventureAction = AdventureActionType.IntoArea,
            GameArea = 2
        };
        ApiBridge.Send(requestData, CallBack);

        void CallBack(SetAdventureActionResponse response)
        {
            btnGoAhead.gameObject.SetActive(true);
            btnRest.gameObject.SetActive(true);
            btnLeave.gameObject.SetActive(true);
            btnInto.gameObject.SetActive(false);
            btnShop.gameObject.SetActive(false);

            var area = GameData.AreaData[response.SaveData.Datas.PlayerData.Area];
            _area.text = area.Name;
            panelLog.ClearBattleLog();
            panelLog.SetLog("進入 " + _area.text);

            deep.text = "深度 " + response.SaveData.Datas.PlayerData.Deep;

            MainController.Instance.RefreshUI();
        }
    }

    void OnGoAhead()
    {
        var requestData = new SetAdventureActionRequest
        {
            AdventureAction = AdventureActionType.GoAhead
        };
        ApiBridge.Send(requestData, CallBack);

        void CallBack(SetAdventureActionResponse response)
        {
            deep.text = "深度 " + response.SaveData.Datas.PlayerData.Deep;

            if (response.SaveData.Datas.EnemyData.enemies.Count != 0)
            {
                OnEnemyAppear(response.SaveData.Datas.EnemyData.enemies);

                RunBattleVisuals(response.ActionResult.BattleResult);
            }
        }
    }

    void OnEnemyAppear(List<MobData> enemies)
    {
        foreach (var enemy in enemies)
        {
            var obj = ObjectPool.Get(itemEnemy, _enemies.transform);
            obj.Toggle.group = _enemies;
            obj.Toggle.isOn = true;
            obj.SetData(enemy);
            enemyList.Add(obj);
            panelLog.SetLog(enemy.CharacterData.Name + " 出現了！");
        }
        var firstEnemy = enemyList.FirstOrDefault();
        if (firstEnemy != null)
            firstEnemy.Toggle.isOn = true;

        btnGoAhead.gameObject.SetActive(false);
        btnRest.gameObject.SetActive(false);
        btnAttack.gameObject.SetActive(true);
    }

    void OnGetBattleState(GetBattleStatusResponse response)
    {
        var result = response.BattleResult;
        RunBattleVisuals(result);

        if (result != null)
        {
            if (result.IsAttackerDead && response.SaveData.Datas.CharacterData.Name == result.Attacker ||
                result.IsDefenderDead && response.SaveData.Datas.CharacterData.Name == result.Defenderer)
                LeaveDungon(response.SaveData.Datas.PlayerData.Area);
        }
    }

    void RunBattleVisuals(BattleResult result)
    {
        if (result != null)
        {
            ShowBattleLog(result);

            var requestData = new GetBattleStatusRequest();
            ApiBridge.Send(requestData, OnGetBattleState);
        }

        MainController.Instance.RefreshUI();
    }

    void OnLeave()
    {
        var requestData = new SetAdventureActionRequest
        {
            AdventureAction = AdventureActionType.Leave
        };
        ApiBridge.Send(requestData, CallBack);

        void CallBack(SetAdventureActionResponse response) => LeaveDungon(response.SaveData.Datas.PlayerData.Area);
    }

    void LeaveDungon(int areaID)
    {
        btnInto.gameObject.SetActive(true);
        btnShop.gameObject.SetActive(true);
        btnGoAhead.gameObject.SetActive(false);
        btnAttack.gameObject.SetActive(false);
        btnRest.gameObject.SetActive(false);
        btnLeave.gameObject.SetActive(false);

        var area = GameData.AreaData[areaID];
        _area.text = area.Name;
        deep.text = "";

        foreach (var enemy in enemyList)
            ObjectPool.Put(enemy);

        enemyList.Clear();

        panelLog.ClearBattleLog();
        panelLog.SetLog("離開迷宮，回到 " + _area.text);

        MainController.Instance.RefreshUI();
    }

    void OnRest()
    {
        var requestData = new SetAdventureActionRequest
        {
            AdventureAction = AdventureActionType.Rest
        };
        ApiBridge.Send(requestData, CallBack);

        void CallBack(SetAdventureActionResponse response)
        {
            var characterData = response.SaveData.Datas.CharacterData;
            var restResult = response.ActionResult.RestResult;

            panelLog.ClearBattleLog();

            panelLog.SetLog($"恢復了{restResult.RecoverHP}HP, {restResult.RecoverMP}MP, {restResult.RecoverSTA}體力");

            if (response.SaveData.Datas.EnemyData.enemies.Count != 0)
            {
                OnEnemyAppear(response.SaveData.Datas.EnemyData.enemies);
            }

            RunBattleVisuals(response.ActionResult.BattleResult);
        }
    }

    async void OnAttack()
    {
        selectedEnemy = enemyList.Find(x => x.Toggle.isOn);

        var requestData = new SetBattleActionRequest
        {
            BattleAction = BattleActionType.Attack,
            AttackTarget = selectedEnemy.Info
        };
        ApiBridge.Send(requestData, CallBack);

        void CallBack(SetBattleActionResponse response)
        {
            var battleResult = response.ActionResult.BattleResult;
            var target = enemyList.Find(x => x.Info.CharacterData.Name == battleResult.Defenderer);

            if (battleResult.IsDefenderDead)
            {
                enemyList.Remove(target);
                ObjectPool.Put(target);

                if (enemyList.Count == 0)
                {
                    btnGoAhead.gameObject.SetActive(true);
                    btnRest.gameObject.SetActive(true);
                    btnAttack.gameObject.SetActive(false);
                }
            }
            else
            {
                target.GetDamage(battleResult.LuckyEventDamage + battleResult.BattleDamage);
            }

            RunBattleVisuals(battleResult);
        }
    }

    void ShowBattleLog(BattleResult result)
    {
        if (result.IsLuckyEventTrigger)
        {
            switch (result.LuckyEventLevel)
            {
                case 2:
                    panelLog.SetLog($"{result.LuckyEventTarget}突然抽筋了!\n受到了{result.LuckyEventDamage}點傷害!", Color.magenta);
                    break;
                case 3:
                    panelLog.SetLog($"一輛大卡車疾駛而來，撞飛了{result.LuckyEventTarget}!\n受到了{result.LuckyEventDamage}點傷害!", Color.red);
                    break;
            }
        }

        if (result.IsDodge)
        {
            panelLog.SetLog($"{result.Defenderer}閃避了{result.Attacker}的攻擊!");
        }

        if (result.IsCritical)
        {
            panelLog.SetLog($"{result.Attacker}命中了要害!", Color.yellow);
        }

        if (result.BattleDamage > 0)
        {
            panelLog.SetLog($"{result.Attacker}對{result.Defenderer}造成了{result.BattleDamage}點傷害!", Color.gray);
        }

        if (result.IsAttackerDead)
        {
            panelLog.SetLog($"{result.Attacker}倒下了!");
        }

        if (result.IsDefenderDead)
        {
            panelLog.SetLog($"{result.Defenderer}倒下了!");
        }

        if (result.IsUnitLevelUp)
        {
            panelLog.SetLog($"{result.LevelUpUnit}升級了!");
        }
    }

    BattleState LuckyEventCheck(BattleData character1, BattleData character2)
    {
        // //先比誰觸發
        // bool isCharacter1Attack = PublicFunc.GetEffectiveStat(character1.LUK) > PublicFunc.GetEffectiveStat(character2.LUK);

        // // 攻擊者與被攻擊者的參考
        // int attackerLUK = isCharacter1Attack ? character1.LUK : character2.LUK;
        // int defenderLUK = isCharacter1Attack ? character2.LUK : character1.LUK;

        // int luckLevel = 0; // 0~3

        // for (int i = 0; i < 3; i++)
        // {
        //     if (PublicFunc.GetEffectiveStat(attackerLUK) > PublicFunc.GetEffectiveStat(defenderLUK * (luckLevel * 2 + 1) * 10))
        //         luckLevel++;
        //     else
        //         break; // 只要輸掉就結束
        // }

        // if (luckLevel > 0)
        // {
        //     int damage;
        //     var hitter = isCharacter1Attack ? character2 : character1;
        //     switch (luckLevel)
        //     {
        //         case 2:
        //             damage = Mathf.Max(PublicFunc.GetEffectiveStat(attackerLUK), 1 * 10 - PublicFunc.GetEffectiveStat(defenderLUK));
        //             panelLog.SetLog($"{hitter.Name}突然抽筋了!\n受到了{damage}點傷害!", Color.magenta);
        //             break;
        //         case 3:
        //             damage = Mathf.Max(PublicFunc.GetEffectiveStat(attackerLUK), 1 * 50 - PublicFunc.GetEffectiveStat(defenderLUK));
        //             panelLog.SetLog($"一輛大卡車疾駛而來，撞飛了{hitter.Name}!\n受到了{damage}點傷害!", Color.red);
        //             break;
        //         default:
        //             return BattleState.Continue;
        //     }

        //     if (DamageProcess(hitter, damage))
        //     {
        //         if (isCharacter1Attack)
        //             return BattleState.DefenderDead;
        //         else
        //             return BattleState.AttackerDead;
        //     }
        // }
        return BattleState.Continue;
    }

    async void RunDurability(bool isAttack)
    {
        // // 1️⃣ 建立武器清單（只需一次）
        // var weapons = typeof(EquipType.One_Hand_Weapon)
        //     .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
        //     .Concat(typeof(EquipType.Two_Hand_Weapon)
        //     .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
        //     .Select(f => f.GetValue(null)?.ToString())
        //     .Where(v => v != null)
        //     .ToHashSet();

        // List<ItemData> items = new();

        // // 2️⃣ 掃描所有裝備欄位
        // foreach (var field in typeof(EquipBase).GetFields(BindingFlags.Public | BindingFlags.Instance))
        // {
        //     long uid = (long)field.GetValue(_playerData.Equips);
        //     if (uid == 0)
        //         continue;

        //     var item = GameData.NowBagData.items
        //         .Find(x => x.uid == uid && ItemTypeCheck.IsEquipType(ItemBaseData.Get(x.itemID).type));
        //     if (item == null)
        //         continue;

        //     // 攻擊時扣武器耐久、防禦時扣防具耐久
        //     bool isWeapon = weapons.Contains(ItemBaseData.Get(item.itemID).type);
        //     if ((isAttack && isWeapon) || (!isAttack && !isWeapon))
        //         items.Add(item);
        // }

        // // 3️⃣ 處理耐久度扣減與移除
        // foreach (var item in items.ToList()) // ToList 避免修改集合時出錯
        // {
        //     // Debug.Log(_item.name);
        //     item.durability--;
        //     if (item.durability <= 0)
        //     {
        //         PublicFunc.UnloadEquip(item.uid);
        //         GameData.NowBagData.items.Remove(item);
        //         await panelLog.SetLogAsync($"{ItemBaseData.Get(item.itemID).name}毀損了", Color.yellow);
        //     }
        // }
    }

    bool DamageProcess(BattleData character, int damage)
    {
        character.HP -= damage;
        if (character.HP <= 0)
        {
            panelLog.SetLog($"{character.Name}倒下了!");
            return true;
        }
        else
        {
            return false;
        }
    }

    #region old

    // async UniTask RunPlayerAttack()
    // {
    //     selectedEnemy = enemyList.Find(x => x.toggle.isOn);
    //     if (selectedEnemy == null && enemyList.Count > 0)
    //     {
    //         selectedEnemy = enemyList.FirstOrDefault();
    //         selectedEnemy.toggle.isOn = true;
    //     }

    //     #region 幸運事件
    //     if (await LuckyEventCheck(selectedEnemy))
    //         return;
    //     #endregion

    //     #region 迴避判定
    //     if (!(PublicFunc.Dice(_playerData.Ability.ACC, 40) > PublicFunc.Dice(selectedEnemy.info.ability.EVA)))
    //     {
    //         await panelLog.SetLogAsync($"{selectedEnemy.info.name}閃避了{_playerData.PlayerName}的攻擊!");
    //         return;
    //     }
    //     #endregion

    //     var damageMulti = 1;
    //     var defenceMulti = 1;
    //     #region 爆擊判定
    //     if (PublicFunc.Dice(_playerData.Ability.CRIT) > PublicFunc.Dice(selectedEnemy.info.ability.EVA))
    //     {
    //         damageMulti = 2;
    //         defenceMulti = 0;
    //         await panelLog.SetLogAsync($"{_playerData.PlayerName}命中了要害!", Color.yellow);
    //     }
    //     #endregion

    //     var damage = PublicFunc.Dice(_playerData.Ability.ATK) * damageMulti - PublicFunc.Dice(selectedEnemy.info.ability.DEF) * defenceMulti;
    //     if (damage <= 0) damage = 1;

    //     RunDurability(true);

    //     await panelLog.SetLogAsync($"{_playerData.PlayerName}對{selectedEnemy.info.name}造成了{damage}點傷害!");
    //     if (await EnemyCheckDead(selectedEnemy, damage))
    //         return;
    // }

    // async UniTask RunEnemyAttack(ItemEnemy enemy)
    // {
    //     #region 幸運事件
    //     if (await LuckyEventCheck(enemy))
    //         return;
    //     #endregion

    //     #region 迴避判定
    //     if (!(PublicFunc.Dice(_playerData.Ability.EVA) < PublicFunc.Dice(enemy.info.ability.ACC, 40)))
    //     {
    //         await panelLog.SetLogAsync($"{_playerData.PlayerName}閃避了{enemy.info.name}的攻擊!", Color.gray);
    //         return;
    //     }
    //     #endregion

    //     var damageMulti = 1;
    //     var defenceMulti = 1;
    //     #region 爆擊判定
    //     if (PublicFunc.Dice(_playerData.Ability.EVA) < PublicFunc.Dice(enemy.info.ability.CRIT))
    //     {
    //         damageMulti = 2;
    //         defenceMulti = 0;
    //         await panelLog.SetLogAsync($"{enemy.info.name}命中了要害!", Color.yellow);
    //     }
    //     #endregion

    //     var damage = PublicFunc.Dice(enemy.info.ability.ATK) * damageMulti - PublicFunc.Dice(_playerData.Ability.DEF) * defenceMulti;
    //     if (damage <= 0)
    //         damage = 1;

    //     await panelLog.SetLogAsync($"{enemy.info.name}對{_playerData.PlayerName}造成了{damage}點傷害!", Color.gray);
    //     if (await PlayerGetDamage(damage))
    //         return;
    // }

    // async UniTask<bool> LuckyEventCheck(ItemEnemy enemy)
    // {
    //     //先比誰觸發
    //     var playerLUK = _playerData.Ability.LUK;
    //     var enemyLUK = enemy.info.ability.LUK;
    //     bool playerAttacks = PublicFunc.Dice(playerLUK) > PublicFunc.Dice(enemyLUK);

    //     // 攻擊者與被攻擊者的參考
    //     int attackerLUK = playerAttacks ? playerLUK : enemyLUK;
    //     int defenderLUK = playerAttacks ? enemyLUK : playerLUK;

    //     int luckLevel = 0; // 0~3

    //     for (int i = 0; i < 3; i++)
    //     {
    //         if (PublicFunc.Dice(attackerLUK) > PublicFunc.Dice(defenderLUK * (luckLevel * 2 + 1) * 10))
    //             luckLevel++;
    //         else
    //             break; // 只要輸掉就結束
    //     }

    //     if (luckLevel > 0)
    //     {
    //         int damage;
    //         var hitter = playerAttacks ? enemy.info.name : _playerData.PlayerName;
    //         switch (luckLevel)
    //         {
    //             case 2:
    //                 damage = Mathf.Max(PublicFunc.Dice(attackerLUK), 1 * 10 - PublicFunc.Dice(defenderLUK));
    //                 await panelLog.SetLogAsync($"{hitter}突然抽筋了!\n受到了{damage}點傷害!", Color.magenta);
    //                 break;
    //             case 3:
    //                 damage = Mathf.Max(PublicFunc.Dice(attackerLUK), 1 * 50 - PublicFunc.Dice(defenderLUK));
    //                 await panelLog.SetLogAsync($"一輛大卡車疾駛而來，撞飛了{hitter}!\n受到了{damage}點傷害!", Color.red);
    //                 break;
    //             default:
    //                 return false;
    //         }

    //         if (playerAttacks)
    //         {
    //             if (await EnemyCheckDead(enemy, damage))
    //                 return true;
    //         }
    //         else
    //         {
    //             if (await PlayerGetDamage(damage))
    //                 return true;
    //         }
    //     }
    //     return false;
    // }

    // async UniTask<bool> PlayerGetDamage(int damage)
    // {
    //     RunDurability(false);
    //     PublicFunc.SetHP(_playerData.CurrentHp - damage);

    //     if (_playerData.CurrentHp <= 0)
    //     {
    //         await panelLog.SetLogAsync($"{_playerData.PlayerName}倒下了!");
    //         PublicFunc.SetHP(1);
    //         OnLeave();
    //         return true;
    //     }
    //     else
    //     {
    //         return false;
    //     }
    // }


    #endregion

    // async UniTask RunSpeed()
    // {
    //     // Debug.Log("跑");
    //     while (enemyList.Count > 0)
    //     {
    //         var fastestEnemy = enemyList
    //             .Aggregate((max, next) => next.info.currentTp > max.info.currentTp ? next : max);

    //         if (_playerData.CurrentTp >= fastestEnemy.info.currentTp && _playerData.CurrentTp > GameData.tpCost)
    //         {
    //             PublicFunc.SaveData();
    //             MainController.Instance.RefreshUI();
    //             // #if UNITY_EDITOR
    //             if (_playerData.Effects.Find(x => x.type == EffectType.Buff.Berserk) != null)
    //             {
    //                 await panelLog.SetLogAsync($"{_playerData.PlayerName}因狂化無法控制", Color.yellow);
    //                 OnAttack();
    //             }
    //             // #endif
    //             return;
    //         }
    //         else if (fastestEnemy.info.currentTp > GameData.tpCost)
    //         {
    //             fastestEnemy.info.currentTp -= GameData.tpCost;
    //             await RunEnemyAttack(fastestEnemy);
    //             continue;
    //         }

    //         _playerData.CurrentTp += _playerData.Ability.SPD;
    //         enemyList.ForEach(x => x.info.currentTp += x.info.ability.SPD);
    //     }
    //     await panelLog.SetLogAsync("戰鬥結束");
    //     _playerData.CurrentTp = 0;
    //     btnGo.gameObject.SetActive(true);
    //     btnRest.gameObject.SetActive(true);
    //     btnAttack.gameObject.SetActive(false);

    //     PublicFunc.SaveData();
    //     MainController.Instance.RefreshUI();
    //     LogList.Clear();
    //     // #if UNITY_EDITOR
    //     //         await UniTask.NextFrame();
    //     //         OnGo();
    //     // #endif
    // }

    // void SaveLog()
    // {
    //     string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    //     string logPath = Path.Combine(desktopPath, "小破遊.log");

    //     var message = "玩家: " + _playerData.CurrentTp;
    //     string logContent = $"[{DateTime.Now}] {message}\n";
    //     LogList.Add(logContent);
    //     Debug.Log(logContent);

    //     foreach (var enemy in enemyList)
    //     {
    //         message = enemy.info.name + ": " + enemy.info.currentTp;
    //         logContent = $"[{DateTime.Now}] {message}\n";
    //         LogList.Add(logContent);
    //         Debug.Log(logContent);
    //     }
    //     File.WriteAllLines(logPath, LogList);
    // }
}