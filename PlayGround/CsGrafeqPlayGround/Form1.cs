using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CsGrafeq;
using System.IO;
namespace CsGrafeqPlayGround
{
    public partial class Form1 : Form
    {
        ImplicitFunctionDisplayer fd=new ImplicitFunctionDisplayer();
        private (double min, double max)[] varScrollRange = new (double min, double max)[26];
        private string[] expressions;
        private string allexpressions;
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
            expressions= new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("CsGrafeqPlayGround.expressions.txt")).ReadToEnd().Replace("\r\n","\n").Split('\n');
            foreach (string expression in expressions)
            {
                allexpressions += expression.Split(';')[0] + "\r\n";
            }
            fd.Size = new System.Drawing.Size(600,600);
            fd.Zero=new PointL(300,300);
            ClientSize = new System.Drawing.Size(600, 600);
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
        private int index = 0;
        private string ss;
        public void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    if (textBox.Text != "")
                    {
                        ImplicitFunction imf=fd.Functions.Add(textBox.Text);
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
                case Keys.Q:
                    if (e.Control)
                    {
                        e.Handled = true;
                        Random rnd = new Random();
                        string[] s = expressions[rnd.Next(expressions.Length)].Split(';');
                        //string[] s = expressions[index++].Split(';');
                        ss=textBox.Text = s[0];
                        if (s.Length == 1)
                            checkBox1.Checked = false;
                        else
                            checkBox1.Checked = s[1] != "no";
                    }
                    break;
                case Keys.S:
                    if (e.Control && e.Shift)
                    {
                    }else if (e.Control)
                    {
                        SaveFileDialog saveFileDialog = new SaveFileDialog();
                        saveFileDialog.Filter = "*.jpg|Jpg File";
                        saveFileDialog.DefaultExt = ".jpg";
                        saveFileDialog.Title = "保存";
                        saveFileDialog.AddExtension = true;
                        saveFileDialog.FileName = "1.jpg";
                        if (saveFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            Bitmap bitmap = new Bitmap(fd.Width, fd.Height);
                            Graphics g = Graphics.FromImage(bitmap);
                            fd.RenderTo(g, fd.ClientRectangle);
                            /*string sss = "";
                            int l = 600 / new Font("Consolas", 14).Height * 2;
                            for (int i = 0; i < ss.Length; i+=l)
                            {
                                sss += SubString(ss, i, i +l) +"\r\n";
                            }
                            g.DrawString(sss, new Font("Consolas", 14), new SolidBrush(Color.Black), new Point(4,4));*/
                            //bitmap.Save();
                            try
                            {
                                bitmap.Save(saveFileDialog.FileName);
                                MessageBox.Show("保存成功");
                            }catch (Exception ex){
                                MessageBox.Show("保存失败"+ex.Message);
                            }
                        }
                    }
                        break;

            }
        }
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            Console.WriteLine(ClientSize);
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
            if (fd.Functions.Count==0)
                return;
            fd.Functions.RemoveAt(fd.Functions.Count-1);
            fd.Render();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            Form f=new Form();
            f.ShowIcon = false;
            f.ShowInTaskbar = false;
            f.FormBorderStyle = FormBorderStyle.Sizable;
            TextBox textBox = new TextBox();
            textBox.ReadOnly = true;
            textBox.Text = allexpressions;
            textBox.Dock = DockStyle.Fill;
            textBox.Multiline = true;
            textBox.ScrollBars = ScrollBars.Both;
            textBox.Font = new Font("Microsoft Yahei",13);
            textBox.WordWrap = false;
            textBox.SelectionStart = 0;
            textBox.SelectionLength = 0;
            f.Controls.Add(textBox);
            f.ShowDialog();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            fd.ShowAxis=checkBox2.Checked;
            fd.Render();
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            fd.ShowNumber=checkBox4.Checked;
            fd.Render();
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            fd.CanZoom = checkBox3.Checked;
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            fd.CanMove = checkBox3.Checked;
        }
        private string SubString(string s,int start,int end)
        {
            return s.Substring(Math.Min(start,s.Length),Math.Min(end,s.Length)- Math.Min(start, s.Length));
        }
    }
}
