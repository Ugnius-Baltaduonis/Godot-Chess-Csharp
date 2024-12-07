using Godot;

public partial class StandardGame : GameLogic
{
	private int fiftyMoveRule = 0;
	public StandardGame(Vector2I boff, Vector2 sprScale, Board b)
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
				MovePiece( move);
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
				}
				else if (move.Type == MoveType.PawnPromote)
				{
					gameState = GameState.PawnPromotion;
				}
			}
		}
		if (move.Type == MoveType.PawnPromote)
		{
			PromotingPiece = selectedPiece;
		}
		selectedPiece.HasMoved = true;
		isWhitesTurn = !isWhitesTurn;
		positions.Add(ConvertBoardToFENNoMoves());
		if (fiftyMoveRule >= 100) gameState = GameState.FiftyMoveRule;
		if (isThreeFoldRepetition()) gameState = GameState.ThreeFoldRepetition;
	}
	private void SetupStandardGame()
	{
		string standardFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"; //starting 
		//string standardFEN = "rnbqkb1r/ppppp1pp/7n/4Pp2/8/8/PPPP1PPP/RNBQKBNR w KQkq f6 0 3"; //fen en passant read test position
		//string standardFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w - - 1 1"; //fen castling rights read test position
		//string standardFEN = "r1b5/pp1Np1k1/7p/q1pp2r1/3P2Q1/1PP1P3/P4PPP/R3K2R w KQ - 1 19"; //random position
		//string standardFEN = "r3k2r/pppppppp/8/8/8/8/PPPPPPPP/R3K2R w KQkq - 0 1"; //castle test
		LoadPositionFromFEN(standardFEN);
		positions.Add(ConvertBoardToFENNoMoves());
	}
}
