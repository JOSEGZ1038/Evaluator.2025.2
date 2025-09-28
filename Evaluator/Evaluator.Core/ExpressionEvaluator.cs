namespace Evaluator.Core{

    using System;
    using System.Collections.Generic;
    using System.Globalization;

    public static class ExpressionEvaluator
    {
        public static double Evaluate(string expression) => Evaluate(expression, CultureInfo.InvariantCulture);


        public static double Evaluate(string expression, CultureInfo culture)
        {
            if (string.IsNullOrWhiteSpace(expression))
                throw new ArgumentException("Expression is empty.", nameof(expression));

            var decimalSep = culture.NumberFormat.NumberDecimalSeparator;
            if (decimalSep != ".")
                expression = expression.Replace(decimalSep, ".");

            var tokens = Tokenize(expression);
            var rpn = ToRPN(tokens);
            return EvalRPN(rpn, culture);
        }

        private static List<string> Tokenize(string expr)
        {
            var tokens = new List<string>();
            int i = 0;
            while (i < expr.Length)
            {
                char c = expr[i];

                if (char.IsWhiteSpace(c))
                {
                    i++; continue;
                }

                if (char.IsDigit(c) || c == '.')
                {
                    int start = i;
                    bool dotSeen = (c == '.');
                    i++;
                    while (i < expr.Length && (char.IsDigit(expr[i]) || (expr[i] == '.' && !dotSeen)))
                    {
                        if (expr[i] == '.') dotSeen = true;
                        i++;
                    }
                    tokens.Add(expr.Substring(start, i - start));
                    continue;
                }

                if ("+-*/^()".IndexOf(c) >= 0)
                {

                    if (c == '-')
                    {
                        bool isUnary = false;
                        if (tokens.Count == 0) isUnary = true;
                        else
                        {
                            var prev = tokens[tokens.Count - 1];
                            if (IsOperator(prev) || prev == "(") isUnary = true;
                        }

                        if (isUnary)
                        {

                            tokens.Add("u-");
                            i++;
                            continue;
                        }
                    }

                    tokens.Add(c.ToString());
                    i++;
                    continue;
                }

                throw new Exception($"Unexpected character at position {i}: '{c}'");
            }

            return tokens;
        }

        private static readonly Dictionary<string, (int prec, bool right)> operators = new Dictionary<string, (int, bool)>()
    {
        { "+", (2, false) },
        { "-", (2, false) },
        { "*", (3, false) },
        { "/", (3, false) },
        { "^", (4, true) },
        { "u-", (5, true) }
    };

        private static bool IsOperator(string token) => operators.ContainsKey(token);

        private static Queue<string> ToRPN(List<string> tokens)
        {
            var output = new Queue<string>();
            var ops = new Stack<string>();

            foreach (var token in tokens)
            {
                if (double.TryParse(token, NumberStyles.Number, CultureInfo.InvariantCulture, out _))
                {
                    output.Enqueue(token);
                }
                else if (IsOperator(token))
                {
                    while (ops.Count > 0 && IsOperator(ops.Peek()))
                    {
                        var top = ops.Peek();
                        var cur = operators[token];
                        var topinfo = operators[top];

                        if ((cur.right == false && cur.prec <= topinfo.prec) ||
                            (cur.right == true && cur.prec < topinfo.prec))
                        {
                            output.Enqueue(ops.Pop());
                        }
                        else break;
                    }
                    ops.Push(token);
                }
                else if (token == "(")
                {
                    ops.Push(token);
                }
                else if (token == ")")
                {
                    while (ops.Count > 0 && ops.Peek() != "(")
                        output.Enqueue(ops.Pop());

                    if (ops.Count == 0 || ops.Peek() != "(")
                        throw new Exception("Mismatched parentheses.");
                    ops.Pop();
                }
                else
                {
                    throw new Exception($"Unknown token '{token}'");
                }
            }

            while (ops.Count > 0)
            {
                var t = ops.Pop();
                if (t == "(" || t == ")")
                    throw new Exception("Mismatched parentheses.");
                output.Enqueue(t);
            }

            return output;
        }

        private static double EvalRPN(Queue<string> rpn, CultureInfo culture)
        {
            var st = new Stack<double>();

            while (rpn.Count > 0)
            {
                var token = rpn.Dequeue();
                if (double.TryParse(token, NumberStyles.Number, CultureInfo.InvariantCulture, out double val))
                {
                    st.Push(val);
                }
                else if (token == "u-")
                {
                    if (st.Count < 1) throw new Exception("Invalid expression: unary minus has no operand.");
                    var a = st.Pop();
                    st.Push(-a);
                }
                else if (IsOperator(token))
                {
                    if (st.Count < 2) throw new Exception("Invalid expression: operator needs two operands.");
                    var b = st.Pop();
                    var a = st.Pop();
                    switch (token)
                    {
                        case "+": st.Push(a + b); break;
                        case "-": st.Push(a - b); break;
                        case "*": st.Push(a * b); break;
                        case "/":
                            if (b == 0) throw new DivideByZeroException("Division by zero.");
                            st.Push(a / b);
                            break;
                        case "^":
                            st.Push(Math.Pow(a, b));
                            break;
                        default:
                            throw new Exception($"Unsupported operator '{token}'");
                    }
                }
                else
                {
                    throw new Exception($"Unexpected RPN token '{token}'");
                }
            }

            if (st.Count != 1) throw new Exception("Invalid expression evaluation.");
            return st.Pop();
        }
    }
}