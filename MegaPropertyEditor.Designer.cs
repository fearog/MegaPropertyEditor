namespace MegaPropertyEditor
{
	partial class MegaPropertyEditor
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose( bool disposing )
		{
			if( disposing && ( components != null ) )
			{
				components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.button1 = new System.Windows.Forms.Button();
			this.m_ctlTreeView = new System.Windows.Forms.TreeView();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.button1.Location = new System.Drawing.Point(0, 345);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(59, 24);
			this.button1.TabIndex = 2;
			this.button1.Text = "Refresh";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.RefreshClicked);
			// 
			// m_ctlTreeView
			// 
			this.m_ctlTreeView.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.m_ctlTreeView.Location = new System.Drawing.Point(0, 0);
			this.m_ctlTreeView.Name = "m_ctlTreeView";
			this.m_ctlTreeView.Scrollable = false;
			this.m_ctlTreeView.Size = new System.Drawing.Size(403, 369);
			this.m_ctlTreeView.TabIndex = 1;
			this.m_ctlTreeView.BeforeCollapse += new System.Windows.Forms.TreeViewCancelEventHandler(this.BeforeTreeChange);
			this.m_ctlTreeView.AfterCollapse += new System.Windows.Forms.TreeViewEventHandler(this.AfterTreeChange);
			this.m_ctlTreeView.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.BeforeTreeChange);
			this.m_ctlTreeView.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.AfterTreeChange);
			this.m_ctlTreeView.DrawNode += new System.Windows.Forms.DrawTreeNodeEventHandler(this.DrawNode);
			// 
			// MegaPropertyEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoScroll = true;
			this.Controls.Add(this.button1);
			this.Controls.Add(this.m_ctlTreeView);
			this.Name = "MegaPropertyEditor";
			this.Size = new System.Drawing.Size(693, 369);
			this.Load += new System.EventHandler(this.MegaPropertyGrid_Load);
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.MegaPropertyGrid_Paint);
			this.ResumeLayout(false);

		}

		#endregion

		public System.Windows.Forms.TreeView m_ctlTreeView;
		private System.Windows.Forms.Button button1;




	}
}
