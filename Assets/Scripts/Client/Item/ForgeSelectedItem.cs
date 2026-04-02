using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class ForgeSelectedItem : MonoBehaviour
{
    public long UID { get; private set; }
    public int Count { get; private set; }

    Text _text;

    void Awake()
    {
        _text = GetComponent<Text>();
    }

    public void SetData(string itemName, long uid, int count)
    {
        UID = uid;
        Count = count;
        _text.text = $"{itemName} x{count}";
    }
}