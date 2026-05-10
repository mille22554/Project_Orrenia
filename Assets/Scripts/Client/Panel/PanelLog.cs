using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class PanelLog : MonoBehaviour
{
    const string _line = "--------------------";
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

    public void SetLine()
    {
        if ((toggleBattleLog.isOn && itemBattleLogs.Count > 0) || (toggleEffect.isOn && itemEffectLogs.Count > 0))
            SetLog(_line);
    }

    public void SetLog(string message) => SetLog(message, Color.white);
    public void SetLog(string message, Color color) => SetLogAsync(message, color);
    public async void SetLogAsync(string message, Color color)
    {
        Debug.Log(message);
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
            PanelLoading.Create(PanelLoading.BGType.None);
            var requestData = new GetPartyEffectsRequest
            {
                UID = DataCenter.UID,
            };
            APIController.Ins.Send(requestData, CallBack);

            void CallBack(GetPartyEffectsResponse response)
            {
                if (response.Code == 0)
                {
                    var members = response.Members;
                    var enemies = response.Enemies;
                    var effects = response.Effects;

                    Text textLog;
                    foreach (var member in members)
                    {
                        if (effects.Count > 0)
                        {
                            textLog = ObjectPool.Get(itemLog, effectContent);
                            itemEffectLogs.Add(textLog);
                            textLog.text = $"{member.Name}:";
                            textLog.color = Color.white;

                            foreach (var effect in effects.Where(x => x.Owner == member.UID))
                            {
                                textLog = ObjectPool.Get(itemLog, effectContent);
                                itemEffectLogs.Add(textLog);
                                textLog.text = $"{effect.Name}－{effect.Times}回合";
                                textLog.color = Color.white;
                            }
                        }
                    }

                    foreach (var enemy in enemies)
                    {
                        if (effects.Count > 0)
                        {
                            textLog = ObjectPool.Get(itemLog, effectContent);
                            itemEffectLogs.Add(textLog);
                            textLog.text = $"{enemy.Name}:";
                            textLog.color = Color.white;

                            foreach (var effect in effects.Where(x => x.Owner == enemy.UID))
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

                PanelLoading.Close();
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