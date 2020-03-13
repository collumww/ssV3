namespace ss {
    partial class ssForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
                }
            base.Dispose(disposing);
            }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.SuspendLayout();
            // 
            // ssForm
            // 
            this.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(649, 124);
            this.MinimumSize = new System.Drawing.Size(200, 100);
            this.Name = "ssForm";
            this.Text = "ssForm";
            this.Activated += new System.EventHandler(this.ssForm_Activated);
            this.Deactivate += new System.EventHandler(this.ssForm_Deactivate);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ssForm_FormClosing);
            this.Load += new System.EventHandler(this.ssForm_Load);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.ssForm_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.ssForm_DragEnter);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.ssForm_Paint);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ssForm_KeyDown);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ssForm_KeyPress);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.ssForm_KeyUp);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ssForm_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ssForm_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ssForm_MouseUp);
            this.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.ssForm_MouseWheel);
            this.Resize += new System.EventHandler(this.ssForm_Resize);
            this.ResumeLayout(false);

            }

        #endregion
        }
    }