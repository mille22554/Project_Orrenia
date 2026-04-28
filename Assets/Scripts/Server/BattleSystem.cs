using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public static class BattleSystem
{
    static BattleData _lastActor;

    public static void InitNewBattle(PartyData partyData)
    {
        _lastActor = null;

        foreach (var member in partyData.Members)
            GameData_Server.GetCharacterData(member).CurrentTP = 0;

        partyData.Enemies = EnemySetting.SetEnemy(partyData.Area, partyData.Deep);
    }

    public static BattleData CreateBattleData(CharacterData characterData)
    {
        var ability = CharacterDataCenter.GetCharacterAbility(characterData);
        var battleData = new BattleData
        {
            Name = characterData.Name,
            Role = characterData.Role,
            Exp = characterData.CurrentExp,
            HP = characterData.CurrentHP,
            MP = characterData.CurrentMP,
            STA = characterData.CurrentSTA,
            TP = characterData.CurrentTP,

            STR = ability.STR,
            DEX = ability.DEX,
            INT = ability.INT,
            VIT = ability.VIT,
            AGI = ability.AGI,
            LUK = ability.LUK,
            ATK = ability.ATK,
            MATK = ability.MATK,
            DEF = ability.DEF,
            MDEF = ability.MDEF,
            ACC = (decimal)(Math.Pow((double)ability.ACC, 1.0 / 4.0) * 100),
            EVA = (decimal)(Math.Pow((double)ability.EVA, 1.0 / 4.0) * 100),
            CRIT = (decimal)(Math.Pow((double)ability.CRIT, 1.0 / 4.0) * 100),
            SPD = (decimal)(Math.Pow((double)ability.SPD, 1.0 / 4.0) * 100),
        };
        return battleData;
    }

    //用decimal算是因為double會有誤差，會卡在9999.99999999多
    public static (CharacterData characterData, int mobID) RunSpeedProcess(PartyData partyData)
    {
        var enemies = partyData.Enemies;
        var units = new Dictionary<BattleData, CharacterData>();

        foreach (var member in partyData.Members)
        {
            var characterData = GameData_Server.GetCharacterData(member);
            units.Add(CreateBattleData(characterData), characterData);
        }

        foreach (var mob in enemies)
        {
            var characterData = mob.CharacterData;
            units.Add(CreateBattleData(characterData), characterData);
        }

        BattleData next = null;
        var minTime = decimal.MaxValue;

        foreach (var unit in units.Keys)
        {
            var time = (GameData_Server.tpCost - unit.TP) / unit.SPD;

            if (time < minTime)
            {
                minTime = time;
                next = unit;
            }
        }

        if (_lastActor != null && _lastActor.Name == next.Name)
            next.Combo = _lastActor.Combo + 1;

        _lastActor = next;

        foreach (var unit in units)
        {
            unit.Value.CurrentTP = unit.Value.CurrentTP + unit.Key.SPD * minTime;
        }

        var actMob = enemies.Find(x => x.CharacterData.Name == units[next].Name);
        var mobID = actMob != null ? actMob.MobID : -1;

        return (units[next], mobID);
    }

    public static void CheckNowActor(PartyData partyData, ActionResult actionResult)
    {
        var debugSwitch = false;
        debugSwitch = true;

        var (nowActor, mobID) = RunSpeedProcess(partyData);
        if (debugSwitch)
            Debug.Log($"當前行動單位: {nowActor.Name}");

        switch (nowActor.Role)
        {
            case ECharacterRole.Player:
                {
                    var skillList = nowActor.Skills.Values
                        .Where(x => SkillWeaponCheck(nowActor, x))
                        .ToList();

                    var skill = skillList.ElementAtOrDefault(Random.Range(0, skillList.Count));

                    if (nowActor.Effects.Find(x => x.ID == EEffectID.Berserk) != null)
                    {
                        if (debugSwitch && skill != null)
                            Debug.Log($"施放技能: {skill.Name}");

                        RunBattle(nowActor, new() { partyData.Enemies.FirstOrDefault().CharacterData }, skill, actionResult.BattleResult);
                        var effectResult = CharacterDataCenter.ActionEndProcess(nowActor);
                        if (effectResult.Infos.Count > 0)
                            actionResult.EffectResult.Results.Add(effectResult);
                    }
                }
                break;
            case ECharacterRole.Mob:
                {
                    var skill = MobDataCenter.SetMobAction(nowActor, mobID);

                    if (debugSwitch && skill != null)
                        Debug.Log($"施放技能: {skill.Name}");

                    if (skill == null)
                    {
                        var target = GameData_Server.GetCharacterData(partyData.Members[Random.Range(0, partyData.Members.Count)]);
                        RunBattle(nowActor, new() { target }, skill, actionResult.BattleResult);
                    }
                    else
                    {
                        switch (skill.SkillType)
                        {
                            case ESkillType.SinglePhysicsAttack:
                            case ESkillType.SingleMagicAttack:
                                var target = GameData_Server.GetCharacterData(partyData.Members[Random.Range(0, partyData.Members.Count)]);
                                RunBattle(nowActor, new() { target }, skill, actionResult.BattleResult);
                                break;
                            case ESkillType.SingleBuff:
                                foreach (var buff in skill.Buffs)
                                    CharacterDataCenter.AddCharacterEffect(nowActor, buff);

                                break;
                        }
                    }

                    var effectResult = CharacterDataCenter.ActionEndProcess(nowActor);
                    if (effectResult.Infos.Count > 0)
                        actionResult.EffectResult.Results.Add(effectResult);
                }
                break;
        }
    }

    static bool SkillWeaponCheck(CharacterData characterData, SkillData skillData)
    {
        if (skillData.Cost > characterData.CurrentMP || skillData.SkillType == ESkillType.Passive || skillData.CurrentCD != 0)
            return false;

        if (skillData.WeaponType.Count == 0)
            return true;

        var isWeaponTypeError = true;
        var weaponType = EItemKind.None;

        foreach (var equip in characterData.Equips)
        {
            var item = characterData.BagItems.Find(x => x.UID == equip);
            if (item != null && ItemDataCenter_Server.IsWeapon(item.Kind))
            {
                weaponType = item.Kind;
                foreach (var type in skillData.WeaponType)
                {
                    if (weaponType == type)
                        isWeaponTypeError = false;
                }
            }
        }

        if (weaponType == EItemKind.None)
        {
            foreach (var type in skillData.WeaponType)
            {
                if (weaponType == type)
                    isWeaponTypeError = false;
            }
        }

        return !isWeaponTypeError;
    }

    public static void RunBattle(CharacterData actor, List<CharacterData> targets, BattleResult battleResult) => RunBattle(actor, targets, null, battleResult);
    public static void RunBattle(CharacterData actor, List<CharacterData> targets, SkillData skillData, BattleResult battleResult)
    {
        actor.CurrentTP -= GameData_Server.tpCost;

        var incapacitatedEffect = actor.Effects.Find(x => x.ID == EEffectID.Stun);
        if (incapacitatedEffect != null)
        {
            battleResult.IsAttakerIncapacitated = true;
            battleResult.IncapacitatedEffect = incapacitatedEffect.Name;
            return;
        }

        var attacker = CreateBattleData(actor);
        var defenders = new Dictionary<BattleData, CharacterData>();

        foreach (var target in targets)
            defenders.Add(CreateBattleData(target), target);

        battleResult.Attacker = attacker.Name;

        foreach (var (defender, target) in defenders.ToList())
        {
            var result = new BattleResult.Result
            {
                Defenderer = defender.Name
            };
            battleResult.Results.Add(result);

            battleResult.IsAttackerDead = LuckyEventCheck(attacker, defender, result);

            if (result.IsDefenderDead)
            {
                RefreshCharacterData(defender, target);
                defenders.Remove(defender);
                target.Effects.Clear();
            }
        }

        if (battleResult.IsAttackerDead || defenders.Count == 0)
        {
            RefreshCharacterData(attacker, actor);
            if (battleResult.IsAttackerDead)
                actor.Effects.Clear();

            foreach (var (defender, target) in defenders)
                RefreshCharacterData(defender, target);

            return;
        }

        if (skillData != null)
        {
            battleResult.IsSkill = true;
            battleResult.SkillName = skillData.Name;

            attacker.MP -= skillData.Cost;
            skillData.CurrentCD = skillData.CoolDown;
        }

        #region 迴避判定
        var attackerACC = GetEffectiveStat(attacker.ACC, 40);
        foreach (var (defender, target) in defenders.ToList())
        {
            if (!(attackerACC > GetEffectiveStat(defender.EVA)))
            {
                Debug.Log($"迴避判定: {defender.EVA}, {attacker.ACC}");
                var result = battleResult.Results.Find(x => x.Defenderer == defender.Name);

                result.IsDodge = true;
                RefreshCharacterData(defender, target);
                defenders.Remove(defender);
            }
        }

        if (defenders.Count == 0)
        {
            _lastActor.Combo = 0;
            RefreshCharacterData(attacker, actor);
            return;
        }
        #endregion

        foreach (var (defender, target) in defenders)
        {
            var result = battleResult.Results.Find(x => x.Defenderer == defender.Name);
            var counterEffect = target.Effects.Find(x => x.ID == EEffectID.Counter);

            var damageMulti = 1;
            var defenceMulti = 1;

            #region 爆擊判定
            if (counterEffect == null)
            {
                if (GetEffectiveStat(attacker.CRIT) > GetEffectiveStat(defender.EVA, 40))
                {
                    Debug.LogWarning($"爆擊判定: {attacker.CRIT}, {defender.EVA}");
                    result.IsCritical = true;

                    damageMulti = 2;
                    defenceMulti = 0;
                }
            }
            #endregion

            #region 攻守計算
            var attackValue = 0.0;
            var defenceValue = 0.0;
            if (skillData == null)
            {
                attackValue = (double)(GetEffectiveStat(attacker.ATK) * damageMulti);
                defenceValue = (double)(GetEffectiveStat(defender.DEF) * defenceMulti);
            }
            else
            {
                var skillDamage = CharacterDataCenter.ParamCalculate(attacker, skillData.Damage);
                var defence = skillData.SkillType switch
                {
                    ESkillType.SinglePhysicsAttack => defender.DEF,
                    ESkillType.SingleMagicAttack => defender.MDEF,
                    _ => 0
                };

                attackValue = (double)(GetEffectiveStat(skillDamage) * damageMulti);
                defenceValue = (double)(GetEffectiveStat(defence) * defenceMulti);
            }
            #endregion

            if (counterEffect != null && (skillData == null || skillData.SkillType == ESkillType.SinglePhysicsAttack))
            {
                result.IsCounter = true;
                result.BattleDamage = (decimal)(attackValue * CharacterDataCenter.ParamCalculate(defender, counterEffect.Value));

                if (DamageProcess(attacker, result.BattleDamage))
                {
                    battleResult.IsAttackerDead = true;
                    actor.Effects.Clear();
                }

                battleResult.BreakEquips.AddRange(RunDurability(actor, defender.Role == ECharacterRole.Player));
            }
            else
            {
                result.BattleDamage = (decimal)(attackValue - defenceValue);
                if (result.BattleDamage <= 0)
                    result.BattleDamage = 1;

                if (skillData != null)
                {
                    foreach (var buff in skillData.Buffs)
                    {
                        CharacterDataCenter.AddCharacterEffect(actor, buff);
                        buff.Name = CharacterDataCenter.GetEffectData(buff.ID).Name;
                        if (battleResult.NewEffects.TryGetValue(actor.Name, out var effects))
                            effects.Add(buff.Name);
                        else
                            battleResult.NewEffects.Add(actor.Name, new() { buff.Name });
                    }

                    foreach (var debuff in skillData.DeBuffs)
                    {
                        var prop = PublicFunc.Dice(1, 100);
                        // Debug.Log($"Debuff判定: {prop}");
                        if (prop < debuff.Prop)
                        {
                            CharacterDataCenter.AddCharacterEffect(target, debuff.Effect);
                            debuff.Effect.Name = CharacterDataCenter.GetEffectData(debuff.Effect.ID).Name;
                            if (battleResult.NewEffects.TryGetValue(target.Name, out var effects))
                                effects.Add(debuff.Effect.Name);
                            else
                                battleResult.NewEffects.Add(target.Name, new() { debuff.Effect.Name });
                        }
                    }
                }

                if (skillData == null || skillData.SkillType == ESkillType.SinglePhysicsAttack)
                {
                    foreach (var equipUID in actor.Equips)
                    {
                        var equip = actor.BagItems.Find(x => x.UID == equipUID);
                        if (ItemDataCenter_Server.IsWeapon(equip.Kind))
                        {
                            if (equip.Trait != null)
                            {
                                if (equip.Trait.Poisoning > 0)
                                {
                                    var effectValue = new List<ParamFormat> { new() { Constant = 1 } };
                                    CharacterDataCenter.AddCharacterEffect(target, EEffectID.Poisoning, effectValue, equip.Trait.Poisoning);
                                }
                            }
                        }
                    }
                }

                if (DamageProcess(defender, result.BattleDamage * (attacker.Combo * 0.1m + 1)))
                {
                    result.IsDefenderDead = true;
                    target.Effects.Clear();
                }

            }

            RefreshCharacterData(defender, target);
        }

        battleResult.BreakEquips.AddRange(RunDurability(actor, attacker.Role == ECharacterRole.Player));
        RefreshCharacterData(attacker, actor);
    }

    static void RefreshCharacterData(BattleData battleData, CharacterData characterData)
    {
        characterData.CurrentHP = battleData.HP;
        characterData.CurrentMP = battleData.MP;
    }

    static bool LuckyEventCheck(BattleData actor, BattleData target, BattleResult.Result result)
    {
        //先比誰觸發
        bool isActorAttack = GetEffectiveStat(actor.LUK) > GetEffectiveStat(target.LUK);

        // 攻擊者與被攻擊者的參考
        decimal attackerLUK = isActorAttack ? actor.LUK : target.LUK;
        decimal defenderLUK = isActorAttack ? target.LUK : actor.LUK;

        result.LuckyEventLevel = 0; // 0~3

        for (int i = 0; i < 3; i++)
        {
            if (GetEffectiveStat(attackerLUK) > GetEffectiveStat(defenderLUK * (result.LuckyEventLevel * 2 + 1) * 20))
                result.LuckyEventLevel++;
            else
                break; // 只要輸掉就結束
        }

        if (result.LuckyEventLevel > 1)
        {
            Debug.Log($"幸運判定: {attackerLUK}, {defenderLUK}");
            result.IsLuckyEventTrigger = true;

            var hitter = isActorAttack ? target : actor;
            result.LuckyEventTarget = hitter.Name;
            var damageMulti = 0;
            switch (result.LuckyEventLevel)
            {
                case 2:
                    damageMulti = 10;
                    break;
                case 3:
                    damageMulti = 50;
                    break;
            }
            result.LuckyEventDamage = (decimal)Mathf.Max(1, (float)(GetEffectiveStat(attackerLUK) - GetEffectiveStat(defenderLUK) + damageMulti));

            if (DamageProcess(hitter, result.LuckyEventDamage))
            {
                if (isActorAttack)
                    result.IsDefenderDead = true;
                else
                    return true;
            }
        }

        return false;
    }

    public static decimal GetEffectiveStat(decimal times) => GetEffectiveStat(times, 20);
    public static decimal GetEffectiveStat(decimal times, decimal prop)
    {
        int count = 0;
        for (int i = 0; i < times; i++)
        {
            if (Random.Range(0, 100) < prop)
                count++;
        }

        return count;
    }

    static bool DamageProcess(BattleData character, decimal damage)
    {
        character.HP -= (int)damage;

        if (character.HP <= 0)
            return true;
        else
            return false;
    }

    static List<string> RunDurability(CharacterData characterData, bool isAttack)
    {
        var breakEquips = new List<string>();

        var bagItems = characterData.BagItems;

        foreach (var equipUID in characterData.Equips.ToList())
        {
            var equip = bagItems.Find(x => x.UID == equipUID);
            var kind = equip.Kind;
            var isWeapon = ItemDataCenter_Server.IsWeapon(kind);

            if ((isAttack && isWeapon) || (!isAttack && !isWeapon))
            {
                equip.Durability--;
                if (equip.Durability <= 0)
                {
                    characterData.Equips.Remove(equipUID);
                    bagItems.Remove(equip);
                    breakEquips.Add(equip.Name);
                }
            }
        }

        return breakEquips;
    }

    public static void EnemyDeadProcess(MobData target, BattleResult.Result result, PartyData partyData, List<string> dropItems)
    {
        foreach (var member in partyData.Members)
        {
            var characterData = GameData_Server.GetCharacterData(member);
            var playerData = GameData_Server.GetPlayerData(member);

            characterData.CurrentExp += 1 << (target.CharacterData.Level - 1);

            var maxExp = PublicFunc.GetExp(characterData.Level);
            if (characterData.CurrentExp >= maxExp)
            {
                result.LevelUpUnits.Add(characterData.Name);
                characterData.Level += 1;
                characterData.CurrentExp -= maxExp;
                playerData.SkillPoint += 1;
                CharacterDataCenter.InitCurrentData(characterData);
            }

            foreach (var drop in target.DropItems)
            {
                if (PublicFunc.Dice(1, 100) > drop.Prop)
                    continue;

                var bagItems = characterData.BagItems;
                var existing = bagItems.Find(item => item.ItemID == drop.Item);
                var itemData = ItemDataCenter_Server.GetItemData(drop.Item);

                ItemDataCenter_Server.DoActionAccordingToCategory(itemData.Kind, EquipCallBack, OtherCallBack, OtherCallBack);

                dropItems.Add(itemData.Name);

                void EquipCallBack() => bagItems.Add(ItemDataCenter_Server.GetNewItem(itemData));

                void OtherCallBack()
                {
                    if (existing == null)
                    {
                        EquipCallBack();
                    }
                    else
                    {
                        existing.Count++;
                    }
                }
            }
        }

        partyData.Enemies.Remove(target);
    }
}