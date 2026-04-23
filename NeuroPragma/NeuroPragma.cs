namespace NeuroPragma;

using app;
using app.gui;
using REFrameworkNET;
using REFrameworkNET.Attributes;

public class NeuroPragma {
    [PluginEntryPoint]
    public static void Main() {
        API.LogInfo("NeuroPragma started");
    }

    //[MethodHook(typeof(app.PuzzleBase), nameof(app.PuzzleBase.onStartPuzzle), MethodHookType.Pre)]
    //static PreHookResult  OnStartPuzzle(Span<ulong> args) {
    //    API.LogInfo($"Starting puzzle 1: {args.ToString()}");
    //    return PreHookResult.Continue;
    //}

    [MethodHook(typeof(app.PuzzleBase), nameof(app.PuzzleBase.startPuzzle), MethodHookType.Pre)]
    static PreHookResult startPuzzle(Span<ulong> args) {
        API.LogInfo($"Starting puzzle 2: {args.ToString()}");
        return PreHookResult.Continue;
    }

    [MethodHook(typeof(app.PuzzleManager), nameof(app.PuzzleManager.createPuzzle), MethodHookType.Pre)]
    static PreHookResult createPuzzle(Span<ulong> args) {
        PuzzleManager a = ManagedObject.ToManagedObject(args[1])?.As<app.PuzzleManager>();
        var instantiateArgs = ManagedObject.ToManagedObject(args[3])?.As<app.PuzzleInstantiateArgs>();
        API.LogInfo($"Create puzzle: {instantiateArgs?.ToString() ?? "Unknown"}");
        return PreHookResult.Continue;
    }

[MethodHook(typeof(app.PuzzleSnake), nameof(app.PuzzleSnake.startPuzzle), MethodHookType.Pre)]
    static PreHookResult startPuzzleSnake(Span<ulong> args) {
        var a = ManagedObject.ToManagedObject(args[1])?.As<app.PuzzleSnake>();;
        args[2] = 1;
        args[3] = 1;
        API.LogInfo($"Create puzzle Snake: {args[2]}, {args[3]}");
        API.LogInfo($"Puzzle Snake Auto: {a.IsJustHackMode}, {a.CanAutoHacking}");
        return PreHookResult.Continue;
    }

    
[MethodHook(typeof(app.PuzzleBase), nameof(app.PuzzleBase.useActiveSkill), MethodHookType.Pre)]
    static PreHookResult puzzleSkill(Span<ulong> args) {
        API.LogInfo($"PuzzleBaseSkill");
        var puzzleBase = ManagedObject.ToManagedObject(args[1])?.As<app.PuzzleBase>();;
        var optionUnitArray = ManagedObject.ToManagedObject(args[2])?.As<app.PuzzleBase.PuzzleOptionUnit_Array1D>();;
        var optionUnit = optionUnitArray?.Get(0);
       
        API.LogInfo($"PuzzleBaseSkill: {optionUnit?.ActiveSkillIndex}, {optionUnit?.Option}, {optionUnit?.Count}");
        return PreHookResult.Continue;
    }

    [MethodHook(typeof(app.gui.ClosedCaptionManager), nameof(app.gui.ClosedCaptionManager.show), MethodHookType.Pre)]
    static PreHookResult ccshow(Span<ulong> args) {
        API.LogInfo($"ccshow: {args[2]}");
        return PreHookResult.Continue;
    }

    [MethodHook(typeof(app.EnhanceManager), nameof(app.EnhanceManager.useActiveSkill), MethodHookType.Pre)]
    static PreHookResult useSkill(Span<ulong> args) {
        API.LogInfo($"UseSkill: {args[2]}");
        return PreHookResult.Continue;
    }

    [MethodHook(typeof(app.GuiManager), nameof(app.GuiManager.openSubTitle), MethodHookType.Pre)]
    static PreHookResult openSubtitlew(Span<ulong> args) {
        var param = ManagedObject.ToManagedObject(args[3])?.As<ui2010.OpenParam>();;
        API.LogInfo($"opensubtitle: {param.SpeakerNameMsgId}, {param.BodyMsgId}");
        GuiDataManager guiDataManager = API.GetManagedSingletonT<app.GuiDataManager>();
        
        API.LogInfo($"opensubtitle common message: {guiDataManager.getCommonMessage((uint)param.BodyMsgId.GetHashCode())} {MessageManager.getMessage(param.BodyMsgId)}");
        return PreHookResult.Continue;
    }


        [MethodHook(typeof(app.MessageManager), nameof(app.MessageManager.requestPlayConvClosedCaption), MethodHookType.Pre)]
    static PreHookResult requestPlayConvClosedCaption(Span<ulong> args) {
        API.LogInfo($"MessageManager requestPlayConvClosedCaption");
        return PreHookResult.Continue;
    }
    

    
    [MethodHook(typeof(app.MessageManager), nameof(app.MessageManager.requestPlayClosedCaption), MethodHookType.Pre)]
    static PreHookResult requestPlayClosedCaption(Span<ulong> args) {
        API.LogInfo($"MessageManager requestPlayClosedCaption");
        return PreHookResult.Continue;
    }
    
        [MethodHook(typeof(app.MessageManager), nameof(app.MessageManager.requestPlayCutSceneClosedCaption), MethodHookType.Pre)]
    static PreHookResult requestPlayCutSceneClosedCaption(Span<ulong> args) {
        API.LogInfo($"MessageManager requestPlayCutSceneClosedCaption");
        return PreHookResult.Continue;
    }

    [MethodHook(typeof(app.Ch14600SoundSubTitleClosedWorker), nameof(app.Ch14600SoundSubTitleClosedWorker.request), MethodHookType.Pre)]
    static PreHookResult ch14600SoundSubTitleClosedWorkerRequest(Span<ulong> args) {
        API.LogInfo($"Ch14600SoundSubTitleClosedWorker request");
        return PreHookResult.Continue;
    }

        [MethodHook(typeof(app.MessageParam), nameof(app.MessageParam.drawSubTitle), MethodHookType.Pre)]
    static PreHookResult messageParamDrawSubTitle(Span<ulong> args) {
        API.LogInfo($"MessageParam drawSubTitle");
        return PreHookResult.Continue;
    }

    // Gets the subtitles and logs them
    [MethodHook(typeof(app.gui.ui2010), nameof(app.gui.ui2010.push), MethodHookType.Pre)]
    static PreHookResult ui2010Push(Span<ulong> args) {
        var param = ManagedObject.ToManagedObject(args[2])?.As<ui2010.OpenParam>();;
        API.LogInfo($"{MessageManager.getName(param.SpeakerNameMsgId)}: {MessageManager.getMessage(param.BodyMsgId)}");
        return PreHookResult.Continue;
    }

    // Automatically triggers auto hacking
    [MethodHook(typeof(app.PlayerPuzzleControlDriver), nameof(app.PlayerPuzzleControlDriver.startPuzzle), MethodHookType.Pre)]
    static PreHookResult playerPuzzleControlDriverStartPuzzle(Span<ulong> args) {
        var puzzleControl = ManagedObject.ToManagedObject(args[1])?.As<PlayerPuzzleControlDriver>();
        puzzleControl._AutoHackWork.startAutoHack();
        return PreHookResult.Continue;
    }

    // Prevents the hacking gauge from being reduced so auto hacking can be used indefinitely
    [MethodHook(typeof(app.PlayerPuzzleControlDriver.AutoHackWorkUnit), nameof(app.PlayerPuzzleControlDriver.AutoHackWorkUnit.requestReduceHackingGauge), MethodHookType.Pre)]
    static PreHookResult hackingGaugeUnitReduce(Span<ulong> args) {
        return PreHookResult.Skip;
    }

    //Potentielle Methoden
    // GuiDataManager.getCommonMessage
    // Type DialogData
    // GuiInputManager lesen vom User Input?
    // Inventory Manager - Item pickup etc?
    // MessageManager
}
