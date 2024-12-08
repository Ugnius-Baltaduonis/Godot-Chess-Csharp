using Godot;

public partial class AtomicGame : GameLogic
{
	private int fiftyMoveRule = 0;
	public AtomicGame(Vector2I boff, Vector2 sprScale, Board b)
	{
		this.board = b;
		this.BoardOffset = boff;
		this.pieceScale = sprScale;
	}
	public override void SetupGame()
	{
		SetupStandardGame();
	}
	public override void HandleMove(Move move)
	{
		Piece selectedPiece = board.Squares[move.From.X, move.From.Y].SquarePiece;
		//Handle fifty move rule
		if (selectedPiece is Pawn) fiftyMoveRule = 0;
		else if (move.IsCapture) fiftyMoveRule = 0;
		else fiftyMoveRule++;
		if (selectedPiece is Rook)
		{
			if (!selectedPiece.HasMoved)
			{
				for (int i = move.From.X; i < Board.BoardSize; i++)
				{
					//revoke castling queenside
					if (board.Squares[i, move.From.Y].SquarePiece is King && board.Squares[i, move.From.Y].SquarePiece.Color == selectedPiece.Color)
					{
						if (selectedPiece.Color == PlayerColor.White) castlingRights &= 0b1011;
						else castlingRights &= 0b1110;
					}
				}
				for (int i = move.From.X; i >= 0; i--)
				{
					//revoke castling kingside
					if (board.Squares[i, move.From.Y].SquarePiece is King && board.Squares[i, move.From.Y].SquarePiece.Color == selectedPiece.Color)
					{
						if (selectedPiece.Color == PlayerColor.White) castlingRights &= 0b0111;
						else castlingRights &= 0b1101;
					}
				}
			}
		}
		if (selectedPiece is King)
		{
			if (move.Type == MoveType.Normal)
			{
				MovePiece(move);
				if (selectedPiece.Color == PlayerColor.White) whiteKingPos = move.To;
				else blackKingPos = move.To;
			}
			else if (move.Type == MoveType.CastleKingSide)
			{
				int ypos = (selectedPiece.Color == PlayerColor.White ? whiteKingPos.Y : blackKingPos.Y);
				MovePiece(new Move(move.From, new Vector2I(6, ypos)));
				MovePiece(new Move(move.To, new Vector2I(5, ypos)));
				if (selectedPiece.Color == PlayerColor.White) whiteKingPos = new Vector2I(6, ypos);
				else blackKingPos = new Vector2I(6, ypos);
				board.Squares[5, ypos].SquarePiece.HasMoved = true;
			}
			else if (move.Type == MoveType.CastleQueenSide)
			{
				int ypos = (selectedPiece.Color == PlayerColor.White ? whiteKingPos.Y : blackKingPos.Y);
				MovePiece(new Move(move.From, new Vector2I(2, ypos)));
				MovePiece(new Move(move.To, new Vector2I(3, ypos)));
				if (selectedPiece.Color == PlayerColor.White) whiteKingPos = new Vector2I(2, ypos);
				else blackKingPos = new Vector2I(2, ypos);
				board.Squares[3, ypos].SquarePiece.HasMoved = true;
			}
			if (selectedPiece.Color == PlayerColor.White) castlingRights &= 0b0011;
			else castlingRights &= 0b1100;
		}
		else
		{
			MovePiece(move);
			if (selectedPiece is Pawn)
			{
				if (move.Type == MoveType.DoublePawn) enPassantSquare = move.To + (selectedPiece.Color == PlayerColor.White ? new Vector2I(0, 1) : new Vector2I(0, -1));
				else enPassantSquare = new Vector2I(-1, -1);
				if (move.Type == MoveType.EnPassant)
				{
					//En passant capture
					RemovePiece(board.GetSquareAtCoordinates(move.To + (selectedPiece.Color == PlayerColor.White ? new Vector2I(0, 1) : new Vector2I(0, -1))).SquarePiece);
					ExplodePiece(move);
				}
				else if (move.Type == MoveType.PawnPromote)
				{
					gameState = GameState.PawnPromotion;
				}
			}
		}
		if (move.Type == MoveType.PawnPromote) PromotingPiece = selectedPiece;
		if (move.IsCapture == true)
		{
			ExplodePiece(move);
			if(selectedPiece is Pawn) RemovePiece(selectedPiece);
		}
		if (blackKingPos == new Vector2I(-1, -1) && whiteKingPos == new Vector2I(-1, -1)) gameState = GameState.Stalemate;
		else if (blackKingPos == new Vector2I(-1, -1))
		{
			gameState = GameState.Checkmate;
			winner = PlayerColor.White;
		}
		else if (whiteKingPos == new Vector2I(-1, -1))
		{
			gameState = GameState.Checkmate;
			winner = PlayerColor.Black;
		}
		if(selectedPiece != null) selectedPiece.HasMoved = true;
		isWhitesTurn = !isWhitesTurn;
		positions.Add(ConvertBoardToFENNoMoves());
		if (fiftyMoveRule >= 100) gameState = GameState.FiftyMoveRule;
		if (isThreeFoldRepetition()) gameState = GameState.ThreeFoldRepetition;
	}
	private void SetupStandardGame()
	{
		string standardFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
		//string standardFEN = "rnbqkbnr/pppppppp/8/8/8/8/8/3RK3 w KQkq - 0 1"; //explosion test
		LoadPositionFromFEN(standardFEN);
		positions.Add(ConvertBoardToFENNoMoves());
	}
	private void ExplodePiece(Move move)
	{
		for (int i = move.To.Y - 1; i <= move.To.Y + 1; i++)
		{
			for (int j = move.To.X - 1; j <= move.To.X + 1; j++)
			{
				Square sqr = board.GetSquareAtCoordinates(new Vector2I(j, i));
				if (sqr == null) continue;
				Piece piece = sqr.SquarePiece;
				if (piece == null) continue;
				if (piece is Pawn) continue;
				if (piece is King && piece.Color == PlayerColor.White) whiteKingPos = new Vector2I(-1, -1);
				if (piece is King && piece.Color == PlayerColor.Black) blackKingPos = new Vector2I(-1, -1);
				RemovePiece(piece);
			}
		}
	}
}
