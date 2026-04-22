using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

[RequireComponent(typeof(CanvasGroup))]
public class PageBattle : MonoBehaviour
{
    static PageBattle _ins;
    const string resourcePath = "Prefabs/PageBattle";

    [SerializeField] Text _area;
    [SerializeField] Text deep;
    [SerializeField] Button btnInto;
    [SerializeField] Button btnGoAhead;
    [SerializeField] Button btnAttack;
    [SerializeField] Button btnSkill;
    [SerializeField] Button btnRest;
    [SerializeField] Button btnLeave;
    [SerializeField] Button btnShop;
    [SerializeField] ToggleGroup _enemies;
    [SerializeField] ItemEnemy itemEnemy;

    [SerializeField] PanelShop panelShop;
    [SerializeField] PanelLog panelLog;
    [SerializeField] PanelSkill _panelSkill;

    CanvasGroup _canvasGroup;

    readonly List<ItemEnemy> enemyList = new();
    ItemEnemy selectedEnemy;

    public static void Create()
    {
        if (_ins == null || !_ins.gameObject.activeSelf)
        {
            _ins = ObjectPool.Get(Resources.Load<GameObject>(resourcePath).GetComponent<PageBattle>(), MainController.Instance.PageContent);

            MainController.Instance.SwitchPage(_ins);
        }
    }

    void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();

        btnInto.onClick.AddListener(OnInto);
        btnGoAhead.onClick.AddListener(OnGoAhead);
        btnAttack.onClick.AddListener(OnAttack);
        btnSkill.onClick.AddListener(_panelSkill.OnSwitch);
        btnRest.onClick.AddListener(OnRest);
        btnLeave.onClick.AddListener(OnLeave);
        btnShop.onClick.AddListener(panelShop.OnShop);

        _panelSkill.SetInfo(OnSkill);

        foreach (Transform enemy in _enemies.transform)
            Destroy(enemy.gameObject);
    }

    void OnEnable()
    {
        _canvasGroup.alpha = 0;

        var requestData = new GetSaveDataRequest();
        ApiBridge.Send(requestData, CallBack);
        PanelLoading.Create(PanelLoading.BGType.Full);

        void CallBack(GetSaveDataResponse response)
        {
            var partyData = response.SaveData.PartyData;

            panelShop.gameObject.SetActive(false);
            var area = DataCenter.GetAreaData(partyData.Area);
            _area.text = area.Name;
            if (partyData.Deep == 0)
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
                deep.text = "深度 " + partyData.Deep;
                btnRest.gameObject.SetActive(true);
                btnLeave.gameObject.SetActive(true);
                btnInto.gameObject.SetActive(false);
                btnShop.gameObject.SetActive(false);

                if (partyData.Enemies != null && partyData.Enemies.Count > 0)
                {
                    foreach (var enemy in partyData.Enemies)
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

            _canvasGroup.alpha = 1;
            PanelLoading.Close();
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
        ApiBridge.Send(requestData, response => LeaderCheck(response, CallBack));
        PanelLoading.Create(PanelLoading.BGType.Half);

        void CallBack(SetAdventureActionResponse response)
        {
            var partyData = response.PartyData;

            btnGoAhead.gameObject.SetActive(true);
            btnRest.gameObject.SetActive(true);
            btnLeave.gameObject.SetActive(true);
            btnInto.gameObject.SetActive(false);
            btnShop.gameObject.SetActive(false);

            var area = DataCenter.GetAreaData(partyData.Area);
            _area.text = area.Name;
            panelLog.ClearBattleLog();
            panelLog.SetLog("進入 " + _area.text);

            deep.text = "深度 " + partyData.Deep;

            MainController.Instance.RefreshUI(response.Datas.CharacterData, response.FullAbility);
        }
    }

    void LeaderCheck(SetAdventureActionResponse response, Action<SetAdventureActionResponse> callback)
    {
        if (response.IsLeader)
        {
            callback?.Invoke(response);
        }
        else
        {
            ItemToastMessage.Create("請透過隊長行動");
        }
        PanelLoading.Close();
    }

    void OnGoAhead()
    {
        var requestData = new SetAdventureActionRequest
        {
            AdventureAction = EAdventureActionType.GoAhead
        };
        ApiBridge.Send(requestData, response => LeaderCheck(response, CallBack));
        PanelLoading.Create(PanelLoading.BGType.Half);

        void CallBack(SetAdventureActionResponse response)
        {
            var fullAbility = response.FullAbility;
            var actionResult = response.ActionResult;
            var effectResult = actionResult.EffectResult;
            var datas = response.Datas;
            var partyData = response.PartyData;
            var characterData = datas.CharacterData;
            var enemies = partyData.Enemies;

            deep.text = "深度 " + partyData.Deep;

            var playerEffectResult = effectResult.Results.Find(x => x.CharacterName == characterData.Name);
            if (playerEffectResult != null)
            {
                ShowEffectLog(playerEffectResult);
                if (playerEffectResult.IsDead)
                {
                    LeaveDungon(partyData.Area, characterData, fullAbility);
                    return;
                }
            }

            if (enemies.Count != 0)
                OnEnemyAppear(enemies);

            RunBattleVisuals(actionResult.BattleResult, effectResult, datas, fullAbility);
        }
    }

    void OnEnemyAppear(List<MobData> enemies)
    {
        panelLog.SetLine();
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
            var battleResult = response.BattleResult;
            var effectResult = response.EffectResult;

            RunBattleVisuals(battleResult, effectResult, datas);
        }
    }

    void RunBattleVisuals(BattleResult battleResult, EffectResult effectResult, Datas datas) => RunBattleVisuals(battleResult, effectResult, datas, null);
    void RunBattleVisuals(BattleResult battleResult, EffectResult effectResult, Datas datas, FullAbilityBase fullAbility)
    {
        MainController.Instance.RefreshUI(datas.CharacterData, fullAbility);

        if (battleResult != null)
        {
            ShowBattleLog(battleResult, datas.CharacterData);
            var attackerEffectResult = effectResult.Results.Find(x => x.CharacterName == battleResult.Attacker);
            ShowEffectLog(attackerEffectResult);

            foreach (var result in battleResult.Results)
            {
                var targetMob = battleResult.Attacker != datas.CharacterData.Name ? battleResult.Attacker : result.Defenderer;
                var target = enemyList.Find(x => x.Info.CharacterData.Name == targetMob);

                MobDeadCheck(battleResult, attackerEffectResult, target);
            }

            if (battleResult.IsAttackerDead && datas.CharacterData.Name == battleResult.Attacker ||
                battleResult.Results.Any(x => x.IsDefenderDead && x.Defenderer == datas.CharacterData.Name) ||
                (attackerEffectResult != null && attackerEffectResult.IsDead && datas.CharacterData.Name == attackerEffectResult.CharacterName))
            {
                LeaveDungon(datas.PartyData.Area, datas.CharacterData, fullAbility);
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
        ApiBridge.Send(requestData, response => LeaderCheck(response, CallBack));
        PanelLoading.Create(PanelLoading.BGType.Half);

        void CallBack(SetAdventureActionResponse response)
        {
            var datas = response.Datas;
            LeaveDungon(response.PartyData.Area, datas.CharacterData, response.FullAbility);
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

        var area = DataCenter.GetAreaData(areaID);
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
        ApiBridge.Send(requestData, response => LeaderCheck(response, CallBack));
        PanelLoading.Create(PanelLoading.BGType.Half);

        void CallBack(SetAdventureActionResponse response)
        {
            var partyData = response.PartyData;
            var enemies = partyData.Enemies;
            var datas = response.Datas;
            var characterData = datas.CharacterData;
            var actionResult = response.ActionResult;
            var restResult = actionResult.RestResult;

            panelLog.ClearBattleLog();

            panelLog.SetLog($"恢復了{restResult.RecoverHP}HP, {restResult.RecoverMP}MP, {restResult.RecoverSTA}體力");

            foreach (var result in actionResult.EffectResult.Results)
            {
                if (result.CharacterName == characterData.Name)
                {
                    ShowEffectLog(result);

                    if (result.IsDead)
                    {
                        LeaveDungon(partyData.Area, datas.CharacterData, response.FullAbility);
                        return;
                    }
                }
            }

            if (enemies.Count != 0)
                OnEnemyAppear(enemies);

            RunBattleVisuals(actionResult.BattleResult, actionResult.EffectResult, datas, response.FullAbility);
        }
    }

    void OnAttack()
    {
        selectedEnemy = enemyList.Find(x => x.Toggle.isOn);

        var requestData = new SetBattleActionRequest
        {
            BattleAction = EBattleActionType.Attack,
            ActionTarget = new() { selectedEnemy.Info.CharacterData }
        };
        ApiBridge.Send(requestData, CallBack);
        PanelLoading.Create(PanelLoading.BGType.None);

        void CallBack(SetBattleActionResponse response)
        {
            var actionResult = response.ActionResult;
            var battleResult = actionResult.BattleResult;
            var effectResult = actionResult.EffectResult;

            RunBattleVisuals(battleResult, effectResult, response.SaveData.Datas, response.FullAbility);
            PanelLoading.Close();
        }
    }

    void OnSkill(ESkillID skillID)
    {
        selectedEnemy = enemyList.Find(x => x.Toggle.isOn);
        if (selectedEnemy == null)
            return;

        var requestData = new SetBattleActionRequest
        {
            BattleAction = EBattleActionType.Attack,
            ActionTarget = new() { selectedEnemy.Info.CharacterData },
            SkillID = skillID
        };
        ApiBridge.Send(requestData, CallBack);
        PanelLoading.Create(PanelLoading.BGType.None);

        void CallBack(SetBattleActionResponse response)
        {
            var actionResult = response.ActionResult;
            var battleResult = actionResult.BattleResult;
            var effectResult = actionResult.EffectResult;

            RunBattleVisuals(battleResult, effectResult, response.SaveData.Datas, response.FullAbility);
            PanelLoading.Close();
        }
    }

    void MobDeadCheck(BattleResult battleResult, EffectResult.Result effectResult, ItemEnemy enemy)
    {
        if (battleResult == null || effectResult == null || enemy == null)
            return;

        if (battleResult.IsAttackerDead && enemy.Info.CharacterData.Name == battleResult.Attacker ||
            battleResult.Results.Any(x => x.IsDefenderDead && x.Defenderer == enemy.Info.CharacterData.Name) ||
            effectResult.IsDead && enemy.Info.CharacterData.Name == effectResult.CharacterName)
        {
            enemyList.Remove(enemy);
            ObjectPool.Put(enemy);

            if (enemyList.Count == 0)
            {
                btnGoAhead.gameObject.SetActive(true);
                btnRest.gameObject.SetActive(true);
                btnAttack.gameObject.SetActive(false);
            }
            else
            {
                enemyList.FirstOrDefault().Toggle.isOn = true;
            }
        }
        else
        {
            foreach (var result in battleResult.Results)
            {
                if (result.IsLuckyEventTrigger)
                {
                    if (result.LuckyEventTarget == enemy.Info.CharacterData.Name)
                        enemy.GetDamage(result.LuckyEventDamage);
                }

                if (result.Defenderer == enemy.Info.CharacterData.Name)
                    enemy.GetDamage(result.BattleDamage);
            }

            if (effectResult.CharacterName == enemy.Info.CharacterData.Name)
            {
                foreach (var info in effectResult.Infos)
                {
                    if (info.MofityAbility.HP != 0)
                        enemy.GetDamage(-1 * info.MofityAbility.HP);
                }
            }
        }
    }

    void ShowBattleLog(BattleResult battleResult, CharacterData characterData)
    {
        if (battleResult == null || battleResult.Results.Count == 0)
            return;

        panelLog.SetLine();
        var commonColor = battleResult.Attacker == characterData.Name ? Color.white : Color.gray;

        if (battleResult.IsAttakerIncapacitated)
            panelLog.SetLog($"{battleResult.Attacker}因{battleResult.IncapacitatedEffect}而無法行動!", commonColor);

        if (battleResult.Results.Count == 0)
            return;

        foreach (var result in battleResult.Results)
        {
            if (result.IsLuckyEventTrigger)
            {
                switch (result.LuckyEventLevel)
                {
                    case 2:
                        panelLog.SetLog($"{result.LuckyEventTarget}突然抽筋了!\n受到了{result.LuckyEventDamage:0}點傷害!", Color.magenta);
                        break;
                    case 3:
                        panelLog.SetLog($"一輛大卡車疾駛而來，撞飛了{result.LuckyEventTarget}!\n受到了{result.LuckyEventDamage:0}點傷害!", Color.red);
                        break;
                }
            }
        }

        if (battleResult.IsSkill)
            panelLog.SetLog($"{battleResult.Attacker}發動了{battleResult.SkillName}!", commonColor);

        foreach (var result in battleResult.Results)
        {
            if (result.IsDodge)
                panelLog.SetLog($"{result.Defenderer}閃避了{battleResult.Attacker}的攻擊!", commonColor);

            if (result.IsCritical)
                panelLog.SetLog($"{battleResult.Attacker}命中了要害!", Color.yellow);

            if (result.IsCounter)
            {
                panelLog.SetLog($"{result.Defenderer}反擊了!", commonColor);
                panelLog.SetLog($"{result.Defenderer}對{battleResult.Attacker}造成了{result.BattleDamage:0}點傷害!", commonColor);
            }
            else
            {
                if (result.BattleDamage > 0)
                    panelLog.SetLog($"{battleResult.Attacker}對{result.Defenderer}造成了{result.BattleDamage:0}點傷害!", commonColor);
            }

            if (result.IsDefenderDead)
            {
                panelLog.SetLog($"{result.Defenderer}倒下了!");
            }
            else if (battleResult.NewEffects.TryGetValue(result.Defenderer, out var effects))
            {
                foreach (var effect in effects)
                    panelLog.SetLog($"{result.Defenderer}獲得了{effect}!");
            }

            foreach (var unit in result.LevelUpUnits)
                panelLog.SetLog($"{unit}升級了!");
        }

        if (battleResult.IsAttackerDead)
        {
            panelLog.SetLog($"{battleResult.Attacker}倒下了!");
        }
        else if (battleResult.NewEffects.TryGetValue(battleResult.Attacker, out var effects))
        {
            foreach (var effect in effects)
                panelLog.SetLog($"{battleResult.Attacker}獲得了{effect}!");
        }

        foreach (var breakEquip in battleResult.BreakEquips)
            panelLog.SetLog($"{breakEquip}毀損了", Color.yellow);

        foreach (var dropItem in battleResult.DropItems)
            panelLog.SetLog($"{battleResult.Attacker}獲得了{dropItem}!");
    }

    void ShowEffectLog(EffectResult.Result result)
    {
        if (result == null)
            return;

        if (result.Infos.Count > 0)
            panelLog.SetLine();

        foreach (var info in result.Infos)
        {
            if (info.MofityAbility.HP != 0)
            {
                if (info.MofityAbility.HP > 0)
                    panelLog.SetLog($"{result.CharacterName}因{info.EffectName}的效果回復了{info.MofityAbility.HP:0}點HP");
                else
                    panelLog.SetLog($"{result.CharacterName}因{info.EffectName}的效果失去了{-1 * info.MofityAbility.HP:0}點HP");
            }
            if (info.MofityAbility.MP != 0)
            {
                if (info.MofityAbility.MP > 0)
                    panelLog.SetLog($"{result.CharacterName}因{info.EffectName}的效果回復了{info.MofityAbility.MP:0}點MP");
                else
                    panelLog.SetLog($"{result.CharacterName}因{info.EffectName}的效果失去了{-1 * info.MofityAbility.MP:0}點MP");
            }
            if (info.MofityAbility.STA != 0)
            {
                if (info.MofityAbility.STA > 0)
                    panelLog.SetLog($"{result.CharacterName}因{info.EffectName}的效果回復了{info.MofityAbility.STA:0}點STA");
                else
                    panelLog.SetLog($"{result.CharacterName}因{info.EffectName}的效果失去了{-1 * info.MofityAbility.STA:0}點STA");
            }

            if (info.IsTimeUp)
                panelLog.SetLog($"{result.CharacterName}的{info.EffectName}狀態已解除");
        }

        if (result.IsDead)
            panelLog.SetLog($"{result.CharacterName}倒下了!");
    }
}