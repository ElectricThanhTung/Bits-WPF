using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Bits {
    public class BitsWindowAnimation {
        private bool WindowShown = false;
        private double topOld;
        private Thread? animationThread = null;
        private Size WindowSize;
        private Window windowsParent;

        enum ZoomAnimationMode {
            OpenWindow = 0,
            CloseWindow = 1,
            WindowMinimized = 2,
            WindowNormal = 3,
        }

        public BitsWindowAnimation(Window parent) {
            windowsParent = parent;
            windowsParent.Opacity = 0D;
            windowsParent.MouseDown += Window_MouseDown;
            windowsParent.ContentRendered += Window_ContentRendered;
        }

        private void Window_MouseDown(object? sender, MouseButtonEventArgs e) {
            if(e.GetPosition(windowsParent).Y < 30 && e.ChangedButton == MouseButton.Left)
                windowsParent.DragMove();
        }

        private void Window_ContentRendered(object? sender, EventArgs e) {
            if(!WindowShown) {
                WindowShown = true;
                topOld = windowsParent.Top;
                WindowSize = new Size(windowsParent.ActualWidth, windowsParent.ActualHeight);
                ZoomAnimation(ZoomAnimationMode.OpenWindow, null);
            }
        }

        private void Window_Activated(object? sender, EventArgs e) {
            if(windowsParent.WindowState != WindowState.Minimized)
                windowsParent.Opacity = 1D;
        }

        private void Window_StateChanged(object? sender, EventArgs e) {
            if(windowsParent.WindowState == WindowState.Minimized)
                ZoomAnimation(ZoomAnimationMode.WindowMinimized, null);
            else
                ZoomAnimation(ZoomAnimationMode.WindowNormal, null);
        }

        private void Window_Deactivated(object? sender, EventArgs e) {
            if(windowsParent.Topmost) {
                if(windowsParent.WindowState != WindowState.Minimized)
                    topOld = windowsParent.Top;
            }
        }

        public void Close() {
            ZoomAnimation(ZoomAnimationMode.CloseWindow, delegate() {
                windowsParent.Close();
            });
        }

        private void ZoomAnimation(ZoomAnimationMode amimatinMode, ThreadStart? animationComplated) {
            if(animationThread != null && animationThread.IsAlive)
                return;

            windowsParent.Activated -= Window_Activated;
            windowsParent.StateChanged -= Window_StateChanged;

            FrameworkElement MainView = (FrameworkElement)windowsParent.Content;

            int N = 15;
            double Left = windowsParent.Left;
            double Top = windowsParent.Top;

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
                topStep = (windowsParent.Top - topOld) / N;
            }
            else {
                windowsParent.WindowState = WindowState.Normal;

                fromScale = 1D;
                toScale = 0.5D;
                fromOpacity = 1;
                toOpacity = 0;
                topStep = (SystemParameters.WorkArea.Height - windowsParent.Top) / (N * 2);
                topOld = windowsParent.Top;
            }

            double fromW = fromScale * WindowSize.Width;
            double fromH = fromScale * WindowSize.Height;
            double toW = toScale * WindowSize.Width;
            double toH = toScale * WindowSize.Height;

            double W = MainView.Width;
            double H = MainView.Height;

            MainView.Width = fromW > toW ? fromW : toW;
            MainView.Height = fromH > toH ? fromH : toH;
            windowsParent.Left = windowsParent.Left + (W - MainView.Width) / 2;
            windowsParent.Top = windowsParent.Top + (H - MainView.Height) / 2;

            Image a = new Image();
            a.Width = WindowSize.Width;
            a.Height = WindowSize.Height;

            MainView.Measure(WindowSize);
            MainView.Arrange(new Rect(WindowSize));
            RenderTargetBitmap bmp = new RenderTargetBitmap((int)WindowSize.Width, (int)WindowSize.Height, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(MainView);

            Grid grid = new Grid();
            grid.Width = MainView.Width;
            grid.Height = MainView.Height;
            grid.Children.Add(a);
            a.Source = bmp;

            windowsParent.Content = grid;

            if(amimatinMode == ZoomAnimationMode.CloseWindow || amimatinMode == ZoomAnimationMode.WindowMinimized)
                windowsParent.Opacity = 1D;

            animationThread = new Thread(() => {
                try {
                    for(int i = 0; i < N; i++) {
                        windowsParent.Dispatcher.Invoke(() => {
                            if(amimatinMode == ZoomAnimationMode.WindowMinimized)
                                windowsParent.Top += topStep;
                            else if(amimatinMode == ZoomAnimationMode.WindowNormal)
                                windowsParent.Top -= topStep;
                            a.Width = fromW + (toW - fromW) * i / (N - 1);
                            a.Height = fromH + (toH - fromH) * i / (N - 1);
                            windowsParent.Opacity = fromOpacity + (toOpacity - fromOpacity) * i / (N - 1);
                        });
                        Thread.Sleep(10);
                    }
                }
                catch { }

                windowsParent.Dispatcher.Invoke(() => {
                    windowsParent.Content = MainView;

                    if(amimatinMode == ZoomAnimationMode.WindowNormal)
                        windowsParent.WindowState = WindowState.Normal;
                    else if(amimatinMode == ZoomAnimationMode.WindowMinimized) {
                        MainView.Width = WindowSize.Width;
                        MainView.Height = WindowSize.Height;
                        windowsParent.WindowState = WindowState.Minimized;
                    }

                    windowsParent.Activated += Window_Activated;
                    windowsParent.StateChanged += Window_StateChanged;

                    if(animationComplated != null)
                        animationComplated();
                });
            });
            animationThread.IsBackground = true;
            animationThread.Start();
        }
    }
}
