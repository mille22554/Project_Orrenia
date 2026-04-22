using UnityEngine;
using UnityEngine.UI;

public class PanelLoading : MonoBehaviour
{
    public enum BGType
    {
        None,
        Half,
        Full,
    }

    static PanelLoading _ins;
    static int _loadingCounter;
    const string resourcePath = "Prefabs/PanelLoading";
    const string _loadingText = "讀取中";

    [SerializeField] Text _text;
    [SerializeField] CanvasGroup _bg;

    int _dotCounter;
    float _dt;
    BGType _nowBGType;

    public static void Create(BGType bgType)
    {
        // Debug.Log("loading open track");
        if (_ins == null || !_ins.gameObject.activeSelf)
        {
            _ins = ObjectPool.Get(Resources.Load<GameObject>(resourcePath).GetComponent<PanelLoading>(), MainController.Instance.Loading);
            _ins.SetBG(bgType);
        }
        else
        {
            _loadingCounter++;
            if (bgType > _ins._nowBGType)
                _ins.SetBG(bgType);
        }
    }
    public static void Close()
    {
        // Debug.Log("loading close track");
        if (_loadingCounter == 0)
            ObjectPool.Put(_ins);
        else
            _loadingCounter--;
    }

    void SetBG(BGType bgType)
    {
        _bg.alpha = (float)bgType / 2f;
        _nowBGType = bgType;
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