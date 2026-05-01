using app;
using NeuroSDKCsharp.Actions;
using NeuroSDKCsharp.Json;
using NeuroSDKCsharp.Websocket;

public class UseAutoHack : NeuroAction
{
    public override string Name => "use_auto_hack";

    protected override string Description => "Use Auto Hack";

    protected override JsonSchema Schema => new();

    private PlayerPuzzleControlDriver _puzzleDriver;

    public UseAutoHack(PlayerPuzzleControlDriver puzzleDriver) : base()
    {
        _puzzleDriver = puzzleDriver;
    }

    protected override ExecutionResult Validate(ActionData actionData)
    {
        return ExecutionResult.Success();
    }

    protected override Task Execute()
    {
        REFrameworkNET.API.LogInfo("Using auto hack");
        _puzzleDriver._AutoHackWork.startAutoHack();
        return Task.CompletedTask;
    }
}