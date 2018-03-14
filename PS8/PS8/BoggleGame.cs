using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PS8
{
    public partial class BoggleGame : Form
    {
        public BoggleGame()
        {
            InitializeComponent();
            Control c = tableLayoutPanel1.GetControlFromPosition(1, 1);
        }
    }
}
