using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
public abstract partial class GameLogic : Node
{
	protected Board board;
    protected Vector2 pieceScale;
	protected Vector2I BoardOffset;
	public Vector2I enPassantSquare = new Vector2I(-1, -1);
	public Vector2I whiteKingPos;
	public Vector2I blackKingPos;
	public GameState gameState = GameState.Normal;
	public Piece PromotingPiece;
	public bool isWhitesTurn = true;
	public PlayerColor winner = PlayerColor.None;
	public byte castlingRights = 0; //1100 - first 2 bits indicate white (queen then king side), last to indicate black
	protected List<string> positions = new List<string>();
	public GameLogic() {}

	protected Dictionary<char, string> pieceTextures = new Dictionary<char, string>() {
		{ 'P', "res://Sprites/white_pawn.png" },
		{ 'p', "res://Sprites/black_pawn.png" },
		{ 'R', "res://Sprites/white_rook.png" },
		{ 'r', "res://Sprites/black_rook.png" },
		{ 'N', "res://Sprites/white_knight.png" },
		{ 'n', "res://Sprites/black_knight.png" },
		{ 'B', "res://Sprites/white_bishop.png" },
		{ 'b', "res://Sprites/black_bishop.png" },
		{ 'Q', "res://Sprites/white_queen.png" },
		{ 'q', "res://Sprites/black_queen.png" },
		{ 'K', "res://Sprites/white_king.png" },
		{ 'k', "res://Sprites/black_king.png" }
	};
	public abstract void SetupGame();
	public abstract void HandleMove(Move move);
	protected void MovePiece(Move move)
	{
		Square SquareTo = board.Squares[move.To.X, move.To.Y];
		Square SquareFrom = board.Squares[move.From.X, move.From.Y];
		if (SquareTo.SquarePiece != null)
		{
			RemovePiece(SquareTo.SquarePiece);
		}
		SquareTo.SquarePiece = SquareFrom.SquarePiece;
		SquareFrom.SquarePiece = null;
		SquareTo.SquarePiece.Sprite.Position = board.GetPositionFromCoordinates(move.To);
		SquareTo.SquarePiece.Position = move.To;
	}
	//Simulates move and checks for check, returns true if king is in check after the move
	public bool SimulateMove(Move move)
	{
		Square SquareTo = board.Squares[move.To.X, move.To.Y];
		Square SquareFrom = board.Squares[move.From.X, move.From.Y];
		Square tempsqr = new Square();
		if (SquareFrom.SquarePiece == null) { GD.Print("Null moving piece in SimulateMove()"); return false; }
		if (SquareTo.SquarePiece != null)
		{
			tempsqr = (Square)board.Squares[move.To.X, move.To.Y].Clone();
		}
		SilentMovePiece(move);
		bool isKingInCheck = SquareTo.SquarePiece.IsKingInCheck(board, (SquareTo.SquarePiece.Color == PlayerColor.White) ? whiteKingPos : blackKingPos);
		Move backwards = new Move(move.To, move.From);
		SilentMovePiece(backwards);
		if (tempsqr.SquarePiece != null)
		{
			board.Squares[move.To.X, move.To.Y] = tempsqr;
		}
		return isKingInCheck;
	}
	// Simulates a move specifically for the king and checks if the king is left in check after the move
	// Returns true if the king is in check after the move
	public bool SimulateKingMove(Move move)
	{
		Square SquareTo = board.Squares[move.To.X, move.To.Y];
		Square SquareFrom = board.Squares[move.From.X, move.From.Y];
		Square tempsqr = new Square();
		Piece movingPiece = SquareFrom.SquarePiece;
		if (movingPiece == null) { GD.Print("Null moving piece in SimulateKingMove()"); return false; }
		Vector2I originalKingPos = (movingPiece.Color == PlayerColor.White) ? whiteKingPos : blackKingPos;
		Vector2I newKingPos = move.To;

		if (SquareTo.SquarePiece != null)
		{
			tempsqr = (Square)board.Squares[move.To.X, move.To.Y].Clone();
		}
		SilentMovePiece(move);
		if (movingPiece.Color == PlayerColor.White)
		{
			whiteKingPos = newKingPos;
		}
		else
		{
			blackKingPos = newKingPos;
		}
		bool isKingInCheck = movingPiece.IsKingInCheck(board, newKingPos);

		Move backwards = new Move(move.To, move.From);
		SilentMovePiece(backwards);
		if (tempsqr.SquarePiece != null)
		{
			board.Squares[move.To.X, move.To.Y] = tempsqr;
		}
		if (movingPiece.Color == PlayerColor.White)
		{
			whiteKingPos = originalKingPos;
		}
		else
		{
			blackKingPos = originalKingPos;
		}

		return isKingInCheck;
	}
	//Same as MovePiece, except that it doesn't adjust the pieces sprite position
	protected void SilentMovePiece(Move move)
	{
		Square SquareTo = board.Squares[move.To.X, move.To.Y];
		Square SquareFrom = board.Squares[move.From.X, move.From.Y];
		if (SquareTo.SquarePiece != null && move.IsCapture)
		{
			RemovePiece(SquareTo.SquarePiece);
		}
		SquareTo.SquarePiece = SquareFrom.SquarePiece;
		SquareFrom.SquarePiece = null;
		SquareTo.SquarePiece.Position = move.To;
	}
	public static bool IsInBounds(Vector2I pos) => (pos.X >= 0 && pos.X < 8 && pos.Y >= 0 && pos.Y < 8);
	protected void RemovePiece(Piece piece)
	{
		board.RemoveChild(piece.Sprite);
		piece.Sprite.QueueFree();
		board.GetSquareAtCoordinates(piece.Position).SquarePiece = null;
	}
	protected void ReplacePiece(Vector2I pos, Piece newpiece)
	{
		newpiece.Position = pos;
		newpiece.HasMoved = true;
		newpiece.Color = board.Squares[pos.X, pos.Y].SquarePiece.Color;
		newpiece.Sprite.Scale = board.Squares[pos.X, pos.Y].SquarePiece.Sprite.Scale;
		newpiece.Sprite.Position = board.Squares[pos.X, pos.Y].SquarePiece.Sprite.Position;
		RemovePiece(board.Squares[pos.X, pos.Y].SquarePiece);
		board.Squares[pos.X, pos.Y].SquarePiece = newpiece;
		switch (newpiece.ToString())
		{
			case "Queen":
				if (newpiece.Color == PlayerColor.White) newpiece.Sprite.Texture = (Texture2D)GD.Load(pieceTextures['Q']);
				if (newpiece.Color == PlayerColor.Black) newpiece.Sprite.Texture = (Texture2D)GD.Load(pieceTextures['q']);
				break;
			case "Knight":
				if (newpiece.Color == PlayerColor.White) newpiece.Sprite.Texture = (Texture2D)GD.Load(pieceTextures['N']);
				if (newpiece.Color == PlayerColor.Black) newpiece.Sprite.Texture = (Texture2D)GD.Load(pieceTextures['n']);
				break;
			case "Bishop":
				if (newpiece.Color == PlayerColor.White) newpiece.Sprite.Texture = (Texture2D)GD.Load(pieceTextures['B']);
				if (newpiece.Color == PlayerColor.Black) newpiece.Sprite.Texture = (Texture2D)GD.Load(pieceTextures['b']);
				break;
			case "Rook":
				if (newpiece.Color == PlayerColor.White) newpiece.Sprite.Texture = (Texture2D)GD.Load(pieceTextures['R']);
				if (newpiece.Color == PlayerColor.Black) newpiece.Sprite.Texture = (Texture2D)GD.Load(pieceTextures['r']);
				break;
		}

		board.AddChild(newpiece.Sprite);
	}
	protected void LoadPositionFromFEN(string fen)
	{
		int boardIndex = 0;
		int fenIndex = 0;
		for (int i = 0; i < fen.Length; i++)
		{
			if (fen[i] == ' ') { fenIndex = i; break; };
			if (fen[i] == '/') continue;
			if (System.Char.IsDigit(fen[i]))
			{
				boardIndex += (fen[i] - '0'); // Increment boardIndex by the number of empty squares
				continue;
			}
			PlayerColor color = Char.IsUpper(fen[i]) ? PlayerColor.White : PlayerColor.Black;
			Piece piece = null;
			switch (Char.ToLower(fen[i]))
			{
				case 'p':
					piece = new Pawn(color, new Vector2I(boardIndex % 8, boardIndex / 8));
					break;
				case 'r':
					piece = new Rook(color, new Vector2I(boardIndex % 8, boardIndex / 8));
					break;
				case 'n':
					piece = new Knight(color, new Vector2I(boardIndex % 8, boardIndex / 8));
					break;
				case 'b':
					piece = new Bishop(color, new Vector2I(boardIndex % 8, boardIndex / 8));
					break;
				case 'q':
					piece = new Queen(color, new Vector2I(boardIndex % 8, boardIndex / 8));
					break;
				case 'k':
					piece = new King(color, new Vector2I(boardIndex % 8, boardIndex / 8));
					if (Char.IsUpper(fen[i])) whiteKingPos = piece.Position;
					else blackKingPos = piece.Position;
					break;
			}
			if (piece != null)
			{
				piece.Sprite.Texture = (Texture2D)GD.Load(pieceTextures[fen[i]]);
				piece.Sprite.Scale = pieceScale;
				piece.Sprite.Position = board.GetPositionFromCoordinates(new Vector2I(boardIndex % 8, boardIndex / 8));
				board.AddChild(piece.Sprite);
				board.Squares[boardIndex % 8, boardIndex / 8].SquarePiece = piece;
				//GD.Print(GetPieceShortName(piece));
			}
			boardIndex++;
		}
		fenIndex++;
		isWhitesTurn = fen[fenIndex] == 'w';
		fenIndex += 2;
		for (int i = fenIndex; i < fen.Length; i++)
		{
			if (fen[i] == '-') { fenIndex = i; break; }
			switch (fen[i])
			{
				case 'K':
					castlingRights |= 0b1000;
					fenIndex++;
					break;
				case 'Q':
					castlingRights |= 0b0100;
					fenIndex++;
					break;
				case 'k':
					castlingRights |= 0b0010;
					fenIndex++;
					break;
				case 'q':
					castlingRights |= 0b0001;
					fenIndex++;
					break;
			}
		}
		if (fen[fenIndex] == '-')
		{
			enPassantSquare = new Vector2I(-1, -1);
			fenIndex += 2;
		}
		else
		{
		fenIndex++;
            enPassantSquare = new Vector2I(fen[fenIndex] - 'a', 8 - (fen[fenIndex + 1] - '0'));
			fenIndex += 3;
		}
	}
	//Handles input when a pawn is promoting
	public void PawnPromotionControl(InputEvent @event)
	{
		bool hasPromoted = false;
		if (@event is InputEventKey keyEvent && keyEvent.Pressed)
		{
			switch (keyEvent.Keycode)
			{
				case Key.Q:
					ReplacePiece(PromotingPiece.Position, new Queen());
					hasPromoted = true;
					break;
				case Key.N:
					ReplacePiece(PromotingPiece.Position, new Knight());
					hasPromoted = true;
					break;
				case Key.K:
					ReplacePiece(PromotingPiece.Position, new Knight());
					hasPromoted = true;
					break;
				case Key.R:
					ReplacePiece(PromotingPiece.Position, new Rook());
					hasPromoted = true;
					break;
				case Key.B:
					ReplacePiece(PromotingPiece.Position, new Bishop());
					hasPromoted = true;
					break;
			}
		}
		if (hasPromoted) gameState = GameState.Normal;
	}
	public bool PlayerHasMoves(PlayerColor color)
	{
		bool isKingInCheck = false;
		foreach (Square square in board.Squares)
		{
			if (square.SquarePiece == null) continue;
			if (!isKingInCheck && color == PlayerColor.White && square.SquarePiece.IsKingInCheck(board, whiteKingPos)) isKingInCheck = true;
			else if (!isKingInCheck && color == PlayerColor.Black && square.SquarePiece.IsKingInCheck(board, blackKingPos)) isKingInCheck = true;
			if (square.SquarePiece.Color == color && square.SquarePiece.GetLegalMoves(board, true).Count > 0) return true;
		}
		if (!isKingInCheck) {
			gameState = GameState.Stalemate;
			return false;
		}
		else {
			if (color == PlayerColor.White) winner = PlayerColor.Black;
			else winner = PlayerColor.White;
			gameState = GameState.Checkmate;
			return false;
		}
	}
	public char GetPieceShortName(Piece piece) => piece switch
	{
		King when piece.Color == PlayerColor.White => 'K',
		King when piece.Color == PlayerColor.Black => 'k',
		Queen when piece.Color == PlayerColor.White => 'Q',
		Queen when piece.Color == PlayerColor.Black => 'q',
		Rook when piece.Color == PlayerColor.White => 'R',
		Rook when piece.Color == PlayerColor.Black => 'r',
		Bishop when piece.Color == PlayerColor.White => 'B',
		Bishop when piece.Color == PlayerColor.Black => 'b',
		Knight when piece.Color == PlayerColor.White => 'N',
		Knight when piece.Color == PlayerColor.Black => 'n',
		Pawn when piece.Color == PlayerColor.White => 'P',
		Pawn when piece.Color == PlayerColor.Black => 'p',
		_ => ' ',
	};
	public bool TryGetPiece(Vector2I position, out Piece piece)
	{
		if (GameLogic.IsInBounds(position))
		{
			piece = board.Squares[position.X, position.Y].SquarePiece;
			return piece != null;
		}
		piece = null;
		return false;
	}
	public String ConvertBoardToFENNoMoves()
	{
		char[] fenarr = new char[Board.BoardSize * Board.BoardSize * 2];
		int fenarrindex = 0;
		int emptycount = 0;
		for (int i = 0; i < Board.BoardSize; i++)
		{
			emptycount = 0;
			for (int j = 0; j < Board.BoardSize; j++)
			{
				char piecechar = GetPieceShortName(board.Squares[j, i].SquarePiece);
				if (piecechar == ' ') emptycount++;
				else if (emptycount != 0)
				{
					fenarr[fenarrindex++] = (char)(emptycount + '0');
					emptycount = 0;
					fenarr[fenarrindex++] = piecechar;
				}
				else fenarr[fenarrindex++] = piecechar;
			}
			if (emptycount != 0)
			{
				fenarr[fenarrindex++] = (char)(emptycount + '0');
				emptycount = 0;
			}
			if (i == Board.BoardSize - 1) break;
			fenarr[fenarrindex++] = '/';
		}
		fenarr[fenarrindex] = ' ';
		fenarrindex++;
		fenarr[fenarrindex] = isWhitesTurn ? 'w' : 'b';
		fenarrindex++;
		fenarr[fenarrindex] = ' ';
		fenarrindex++;
        if (castlingRights > 0)
        {
            // For each bit, we check and append the appropriate character if the right is present
            if ((castlingRights & 0b1000) != 0) fenarr[fenarrindex++] = 'K';
            if ((castlingRights & 0b0100) != 0) fenarr[fenarrindex++] = 'Q';
            if ((castlingRights & 0b0010) != 0) fenarr[fenarrindex++] = 'k';
            if ((castlingRights & 0b0001) != 0) fenarr[fenarrindex++] = 'q';
        }
        else
		{
			fenarr[fenarrindex] = '-';
			fenarrindex++;
		}
		fenarr[fenarrindex++] = ' ';
		if (enPassantSquare.X != -1)
		{
			fenarr[fenarrindex++] = (char)('h' - enPassantSquare.Y);
			fenarr[fenarrindex++] = (char)(enPassantSquare.X + 1 + '0');
		}
		else fenarr[fenarrindex++] = '-';
		return new string(fenarr);
	}
	protected bool isThreeFoldRepetition()
	{
		if (positions.Count() < 3) return false;
		if (positions.Count(p => p == positions.Last()) >= 3) return true;
		return false;
	}
}
