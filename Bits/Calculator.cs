using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Bits {
    internal static class Calculator {
        private static NumberFormatInfo provider = new NumberFormatInfo();

        public static long HexToInt(string hex) {
            int index = 0;
            long value = 0;

            hex = hex.ToUpper().Replace("0X", "");

            if(hex[0] == '-' || hex[0] == '+')
                index++;

            for(int i = index; i < hex.Length; i++) {
                value <<= 4;
                if(hex[i] >= '0' && hex[i] <= '9')
                    value |= (long)(hex[i] - '0');
                else if(hex[i] >= 'A' && hex[i] <= 'F')
                    value |= (long)(hex[i] - 'A' + 10);
                else
                    throw new Exception("Invalid hex number format");
            }
            if(hex[0] == '-')
                return -value;
            return value;
        }

        public static long BinToInt(string bin) {
            long value = 0;
            int index = 2;
            if(bin[0] == '-' || bin[0] == '+')
                index++;
            for(int j = index; j < bin.Length; j++) {
                value <<= 1;
                if(bin[j] == '1')
                    value |= 1;
                else if(bin[j] != '0')
                    throw new Exception("Invalid bin number format");
            }
            if(bin[0] == '-')
                return -value;
            return value;
        }

        public static string IntToHex(ulong num) {
            ulong temp = (ulong)num;
            string hexchar = "0123456789ABCDEF";
            string str = "";
            do {
                str = hexchar[(int)(temp % 16)] + str;
                temp /= 16;
            }
            while(temp > 0);
            if((str.Length % 2) == 1)
                str = "0" + str;
            return str;
        }

        public static string ByteToBIN(byte value) {
            string res = "";
            for(int i = 0; i < 8; i++) {
                res += ((value & 0x80) == 0x80) ? 1 : 0;
                value <<= 1;
            }
            return res;
        }

        private static StringBuilder Upper(StringBuilder expression, string str) {
            return new StringBuilder(Upper(expression.ToString(), str));
        }

        private static string Upper(string expression, string str) {
            str = str.ToLower();
            string patten = "";
            for(int i = 0; i < str.Length; i++)
                patten += "[" + str[i] + (char)(str[i] - 32) + "]";
            patten += @" *\(";
            str = str.ToUpper();
            foreach(Match match in Regex.Matches(expression, patten))
                expression = expression.Replace(match.Value, str + "(");
            return expression;
        }

        private static string? GetContentOfCalculation(string mainExpression, string calculation) {
            calculation = calculation.ToUpper() + "(";
            int index = mainExpression.IndexOf(calculation);
            if(index < 0)
                return null;
            string str = mainExpression.Substring(index + calculation.Length);
            int count = 0;
            int index_end = -1;
            for(int i = 0; i < str.Length; i++) {
                if(str[i] == ')') {
                    if(count == 0) {
                        index_end = i;
                        break;
                    }
                    else
                        count--;
                }
                else if(str[i] == '(')
                    count++;
            }
            if(index_end < 0)
                throw new Exception("Invalid expression");
            return str.Substring(0, index_end);
        }

        private static string CalSqrt(string expression) {
            expression = Upper(expression, "sqrt");
            while(true) {
                string? str = GetContentOfCalculation(expression, "sqrt");
                if(str == null)
                    return expression;
                double res = (double)Decimal(str);
                res = Math.Sqrt(res);
                expression = expression.Replace("SQRT(" + str + ")", res + "");
            }
        }

        private static string CalSin(string expression) {
            expression = Upper(expression, "sin");
            while(true) {
                string? str = GetContentOfCalculation(expression, "sin");
                if(str == null)
                    return expression;
                double res = (double)Decimal(str);
                res = Math.Sin(res * Math.PI / 180);
                expression = expression.Replace("SIN(" + str + ")", res + "");
            }
        }

        private static string CalCos(string expression) {
            expression = Upper(expression, "cos");
            while(true) {
                string? str = GetContentOfCalculation(expression, "cos");
                if(str == null)
                    return expression;
                double res = (double)Decimal(str);
                res = Math.Cos(res * Math.PI / 180);
                expression = expression.Replace("COS(" + str + ")", res + "");
            }
        }

        private static string CalTan(string expression) {
            expression = Upper(expression, "tan");
            while(true) {
                string? str = GetContentOfCalculation(expression, "tan");
                if(str == null)
                    return expression;
                double res = (double)Decimal(str);
                res = Math.Tan(res * Math.PI / 180);
                expression = expression.Replace("TAN(" + str + ")", res + "");
            }
        }

        private static string CalCot(string expression) {
            expression = Upper(expression, "cot");
            while(true) {
                string? str = GetContentOfCalculation(expression, "cot");
                if(str == null)
                    return expression;
                double res = (double)Decimal(str);
                res = 1 / Math.Tan(res * Math.PI / 180);
                expression = expression.Replace("COT(" + str + ")", res + "");
            }
        }

        private static string CalLog10(string expression) {
            expression = Upper(expression, "log");
            while(true) {
                string? str = GetContentOfCalculation(expression, "log");
                if(str == null)
                    return expression;
                double res = (double)Decimal(str);
                res = Math.Log10(res);
                expression = expression.Replace("LOG(" + str + ")", res + "");
            }
        }

        private static string CalLn(string expression) {
            expression = Upper(expression, "ln");
            while(true) {
                string? str = GetContentOfCalculation(expression, "ln");
                if(str == null)
                    return expression;
                double res = (double)Decimal(str);
                res = Math.Log(res, Math.E);
                expression = expression.Replace("LN(" + str + ")", res + "");
            }
        }

        private static decimal CalFactorial(long value) {
            decimal ret = 1;
            if(value < 0)
                throw new ArgumentOutOfRangeException("value");
            for(int i = 2; i < (value + 1); i++)
                ret *= i;
            return ret; ;
        }

        public static decimal Decimal(string expression) {
            provider.NumberDecimalSeparator = ",";
            provider.NumberGroupSeparator = ".";

            expression = CalSqrt(expression);
            expression = CalSin(expression);
            expression = CalCos(expression);
            expression = CalTan(expression);
            expression = CalCot(expression);
            expression = CalLog10(expression);
            expression = CalLn(expression);

            List<StringBuilder> temp;
            Stack<StringBuilder> stack = new Stack<StringBuilder>();
            CheckSyntax(expression);
            StringBuilder exp = Normalization(new StringBuilder(expression));
            temp = SeparationOfExpressions(exp);
            ConvertAllToInt(temp);
            temp = Postfix(temp);

            for(int i = 0; i < temp.Count; i++) {
                StringBuilder str = temp[temp.Count - i - 1];
                if(IsNumber(str.ToString()))
                    stack.Push(str);
                else {
                    char Operator = str[0];
                    if(Operator == '!' || Operator == '~' || Operator == 'I') {
                        long value = (long)Convert.ToDecimal(stack.Pop().ToString(), provider);
                        if(Operator == '!')
                            stack.Push(new StringBuilder(1).Append(value > 0 ? '0' : '1'));
                        else if(Operator == '~')
                            stack.Push(new StringBuilder(1).Append(~value));
                        else
                            stack.Push(new StringBuilder(1).Append(CalFactorial(value)));
                    }
                    else {
                        decimal b = Convert.ToDecimal(stack.Pop().ToString(), provider);
                        decimal a = Convert.ToDecimal(stack.Pop().ToString(), provider);
                        decimal res = 0;
                        if(Operator == '+')
                            res = a + b;
                        if(Operator == '-')
                            res = a - b;
                        else if(Operator == '*')
                            res = a * b;
                        else if(Operator == '/')
                            res = a / b;
                        else if(Operator == '^') {
                            long b1 = (long)(b / 2);
                            decimal res1 = (decimal)Math.Pow((double)a, b1);
                            decimal res2 = (decimal)Math.Pow((double)a, (double)(b - b1));
                            res = res1 * res2;
                        }
                        else if(Operator == '&')
                            res = (long)a & (long)b;
                        else if(Operator == '|')
                            res = (long)a | (long)b;
                        else if(Operator == '#')
                            res = (long)a ^ (long)b;
                        else if(Operator == '%')
                            res = a % b;
                        else if(Operator == '>')
                            res = (long)a >> (int)b;
                        else if(Operator == '<')
                            res = (long)a << (int)b;
                        stack.Push(new StringBuilder().Append(res.ToString(provider)));
                    }
                }
            }
            return Convert.ToDecimal(stack.Pop().ToString(), provider);
        }

        private static bool IsNumber(string str) {
            try {
                Convert.ToDecimal(str, provider);
                return true;
            }
            catch {
                return false;
            }
        }

        private static int IsOperator(char str) {
            char[] Operator1 = { '&', '|', '#', '>', '<' };
            char[] Operator2 = { '+', '-', '%' };
            char[] Operator3 = { '*', '/' };
            char[] Operator4 = { '^' };
            char[] Operator5 = { '!', '~', 'I' };
            for(int i = 0; i < Operator5.Length; i++)
                if(str == Operator5[i])
                    return 5;
            for(int i = 0; i < Operator4.Length; i++)
                if(str == Operator4[i])
                    return 4;
            for(int i = 0; i < Operator3.Length; i++)
                if(str == Operator3[i])
                    return 3;
            for(int i = 0; i < Operator2.Length; i++)
                if(str == Operator2[i])
                    return 2;
            for(int i = 0; i < Operator1.Length; i++)
                if(str == Operator1[i])
                    return 1;
            return 0;
        }

        private static void CheckSyntax(string expression) {
            if(Regex.Match(expression, @"\d+\s*I").Value != "")
                throw new Exception("Invalid expression");
            else if(Regex.Match(expression, @"[^<]<[^<]").Value != "")
                throw new Exception("Invalid expression");
            else if(Regex.Match(expression, @"[^>]>[^>]").Value != "")
                throw new Exception("Invalid expression");
        }

        private static StringBuilder Normalization(StringBuilder expression) {
            expression = expression.Replace(" ", "");
            expression = expression.Replace("++", "+");
            expression = expression.Replace("--", "+");
            expression = expression.Replace("+-", "-");
            expression = expression.Replace("-+", "-");
            expression = expression.Replace("<<", "<");
            expression = expression.Replace(">>", ">");

            expression = Upper(expression, "and");
            expression = Upper(expression, "xor");
            expression = Upper(expression, "or");
            expression = expression.Replace("AND", "&");
            expression = expression.Replace("XOR", "#");
            expression = expression.Replace("OR", "|");

            expression = expression.Replace("{", "(");
            expression = expression.Replace("}", ")");
            expression = expression.Replace("[", "(");
            expression = expression.Replace("]", ")");

            expression = expression.Replace(")(", ")*(");

            expression = expression.Replace(".", ",");

            foreach(Match match in Regex.Matches(expression.ToString(), @"\d+\("))
                expression = expression.Replace(match.Value, match.Value.Replace("(", "*("));
            foreach(Match match in Regex.Matches(expression.ToString(), @"\)\d+"))
                expression = expression.Replace(match.Value, match.Value.Replace(")", ")*"));
            foreach(Match match in Regex.Matches(expression.ToString(), @"\d+PI"))
                expression = expression.Replace(match.Value, "(" + match.Value.Replace("PI", "*PI") + ")");
            foreach(Match match in Regex.Matches(expression.ToString(), @"PI\d+"))
                expression = expression.Replace(match.Value, "(" + match.Value.Replace("PI", "PI*") + ")");
            foreach(Match match in Regex.Matches(expression.ToString(), @"\d+!"))
                expression = expression.Replace(match.Value, match.Value.Replace("!", "I"));

            expression = expression.Replace("PI", Math.PI.ToString(provider));
            char lastChar = expression[expression.Length - 1];
            if(IsOperator(lastChar) > 0 && lastChar != 'I')
                expression = expression.Remove(expression.Length - 1, 1);
            return expression;
        }

        private static List<StringBuilder> SeparationOfExpressions(StringBuilder expression) {
            bool isNumberTurn = true;
            List<StringBuilder> ret = new List<StringBuilder>();
            StringBuilder temp = new StringBuilder();
            for(int i = 0; i < expression.Length; i++) {
                int optor = IsOperator(expression[i]);
                if(optor == 5 || expression[i] == '(' || expression[i] == ')' || !(isNumberTurn || optor == 0)) {
                    isNumberTurn = (expression[i] == 'I' || expression[i] == ')') ? false : true;
                    if(temp.Length > 0)
                        ret.Add(temp);
                    ret.Add(new StringBuilder(1).Append(expression[i]));
                    temp = new StringBuilder();
                }
                else {
                    isNumberTurn = false;
                    temp.Append(expression[i]);
                }
            }
            if(temp.Length > 0)
                ret.Add(temp);
            return ret;
        }

        private static bool IsHexNumber(StringBuilder str) {
            int index = 0;
            if(str[index] == '-' || str[index] == '+')
                index++;
            if(str[index] == '0' && (str[index + 1] == 'x' || str[index + 1] == 'X'))
                return true;
            return false;
        }

        private static bool IsCharacter(StringBuilder str) {
            int index = 0;
            int length = 3;
            if(str[index] == '-' || str[index] == '+') {
                index++;
                length++;
            }
            if(str.Length == length && str[index] == '\'' && str[index + 2] == '\'')
                return true;
            return false;
        }

        private static bool IsBinNumber(StringBuilder str) {
            int index = 0;
            if(str[index] == '-' || str[index] == '+')
                index++;
            if(str[index] == '0' && (str[index + 1] == 'b' || str[index + 1] == 'B'))
                return true;
            return false;
        }

        private static void ConvertAllToInt(List<StringBuilder> list) {
            for(int i = 0; i < list.Count; i++) {
                if(list[i].Length > 2) {
                    if(IsHexNumber(list[i]))
                        list[i] = new StringBuilder().Append(HexToInt(list[i].ToString()));
                    else if(IsCharacter(list[i])) {
                        if(list[i][0] != '\'')
                            list[i] = new StringBuilder().Append(-(int)list[i][2]);
                        else
                            list[i] = new StringBuilder().Append((int)list[i][1]);
                    }
                    else if(IsBinNumber(list[i]))
                        list[i] = new StringBuilder().Append(BinToInt(list[i].ToString()).ToString());
                }
                if(list[i].Length >= 3) {
                    char c = '0';
                    decimal value = 0;
                    string str = list[i].ToString();
                    Match match = Regex.Match(str, @"\A\d+(,\d+)?((KB)|(MB)|(GB)|(TB))\z");
                    if(match.Length != 0) {
                        int index = Regex.Match(str, @"\A\d+(,\d+)?").Length;
                        c = list[i][index];
                        str = str.Substring(0, index);
                        value = decimal.Parse(str, provider);

                        if(c == 'K')
                            value *= 1024;
                        else if(c == 'M')
                            value *= 1024 * 1024;
                        else if(c == 'G')
                            value *= 1024 * 1024 * 1024;
                        else if(c == 'T')
                            value *= (decimal)1024 * 1024 * 1024 * 1024;
                        list[i].Clear();
                        list[i].Append(value.ToString(provider));
                        continue;
                    }
                }
                if(list[i].Length >= 2) {
                    char c = '0';
                    decimal value = 0;
                    string str = list[i].ToString();
                    Match match = Regex.Match(str, @"\A\d+[KMT]\d+\z");
                    if(match.Value.Length != 0) {
                        int index = Regex.Match(str, @"\A\d+[KMT]").Length - 1;
                        c = list[i][index];
                        str = str.Replace(str[index], ',');
                        value = decimal.Parse(str, provider);
                    }
                    else if((match = Regex.Match(str, @"\A\d+(,\d+)?[KMT]\z")).Length != 0) {
                        value = decimal.Parse(str.Substring(0, str.Length - 1), provider);
                        c = list[i][list[i].Length - 1];
                    }
                    if(match.Length != 0) {
                        if(c == 'K')
                            value *= 1000;
                        else if(c == 'M')
                            value *= 1000000;
                        else if(c == 'T')
                            value *= 1000000000;
                        list[i].Clear();
                        list[i].Append(value.ToString(provider));
                        continue;
                    }
                }
            }
        }

        private static List<StringBuilder> Postfix(List<StringBuilder> list) {
            Stack<StringBuilder> P = new Stack<StringBuilder>();
            Stack<char> S = new Stack<char>();
            for(int i = 0; i < list.Count; i++) {
                if(list[i].Length == 1 && list[i][0] == ')') {
                    char temp = S.Pop();
                    if(temp != '(') {
                        do {
                            P.Push(new StringBuilder(1).Append(temp));
                            temp = S.Pop();
                        } while(temp != '(');
                    }
                }
                else if(list[i].Length == 1 && list[i][0] == '(')
                    S.Push(list[i][0]);
                else if(list[i].Length == 1 && !IsNumber(list[i].ToString())) {
                    if(S.Count > 0) {
                        int lastOperator = IsOperator(S.ElementAt(0));
                        int currOperator = IsOperator(list[i][0]);
                        while((lastOperator >= currOperator) && currOperator != 5) {
                            P.Push(new StringBuilder(1).Append(S.Pop()));
                            if(S.Count > 0)
                                lastOperator = IsOperator(S.ElementAt(0));
                            else
                                break;
                        }
                    }
                    S.Push(list[i][0]);
                }
                else
                    P.Push(list[i]);
            }
            while(S.Count > 0)
                P.Push(new StringBuilder(1).Append(S.Pop()));
            return P.ToList<StringBuilder>();
        }
    }
}
