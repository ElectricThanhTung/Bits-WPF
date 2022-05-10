using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Windows.Documents;

namespace Bits {
    public partial class MainWindow : Window {
        private bool WindowShown = false;
        private bool RegisterEventEnabled = true;
        private double topOld;
        private Thread? animationThread = null;

        enum ZoomAnimationMode {
            OpenWindow = 0,
            CloseWindow = 1,
            WindowMinimized = 2,
            WindowNormal = 3,
        }

        public MainWindow() {
            InitializeComponent();
            this.Opacity = 0D;
            expression_textbox.Width = 394;
            Thread CalculationProcess = new Thread(CalculationHandler);
            CalculationProcess.IsBackground = true;
            CalculationProcess.Start();
        }

        private void ZoomAnimation(ZoomAnimationMode amimatinMode, ThreadStart? animationComplated) {
            if(animationThread != null && animationThread.IsAlive)
                return;

            this.Activated -= Window_Activated;
            this.Deactivated -= Window_Deactivated;
            this.StateChanged -= Window_StateChanged;

            Size WindowSize = new Size(408, 239);
            int N = 15;
            double Left = this.Left;
            double Top = this.Top;
            
            double fromScale;
            double toScale;
            double fromOpacity;
            double toOpacity;
            double topStep = 0;

            if(amimatinMode == ZoomAnimationMode.OpenWindow || amimatinMode == ZoomAnimationMode.WindowNormal) {
                fromScale = 0.5D;
                toScale = 1D;
                fromOpacity = 0;
                toOpacity = 1;
                topStep = (this.Top - topOld) / N;
            }
            else {
                this.WindowState = WindowState.Normal;

                fromScale = 1D;
                toScale = 0.5D;
                fromOpacity = 1;
                toOpacity = 0;
                topStep = (SystemParameters.WorkArea.Height - this.Top) / (N * 2);
                topOld = this.Top;
            }

            double fromW = fromScale * WindowSize.Width;
            double fromH = fromScale * WindowSize.Height;
            double toW = toScale * WindowSize.Width;
            double toH = toScale * WindowSize.Height;

            double W = MainGrid.Width;
            double H = MainGrid.Height;

            MainGrid.Width = fromW > toW ? fromW : toW;
            MainGrid.Height = fromH > toH ? fromH : toH;
            this.Left = this.Left + (W - MainGrid.Width) / 2;
            this.Top = this.Top + (H - MainGrid.Height) / 2;

            Image a = new Image();
            a.Width = WindowSize.Width;
            a.Height = WindowSize.Height;

            MainView.Measure(WindowSize);
            MainView.Arrange(new Rect(WindowSize));
            RenderTargetBitmap bmp = new RenderTargetBitmap((int)WindowSize.Width, (int)WindowSize.Height, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(MainView);

            a.Source = bmp;
            MainGrid.Children.RemoveAt(0);
            MainGrid.Children.Add(a);

            if(amimatinMode == ZoomAnimationMode.CloseWindow || amimatinMode == ZoomAnimationMode.WindowMinimized)
                this.Opacity = 1D;

            animationThread = new Thread(() => {
                try {
                    for(int i = 0; i < N; i++) {
                        this.Dispatcher.Invoke(() => {
                            if(amimatinMode == ZoomAnimationMode.WindowMinimized)
                                this.Top += topStep;
                            else if(amimatinMode == ZoomAnimationMode.WindowNormal)
                                this.Top -= topStep;
                            a.Width = fromW + (toW - fromW) * i / (N - 1);
                            a.Height = fromH + (toH - fromH) * i / (N - 1);
                            this.Opacity = fromOpacity + (toOpacity - fromOpacity) * i / (N - 1);
                        });
                        Thread.Sleep(10);
                    }
                } catch { }

                this.Dispatcher.Invoke(() => {
                    MainGrid.Children.Add(MainView);
                    MainGrid.Children.RemoveAt(0);
                    expression_textbox.Focus();

                    if(amimatinMode == ZoomAnimationMode.WindowNormal)
                        this.WindowState = WindowState.Normal;
                    else if(amimatinMode == ZoomAnimationMode.WindowMinimized) {
                        MainGrid.Width = WindowSize.Width;
                        MainGrid.Height = WindowSize.Height;
                        this.WindowState = WindowState.Minimized;
                    }

                    this.Activated += Window_Activated;
                    this.Deactivated += Window_Deactivated;
                    this.StateChanged += Window_StateChanged;

                    if(animationComplated != null)
                        animationComplated();
                });
            });
            animationThread.IsBackground = true;
            animationThread.Start();
        }

        override protected void OnContentRendered(EventArgs e) {
            if(!WindowShown) {
                WindowShown = true;
                expression_textbox.MaxWidth = expression_textbox.ActualWidth;
                interger_textbox.MaxWidth = interger_textbox.ActualWidth;
                hex_textbox.MaxWidth = hex_textbox.ActualWidth;
                result_textblock.MaxWidth = result_textblock.ActualWidth;
                topOld = this.Top;

                ZoomAnimation(ZoomAnimationMode.OpenWindow, null);
            }
            base.OnContentRendered(e);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {
            if(e.GetPosition(this).Y < 30 && e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Register_ValueChanged(object sender) {
            if(RegisterEventEnabled) {
                uint value = reg0.Value;
                value |= (uint)reg1.Value << 8;
                value |= (uint)reg2.Value << 16;
                value |= (uint)reg3.Value << 24;
                expression_textbox.Text = value.ToString();
                expression_textbox.SelectionStart = expression_textbox.Text.Length;
            }
        }

        private void TextBlock_MouseDown(object sender, MouseEventArgs e) {
            ClrResult();
        }

        private void TextBlock_MouseEnter(object sender, MouseEventArgs e) {
            ((TextBlock)sender).Foreground = Brushes.Red;
        }

        private void TextBlock_MouseLeave(object sender, MouseEventArgs e) {
            ((TextBlock)sender).Foreground = Brushes.Blue;
        }

        private void Button_CloseWindows(object sender, RoutedEventArgs e) {
            ZoomAnimation(ZoomAnimationMode.CloseWindow, delegate () {
                this.Close();
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            this.Activated -= Window_Activated;
            this.Deactivated -= Window_Deactivated;
            this.StateChanged -= Window_StateChanged;
            ZoomAnimation(ZoomAnimationMode.WindowMinimized, null);
        }

        private void ClrResult() {
            this.Dispatcher.BeginInvoke(() => {
                interger_textbox.Text = "";
                hex_textbox.Text = "";
                expression_textbox.Text = "";
                result_textblock.Text = "0";
                result_textblock.Foreground = Brushes.Blue;
                RegisterEventEnabled = false;
                reg0.Value = 0;
                reg1.Value = 0;
                reg2.Value = 0;
                reg3.Value = 0;
                binary_text_0.Text = Calculator.ByteToBIN(reg0.Value);
                binary_text_1.Text = Calculator.ByteToBIN(reg1.Value);
                binary_text_2.Text = Calculator.ByteToBIN(reg2.Value);
                binary_text_3.Text = Calculator.ByteToBIN(reg3.Value);
                RegisterEventEnabled = true;
            });
        }

        private void ShowResult(decimal value, bool updateExpression) {
            this.Dispatcher.BeginInvoke(() => {
                System.Numerics.BigInteger interger = (System.Numerics.BigInteger)value;
                interger_textbox.Text = interger.ToString();
                interger &= 0xFFFFFFFFFFFFFFFF;
                ulong uint64 = (ulong)interger;
                hex_textbox.Text = "0x" + Calculator.IntToHex(uint64);
                result_textblock.Cursor = Cursors.IBeam;
                result_textblock.Foreground = Brushes.Blue;
                result_textblock.Text = ((value % 1) == 0) ? interger_textbox.Text : value.ToString();
                if(updateExpression) {
                    expression_textbox.Text = result_textblock.Text;
                    expression_textbox.SelectionStart = expression_textbox.Text.Length;
                }
                RegisterEventEnabled = false;
                reg0.Value = (byte)(((uint64) >> 0) & 0xFF);
                reg1.Value = (byte)(((uint64) >> 8) & 0xFF);
                reg2.Value = (byte)(((uint64) >> 16) & 0xFF);
                reg3.Value = (byte)(((uint64) >> 24) & 0xFF);
                binary_text_0.Text = Calculator.ByteToBIN(reg0.Value);
                binary_text_1.Text = Calculator.ByteToBIN(reg1.Value);
                binary_text_2.Text = Calculator.ByteToBIN(reg2.Value);
                binary_text_3.Text = Calculator.ByteToBIN(reg3.Value);
                RegisterEventEnabled = true;
            });
        }

        private void CalculationHandler() {
            string oldStr = "";
            string newStr = "";
            expression_textbox.Dispatcher.Invoke(() => {
                oldStr = expression_textbox.Text;
            });
            while(true) {
                try {
                    do {
                        this.Dispatcher.Invoke(() => {
                            newStr = expression_textbox.Text;
                        });
                        Thread.Sleep(50);
                    } while(oldStr == newStr);
                    oldStr = newStr;
                    if(oldStr.Trim() == "") {
                        ClrResult();
                        continue;
                    }
                    ShowResult(Calculator.Decimal(oldStr), false);
                }
                catch {
                    try {
                        this.Dispatcher.BeginInvoke(() => {
                            result_textblock.Cursor = Cursors.Arrow;
                            result_textblock.Text = "Invalid expression";
                            result_textblock.Foreground = Brushes.Red;
                        });
                    }
                    catch {

                    }
                }
            }
        }

        private void CheckBox_Changed(object sender, RoutedEventArgs e) {
            bool? topmost = ((CheckBox)sender).IsChecked;
            this.Topmost = (topmost == null || topmost == false) ? false : true;
        }

        private void expression_textbox_KeyDown(object sender, KeyEventArgs e) {
            if(e.Key == Key.Enter) {
                try {
                    ShowResult(Calculator.Decimal(expression_textbox.Text), true);
                }
                catch {

                }
            }
        }

        private void result_textblock_SelectionChanged(object sender, RoutedEventArgs e) {
            if(result_textblock.Foreground != Brushes.Blue)
                if(result_textblock.SelectionLength != 0)
                    result_textblock.SelectionLength = 0;
        }

        private void Window_Activated(object? sender, EventArgs e) {
            if(this.WindowState != WindowState.Minimized)
                this.Opacity = 1D;
        }

        private void Window_Deactivated(object? sender, EventArgs e) {
            if(this.Topmost) {
                if(this.WindowState != WindowState.Minimized) {
                    topOld = this.Top;
                    this.Opacity = 0.6D;
                }
                else
                    this.Opacity = 0D;
            }
        }

        private void Window_StateChanged(object? sender, EventArgs e) {
            if(this.WindowState == WindowState.Minimized)
                ZoomAnimation(ZoomAnimationMode.WindowMinimized, null);
            else
                ZoomAnimation(ZoomAnimationMode.WindowNormal, null);
        }

        private void expression_textbox_ScrollChanged(object sender, ScrollChangedEventArgs e) {
            if(expression_textbox.HorizontalOffset == 0)
                expression_textbox.Padding = new Thickness(-3, 3, 0, 3);
            else
                expression_textbox.Padding = new Thickness(2, 3, 0, 3);
        }
    }
}
