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
        var requestData = new GetSaveDataRequest();
        ApiBridge.Send(requestData, CallBack);

        void CallBack(GetSaveDataResponse response)
        {
            var saveData = response.SaveData;

            if (string.IsNullOrEmpty(saveData.Datas.CharacterData.Name))
                PageRegister.Create();
            else
                MainController.Instance.Login();
        }
    }
}