using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Bits {
    public partial class LED7Interface : UserControl {
        private byte[] numCode = { 0x3F, 0x06, 0x5B, 0x4F, 0x66, 0x6D, 0x7D, 0x07, 0x7F, 0x6F };

        public LED7Interface() {
            InitializeComponent();
            SetArrayResult(numCode);
        }

        private void AddText(Paragraph paragraph, string str, Brush color) {
            Run txt = new Run(str);
            txt.Foreground = color;
            paragraph.Inlines.Add(txt);
        }

        private void SetArrayResult(byte[] value) {
            Brush numColor = new SolidColorBrush(Color.FromArgb(255, 0, 128, 0));
            Paragraph paragraph = new Paragraph();
            AddText(paragraph, "const unsigned char ", Brushes.Blue);
            AddText(paragraph, "NumCode[", Brushes.Black);
            AddText(paragraph, value.Length.ToString(), numColor);
            AddText(paragraph, "] = {\r\n    ", Brushes.Black);
            for(int i = 0; i < value.Length; i++) {
                AddText(paragraph, (i != 0) ? ", " : ((i == 5) ? "," : ""), Brushes.Black);
                byte temp = value[i];
                if(LEDTypeComboBox.SelectedIndex != 0)
                    temp = (byte)~temp;
                if(i == 5)
                    AddText(paragraph, "\r\n    0x" + Calculator.IntToHex(temp), numColor);
                else
                    AddText(paragraph, "0x" + Calculator.IntToHex(temp), numColor);
            }
            AddText(paragraph, "\r\n};", Brushes.Black);
            ArrayResultBox.Document.Blocks.Clear();
            ArrayResultBox.Document.Blocks.Add(paragraph);
        }

        private void led_ValueChanged(object sender) {
            ResultBox.TextChanged -= ResultBox_TextChanged;
            byte value = ((LED7Seg)sender).Value;
            if(LEDTypeComboBox.SelectedIndex != 0)
                value = (byte)~value;
            ResultBox.Text = "0x" + Calculator.IntToHex(value);
            ResultBox.SelectionStart = ResultBox.Text.Length;
            ResultBox.TextChanged += ResultBox_TextChanged;
        }

        private void LEDTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if(ResultBox == null)
                return;
            ResultBox.TextChanged -= ResultBox_TextChanged;
            byte value = led.Value;
            if(LEDTypeComboBox.SelectedIndex != 0)
                value = (byte)~value;
            ResultBox.Text = "0x" + Calculator.IntToHex(value);
            ResultBox.SelectionStart = ResultBox.Text.Length;
            SetArrayResult(numCode);
            ResultBox.TextChanged += ResultBox_TextChanged;
        }

        private void ResultBox_TextChanged(object sender, TextChangedEventArgs e) {
            led.ValueChanged -= led_ValueChanged;
            try {
                string str = ResultBox.Text.Replace(" ", "").ToUpper();
                byte value;
                if(str.Length > 2 && str[0] == '0' && str[1] == 'X')
                    value = (byte)Calculator.HexToInt(ResultBox.Text);
                else
                    value = (byte)Convert.ToInt32(ResultBox.Text);
                if(LEDTypeComboBox.SelectedIndex != 0)
                    value = (byte)~value;
                led.Value = value;
            }
            catch {

            }
            led.ValueChanged += led_ValueChanged;
        }

        private void rtb_SelectionChanged(object sender, RoutedEventArgs e) {
            int selEnd = ArrayResultBox.Document.ContentStart.GetOffsetToPosition(ArrayResultBox.Selection.End);
            int docEnd = ArrayResultBox.Document.ContentStart.GetOffsetToPosition(ArrayResultBox.Document.ContentEnd);
            if(selEnd == docEnd)
                ArrayResultBox.Selection.Select(ArrayResultBox.Selection.Start, ArrayResultBox.Document.ContentStart.GetPositionAtOffset(docEnd - 2));
        }

        private void Button1_Click(object sender, RoutedEventArgs e) {
            led.Value = 0;
        }

        private void Button2_Click(object sender, RoutedEventArgs e) {
            string txt = new TextRange(ArrayResultBox.Document.ContentStart, ArrayResultBox.Document.ContentEnd).Text;
            Clipboard.SetText(txt);
        }
    }
}
