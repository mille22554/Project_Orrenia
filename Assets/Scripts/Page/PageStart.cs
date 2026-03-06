using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public class PageStart : MonoBehaviour
{
    const string resourcePath = "Prefabs/PageStart";
    [SerializeField] Button btnStart;

    public static void Create()
    {
        var page = ObjectPool.Get(Resources.Load<GameObject>(resourcePath).GetComponent<PageStart>(), MainController.Instance.PageContent);
        MainController.Instance.SwitchPage(page);
    }

    void Awake()
    {
        btnStart.onClick.AddListener(OnStart);
    }

    void OnStart()
    {
        string path = GameData.SaveDataPath;
        Debug.Log($"從 {path} 讀取遊戲資料");

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            var data = JsonConvert.DeserializeObject<GameSaveData>(json);

            if (data.version != GameData.version)
            {
                GameData.gameData = PublicFunc.UpdateSaveData(data);
                PublicFunc.SaveData();
            }
            else
            {
                GameData.gameData = data;
            }

            PublicFunc.CheckFlags();
            PublicFunc.SetPlayerAbility();

            MainController.Instance.Login();
        }
        else
        {
            PageRegister.Create();
        }
    }
}