using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;
using Object = UnityEngine.Object;

public static class ApiBridge
{
    public static bool IsLocal { get; private set; }

    static string _serverUrl;
    static ServerController _server;
    readonly static Dictionary<Type, IApiHandlerBase> handlerBases = new();

    public static void Initialize(string serverUrl)
    {
        _serverUrl = serverUrl;
        RegisterHandlers();

        IsLocal = string.IsNullOrEmpty(_serverUrl);
        if (IsLocal)
        {
            _server = new GameObject("ServerController").AddComponent<ServerController>();
            Object.DontDestroyOnLoad(_server.gameObject);
        }
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
            var requestType = typeof(IRequestBase<>).MakeGenericType(responseType);

            var instance = (IApiHandlerBase)Activator.CreateInstance(type);

            Debug.Log($"Register handler: {type.Name} -> {requestType.Name}");

            handlerBases[requestType] = instance;
        }
    }

    public static async void Send<T>(IRequestBase<T> request, Action<T> callback)
    {
        if (handlerBases.TryGetValue(typeof(IRequestBase<T>), out var handlerBase))
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
                responseJson = _server.Get(requestJson);
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
            Debug.LogError($"No handler found for {typeof(IRequestBase<T>).Name}");
        }
    }
}

public interface IApiHandler<T> : IApiHandlerBase
{
    T Get(object response);
}

public interface IApiHandlerBase { }

public interface IRequestBase<T>
{
    string Cmd { get; }
}

public class RequestData
{
    public string Cmd;
    public object Data;
}

public class ResponseData
{
    public int Code;
    public object Data;
}