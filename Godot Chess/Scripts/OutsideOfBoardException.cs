using System;
using Godot;

public class OutsideOfBoardException : Exception
{
	public int boardSize { get; init; }
	public Vector2I pos {  get; init; }
	public OutsideOfBoardException(int boardSize, Vector2I pos)
	{
		this.boardSize = boardSize;
		this.pos = pos;
	}
	public override string Message => "Position " + pos.X + " " + pos.Y + " is outside of " + boardSize + "x" + boardSize + " board";
}
