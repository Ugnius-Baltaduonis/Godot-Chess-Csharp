using Godot;
using System;
using System.Collections.Generic;

public sealed partial class Pawn : Piece
{
	public Pawn() { }
	public Pawn(PlayerColor color, Vector2I pos)
	{
		HasMoved = false;
		Color = color;
		Position = pos;
	}

	~Pawn() { }

	Vector2I singleMove = new Vector2I(0, 1);
	Vector2I doubleMove = new Vector2I(0, 2);
	Vector2I[] captures = new Vector2I[]
	{
		new Vector2I(1, 1),
		new Vector2I(-1, 1)
	};


	public override List<Move> GetLegalMoves(Board board, bool checkPins)
	{
		List<Move> moves = new List<Move>();
		int direction = (Color == PlayerColor.White) ? -1 : 1;

		//Check if next singleMove results in the pawn being on the far edge of the board, if so, add a pawn promotion move
		if (!GameLogic.IsInBounds(Position + doubleMove * direction))
		{
			if (GameLogic.IsInBounds(Position + singleMove * direction) && board.GetSquareAtCoordinates(Position + singleMove * direction).SquarePiece == null)
			{
				moves.Add(new Move(Position, Position + singleMove * direction, false, MoveType.PawnPromote));
			}
			foreach (Vector2I move in captures)
			{
				Vector2I targetPosition = Position + move * direction;

				if (GameLogic.IsInBounds(targetPosition))
				{
					Square square = board.GetSquareAtCoordinates(targetPosition);

					if (square != null && square.SquarePiece != null && square.SquarePiece.Color != Color)
					{
						moves.Add(new Move(Position, targetPosition, false, MoveType.PawnPromote));
					}
				}
			}
		}
		// Check forward moves, if double is available add it as type DoublePawn for EnPassant 
		else
		{
			if (GameLogic.IsInBounds(Position + singleMove * direction) && board.GetSquareAtCoordinates(Position + singleMove * direction).SquarePiece == null)
			{
				moves.Add(new Move(Position, Position + singleMove * direction, false, MoveType.Normal));

				if (GameLogic.IsInBounds(Position + doubleMove * direction) && board.GetSquareAtCoordinates(Position + doubleMove * direction).SquarePiece == null && !HasMoved)
				{
					moves.Add(new Move(Position, Position + doubleMove * direction, false, MoveType.DoublePawn));
				}
			}

			// Check attacks for enemy pieces and EnPassant
			foreach (Vector2I move in captures)
			{
				Vector2I targetPosition = Position + move * direction;

				if (GameLogic.IsInBounds(targetPosition))
				{
					Square square = board.GetSquareAtCoordinates(targetPosition);

					if (square != null && square.SquarePiece != null && square.SquarePiece.Color != Color)
					{
						moves.Add(new Move(Position, targetPosition, true, MoveType.Normal));
					}

					// En Passant check
					if (board.gameLogic != null && board.gameLogic.enPassantSquare == targetPosition)
					{
						// Ensure the EnPassant capturing conditions are met
						Square enPassantTargetSquare = board.GetSquareAtCoordinates(board.gameLogic.enPassantSquare + singleMove * -direction);

						if (enPassantTargetSquare != null && enPassantTargetSquare.SquarePiece != null && enPassantTargetSquare.SquarePiece.Color != Color)
						{
							moves.Add(new Move(Position, targetPosition, true, MoveType.EnPassant));
						}
					}
				}
			}
		}

		if (checkPins) moves = RemovePins(board, moves);
		return moves;
	}
	public override object Clone()
	{
		Pawn clone = new Pawn();
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
