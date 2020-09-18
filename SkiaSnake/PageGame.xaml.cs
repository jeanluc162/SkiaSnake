using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using SkiaSharp;

namespace SkiaSnake
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PageGame : ContentPage
    {
        private Random EdibleGenerator = new Random();
        private Int32 Highscore = 0;
        private List<int[]> SnakeSegments = new List<int[]>();
        private List<int[]> Edibles = new List<int[]>();
        private Boolean ControlLocked = false;
        private Int32 GameWidth = -1;
        private Int32 GameHeight = -1;
        private Int32 BlockSize = -1;
        private Int32 BorderThickness = -1;
        private SKPaint BlackPaint = new SKPaint { Color = SKColors.Black }; //Snake & Borders
        private SKPaint BrownPaint = new SKPaint { Color = SKColors.Brown }; //Snakehead
        private SKPaint WhitePaint = new SKPaint { Color = SKColors.White }; //Highscore
        private SKPaint RedPaint = new SKPaint { Color = SKColors.Red }; //Edibles
        private SKFont HighscoreFont = new SKFont (); //Highscore Font
        private const Int32 VerticalBlockCount = 12;
        private Int32 HorizontalBlockCount = -1;
        private enum Directions { Up, Right,Down,Left};
        private Directions _Direction = Directions.Right;
        public PageGame()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }

        private Boolean GameLoop()
        {
            int[] LastSegmentCache = new int[2]; //Used for elongating the snek
            Boolean ate = false;
            for(int i = Edibles.Count - 1; i >= 0; i--)
            {
                if(Edibles[i][0] == SnakeSegments[0][0] && Edibles[i][1] == SnakeSegments[0][1])
                {
                    ate = true;
                    Highscore++;
                    Edibles.RemoveAt(i);
                    break;
                }
            }
            //Move Snake
            for (int i = SnakeSegments.Count - 1; i >= 0; i--)
            {
                if (i == SnakeSegments.Count - 1)
                {
                    LastSegmentCache[0] = SnakeSegments[i][0];
                    LastSegmentCache[1] = SnakeSegments[i][1];
                }
                if (i > 0)
                {
                    SnakeSegments[i][0] = SnakeSegments[i - 1][0];
                    SnakeSegments[i][1] = SnakeSegments[i - 1][1];
                }
                else
                {
                    switch (_Direction)
                    {
                        case Directions.Up:
                            if(SnakeSegments[i][1] - 1 < 0)
                            {
                                System.Diagnostics.Debug.WriteLine("Snake out of Bounds!");
                                Device.BeginInvokeOnMainThread(() =>
                                {
                                    Application.Current.MainPage.Navigation.PopAsync();
                                });
                                return false;
                            }
                            SnakeSegments[i][1]--;
                            break;
                        case Directions.Right:
                            if (SnakeSegments[i][0] + 1 >= HorizontalBlockCount)
                            {
                                System.Diagnostics.Debug.WriteLine("Snake out of Bounds!");
                                Device.BeginInvokeOnMainThread(() =>
                                {
                                    Application.Current.MainPage.Navigation.PopAsync();
                                });
                                return false;
                            }
                            SnakeSegments[i][0]++;
                            break;
                        case Directions.Left:
                            if (SnakeSegments[i][0] - 1 < 0)
                            {
                                System.Diagnostics.Debug.WriteLine("Snake out of Bounds!");
                                Device.BeginInvokeOnMainThread(() =>
                                {
                                    Application.Current.MainPage.Navigation.PopAsync();
                                });
                                return false;
                            }
                            SnakeSegments[i][0]--;
                            break;
                        case Directions.Down:
                            if (SnakeSegments[i][1] + 1 >= VerticalBlockCount)
                            {
                                System.Diagnostics.Debug.WriteLine("Snake out of Bounds!");
                                Device.BeginInvokeOnMainThread(() =>
                                {
                                    Application.Current.MainPage.Navigation.PopAsync();
                                });
                                return false;
                            }
                            SnakeSegments[i][1]++;
                            break;
                    }
                }
            }
            if(ate)
            {
                SnakeSegments.Add(LastSegmentCache);
            }
            for (int i = 1; i < SnakeSegments.Count; i++)
            {
                if (SnakeSegments[i][0] == SnakeSegments[0][0] && SnakeSegments[i][1] == SnakeSegments[0][1]) //Snake Colided with itself
                {
                    System.Diagnostics.Debug.WriteLine("Snake ate itself!");
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        Application.Current.MainPage.Navigation.PopAsync();
                    });
                    return false;
                }
            }
            if(Edibles.Count < 4)
            {
                int X = -1;
                int Y = -1;
                bool FindPosition = true;
                while (FindPosition)
                {
                    FindPosition = false;
                    X = EdibleGenerator.Next(0, HorizontalBlockCount);
                    Y = EdibleGenerator.Next(0, VerticalBlockCount);
                    foreach(int[] segment in SnakeSegments)
                    {
                        if (segment[0] == X && segment[1] == Y)
                        {
                            FindPosition = true;
                            break;
                        }
                    }
                    if (FindPosition) continue;
                    foreach(int[] edible in Edibles)
                    {
                        if(edible[0] == X && edible[1] == Y)
                        {
                            FindPosition = true;
                            break;
                        }
                    }
                }
                Edibles.Add(new int[] { X, Y });
            }
            ControlLocked = false; //Reenables user input


            return true;
        }
        private void SKGLView_PaintSurface(object sender, SkiaSharp.Views.Forms.SKPaintGLSurfaceEventArgs e)
        {
            if (GameWidth != e.BackendRenderTarget.Width || GameHeight != e.BackendRenderTarget.Height) //Should only be true once lol, else it will feck up the Game xD
            {
                GameWidth = e.BackendRenderTarget.Width;
                GameHeight = e.BackendRenderTarget.Height;
                System.Diagnostics.Debug.WriteLine("Game Dimensions are now " + GameWidth + "x" + GameHeight);
                BlockSize = GameHeight / VerticalBlockCount;
                BorderThickness = (GameWidth % BlockSize) / 2;
                HorizontalBlockCount = (GameWidth - 2 * BorderThickness) / BlockSize;
                System.Diagnostics.Debug.WriteLine("Block Size is now " + BlockSize + " requiring Borders of " + BorderThickness);

                HighscoreFont.Size = BlockSize;

                //Create the Snake
                SnakeSegments.Add(new int[] { HorizontalBlockCount / 2 + 1, VerticalBlockCount / 2 });
                SnakeSegments.Add(new int[] { HorizontalBlockCount / 2, VerticalBlockCount / 2 });
                Device.StartTimer(new TimeSpan(0, 0, 0, 0, 500), GameLoop);
            }
            
            //Clear Canvas with Background-Color
            e.Surface.Canvas.Clear(SKColors.LightGreen);

            //Draw Borders
            e.Surface.Canvas.DrawRect(new SKRect(0, 0, BorderThickness, GameHeight), BlackPaint);
            e.Surface.Canvas.DrawRect(new SKRect(GameWidth - BorderThickness, 0, GameWidth, GameHeight), BlackPaint);

            //Draw Edibles
            for (int i = 0; i < Edibles.Count; i++)
            {
                e.Surface.Canvas.DrawRect(new SKRect(BorderThickness + Edibles[i][0] * BlockSize, Edibles[i][1] * BlockSize, BorderThickness + BlockSize + Edibles[i][0] * BlockSize, BlockSize + Edibles[i][1] * BlockSize), RedPaint);
            }
            
            //Draw the Snake
            for(int i = 0; i < SnakeSegments.Count; i++)
            {
                if (i == 0) e.Surface.Canvas.DrawRect(new SKRect(BorderThickness + SnakeSegments[i][0] * BlockSize, SnakeSegments[i][1] * BlockSize, BorderThickness + BlockSize + SnakeSegments[i][0] * BlockSize, BlockSize + SnakeSegments[i][1] * BlockSize), BrownPaint);
                else e.Surface.Canvas.DrawRect(new SKRect(BorderThickness + SnakeSegments[i][0] * BlockSize, SnakeSegments[i][1] * BlockSize, BorderThickness + BlockSize + SnakeSegments[i][0] * BlockSize, BlockSize + SnakeSegments[i][1] * BlockSize), BlackPaint);
            }

            e.Surface.Canvas.DrawText(Highscore.ToString(), BorderThickness * 2, HighscoreFont.Size,HighscoreFont, WhitePaint);
        }

        private void SKGLView_Touch(object sender, SkiaSharp.Views.Forms.SKTouchEventArgs e)
        {
            if(e.Id == 0)
            {
                if(e.ActionType == SkiaSharp.Views.Forms.SKTouchAction.Pressed && !ControlLocked)
                {                    
                    System.Diagnostics.Debug.WriteLine("Current Direction: " + _Direction.ToString());
                    System.Diagnostics.Debug.WriteLine("Touch at: " + e.Location.X + "|" + e.Location.Y);
                    if (e.Location.X > GameWidth/2)
                    {
                        System.Diagnostics.Debug.WriteLine("Right Side of the Screen has been touched. Turning Right.");
                        switch(_Direction)
                        {
                            case Directions.Up:
                                _Direction = Directions.Right;
                                break;
                            case Directions.Right:
                                _Direction = Directions.Down;
                                break;
                            case Directions.Down:
                                _Direction = Directions.Left;
                                break;
                            case Directions.Left:
                                _Direction = Directions.Up;
                                break;
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Left Side of the Screen has been touched. Turning Left.");
                        switch (_Direction)
                        {
                            case Directions.Up:
                                _Direction = Directions.Left;
                                break;
                            case Directions.Left:
                                _Direction = Directions.Down;
                                break;
                            case Directions.Down:
                                _Direction = Directions.Right;
                                break;
                            case Directions.Right:
                                _Direction = Directions.Up;
                                break;
                        }
                    }
                    ControlLocked = true;
                    System.Diagnostics.Debug.WriteLine("Current Direction: " + _Direction.ToString());
                }
            }
            e.Handled = true;
        }
    }
}