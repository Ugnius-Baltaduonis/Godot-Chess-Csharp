using Godot;
using System;
using System.Collections.Generic;

public sealed partial class King : Piece
{
	King() { }
	public King(PlayerColor color, Vector2I pos)
	{
		HasMoved = false;
		Color = color;
		Position = pos;
	}
	//King moves in all directions 1 square + castling
	private readonly Vector2I[] MoveDirections = new Vector2I[] {
		new Vector2I(1, 1),   // Top-right diagonal
		new Vector2I(-1, -1), // Bottom-left diagonal
		new Vector2I(-1, 1),  // Top-left diagonal
		new Vector2I(1, -1),   // Bottom-right diagonal
		new Vector2I(1, 0),   // Right
		new Vector2I(-1, 0), // Left
		new Vector2I(0, 1),  // Top
		new Vector2I(0, -1)   // Bottom
	};

	~King() { }

	public override List<Move> GetLegalMoves(Board board, bool checkPins)
	{
		List<Move> moves = new List<Move>();
		Vector2I kingPos = (Color == PlayerColor.White) ? board.gameLogic.whiteKingPos : board.gameLogic.blackKingPos;
		foreach (var direction in MoveDirections) {
			if (GameLogic.IsInBounds(Position + direction))
			{
				Square targetSquare = board.GetSquareAtCoordinates(Position + direction);
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
		//Castle
		if (!HasMoved)
		{
			//King side
			Square castleKingSquare = board.Squares[6, kingPos.Y];
			Square castleRookSquare = board.Squares[5, kingPos.Y];
			if ((board.gameLogic.castlingRights & (Color == PlayerColor.White ? 0b1000 : 0b0010)) != 0)
			{
				if (castleKingSquare.SquarePiece == null || (castleKingSquare.SquarePiece is Rook && castleKingSquare.SquarePiece.Color == this.Color))
				{
					for (int i = kingPos.X + 1; GameLogic.IsInBounds(new Vector2I(i, kingPos.Y)); i++)
					{
						Square sqr = board.Squares[i, kingPos.Y];
						if (sqr.SquarePiece == null) continue;
						if (sqr.SquarePiece.Color != this.Color) break;
						if (sqr.SquarePiece is not Rook) break;
						if (!sqr.SquarePiece.HasMoved) { moves.Add(new Move(Position, sqr.SquarePosition, false, MoveType.CastleKingSide)); break; }
					}
				}
			}
			castleKingSquare = board.Squares[2, kingPos.Y];
			castleRookSquare = board.Squares[3, kingPos.Y];
			if ((board.gameLogic.castlingRights & (Color == PlayerColor.White ? 0b0100 : 0b0001)) != 0)
			{
				if (castleKingSquare.SquarePiece == null || (castleKingSquare.SquarePiece is Rook && castleKingSquare.SquarePiece.Color == this.Color))
				{
					for (int i = kingPos.X - 1; GameLogic.IsInBounds(new Vector2I(i, kingPos.Y)); i--)
					{
						Square sqr = board.Squares[i, kingPos.Y];
						if (sqr.SquarePiece == null) continue;
						if (sqr.SquarePiece.Color != this.Color) break;
						if (sqr.SquarePiece is not Rook) break;
						if (!sqr.SquarePiece.HasMoved) { moves.Add(new Move(Position, sqr.SquarePosition, false, MoveType.CastleQueenSide)); break; }
					}
				}
			}
		}
		if (checkPins) moves = AvoidCheck(board, moves);
		return moves;
	}

	private List<Move> AvoidCheck(Board board, List<Move> moves)
	{
		if (moves == null || moves.Count == 0) return moves;
		List<Move> validMoves = new List<Move>();
		foreach (Move move in moves)
		{
			if(move.Type == MoveType.CastleKingSide || move.Type == MoveType.CastleQueenSide) {
				if (!IsKingInCheck(board, this.Position)) validMoves.Add(move);
				continue;
			}
			// If the king is not in check, add the move to valid moves
			if (!board.gameLogic.SimulateKingMove(move))
			{
				validMoves.Add(move);
			}
		}
		return validMoves;
	}
	public override object Clone()
	{
		King clone = new King();
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
