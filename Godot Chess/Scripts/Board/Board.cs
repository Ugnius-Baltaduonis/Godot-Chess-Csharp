using Godot;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

public partial class Board : Control, IEnumerable<Square>
{
	public Board(Square[,] squares)
	{
		this.Squares = squares;
	}
	public Board() { }
	//Visual settings
	List<ColorRect> legalMovesDisplay = new List<ColorRect>();
	private Control legalMovesContainer = new Control();
	private const float LegalMoveMarkerScale = 0.55f;
	private const int BoardOffsetX = 100;
	private const int BoardOffsetY = 100;
	public const int BoardSize = 8; // 8x8 chessboard
	private const int SquareSize = 72; // Size of each square (72x72 pixels)
	private static Vector2 pieceScale = new Vector2(SquareSize / 128f, SquareSize / 128f); // Auto scale pieces (128x128px) to square size
	private static Vector2I BoardOffset = new Vector2I(BoardOffsetX, BoardOffsetY);

	// Colors for the board
	private Color lightColor = new Color(1, 1, 1); // White
	private Color darkColor = new Color(0, 0, 0, 0.2f); // Black
	private Color legalMoveColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

	//Game variables
	public Square[,] Squares = new Square[8, 8];
	public GameLogic gameLogic;
	public GameType gameType = GameType.Standard;
	private void CreateChessBoard()
	{
		var boardRange = Enumerable.Range(0, BoardSize);
		foreach (var row in boardRange)
		{
			foreach (var col in boardRange)
			{
				Squares[col, row] ??= new Square(this, new Vector2I(col,row));
				ColorRect square = new ColorRect
				{
					Size = new Vector2(SquareSize, SquareSize),
					Position = new Vector2(col * SquareSize + BoardOffsetX, row * SquareSize + BoardOffsetY)
				};
				square.Color = ((row + col) % 2 == 0) ? lightColor : darkColor;
				AddChild(square);
			}
		}
	}


	public override void _Ready()
	{
		GetWindow().Size = new Vector2I(SquareSize * BoardSize, SquareSize * BoardSize) + BoardOffset * 2;
		gameType = Globals.GAME_TYPE;
		switch (gameType)
		{
			case GameType.Standard:
				CreateChessBoard();
				gameLogic = new StandardGame(BoardOffset, pieceScale, this);
				gameLogic.SetupGame();
				break;
			case GameType.Atomic:
				lightColor = new Color(0.968627f, 0.901960f, 0.309803f);
				darkColor = new Color(0.188235f, 0.176470f, 0.180392f);
				CreateChessBoard();
				gameLogic = new AtomicGame(BoardOffset, pieceScale, this);
				gameLogic.SetupGame();
				break;
		}
		AddChild(legalMovesContainer);
	}

	Piece selectedPiece = null;
	Vector2 offset = new Vector2();
	Vector2I selectedSquareCoords = new Vector2I();
	Square selectedSquare = new Square();
	List<Move> legalMoves = new List<Move>();
	public override void _Input(InputEvent @event)
	{
		switch (gameLogic.gameState) {
			case GameState.Normal:
				BoardControl(@event);
				break;
			case GameState.PawnPromotion:
				gameLogic.PawnPromotionControl(@event);
				break;
			case GameState.Checkmate:
				GetWindow().Size = new Vector2I((int)(SquareSize * 1.5 * BoardSize), SquareSize * BoardSize) + BoardOffset * 2;
				if (gameLogic.winner == PlayerColor.White) GetTree().ChangeSceneToFile("res://Scenes/WhiteCheckmate.tscn");
				else GetTree().ChangeSceneToFile("res://Scenes/BlackCheckmate.tscn");
				break;
			case GameState.Stalemate:
				GetWindow().Size = new Vector2I((int)(SquareSize * 1.5 * BoardSize), SquareSize * BoardSize) + BoardOffset * 2;
				GetTree().ChangeSceneToFile("res://Scenes/Stalemate.tscn");
				break;
			case GameState.FiftyMoveRule:
				GetWindow().Size = new Vector2I((int)(SquareSize * 1.5 * BoardSize), SquareSize * BoardSize) + BoardOffset * 2;
				GetTree().ChangeSceneToFile("res://Scenes/FiftyMoveRule.tscn");
				break;
			case GameState.ThreeFoldRepetition:
				GetWindow().Size = new Vector2I((int)(SquareSize * 1.5 * BoardSize), SquareSize * BoardSize) + BoardOffset * 2;
				GetTree().ChangeSceneToFile("res://Scenes/ThreeFoldRepetition.tscn");
				break;
		}
	}

	public Vector2I GetSquareCoordinatesAtPosition(Vector2 pos)
	{
		Vector2I square = new Vector2I();
		try
		{
			square.X = (int)Mathf.Floor((pos.X - BoardOffset.X) / SquareSize);
			square.Y = (int)Mathf.Floor((pos.Y - BoardOffset.Y) / SquareSize);
			if (square.X >= BoardSize || square.Y >= BoardSize) throw new OutsideOfBoardException(BoardSize, square);
		}
		catch (OutsideOfBoardException ex) {
			GD.Print(ex.Message);
			return new Vector2I();
		}
		return square;
	}
	public Square GetSquareAtCoordinates(Vector2I pos) {
		try
		{
			Square square = Squares[pos.X, pos.Y];
		}
		catch (IndexOutOfRangeException)
		{
			GD.Print("Selected position (" + pos.X + ", " + pos.Y + ") is outside of the board");
			return new Square();
		}
			return Squares[pos.X, pos.Y]; 
	}
	public Vector2 GetPositionFromCoordinates(Vector2I pos)
	{
		// Convert Vector2I to Vector2 for the Sprite position
		return new Vector2(pos.X * SquareSize + SquareSize / 2, pos.Y * SquareSize + SquareSize / 2) + BoardOffset;
	}
	private void DisplayLegalMoves(List<Move> moves)
	{
		foreach (var i in moves)
		{
			ColorRect colorRect = new ColorRect
			{
				Size = new Vector2(SquareSize * LegalMoveMarkerScale, SquareSize * LegalMoveMarkerScale),
				Color = legalMoveColor,
				Position = i.To * SquareSize + new Vector2(0.5f * SquareSize - (SquareSize * LegalMoveMarkerScale) / 2, 0.5f * SquareSize - (SquareSize * LegalMoveMarkerScale) / 2) + BoardOffset
			};
			legalMovesContainer.AddChild(colorRect);
			legalMovesDisplay.Add(colorRect);
		}
	}
	
	//Handles input on the board during regular play
	private void BoardControl(InputEvent @event)
	{
		if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.ButtonIndex == MouseButton.Left)
		{
			if (eventMouseButton.IsPressed())
			{
				// Get the square coordinates where the mouse was clicked
				selectedSquareCoords = GetSquareCoordinatesAtPosition(eventMouseButton.Position);
				selectedSquare = GetSquareAtCoordinates(selectedSquareCoords);
				if (selectedSquare.SquarePiece != null && selectedSquare.SquarePiece.Color == (gameLogic.isWhitesTurn ? PlayerColor.White : PlayerColor.Black))
				{
					selectedPiece = selectedSquare.SquarePiece;
					switch (gameType) {
						case GameType.Atomic: legalMoves = selectedPiece.GetLegalMoves(this, false); break;
						default: legalMoves = selectedPiece.GetLegalMoves(this, true); break;
					}
					DisplayLegalMoves(legalMoves);
					offset = selectedPiece.Sprite.Position - eventMouseButton.Position;
				}
			}
			else
			{
				// Get the drop square coordinates from the current mouse position
				if (selectedPiece != null)
				{
					Vector2 dropsquarePosition = GetViewport().GetMousePosition();
					Vector2I dropsquare = GetSquareCoordinatesAtPosition(dropsquarePosition);
					List<Vector2I> legaldrops = legalMoves.Select(move => move.To).ToList();
					if (legaldrops.Contains(dropsquare))
					{
						// Make the move
						Move makemove = legalMoves.Single(move => move.To.Equals(dropsquare));
						makemove.MakeMove(this);
						if(!gameLogic.PlayerHasMoves((gameLogic.isWhitesTurn ? PlayerColor.White : PlayerColor.Black))) return;
					}
					else selectedPiece.Sprite.Position = GetPositionFromCoordinates(selectedPiece.Position);
				}
				var ClearLegalMoveDisplay = () =>
				{
					foreach (ColorRect rect in legalMovesDisplay) rect.QueueFree();
					legalMovesDisplay.Clear();
				};
				ClearLegalMoveDisplay();
				legalMoves.Clear();
				selectedPiece = null;
			}
		}

		//Update piece position based on mouse movement
		if (selectedPiece != null) selectedPiece.Sprite.Position = new Vector2(GetViewport().GetMousePosition().X, GetViewport().GetMousePosition().Y) + offset;
	}

	public Square this[int index]
	{
		get
		{
			if (index < 0 || index >= Squares.Length)
				throw new IndexOutOfRangeException("Index must be between 0 and 63.");
			return Squares[index%BoardSize, index/BoardSize];
		}
	}
	public IEnumerator<Square> GetEnumerator()
	{
		for (int row = 0; row < BoardSize; row++)
		{
			for (int col = 0; col < BoardSize; col++)
			{
				yield return Squares[col, row];
			}
		}
	}
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
