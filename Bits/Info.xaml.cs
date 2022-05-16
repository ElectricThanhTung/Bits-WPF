using System.Windows;

namespace Bits {
    public partial class Info : Window {
        private BitsWindowAnimation windowAnimation;

        public Info() {
            InitializeComponent();
            windowAnimation = new BitsWindowAnimation(this);
        }

        private void Button_CloseWindows(object sender, RoutedEventArgs e) {
            windowAnimation.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            windowAnimation.Close();
        }
    }
}
