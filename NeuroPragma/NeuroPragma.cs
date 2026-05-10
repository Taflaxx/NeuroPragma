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
        var puzzleUnit = ManagedObject.ToManagedObject(args[2])?.As<PuzzleUnit>();
        if (puzzleUnit.MyPuzzleSnake != null)
        {                    
            NeuroSDKCsharp.Messages.Outgoing.Context.Send($"Solve the following puzzle manually by using the \"hack_enemy\" action or automatically using the \"use_auto_hack\" action:\n{getSnakePuzzleGrid(puzzleUnit.MyPuzzleSnake)}", false);
            NeuroActionHandler.RegisterActions(new UseAutoHack(puzzleControl!), new HackEnemy(puzzleControl, puzzleUnit));
        }        
        return PreHookResult.Continue;
    }


    /// <summary>
    /// Unregisters the use auto hack action when a puzzle is finished or interrupted
    /// </summary>
    [MethodHook(typeof(app.PuzzleUnit), nameof(app.PuzzleUnit.finishPuzzle), MethodHookType.Pre)]
    static PreHookResult PuzzleUnitFinishPuzzle(Span<ulong> args)
    {
        NeuroActionHandler.UnregisterActions("use_auto_hack", "hack_enemy");
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
    /// Reads the grid of the snake puzzle.
    /// 
    /// TODO: check if all GridType are handled correctly
    /// </summary>
    static string getSnakePuzzleGrid(PuzzleSnake puzzleSnake)
    {
        PuzzleSnake.Grid_Array1D_Array1D grid2D = puzzleSnake._GridAccessor._GridController._ActualGrid;
        string puzzle = $"Grid size: {puzzleSnake.GRID_ACTUAL_SIZE_X} x {puzzleSnake.GRID_ACTUAL_SIZE_Y}\n";
        API.LogInfo($"Grid size: {puzzleSnake.GRID_ACTUAL_SIZE_X} x {puzzleSnake.GRID_ACTUAL_SIZE_Y}");
        HashSet<String> gridTypes = new HashSet<string>();
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
                else if (!field.canEnter() || field.GridType == PuzzleSnakeGridType.Obstacle || field.GridType == PuzzleSnakeGridType.Impassable)
                {
                    puzzle += "X ";
                }
                else
                {
                    gridTypes.Add(PuzzleSnakeGridType.getName(field.GridType));
                    puzzle += PuzzleSnakeGridType.getName(field.GridType)[0] + " ";
                }
            }
            puzzle += "\n";
        }
        API.LogInfo($"Puzzle:\n{puzzle}");
        API.LogInfo($"Grid Types: {string.Join(", ", gridTypes)}");
        puzzle += $"Grid Types: {string.Join(", ", gridTypes)}\n";
        return puzzle;
    }
}
