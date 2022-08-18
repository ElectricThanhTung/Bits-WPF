
﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Threading;

namespace Bits {
    public partial class CalcInterface : UserControl {
        private bool WindowShown = false;
        private bool RegisterEventEnabled = true;

        public CalcInterface() {
            InitializeComponent();
            expression_textbox.Width = 394;
            Thread CalculationProcess = new Thread(CalculationHandler);
            CalculationProcess.IsBackground = true;
            CalculationProcess.Start();
        }

        override protected void OnRender(DrawingContext drawingContext) {
            if(!WindowShown) {
                WindowShown = true;
                expression_textbox.MaxWidth = expression_textbox.ActualWidth;
                interger_textbox.MaxWidth = interger_textbox.ActualWidth;
                hex_textbox.MaxWidth = hex_textbox.ActualWidth;
                result_textblock.MaxWidth = result_textblock.ActualWidth;
            }
            base.OnRender(drawingContext);
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

        private void ClrResult(bool clearExpression) {
            this.Dispatcher.BeginInvoke(() => {
                interger_textbox.Text = "";
                hex_textbox.Text = "";
                if(clearExpression)
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
                        ClrResult(false);
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

        private void TextBlock_MouseEnter(object sender, MouseEventArgs e) {
            ((TextBlock)sender).Foreground = Brushes.Red;
        }

        private void TextBlock_MouseLeave(object sender, MouseEventArgs e) {
            ((TextBlock)sender).Foreground = Brushes.Blue;
        }

        private void TextBlock_MouseDown(object sender, MouseEventArgs e) {
            ClrResult(true);
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

        private void expression_textbox_ScrollChanged(object sender, ScrollChangedEventArgs e) {
            if(expression_textbox.HorizontalOffset == 0)
                expression_textbox.Padding = new Thickness(-3, 3, 0, 3);
            else
                expression_textbox.Padding = new Thickness(2, 3, 0, 3);
        }
    }
}
