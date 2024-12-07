using Godot;
using System;
using System.Collections.Generic;

public sealed partial class Knight : Piece
{
	public Knight() { }
	public Knight(PlayerColor color, Vector2I pos)
	{
		HasMoved = false;
		Color = color;
		Position = pos;
	}

	~Knight() { }

	// Knight moves into preset locations
	private readonly Vector2I[] MoveDirections = new Vector2I[] {
		new Vector2I(1, 2),   // 1    . . . . . . . .
		new Vector2I(2, 1),   // 2    . . . . . . . .
		new Vector2I(-1, 2),  // 3    . . . 4 . 1 . .
		new Vector2I(-2, 1),  // 4    . . 3 . . . 2 .
		new Vector2I(1, -2),  // 5    . . . . N . . .
		new Vector2I(2, -1),  // 6    . . 8 . . . 6 .
		new Vector2I(-1, -2), // 7    . . . 7 . 5 . .
		new Vector2I(-2, -1)  // 8    . . . . . . . .
	};

	public override List<Move> GetLegalMoves(Board board, bool checkPins)
	{
		List<Move> moves = new List<Move>();
		foreach (var direction in MoveDirections)
		{
			// If move is within bounds
			if (GameLogic.IsInBounds(Position + direction))
			{
				Square targetSquare = board.GetSquareAtCoordinates(Position+direction);

				// If the square is occupied by a piece
				if (targetSquare.SquarePiece != null)
				{
					// If it's an enemy piece add it as a capture move
					if (targetSquare.SquarePiece.Color != this.Color)
					{
						moves.Add(new Move(Position, Position + direction, true, MoveType.Normal)); // Capture move

					}
				}
				else
				{
					// Add normal move if the square is empty
					moves.Add(new Move(Position, Position + direction, false, MoveType.Normal));
				}
			}
		}
		if(checkPins) moves = RemovePins(board, moves);
		return moves;
	}
	public override object Clone()
	{
		Knight clone = new Knight();
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
