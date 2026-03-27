using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public enum EventName
{
    ServerRequest
}

public static class EventMng
{
    // 使用 List 儲存 Delegate，維持參數靈活性
    static readonly Dictionary<EventName, List<Delegate>> _eventTable = new();

    // 記錄正在執行中的事件，處理「邊執行邊刪除」的情況
    static readonly HashSet<EventName> _emittingSet = new();

    public static void SetEvent(EventName eventName, Delegate callback)
    {
        if (callback == null)
            return;

        if (!_eventTable.TryGetValue(eventName, out var list))
        {
            list = new List<Delegate>();
            _eventTable.Add(eventName, list);
        }
        list.Add(callback);
    }

    public static void EmitEvent<T, T2>(EventName eventName, T arg, T2 arg2)
    {
        if (!_eventTable.TryGetValue(eventName, out var list))
            return;

        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (list[i] is Action<T, T2> del)
                del.Invoke(arg, arg2);
            else
                list[i].DynamicInvoke(arg, arg2);
        }
    }

    public static void EmitEvent(EventName eventName, params object[] args)
    {
        if (!_eventTable.TryGetValue(eventName, out var list))
        {
            Debug.LogWarning($"[EventMng] {eventName} 沒有任何註冊者");
            return;
        }

        _emittingSet.Add(eventName);

        // 使用反向遍歷：即使在 Callback 中呼叫 DelEvent 也不會出錯
        for (int i = list.Count - 1; i >= 0; i--)
        {
            try
            {
                // DynamicInvoke 是達成「參數不限」的關鍵
                list[i]?.DynamicInvoke(args);
            }
            catch (Exception e)
            {
                Debug.LogError($"[EventMng] {eventName} 執行錯誤: {e.InnerException ?? e}");
            }
        }

        _emittingSet.Remove(eventName);
    }

    public static void DelEvent(EventName eventName, Delegate callback = null)
    {
        if (!_eventTable.TryGetValue(eventName, out var list))
            return;

        if (callback == null)
        {
            list.Clear();
        }
        else
        {
            list.Remove(callback);
        }
    }
}

// public class EventMng
// {
//     static readonly Dictionary<EventName, EventInfo> events = new();

//     public static void SetEvent(EventName eventName, Delegate callback)
//     {
//         if (events.TryGetValue(eventName, out var eventInfo))
//         {
//             eventInfo.callbacks.Add(callback);
//         }
//         else
//         {
//             eventInfo = new() { callbacks = new() { callback } };
//             events.Add(eventName, eventInfo);
//         }
//     }

//     public static void EmitEvent(EventName eventName, params object[] args)
//     {
//         if (events.TryGetValue(eventName, out var eventInfo))
//         {
//             if (eventInfo.callbacks != null)
//             {
//                 eventInfo.isEmitting = true;
//                 foreach (var callback in eventInfo.callbacks)
//                     callback.DynamicInvoke(args);

//                 eventInfo.isEmitting = false;
//             }
//             else
//             {
//                 Debug.LogWarning($"{eventName}沒有事件");
//             }
//         }
//         else
//         {
//             Debug.LogWarning($"{eventName}沒有事件");
//         }
//     }
//     public static async void DelEvent(EventName eventName, Delegate callback = null)
//     {
//         if (events.TryGetValue(eventName, out var eventInfo))
//         {
//             await UniTask.WaitUntil(() => !eventInfo.isEmitting);
//             if (callback != null)
//                 eventInfo.callbacks.Remove(callback);
//             else
//                 eventInfo.callbacks.Clear();
//         }
//         else
//         {
//             Debug.LogWarning($"{eventName}沒有事件");
//         }
//     }
// }

// public class EventInfo
// {
//     public bool isEmitting = false;
//     public List<Delegate> callbacks;
// }