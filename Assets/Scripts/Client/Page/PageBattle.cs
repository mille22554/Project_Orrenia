using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

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
    [SerializeField] ItemEnemy itemEnemy;

    [SerializeField] PanelShop panelShop;
    [SerializeField] PanelLog panelLog;

    readonly List<ItemEnemy> enemyList = new();
    ItemEnemy selectedEnemy;
    Dictionary<int, AreaData> _areaDatas;

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
        ApiBridge.Send(requestData, CallBack);

        void CallBack(GetSaveDataResponse response)
        {
            var playerData = response.SaveData.Datas.PlayerData;
            var enemyData = response.SaveData.Datas.EnemyData;

            panelShop.gameObject.SetActive(false);
            var area = GetAreaData(playerData.Area);
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

                if (enemyData.Enemies != null && enemyData.Enemies.Count > 0)
                {
                    foreach (var enemy in enemyData.Enemies)
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

                    OnGetBattleState();
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
            AdventureAction = EAdventureActionType.IntoArea,
            GameArea = 2
        };
        ApiBridge.Send(requestData, CallBack);

        void CallBack(SetAdventureActionResponse response)
        {
            var datas = response.SaveData.Datas;

            btnGoAhead.gameObject.SetActive(true);
            btnRest.gameObject.SetActive(true);
            btnLeave.gameObject.SetActive(true);
            btnInto.gameObject.SetActive(false);
            btnShop.gameObject.SetActive(false);

            var area = GetAreaData(datas.PlayerData.Area);
            _area.text = area.Name;
            panelLog.ClearBattleLog();
            panelLog.SetLog("進入 " + _area.text);

            deep.text = "深度 " + datas.PlayerData.Deep;

            MainController.Instance.RefreshUI(datas.CharacterData, response.FullAbility);
        }
    }

    void OnGoAhead()
    {
        var requestData = new SetAdventureActionRequest
        {
            AdventureAction = EAdventureActionType.GoAhead
        };
        ApiBridge.Send(requestData, CallBack);

        void CallBack(SetAdventureActionResponse response)
        {
            var datas = response.SaveData.Datas;
            var enemies = datas.EnemyData.Enemies;

            deep.text = "深度 " + datas.PlayerData.Deep;

            if (enemies.Count != 0)
            {
                OnEnemyAppear(enemies);

                RunBattleVisuals(response.ActionResult.BattleResult, datas, response.FullAbility);
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

    void OnGetBattleState()
    {
        var requestData = new GetBattleStatusRequest();
        ApiBridge.Send(requestData, CallBack);

        void CallBack(GetBattleStatusResponse response)
        {
            var datas = response.SaveData.Datas;
            var result = response.BattleResult;

            RunBattleVisuals(result, datas);
        }
    }

    void RunBattleVisuals(BattleResult result, Datas datas) => RunBattleVisuals(result, datas, null);
    void RunBattleVisuals(BattleResult result, Datas datas, FullAbilityBase fullAbility)
    {
        MainController.Instance.RefreshUI(datas.CharacterData, fullAbility);

        if (result != null)
        {
            ShowBattleLog(result);

            if (result.IsAttackerDead && datas.CharacterData.Name != result.Attacker)
            {
                var target = enemyList.Find(x => x.Info.CharacterData.Name == result.Attacker);

                MobDeadCheck(result, target);
            }

            if (result.IsAttackerDead && datas.CharacterData.Name == result.Attacker ||
                result.IsDefenderDead && datas.CharacterData.Name == result.Defenderer)
            {
                LeaveDungon(datas.PlayerData.Area, datas.CharacterData, fullAbility);
            }
            else
            {
                OnGetBattleState();
            }

        }
    }

    void OnLeave()
    {
        var requestData = new SetAdventureActionRequest
        {
            AdventureAction = EAdventureActionType.Leave
        };
        ApiBridge.Send(requestData, CallBack);

        void CallBack(SetAdventureActionResponse response)
        {
            var datas = response.SaveData.Datas;
            LeaveDungon(datas.PlayerData.Area, datas.CharacterData, response.FullAbility);
        }
    }

    void LeaveDungon(int areaID, CharacterData characterData, FullAbilityBase fullAbility)
    {
        btnInto.gameObject.SetActive(true);
        btnShop.gameObject.SetActive(true);
        btnGoAhead.gameObject.SetActive(false);
        btnAttack.gameObject.SetActive(false);
        btnRest.gameObject.SetActive(false);
        btnLeave.gameObject.SetActive(false);

        var area = GetAreaData(areaID);
        _area.text = area.Name;
        deep.text = "";

        foreach (var enemy in enemyList)
            ObjectPool.Put(enemy);

        enemyList.Clear();

        panelLog.ClearBattleLog();
        panelLog.SetLog("離開迷宮，回到 " + _area.text);

        MainController.Instance.RefreshUI(characterData, fullAbility);
    }

    void OnRest()
    {
        var requestData = new SetAdventureActionRequest
        {
            AdventureAction = EAdventureActionType.Rest
        };
        ApiBridge.Send(requestData, CallBack);

        void CallBack(SetAdventureActionResponse response)
        {
            var datas = response.SaveData.Datas;
            var characterData = datas.CharacterData;
            var restResult = response.ActionResult.RestResult;

            panelLog.ClearBattleLog();

            panelLog.SetLog($"恢復了{restResult.RecoverHP}HP, {restResult.RecoverMP}MP, {restResult.RecoverSTA}體力");

            if (datas.EnemyData.Enemies.Count != 0)
            {
                OnEnemyAppear(datas.EnemyData.Enemies);
            }

            RunBattleVisuals(response.ActionResult.BattleResult, datas, response.FullAbility);
        }
    }

    async void OnAttack()
    {
        selectedEnemy = enemyList.Find(x => x.Toggle.isOn);

        var requestData = new SetBattleActionRequest
        {
            BattleAction = EBattleActionType.Attack,
            AttackTarget = selectedEnemy.Info
        };
        ApiBridge.Send(requestData, CallBack);

        void CallBack(SetBattleActionResponse response)
        {
            var battleResult = response.ActionResult.BattleResult;
            var target = enemyList.Find(x => x.Info.CharacterData.Name == battleResult.Defenderer);

            MobDeadCheck(battleResult, target);

            RunBattleVisuals(battleResult, response.SaveData.Datas, response.FullAbility);
        }
    }

    void MobDeadCheck(BattleResult result, ItemEnemy enemy)
    {
        if (result.IsAttackerDead && enemy.Info.CharacterData.Name == result.Attacker ||
            result.IsDefenderDead && enemy.Info.CharacterData.Name == result.Defenderer)
        {
            enemyList.Remove(enemy);
            ObjectPool.Put(enemy);

            if (enemyList.Count == 0)
            {
                btnGoAhead.gameObject.SetActive(true);
                btnRest.gameObject.SetActive(true);
                btnAttack.gameObject.SetActive(false);
            }
        }
        else
        {
            if (result.IsLuckyEventTrigger)
            {
                if (result.LuckyEventTarget == enemy.Info.CharacterData.Name)
                    enemy.GetDamage(result.LuckyEventDamage);
            }
            enemy.GetDamage(result.BattleDamage);
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

        foreach (var breakEquip in result.BreakEquips)
        {
            panelLog.SetLog($"{breakEquip}毀損了", Color.yellow);
        }

        foreach (var dropItem in result.DropItems)
        {
            panelLog.SetLog($"{result.Attacker}獲得了{dropItem}!");
        }
    }

    AreaData GetAreaData(int areaID)
    {
        var requestData = new GetAreaDataRequest();
        ApiBridge.Send(requestData, CallBack);

        _areaDatas.TryGetValue(areaID, out var areaData);
        return areaData;

        void CallBack(GetAreaDataResponse response)
        {
            _areaDatas = response.AreaData;
        }
    }
}