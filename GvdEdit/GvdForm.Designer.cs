using System.Drawing;
using System.Windows.Forms;

namespace GvdEdit
{
    partial class GvdForm
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
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            Canvas = new PictureBox();
            toolStrip1 = new ToolStrip();
            panel1 = new Panel();
            toolStripLabel1 = new ToolStripLabel();
            ((System.ComponentModel.ISupportInitialize)Canvas).BeginInit();
            toolStrip1.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // Canvas
            // 
            Canvas.Location = new Point(0, 0);
            Canvas.Name = "Canvas";
            Canvas.Size = new Size(8000, 2400);
            Canvas.TabIndex = 0;
            Canvas.TabStop = false;
            Canvas.Click += Canvas_Click;
            Canvas.Paint += Canvas_Paint;
            // 
            // toolStrip1
            // 
            toolStrip1.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip1.ImageScalingSize = new Size(20, 20);
            toolStrip1.Items.AddRange(new ToolStripItem[] { toolStripLabel1 });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(1182, 43);
            toolStrip1.TabIndex = 2;
            toolStrip1.Text = "toolStrip1";
            // 
            // panel1
            // 
            panel1.AutoScroll = true;
            panel1.Controls.Add(Canvas);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(0, 43);
            panel1.Name = "panel1";
            panel1.Size = new Size(1182, 510);
            panel1.TabIndex = 3;
            // 
            // toolStripLabel1
            // 
            toolStripLabel1.Name = "toolStripLabel1";
            toolStripLabel1.Padding = new Padding(20, 10, 20, 10);
            toolStripLabel1.Size = new Size(151, 40);
            toolStripLabel1.Text = "toolStripLabel1";
            // 
            // GvdForm
            // 
            AutoScaleDimensions = new SizeF(120F, 120F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.White;
            ClientSize = new Size(1182, 553);
            Controls.Add(panel1);
            Controls.Add(toolStrip1);
            Name = "GvdForm";
            Text = "Zobrazení listu GVD";
            FormClosing += GvdForm_FormClosing;
            ((System.ComponentModel.ISupportInitialize)Canvas).EndInit();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            panel1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox Canvas;
        private ToolStrip toolStrip1;
        private Panel panel1;
        private ToolStripLabel toolStripLabel1;
    }
}