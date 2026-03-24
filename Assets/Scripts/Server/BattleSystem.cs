using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Random = UnityEngine.Random;

public static class BattleSystem
{
    public static CharacterData RunSpeedProcess()
    {
        var playerCharacterData = GameData_Server.NowCharacterData;
        var units = new Dictionary<BattleData, CharacterData>
        {
            { BattleData.Create(playerCharacterData), playerCharacterData }
        };

        foreach (var mob in GameData_Server.NowEnemyData.Enemies)
        {
            units.Add(BattleData.Create(mob.CharacterData), mob.CharacterData);
        }

        BattleData next = null;
        decimal minTime = decimal.MaxValue;

        foreach (var unit in units.Keys)
        {
            decimal time = (GameData_Server.tpCost - unit.TP) / unit.SPD;

            if (time < minTime)
            {
                minTime = time;
                next = unit;
            }
        }

        foreach (var unit in units)
        {
            unit.Value.CurrentTP += unit.Key.SPD * minTime;
        }

        return units[next];
    }

    public static BattleResult CheckNowActor()
    {
        var nowActor = RunSpeedProcess();
        BattleResult result = null;

        switch (nowActor.Role)
        {
            case CharacterRole.Player:

                break;
            case CharacterRole.Mob:
                result = RunBattle(nowActor, GameData_Server.SaveData.Datas.CharacterData);
                break;
        }

        return result;
    }

    public static BattleResult RunBattle(CharacterData character1, CharacterData character2)
    {
        character1.CurrentTP -= GameData_Server.tpCost;

        var result = new BattleResult
        {
            BreakEquips = new()
        };

        var attacker = BattleData.Create(character1);
        var defender = BattleData.Create(character2);

        result.Attacker = attacker.Name;
        result.Defenderer = defender.Name;

        LuckyEventCheck(attacker, defender, result);
        if (result.IsAttackerDead || result.IsDefenderDead)
        {
            RefreshCharacterData(attacker, character1);
            RefreshCharacterData(defender, character2);
            return result;
        }

        #region 迴避判定
        if (!(GetEffectiveStat(attacker.ACC, 40) > GetEffectiveStat(defender.EVA)))
        {
            Debug.Log($"{result.Defenderer}閃避了{result.Attacker}的攻擊!");
            result.IsDodge = true;
            RefreshCharacterData(attacker, character1);
            RefreshCharacterData(defender, character2);
            return result;
        }
        #endregion

        var damageMulti = 1;
        var defenceMulti = 1;
        #region 爆擊判定
        if (GetEffectiveStat(attacker.CRIT) > GetEffectiveStat(defender.EVA))
        {
            result.IsCritical = true;

            damageMulti = 2;
            defenceMulti = 0;
            Debug.Log($"{result.Attacker}命中了要害!");
        }
        #endregion

        result.BattleDamage = GetEffectiveStat(attacker.ATK) * damageMulti - GetEffectiveStat(defender.DEF) * defenceMulti;
        if (result.BattleDamage <= 0)
            result.BattleDamage = 1;

        Debug.Log($"{result.Attacker}對{result.Defenderer}造成了{result.BattleDamage}點傷害!");
        if (DamageProcess(defender, result.BattleDamage))
        {
            result.IsDefenderDead = true;
        }
        result.BreakEquips.AddRange(RunDurability(attacker.Role == CharacterRole.Player));

        RefreshCharacterData(attacker, character1);
        RefreshCharacterData(defender, character2);
        return result;
    }

    static void RefreshCharacterData(BattleData battleData, CharacterData characterData)
    {
        characterData.CurrentHP = battleData.HP;
        characterData.CurrentMP = battleData.MP;
    }

    static void LuckyEventCheck(BattleData character1, BattleData character2, BattleResult result)
    {
        //先比誰觸發
        bool isCharacter1Attack = GetEffectiveStat(character1.LUK) > GetEffectiveStat(character2.LUK);

        // 攻擊者與被攻擊者的參考
        int attackerLUK = isCharacter1Attack ? character1.LUK : character2.LUK;
        int defenderLUK = isCharacter1Attack ? character2.LUK : character1.LUK;

        result.LuckyEventLevel = 0; // 0~3

        for (int i = 0; i < 3; i++)
        {
            if (GetEffectiveStat(attackerLUK) > GetEffectiveStat(defenderLUK * (result.LuckyEventLevel * 2 + 1) * 10))
                result.LuckyEventLevel++;
            else
                break; // 只要輸掉就結束
        }

        if (result.LuckyEventLevel > 1)
        {
            result.IsLuckyEventTrigger = true;

            var hitter = isCharacter1Attack ? character2 : character1;
            result.LuckyEventTarget = hitter.Name;
            var damageMulti = 0;
            switch (result.LuckyEventLevel)
            {
                case 2:
                    damageMulti = 10;
                    // result.LuckyEventDamage = Mathf.Max(GetEffectiveStat(attackerLUK), 1 * 10 - GetEffectiveStat(defenderLUK));
                    // Debug.Log($"{result.LuckyEventTarget }突然抽筋了!\n受到了{result.LuckyEventDamage}點傷害!");
                    break;
                case 3:
                    damageMulti = 50;
                    // result.LuckyEventDamage = Mathf.Max(GetEffectiveStat(attackerLUK), 1 * 50 - GetEffectiveStat(defenderLUK));
                    // Debug.Log($"一輛大卡車疾駛而來，撞飛了{result.LuckyEventTarget }!\n受到了{result.LuckyEventDamage}點傷害!");
                    break;
            }
            result.LuckyEventDamage = Mathf.Max(1, (GetEffectiveStat(attackerLUK) - GetEffectiveStat(defenderLUK)) * damageMulti);

            if (DamageProcess(hitter, result.LuckyEventDamage))
            {
                if (isCharacter1Attack)
                    result.IsDefenderDead = true;
                else
                    result.IsAttackerDead = true;
            }
        }
    }

    public static int GetEffectiveStat(int times) => GetEffectiveStat(times, 20);
    public static int GetEffectiveStat(int times, int prop)
    {
        int count = 0;
        for (int i = 0; i < times; i++)
        {
            if (Random.Range(0, 100) < prop)
                count++;
        }

        return count;
    }

    static bool DamageProcess(BattleData character, int damage)
    {
        character.HP -= damage;
        if (character.HP <= 0)
        {
            // Debug.Log($"{character.Name}倒下了!");
            return true;
        }
        else
        {
            return false;
        }
    }

    static List<string> RunDurability(bool isAttack)
    {
        var breakEquips = new List<string>();
        var items = new List<ItemData>();

        // 2️⃣ 掃描所有裝備欄位
        foreach (var field in typeof(EquipBase).GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            long uid = (long)field.GetValue(GameData_Server.NowCharacterData.Equips);
            if (uid == 0)
                continue;

            var item = GameData_Server.NowBagData.Items
                .Find(x => x.UID == uid && ItemTypeCheck.IsEquipType(ItemBaseData.Get(x.ItemID).Type));

            if (item == null)
                continue;

            // 攻擊時扣武器耐久、防禦時扣防具耐久
            bool isWeapon = GameData_Server.Weapons.Contains(ItemBaseData.Get(item.ItemID).Type);

            if ((isAttack && isWeapon) || (!isAttack && !isWeapon))
                items.Add(item);
        }

        // 3️⃣ 處理耐久度扣減與移除
        foreach (var item in items.ToList()) // ToList 避免修改集合時出錯
        {
            // Debug.Log(_item.name);
            item.Durability--;
            if (item.Durability <= 0)
            {
                PublicFunc.UnloadEquip(item.UID);
                GameData_Server.NowBagData.Items.Remove(item);
                breakEquips.Add(ItemBaseData.Get(item.ItemID).Name);
                // await Debug.LogAsync($"{ItemBaseData.Get(item.itemID).name}毀損了", Color.yellow);
            }
        }

        return breakEquips;
    }

    public static void EnemyDeadProcess(MobData target, BattleResult result)
    {
        var characterData = GameData_Server.SaveData.Datas.CharacterData;
        var playerData = GameData_Server.SaveData.Datas.PlayerData;
        var enemyData = GameData_Server.SaveData.Datas.EnemyData;

        characterData.CurrentExp += 1 << (target.CharacterData.Level - 1);

        var MaxExp = PublicFunc.GetExp(characterData);
        if (characterData.CurrentExp >= MaxExp)
        {
            result.IsUnitLevelUp = true;
            result.LevelUpUnit = characterData.Name;
            characterData.Level += 1;
            characterData.CurrentExp -= MaxExp;
            playerData.SkillPoint += 1;
            PublicFunc.InitCurrentData(characterData);
        }

        // foreach (var drop in target.DropItems)
        // {
        //     if (PublicFunc.Dice(1, 100) > drop.Prop)
        //         continue;

        //     var existing = GameData_Server.NowBagData.items.Find(item => item.itemID == drop.Item);

        //     if (ItemTypeCheck.IsEquipType(drop.Item.type) || existing == null)
        //     {
        //         GameData_Server.NowBagData.items.Add(PublicFunc.GetItem(drop.item));
        //         await panelLog.SetLogAsync($"{_playerData.PlayerName}獲得了{drop.item.name}!");
        //     }
        //     else
        //     {
        //         existing.count++;
        //         await panelLog.SetLogAsync($"{_playerData.PlayerName}獲得了{ItemBaseData.Get(existing.itemID).name}!");
        //     }
        // }

        enemyData.Enemies.Remove(target);
    }
}