using UnityEngine;
using UnityEngine.UI;

public class PanelBtns : MonoBehaviour
{
    const string resourcePath = "Prefabs/PanelBtns";
    [SerializeField] Button btnBattle;
    [SerializeField] Button btnCharacter;
    [SerializeField] Button btnBag;
    [SerializeField] Button btnForge;

    public static void Create()
    {
        ObjectPool.Get(Resources.Load<GameObject>(resourcePath).GetComponent<PanelBtns>(), MainController.Instance.InfoContent);
    }

    void Awake()
    {
        btnBattle.onClick.AddListener(OnBattle);
        btnCharacter.onClick.AddListener(OnCharacter);
        btnBag.onClick.AddListener(OnBag);
        btnForge.onClick.AddListener(OnForge);
    }

    void OnBattle() => PageBattle.Create();

    void OnCharacter() => PageCharacter.Create();

    void OnBag() => PageBag.Create();

    void OnForge() => PageForge.Create();
}