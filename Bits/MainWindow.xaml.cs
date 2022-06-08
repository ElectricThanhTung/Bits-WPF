using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Threading;
using System.Windows.Media.Animation;

namespace Bits {
    public partial class MainWindow : Window {
        private BitsWindowAnimation windowAnimation;
        private int featureIndex = 0;
        private bool menuIsShow = false;

        enum ZoomAnimationMode {
            OpenWindow = 0,
            CloseWindow = 1,
            WindowMinimized = 2,
            WindowNormal = 3,
        }

        public MainWindow() {
            InitializeComponent();
            Menu.Width = 0;
            windowAnimation = new BitsWindowAnimation(this);
        }

        private int CurrentFeature {
            get {
                return featureIndex;
            }
            set {
                if(featureIndex != value) {
                    int oldValue = featureIndex;
                    featureIndex = value;
                    Thread thread = new Thread(delegate () {
                        int N = 30;
                        int N_1 = N - 1;
                        double target = -181 * value;
                        double currTop = 0;
                        this.Dispatcher.Invoke(new Action(() => {
                            currTop = FeatureView.Margin.Top;
                        }));
                        for(int i = 0; i < N; i++) {
                            this.Dispatcher.Invoke(new Action(() => {
                                FeatureView.Margin = new Thickness(FeatureView.Margin.Left, currTop + (target - currTop) * i / N_1, FeatureView.Margin.Right, FeatureView.Margin.Bottom);
                            }));
                            Thread.Sleep(200 / N);
                        }
                    });
                    thread.IsBackground = true;
                    thread.Start();

                    ((MenuItem)Menu.Items[oldValue]).Template = Menu.Resources["MenuItemStyle"] as ControlTemplate;
                    ((MenuItem)Menu.Items[value]).Template = Menu.Resources["MenuItemSelectedStyle"] as ControlTemplate;
                }
            }
        }

        private void Button_CloseWindows(object sender, RoutedEventArgs e) {
            windowAnimation.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            this.WindowState = WindowState.Minimized;
        }

        private void CheckBox_Changed(object sender, RoutedEventArgs e) {
            bool? topmost = ((CheckBox)sender).IsChecked;
            this.Topmost = (topmost == null || topmost == false) ? false : true;
        }

        private void Window_Deactivated(object? sender, EventArgs e) {
            if(this.Topmost) {
                if(this.WindowState != WindowState.Minimized) {
                    this.Opacity = 0.6D;
                }
                else
                    this.Opacity = 0D;
            }
        }

        private void ShowInfomation() {
            Info info = new Info();
            info.Owner = this;
            info.ShowDialog();
        }

        private void ShowMenu(bool isShow) {
            if(menuIsShow == isShow)
                return;
            menuIsShow = isShow;
            DoubleAnimation myDoubleAnimation = new DoubleAnimation();
            if(!isShow) {
                myDoubleAnimation.From = 100;
                myDoubleAnimation.To = 0;
            }
            else {
                myDoubleAnimation.From = 0;
                myDoubleAnimation.To = 100;
            }
            myDoubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(200));
            Storyboard.SetTargetName(myDoubleAnimation, "Menu");
            Storyboard.SetTargetProperty(myDoubleAnimation, new PropertyPath("Width"));

            Storyboard myStoryboard = new Storyboard();
            myStoryboard.Children.Add(myDoubleAnimation);

            myStoryboard.Begin(this);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e) {
            if(sender == Menu.Items[Menu.Items.Count - 1]) {
                ShowMenu(false);
                bool topmodOld = Topmost;
                Topmost = false;
                ShowInfomation();
                Topmost = topmodOld;
            }
            else {
                for(int i = 0; i < (Menu.Items.Count - 1); i++) {
                    if(sender == Menu.Items[i]) {
                        CurrentFeature = i;
                        break;
                    }
                }
            }
        }

        private bool CheckMouseInControl(Point point, FrameworkElement control) {
            Point relativePoint = control.TransformToAncestor(this).Transform(new Point(0, 0));
            if(point.X < relativePoint.X || point.X > (relativePoint.X + control.ActualWidth))
                return false;
            if(point.Y < relativePoint.Y || point.Y > (relativePoint.Y + control.ActualHeight))
                return false;
            return true;
        }

        private void Grid_PreviewMouseMove(object sender, MouseEventArgs e) {
            Point point = e.GetPosition(this);
            bool shown = false;
            if(CheckMouseInControl(point, MenuButton)) {
                shown = true;
                ShowMenu(true);
            }
            else {
                shown |= CheckMouseInControl(point, Menu);
                if(!shown)
                    ShowMenu(false);
            }
        }
    }
}
