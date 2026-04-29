namespace NeuroPragma;

using app;
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


    /// <summary>
    /// Reads the grid of the hacking puzzle.
    /// 
    /// TODO: check if all GridType are handled correctly, send as action to Neuro
    /// </summary>
    [MethodHook(typeof(app.PuzzleSnake), nameof(app.PuzzleSnake.onStartPuzzle), MethodHookType.Pre)]
    static PreHookResult readPuzzleGrid(Span<ulong> args)
    {
        API.LogInfo($"Snake puzzle started");
        PuzzleSnake puzzleSnake = ManagedObject.ToManagedObject(args[1])?.As<PuzzleSnake>()!;
        PuzzleSnake.Grid_Array1D_Array1D grid2D = puzzleSnake._GridAccessor._GridController._ActualGrid;
        string puzzle = "";
        API.LogInfo($"Grid size: {puzzleSnake.GRID_ACTUAL_SIZE_X} x {puzzleSnake.GRID_ACTUAL_SIZE_Y}");
        for (int j = 0; j < puzzleSnake.GRID_ACTUAL_SIZE_Y; j++)
        {
            for (int i = 0; i < puzzleSnake.GRID_ACTUAL_SIZE_X; i++)
            {
                PuzzleSnake.Grid field = grid2D.Get(i).Get(j);
                // Ignore finish blow fields if finish blow is not ready
                if (field.GridType == PuzzleSnakeGridType.None || field.IsHide)
                {
                    puzzle += ". ";
                }
                else if (!field.canEnter())
                {
                    puzzle += "X ";
                }
                else
                {
                    puzzle += PuzzleSnakeGridType.getName(field.GridType)[0] + " ";
                }
            }
            puzzle += "\n";
        }
        API.LogInfo($"Puzzle:\n{puzzle}");

        return PreHookResult.Continue;
    }
}
