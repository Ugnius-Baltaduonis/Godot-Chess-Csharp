using Godot;
using System;
using System.Collections;

public partial class Square : ICloneable
{
	public Vector2I SquarePosition { get; set;  }
	public Piece SquarePiece;
	private Board BoardReferenace { get; set; }
	public Square() { }
	public Square(Board board, Vector2I pos)
	{
		SquarePosition = pos;
		BoardReferenace = board;
	}
	public PlayerColor ReturnColor()
	{
		return ((SquarePosition.X + SquarePosition.Y) % 2 == 0) ? PlayerColor.White : PlayerColor.Black;
	}
	public object Clone()
	{
		Square clone = new Square();
		clone.SquarePosition = this.SquarePosition;
		clone.SquarePiece = (Piece)this.SquarePiece.Clone();
		clone.BoardReferenace = this.BoardReferenace;
		this.SquarePiece.Sprite.QueueFree();
		this.BoardReferenace.AddChild(clone.SquarePiece.Sprite);
		return clone;
	}
}
