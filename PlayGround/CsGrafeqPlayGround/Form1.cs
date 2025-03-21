using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CsGrafeq;
namespace CsGrafeqPlayGround
{
    public partial class Form1 : Form
    {
        FunctionDisplayer fd=new FunctionDisplayer();
        private (double min, double max)[] varScrollRange = new (double min, double max)[26];
        
        private List<string> history = new List<string>();
        public Form1()
        {
            InitializeComponent();
            for (int i = 0; i < 26; i++)
            {
                varScrollRange[i] = (-10, 10);
            }
            panel2.Location = panel1.Location;
            panel2.Visible = true;
            panel1.Visible = true;
            InputLanguage.CurrentInputLanguage = InputLanguage.DefaultInputLanguage;
            Controls.Add(fd);
            fd.Dock = DockStyle.Fill;
            fd.SendToBack();
        }
        public void button2_Click(object sender, EventArgs e)
        {
            Button tb = (Button)sender;
            if (panel1.Visible)
            {
                tb.Text = "常量";
            }
            else
            {
                tb.Text = "函数";
            }
            panel1.Visible = !panel1.Visible;
        }
        private int historyindex = 0;
        private string latesttext = "";
        public void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    if (textBox.Text != "")
                    {
                        ImplicitFunction imf=fd.ImpFuncs.Add(textBox.Text);
                        imf.CheckPixelMode = checkBox1.Checked ? CheckPixelMode.UseMarchingSquares:CheckPixelMode.None;
                        history.Add(textBox.Text);
                        historyindex = history.Count;
                        textBox1.Text = "";
                        latesttext = "";
                        fd.Render();
                    }
                    break;
                case Keys.Up:
                    if (historyindex == history.Count)
                    {
                        latesttext = textBox1.Text;
                    }
                    historyindex--;
                    if (historyindex < 0)
                    {
                        historyindex = 0;
                    }
                    else
                    {
                        textBox1.Text = history[historyindex];
                    }
                    textBox1.SelectionStart = textBox1.TextLength;
                    e.Handled = true;
                    break;
                case Keys.Down:
                    if (historyindex + 1 == history.Count)
                    {
                        textBox1.Text = latesttext;
                        historyindex++;
                    }
                    else
                    {
                        historyindex++;
                        if (historyindex > history.Count)
                        {
                            historyindex = history.Count;
                        }
                        else
                        {
                            textBox1.Text = history[historyindex];
                        }
                    }
                    textBox1.SelectionStart = textBox1.TextLength;
                    e.Handled = true;
                    break;

            }
        }
        private void label3_Click(object sender, EventArgs e)
        {
            string[] s;
            string ss = Microsoft.VisualBasic.Interaction.InputBox("");
            s = ss.Split(' ');
            if (!(s.Length != 2 || !Double.TryParse(s[0], out double n1) || !Double.TryParse(s[1], out double n2)))
            {
                double min = Double.Parse(s[0]);
                double max = Double.Parse(s[1]);
                varScrollRange[comboBox1.SelectedIndex] = (min, max);
                TrackBarNumberTo(min, max, double.NaN);
            }
        }
        private int sb = -1;
        private void TrackBarNumberTo(double min, double max, double v)
        {
            double value;
            if (Double.IsNaN(v))
            {
                value = trackBar1.Value * Math.Pow(10, sb);
            }
            else
            {
                value = v;
            }
            sb = max == min ? -3 : (int)Math.Log10(max - min) - 3;
            trackBar1.Minimum = (int)(min / Math.Pow(10, sb));
            trackBar1.Maximum = (int)(max / Math.Pow(10, sb));
            trackBar1.Value = (int)(range(min, max, value) / Math.Pow(10, sb));
            label3.Text = range(min, max, value).ToString();
        }
        public static double range(double min, double max, double num)
        {
            if (min > num) { return min; }
            if (max < num) { return max; }
            return num;
        }
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (
                 (e.KeyChar == '.') || (e.KeyChar == '\b') ||
                ('0' <= e.KeyChar && e.KeyChar <= '9') ||
                ('a' <= e.KeyChar && e.KeyChar <= 'z') ||
                (e.KeyChar == '%') || (e.KeyChar == '^') || (e.KeyChar == '*') ||
                 (e.KeyChar == '+') || (e.KeyChar == '(') || (e.KeyChar == ')') ||
                (e.KeyChar == '-') || (e.KeyChar == '/') || (e.KeyChar == ',') ||
                (e.KeyChar == '=') || (e.KeyChar == '<') || (e.KeyChar == '>') ||
                (e.KeyChar == '{') || (e.KeyChar == '}') ||
                (e.KeyChar == 1) ||
                (e.KeyChar == 3) ||
                (e.KeyChar == 22) ||
                (e.KeyChar == 24)
            )
            {
            }
            else if (('A' <= e.KeyChar && e.KeyChar <= 'Z'))
            {
                e.KeyChar = (char)(e.KeyChar - 'A' + 'a');
            }
            else
            {
                e.Handled = true;
            }
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            trackBar1.Enabled = true;
            label3.Enabled = true;
            TrackBarNumberTo(varScrollRange[((string)comboBox1.Items[comboBox1.SelectedIndex])[0] - 'a'].min, varScrollRange[((string)comboBox1.Items[comboBox1.SelectedIndex])[0] - 'a'].max, fd.GetConstantValue(((string)comboBox1.Items[comboBox1.SelectedIndex])[0] - 'a'));
            label3.Text = fd.GetConstantValue(((string)comboBox1.Items[comboBox1.SelectedIndex])[0] - 'a').ToString();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            TrackBar bar = (TrackBar)sender;
            string varname = (string)comboBox1.Items[comboBox1.SelectedIndex];
            double variable = ((double)bar.Value) * Math.Pow(10, sb);
            label3.Text = (variable).ToString();
            fd.SetConstantValue(varname[0] - 'a' , variable);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (fd.ImpFuncs.Count==0)
                return;
            fd.ImpFuncs.RemoveAt(fd.ImpFuncs.Count-1);
            fd.Render();
        }
    }
}
