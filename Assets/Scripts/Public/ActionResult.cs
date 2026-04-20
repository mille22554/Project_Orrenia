using System.Collections.Generic;

public class ActionResult
{
    public BattleResult BattleResult = new();
    public RestResult RestResult = new();
    public EffectResult EffectResult = new();
}

public class BattleResult
{
    public List<Result> Results = new();
    public string Attacker;
    public bool IsAttackerDead;
    public List<string> BreakEquips = new();
    public List<string> DropItems = new();
    public bool IsSkill;
    public string SkillName;
    public bool IsAttakerIncapacitated;
    public string IncapacitatedEffect;

    public class Result
    {
        public string Defenderer;
        public bool IsLuckyEventTrigger;
        public int LuckyEventLevel;
        public string LuckyEventTarget;
        public decimal LuckyEventDamage;
        public bool IsDodge;
        public bool IsCritical;
        public decimal BattleDamage;

        public bool IsDefenderDead;
        public bool IsUnitLevelUp;
        public string LevelUpUnit;
        public bool IsCounter;
    }
}

public class RestResult
{
    public int RecoverHP;
    public int RecoverMP;
    public int RecoverSTA;
}

public class EffectResult
{
    public List<Result> Results = new();

    public class Result
    {
        public List<Info> Infos = new();
        public string CharacterName;
        public bool IsDead;

        public class Info
        {
            public string EffectName;
            public bool IsTimeUp;
            public FullAbilityBase MofityAbility = new();
        }
    }
}