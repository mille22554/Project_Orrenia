using System.Reflection;

public static class  GameItemData
{
    public static class Equip
    {
        public static ItemBaseData BasicDagger = new()
        {
            Name = "初始短刀",
            ID = 1,
            Type = EquipType.One_Hand_Weapon.Dagger,
            Description = "新手冒險者的標配，感覺拿著就能受到一點保佑。",
            Price = 500,
            Ability = new()
            {
                ATK = 10,
                LUK = 10
            },
            Durability = 500,
            Count = 1
        };

        public static ItemBaseData WoodenSword = new()
        {
            Name = "木製長劍",
            ID = 2,
            Type = EquipType.One_Hand_Weapon.Sword,
            Description = "訓練用的木劍，比空手好一點。",
            Price = 500,
            Ability = new()
            {
                ATK = 1,
            },
            Durability = 500,
            Count = 1
        };

        public static ItemBaseData WoodenShield = new()
        {
            Name = "木盾",
            ID = 3,
            Type = EquipType.Shield,
            Description = "訓練用的木盾，盾反甚麼的別想了。",
            Price = 500,
            Ability = new()
            {
                DEF = 1,
            },
            Durability = 500,
            Count = 1
        };

        public static ItemBaseData WoodenHelmet = new()
        {
            Name = "木製頭盔",
            ID = 4,
            Type = EquipType.Helmet,
            Description = "訓練用的木盔，戴起來沒有很舒服。",
            Price = 500,
            Ability = new()
            {
                DEF = 1,
            },
            Durability = 500,
            Count = 1
        };

        public static ItemBaseData ClothArmor = new()
        {
            Name = "布製護甲",
            ID = 5,
            Type = EquipType.Armor,
            Description = "訓練用的布甲，木劍程度的傷害還是可以防住的。",
            Price = 500,
            Ability = new()
            {
                DEF = 1,
            },
            Durability = 500,
            Count = 1
        };

        public static ItemBaseData ClothGreaves = new()
        {
            Name = "布製護腿",
            ID = 6,
            Type = EquipType.Greaves,
            Description = "訓練用的護腿，不用怕打到小腿。",
            Price = 500,
            Ability = new()
            {
                DEF = 1,
            },
            Durability = 500,
            Count = 1
        };

        public static ItemBaseData ClothBoots = new()
        {
            Name = "布靴",
            ID = 7,
            Type = EquipType.Shoes,
            Description = "訓練用的布靴，比赤腳好一點。",
            Price = 500,
            Ability = new()
            {
                SPD = 1,
            },
            Durability = 500,
            Count = 1
        };
    }

    public static class Use
    {
        public static ItemBaseData SmallHpPotion = new()
        {
            Name = "小型生命藥水",
            ID = 101,
            Type = UseType.Use,
            Description = "恢復少量生命。",
            Price = 50,
            Ability = new()
            {
                HP = 50
            },
            Count = 1
        };

        public static ItemBaseData SmallMpPotion = new()
        {
            Name = "小型魔力藥水",
            ID = 102,
            Type = UseType.Use,
            Description = "恢復少量魔力。",
            Price = 30,
            Ability = new()
            {
                MP = 15
            },
            Count = 1
        };

        public static ItemBaseData SmallSTAPotion = new()
        {
            Name = "小型體力藥水",
            ID = 103,
            Type = UseType.Use,
            Description = "恢復少量體力。",
            Price = 50,
            Ability = new()
            {
                STA = 10
            },
            Count = 1
        };

        public static ItemBaseData BerserkPotion = new()
        {
            Name = "狂暴藥水",
            ID = 104,
            Type = UseType.Use,
            Description = "使全數值呈倍數上升，但會不受控。\n狂化－100回合",
            Price = 1000,
            Count = 1
        };
    }

    public static class Material
    {
        public static ItemBaseData SlimeGel = new()
        {
            Name = "史萊姆凝膠",
            ID = 201,
            Type = MaterialType.Material,
            Description = "史萊姆的一部分，軟軟的帶點彈力。",
            Price = 10,
            Ability = new()
            {
                VIT = 1
            },
            Count = 1
        };

        public static ItemBaseData WhiteRabbitFur = new()
        {
            Name = "白兔毛皮",
            ID = 202,
            Type = MaterialType.Material,
            Description = "白兔的毛皮，感受到些微的庇護。",
            Price = 10,
            Ability = new()
            {
                LUK = 1
            },
            Count = 1
        };

        public static ItemBaseData SparrowFeather = new()
        {
            Name = "麻雀羽毛",
            ID = 203,
            Type = MaterialType.Material,
            Description = "麻雀的羽毛，輕巧的可以隨風飛翔。",
            Price = 10,
            Ability = new()
            {
                SPD = 1
            },
            Count = 1
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
