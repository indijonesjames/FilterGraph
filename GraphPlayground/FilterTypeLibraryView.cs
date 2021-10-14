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
    public partial class FilterTypeLibraryView : UserControl
    {
        private FilterTypeLibrary filterTypeLibrary;
        public FilterTypeLibrary FilterTypeLibrary {
            get {
                return filterTypeLibrary;
            }
            set {
                filterTypeLibrary = value;
                ToForm();
            }
        }

        void ToForm()
        {
            this.listBox1.Items.Clear();
            if (filterTypeLibrary != null) {
                foreach (var filterType in filterTypeLibrary.filterTypes) {
                    this.listBox1.Items.Add(filterType);
                }
            }

        }
        public FilterTypeLibraryView()
        {
            InitializeComponent();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.toolStripButton1.Enabled = this.listBox1.SelectedItem != null;
        }

        public event Action<FilterType> AddFilterByType;

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            var filterType = this.listBox1.SelectedItem as FilterType;
            if (this.AddFilterByType != null && filterType != null) {
                this.AddFilterByType(filterType);
            }
        }
    }
}
