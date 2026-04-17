using System.Linq;
using UnityEngine;

public class Mob_5_Handler : IMobHandler
{
    public int ID => 5;

    public SkillData Handler(CharacterData mob)
    {
        var attackSkillList = mob.Skills.Values.Where(x =>
            (x.SkillType == ESkillType.SinglePhysicsAttack || x.SkillType == ESkillType.SingleMagicAttack) && x.CurrentCD == 0 && x.Cost <= mob.CurrentMP
        ).ToList();

        if (attackSkillList.Count > 0)
            return attackSkillList.ElementAtOrDefault(Random.Range(0, attackSkillList.Count));

        return null;
    }
}