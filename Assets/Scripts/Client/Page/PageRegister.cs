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

        var requestData = new SetPlayerNameRequest { PlayerName = inputUsername.text };
        ApiBridge.Send(requestData, CallBack);
        PanelLoading.Create();

        void CallBack(SetPlayerNameResponse response)
        {
            MainController.Instance.Login();
            PanelLoading.Close();
        }

    }
}