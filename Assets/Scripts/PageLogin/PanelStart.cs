using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public class PanelStart : MonoBehaviour
{
    public Button btnStart;

    private void Start()
    {
        btnStart.onClick.AddListener(OnStart);
    }

    private void OnDestroy()
    {
        btnStart.onClick.RemoveListener(OnStart);
    }

    private void OnStart()
    {
        string path = Path.Combine(Application.persistentDataPath, "savedata.json");
        Debug.Log($"讀取遊戲資料從 {path}");

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            var data = JsonConvert.DeserializeObject<GameSaveData>(json);

            if (data.version != GameData.version)
            {
                GameData.gameData = PublicFunc.UpdateSaveData(data);
                PublicFunc.SaveData();
            }
            else GameData.gameData = data;

            if (!GameData.NowPlayerData.isGetBasicDagger2)
            {
                GameData.NowBagData.items.Add(PublicFunc.GetItem(GameItem.Equip.BasicDagger));
                GameData.NowPlayerData.isGetBasicDagger2 = true;
                PublicFunc.SaveData();
            }

            EventMng.EmitEvent(EventName.SwitchPage, PageName.Main);
        }
        else
        {
            EventMng.EmitEvent(EventName.Login_Start_Switch_To_Register);
        }
    }
}