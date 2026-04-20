using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PanelLoading : MonoBehaviour
{
    static PanelLoading _ins;
    static int _loadingCounter;
    const string resourcePath = "Prefabs/PanelLoading";
    const string _loadingText = "讀取中";

    [SerializeField] Text _text;

    int _dotCounter;
    float _dt;

    public static void Create()
    {
        // Debug.Log("loading open track");
        if (_ins == null || !_ins.gameObject.activeSelf)
            _ins = ObjectPool.Get(Resources.Load<GameObject>(resourcePath).GetComponent<PanelLoading>(), MainController.Instance.Loading);
        else
            _loadingCounter++;
    }
    public static void Close()
    {
        // Debug.Log("loading close track");
        if (_loadingCounter == 0)
            ObjectPool.Put(_ins);
        else
            _loadingCounter--;
    }

    void OnEnable()
    {
        _text.text = _loadingText;
        _dotCounter = 0;
        _dt = 0;
    }

    void Update()
    {
        _dt += Time.deltaTime;
        if (_dt >= 0.5f)
        {
            _dt = 0;
            _dotCounter++;
            if (_dotCounter > 3)
                _dotCounter = 0;

            _text.text = _loadingText + new string('.', _dotCounter);
        }
    }
}