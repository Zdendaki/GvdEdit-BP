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
            ScrollPanel = new Panel();
            HourStart = new NumericUpDown();
            HourEnd = new NumericUpDown();
            ScaleX = new NumericUpDown();
            label1 = new Label();
            label2 = new Label();
            ScaleY = new NumericUpDown();
            flowLayoutPanel1 = new FlowLayoutPanel();
            ExportButton = new Button();
            MoveLeft = new Button();
            MoveRight = new Button();
            RefreshButton = new Button();
            ((System.ComponentModel.ISupportInitialize)Canvas).BeginInit();
            ScrollPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)HourStart).BeginInit();
            ((System.ComponentModel.ISupportInitialize)HourEnd).BeginInit();
            ((System.ComponentModel.ISupportInitialize)ScaleX).BeginInit();
            ((System.ComponentModel.ISupportInitialize)ScaleY).BeginInit();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // Canvas
            // 
            Canvas.Location = new Point(0, 0);
            Canvas.Name = "Canvas";
            Canvas.Size = new Size(100, 100);
            Canvas.TabIndex = 0;
            Canvas.TabStop = false;
            Canvas.Paint += Canvas_Paint;
            Canvas.MouseDown += Canvas_MouseDown;
            Canvas.MouseMove += Canvas_MouseMove;
            Canvas.MouseUp += Canvas_MouseUp;
            // 
            // ScrollPanel
            // 
            ScrollPanel.AutoScroll = true;
            ScrollPanel.Controls.Add(Canvas);
            ScrollPanel.Dock = DockStyle.Fill;
            ScrollPanel.Location = new Point(0, 39);
            ScrollPanel.Margin = new Padding(3, 39, 3, 3);
            ScrollPanel.Name = "ScrollPanel";
            ScrollPanel.Size = new Size(1182, 514);
            ScrollPanel.TabIndex = 3;
            // 
            // HourStart
            // 
            HourStart.Location = new Point(218, 5);
            HourStart.Margin = new Padding(0);
            HourStart.Maximum = new decimal(new int[] { 23, 0, 0, 0 });
            HourStart.Name = "HourStart";
            HourStart.Size = new Size(50, 27);
            HourStart.TabIndex = 4;
            HourStart.ValueChanged += ValueChanged;
            // 
            // HourEnd
            // 
            HourEnd.Location = new Point(273, 5);
            HourEnd.Margin = new Padding(5, 0, 0, 0);
            HourEnd.Maximum = new decimal(new int[] { 24, 0, 0, 0 });
            HourEnd.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            HourEnd.Name = "HourEnd";
            HourEnd.Size = new Size(50, 27);
            HourEnd.TabIndex = 5;
            HourEnd.Value = new decimal(new int[] { 12, 0, 0, 0 });
            HourEnd.ValueChanged += ValueChanged;
            // 
            // ScaleX
            // 
            ScaleX.Location = new Point(491, 5);
            ScaleX.Margin = new Padding(0);
            ScaleX.Maximum = new decimal(new int[] { 20, 0, 0, 0 });
            ScaleX.Minimum = new decimal(new int[] { 2, 0, 0, 0 });
            ScaleX.Name = "ScaleX";
            ScaleX.Size = new Size(50, 27);
            ScaleX.TabIndex = 6;
            ScaleX.Value = new decimal(new int[] { 6, 0, 0, 0 });
            ScaleX.ValueChanged += ValueChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = Color.Transparent;
            label1.Location = new Point(119, 8);
            label1.Margin = new Padding(10, 3, 0, 0);
            label1.Name = "label1";
            label1.Size = new Size(99, 20);
            label1.TabIndex = 7;
            label1.Text = "Čas (od - do):";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.BackColor = Color.Transparent;
            label2.Location = new Point(333, 8);
            label2.Margin = new Padding(10, 3, 0, 0);
            label2.Name = "label2";
            label2.Size = new Size(158, 20);
            label2.TabIndex = 8;
            label2.Text = "Měřítko (čas - stanice):";
            // 
            // ScaleY
            // 
            ScaleY.Location = new Point(551, 5);
            ScaleY.Margin = new Padding(10, 0, 0, 0);
            ScaleY.Name = "ScaleY";
            ScaleY.Size = new Size(50, 27);
            ScaleY.TabIndex = 9;
            ScaleY.Value = new decimal(new int[] { 10, 0, 0, 0 });
            ScaleY.ValueChanged += ValueChanged;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutPanel1.Controls.Add(ExportButton);
            flowLayoutPanel1.Controls.Add(label1);
            flowLayoutPanel1.Controls.Add(HourStart);
            flowLayoutPanel1.Controls.Add(HourEnd);
            flowLayoutPanel1.Controls.Add(label2);
            flowLayoutPanel1.Controls.Add(ScaleX);
            flowLayoutPanel1.Controls.Add(ScaleY);
            flowLayoutPanel1.Controls.Add(MoveLeft);
            flowLayoutPanel1.Controls.Add(MoveRight);
            flowLayoutPanel1.Controls.Add(RefreshButton);
            flowLayoutPanel1.Dock = DockStyle.Top;
            flowLayoutPanel1.Location = new Point(0, 0);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Padding = new Padding(5);
            flowLayoutPanel1.Size = new Size(1182, 39);
            flowLayoutPanel1.TabIndex = 10;
            // 
            // ExportButton
            // 
            ExportButton.Enabled = false;
            ExportButton.Location = new Point(10, 5);
            ExportButton.Margin = new Padding(5, 0, 5, 0);
            ExportButton.Name = "ExportButton";
            ExportButton.Size = new Size(94, 29);
            ExportButton.TabIndex = 0;
            ExportButton.Text = "Exportovat";
            ExportButton.UseVisualStyleBackColor = true;
            ExportButton.Click += ExportButton_Click;
            // 
            // MoveLeft
            // 
            MoveLeft.Location = new Point(626, 5);
            MoveLeft.Margin = new Padding(25, 0, 5, 0);
            MoveLeft.Name = "MoveLeft";
            MoveLeft.Size = new Size(130, 29);
            MoveLeft.TabIndex = 10;
            MoveLeft.Text = "Posunout vlevo";
            MoveLeft.UseVisualStyleBackColor = true;
            MoveLeft.Click += MoveLeft_Click;
            // 
            // MoveRight
            // 
            MoveRight.Location = new Point(766, 5);
            MoveRight.Margin = new Padding(5, 0, 5, 0);
            MoveRight.Name = "MoveRight";
            MoveRight.Size = new Size(130, 29);
            MoveRight.TabIndex = 11;
            MoveRight.Text = "Posunout vpravo";
            MoveRight.UseVisualStyleBackColor = true;
            MoveRight.Click += MoveRight_Click;
            // 
            // RefreshButton
            // 
            RefreshButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            RefreshButton.Location = new Point(926, 5);
            RefreshButton.Margin = new Padding(25, 0, 5, 0);
            RefreshButton.Name = "RefreshButton";
            RefreshButton.Size = new Size(150, 29);
            RefreshButton.TabIndex = 12;
            RefreshButton.Text = "AKTUALIZOVAT";
            RefreshButton.UseVisualStyleBackColor = true;
            RefreshButton.Click += RefreshButton_Click;
            // 
            // GvdForm
            // 
            AutoScaleDimensions = new SizeF(120F, 120F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.White;
            ClientSize = new Size(1182, 553);
            Controls.Add(ScrollPanel);
            Controls.Add(flowLayoutPanel1);
            Name = "GvdForm";
            Text = "Zobrazení listu GVD";
            FormClosing += GvdForm_FormClosing;
            ((System.ComponentModel.ISupportInitialize)Canvas).EndInit();
            ScrollPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)HourStart).EndInit();
            ((System.ComponentModel.ISupportInitialize)HourEnd).EndInit();
            ((System.ComponentModel.ISupportInitialize)ScaleX).EndInit();
            ((System.ComponentModel.ISupportInitialize)ScaleY).EndInit();
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox Canvas;
        private Panel ScrollPanel;
        private NumericUpDown HourStart;
        private NumericUpDown HourEnd;
        private NumericUpDown ScaleX;
        private Label label1;
        private Label label2;
        private NumericUpDown ScaleY;
        private FlowLayoutPanel flowLayoutPanel1;
        private Button ExportButton;
        private Button MoveLeft;
        private Button MoveRight;
        private Button RefreshButton;
    }
}