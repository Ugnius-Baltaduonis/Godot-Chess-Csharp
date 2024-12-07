using Godot;
using System;
using System.Collections.Generic;

public sealed partial class Bishop : Piece
{
	public Bishop() { }
	public Bishop(PlayerColor color, Vector2I pos)
	{
		HasMoved = false;
		Color = color;
		Position = pos;
	}

	~Bishop() { }

	// Bishop moves diagonally in four directions
	private readonly Vector2I[] MoveDirections = new Vector2I[] {
		new Vector2I(1, 1),   // Top-right diagonal
		new Vector2I(-1, -1), // Bottom-left diagonal
		new Vector2I(-1, 1),  // Top-left diagonal
		new Vector2I(1, -1)   // Bottom-right diagonal
	};

	public override List<Move> GetLegalMoves(Board board, bool checkPins)
	{
		List<Move> moves = new List<Move>();
		foreach (var direction in MoveDirections)
		{
			// Move along each direction until a piece is encountered or the board boundary is reached
			for (Vector2I currentPos = Position + direction; GameLogic.IsInBounds(currentPos); currentPos += direction)
			{
				Square targetSquare = board.GetSquareAtCoordinates(currentPos);
				// If the square is occupied by a piece
				if (targetSquare.SquarePiece != null)
				{
					// If it's an enemy piece, add it as a capture move
					if (targetSquare.SquarePiece.Color != this.Color)
					{
						moves.Add(new Move(Position, currentPos, true)); // Capture move
					}
					break;
				}
				else
				{
					// Add normal move if the square is empty
					moves.Add(new Move(Position, currentPos));
				}
			}
		}
		if (checkPins) moves = RemovePins(board, moves);
		return moves;
	}
	public override object Clone()
	{
		Bishop clone = new Bishop();
		clone.Position = this.Position;
		clone.HasMoved = this.HasMoved;
		clone.Color = this.Color;
		clone.Sprite = new Sprite2D();
		clone.Sprite.Position = this.Sprite.Position;
		clone.Sprite.Texture = this.Sprite.Texture;
		clone.Sprite.Scale = this.Sprite.Scale;
		return clone;
	}
}
