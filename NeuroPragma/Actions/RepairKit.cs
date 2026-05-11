using app;
using NeuroSDKCsharp.Actions;
using NeuroSDKCsharp.Json;
using NeuroSDKCsharp.Websocket;

public class RepairKit : NeuroAction
{
    public override string Name => "use_repair_kit";

    protected override string Description => "Use a repair kit to recover health.";

    protected override JsonSchema Schema => new();

    private PlayerActionControlDriver _pairActionControlDriver;

    public RepairKit(PlayerActionControlDriver playerActionControlDriver) : base()
    {
        _pairActionControlDriver = playerActionControlDriver;
    }

    protected override ExecutionResult Validate(ActionData actionData)
    {
        if (_pairActionControlDriver.HitPoint.CurrentHitPoint >= _pairActionControlDriver.HitPoint.DefaultHitPoint)
        {
             REFrameworkNET.API.LogInfo("Health is full, repair kit cannot be used: " + _pairActionControlDriver.HitPoint.CurrentHitPoint + " / " + _pairActionControlDriver.HitPoint.DefaultHitPoint + " HP");
             return ExecutionResult.Failure("Health is full, repair kit cannot be used: " + _pairActionControlDriver.HitPoint.CurrentHitPoint + " / " + _pairActionControlDriver.HitPoint.DefaultHitPoint + " HP");
        }
        return ExecutionResult.Success();
    }

    protected override Task Execute()
    {
        REFrameworkNET.API.LogInfo("Using repair kit");
        _pairActionControlDriver.Updater.PlayerItemControlDriver._RepairKitWork.useRepairKit();
        return Task.CompletedTask;
    }
}