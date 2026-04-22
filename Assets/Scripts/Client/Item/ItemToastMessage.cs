using UnityEngine;
using UnityEngine.UI;

public class ItemToastMessage : MonoBehaviour
{
    const string resourcePath = "Prefabs/ItemToastMessage";

    [SerializeField] CanvasGroup _canvasGroup;
    [SerializeField] Text _text;

    public static void Create(string message)
    {
        var item = ObjectPool.Get(Resources.Load<GameObject>(resourcePath).GetComponent<ItemToastMessage>(), MainController.Instance.SystemMessage);
        item._text.text = message;
    }

    void OnEnable()
    {
        _canvasGroup.transform.localPosition = Vector3.zero;
        _canvasGroup.alpha = 1;
    }

    void Update()
    {
        if (_canvasGroup.alpha > 0)
        {
            _canvasGroup.transform.localPosition += 50 * Time.deltaTime * Vector3.up;
            _canvasGroup.alpha -= Time.deltaTime / 2;
        }
        else
        {
            ObjectPool.Put(this);
        }
    }
}