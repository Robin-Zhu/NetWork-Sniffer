namespace MySniffer
{
    partial class FilterForm
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
            this.ProtocalGroupBox = new System.Windows.Forms.GroupBox();
            this.IgmpCheckBox = new System.Windows.Forms.CheckBox();
            this.IcmpCheckBox = new System.Windows.Forms.CheckBox();
            this.UdpCheckBox = new System.Windows.Forms.CheckBox();
            this.TcpCheckBox = new System.Windows.Forms.CheckBox();
            this.ArpCheckBox = new System.Windows.Forms.CheckBox();
            this.SrcGroupBox = new System.Windows.Forms.GroupBox();
            this.SrcIpTextBox = new System.Windows.Forms.TextBox();
            this.SrcIpLabel = new System.Windows.Forms.Label();
            this.DstGroupBox = new System.Windows.Forms.GroupBox();
            this.DstIpTextBox = new System.Windows.Forms.TextBox();
            this.DstIpLabel = new System.Windows.Forms.Label();
            this.ConfirmButton = new System.Windows.Forms.Button();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.CancelButton = new System.Windows.Forms.Button();
            this.ProtocalGroupBox.SuspendLayout();
            this.SrcGroupBox.SuspendLayout();
            this.DstGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // ProtocalGroupBox
            // 
            this.ProtocalGroupBox.Controls.Add(this.IgmpCheckBox);
            this.ProtocalGroupBox.Controls.Add(this.IcmpCheckBox);
            this.ProtocalGroupBox.Controls.Add(this.UdpCheckBox);
            this.ProtocalGroupBox.Controls.Add(this.TcpCheckBox);
            this.ProtocalGroupBox.Controls.Add(this.ArpCheckBox);
            this.ProtocalGroupBox.Location = new System.Drawing.Point(23, 12);
            this.ProtocalGroupBox.Name = "ProtocalGroupBox";
            this.ProtocalGroupBox.Size = new System.Drawing.Size(293, 70);
            this.ProtocalGroupBox.TabIndex = 0;
            this.ProtocalGroupBox.TabStop = false;
            this.ProtocalGroupBox.Text = "协议";
            // 
            // IgmpCheckBox
            // 
            this.IgmpCheckBox.AutoSize = true;
            this.IgmpCheckBox.Checked = true;
            this.IgmpCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.IgmpCheckBox.Location = new System.Drawing.Point(219, 32);
            this.IgmpCheckBox.Name = "IgmpCheckBox";
            this.IgmpCheckBox.Size = new System.Drawing.Size(48, 16);
            this.IgmpCheckBox.TabIndex = 4;
            this.IgmpCheckBox.Text = "IGMP";
            this.IgmpCheckBox.UseVisualStyleBackColor = true;
            // 
            // IcmpCheckBox
            // 
            this.IcmpCheckBox.AutoSize = true;
            this.IcmpCheckBox.Checked = true;
            this.IcmpCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.IcmpCheckBox.Location = new System.Drawing.Point(165, 32);
            this.IcmpCheckBox.Name = "IcmpCheckBox";
            this.IcmpCheckBox.Size = new System.Drawing.Size(48, 16);
            this.IcmpCheckBox.TabIndex = 3;
            this.IcmpCheckBox.Text = "ICMP";
            this.IcmpCheckBox.UseVisualStyleBackColor = true;
            // 
            // UdpCheckBox
            // 
            this.UdpCheckBox.AutoSize = true;
            this.UdpCheckBox.Checked = true;
            this.UdpCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.UdpCheckBox.Location = new System.Drawing.Point(117, 32);
            this.UdpCheckBox.Name = "UdpCheckBox";
            this.UdpCheckBox.Size = new System.Drawing.Size(42, 16);
            this.UdpCheckBox.TabIndex = 2;
            this.UdpCheckBox.Text = "UDP";
            this.UdpCheckBox.UseVisualStyleBackColor = true;
            // 
            // TcpCheckBox
            // 
            this.TcpCheckBox.AutoSize = true;
            this.TcpCheckBox.Checked = true;
            this.TcpCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.TcpCheckBox.Location = new System.Drawing.Point(69, 32);
            this.TcpCheckBox.Name = "TcpCheckBox";
            this.TcpCheckBox.Size = new System.Drawing.Size(42, 16);
            this.TcpCheckBox.TabIndex = 1;
            this.TcpCheckBox.Text = "TCP";
            this.TcpCheckBox.UseVisualStyleBackColor = true;
            // 
            // ArpCheckBox
            // 
            this.ArpCheckBox.AutoSize = true;
            this.ArpCheckBox.Checked = true;
            this.ArpCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ArpCheckBox.Location = new System.Drawing.Point(21, 32);
            this.ArpCheckBox.Name = "ArpCheckBox";
            this.ArpCheckBox.Size = new System.Drawing.Size(42, 16);
            this.ArpCheckBox.TabIndex = 0;
            this.ArpCheckBox.Text = "ARP";
            this.ArpCheckBox.UseVisualStyleBackColor = true;
            // 
            // SrcGroupBox
            // 
            this.SrcGroupBox.Controls.Add(this.SrcIpTextBox);
            this.SrcGroupBox.Controls.Add(this.SrcIpLabel);
            this.SrcGroupBox.Location = new System.Drawing.Point(23, 101);
            this.SrcGroupBox.Name = "SrcGroupBox";
            this.SrcGroupBox.Size = new System.Drawing.Size(293, 74);
            this.SrcGroupBox.TabIndex = 1;
            this.SrcGroupBox.TabStop = false;
            this.SrcGroupBox.Text = "源地址";
            // 
            // SrcIpTextBox
            // 
            this.SrcIpTextBox.Location = new System.Drawing.Point(44, 33);
            this.SrcIpTextBox.Name = "SrcIpTextBox";
            this.SrcIpTextBox.Size = new System.Drawing.Size(158, 21);
            this.SrcIpTextBox.TabIndex = 1;
            this.SrcIpTextBox.TextChanged += new System.EventHandler(this.SrcIpTextBox_TextChanged);
            // 
            // SrcIpLabel
            // 
            this.SrcIpLabel.AutoSize = true;
            this.SrcIpLabel.Location = new System.Drawing.Point(21, 36);
            this.SrcIpLabel.Name = "SrcIpLabel";
            this.SrcIpLabel.Size = new System.Drawing.Size(17, 12);
            this.SrcIpLabel.TabIndex = 0;
            this.SrcIpLabel.Text = "IP";
            // 
            // DstGroupBox
            // 
            this.DstGroupBox.Controls.Add(this.DstIpTextBox);
            this.DstGroupBox.Controls.Add(this.DstIpLabel);
            this.DstGroupBox.Location = new System.Drawing.Point(23, 193);
            this.DstGroupBox.Name = "DstGroupBox";
            this.DstGroupBox.Size = new System.Drawing.Size(293, 75);
            this.DstGroupBox.TabIndex = 2;
            this.DstGroupBox.TabStop = false;
            this.DstGroupBox.Text = "目的地址";
            // 
            // DstIpTextBox
            // 
            this.DstIpTextBox.Location = new System.Drawing.Point(44, 29);
            this.DstIpTextBox.Name = "DstIpTextBox";
            this.DstIpTextBox.Size = new System.Drawing.Size(158, 21);
            this.DstIpTextBox.TabIndex = 1;
            this.DstIpTextBox.TextChanged += new System.EventHandler(this.DstIpTextBox_TextChanged);
            // 
            // DstIpLabel
            // 
            this.DstIpLabel.AutoSize = true;
            this.DstIpLabel.Location = new System.Drawing.Point(21, 32);
            this.DstIpLabel.Name = "DstIpLabel";
            this.DstIpLabel.Size = new System.Drawing.Size(17, 12);
            this.DstIpLabel.TabIndex = 0;
            this.DstIpLabel.Text = "IP";
            // 
            // ConfirmButton
            // 
            this.ConfirmButton.Location = new System.Drawing.Point(150, 289);
            this.ConfirmButton.Name = "ConfirmButton";
            this.ConfirmButton.Size = new System.Drawing.Size(75, 23);
            this.ConfirmButton.TabIndex = 3;
            this.ConfirmButton.Text = "确定";
            this.ConfirmButton.UseVisualStyleBackColor = true;
            this.ConfirmButton.Click += new System.EventHandler(this.ConfirmButton_Click);
            // 
            // CancelButton
            // 
            this.CancelButton.Location = new System.Drawing.Point(242, 289);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(75, 23);
            this.CancelButton.TabIndex = 4;
            this.CancelButton.Text = "取消";
            this.CancelButton.UseVisualStyleBackColor = true;
            this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // FilterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(350, 334);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.ConfirmButton);
            this.Controls.Add(this.DstGroupBox);
            this.Controls.Add(this.SrcGroupBox);
            this.Controls.Add(this.ProtocalGroupBox);
            this.Name = "FilterForm";
            this.Text = "过滤选项";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FilterForm_FormClosing);
            this.ProtocalGroupBox.ResumeLayout(false);
            this.ProtocalGroupBox.PerformLayout();
            this.SrcGroupBox.ResumeLayout(false);
            this.SrcGroupBox.PerformLayout();
            this.DstGroupBox.ResumeLayout(false);
            this.DstGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox ProtocalGroupBox;
        private System.Windows.Forms.GroupBox SrcGroupBox;
        private System.Windows.Forms.CheckBox UdpCheckBox;
        private System.Windows.Forms.CheckBox TcpCheckBox;
        private System.Windows.Forms.CheckBox ArpCheckBox;
        private System.Windows.Forms.GroupBox DstGroupBox;
        private System.Windows.Forms.CheckBox IgmpCheckBox;
        private System.Windows.Forms.CheckBox IcmpCheckBox;
        private System.Windows.Forms.Label SrcIpLabel;
        private System.Windows.Forms.Label DstIpLabel;
        private System.Windows.Forms.TextBox SrcIpTextBox;
        private System.Windows.Forms.TextBox DstIpTextBox;
        private System.Windows.Forms.Button ConfirmButton;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.Button CancelButton;

    }
}