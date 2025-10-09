using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Random = UnityEngine.Random;
using System;
using System.IO;

public class PanelBattle : MonoBehaviour
{
    public Text area;
    public Text deep;
    public Button btnGo;
    public Button btnAttack;
    public Button btnLeave;
    public Button btnShop;
    public ToggleGroup enemies;
    public ScrollRect log;
    public ItemEnemy itemEnemy;
    public Text itemLog;
    public GameObject block;

    public Button shop;
    public Toggle toggleBuy;
    public Toggle toggleSell;
    public Text itemName;
    public Text type;
    public Text price;
    public Text description;
    public Text ability;
    public Text gold;
    public Button btnTrade;
    public Text textTrade;
    public ScrollRect itemList;
    public ShopItem shopItem;

    private ToggleGroup toggleItems;
    private readonly List<ShopItem> shopItemList = new();
    private ShopItem selectedShopItem;
    private readonly List<ItemEnemy> enemyList = new();
    private ItemEnemy selectedEnemy;
    private const int tpCost = 10000;
    private readonly List<string> LogList = new();

    private void Start()
    {
        toggleItems = itemList.content.GetComponent<ToggleGroup>();
        btnGo.onClick.AddListener(OnGo);
        btnAttack.onClick.AddListener(OnAttack);
        btnLeave.onClick.AddListener(OnLeave);
        btnShop.onClick.AddListener(OnShop);
        shop.onClick.AddListener(OnShop);
        toggleBuy.onValueChanged.AddListener(OnSwitchBuy);
        toggleSell.onValueChanged.AddListener(OnSwitchSell);
        btnTrade.onClick.AddListener(OnTrade);

        block.SetActive(false);
        foreach (Transform enemy in enemies.transform)
            Destroy(enemy.gameObject);
        foreach (Transform child in log.content)
            Destroy(child.gameObject);
    }

    private void OnEnable()
    {
        if (GameData.gameData != null && GameData.NowPlayerData != null)
        {
            shop.gameObject.SetActive(false);
            area.text = GameData.NowPlayerData.area;
            toggleSell.isOn = true;
            toggleSell.isOn = false;
            toggleBuy.isOn = true;
            toggleBuy.isOn = false;
            if (GameData.NowPlayerData.deep == 0)
            {
                deep.text = "";
                btnGo.gameObject.SetActive(true);
                btnShop.gameObject.SetActive(true);
                btnLeave.gameObject.SetActive(false);
                btnAttack.gameObject.SetActive(false);
                log.gameObject.SetActive(false);
            }
            else
            {
                deep.text = "深度 " + GameData.NowPlayerData.deep;
                btnLeave.gameObject.SetActive(true);
                btnShop.gameObject.SetActive(false);
                log.gameObject.SetActive(true);

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
                    btnAttack.gameObject.SetActive(true);
                    RunSpeed().Forget();
                }
                else
                {
                    btnGo.gameObject.SetActive(true);
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
        btnLeave.onClick.RemoveListener(OnLeave);
        btnShop.onClick.RemoveListener(OnShop);
        shop.onClick.RemoveListener(OnShop);
        toggleBuy.onValueChanged.RemoveListener(OnSwitchBuy);
        toggleSell.onValueChanged.RemoveListener(OnSwitchSell);
        btnTrade.onClick.RemoveListener(OnTrade);
    }

    private void OnShop()
    {
        if (shop.gameObject.activeSelf)
        {
            shop.gameObject.SetActive(false);
        }
        else
        {
            ResetBagInfo();
            gold.text = GameData.NowPlayerData.gold.ToString();
            toggleBuy.isOn = true;
            shop.gameObject.SetActive(true);
        }
    }

    private void OnSwitchBuy(bool isOn)
    {
        if (!isOn) return;

        ResetBagInfo();
        textTrade.text = "購買";
        foreach (var item in shopItemList)
            Destroy(item.gameObject);
        shopItemList.Clear();

        foreach (var itemInfo in GameShopItem.list)
        {
            var item = Instantiate(shopItem, itemList.content);
            item.SetInfo(itemInfo);
            item.refreshBagInfo = RefreshBagInfo;
            item.toggle.group = toggleItems;
            item.toggle.isOn = true;
            item.toggle.isOn = false;
            shopItemList.Add(item);
        }
    }

    private void OnSwitchSell(bool isOn)
    {
        if (!isOn) return;

        ResetBagInfo();
        textTrade.text = "販賣";
        foreach (var item in shopItemList)
            Destroy(item.gameObject);
        shopItemList.Clear();

        foreach (var itemInfo in GameData.NowBagData.items)
        {
            var item = Instantiate(shopItem, itemList.content);
            item.SetInfo(itemInfo);
            item.refreshBagInfo = RefreshBagInfo;
            item.toggle.group = toggleItems;
            item.toggle.isOn = true;
            item.toggle.isOn = false;
            shopItemList.Add(item);
        }
    }

    private void OnTrade()
    {
        if (selectedShopItem == null) return;

        if (toggleBuy.isOn)
        {
            if (GameData.NowPlayerData.gold > selectedShopItem.info.price)
            {
                GameData.NowPlayerData.gold -= selectedShopItem.info.price;

                var existing = GameData.NowBagData.items.Find(item => item.id == selectedShopItem.info.id);
                if (ItemTypeCheck.IsEquipType(selectedShopItem.info.type) || existing == null)
                {
                    ItemData newItem = new();
                    GameItem.CopyFields(selectedShopItem.info, newItem);
                    newItem.uid = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    GameData.NowBagData.items.Add(newItem);
                }
                else existing.count++;
            }
        }
        else
        {
            GameData.NowPlayerData.gold += selectedShopItem.info.price / 2;

            var existing = GameData.NowBagData.items.Find(item => item.id == selectedShopItem.info.id);
            if (existing != null)
            {
                existing.count--;
            }
            if (ItemTypeCheck.IsEquipType(selectedShopItem.info.type) || existing?.count == 0)
            {
                GameData.NowBagData.items.Remove(selectedShopItem.info);
                shopItemList.Remove(selectedShopItem);
                Destroy(selectedShopItem.gameObject);
            }
            else
                selectedShopItem.SetInfo(existing);
        }
        gold.text = GameData.NowPlayerData.gold.ToString();

        PublicFunc.SaveData();
    }

    private void RefreshBagInfo(ShopItem item)
    {
        selectedShopItem = item;
        itemName.text = item.info.name;
        type.text = item.info.type;
        price.transform.parent.gameObject.SetActive(true);
        var itemPrice = toggleBuy.isOn ? item.info.price : item.info.price / 2;
        price.text = $"{itemPrice}";
        description.text = item.info.description;
        if (ItemTypeCheck.IsEquipType(item.info.type))
            ability.text = item.info.GetAbilityString();
    }

    private void ResetBagInfo()
    {
        itemName.text = "";
        type.text = "";
        price.transform.parent.gameObject.SetActive(false);
        price.text = "";
        description.text = "";
        ability.text = "";
    }

    private async void OnGo()
    {
        if (GameData.NowPlayerData.deep == 0)
        {
            area.text = GameData.NowPlayerData.area = GameArea.Floor1;
            btnLeave.gameObject.SetActive(true);
            log.gameObject.SetActive(true);
            await SetLog("進入 " + area.text);
        }

        GameData.NowPlayerData.deep += 1;
        deep.text = "深度 " + GameData.NowPlayerData.deep;

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
            SetLog(enemy.name + " 出現了！").Forget();
        }
        enemyList[0].toggle.isOn = true;
        btnGo.gameObject.SetActive(false);
        btnAttack.gameObject.SetActive(true);

        PublicFunc.SaveData();
        RunSpeed().Forget();
    }

    private async void OnLeave()
    {
        area.text = GameData.NowPlayerData.area = GameArea.Home;
        GameData.NowPlayerData.deep = 0;
        deep.text = "";

        btnGo.gameObject.SetActive(true);
        btnShop.gameObject.SetActive(true);
        btnLeave.gameObject.SetActive(false);
        btnAttack.gameObject.SetActive(false);

        foreach (var enemy in enemyList)
            Destroy(enemy.gameObject);
        GameData.NowEnemyData.enemies.Clear();
        enemyList.Clear();

        await SetLog("離開迷宮，回到 " + area.text);
        GameData.NowPlayerData.CurrentHp = GameData.NowPlayerData.ability.HP;
        GameData.NowPlayerData.CurrentMp = GameData.NowPlayerData.ability.MP;

        PublicFunc.SaveData();
    }

    private async void OnAttack()
    {
        SaveLog();
        if (GameData.NowPlayerData.currentTp < tpCost) return;

        GameData.NowPlayerData.currentTp -= tpCost;
        await RunPlayerAttack();

        PublicFunc.SaveData();
        RunSpeed().Forget();
    }

    private int Dice(int times) => Dice(times, 20);
    private int Dice(int times, int prop)
    {
        int count = 0;
        for (int i = 0; i < times; i++)
        {
            if (Random.Range(0, 100) < prop)
            {
                count++;
            }
        }
        return count;
    }

    private async UniTask RunPlayerAttack()
    {
        selectedEnemy = enemyList.Find(x => x.toggle.isOn);

        #region 幸運事件
        if (await LuckyEventCheck(selectedEnemy)) return;
        #endregion

        #region 迴避判定
        if (!(Dice(GameData.NowPlayerData.ability.ACC, 40) > Dice(selectedEnemy.info.ability.EVA)))
        {
            await SetLog($"{selectedEnemy.info.name}閃避了{GameData.NowPlayerData.name}的攻擊!");
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
            await SetLog($"{GameData.NowPlayerData.name}命中了要害!", Color.yellow);
        }
        #endregion

        var damage = Dice(GameData.NowPlayerData.ability.ATK) * damageMulti - Dice(selectedEnemy.info.ability.DEF) * defenceMulti;
        if (damage <= 0) damage = 1;

        await SetLog($"{GameData.NowPlayerData.name}對{selectedEnemy.info.name}造成了{damage}點傷害!");
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
            await SetLog($"{GameData.NowPlayerData.name}閃避了{enemy.info.name}的攻擊!", Color.gray);
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
            await SetLog($"{enemy.info.name}命中了要害!", Color.yellow);
        }
        #endregion

        var damage = Dice(enemy.info.ability.ATK) * damageMulti - Dice(GameData.NowPlayerData.ability.DEF) * defenceMulti;
        if (damage <= 0) damage = 1;

        await SetLog($"{enemy.info.name}對{GameData.NowPlayerData.name}造成了{damage}點傷害!", Color.gray);
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
            int damage = 0;
            var hitter = playerAttacks ? enemy.info.name : GameData.NowPlayerData.name;
            switch (luckLevel)
            {
                case 2:
                    damage = Mathf.Max(Dice(attackerLUK), 1 * 5 - Dice(defenderLUK));
                    await SetLog($"{hitter}突然抽筋了!\n受到了{damage}點傷害!", Color.magenta);
                    break;
                case 3:
                    damage = Mathf.Max(Dice(attackerLUK), 1 * 10 - Dice(defenderLUK));
                    await SetLog($"一輛大卡車疾駛而來，撞飛了{hitter}!\n受到了{damage}點傷害!", Color.red);
                    break;
            }

            if (playerAttacks) { if (await EnemyCheckDead(enemy, damage)) return true; }
            else { if (await PlayerGetDamage(damage)) return true; }
        }
        return false;
    }

    private async UniTask<bool> PlayerGetDamage(int damage)
    {
        GameData.NowPlayerData.CurrentHp -= damage;

        if (GameData.NowPlayerData.CurrentHp <= 0)
        {
            await SetLog($"{GameData.NowPlayerData.name}倒下了!");
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
            await SetLog($"{enemy.info.name}倒下了!");

            GameData.NowPlayerData.CurrentExp += 1 << (enemy.info.level - 1);
            if (GameData.NowPlayerData.CurrentExp >= GameData.NowPlayerData.maxExp)
            {
                await SetLog($"{GameData.NowPlayerData.name}升級了!");
                GameData.NowPlayerData.level += 1;
                GameData.NowPlayerData.CurrentExp -= GameData.NowPlayerData.maxExp;
                GameData.NowPlayerData.maxExp = (1 << (GameData.NowPlayerData.level - 1)) * 100;
                GameData.NowPlayerData.AbilityPoint += 6;
                GameData.NowPlayerData.skillPoint += 1;
                GameData.NowPlayerData.CurrentHp = GameData.NowPlayerData.ability.HP;
                GameData.NowPlayerData.CurrentMp = GameData.NowPlayerData.ability.MP;
            }

            foreach (var drop in enemy.info.dropItems)
            {
                if (Dice(drop.prop) <= 0) continue;

                var existing = GameData.NowBagData.items.Find(item => item.id == drop.item.id);

                if (ItemTypeCheck.IsEquipType(drop.item.type) || existing == null)
                {
                    ItemData newItem = new();
                    GameItem.CopyFields(drop.item, newItem);
                    newItem.uid = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    GameData.NowBagData.items.Add(newItem);
                    await SetLog($"{GameData.NowPlayerData.name}獲得了{drop.item.name}!");
                    continue;
                }
                else
                {
                    existing.count++;
                    await SetLog($"{GameData.NowPlayerData.name}獲得了{existing.name}!");
                }
            }

            Destroy(enemy.gameObject);
            enemyList.Remove(enemy);
            GameData.NowEnemyData.enemies.Remove(enemy.info);
            return true;
        }
        else return false;
    }

    private async UniTask SetLog(string message) => await SetLog(message, Color.white);
    private async UniTask SetLog(string message, Color color)
    {
        block.SetActive(true);
        var textLog = Instantiate(itemLog, log.content);
        textLog.text = message;
        textLog.color = color;

        await UniTask.Yield();
        log.verticalNormalizedPosition = 0;
        await UniTask.WaitForSeconds(0.1f);
        block.SetActive(false);
    }

    private async UniTask RunSpeed()
    {
        while (enemyList.Count > 0)
        {
            var fastestEnemy = enemyList
                .Aggregate((max, next) => next.info.currentTp > max.info.currentTp ? next : max);

            if (GameData.NowPlayerData.currentTp >= fastestEnemy.info.currentTp && GameData.NowPlayerData.currentTp > tpCost)
            {
                PublicFunc.SaveData();
                return;
            }
            else if (fastestEnemy.info.currentTp > tpCost)
            {
                fastestEnemy.info.currentTp -= tpCost;
                await RunEnemyAttack(fastestEnemy);
                continue;
            }

            GameData.NowPlayerData.currentTp += GameData.NowPlayerData.ability.SPD;
            enemyList.ForEach(x => x.info.currentTp += x.info.ability.SPD);
        }
        await SetLog("戰鬥結束");
        GameData.NowPlayerData.currentTp = 0;
        btnGo.gameObject.SetActive(true);
        btnAttack.gameObject.SetActive(false);

        PublicFunc.SaveData();
        LogList.Clear();
    }

    private void SaveLog()
    {
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string logPath = Path.Combine(desktopPath, "小破遊.log");

        var message = "玩家: " + GameData.NowPlayerData.currentTp;
        string logContent = $"[{DateTime.Now}] {message}\n";
        LogList.Add(logContent);

        foreach (var enemy in enemyList)
        {
            message = enemy.info.name + ": " + enemy.info.currentTp;
            logContent = $"[{DateTime.Now}] {message}\n";
            LogList.Add(logContent);
        }
        File.WriteAllLines(logPath, LogList);
    }
}