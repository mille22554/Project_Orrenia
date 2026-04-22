using UnityEngine;
using UnityEngine.UI;

public class PageStart : MonoBehaviour
{
    const string resourcePath = "Prefabs/PageStart";

    [SerializeField] InputField _inputUserAccount;
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
        if (string.IsNullOrEmpty(_inputUserAccount.text) || _inputUserAccount.text.Length < 8)
        {
            ItemToastMessage.Create("帳號不可為空，最小長度須為8");
            return;
        }

        DataCenter.Account = _inputUserAccount.text;

        var requestData = new GetSaveDataRequest();
        ApiBridge.Send(requestData, CallBack);
        PanelLoading.Create(PanelLoading.BGType.Full);

        void CallBack(GetSaveDataResponse response)
        {
            var saveData = response.SaveData;

            if (string.IsNullOrEmpty(saveData.CharacterData.Name))
                PageRegister.Create();
            else
                MainController.Instance.Login();

            PanelLoading.Close();
        }
    }
}