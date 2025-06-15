using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using CsGrafeq.Base;

namespace CsGrafeq
{
    public class Program
    {
        public static void Main()
        {
            Application.EnableVisualStyles();
            Form form = new Form();
            form.Size=new Size(900,650);
            GraphicPanel db=new GraphicPanel();
            db.Dock = DockStyle.Fill;
            form.Controls.Add(db);
            Application.Run(form);
        }
    }
}
