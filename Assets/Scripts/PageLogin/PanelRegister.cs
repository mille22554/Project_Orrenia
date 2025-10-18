using UnityEngine;
using UnityEngine.UI;

public class PanelRegister : MonoBehaviour
{
    public Button btnRegister;
    public InputField inputUsername;

    private void Start()
    {
        btnRegister.onClick.AddListener(OnRegister);
    }

    private void OnDestroy()
    {
        btnRegister.onClick.RemoveListener(OnRegister);
    }

    private void OnRegister()
    {
        GameData.gameData = new();
        GameData.NowPlayerData.name = inputUsername.text;
        GameData.NowBagData.items.Add(PublicFunc.GetItem(GameItem.Equip.BasicDagger));
        GameData.NowPlayerData.isGetBasicDagger2 = true;
        PublicFunc.SaveData();
        EventMng.EmitEvent(EventName.SwitchPage, PageName.Main);
    }
}