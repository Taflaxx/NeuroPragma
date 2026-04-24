using NeuroSDKCsharp.Actions;
using NeuroSDKCsharp.Utilities;
using NeuroSDKCsharp.Websocket;
using REFrameworkNET;

namespace NeuroSDKCsharp;

public static class SdkSetup
{
    public static void Initialize(string game,string uriString)
    {
	    TaskDispatcher.Initialize();
	    WebsocketHandler ws = new WebsocketHandler(game,uriString);
	    ws.Initialize();
	    
	    ExitApplicationEvent.Initialize();
	    ExitApplicationEvent.ApplicationExiting += NeuroActionHandler.OnApplicationQuit;
    }
}