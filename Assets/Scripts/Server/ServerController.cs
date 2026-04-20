using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

public static class ServerController
{
    readonly static Dictionary<string, IApiHandler_Server> handlerBases = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        RegisterHandlers();
        EventMng.SetEvent(EventName.ServerRequest, (Action<string, Action<string>>)Get);
    }

    static void RegisterHandlers()
    {
        var types = typeof(ServerController).Assembly.GetTypes();

        foreach (var type in types)
        {
            if (type.IsInterface || type.IsAbstract)
                continue;

            if (!typeof(IApiHandler_Server).IsAssignableFrom(type))
                continue;

            var instance = (IApiHandler_Server)Activator.CreateInstance(type);

            Debug.Log($"Register server handler: {instance.Cmd} -> {type.Name}");

            handlerBases[instance.Cmd] = instance;
        }
    }

    static async void Get(string request, Action<string> callback)
    {
        await UniTask.NextFrame();
        // await UniTask.WaitForSeconds(0.5f);

        var requestData = JsonConvert.DeserializeObject<RequestData_Server>(request);

        if (handlerBases.TryGetValue(requestData.cmd, out var handlerBase))
        {
            callback.Invoke(handlerBase.Get(requestData.data));
        }
        else
        {
            Debug.LogError($"No handler found for {requestData.cmd}");
            callback.Invoke("");
        }
    }
}

public class RequestData_Server
{
    public string cmd;
    public object data;
}

public class ResponseData_Server
{
    public int Code;
    public object Data = "";
}

public interface IApiHandler_Server
{
    string Cmd { get; }
    string Get(object response);
}