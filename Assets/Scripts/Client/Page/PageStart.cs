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

        PanelLoading.Create(PanelLoading.BGType.Full);
        var requestData = new GetSaveDataRequest
        {
            Account = DataCenter.Account,
        };
        APIController.Ins.Send(requestData, CallBack);

        void CallBack(GetSaveDataResponse response)
        {
            if (response.Code == 0)
            {
                if (string.IsNullOrEmpty(response.SaveData.CharacterData.Name))
                    PageRegister.Create();
                else
                    MainController.Instance.Login();
            }

            PanelLoading.Close();
        }
    }
}