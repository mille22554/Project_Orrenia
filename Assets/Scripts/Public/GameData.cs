using System.Collections.Generic;

public static class GameData
{
    public static string version = "0.0.4";
    public static GameSaveData gameData;

    public static PlayerData NowPlayerData => gameData.datas.playerData;
    public static EnemyData NowEnemyData => gameData.datas.enemyData;
}

public static class GameArea
{
    public const string Home = "迷宮都市 － 奧雷尼亞";
    public const string Floor1 = "第一層 露米亞草原";
}

public static class GameEnemy
{
    public static class Floor1
    {
        public const string mob1 = "史萊姆";
        public const string mob2 = "小白兔";
        public const string mob3 = "麻雀";
    }
}

public static class GameItem
{
    public static class Equip
    {
        public static ItemData BasicDagger = new()
        {
            name = "初始短刀",
            id = 1,
            type = EquipType.One_Hand_Weapon,
            description = "新手冒險者的標配，感覺拿著就能受到一點保佑。",
            price = 500,
            ability = new AbilityBase
            {
                ATK = 10,
                LUK = 10
            },
            durability = 500,
            count = 1
        };
    }

    public static class Use
    {
        public static ItemData SmallHpPotion = new()
        {
            name = "小型生命藥水",
            id = 101,
            type = UseType.Use,
            description = "恢復少量生命。",
            price = 50,
            ability = new AbilityBase
            {
                HP = 50
            },
            count = 1
        };

        public static ItemData SmallMpPotion = new()
        {
            name = "小型魔力藥水",
            id = 102,
            type = UseType.Use,
            description = "恢復少量魔力。",
            price = 30,
            ability = new AbilityBase
            {
                MP = 15
            },
            count = 1
        };
    }

    public static class Material
    {
        public static ItemData SlimeGel = new()
        {
            name = "史萊姆凝膠",
            id = 201,
            type = MaterialType.Material,
            description = "史萊姆的一部分，軟軟的帶點彈力。",
            price = 10,
            ability = new AbilityBase
            {
                VIT = 1
            },
            count = 1
        };

        public static ItemData WhiteRabbitFur = new()
        {
            name = "白兔毛皮",
            id = 202,
            type = MaterialType.Material,
            description = "白兔的毛皮，感受到些微的庇護。",
            price = 10,
            ability = new AbilityBase
            {
                LUK = 1
            },
            count = 1
        };

        public static ItemData SparrowFeather = new()
        {
            name = "麻雀羽毛",
            id = 203,
            type = MaterialType.Material,
            description = "麻雀的羽毛，輕巧的可以隨風飛翔。",
            price = 10,
            ability = new AbilityBase
            {
                SPD = 1
            },
            count = 1
        };
    }
}

public class EquipType
{
    public const string One_Hand_Weapon = "單手武器";
    public const string Shield = "盾牌";
    public const string Two_Hand_Weapon = "雙手武器";
    public const string Head = "頭盔";
    public const string Armor = "護胸";
    public const string Greaves = "護腿";
    public const string Shoes = "鞋子";
    public const string Gloves = "手套";
    public const string Cape = "披風";
    public const string Ring = "戒指";
    public const string Pendant = "項鍊";
}

public class UseType
{
    public const string Use = "消耗品";
}

public class MaterialType
{
    public const string Material = "素材";
}