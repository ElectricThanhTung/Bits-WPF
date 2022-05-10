using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Bits {
    class CustomTextBox : RichTextBox {
        private int maxLength = 128;
        private Brush[] colorList = { Brushes.Red, Brushes.Orange, Brushes.Green, Brushes.Aqua, Brushes.Blue, Brushes.Purple };

        public CustomTextBox() {
            this.Padding = new Thickness(-3, 3, 0, 3);
            this.TextChanged += rtb_TextChanged;
            this.SelectionChanged += rtb_SelectionChanged;
            this.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            this.Document.PageWidth = 1500;
        }

        public int MaxLength {
            get {
                return maxLength;
            }
            set {
                maxLength = value;
            }
        }

        public string Text {
            get {
                string txt = new TextRange(this.Document.ContentStart, this.Document.ContentEnd).Text;
                txt = txt.Replace("\r", "");
                txt = txt.Replace("\n", "");
                return txt;
            }
            set {
                SetText(value);
            }
        }

        public int SelectionStart {
            get {
                return this.Document.ContentStart.GetOffsetToPosition(this.CaretPosition);
            }
            set {
                int index = value;
                try {
                    while(true) {
                        this.CaretPosition = this.Document.ContentStart.GetPositionAtOffset(index);
                        if(this.CaretPosition.LogicalDirection == LogicalDirection.Forward || this.Document.ContentStart.GetOffsetToPosition(this.CaretPosition) < value)
                            index++;
                        else
                            break;
                    }
                }
                catch {
                    this.CaretPosition = this.Document.ContentEnd;
                }
            }
        }

        private void SetText(string text) {
            this.Document.Blocks.Clear();
            this.Document.Blocks.Add(new Paragraph(new Run(text)));
        }

        private void rtb_TextChanged(object sender, TextChangedEventArgs e) {
            TextRange textRange = new TextRange(this.Document.ContentStart, this.Document.ContentEnd);
            string text = textRange.Text;

            this.TextChanged -= rtb_TextChanged;

            int currPos = this.SelectionStart;

            int i = 0;
            int count = 0;
            StringBuilder temp = new StringBuilder();
            Paragraph paragraph = new Paragraph();
            this.Document.Blocks.Clear();
            while(i < text.Length && i < maxLength) {
                if(text[i] == '(') {
                    Run txt;
                    if(temp.Length > 0) {
                        txt = new Run(temp.ToString());
                        txt.Foreground = Brushes.Black;
                        paragraph.Inlines.Add(txt);
                        temp.Clear();
                    }
                    txt = new Run("(");
                    txt.Foreground = colorList[count % colorList.Length];
                    paragraph.Inlines.Add(txt);
                    count++;
                }
                else if(text[i] == ')') {
                    if(count > 0)
                        count--;
                    Run txt;
                    if(temp.Length > 0) {
                        txt = new Run(temp.ToString());
                        txt.Foreground = Brushes.Black;
                        paragraph.Inlines.Add(txt);
                        temp.Clear();
                    }
                    txt = new Run(")");
                    txt.Foreground = colorList[count % colorList.Length];
                    paragraph.Inlines.Add(txt);
                }
                else if(text[i] != '\r' && text[i] != '\n')
                    temp.Append(text[i]);
                i++;
            }
            if(temp.Length > 0) {
                Run txt = new Run(temp.ToString());
                txt.Foreground = Brushes.Black;
                paragraph.Inlines.Add(txt);
            }
            this.Document.Blocks.Add(paragraph);
            this.SelectionStart = currPos;
            //this.CaretPosition = this.Document.ContentEnd;
            currPos = this.SelectionStart;

            this.TextChanged += rtb_TextChanged;
        }

        private void rtb_SelectionChanged(object sender, RoutedEventArgs e) {
            int selEnd = this.Document.ContentStart.GetOffsetToPosition(this.Selection.End);
            int docEnd = this.Document.ContentStart.GetOffsetToPosition(this.Document.ContentEnd);
            if(selEnd == docEnd)
                this.Selection.Select(this.Selection.Start, this.Document.ContentStart.GetPositionAtOffset(docEnd - 2));
        }
    }
}
