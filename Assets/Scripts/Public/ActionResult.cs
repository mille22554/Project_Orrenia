public class ActionResult
{
    public BattleResult BattleResult;

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
}