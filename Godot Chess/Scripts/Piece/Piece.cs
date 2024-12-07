using Godot;
using System;
using System.Collections.Generic;


public abstract partial class Piece : ICloneable
{
	public Sprite2D Sprite = new Sprite2D();
	public PlayerColor Color { get; set; }
	public Vector2I Position { get; set; }
	public bool HasMoved { get; set; }
	static Piece() { }
	~Piece() { }
	abstract public List<Move> GetLegalMoves(Board board, bool checkPins);
	public void MoveToPosition(Vector2I position) => Position = position;
	public bool IsKingInCheck(Board board, Vector2I kingpos)
	{
		foreach (Square square in board.Squares)
		{
			if (square == null || square.SquarePiece == null || square.SquarePiece.Color == Color) continue;
			Piece attackingPiece = square.SquarePiece;
			List<Move> legalMoves = attackingPiece.GetLegalMoves(board, false);
			foreach (Move move in legalMoves)
			{
				if (move.To == kingpos)
				{
					return true; // King is in check
				}
			}
		}

		return false; // No attacking pieces can check the king
	}

	protected List<Move> RemovePins(Board board, List<Move> moves)
	{
		if (moves == null || moves.Count == 0) return moves;
		List<Move> validMoves = new List<Move>();
		Vector2I kingPos = (Color == PlayerColor.White) ? board.gameLogic.whiteKingPos : board.gameLogic.blackKingPos;
		foreach (Move move in moves)
		{
			// If the king is not in check, add the move to valid moves
			if (!board.gameLogic.SimulateMove(move))
			{
				validMoves.Add(move);
			}
		}
		return validMoves;
	}
	public abstract object Clone();
}
