namespace NeuroPragma;

using app;
using app.gui;
using NeuroSDKCsharp;
using NeuroSDKCsharp.Actions;
using NeuroSDKCsharp.Websocket;
using REFrameworkNET;
using REFrameworkNET.Attributes;

public class NeuroPragma
{
    [PluginEntryPoint]
    public static void Main()
    {
        // necessary to send the enqueued messages
        // TODO: fix/impove
        Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(16);
                WebsocketHandler.Instance?.Update();
            }
        });
        API.LogInfo("NeuroPragma started");
        SdkSetup.Initialize("NeuroPragma", "ws://localhost:8000");
        NeuroSDKCsharp.Messages.Outgoing.Context.Send("Pragmata was started");
    }

    /// <summary>
    /// Sends the dialog as context
    /// </summary>
    [MethodHook(typeof(app.gui.ui2010), nameof(app.gui.ui2010.push), MethodHookType.Pre)]
    static PreHookResult LogDialog(Span<ulong> args)
    {
        var param = ManagedObject.ToManagedObject(args[2])?.As<ui2010.OpenParam>(); ;
        NeuroSDKCsharp.Messages.Outgoing.Context.Send($"{MessageManager.getName(param.SpeakerNameMsgId)}: {MessageManager.getMessage(param.BodyMsgId)}");
        return PreHookResult.Continue;
    }

    /// <summary>
    /// Registers the use auto hack action when a puzzle is started
    /// </summary>
    [MethodHook(typeof(app.PlayerPuzzleControlDriver), nameof(app.PlayerPuzzleControlDriver.startPuzzle), MethodHookType.Pre)]
    static PreHookResult playerPuzzleControlDriverStartPuzzle(Span<ulong> args)
    {
        var puzzleControl = ManagedObject.ToManagedObject(args[1])?.As<PlayerPuzzleControlDriver>();
        NeuroActionHandler.RegisterActions(new UseAutoHack(puzzleControl!));
        return PreHookResult.Continue;
    }


    /// <summary>
    /// Unregisters the use auto hack action when a puzzle is finished or interrupted
    /// </summary>
    [MethodHook(typeof(app.PuzzleUnit), nameof(app.PuzzleUnit.finishPuzzle), MethodHookType.Pre)]
    static PreHookResult PuzzleUnitFinishPuzzle(Span<ulong> args)
    {
        NeuroActionHandler.UnregisterActions("use_auto_hack");
        return PreHookResult.Continue;
    }

    /// <summary>
    /// Prevents the hacking gauge from being reduced so auto hacking can be used indefinitely
    /// This should be removed if we find a better way to implement hacking
    /// </summary>
    [MethodHook(typeof(app.PlayerPuzzleControlDriver.AutoHackWorkUnit), nameof(app.PlayerPuzzleControlDriver.AutoHackWorkUnit.requestReduceHackingGauge), MethodHookType.Pre)]
    static PreHookResult hackingGaugeUnitReduce(Span<ulong> args)
    {
        return PreHookResult.Skip;
    }
}
