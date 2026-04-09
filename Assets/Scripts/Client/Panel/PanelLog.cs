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

    void OnEnable()
    {
        if (toggleBattleLog.isOn)
            OnBattleLog(true);
        else
            OnEffect(true);
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
            var requestData = new GetSaveDataRequest();
            ApiBridge.Send(requestData, CallBack);

            void CallBack(GetSaveDataResponse response)
            {
                var characterData = response.SaveData.Datas.CharacterData;
                var enemies = response.SaveData.Datas.EnemyData.Enemies;

                Text textLog;
                if (characterData.Effects.Count > 0)
                {
                    textLog = ObjectPool.Get(itemLog, effectContent);
                    itemEffectLogs.Add(textLog);
                    textLog.text = $"{characterData.Name}:";
                    textLog.color = Color.white;

                    foreach (var effect in characterData.Effects)
                    {
                        textLog = ObjectPool.Get(itemLog, effectContent);
                        itemEffectLogs.Add(textLog);
                        textLog.text = $"{effect.Name}－{effect.Times}回合";
                        textLog.color = Color.white;
                    }
                }

                foreach (var enemy in enemies)
                {
                    if (enemy.Effects.Count > 0)
                    {
                        textLog = ObjectPool.Get(itemLog, effectContent);
                        itemEffectLogs.Add(textLog);
                        textLog.text = $"{enemy.CharacterData.Name}:";
                        textLog.color = Color.white;

                        foreach (var effect in enemy.Effects)
                        {
                            textLog = ObjectPool.Get(itemLog, effectContent);
                            itemEffectLogs.Add(textLog);
                            textLog.text = $"{effect.Name}－{effect.Times}回合";
                            textLog.color = Color.white;
                        }
                    }
                }
                log.verticalNormalizedPosition = 1;
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