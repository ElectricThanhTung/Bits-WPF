using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Globalization;

namespace Bits {
    public partial class Register : UserControl {
        private byte registerValue = 0x00;
        private uint bitOffset = 0;
        public delegate void ValueChangedHandler(object sender);
        public event ValueChangedHandler? ValueChanged;

        public Register() {
            InitializeComponent();
            this.MouseDown += new MouseButtonEventHandler(MouseDownEventHandler);
        }

        private void MouseDownEventHandler(object sender, MouseButtonEventArgs e) {
            int bit = 7 - (int)((e.GetPosition(this).X - 1) * 8 / this.Width);
            registerValue ^= (byte)(1 << bit);
            if(ValueChanged != null)
                ValueChanged(this);
            this.InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext) {
            double X = this.Width;
            double Y = this.Height;
            Pen pen = new Pen(Brushes.Black, 1);
            for(int i = 0; i < 8; i++) {
                bool bit = ((registerValue & (1 << (7 - i))) > 0) ? true : false;
                Rect rect = new Rect(i * X / 8, 0, this.Width / 8, RenderSize.Height);
                drawingContext.DrawRectangle(bit ? Brushes.Red : Brushes.White, pen, rect);
                FormattedText formattedText = new FormattedText(((7 - i) + bitOffset) + "",
                                                                CultureInfo.GetCultureInfo("en-us"),
                                                                FlowDirection.LeftToRight, new Typeface("Verdana"),
                                                                11,
                                                                bit ? Brushes.WhiteSmoke : Brushes.Black,
                                                                VisualTreeHelper.GetDpi(this).PixelsPerDip);
                double x = i * X / 8 + (this.Width / 8 - formattedText.Width) / 2;
                double y = (this.Height - formattedText.Height) / 2;
                drawingContext.DrawText(formattedText, new Point(x, y));
            }
            pen = new Pen(Brushes.Black, 0.1);
            drawingContext.DrawLine(pen, new Point(0, 0), new Point(this.Width, 0));
            drawingContext.DrawLine(pen, new Point(0, Y), new Point(X, Y));
            for(int i = 0; i < 9; i++)
                drawingContext.DrawLine(pen, new Point(i * X / 8, 0), new Point(i * X / 8, Y));
        }

        public uint BitOffset {
            get {
                return bitOffset;
            }
            set {
                bitOffset = value;
                this.InvalidateVisual();
            }
        }

        public byte Value {
            get {
                return registerValue;
            }
            set {
                if(value != registerValue) {
                    registerValue = value;
                    this.InvalidateVisual();
                    if(ValueChanged != null)
                        ValueChanged(this);
                }
            }
        }
    }
}
