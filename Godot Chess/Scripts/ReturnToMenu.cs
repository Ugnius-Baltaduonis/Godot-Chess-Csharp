using Godot;
using System;

public partial class ReturnToMenu : Label
{
	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
	}
	
	public void OnGuiInput(){
		GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn");
	}
}
