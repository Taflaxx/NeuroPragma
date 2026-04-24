using System.Net.WebSockets;
using System.Text;
using NeuroSDKCsharp.Json;
using NeuroSDKCsharp.Messages.API;
using NeuroSDKCsharp.Utilities;
using Newtonsoft.Json.Linq;
using REFrameworkNET;

namespace NeuroSDKCsharp.Websocket;

public class WebsocketHandler
{
    private bool _tryingReconnect;
    private const float ReconnectInterval = 30;
    
    private static WebsocketHandler? _instance;

    public WebsocketHandler(string gameName, string? uriString)
    {
        GameName = gameName;
        _messageQueue = new MessageQueue();
        _commandHandler = new CommandHandler();
        _uriString = uriString;
    }
    public static WebsocketHandler? Instance
    {
        get
        {
            if (_instance is null) REFrameworkNET.API.LogError("WebsocketHandlerInstance was accessed without an instance being present");
            return _instance;
        }
        private set => _instance = value;
    }

    private ClientWebSocket? _webSocket = new();

    public readonly string GameName; // will be used for Messages
    private readonly MessageQueue _messageQueue;
    private readonly CommandHandler _commandHandler;

    private string? _uriString; // this will be changed to be able to be changed through file in future
    public async void Initialize()
    {
        try
        {
            await StartWs();
        }
        catch (Exception e)
        {
            REFrameworkNET.API.LogError($"issue in initialize: {e}");
        }
    }

    private async Task Reconnect(bool fromUpdate = false)
    {
        if (_tryingReconnect && fromUpdate) return;
        _tryingReconnect = true;
        await Task.Delay(TimeSpan.FromSeconds(ReconnectInterval));
        await StartWs();
    }

    private Task? _connectTask;
    private async Task StartWs()
    {
        Instance = this;
        
        try
        {
            if (_webSocket!.State is WebSocketState.Open or WebSocketState.Connecting)
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Websocket has closed, as one is already open.", CancellationToken.None);
        }
        catch (Exception e)
        {
            REFrameworkNET.API.LogError($"issue with closing websocket if already open: {e}");
            throw;
        }

        if (_uriString is null or "")
        {
            _uriString = Environment.GetEnvironmentVariable("NEURO_SDK_WS_URL", EnvironmentVariableTarget.Process) ??
                         Environment.GetEnvironmentVariable("NEURO_SDK_WS_URL", EnvironmentVariableTarget.User) ??
                         Environment.GetEnvironmentVariable("NEURO_SDK_WS_URL", EnvironmentVariableTarget.Machine);
            REFrameworkNET.API.LogInfo($"UriString: {_uriString}");
        }
        
        if (_uriString is null or "")
        {
            REFrameworkNET.API.LogError("Could not get websocket URL. You need to set the NEURO_SDK_WS_URL environment variable");
            return;
        }

        _webSocket = new ClientWebSocket();
        Uri websocketUri = new Uri(_uriString);

        _webSocket.Options.KeepAliveInterval = TimeSpan.FromMinutes(10); // should substitute ping pong
        
        try
        {
            _connectTask = _webSocket.ConnectAsync(websocketUri, CancellationToken.None);
            _connectTask.Wait();

            REFrameworkNET.API.LogInfo($"Starting Task    Websocket state: {_webSocket.State}");
            
            Task.Run(() => ReceiveMessage());
            _tryingReconnect = false;
        }
        catch (Exception e)
        {
            if (e is WebSocketException we && we.WebSocketErrorCode is WebSocketError.Faulted)
            {
                REFrameworkNET.API.LogError($"Error code is {we.WebSocketErrorCode}  message: {we.Message}  error code: {we.ErrorCode}");
                Task.Run(() => Reconnect());
            }
        }
    }

    private async Task SendTask(OutgoingMessageHandler handler)
    {
        WsMessage wsMessage = handler.GetWsMessage(); 
        
        string message = JsonSerialize.Serialize(wsMessage);
        
        REFrameworkNET.API.LogInfo($"Sending the Ws Message {message}");

        var sendBytes = Encoding.UTF8.GetBytes(message);
        
        try
        {
            await _webSocket!.SendAsync(sendBytes, WebSocketMessageType.Text, true, CancellationToken.None);
        }
        catch (Exception e)
        {
            REFrameworkNET.API.LogError($"error when sending message: {e}");
            _messageQueue.Enqueue(handler);
        }
    }

    public void Send(OutgoingMessageHandler messageHandler) => _messageQueue.Enqueue(messageHandler);

    public async Task SendImmediate(OutgoingMessageHandler messageHandler)
    {
        string message = JsonSerialize.Serialize(messageHandler.GetWsMessage());

        if (_webSocket is null) return;
        
        if (_webSocket.State is not WebSocketState.Open)
        {
            REFrameworkNET.API.LogError($"Websocket is not open. Could not send message: {message}");
        }

        REFrameworkNET.API.LogInfo($"Sending Immediate message {message}");

        var sendBytes = Encoding.UTF8.GetBytes(message);
        await _webSocket!.SendAsync(sendBytes, WebSocketMessageType.Text, false, CancellationToken.None);
    }

    private async Task ReceiveMessage()
    {
        REFrameworkNET.API.LogInfo($"Start of ReceiveMessage");

        if (_webSocket is null) return;
        
        var buffer = new byte[1024 * 4];
        
        while (_webSocket.State == WebSocketState.Open)
        {
            WebSocketReceiveResult result;
            MemoryStream memoryStream = new MemoryStream();
            do
            {
                result = await _webSocket.ReceiveAsync(buffer, CancellationToken.None);
                memoryStream.Write(buffer,0,result.Count);
            } while (!result.EndOfMessage);
            
            REFrameworkNET.API.LogInfo($"Receive message result: {result} || {result.MessageType}  || {result.CloseStatus}");
            
            if (result.MessageType == WebSocketMessageType.Close)
            {
                REFrameworkNET.API.LogWarning("Server closed connection.");
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                break;
            }

            var memoryStreamArray = memoryStream.ToArray();
            var messageData = Encoding.UTF8.GetString(memoryStreamArray,0,memoryStreamArray.Length);

            await TaskDispatcher.SwitchToMainThread();
            GetMessage(messageData);
        }
    }

    public async void Update()
    {
        TaskDispatcher.RunPending();
        if (_connectTask is not null && _connectTask.IsCompleted)
        {
            REFrameworkNET.API.LogInfo(_connectTask.IsFaulted ? "Issue with connecting, trying again." : "Connected successfully!");

            _tryingReconnect = false;
            _connectTask = null;
        }
        
        if (_webSocket is { State: WebSocketState.Closed or WebSocketState.Aborted or WebSocketState.CloseReceived or WebSocketState.CloseSent} or null) // Best I can get to OnClose event with default websocket :(
        {
            Task.Run(() => Reconnect(true));
            return;
        }
        
        try
        {
            if (_webSocket is null) throw new NullReferenceException("Websocket was null.");
            
            if (_webSocket.State != WebSocketState.Open) return;

            while (_messageQueue.Count > 0)
            {
                OutgoingMessageHandler handler = _messageQueue.Dequeue()!;
                await SendTask(handler);
            }
        }
        catch (Exception e)
        {
            REFrameworkNET.API.LogError($"Issue in update of ws: {e}");
        }
    }
    
    private void GetMessage(string messageData)
    {
        try
        {
            Dictionary<string, object> dataArray = ProcessJsonMessage(messageData);
            _commandHandler.Handle((string)dataArray["command"], (IncomingData)dataArray["data"]);
        }
        catch (Exception e)
        {
            REFrameworkNET.API.LogError($"Error in GetMessage try   {e}");
        }
    }

    public Dictionary<string, object> ProcessJsonMessage(string messageData)
    {
        JObject message = JObject.Parse(messageData);
        
        string? command = message["command"]?.Value<string>();
        IncomingData data = new(message["data"]);

        if (command is null)
        {
            REFrameworkNET.API.LogWarning("Received command that could not be deserialized.");
            return new();
        }
        
        Dictionary<string, object> dataDictionary = new Dictionary<string, object>{{"message", message},{"command",command},{"data",data}};
        return dataDictionary;
    }
}