using app;
using NeuroSDKCsharp.Actions;
using NeuroSDKCsharp.Json;
using NeuroSDKCsharp.Websocket;

public class Scan : NeuroAction
{
    public override string Name => "scan";

    protected override string Description => "Scan the environment for enemies and resources.";

    protected override JsonSchema Schema => new();

    private PlayerActionControlDriver.Scan _scan;

    public Scan(PlayerActionControlDriver.Scan scan) : base()
    {
        _scan = scan;
    }

    protected override ExecutionResult Validate(ActionData actionData)
    {
        if (!_scan.canAction())
        {
            REFrameworkNET.API.LogInfo("Scan not available");
            return ExecutionResult.Failure("Scan not available");
        }
        return ExecutionResult.Success();
    }

    protected override Task Execute()
    {
        REFrameworkNET.API.LogInfo("Using scan");
        _scan.startScan();
        return Task.CompletedTask;
    }
}