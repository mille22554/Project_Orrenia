using UnityEngine;
using UnityEngine.UI;

public class PanelBtns : MonoBehaviour
{
    public Button btnBattle;
    public Button btnCharacter;
    public Button btnBag;
    public Button btnForge;

    private void Start()
    {
        btnBattle.onClick.AddListener(OnBattle);
        btnCharacter.onClick.AddListener(OnCharacter);
        btnBag.onClick.AddListener(OnBag);
        btnForge.onClick.AddListener(OnForge);
    }

    private void OnBattle()
    {
        EventMng.EmitEvent(EventName.Switch_Main_Panel, PanelName.Battle);
    }

    private void OnCharacter()
    {
        EventMng.EmitEvent(EventName.Switch_Main_Panel, PanelName.Character);
    }

    private void OnBag()
    {
        EventMng.EmitEvent(EventName.Switch_Main_Panel, PanelName.Bag);
    }

    private void OnForge()
    {
        EventMng.EmitEvent(EventName.Switch_Main_Panel, PanelName.Forge);
    }
}