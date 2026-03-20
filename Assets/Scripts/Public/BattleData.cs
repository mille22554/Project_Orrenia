public class BattleData
{
    public string Name;
    public CharacterRole Role;
    public int Exp;
    public int HP;
    public int MP;
    public int LUK;
    public int STA;
    public decimal TP;
    public int ATK;
    public int MATK;
    public int DEF;
    public int MDEF;
    public int ACC;
    public int EVA;
    public int CRIT;
    public int SPD;

    public static BattleData Create(CharacterData characterData)
    {
        var fullAbility = PublicFunc.GetCharacterAbility(characterData);
        var battleData = new BattleData
        {
            Name = characterData.Name,
            Role = characterData.Role,
            Exp = characterData.CurrentExp,
            HP = characterData.CurrentHP,
            MP = characterData.CurrentMP,
            STA = characterData.CurrentSTA,
            TP = characterData.CurrentTP,
            LUK = fullAbility.LUK,
            ATK = fullAbility.ATK,
            MATK = fullAbility.MATK,
            DEF = fullAbility.DEF,
            MDEF = fullAbility.MDEF,
            ACC = fullAbility.ACC,
            EVA = fullAbility.EVA,
            CRIT = fullAbility.CRIT,
            SPD = fullAbility.SPD,
        };

        return battleData;
    }
}