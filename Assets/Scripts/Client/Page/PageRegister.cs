using UnityEngine;
using UnityEngine.UI;

public class PageRegister : MonoBehaviour
{
    const string resourcePath = "Prefabs/PageRegister";
    [SerializeField] Button btnRegister;
    [SerializeField] InputField inputUsername;

    public static void Create()
    {
        var page = ObjectPool.Get(Resources.Load<GameObject>(resourcePath).GetComponent<PageRegister>(), MainController.Instance.PageContent);
        MainController.Instance.SwitchPage(page);
    }

    void Awake()
    {
        btnRegister.onClick.AddListener(OnRegister);
    }

    void OnRegister()
    {
        if (string.IsNullOrEmpty(inputUsername.text))
        {
            Debug.LogWarning("Username cannot be empty");
            return;
        }

        PanelLoading.Create(PanelLoading.BGType.Full);
        var requestData = new SetPlayerNameRequest
        {
            Account = DataCenter.Account,
        };
        APIController.Ins.Send(requestData, CallBack);

        void CallBack(SetPlayerNameResponse response)
        {
            if (response.Code == 0)
            {
                MainController.Instance.Login();
            }

            PanelLoading.Close();
        }
    }
}