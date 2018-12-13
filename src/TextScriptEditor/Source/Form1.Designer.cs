// -----------------------------------------------------------------------
// <copyright file="Form1.Designer.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace TextScriptEditor
{
    partial class Form1
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.inputBox = new System.Windows.Forms.TextBox();
            this.outputView = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.inputBox);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.outputView);
            this.splitContainer1.Size = new System.Drawing.Size(1202, 737);
            this.splitContainer1.SplitterDistance = 565;
            this.splitContainer1.TabIndex = 0;
            // 
            // inputBox
            // 
            this.inputBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputBox.Font = new System.Drawing.Font("Consolas", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.inputBox.Location = new System.Drawing.Point(0, 0);
            this.inputBox.Multiline = true;
            this.inputBox.Name = "inputBox";
            this.inputBox.Size = new System.Drawing.Size(565, 737);
            this.inputBox.TabIndex = 0;
            this.inputBox.Text = "Type something here...";
            this.inputBox.TextChanged += new System.EventHandler(this.inputBox_TextChanged);
            // 
            // outputView
            // 
            this.outputView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.outputView.Font = new System.Drawing.Font("Consolas", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.outputView.Location = new System.Drawing.Point(0, 0);
            this.outputView.Multiline = true;
            this.outputView.Name = "outputView";
            this.outputView.ReadOnly = true;
            this.outputView.Size = new System.Drawing.Size(633, 737);
            this.outputView.TabIndex = 1;
            this.outputView.Text = "Type something here...";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1202, 737);
            this.Controls.Add(this.splitContainer1);
            this.Name = "Form1";
            this.Text = "TextScript Editor";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TextBox inputBox;
        private System.Windows.Forms.TextBox outputView;
    }
}

