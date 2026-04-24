namespace NeuroPragma;

using app;
using app.gui;
using REFrameworkNET;
using REFrameworkNET.Attributes;
public class Context
{
    /// <summary>
    /// Sends the dialog as context
    /// </summary>
    [MethodHook(typeof(app.gui.ui2010), nameof(app.gui.ui2010.push), MethodHookType.Pre)]
    static PreHookResult OnDialog(Span<ulong> args)
    {
        var param = ManagedObject.ToManagedObject(args[2])?.As<ui2010.OpenParam>(); ;
        NeuroSDKCsharp.Messages.Outgoing.Context.Send($"{MessageManager.getName(param.SpeakerNameMsgId)}: {MessageManager.getMessage(param.BodyMsgId)}");
        return PreHookResult.Continue;
    }

    /// <summary>
    /// Sends context when taking damage
    /// </summary>
    [MethodHook(typeof(app.PlayerUpdater), nameof(app.PlayerUpdater.onHitDamage), MethodHookType.Pre)]
    static PreHookResult OnDamage(Span<ulong> args)
    {
        var damageInfo = ManagedObject.ToManagedObject(args[2])?.As<HitController.IDamageInfo>();
        NeuroSDKCsharp.Messages.Outgoing.Context.Send($"Took {damageInfo?.Damage} damage", true);
        return PreHookResult.Continue;
    }

    /// <summary>
    /// Sends context when the player dies
    /// </summary>
    [MethodHook(typeof(app.PlayerUpdater), nameof(app.PlayerUpdater.onDead), MethodHookType.Pre)]
    static PreHookResult OnDead(Span<ulong> args)
    {
        NeuroSDKCsharp.Messages.Outgoing.Context.Send($"Player died");
        return PreHookResult.Continue;
    }   
}