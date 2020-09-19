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
        private Dictionary<Directions, List<int[]>> Buttons = new Dictionary<Directions, List<int[]>>();
        private Boolean ControlLocked = false;
        private Int32 GameWidth = -1;
        private Int32 GameHeight = -1;
        private Int32 BlockSize = -1;
        private Int32 BorderThickness = -1;
        private SKPaint TransparentGray = new SKPaint { Color = SKColors.Black.WithAlpha(96) }; //Button
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

                //Create the Buttons
                //Left
                Buttons.Add(Directions.Left, new List<int[]>());
                Buttons[Directions.Left].Add(new int[] { 0, VerticalBlockCount - 3 });
                Buttons[Directions.Left].Add(new int[] { 0, VerticalBlockCount - 4 });
                Buttons[Directions.Left].Add(new int[] { 1, VerticalBlockCount - 3 });
                Buttons[Directions.Left].Add(new int[] { 1, VerticalBlockCount - 4 });
                Buttons[Directions.Left].Add(new int[] { HorizontalBlockCount - 5, VerticalBlockCount - 3 });
                Buttons[Directions.Left].Add(new int[] { HorizontalBlockCount - 5, VerticalBlockCount - 4 });
                Buttons[Directions.Left].Add(new int[] { HorizontalBlockCount - 6, VerticalBlockCount - 3 });
                Buttons[Directions.Left].Add(new int[] { HorizontalBlockCount - 6, VerticalBlockCount - 4 });

                //Right
                Buttons.Add(Directions.Right, new List<int[]>());
                Buttons[Directions.Right].Add(new int[] { 4, VerticalBlockCount - 3 });
                Buttons[Directions.Right].Add(new int[] { 4, VerticalBlockCount - 4 });
                Buttons[Directions.Right].Add(new int[] { 5, VerticalBlockCount - 3 });
                Buttons[Directions.Right].Add(new int[] { 5, VerticalBlockCount - 4 });
                Buttons[Directions.Right].Add(new int[] { HorizontalBlockCount - 2, VerticalBlockCount - 3 });
                Buttons[Directions.Right].Add(new int[] { HorizontalBlockCount - 2, VerticalBlockCount - 4 });
                Buttons[Directions.Right].Add(new int[] { HorizontalBlockCount - 1, VerticalBlockCount - 3 });
                Buttons[Directions.Right].Add(new int[] { HorizontalBlockCount - 1, VerticalBlockCount - 4 });

                //Up
                Buttons.Add(Directions.Up, new List<int[]>());
                Buttons[Directions.Up].Add(new int[] { 2, VerticalBlockCount - 5 });
                Buttons[Directions.Up].Add(new int[] { 2, VerticalBlockCount - 6 });
                Buttons[Directions.Up].Add(new int[] { 3, VerticalBlockCount - 5 });
                Buttons[Directions.Up].Add(new int[] { 3, VerticalBlockCount - 6 });
                Buttons[Directions.Up].Add(new int[] { HorizontalBlockCount - 4, VerticalBlockCount - 5 });
                Buttons[Directions.Up].Add(new int[] { HorizontalBlockCount - 4, VerticalBlockCount - 6 });
                Buttons[Directions.Up].Add(new int[] { HorizontalBlockCount - 3, VerticalBlockCount - 5 });
                Buttons[Directions.Up].Add(new int[] { HorizontalBlockCount - 3, VerticalBlockCount - 6 });

                //Down
                Buttons.Add(Directions.Down, new List<int[]>());
                Buttons[Directions.Down].Add(new int[] { 2, VerticalBlockCount - 1 });
                Buttons[Directions.Down].Add(new int[] { 2, VerticalBlockCount - 2 });
                Buttons[Directions.Down].Add(new int[] { 3, VerticalBlockCount - 1 });
                Buttons[Directions.Down].Add(new int[] { 3, VerticalBlockCount - 2 });
                Buttons[Directions.Down].Add(new int[] { HorizontalBlockCount - 4, VerticalBlockCount - 1 });
                Buttons[Directions.Down].Add(new int[] { HorizontalBlockCount - 4, VerticalBlockCount - 2 });
                Buttons[Directions.Down].Add(new int[] { HorizontalBlockCount - 3, VerticalBlockCount - 1 });
                Buttons[Directions.Down].Add(new int[] { HorizontalBlockCount - 3, VerticalBlockCount - 2 });


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

            //Draw Highscore
            e.Surface.Canvas.DrawText(Highscore.ToString(), BorderThickness * 2, HighscoreFont.Size,HighscoreFont, WhitePaint);

            //Draw Buttons
            foreach(Directions Button in Buttons.Keys)
            {
                foreach(int[] segment in Buttons[Button])
                {
                    e.Surface.Canvas.DrawRect(new SKRect(BorderThickness + segment[0] * BlockSize, segment[1] * BlockSize, BorderThickness + BlockSize + segment[0] * BlockSize, BlockSize + segment[1] * BlockSize), TransparentGray);
                }
            }
        }

        private void SKGLView_Touch(object sender, SkiaSharp.Views.Forms.SKTouchEventArgs e)
        {
            if(e.Id == 0)
            {
                if(e.ActionType == SkiaSharp.Views.Forms.SKTouchAction.Pressed && !ControlLocked)
                {
                    Int32 SegX, SegY;
                    System.Diagnostics.Debug.WriteLine("Current Direction: " + _Direction.ToString());
                    System.Diagnostics.Debug.WriteLine("Touch at: " + e.Location.X + "|" + e.Location.Y);
                    SegX = (Int32)(e.Location.X / BlockSize);
                    SegY = (Int32)(e.Location.Y / BlockSize);
                    System.Diagnostics.Debug.WriteLine("Segment touched: " + SegX + "|" + SegY);

                    foreach(Directions direction in Buttons.Keys)
                    {
                        Directions found = _Direction;
                        Boolean stop = false;
                        foreach(int[] ButtonSegment in Buttons[direction])
                        {
                            if(ButtonSegment[0] == SegX && ButtonSegment[1] == SegY)
                            {
                                stop = true;
                                found = direction;
                                break;
                            }
                        }
                        if (stop)
                        {
                            if(_Direction != found)
                            {
                                if ((_Direction == Directions.Up && found == Directions.Down) || (_Direction == Directions.Down && found == Directions.Up) || (_Direction == Directions.Left && found == Directions.Right) || (_Direction == Directions.Right && found == Directions.Left)) break;
                                _Direction = found;
                                ControlLocked = true;
                            }                 
                            break;
                        }
                    }                    
                    System.Diagnostics.Debug.WriteLine("Current Direction: " + _Direction.ToString());
                }
            }
            e.Handled = true;
        }
    }
}