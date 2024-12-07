using Godot;

public partial class StartGame : Label
{
	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
	}

	public void StartStandard() {
		Globals.GAME_TYPE = GameType.Standard;
		GetTree().ChangeSceneToFile("res://Scenes/Game.tscn");
	}
	
	public void StartAtomic	() {
		Globals.GAME_TYPE = GameType.Atomic;
		GetTree().ChangeSceneToFile("res://Scenes/Game.tscn");
	}
}
