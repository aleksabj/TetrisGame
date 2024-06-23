using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Globalization;

namespace TetrisGame
{
    public partial class MainWindow : Window
    {
        private const int CanvasWidth = 400;
        private const int CanvasHeight = 800;
        private const int CellSize = 40;
        private const int Columns = CanvasWidth / CellSize;
        private const int Rows = CanvasHeight / CellSize;
        private const int MaxConsecutiveSamePiece = 2;

        private readonly Dictionary<string, Bitmap> squareBitmaps;
        private Bitmap currentBitmap;
        private bool[,] gameGrid;
        private DispatcherTimer gameTimer;
        private string currentPiece;
        private string previousPiece;
        private int consecutivePieceCount;
        private List<PixelPoint> currentPiecePositions;
        private Random random = new Random();
        private int score;

        public MainWindow()
        {
            InitializeComponent();
            gameGrid = new bool[Columns, Rows];
            squareBitmaps = LoadSquareBitmaps();
            currentPiece = GetRandomPiece();
            currentBitmap = GetRandomBitmap();
            currentPiecePositions = GetPiecePositions(currentPiece);
            gameTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            gameTimer.Tick += (sender, e) => GameTick();
            StartGame();
        }

        private Dictionary<string, Bitmap> LoadSquareBitmaps()
        {
            var bitmaps = new Dictionary<string, Bitmap>();
            var files = Directory.GetFiles("pieces", "*.png");
            foreach (var file in files)
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                bitmaps[fileName] = new Bitmap(file);
            }
            return bitmaps;
        }

        private Bitmap GetRandomBitmap()
        {
            var keys = squareBitmaps.Keys.ToList();
            var key = keys[random.Next(keys.Count)];
            return squareBitmaps[key];
        }

        private void StartGame()
        {
            gameTimer.Start();
            this.KeyDown += OnKeyDown;
        }

        private void GameTick()
        {
            MovePieceDown();
            InvalidateVisual();
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);
            DrawGame(context);
        }

        

        private void DrawGame(DrawingContext context)
        {
            for (int y = 0; y < Rows; y++)
            {
                for (int x = 0; x < Columns; x++)
                {
                    if (gameGrid[x, y])
                    {
                        var rect = new Rect(x * CellSize, y * CellSize, CellSize, CellSize);
                        if (currentBitmap != null)
                        {
                            context.DrawImage(currentBitmap, rect);
                        }
                    }
                }
            }

            // Draw the current piece
            foreach (var position in currentPiecePositions)
            {
                if (currentBitmap != null)
                {
                    var rect = new Rect(position.X * CellSize, position.Y * CellSize, CellSize, CellSize);
                    context.DrawImage(currentBitmap, rect);
                }
            }
            var gameOverLineY = 4; 
            var gameOverLineStartPoint = new Point(0, gameOverLineY * CellSize);
            var gameOverLineEndPoint = new Point(CanvasWidth, gameOverLineY * CellSize);
            var gameOverLinePen = new Pen(Brushes.Red, 2, DashStyle.Dash);
            context.DrawLine(gameOverLinePen, gameOverLineStartPoint, gameOverLineEndPoint);

            var scoreText = new FormattedText(
                $"Score: {score}",
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Arial", (FontStyle)24),
                24,
                Brushes.White
            );
            context.DrawText(scoreText, new Point(10, 10));
        }


        private List<PixelPoint> GetPiecePositions(string pieceType, int offsetX = 0, int offsetY = 0)
        {
            var positions = new List<PixelPoint>();
            
            switch (pieceType)
            {
                case "I":
                    positions.AddRange(new[] {
                        new PixelPoint(0 + offsetX, 1 + offsetY),
                        new PixelPoint(1 + offsetX, 1 + offsetY),
                        new PixelPoint(2 + offsetX, 1 + offsetY),
                        new PixelPoint(3 + offsetX, 1 + offsetY)
                    });
                    break;

                case "L":
                    positions.AddRange(new[] {
                        new PixelPoint(0 + offsetX, 0 + offsetY),
                        new PixelPoint(0 + offsetX, 1 + offsetY),
                        new PixelPoint(1 + offsetX, 1 + offsetY),
                        new PixelPoint(2 + offsetX, 1 + offsetY)
                    });
                    break;

                case "J":
                    positions.AddRange(new[] {
                        new PixelPoint(2 + offsetX, 0 + offsetY),
                        new PixelPoint(0 + offsetX, 1 + offsetY),
                        new PixelPoint(1 + offsetX, 1 + offsetY),
                        new PixelPoint(2 + offsetX, 1 + offsetY)
                    });
                    break;

                case "O":
                    positions.AddRange(new[] {
                        new PixelPoint(0 + offsetX, 0 + offsetY),
                        new PixelPoint(1 + offsetX, 0 + offsetY),
                        new PixelPoint(0 + offsetX, 1 + offsetY),
                        new PixelPoint(1 + offsetX, 1 + offsetY)
                    });
                    break;

                case "S":
                    positions.AddRange(new[] {
                        new PixelPoint(1 + offsetX, 0 + offsetY),
                        new PixelPoint(2 + offsetX, 0 + offsetY),
                        new PixelPoint(0 + offsetX, 1 + offsetY),
                        new PixelPoint(1 + offsetX, 1 + offsetY)
                    });
                    break;

                case "T":
                    positions.AddRange(new[] {
                        new PixelPoint(1 + offsetX, 0 + offsetY),
                        new PixelPoint(0 + offsetX, 1 + offsetY),
                        new PixelPoint(1 + offsetX, 1 + offsetY),
                        new PixelPoint(2 + offsetX, 1 + offsetY)
                    });
                    break;

                case "Z":
                    positions.AddRange(new[] {
                        new PixelPoint(0 + offsetX, 0 + offsetY),
                        new PixelPoint(1 + offsetX, 0 + offsetY),
                        new PixelPoint(1 + offsetX, 1 + offsetY),
                        new PixelPoint(2 + offsetX, 1 + offsetY)
                    });
                    break;
            }

            return positions;
        }

        private string GetRandomPiece()
        {
            if (consecutivePieceCount >= MaxConsecutiveSamePiece && currentPiece == previousPiece)
            {
                var newPieces = new[] { "I", "L", "J", "O", "S", "T", "Z" }.Where(piece => piece != currentPiece).ToArray();
                previousPiece = currentPiece;
                currentPiece = newPieces[random.Next(newPieces.Length)];
                consecutivePieceCount = 1;
            }
            else
            {
                previousPiece = currentPiece;
                currentPiece = new[] { "I", "L", "J", "O", "S", "T", "Z" }.OrderBy(x => random.Next()).First();
                if (currentPiece == previousPiece)
                {
                    consecutivePieceCount++;
                }
                else
                {
                    consecutivePieceCount = 1;
                }
            }

            return currentPiece;
        }

        private void MovePieceDown()
        {
            for (int i = 0; i < currentPiecePositions.Count; i++)
            {
                currentPiecePositions[i] = new PixelPoint(currentPiecePositions[i].X, currentPiecePositions[i].Y + 1);
            }

            if (IsPieceColliding())
            {
                for (int i = 0; i < currentPiecePositions.Count; i++)
                {
                    currentPiecePositions[i] = new PixelPoint(currentPiecePositions[i].X, currentPiecePositions[i].Y - 1);
                }

                LockCurrentPiece();
                currentPiece = GetRandomPiece();
                currentBitmap = GetRandomBitmap();
                currentPiecePositions = GetPiecePositions(currentPiece);

                if (IsPieceColliding() && IsGameOver())
                {
                    gameTimer.Stop();
                    // Handle game over
                    RestartGame();
            }
            }
        }
        private bool IsGameOver() {
            var gameOverLineY = 4; 
            foreach (var position in currentPiecePositions) {
                if (position.Y <= gameOverLineY) {
                    return true;
                }
            }
            return false;
        }

        private void RestartGame()
        {
            gameGrid = new bool[Columns, Rows];
            score = 0;
            currentPiece = GetRandomPiece();
            currentBitmap = GetRandomBitmap();
            currentPiecePositions = GetPiecePositions(currentPiece);
            gameTimer.Start();
        }

        private bool IsPieceColliding()
        {
            foreach (var position in currentPiecePositions)
            {
                if (position.Y < 0 || position.Y >= Rows || position.X < 0 || position.X >= Columns)
                {
                    return true;
                }

                if (gameGrid[position.X, position.Y])
                {
                    return true;
                }
            }

            return false;
        }

        private void LockCurrentPiece()
        {
            foreach (var position in currentPiecePositions)
            {
                if (position.Y >= 0 && position.Y < Rows && position.X >= 0 && position.X < Columns)
                {
                    gameGrid[position.X, position.Y] = true;
                }
            }

            ClearFullRows();
        }

        private void ClearFullRows()
        {
            for (int y = 0; y < Rows; y++)
            {
                bool isRowFull = true;
                for (int x = 0; x < Columns; x++)
                {
                    if (!gameGrid[x, y])
                    {
                        isRowFull = false;
                        break;
                    }
                }

                if (isRowFull)
                {
                    for (int row = y; row > 0; row--)
                    {
                        for (int x = 0; x < Columns; x++)
                        {
                            gameGrid[x, row] = gameGrid[x, row - 1];
                        }
                    }

                    for (int x = 0; x < Columns; x++)
                    {
                        gameGrid[x, 0] = false;
                    }

                    score += 100;
                }
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left:
                    MovePieceHorizontally(-1);
                    break;

                case Key.Right:
                    MovePieceHorizontally(1);
                    break;

                case Key.Down:
                    MovePieceDown();
                    break;

                case Key.Up:
                    RotatePiece();
                    break;
            }

            InvalidateVisual();
        }

        private void MovePieceHorizontally(int direction)
        {
            for (int i = 0; i < currentPiecePositions.Count; i++)
            {
                currentPiecePositions[i] = new PixelPoint(currentPiecePositions[i].X + direction, currentPiecePositions[i].Y);
            }

            if (IsPieceColliding())
            {
                for (int i = 0; i < currentPiecePositions.Count; i++)
                {
                    currentPiecePositions[i] = new PixelPoint(currentPiecePositions[i].X - direction, currentPiecePositions[i].Y);
                }
            }
        }

        private void RotatePiece()
        {
            var center = currentPiecePositions[1];
            var newPositions = currentPiecePositions.Select(p => new PixelPoint(center.X - center.Y + p.Y, center.Y + center.X - p.X)).ToList();

            if (IsValidPosition(newPositions))
            {
                currentPiecePositions = newPositions;
            }
        }

        private bool IsValidPosition(List<PixelPoint> positions)
        {
            foreach (var position in positions)
            {
                if (position.Y < 0 || position.Y >= Rows || position.X < 0 || position.X >= Columns)
                {
                    return false;
                }

                if (gameGrid[position.X, position.Y])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
