using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class EventMng
{
    private static readonly Dictionary<EventName, EventInfo> events = new();

    public static void SetEvent(EventName eventName, Delegate callback)
    {
        if (events.TryGetValue(eventName, out var eventInfo)) eventInfo.callbacks.Add(callback);
        else
        {
            eventInfo = new() { callbacks = new() { callback } };
            events.Add(eventName, eventInfo);
        }
    }
    public static void EmitEvent(EventName eventName, params object[] args)
    {
        if (events.TryGetValue(eventName, out var eventInfo))
        {
            if (eventInfo.callbacks != null)
            {
                eventInfo.isEmitting = true;
                foreach (var callback in eventInfo.callbacks) callback.DynamicInvoke(args);
                eventInfo.isEmitting = false;
            }
            else Debug.LogWarning($"{eventName}沒有事件");
        }
        else Debug.LogWarning($"{eventName}沒有事件");
    }
    public static async void DelEvent(EventName eventName, Delegate callback = null)
    {
        if (events.TryGetValue(eventName, out var eventInfo))
        {
            await UniTask.WaitUntil(() => !eventInfo.isEmitting);
            if (callback != null) eventInfo.callbacks.Remove(callback);
            else eventInfo.callbacks.Clear();
        }
        else Debug.LogWarning($"{eventName}沒有事件");
    }
}
public class EventInfo
{
    public bool isEmitting = false;
    public List<Delegate> callbacks;
}
public enum EventName
{
    Login_Start_Switch_To_Register,
    SwitchPage,
    Switch_Main_Panel,
    RefreshPlayerInfo,
    RefreshAbilityPoint
}