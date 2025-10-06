using UnityEngine;

public class PageMain : MonoBehaviour
{
    public PanelInfo panelInfo;

    private void OnEnable()
    {
        panelInfo.RefreshInfo();
    }
}