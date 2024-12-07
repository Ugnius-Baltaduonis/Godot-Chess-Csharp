using Godot;
using System;

public partial class Move : Node
{
	public bool IsCapture { get; set; }
	public MoveType Type { get; set;  }
	public Vector2I From { get; set; }
	public Vector2I To { get; set; }

	public Move() { }
	public Move(Vector2I from, Vector2I to) { From = from; To = to; IsCapture = false; }
	public Move(Vector2I from, Vector2I to, bool isCapture) { From = from; To = to; IsCapture = isCapture; Type = MoveType.Normal; }
	public Move(Vector2I from, Vector2I to, bool isCapture, MoveType type) { From = from; To = to; IsCapture = isCapture; Type = type; }
	public void MakeMove(Board board) => board.gameLogic.HandleMove(this);
}
