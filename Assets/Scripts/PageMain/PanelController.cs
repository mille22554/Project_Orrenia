using System;
using System.Collections.Generic;
using UnityEngine;

public class PanelController : MonoBehaviour
{
    public List<MonoBehaviour> panels;
    [NonSerialized] public int currentPanelIndex = 0;

    private void Awake()
    {
        panels.ForEach(page => page.gameObject.SetActive(true));
    }

    private void Start()
    {
        panels.ForEach(panel => panel.gameObject.SetActive(false));
        SwitchPanel(PanelName.Battle);

        EventMng.SetEvent(EventName.Switch_Main_Panel, (Action<PanelName>)SwitchPanel);
    }

    private void OnDestroy()
    {
        EventMng.DelEvent(EventName.Switch_Main_Panel, (Action<PanelName>)SwitchPanel);
    }

    public void SwitchPanel(PanelName panelName)
    {
        panels[currentPanelIndex].gameObject.SetActive(false);
        currentPanelIndex = (int)panelName;
        panels[currentPanelIndex].gameObject.SetActive(true);
    }
}

public enum PanelName
{
    Battle,
    Character,
    Bag,
    Forge,
}
