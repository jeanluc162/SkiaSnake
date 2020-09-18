using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using SkiaSharp;

namespace SkiaSnake
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PageGame : ContentPage
    {
        private enum Directions { Up, Right,Down,Left};
        private Directions _Direction = Directions.Right;
        public PageGame()
        {
            InitializeComponent();
        }
        private void SKGLView_PaintSurface(object sender, SkiaSharp.Views.Forms.SKPaintGLSurfaceEventArgs e)
        {
            e.Surface.Canvas.Clear(SKColors.LightGreen);
        }

        private void SKGLView_Touch(object sender, SkiaSharp.Views.Forms.SKTouchEventArgs e)
        {

        }
    }
}