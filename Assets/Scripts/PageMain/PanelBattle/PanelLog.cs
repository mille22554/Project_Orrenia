using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class PanelLog : MonoBehaviour
{
    public ScrollRect log;
    public Toggle toggleBattleLog;
    public Toggle toggleEffect;
    public GameObject block;
    public Text itemLog;

    private void Start()
    {
        foreach (Transform child in log.content)
            Destroy(child.gameObject);

        toggleEffect.isOn = true;
        toggleBattleLog.isOn = true;

        toggleEffect.onValueChanged.AddListener(OnEffect);

        block.SetActive(false);
    }

    private void OnDisable()
    {
        foreach (Transform child in log.content)
            Destroy(child.gameObject);
    }

    public async UniTask SetLog(string message) => await SetLog(message, Color.white);
    public async UniTask SetLog(string message, Color color)
    {
        if (toggleBattleLog.isOn)
        {
            block.SetActive(true);
            var textLog = Instantiate(itemLog, log.content);
            textLog.text = message;
            textLog.color = color;
            if (log.content.childCount > 30) Destroy(log.content.GetChild(0).gameObject);

            await UniTask.Yield();
            log.verticalNormalizedPosition = 0;
            await UniTask.WaitForSeconds(0.1f);
            block.SetActive(false);
        }
        else if (toggleEffect.isOn)
            OnEffect(true);
    }

    private void OnEffect(bool isOn)
    {
        foreach (Transform child in log.content)
            Destroy(child.gameObject);

        if (isOn)
        {
            Text textLog;
            if (GameData.NowPlayerData.effects.Count > 0)
            {
                textLog = Instantiate(itemLog, log.content);
                textLog.text = $"{GameData.NowPlayerData.name}:";

                foreach (var effect in GameData.NowPlayerData.effects)
                {
                    textLog = Instantiate(itemLog, log.content);
                    textLog.text = $"{effect.type}－{effect.times}回合";
                }
            }

            foreach (var enemy in GameData.NowEnemyData.enemies)
            {
                if (enemy.effects.Count > 0)
                {
                    textLog = Instantiate(itemLog, log.content);
                    textLog.text = $"{enemy.name}:";

                    foreach (var effect in enemy.effects)
                    {
                        textLog = Instantiate(itemLog, log.content);
                        textLog.text = $"{effect.type}－{effect.times}回合";
                    }
                }
            }
        }
    }
}