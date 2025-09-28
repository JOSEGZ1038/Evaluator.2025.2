using Evaluator.Core;
using System.Globalization;
namespace Evaluator.UI.Windows


{
    public partial class Form1 : Form
    {
        private TextBox display = null!;

        public Form1()
        {
            InitializeComponent();
            BuildCalculatorUI();
        }

        private void BuildCalculatorUI()
        {
            this.Text = "Evaluator - Calculadora";
            this.MinimumSize = new Size(360, 480);

            var tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 6,
                Padding = new Padding(6)
            };

            for (int i = 0; i < 4; i++) tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 80F));
            for (int r = 1; r < 6; r++) tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));

            display = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 22F, FontStyle.Regular),
                TextAlign = HorizontalAlignment.Right,
                ReadOnly = true
            };
            display.Margin = new Padding(4);
            tlp.Controls.Add(display, 0, 0);
            tlp.SetColumnSpan(display, 4);

            string[][] rows = new string[][]
            {
                new[] {"7","8","9","(",},
                new[] {"4","5","6",")"},
                new[] {"1","2","3","Delete"},
                new[] {"0",".","^","Clear"},
                new[] {"+","-","*","/"},
                new[] {"=","","",""}
            };

            for (int r = 0; r < rows.Length; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    string text = rows[r][c].Trim();
                    if (string.IsNullOrEmpty(text)) continue;
                    var btn = new Button
                    {
                        Text = text,
                        Dock = DockStyle.Fill,
                        Font = new Font("Segoe UI", 14F, FontStyle.Regular),
                        Margin = new Padding(4)
                    };
                    btn.Click += Btn_Click;

                    if (text == "=" && r == 5 && c == 0)
                    {
                        tlp.Controls.Add(btn, 0, r + 1);
                        tlp.SetColumnSpan(btn, 4);
                        break;
                    }
                    tlp.Controls.Add(btn, c, r + 1);
                }
            }

            this.Controls.Add(tlp);
        }

        private void Btn_Click(object? sender, EventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;
            var t = btn.Text;

            if (t == "Clear")
            {
                display.Text = "";
                return;
            }

            if (t == "Delete")
            {
                if (display.Text.Length > 0)
                    display.Text = display.Text.Substring(0, display.Text.Length - 1);
                return;
            }

            if (t == "=")
            {
                try
                {
                    string expr = display.Text;
                    double result = ExpressionEvaluator.Evaluate(expr);
                    display.Text = expr + "=" + result.ToString(CultureInfo.InvariantCulture);
                    display.SelectAll();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Evaluador", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return;
            }

            if (display.Text.Contains("="))
            {
                display.Text = "";
            }
            display.Text += t;
        }
    }
}