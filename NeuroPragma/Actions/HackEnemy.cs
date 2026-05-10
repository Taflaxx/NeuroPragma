using app;
using app.hid;
using NeuroSDKCsharp.Actions;
using NeuroSDKCsharp.Json;
using NeuroSDKCsharp.Websocket;
using REFrameworkNET;
using via;

public class HackEnemy : NeuroAction<string[]>
{
    public override string Name => "hack_enemy";

    protected override string Description => @"Hack the enemy manually. 
    The directions should be provided as a list of moves (U for up, D for down, L for left, R for right) that will navigate the grid and reach the enemy. 
    Do not include invalid moves such as going outside the grid or reversing direction.
    Navigate from the starting position S to the goal G while collecting as many O and A Nodes as possible.";

    protected override JsonSchema Schema => new()
    {
        Type = JsonSchemaType.Object,
        Required = new List<string> { "directions" },
        Properties = new Dictionary<string, JsonSchema>
        {
            ["directions"] = new JsonSchema
            {
                Type = JsonSchemaType.Array,
                MinItems = 1,
                Items = new JsonSchema
                {
                    Type = JsonSchemaType.String,
                    Enum = new List<object> { "U", "D", "L", "R" },
                }
            }
        }
    };

    private PuzzleUnit _puzzleUnit;
    private PuzzleSnake _puzzleSnake;
    private PlayerPuzzleControlDriver _puzzleControl;

    public HackEnemy(PlayerPuzzleControlDriver puzzleControl, PuzzleUnit puzzleUnit) : base()
    {
        _puzzleUnit = puzzleUnit;
        _puzzleSnake = puzzleUnit.MyPuzzleSnake;
        _puzzleControl = puzzleControl;
    }

    protected override ExecutionResult Validate(ActionData actionData, out string[] resultData)
    {
        resultData = actionData.Data?["directions"]?.ToObject<string[]>() ?? Array.Empty<string>();
        REFrameworkNET.API.LogInfo("directions: " + string.Join(", ", resultData));
        if (resultData.Length == 0)
        {
            return ExecutionResult.Failure("No directions provided");
        }
        return ExecutionResult.Success();
    }

    protected override Task Execute(string[]? directions)
    {
        API.LogInfo("Executing hack_enemy action");
        API.LogInfo($"Recieved directions: {string.Join(", ", directions!)}");
        bool completed = false;
        //var guiInputManager = API.GetNativeSingletonT<GuiInputManager>();
        foreach (string direction in directions!)
        {
            Task.Delay(100).Wait();
            Int2 move = _puzzleSnake._CurrentUnit.Position;
            switch (direction)
            {
                case "U":
                    move.y -= 1;                  
                    break;
                case "D":
                    move.y += 1;
                    break;
                case "R":
                    move.x += 1;
                    break;
                case "L":
                    move.x -= 1;
                    break;
            }
            API.LogInfo($"Moving {direction} => ({move.x}, {move.y})");
            _puzzleSnake._CurrentUnit.move(move);
            if (_puzzleSnake._GridAccessor[move].GridType == PuzzleSnakeGridType.Goal)
            {
                API.LogInfo("Goal reached!");
                _puzzleSnake._RequestForceSuccess = true;
                completed = true;
                break;
            }
        } 
        if (!completed)
        {
            API.LogInfo("Directions executed but goal not reached");
            _puzzleSnake.reset();
        }

        return Task.CompletedTask;
    }
}