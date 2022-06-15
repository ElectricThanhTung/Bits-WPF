using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Reflection;

namespace Bits {
    public partial class LED7Seg : UserControl {
        private byte value;
        private ImageSource led7Background;
        private ImageSource[] ledSeg = new ImageSource[8];
        public delegate void ValueChangedHandler(object sender);
        public event ValueChangedHandler? ValueChanged;

        public LED7Seg() {
            InitializeComponent();
            value = 0x00;
            led7Background = GetImageSource("Images/led7.png");
            ledSeg[0] = GetImageSource("Images/led7_a.png");
            ledSeg[1] = GetImageSource("Images/led7_b.png");
            ledSeg[2] = GetImageSource("Images/led7_c.png");
            ledSeg[3] = GetImageSource("Images/led7_d.png");
            ledSeg[4] = GetImageSource("Images/led7_e.png");
            ledSeg[5] = GetImageSource("Images/led7_f.png");
            ledSeg[6] = GetImageSource("Images/led7_g.png");
            ledSeg[7] = GetImageSource("Images/led7_dp.png");
        }

        private ImageSource GetImageSource(string imageName) {
            string uri = @"pack://application:,,,/";
            uri += Assembly.GetExecutingAssembly().GetName().Name;
            uri += @";component/";
            uri += imageName;
            return BitmapFrame.Create(new Uri(uri, UriKind.Absolute));
        }

        public byte Value {
            get {
                return this.value;
            }
            set {
                if(this.value != value) {
                    this.value = value;
                    if(ValueChanged != null)
                        ValueChanged(this);
                    this.InvalidateVisual();
                }
            }
        }

        protected override void OnRender(DrawingContext drawingContext) {
            Rect rect = new Rect(new Size(this.Width, this.Height));
            drawingContext.DrawImage(led7Background, rect);
            for(int i = 0; i < ledSeg.Length; i++) {
                if((value & (1 << i)) != 0)
                    drawingContext.DrawImage(ledSeg[i], rect);
            }
        }

        private Color GetPixel(BitmapSource imageSource, double x, double y) {
            if(x < 0 || x >= (int)imageSource.Width || y < 0 || y >= (int)imageSource.Height)
                return Color.FromArgb(0, 0, 0, 0);
            byte[] bytes = new byte[(int)((int)imageSource.Width * (int)imageSource.Height * 4)];
            imageSource.CopyPixels(bytes, (int)(imageSource.Width * 4), 0);
            int index = (int)((int)x + (int)y * (int)imageSource.Width) * 4;
            byte A = bytes[index + 3];
            byte R = bytes[index + 2];
            byte G = bytes[index + 1];
            byte B = bytes[index + 0];
            return Color.FromArgb(A, R, G, B);
        }

        private void UserControl_MouseMove(object sender, MouseEventArgs e) {
            Point point = e.GetPosition(this);
            double x = point.X * led7Background.Width / this.Width;
            double y = point.Y * led7Background.Height / this.Height;
            if(GetPixel((BitmapSource)led7Background, x, y).A != 0)
                this.Cursor = Cursors.Hand;
            else
                this.Cursor = null;
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e) {
            Point point = e.GetPosition(this);
            for(int i = 0; i < ledSeg.Length; i++) {
                if(GetPixel((BitmapSource)ledSeg[i], point.X, point.Y).A != 0) {
                    value ^= (byte)(1 << i);
                    if(ValueChanged != null)
                        ValueChanged(this);
                    this.InvalidateVisual();
                    break;
                }
            }
        }
    }
}
