using System.Linq;
using UnityEngine;

public class Mob_7_Handler : IMobHandler
{
    public int ID => 7;

    public SkillData Handler(CharacterData mob)
    {
        var skillList = mob.Skills;

        if (!mob.Effects.Any(x => x.ID == EEffectID.SPD_UP) || !mob.Effects.Any(x => x.ID == EEffectID.DEX_UP))
        {
            skillList.TryGetValue(ESkillID.響尾, out var skill);
            return skill;
        }
        else
        {
            var attackSkillList = skillList.Values.Where(x =>
                (x.SkillType == ESkillType.SinglePhysicsAttack || x.SkillType == ESkillType.SingleMagicAttack) && x.CurrentCD == 0 && x.Cost <= mob.CurrentMP
            ).ToList();

            if (attackSkillList.Count > 0)
                return attackSkillList.ElementAtOrDefault(Random.Range(0, attackSkillList.Count));
        }

        return null;
    }
}