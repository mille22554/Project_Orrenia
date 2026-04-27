using System.Collections.Generic;
using Unity.Netcode;

public class ActionResult : INetworkSerializable
{
    public BattleResult BattleResult = new();
    public RestResult RestResult = new();
    public EffectResult EffectResult = new();

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref BattleResult);
        serializer.SerializeValue(ref RestResult);
        serializer.SerializeValue(ref EffectResult);
    }
}

public class BattleResult : INetworkSerializable
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
    public Dictionary<string, List<string>> NewEffects = new();

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        var results = Results.ToArray();

        serializer.SerializeValue(ref results);
        serializer.SerializeValue(ref Attacker);
        serializer.SerializeValue(ref IsAttackerDead);
        serializer.SerializeValue(ref IsSkill);
        serializer.SerializeValue(ref SkillName);
        serializer.SerializeValue(ref IsAttakerIncapacitated);
        serializer.SerializeValue(ref IncapacitatedEffect);

        for (var i = 0; i < BreakEquips.Count; i++)
        {
            var temp = BreakEquips[i];
            serializer.SerializeValue(ref temp);
        }

        for (var i = 0; i < DropItems.Count; i++)
        {
            var temp = DropItems[i];
            serializer.SerializeValue(ref temp);
        }

        foreach (var effect in NewEffects)
        {
            var key = effect.Key;
            var value = effect.Value;

            serializer.SerializeValue(ref key);

            for (var i = 0; i < value.Count; i++)
            {
                var temp = value[i];
                serializer.SerializeValue(ref temp);
            }
        }
    }

    public class Result : INetworkSerializable
    {
        public string Defenderer = "";
        public bool IsLuckyEventTrigger;
        public int LuckyEventLevel;
        public string LuckyEventTarget = "";
        public decimal LuckyEventDamage;
        public bool IsDodge;
        public bool IsCritical;
        public decimal BattleDamage;

        public bool IsDefenderDead;
        public List<string> LevelUpUnits = new();
        public bool IsCounter;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Defenderer);
            serializer.SerializeValue(ref IsLuckyEventTrigger);
            serializer.SerializeValue(ref LuckyEventLevel);
            serializer.SerializeValue(ref LuckyEventTarget);
            serializer.SerializeValue(ref LuckyEventDamage);
            serializer.SerializeValue(ref IsDodge);
            serializer.SerializeValue(ref IsCritical);
            serializer.SerializeValue(ref BattleDamage);
            serializer.SerializeValue(ref IsDefenderDead);
            serializer.SerializeValue(ref IsCounter);

            for (var i = 0; i < LevelUpUnits.Count; i++)
            {
                var unit = LevelUpUnits[i];
                serializer.SerializeValue(ref unit);
            }
        }
    }
}

public class RestResult:INetworkSerializable
{
    public int RecoverHP;
    public int RecoverMP;
    public int RecoverSTA;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref RecoverHP);
        serializer.SerializeValue(ref RecoverMP);
        serializer.SerializeValue(ref RecoverSTA);
    }
}

public class EffectResult : INetworkSerializable
{
    public List<Result> Results = new();

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        var results = Results.ToArray();

        serializer.SerializeValue(ref results);
    }

    public class Result : INetworkSerializable
    {
        public List<Info> Infos = new();
        public string CharacterName;
        public bool IsDead;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            var infos = Infos.ToArray();

            serializer.SerializeValue(ref infos);
        }

        public class Info : INetworkSerializable
        {
            public string EffectName;
            public bool IsTimeUp;
            public FullAbilityBase MofityAbility = new();

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref EffectName);
                serializer.SerializeValue(ref IsTimeUp);
                serializer.SerializeValue(ref MofityAbility);
            }
        }
    }
}