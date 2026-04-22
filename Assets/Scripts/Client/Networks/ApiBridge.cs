using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

public static class ApiBridge
{
    public static bool IsLocal { get; private set; }

    static string _serverUrl;
    readonly static Dictionary<Type, IApiHandlerBase> handlerBases = new();

    public static void Initialize(string serverUrl)
    {
        _serverUrl = serverUrl;
        RegisterHandlers();

        IsLocal = string.IsNullOrEmpty(_serverUrl);
    }

    static void RegisterHandlers()
    {
        var types = typeof(ApiBridge).Assembly.GetTypes();

        foreach (var type in types)
        {
            if (type.IsInterface || type.IsAbstract)
                continue;

            var handlerInterface = type.GetInterfaces()
                .FirstOrDefault(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IApiHandler<>));

            if (handlerInterface == null)
                continue;

            // 取得 T
            var responseType = handlerInterface.GetGenericArguments()[0];

            // 產生 IRequestBase<T>
            var requestType = typeof(RequestBase<>).MakeGenericType(responseType);

            var instance = (IApiHandlerBase)Activator.CreateInstance(type);

            Debug.Log($"Register handler: {type.Name} -> {requestType.Name}");

            handlerBases[requestType] = instance;
        }
    }

    public static async void Send<T>(RequestBase<T> request, Action<T> callback)
    {
        if (handlerBases.TryGetValue(typeof(RequestBase<T>), out var handlerBase))
        {
            var handler = (IApiHandler<T>)handlerBase;

            var requestData = new RequestData
            {
                Cmd = request.Cmd,
                Data = request
            };
            var requestJson = JsonConvert.SerializeObject(requestData);
            Debug.Log($"送: {requestJson}");

            var responseJson = "";
            if (IsLocal)
            {
                var isDataBack = false;
                EventMng.EmitEvent(EventName.ServerRequest, requestJson, (Action<string>)CallBack);
                await UniTask.WaitUntil(() => isDataBack);

                void CallBack(string response)
                {
                    responseJson = response;
                    isDataBack = true;
                }
            }
            else
            {

            }

            Debug.Log($"收: {responseJson}");
            var response = JsonConvert.DeserializeObject<ResponseData>(responseJson);
            if (response.Code != 0)
            {
                Debug.LogError($"API Error: {response.Data}");
                return;
            }
            else
            {
                var result = handler.Get(response.Data);
                callback(result);
            }
        }
        else
        {
            Debug.LogError($"No handler found for {typeof(RequestBase<T>).Name}");
        }
    }
}

public interface IApiHandler<T> : IApiHandlerBase
{
    T Get(object response);
}

public interface IApiHandlerBase { }

public abstract class RequestBase<T>
{
    public abstract string Cmd { get; }
    public string Account => DataCenter.Account;
}

public struct RequestData
{
    public string Cmd;
    public object Data;
}

public struct ResponseData
{
    public int Code;
    public object Data;
}