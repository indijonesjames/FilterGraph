using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GraphPlayground
{
    public partial class EditNodeDesignForm : Form
    {
        public string Source {
            get {
                return this.richTextBox1.Text;
            }
            set {
                this.richTextBox1.Text = value;
            }
        }

        public EditNodeDesignForm()
        {
            InitializeComponent();
        }
    }
}
