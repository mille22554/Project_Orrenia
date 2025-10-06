using System.Collections.Generic;

public static class GameData
{
    public static string version = "0.0.3";
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