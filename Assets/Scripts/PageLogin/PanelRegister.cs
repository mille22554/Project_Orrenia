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
        GameData.gameData = new()
        {
            version = GameData.version,
            datas = new()
            {
                playerData = new()
                {
                    name = inputUsername.text
                }
            }
        };
        PublicFunc.SaveData();
        EventMng.EmitEvent(EventName.SwitchPage, PageName.Main);
    }
}