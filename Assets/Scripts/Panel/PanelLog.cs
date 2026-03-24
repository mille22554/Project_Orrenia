using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class PanelLog : MonoBehaviour
{
    [SerializeField] ScrollRect log;
    [SerializeField] Toggle toggleBattleLog;
    [SerializeField] Toggle toggleEffect;
    [SerializeField] Transform battleLogContent;
    [SerializeField] Transform effectContent;
    [SerializeField] GameObject block;
    [SerializeField] Text itemLog;

    CharacterData CharacterData => GameData_Server.NowCharacterData;
    readonly List<Text> itemBattleLogs = new();
    readonly List<Text> itemEffectLogs = new();

    void Awake()
    {
        toggleBattleLog.onValueChanged.AddListener(OnBattleLog);
        toggleEffect.onValueChanged.AddListener(OnEffect);
    }

    void Start()
    {
        ClearBattleLog();
        ClearEffectLog();

        toggleEffect.isOn = true;
        toggleBattleLog.isOn = true;

        block.SetActive(false);
    }

    void OnDisable()
    {
        ClearBattleLog();
        ClearEffectLog();
    }

    public void SetLog(string message) => SetLog(message, Color.white);
    public void SetLog(string message, Color color) => SetLogAsync(message, color).Forget();
    public async UniTask SetLogAsync(string message) => await SetLogAsync(message, Color.white);
    public async UniTask SetLogAsync(string message, Color color)
    {
        block.SetActive(true);
        var textLog = ObjectPool.Get(itemLog, battleLogContent);
        itemBattleLogs.Add(textLog);
        textLog.text = message;
        textLog.color = color;

        if (toggleBattleLog.isOn)
        {
            await UniTask.Yield();
            log.verticalNormalizedPosition = 0;
            await UniTask.WaitForSeconds(0.1f);
        }
        else if (toggleEffect.isOn)
        {
            OnEffect(true);
        }

        block.SetActive(false);
    }

    async void OnBattleLog(bool isOn)
    {
        if (isOn)
        {
            await UniTask.Yield();
            log.verticalNormalizedPosition = 0;
        }
    }

    void OnEffect(bool isOn)
    {
        ClearEffectLog();

        battleLogContent.gameObject.SetActive(!isOn);
        effectContent.gameObject.SetActive(isOn);

        if (isOn)
        {
            Text textLog;
            if (CharacterData.Effects.Count > 0)
            {
                textLog = ObjectPool.Get(itemLog, effectContent);
                itemEffectLogs.Add(textLog);
                textLog.text = $"{CharacterData.Name}:";

                foreach (var effect in CharacterData.Effects)
                {
                    textLog = ObjectPool.Get(itemLog, effectContent);
                    itemEffectLogs.Add(textLog);
                    textLog.text = $"{effect.type}－{effect.times}回合";
                }
            }

            foreach (var enemy in GameData_Server.NowEnemyData.Enemies)
            {
                if (enemy.Effects.Count > 0)
                {
                    textLog = ObjectPool.Get(itemLog, effectContent);
                    itemEffectLogs.Add(textLog);
                    textLog.text = $"{enemy.CharacterData.Name}:";

                    foreach (var effect in enemy.Effects)
                    {
                        textLog = ObjectPool.Get(itemLog, effectContent);
                        itemEffectLogs.Add(textLog);
                        textLog.text = $"{effect.type}－{effect.times}回合";
                    }
                }
            }
        }
    }

    public void ClearBattleLog()
    {
        foreach (var item in itemBattleLogs)
            ObjectPool.Put(item);
    }

    public void ClearEffectLog()
    {
        foreach (var item in itemEffectLogs)
            ObjectPool.Put(item);
    }
}