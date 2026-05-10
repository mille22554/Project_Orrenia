using System.Linq;
using UnityEngine;

public class Mob_7_Handler : IMobHandler
{
    public int ID => 7;

    public SkillData Handler(CharacterData mob)
    {
        var skillList = SaveDataCenter.GetSkills(mob.UID);
        var effects = SaveDataCenter.GetEffects(mob.UID);

        if (!effects.Any(x => x.ID == EEffectID.SPD_UP) || !effects.Any(x => x.ID == EEffectID.DEX_UP))
        {
            var skill = skillList.Find(x => x.ID == ESkillID.響尾);
            return skill;
        }
        else
        {
            var attackSkillList = skillList.Where(x =>
                (x.SkillType == ESkillType.SinglePhysicsAttack || x.SkillType == ESkillType.SingleMagicAttack) && x.CurrentCD == 0 && x.Cost <= mob.CurrentMP
            ).ToList();

            if (attackSkillList.Count > 0)
                return attackSkillList.ElementAtOrDefault(Random.Range(0, attackSkillList.Count));
        }

        return null;
    }
}