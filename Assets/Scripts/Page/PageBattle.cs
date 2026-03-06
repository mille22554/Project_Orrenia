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
    [SerializeField] Text area;
    [SerializeField] Text deep;
    [SerializeField] Button btnGo;
    [SerializeField] Button btnAttack;
    [SerializeField] Button btnRest;
    [SerializeField] Button btnLeave;
    [SerializeField] Button btnShop;
    [SerializeField] ToggleGroup enemies;
    [SerializeField] ScrollRect log;
    [SerializeField] ItemEnemy itemEnemy;

    [SerializeField] PanelShop panelShop;
    [SerializeField] PanelLog panelLog;

    readonly List<ItemEnemy> enemyList = new();
    ItemEnemy selectedEnemy;
    readonly List<string> LogList = new();
    PlayerData _playerData => GameData.NowPlayerData;
    EnemyData _enemyData => GameData.NowEnemyData;

    public static void Create()
    {
        var page = ObjectPool.Get(Resources.Load<GameObject>(resourcePath).GetComponent<PageBattle>(), MainController.Instance.PageContent);

        MainController.Instance.SwitchPage(page);
    }

    void Awake()
    {
        btnGo.onClick.AddListener(OnGo);
        btnAttack.onClick.AddListener(OnAttack);
        btnRest.onClick.AddListener(OnRest);
        btnLeave.onClick.AddListener(OnLeave);
        btnShop.onClick.AddListener(panelShop.OnShop);

        foreach (Transform enemy in enemies.transform)
            Destroy(enemy.gameObject);
    }

    void OnEnable()
    {
        if (GameData.gameData != null && _playerData != null)
        {
            panelShop.gameObject.SetActive(false);
            area.text = _playerData.Area;
            if (_playerData.Deep == 0)
            {
                deep.text = "";
                btnGo.gameObject.SetActive(true);
                btnShop.gameObject.SetActive(true);
                btnRest.gameObject.SetActive(false);
                btnLeave.gameObject.SetActive(false);
                btnAttack.gameObject.SetActive(false);
            }
            else
            {
                deep.text = "深度 " + _playerData.Deep;
                btnRest.gameObject.SetActive(true);
                btnLeave.gameObject.SetActive(true);
                btnShop.gameObject.SetActive(false);

                if (_enemyData.enemies != null && _enemyData.enemies.Count > 0)
                {
                    foreach (var enemy in _enemyData.enemies)
                    {
                        var obj = Instantiate(itemEnemy, enemies.transform);
                        obj.toggle.group = enemies;
                        obj.toggle.isOn = true;
                        obj.SetData(enemy);
                        enemyList.Add(obj);
                    }
                    enemyList[0].toggle.isOn = true;
                    btnGo.gameObject.SetActive(false);
                    btnRest.gameObject.SetActive(false);
                    btnAttack.gameObject.SetActive(true);
                    RunSpeed().Forget();
                }
                else
                {
                    btnGo.gameObject.SetActive(true);
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

    async void OnGo()
    {
        PublicFunc.SetCurrentSTA(_playerData.CurrentSTA - 1);

        foreach (var effectAction in _playerData.effectActions.ToList())
            effectAction.Invoke(false);

        if (_playerData.Deep == 0)
        {
            area.text = _playerData.Area = GameArea.Floor1;
            btnRest.gameObject.SetActive(true);
            btnLeave.gameObject.SetActive(true);
            btnShop.gameObject.SetActive(false);
            await panelLog.SetLog("進入 " + area.text);
        }

        _playerData.Deep += 1;
        deep.text = "深度 " + _playerData.Deep;

        panelLog.ClearBattleLog();
        OnEnemyAppear();

        PublicFunc.SaveData();
        PanelInfo.Instance.RefreshInfo();
        RunSpeed().Forget();
    }

    void OnEnemyAppear()
    {
        _enemyData.enemies = EnemySetting.SetEnemy(
            _playerData.Area,
            _playerData.Deep
        );
        foreach (var enemy in _enemyData.enemies)
        {
            var obj = ObjectPool.Get(itemEnemy, enemies.transform);
            obj.toggle.group = enemies;
            obj.toggle.isOn = true;
            obj.SetData(enemy);
            enemyList.Add(obj);
            panelLog.SetLog(enemy.name + " 出現了！").Forget();
        }
        var firstEnemy = enemyList.FirstOrDefault();
        if (firstEnemy != null)
            firstEnemy.toggle.isOn = true;

        btnGo.gameObject.SetActive(false);
        btnRest.gameObject.SetActive(false);
        btnAttack.gameObject.SetActive(true);
    }

    async void OnLeave()
    {
        area.text = _playerData.Area = GameArea.Home;
        _playerData.Deep = 0;
        deep.text = "";

        btnGo.gameObject.SetActive(true);
        btnShop.gameObject.SetActive(true);
        btnRest.gameObject.SetActive(false);
        btnLeave.gameObject.SetActive(false);
        btnAttack.gameObject.SetActive(false);

        foreach (var enemy in enemyList)
            ObjectPool.Put(enemy);

        _enemyData.enemies.Clear();
        enemyList.Clear();

        panelLog.ClearBattleLog();
        await panelLog.SetLog("離開迷宮，回到 " + area.text);
        PublicFunc.SetCurrentSTA(_playerData.ability.STA);

        foreach (var effectAction in _playerData.effectActions.ToList())
            effectAction.Invoke(false);

        PublicFunc.SetHP(_playerData.ability.HP);
        PublicFunc.SetMP(_playerData.ability.MP);

        PublicFunc.SaveData();
        PanelInfo.Instance.RefreshInfo();
    }

    async void OnRest()
    {
        var hp0 = _playerData.CurrentHp;
        var mp0 = _playerData.CurrentMp;
        var sta0 = _playerData.CurrentSTA;
        var prop = 0;
        while (
            _playerData.CurrentHp < _playerData.ability.HP ||
            _playerData.CurrentMp < _playerData.ability.MP ||
            _playerData.CurrentSTA < _playerData.ability.STA
        )
        {
            PublicFunc.SetHP(_playerData.CurrentHp + 1);
            PublicFunc.SetMP(_playerData.CurrentMp + 1);
            PublicFunc.SetCurrentSTA(_playerData.CurrentSTA + 1);

            prop = Dice(1, 3);
            if (prop > 0)
                break;
        }
        panelLog.ClearBattleLog();
        await panelLog.SetLog($"恢復了{_playerData.CurrentHp - hp0}HP, {_playerData.CurrentMp - mp0}MP, {_playerData.CurrentSTA - sta0}體力");

        if (prop > 0)
        {
            OnEnemyAppear();
            RunSpeed().Forget();
        }

        PublicFunc.SaveData();
        PanelInfo.Instance.RefreshInfo();
    }

    async void OnAttack()
    {
        if (_playerData.CurrentTp < GameData.tpCost)
            return;

        PublicFunc.SetCurrentSTA(_playerData.CurrentSTA - 1);
        _playerData.CurrentTp -= GameData.tpCost;
        await RunPlayerAttack();
        foreach (var effectAction in _playerData.effectActions.ToList())
            effectAction.Invoke(true);

        PublicFunc.SaveData();
        PanelInfo.Instance.RefreshInfo();
        RunSpeed().Forget();
    }

    int Dice(int times) => Dice(times, 20);
    int Dice(int times, int prop)
    {
        int count = 0;
        for (int i = 0; i < times; i++)
            if (Random.Range(0, 100) < prop) count++;

        return count;
    }

    async UniTask RunPlayerAttack()
    {
        selectedEnemy = enemyList.Find(x => x.toggle.isOn);
        if (selectedEnemy == null && enemyList.Count > 0)
        {
            selectedEnemy = enemyList.FirstOrDefault();
            selectedEnemy.toggle.isOn = true;
        }

        #region 幸運事件
        if (await LuckyEventCheck(selectedEnemy))
            return;
        #endregion

        #region 迴避判定
        if (!(Dice(_playerData.ability.ACC, 40) > Dice(selectedEnemy.info.ability.EVA)))
        {
            await panelLog.SetLog($"{selectedEnemy.info.name}閃避了{_playerData.PlayerName}的攻擊!");
            return;
        }
        #endregion

        var damageMulti = 1;
        var defenceMulti = 1;
        #region 爆擊判定
        if (Dice(_playerData.ability.CRIT) > Dice(selectedEnemy.info.ability.EVA))
        {
            damageMulti = 2;
            defenceMulti = 0;
            await panelLog.SetLog($"{_playerData.PlayerName}命中了要害!", Color.yellow);
        }
        #endregion

        var damage = Dice(_playerData.ability.ATK) * damageMulti - Dice(selectedEnemy.info.ability.DEF) * defenceMulti;
        if (damage <= 0) damage = 1;

        RunDurability(true);

        await panelLog.SetLog($"{_playerData.PlayerName}對{selectedEnemy.info.name}造成了{damage}點傷害!");
        if (await EnemyCheckDead(selectedEnemy, damage))
            return;
    }

    async UniTask RunEnemyAttack(ItemEnemy enemy)
    {
        #region 幸運事件
        if (await LuckyEventCheck(enemy))
            return;
        #endregion

        #region 迴避判定
        if (!(Dice(_playerData.ability.EVA) < Dice(enemy.info.ability.ACC, 40)))
        {
            await panelLog.SetLog($"{_playerData.PlayerName}閃避了{enemy.info.name}的攻擊!", Color.gray);
            return;
        }
        #endregion

        var damageMulti = 1;
        var defenceMulti = 1;
        #region 爆擊判定
        if (Dice(_playerData.ability.EVA) < Dice(enemy.info.ability.CRIT))
        {
            damageMulti = 2;
            defenceMulti = 0;
            await panelLog.SetLog($"{enemy.info.name}命中了要害!", Color.yellow);
        }
        #endregion

        var damage = Dice(enemy.info.ability.ATK) * damageMulti - Dice(_playerData.ability.DEF) * defenceMulti;
        if (damage <= 0)
            damage = 1;

        await panelLog.SetLog($"{enemy.info.name}對{_playerData.PlayerName}造成了{damage}點傷害!", Color.gray);
        if (await PlayerGetDamage(damage))
            return;
    }

    async UniTask<bool> LuckyEventCheck(ItemEnemy enemy)
    {
        //先比誰觸發
        var playerLUK = _playerData.ability.LUK;
        var enemyLUK = enemy.info.ability.LUK;
        bool playerAttacks = Dice(playerLUK) > Dice(enemyLUK);

        // 攻擊者與被攻擊者的參考
        int attackerLUK = playerAttacks ? playerLUK : enemyLUK;
        int defenderLUK = playerAttacks ? enemyLUK : playerLUK;

        int luckLevel = 0; // 0~3

        for (int i = 0; i < 3; i++)
        {
            if (Dice(attackerLUK) > Dice(defenderLUK * (luckLevel * 2 + 1) * 10))
                luckLevel++;
            else
                break; // 只要輸掉就結束
        }

        if (luckLevel > 0)
        {
            int damage;
            var hitter = playerAttacks ? enemy.info.name : _playerData.PlayerName;
            switch (luckLevel)
            {
                case 2:
                    damage = Mathf.Max(Dice(attackerLUK), 1 * 10 - Dice(defenderLUK));
                    await panelLog.SetLog($"{hitter}突然抽筋了!\n受到了{damage}點傷害!", Color.magenta);
                    break;
                case 3:
                    damage = Mathf.Max(Dice(attackerLUK), 1 * 50 - Dice(defenderLUK));
                    await panelLog.SetLog($"一輛大卡車疾駛而來，撞飛了{hitter}!\n受到了{damage}點傷害!", Color.red);
                    break;
                default:
                    return false;
            }

            if (playerAttacks)
            {
                if (await EnemyCheckDead(enemy, damage))
                    return true;
            }
            else
            {
                if (await PlayerGetDamage(damage))
                    return true;
            }
        }
        return false;
    }

    async UniTask<bool> PlayerGetDamage(int damage)
    {
        RunDurability(false);
        PublicFunc.SetHP(_playerData.CurrentHp - damage);

        if (_playerData.CurrentHp <= 0)
        {
            await panelLog.SetLog($"{_playerData.PlayerName}倒下了!");
            PublicFunc.SetHP(1);
            OnLeave();
            return true;
        }
        else
        {
            return false;
        }
    }

    async UniTask<bool> EnemyCheckDead(ItemEnemy enemy, int damage)
    {
        enemy.GetDamage(damage);

        if (enemy.info.currentHp <= 0)
        {
            await panelLog.SetLog($"{enemy.info.name}倒下了!");

            PublicFunc.SetEXP(_playerData.CurrentExp + 1 << (enemy.info.level - 1));
            if (_playerData.CurrentExp >= _playerData.MaxExp)
            {
                await panelLog.SetLog($"{_playerData.PlayerName}升級了!");
                _playerData.Level += 1;
                PublicFunc.SetEXP(_playerData.CurrentExp - _playerData.MaxExp);
                _playerData.MaxExp = (1 << (_playerData.Level - 1)) * 100;
                PublicFunc.SetAbilityPoint(_playerData.AbilityPoint + 6);
                _playerData.skillPoint += 1;
                PublicFunc.SetHP(_playerData.ability.HP);
                PublicFunc.SetMP(_playerData.ability.MP);
                PublicFunc.SetCurrentSTA(_playerData.ability.STA);
            }

            foreach (var drop in enemy.info.dropItems)
            {
                if (Dice(drop.prop) <= 0)
                    continue;

                var existing = GameData.NowBagData.items.Find(item => item.itemID == drop.item.id);

                if (ItemTypeCheck.IsEquipType(drop.item.type) || existing == null)
                {
                    GameData.NowBagData.items.Add(PublicFunc.GetItem(drop.item));
                    await panelLog.SetLog($"{_playerData.PlayerName}獲得了{drop.item.name}!");
                }
                else
                {
                    existing.count++;
                    await panelLog.SetLog($"{_playerData.PlayerName}獲得了{ItemBaseData.Get(existing.itemID).name}!");
                }
            }

            enemyList.Remove(enemy);
            _enemyData.enemies.Remove(enemy.info);
            ObjectPool.Put(enemy);
            return true;
        }
        else
        {
            return false;
        }
    }

    async void RunDurability(bool isAttack)
    {
        // 1️⃣ 建立武器清單（只需一次）
        var weapons = typeof(EquipType.One_Hand_Weapon)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Concat(typeof(EquipType.Two_Hand_Weapon)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
            .Select(f => f.GetValue(null)?.ToString())
            .Where(v => v != null)
            .ToHashSet();

        List<ItemData> items = new();

        // 2️⃣ 掃描所有裝備欄位
        foreach (var field in typeof(EquipBase).GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            long uid = (long)field.GetValue(_playerData.equips);
            if (uid == 0)
                continue;

            var item = GameData.NowBagData.items
                .Find(x => x.uid == uid && ItemTypeCheck.IsEquipType(ItemBaseData.Get(x.itemID).type));
            if (item == null)
                continue;

            // 攻擊時扣武器耐久、防禦時扣防具耐久
            bool isWeapon = weapons.Contains(ItemBaseData.Get(item.itemID).type);
            if ((isAttack && isWeapon) || (!isAttack && !isWeapon))
                items.Add(item);
        }

        // 3️⃣ 處理耐久度扣減與移除
        foreach (var item in items.ToList()) // ToList 避免修改集合時出錯
        {
            // Debug.Log(_item.name);
            item.durability--;
            if (item.durability <= 0)
            {
                PublicFunc.UnloadEquip(item.uid);
                GameData.NowBagData.items.Remove(item);
                await panelLog.SetLog($"{ItemBaseData.Get(item.itemID).name}毀損了", Color.yellow);
            }
        }
    }

    async UniTask RunSpeed()
    {
        // Debug.Log("跑");
        while (enemyList.Count > 0)
        {
            var fastestEnemy = enemyList
                .Aggregate((max, next) => next.info.currentTp > max.info.currentTp ? next : max);

            if (_playerData.CurrentTp >= fastestEnemy.info.currentTp && _playerData.CurrentTp > GameData.tpCost)
            {
                PublicFunc.SaveData();
                PanelInfo.Instance.RefreshInfo();
                // #if UNITY_EDITOR
                if (_playerData.effects.Find(x => x.type == EffectType.Buff.Berserk) != null)
                {
                    await panelLog.SetLog($"{_playerData.PlayerName}因狂化無法控制", Color.yellow);
                    OnAttack();
                }
                // #endif
                return;
            }
            else if (fastestEnemy.info.currentTp > GameData.tpCost)
            {
                fastestEnemy.info.currentTp -= GameData.tpCost;
                await RunEnemyAttack(fastestEnemy);
                continue;
            }

            _playerData.CurrentTp += _playerData.ability.SPD;
            enemyList.ForEach(x => x.info.currentTp += x.info.ability.SPD);
        }
        await panelLog.SetLog("戰鬥結束");
        _playerData.CurrentTp = 0;
        btnGo.gameObject.SetActive(true);
        btnRest.gameObject.SetActive(true);
        btnAttack.gameObject.SetActive(false);

        PublicFunc.SaveData();
        PanelInfo.Instance.RefreshInfo();
        LogList.Clear();
        // #if UNITY_EDITOR
        //         await UniTask.NextFrame();
        //         OnGo();
        // #endif
    }

    void SaveLog()
    {
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string logPath = Path.Combine(desktopPath, "小破遊.log");

        var message = "玩家: " + _playerData.CurrentTp;
        string logContent = $"[{DateTime.Now}] {message}\n";
        LogList.Add(logContent);
        Debug.Log(logContent);

        foreach (var enemy in enemyList)
        {
            message = enemy.info.name + ": " + enemy.info.currentTp;
            logContent = $"[{DateTime.Now}] {message}\n";
            LogList.Add(logContent);
            Debug.Log(logContent);
        }
        File.WriteAllLines(logPath, LogList);
    }
}