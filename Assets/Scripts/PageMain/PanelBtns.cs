using UnityEngine;
using UnityEngine.UI;

public class PanelBtns : MonoBehaviour
{
    public Button btnBattle;
    public Button btnCharacter;
    public Button btnBag;

    private void Start()
    {
        btnBattle.onClick.AddListener(OnBattle);
        btnCharacter.onClick.AddListener(OnCharacter);
        btnBag.onClick.AddListener(OnBag);
    }

    private void OnDestroy()
    {
        btnBattle.onClick.RemoveListener(OnBattle);
        btnCharacter.onClick.RemoveListener(OnCharacter);
        btnBag.onClick.RemoveListener(OnBag);
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
}