using System.Reflection;

public static class  GameItemData
{
    public static class Equip
    {
        public static ItemBaseData BasicDagger = new()
        {
            name = "初始短刀",
            id = 1,
            type = EquipType.One_Hand_Weapon.Dagger,
            description = "新手冒險者的標配，感覺拿著就能受到一點保佑。",
            price = 500,
            ability = new()
            {
                ATK = 10,
                LUK = 10
            },
            durability = 500,
            count = 1
        };

        public static ItemBaseData WoodenSword = new()
        {
            name = "木製長劍",
            id = 2,
            type = EquipType.One_Hand_Weapon.Sword,
            description = "訓練用的木劍，比空手好一點。",
            price = 500,
            ability = new()
            {
                ATK = 1,
            },
            durability = 500,
            count = 1
        };

        public static ItemBaseData WoodenShield = new()
        {
            name = "木盾",
            id = 3,
            type = EquipType.Shield,
            description = "訓練用的木盾，盾反甚麼的別想了。",
            price = 500,
            ability = new()
            {
                DEF = 1,
            },
            durability = 500,
            count = 1
        };

        public static ItemBaseData WoodenHelmet = new()
        {
            name = "木製頭盔",
            id = 4,
            type = EquipType.Helmet,
            description = "訓練用的木盔，戴起來沒有很舒服。",
            price = 500,
            ability = new()
            {
                DEF = 1,
            },
            durability = 500,
            count = 1
        };

        public static ItemBaseData ClothArmor = new()
        {
            name = "布製護甲",
            id = 5,
            type = EquipType.Armor,
            description = "訓練用的布甲，木劍程度的傷害還是可以防住的。",
            price = 500,
            ability = new()
            {
                DEF = 1,
            },
            durability = 500,
            count = 1
        };

        public static ItemBaseData ClothGreaves = new()
        {
            name = "布製護腿",
            id = 6,
            type = EquipType.Greaves,
            description = "訓練用的護腿，不用怕打到小腿。",
            price = 500,
            ability = new()
            {
                DEF = 1,
            },
            durability = 500,
            count = 1
        };

        public static ItemBaseData ClothBoots = new()
        {
            name = "布靴",
            id = 7,
            type = EquipType.Shoes,
            description = "訓練用的布靴，比赤腳好一點。",
            price = 500,
            ability = new()
            {
                SPD = 1,
            },
            durability = 500,
            count = 1
        };
    }

    public static class Use
    {
        public static ItemBaseData SmallHpPotion = new()
        {
            name = "小型生命藥水",
            id = 101,
            type = UseType.Use,
            description = "恢復少量生命。",
            price = 50,
            ability = new()
            {
                HP = 50
            },
            count = 1
        };

        public static ItemBaseData SmallMpPotion = new()
        {
            name = "小型魔力藥水",
            id = 102,
            type = UseType.Use,
            description = "恢復少量魔力。",
            price = 30,
            ability = new()
            {
                MP = 15
            },
            count = 1
        };

        public static ItemBaseData SmallSTAPotion = new()
        {
            name = "小型體力藥水",
            id = 103,
            type = UseType.Use,
            description = "恢復少量體力。",
            price = 50,
            ability = new()
            {
                STA = 10
            },
            count = 1
        };

        public static ItemBaseData BerserkPotion = new()
        {
            name = "狂暴藥水",
            id = 104,
            type = UseType.Use,
            description = "使全數值呈倍數上升，但會不受控。\n狂化－100回合",
            price = 1000,
            count = 1
        };
    }

    public static class Material
    {
        public static ItemBaseData SlimeGel = new()
        {
            name = "史萊姆凝膠",
            id = 201,
            type = MaterialType.Material,
            description = "史萊姆的一部分，軟軟的帶點彈力。",
            price = 10,
            ability = new()
            {
                VIT = 1
            },
            count = 1
        };

        public static ItemBaseData WhiteRabbitFur = new()
        {
            name = "白兔毛皮",
            id = 202,
            type = MaterialType.Material,
            description = "白兔的毛皮，感受到些微的庇護。",
            price = 10,
            ability = new()
            {
                LUK = 1
            },
            count = 1
        };

        public static ItemBaseData SparrowFeather = new()
        {
            name = "麻雀羽毛",
            id = 203,
            type = MaterialType.Material,
            description = "麻雀的羽毛，輕巧的可以隨風飛翔。",
            price = 10,
            ability = new()
            {
                SPD = 1
            },
            count = 1
        };
    }

    // public static void CopyFields<T>(T source, T target)
    // {
    //     var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance);
    //     foreach (var field in fields)
    //     {
    //         field.SetValue(target, field.GetValue(source));
    //     }
    // }
}
