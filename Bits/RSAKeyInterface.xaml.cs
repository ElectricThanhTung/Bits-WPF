using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Numerics;

namespace Bits {
    public partial class RSAKeyInterface : UserControl {
        private uint e;
        private BigInteger p;
        private BigInteger q;
        private BigInteger n;
        private BigInteger d;

        public RSAKeyInterface() {
            InitializeComponent();
            e = 65537;
            public_exponent.Text = e.ToString("X");
            public_exponent.TextChanged += public_exponent_TextChanged;
            display_combobox.SelectionChanged += display_combobox_SelectionChanged;
        }

        private void DisableControl() {
            this.Dispatcher.BeginInvoke(() => {
                bit_size_combobox.IsEnabled = false;
                display_combobox.IsEnabled = false;
                generate_button.IsEnabled = false;
            });
        }

        private void EnableControl() {
            this.Dispatcher.BeginInvoke(() => {
                bit_size_combobox.IsEnabled = true;
                display_combobox.IsEnabled = true;
                generate_button.IsEnabled = true;
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            int bitSize = Convert.ToInt32(bit_size_combobox.Text);
            RSAKeyGen(bitSize, this.e, true);
        }

        private void ShowResult(bool hexaMode) {
            string p_str, q_str, n_str, e_str, d_str;
            string mode = hexaMode ? "X" : "";
            p_str = (p == 0) ? "" : p.ToString(mode);
            q_str = (q == 0) ? "" : q.ToString(mode);
            n_str = (n == 0) ? "" : n.ToString(mode);
            e_str = (e == 0) ? "" : e.ToString(mode);
            d_str = (d == 0) ? "" : d.ToString(mode);

            this.Dispatcher.BeginInvoke(() => {
                public_exponent.TextChanged -= public_exponent_TextChanged;
                if(p > q) {
                    p_prime.Text = p_str;
                    q_prime.Text = q_str;
                }
                else {
                    p_prime.Text = q_str;
                    q_prime.Text = p_str;
                }
                modulus.Text = n_str;
                public_exponent.Text = e_str;
                private_exponent.Text = d_str;
                public_exponent.TextChanged += public_exponent_TextChanged;
            });
        }

        private void RSAKeyGen(int bitSize, uint publicExponent, bool genPrime) {
            DisableControl();
            error_textblock.Text = "";
            bool hexaMode = display_combobox.SelectedIndex == 1;
            Thread thread = new Thread(() => {
                if(genPrime) {
                    if(bitSize >= 512) {
                        Thread gen_q = new Thread(() => {
                            q = RSAGenKey.GenPrime(bitSize / 2);
                        });
                        gen_q.IsBackground = true;
                        gen_q.Start();
                        p = RSAGenKey.GenPrime(bitSize / 2);
                        while(gen_q.IsAlive)
                            Thread.Sleep(1);
                    }
                    else {
                        q = RSAGenKey.GenPrime(bitSize / 2);
                        p = RSAGenKey.GenPrime(bitSize / 2);
                    }
                }
                else if(p == 0 || q == 0) {
                    EnableControl();
                    return;
                }
                n = p * q;
                BigInteger phi = (p - 1) * (q - 1);
                if(e <= 1 || e >= phi) {
                    this.Dispatcher.BeginInvoke(() => {
                        error_textblock.Text = "Invalid value: 1 < e < phi";
                    });
                }
                else if(RSAGenKey.GCD(e, phi) != 1) {
                    this.Dispatcher.BeginInvoke(() => {
                        error_textblock.Text = "Invalid value: GDC(e, phi) must be 1";
                    });
                }
                else {
                    d = RSAGenKey.GenPrivateExponent(phi, publicExponent);
                    ShowResult(hexaMode);
                }
                EnableControl();
            });
            thread.IsBackground = true;
            thread.Start();
        }

        private void display_combobox_SelectionChanged(object? sender, SelectionChangedEventArgs e) {
            ShowResult(display_combobox.SelectedIndex == 1);
        }

        private void public_exponent_TextChanged(object sender, TextChangedEventArgs e) {
            try {
                if(display_combobox.SelectedIndex == 1)
                    this.e = (uint)Calculator.HexToInt(public_exponent.Text);
                else
                    this.e = Convert.ToUInt32(public_exponent.Text);
                int bitSize = Convert.ToInt32(bit_size_combobox.Text);
                RSAKeyGen(bitSize, this.e, false);
            }
            catch {
                this.e = 65537;
                error_textblock.Text = "Invalid value: Must be a number";
            }
        }
    }
}
