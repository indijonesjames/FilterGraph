using GraphLayoutLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VectorMath;

namespace GraphPlayground
{
    public partial class MainForm : Form
    {
        GraphDesign graphDesign;

        public MainForm()
        {
            InitializeComponent();
            UpdateTitle();

            this.graphDesign = new GraphDesign();

            //var node = new NodeDesign();
            //node.name = "foo is a short name";
            //node.size = new int2(192, 64);
            //PinDesign input = new PinDesign() { name = "input" };
            //PinDesign output = new PinDesign() { name = "output" };
            //node.inputPinDesigns.Add(input);
            //node.outputPinDesigns.Add(output);
            //this.graphDesign.nodeDesigns.Add(node);


            this.graphDesignView1.Changed += GraphDesignView1_Changed;
            this.graphDesignView1.GraphDesign = graphDesign;
            this.graphDesignView1.EditNodeDesign += GraphDesignView1_EditNodeDesign;
        }

        private void GraphDesignView1_EditNodeDesign(NodeDesign nodeDesign)
        {
            // Open the external editor for the node
            EditNodeDesignForm form = new EditNodeDesignForm();
            form.Source = nodeDesign.@params;
            DialogResult result = form.ShowDialog(this);
            //if (result == DialogResult.OK) {
            nodeDesign.@params = form.Source;
            graphDesignView1.Invalidate();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Home) {
                doCenter();
                return true;
            }
            if (keyData == Keys.F5) {
                doTest();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void GraphDesignView1_Changed()
        {
            dirty();
        }

        private FilterTypeLibrary filterTypeLibrary;
        public FilterTypeLibrary FilterTypeLibrary {
            get {
                return filterTypeLibrary;
            }
            set {
                filterTypeLibrary = value;
                this.filterTypeLibraryView1.FilterTypeLibrary = value;
            }
        }

        void dirty()
        {
            if (!this.isDocumentDirty) {
                this.isDocumentDirty = true;
                UpdateTitle();
            } else {
                this.isDocumentDirty = true;
            }
        }

        void UpdateTitle()
        {
            this.Text = $"{AppTitleText()} - {DocumentTitleText()}{DirtyFlagText()}";
        }

        string AppTitleText()
        {
            return "GraphPlayground";
        }

        string DocumentTitleText()
        {
            return currentFileName != null ? currentFileName : "[Unnamed]";
        }

        string DirtyFlagText()
        {
            return isDocumentDirty ? " *" : string.Empty;
        }

        private void filterTypeLibraryView1_AddFilterByType(FilterType filterType)
        {
            // Add a new filter of this type to the graph
            var node = new NodeDesign();
            node.name = filterType.Name;
            node.size = new int2(192, 64);
            foreach (var x in filterType.inputPinNames) {
                PinDesign input = new PinDesign() { name = x };
                node.inputPinDesigns.Add(input);
            }
            foreach (var x in filterType.outputPinNames) {
                PinDesign output = new PinDesign() { name = x };
                node.outputPinDesigns.Add(output);
            }
            this.graphDesign.nodeDesigns.Add(node);
            dirty();
            this.graphDesignView1.Invalidate();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            doFileNew();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            doFileOpen();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            doFileSave();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            doFileSaveAs();
        }

        bool confirmDiscard()
        {
            DialogResult result = MessageBox.Show("You have unsaved changes that will be lost.", "Discard Changes?", MessageBoxButtons.OKCancel);
            return result == DialogResult.OK;
        }

        private void doFileNew()
        {
            if (isDocumentDirty && !confirmDiscard()) return;
            this.graphDesign = new GraphDesign();
            this.graphDesignView1.GraphDesign = graphDesign;
            isDocumentDirty = false;
            currentFileName = null;
            UpdateTitle();
        }

        private void doFileOpen()
        {
            if (isDocumentDirty && !confirmDiscard()) return;
            OpenFileDialog dlg = new OpenFileDialog();
            DialogResult result = dlg.ShowDialog(this);
            if (result == DialogResult.OK) {
                loadFromFile(dlg.FileName);
                isDocumentDirty = false;
                currentFileName = dlg.FileName;
                UpdateTitle();
            }
        }

        bool isDocumentDirty = false;

        private void doFileSave()
        {
            if (currentFileName != null) {
                saveToFile(currentFileName);
            } else {
                doFileSaveAs();
            }
            isDocumentDirty = false;
            UpdateTitle();
        }

        private void doFileSaveAs()
        {
            SaveFileDialog dlg = new SaveFileDialog();
            DialogResult result = dlg.ShowDialog(this);
            if (result == DialogResult.OK) {
                currentFileName = dlg.FileName;
                saveToFile(currentFileName);
                isDocumentDirty = false;
                UpdateTitle();
            }
        }


        string currentFileName = null;

        void saveToFile(string filename)
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter json = new JsonTextWriter(sw)) {
                json.Formatting = Formatting.Indented;
                int nextUnusedId = 1;
                Dictionary<object, int> ids = new Dictionary<object, int>();
                json.WriteStartObject();
                json.WritePropertyName("nodes");
                json.WriteStartArray();
                foreach (var nodeDesign in graphDesign.nodeDesigns) {
                    // allocate an Id for each
                    json.WriteStartObject();

                    json.WritePropertyName("name");
                    json.WriteValue(nodeDesign.name);

                    json.WritePropertyName("position");
                    json.WriteStartObject();
                    json.WritePropertyName("x");
                    json.WriteValue(nodeDesign.position.x);
                    json.WritePropertyName("y");
                    json.WriteValue(nodeDesign.position.y);
                    json.WriteEndObject();

                    json.WritePropertyName("size");
                    json.WriteStartObject();
                    json.WritePropertyName("x");
                    json.WriteValue(nodeDesign.size.x);
                    json.WritePropertyName("y");
                    json.WriteValue(nodeDesign.size.y);
                    json.WriteEndObject();

                    json.WritePropertyName("params");
                    json.WriteValue(nodeDesign.@params);

                    json.WritePropertyName("inputs");
                    json.WriteStartArray();
                    foreach (var inputPinDesign in nodeDesign.inputPinDesigns) {
                        json.WriteStartObject();
                        int id = nextUnusedId++;
                        ids.Add(inputPinDesign, id);
                        json.WritePropertyName("id");
                        json.WriteValue(id);
                        json.WritePropertyName("name");
                        json.WriteValue(inputPinDesign.name);
                        json.WriteEndObject();
                    }
                    json.WriteEnd();


                    json.WritePropertyName("outputs");
                    json.WriteStartArray();
                    foreach (var outputPinDesign in nodeDesign.outputPinDesigns) {
                        json.WriteStartObject();
                        int id = nextUnusedId++;
                        ids.Add(outputPinDesign, id);
                        json.WritePropertyName("id");
                        json.WriteValue(id);
                        json.WritePropertyName("name");
                        json.WriteValue(outputPinDesign.name);
                        json.WriteEndObject();
                    }
                    json.WriteEnd();

                    json.WriteEndObject();
                }
                json.WriteEnd();
                json.WritePropertyName("connections");
                json.WriteStartArray();
                foreach (var pinConnectionDesign in graphDesign.pinConnectionDesigns) {
                    if (ids.TryGetValue(pinConnectionDesign.inputPinDesign, out int inputPinId)) {
                        if (ids.TryGetValue(pinConnectionDesign.outputPinDesign, out int outputPinId)) {
                            json.WriteStartObject();
                            json.WritePropertyName("upstream");
                            json.WriteValue(outputPinId);
                            json.WritePropertyName("downstream");
                            json.WriteValue(inputPinId);
                            json.WriteEndObject();
                        }
                    }
                }
                json.WriteEnd();
                json.WriteEndObject();

            }

            File.WriteAllText(filename, sb.ToString());
        }

        enum Context
        {
            None,
            Root,
            NodesProperty
        }

        void loadFromFile(string filename)
        {
            string text = File.ReadAllText(filename);
            JObject json = JObject.Parse(text);
            Dictionary<int, object> objs = new Dictionary<int, object>();
            graphDesign = new GraphDesign();
            foreach (var jNode in json["nodes"]) {
                Debug.WriteLine("found node");
                var node = new NodeDesign();
                node.name = (string)jNode["name"];
                node.position = new int2(
                    (int)jNode["position"]["x"],
                    (int)jNode["position"]["y"]);
                node.size = new int2(
                    (int)jNode["size"]["x"],
                    (int)jNode["size"]["y"]);
                node.@params = (string)jNode["params"];

                foreach (var jInput in jNode["inputs"]) {
                    var input = new PinDesign();
                    objs.Add((int)jInput["id"], input);
                    input.name = (string)jInput["name"];
                    node.inputPinDesigns.Add(input);
                }
                foreach (var jOutput in jNode["outputs"]) {
                    var output = new PinDesign();
                    objs.Add((int)jOutput["id"], output);
                    output.name = (string)jOutput["name"];
                    node.outputPinDesigns.Add(output);
                }
                graphDesign.nodeDesigns.Add(node);
            }
            foreach (var jConnection in json["connections"]) {
                Debug.WriteLine("found connection");
                int upstreamId = (int)jConnection["upstream"];
                int downstreamId = (int)jConnection["downstream"];
                var connection = new PinConnectionDesign();
                if (objs.TryGetValue(upstreamId, out object upstreamObj)) {
                    if (objs.TryGetValue(downstreamId, out object downstreamObj)) {
                        connection.outputPinDesign = upstreamObj as PinDesign;
                        connection.inputPinDesign = downstreamObj as PinDesign;
                    }
                }
                graphDesign.pinConnectionDesigns.Add(connection);
            }
            this.graphDesignView1.GraphDesign = graphDesign;
        }

        void doCenter()
        {
            this.graphDesignView1.Center();
        }

        private void centerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            doCenter();
        }

        async void doTest()
        {
            ManualJsonRpc rpc = new ManualJsonRpc();
            SetStatus(string.Empty);
            SetResult(string.Empty);
            await rpc.Connect("localhost", 8337);
            rpc.Notify += Rpc_Notify;

            try {

                string result = await GraphUtils.Test(rpc, this.graphDesign);

                SetStatus(result);
            } catch (Exception ex) {
                SetStatus(ex.Message);
            }

            rpc.Close();
        }

        private void Rpc_Notify(string id, string value)
        {
            SetResult(value);
        }

        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {
            doTest();
        }

        private void SetStatus(string str)
        {
            if (this.InvokeRequired) {
                this.Invoke(new MethodInvoker(() => SetStatus(str)));
                return;
            }
            this.toolStripStatusLabel1.Text = str;
        }

        private void SetResult(string text)
        {
            if (this.InvokeRequired) {
                this.Invoke(new MethodInvoker(() => SetResult(text)));
                return;
            }
            this.toolStripStatusLabel2.Text = $"{text}";
        }

    }
}
