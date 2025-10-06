using UnityEngine;
using UnityEngine.UI;

public class PanelBtns : MonoBehaviour
{
    public Button btnBattle;
    public Button btnCharacter;

    private void Start()
    {
        btnBattle.onClick.AddListener(OnBattle);
        btnCharacter.onClick.AddListener(OnCharacter);
    }

    private void OnDestroy()
    {
        btnBattle.onClick.RemoveListener(OnBattle);
        btnCharacter.onClick.RemoveListener(OnCharacter);
    }

    private void OnBattle()
    {
        EventMng.EmitEvent(EventName.Switch_Main_Panel, PanelName.Battle);
    }

    private void OnCharacter()
    {
        EventMng.EmitEvent(EventName.Switch_Main_Panel, PanelName.Character);
    }
}