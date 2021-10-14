
namespace GraphLayoutLib
{
    partial class GraphDesignView
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // GraphLayoutView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.DoubleBuffered = true;
            this.Name = "GraphLayoutView";
            this.Size = new System.Drawing.Size(336, 246);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.GraphDesignView_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.GraphDesignView_DragEnter);
            this.DragOver += new System.Windows.Forms.DragEventHandler(this.GraphDesignView_DragOver);
            this.DragLeave += new System.EventHandler(this.GraphDesignView_DragLeave);
            this.GiveFeedback += new System.Windows.Forms.GiveFeedbackEventHandler(this.GraphDesignView_GiveFeedback);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.GraphDesignView_Paint);
            this.QueryContinueDrag += new System.Windows.Forms.QueryContinueDragEventHandler(this.GraphDesignView_QueryContinueDrag);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GraphDesignView_KeyDown);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.GraphDesignView_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.GraphDesignView_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.GraphDesignView_MouseUp);
            this.Resize += new System.EventHandler(this.GraphDesignView_Resize);
            this.ResumeLayout(false);

        }

        #endregion
    }
}
