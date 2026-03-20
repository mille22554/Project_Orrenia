using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class ServerController : MonoBehaviour
{
    public static ServerController Instance { get; private set; }

    readonly static Dictionary<string, IApiHandler_Server> handlerBases = new();

    void Awake()
    {
        Instance = this;
        RegisterHandlers();
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

    public string Get(string request)
    {
        var requestData = JsonConvert.DeserializeObject<RequestData_Server>(request);

        if (handlerBases.TryGetValue(requestData.cmd, out var handlerBase))
        {
            return handlerBase.Get(requestData.data);
        }
        else
        {
            Debug.LogError($"No handler found for {requestData.cmd}");
            return "";
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