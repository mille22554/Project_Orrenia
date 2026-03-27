using System.Collections.Generic;

public class ActionResult
{
    public BattleResult BattleResult;
    public RestResult RestResult;
}

public class BattleResult
{
    public string Attacker;
    public string Defenderer;
    public bool IsLuckyEventTrigger;
    public int LuckyEventLevel;
    public string LuckyEventTarget;
    public int LuckyEventDamage;
    public bool IsDodge;
    public bool IsCritical;
    public int BattleDamage;
    public bool IsAttackerDead;
    public bool IsDefenderDead;
    public bool IsUnitLevelUp;
    public string LevelUpUnit;
    public List<string> BreakEquips;
    public List<string> DropItems;
}

public class RestResult
{
    public int RecoverHP;
    public int RecoverMP;
    public int RecoverSTA;
}