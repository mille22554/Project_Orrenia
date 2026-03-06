using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class PanelLog : MonoBehaviour
{
    public ScrollRect log;
    public Toggle toggleBattleLog;
    public Toggle toggleEffect;
    public Transform battleLogContent;
    public Transform effectContent;
    public GameObject block;
    public Text itemLog;

    private void Start()
    {
        ClearBattleLog();
        foreach (Transform child in effectContent)
            Destroy(child.gameObject);

        toggleEffect.isOn = true;
        toggleBattleLog.isOn = true;

        toggleBattleLog.onValueChanged.AddListener(OnBattleLog);
        toggleEffect.onValueChanged.AddListener(OnEffect);

        block.SetActive(false);
    }

    private void OnDisable()
    {
        ClearBattleLog();
        foreach (Transform child in effectContent)
            Destroy(child.gameObject);
    }

    public async UniTask SetLog(string message) => await SetLog(message, Color.white);
    public async UniTask SetLog(string message, Color color)
    {
        block.SetActive(true);
        var textLog = Instantiate(itemLog, battleLogContent);
        textLog.text = message;
        textLog.color = color;

        if (toggleBattleLog.isOn)
        {
            await UniTask.Yield();
            log.verticalNormalizedPosition = 0;
            await UniTask.WaitForSeconds(0.1f);
        }
        else if (toggleEffect.isOn)
            OnEffect(true);

        block.SetActive(false);
    }

    private async void OnBattleLog(bool isOn)
    {
        if (isOn)
        {
            await UniTask.Yield();
            log.verticalNormalizedPosition = 0;
        }
    }

    private void OnEffect(bool isOn)
    {
        foreach (Transform child in effectContent)
            Destroy(child.gameObject);

        battleLogContent.gameObject.SetActive(!isOn);
        effectContent.gameObject.SetActive(isOn);

        if (isOn)
        {
            Text textLog;
            if (GameData.NowPlayerData.effects.Count > 0)
            {
                textLog = Instantiate(itemLog, effectContent);
                textLog.text = $"{GameData.NowPlayerData.PlayerName}:";

                foreach (var effect in GameData.NowPlayerData.effects)
                {
                    textLog = Instantiate(itemLog, effectContent);
                    textLog.text = $"{effect.type}－{effect.times}回合";
                }
            }

            foreach (var enemy in GameData.NowEnemyData.enemies)
            {
                if (enemy.effects.Count > 0)
                {
                    textLog = Instantiate(itemLog, effectContent);
                    textLog.text = $"{enemy.name}:";

                    foreach (var effect in enemy.effects)
                    {
                        textLog = Instantiate(itemLog, effectContent);
                        textLog.text = $"{effect.type}－{effect.times}回合";
                    }
                }
            }
        }
    }

    public void ClearBattleLog()
    {
        foreach (Transform child in battleLogContent)
            Destroy(child.gameObject);
    }
}