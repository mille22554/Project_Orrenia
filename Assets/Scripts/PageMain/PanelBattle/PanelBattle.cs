using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Random = UnityEngine.Random;
using System;
using System.IO;
using System.Reflection;

public class PanelBattle : MonoBehaviour
{
    public Text area;
    public Text deep;
    public Button btnGo;
    public Button btnAttack;
    public Button btnRest;
    public Button btnLeave;
    public Button btnShop;
    public ToggleGroup enemies;
    public ScrollRect log;
    public ItemEnemy itemEnemy;

    public PanelShop panelShop;
    public PanelLog panelLog;

    private readonly List<ItemEnemy> enemyList = new();
    private ItemEnemy selectedEnemy;
    private readonly List<string> LogList = new();

    private void Start()
    {
        btnGo.onClick.AddListener(OnGo);
        btnAttack.onClick.AddListener(OnAttack);
        btnRest.onClick.AddListener(OnRest);
        btnLeave.onClick.AddListener(OnLeave);
        btnShop.onClick.AddListener(panelShop.OnShop);

        foreach (Transform enemy in enemies.transform)
            Destroy(enemy.gameObject);
    }

    private void OnEnable()
    {
        if (GameData.gameData != null && GameData.NowPlayerData != null)
        {
            panelShop.gameObject.SetActive(false);
            area.text = GameData.NowPlayerData.area;
            if (GameData.NowPlayerData.deep == 0)
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
                deep.text = "深度 " + GameData.NowPlayerData.deep;
                btnRest.gameObject.SetActive(true);
                btnLeave.gameObject.SetActive(true);
                btnShop.gameObject.SetActive(false);

                if (GameData.NowEnemyData.enemies != null && GameData.NowEnemyData.enemies.Count > 0)
                {
                    foreach (var enemy in GameData.NowEnemyData.enemies)
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

    private void OnDisable()
    {
        foreach (var enemy in enemyList)
            Destroy(enemy.gameObject);
        enemyList.Clear();
    }

    private void OnDestroy()
    {
        btnGo.onClick.RemoveListener(OnGo);
        btnAttack.onClick.RemoveListener(OnAttack);
        btnRest.onClick.RemoveListener(OnRest);
        btnLeave.onClick.RemoveListener(OnLeave);
        btnShop.onClick.RemoveListener(panelShop.OnShop);
    }

    private async void OnGo()
    {
        GameData.NowPlayerData.CurrentSTA -= 1;
        if (GameData.NowPlayerData.deep == 0)
        {
            area.text = GameData.NowPlayerData.area = GameArea.Floor1;
            btnRest.gameObject.SetActive(true);
            btnLeave.gameObject.SetActive(true);
            btnShop.gameObject.SetActive(false);
            await panelLog.SetLog("進入 " + area.text);
        }

        GameData.NowPlayerData.deep += 1;
        deep.text = "深度 " + GameData.NowPlayerData.deep;

        OnEnemyAppear();

        PublicFunc.SaveData();
        RunSpeed().Forget();
    }

    private void OnEnemyAppear()
    {
        GameData.NowEnemyData.enemies = EnemySetting.SetEnemy(
            GameData.NowPlayerData.area,
            GameData.NowPlayerData.deep
        );
        foreach (var enemy in GameData.NowEnemyData.enemies)
        {
            var obj = Instantiate(itemEnemy, enemies.transform);
            obj.toggle.group = enemies;
            obj.toggle.isOn = true;
            obj.SetData(enemy);
            enemyList.Add(obj);
            panelLog.SetLog(enemy.name + " 出現了！").Forget();
        }
        enemyList[0].toggle.isOn = true;
        btnGo.gameObject.SetActive(false);
        btnRest.gameObject.SetActive(false);
        btnAttack.gameObject.SetActive(true);
    }

    private async void OnLeave()
    {
        area.text = GameData.NowPlayerData.area = GameArea.Home;
        GameData.NowPlayerData.deep = 0;
        deep.text = "";

        btnGo.gameObject.SetActive(true);
        btnShop.gameObject.SetActive(true);
        btnRest.gameObject.SetActive(false);
        btnLeave.gameObject.SetActive(false);
        btnAttack.gameObject.SetActive(false);

        foreach (var enemy in enemyList)
            Destroy(enemy.gameObject);
        GameData.NowEnemyData.enemies.Clear();
        enemyList.Clear();

        await panelLog.SetLog("離開迷宮，回到 " + area.text);
        GameData.NowPlayerData.CurrentSTA = GameData.NowPlayerData.ability.STA;

        foreach (var effectAction in GameData.NowPlayerData.effectActions.ToList())
            effectAction.Invoke();

        GameData.NowPlayerData.CurrentHp = GameData.NowPlayerData.ability.HP;
        GameData.NowPlayerData.CurrentMp = GameData.NowPlayerData.ability.MP;

        PublicFunc.SaveData();
    }

    private async void OnRest()
    {
        var hp0 = GameData.NowPlayerData.CurrentHp;
        var mp0 = GameData.NowPlayerData.CurrentMp;
        var sta0 = GameData.NowPlayerData.CurrentSTA;
        var prop = 0;
        while (
            GameData.NowPlayerData.CurrentHp < GameData.NowPlayerData.ability.HP ||
            GameData.NowPlayerData.CurrentMp < GameData.NowPlayerData.ability.MP ||
            GameData.NowPlayerData.CurrentSTA < GameData.NowPlayerData.ability.STA
        )
        {
            GameData.NowPlayerData.CurrentHp++;
            GameData.NowPlayerData.CurrentMp++;
            GameData.NowPlayerData.CurrentSTA++;

            prop = Dice(1, 3);
            if (prop > 0) break;
        }
        await panelLog.SetLog($"恢復了{GameData.NowPlayerData.CurrentHp - hp0}HP, {GameData.NowPlayerData.CurrentMp - mp0}MP, {GameData.NowPlayerData.CurrentSTA - sta0}體力");

        if (prop > 0)
        {
            OnEnemyAppear();
            RunSpeed().Forget();
        }

        PublicFunc.SaveData();
    }

    private async void OnAttack()
    {
        if (GameData.NowPlayerData.currentTp < GameData.tpCost) return;

        GameData.NowPlayerData.CurrentSTA -= 1;
        GameData.NowPlayerData.currentTp -= GameData.tpCost;
        await RunPlayerAttack();
        foreach (var effectAction in GameData.NowPlayerData.effectActions.ToList())
            effectAction.Invoke();

        PublicFunc.SaveData();
        RunSpeed().Forget();
    }

    private int Dice(int times) => Dice(times, 20);
    private int Dice(int times, int prop)
    {
        int count = 0;
        for (int i = 0; i < times; i++)
            if (Random.Range(0, 100) < prop) count++;

        return count;
    }

    private async UniTask RunPlayerAttack()
    {
        selectedEnemy = enemyList.Find(x => x.toggle.isOn);
        if (selectedEnemy == null && enemyList.Count > 0)
        {
            selectedEnemy = enemyList[0];
            selectedEnemy.toggle.isOn = true;
        }

        #region 幸運事件
        if (await LuckyEventCheck(selectedEnemy)) return;
        #endregion

        #region 迴避判定
        if (!(Dice(GameData.NowPlayerData.ability.ACC, 40) > Dice(selectedEnemy.info.ability.EVA)))
        {
            await panelLog.SetLog($"{selectedEnemy.info.name}閃避了{GameData.NowPlayerData.name}的攻擊!");
            return;
        }
        #endregion

        var damageMulti = 1;
        var defenceMulti = 1;
        #region 爆擊判定
        if (Dice(GameData.NowPlayerData.ability.CRIT) > Dice(selectedEnemy.info.ability.EVA))
        {
            damageMulti = 2;
            defenceMulti = 0;
            await panelLog.SetLog($"{GameData.NowPlayerData.name}命中了要害!", Color.yellow);
        }
        #endregion

        var damage = Dice(GameData.NowPlayerData.ability.ATK) * damageMulti - Dice(selectedEnemy.info.ability.DEF) * defenceMulti;
        if (damage <= 0) damage = 1;

        RunDurability(true);

        await panelLog.SetLog($"{GameData.NowPlayerData.name}對{selectedEnemy.info.name}造成了{damage}點傷害!");
        if (await EnemyCheckDead(selectedEnemy, damage)) return;
    }

    private async UniTask RunEnemyAttack(ItemEnemy enemy)
    {
        #region 幸運事件
        if (await LuckyEventCheck(enemy)) return;
        #endregion

        #region 迴避判定
        if (!(Dice(GameData.NowPlayerData.ability.EVA) < Dice(enemy.info.ability.ACC, 40)))
        {
            await panelLog.SetLog($"{GameData.NowPlayerData.name}閃避了{enemy.info.name}的攻擊!", Color.gray);
            return;
        }
        #endregion

        var damageMulti = 1;
        var defenceMulti = 1;
        #region 爆擊判定
        if (Dice(GameData.NowPlayerData.ability.EVA) < Dice(enemy.info.ability.CRIT))
        {
            damageMulti = 2;
            defenceMulti = 0;
            await panelLog.SetLog($"{enemy.info.name}命中了要害!", Color.yellow);
        }
        #endregion

        var damage = Dice(enemy.info.ability.ATK) * damageMulti - Dice(GameData.NowPlayerData.ability.DEF) * defenceMulti;
        if (damage <= 0) damage = 1;

        await panelLog.SetLog($"{enemy.info.name}對{GameData.NowPlayerData.name}造成了{damage}點傷害!", Color.gray);
        if (await PlayerGetDamage(damage)) return;
    }

    private async UniTask<bool> LuckyEventCheck(ItemEnemy enemy)
    {
        //先比誰觸發
        var playerLUK = GameData.NowPlayerData.ability.LUK;
        var enemyLUK = enemy.info.ability.LUK;
        bool playerAttacks = Dice(playerLUK) > Dice(enemyLUK);

        // 攻擊者與被攻擊者的參考
        int attackerLUK = playerAttacks ? playerLUK : enemyLUK;
        int defenderLUK = playerAttacks ? enemyLUK : playerLUK;

        int luckLevel = 0; // 0~3

        for (int i = 0; i < 3; i++)
        {
            if (Dice(attackerLUK) > Dice(defenderLUK * (luckLevel * 2 + 1) * 10)) luckLevel++;
            else break; // 只要輸掉就結束
        }

        if (luckLevel > 0)
        {
            int damage;
            var hitter = playerAttacks ? enemy.info.name : GameData.NowPlayerData.name;
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

            if (playerAttacks) { if (await EnemyCheckDead(enemy, damage)) return true; }
            else { if (await PlayerGetDamage(damage)) return true; }
        }
        return false;
    }

    private async UniTask<bool> PlayerGetDamage(int damage)
    {
        RunDurability(false);
        GameData.NowPlayerData.CurrentHp -= damage;

        if (GameData.NowPlayerData.CurrentHp <= 0)
        {
            await panelLog.SetLog($"{GameData.NowPlayerData.name}倒下了!");
            GameData.NowPlayerData.CurrentHp = 1;
            OnLeave();
            return true;
        }
        else return false;
    }

    private async UniTask<bool> EnemyCheckDead(ItemEnemy enemy, int damage)
    {
        enemy.GetDamage(damage);

        if (enemy.info.currentHp <= 0)
        {
            await panelLog.SetLog($"{enemy.info.name}倒下了!");

            GameData.NowPlayerData.CurrentExp += 1 << (enemy.info.level - 1);
            if (GameData.NowPlayerData.CurrentExp >= GameData.NowPlayerData.maxExp)
            {
                await panelLog.SetLog($"{GameData.NowPlayerData.name}升級了!");
                GameData.NowPlayerData.level += 1;
                GameData.NowPlayerData.CurrentExp -= GameData.NowPlayerData.maxExp;
                GameData.NowPlayerData.maxExp = (1 << (GameData.NowPlayerData.level - 1)) * 100;
                GameData.NowPlayerData.AbilityPoint += 6;
                GameData.NowPlayerData.skillPoint += 1;
                GameData.NowPlayerData.CurrentHp = GameData.NowPlayerData.ability.HP;
                GameData.NowPlayerData.CurrentMp = GameData.NowPlayerData.ability.MP;
                GameData.NowPlayerData.CurrentSTA = GameData.NowPlayerData.ability.STA;
            }

            foreach (var drop in enemy.info.dropItems)
            {
                if (Dice(drop.prop) <= 0) continue;

                var existing = GameData.NowBagData.items.Find(item => item.id == drop.item.id);

                if (ItemTypeCheck.IsEquipType(drop.item.type) || existing == null)
                {
                    GameData.NowBagData.items.Add(PublicFunc.GetItem(drop.item));
                    await panelLog.SetLog($"{GameData.NowPlayerData.name}獲得了{drop.item.name}!");
                }
                else
                {
                    existing.count++;
                    await panelLog.SetLog($"{GameData.NowPlayerData.name}獲得了{existing.name}!");
                }
            }

            enemyList.Remove(enemy);
            Destroy(enemy.gameObject);
            GameData.NowEnemyData.enemies.Remove(enemy.info);
            return true;
        }
        else return false;
    }

    private async void RunDurability(bool isAttack)
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
            long uid = (long)field.GetValue(GameData.NowPlayerData.equips);
            if (uid == 0) continue;

            var item = GameData.NowBagData.items
                .Find(x => x.uid == uid && ItemTypeCheck.IsEquipType(x.type));
            if (item == null) continue;

            // 攻擊時扣武器耐久、防禦時扣防具耐久
            bool isWeapon = weapons.Contains(item.type);
            if ((isAttack && isWeapon) || (!isAttack && !isWeapon))
                items.Add(item);
        }

        // 3️⃣ 處理耐久度扣減與移除
        foreach (var _item in items.ToList()) // ToList 避免修改集合時出錯
        {
            // Debug.Log(_item.name);
            _item.durability--;
            if (_item.durability <= 0)
            {
                PublicFunc.UnloadEquip(_item.uid);
                GameData.NowBagData.items.Remove(_item);
                await panelLog.SetLog($"{_item.name}毀損了", Color.yellow);
            }
        }
    }

    private async UniTask RunSpeed()
    {
        // Debug.Log("跑");
        while (enemyList.Count > 0)
        {
            var fastestEnemy = enemyList
                .Aggregate((max, next) => next.info.currentTp > max.info.currentTp ? next : max);

            if (GameData.NowPlayerData.currentTp >= fastestEnemy.info.currentTp && GameData.NowPlayerData.currentTp > GameData.tpCost)
            {
                PublicFunc.SaveData();
                // #if UNITY_EDITOR
                if (GameData.NowPlayerData.effects.Find(x => x.type == EffectType.Buff.Berserk) != null)
                {
                    await panelLog.SetLog($"{GameData.NowPlayerData.name}因狂化無法控制", Color.yellow);
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

            GameData.NowPlayerData.currentTp += GameData.NowPlayerData.ability.SPD;
            enemyList.ForEach(x => x.info.currentTp += x.info.ability.SPD);
        }
        await panelLog.SetLog("戰鬥結束");
        GameData.NowPlayerData.currentTp = 0;
        btnGo.gameObject.SetActive(true);
        btnRest.gameObject.SetActive(true);
        btnAttack.gameObject.SetActive(false);

        PublicFunc.SaveData();
        LogList.Clear();
        // #if UNITY_EDITOR
        //         await UniTask.NextFrame();
        //         OnGo();
        // #endif
    }

    private void SaveLog()
    {
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string logPath = Path.Combine(desktopPath, "小破遊.log");

        var message = "玩家: " + GameData.NowPlayerData.currentTp;
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