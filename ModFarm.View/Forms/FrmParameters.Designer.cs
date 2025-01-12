using System.Xml.Linq;

namespace Scada.Server.Modules.ModFarm.View.Forms
{
    partial class FrmParameters
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        public FrmModuleConfig frmParentGloabal;        // global general form


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
            dgvCData = new DataGridView();
            rtbNode = new RichTextBox();
            dgvAttribute = new DataGridView();
            gbComment = new GroupBox();
            rtbParam = new RichTextBox();
            ((System.ComponentModel.ISupportInitialize)dgvCData).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvAttribute).BeginInit();
            gbComment.SuspendLayout();
            SuspendLayout();
            // 
            // dgvCData
            // 
            dgvCData.BackgroundColor = SystemColors.Control;
            dgvCData.BorderStyle = BorderStyle.None;
            dgvCData.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvCData.Location = new Point(11, 411);
            dgvCData.Name = "dgvCData";
            dgvCData.RowHeadersWidth = 51;
            dgvCData.Size = new Size(571, 139);
            dgvCData.TabIndex = 9;
            //dgvCData.CellValueChanged += DgvCData_CellValueChanged;
            // 
            // rtbNode
            // 
            rtbNode.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            rtbNode.BackColor = SystemColors.Info;
            rtbNode.BorderStyle = BorderStyle.None;
            rtbNode.Location = new Point(6, 27);
            rtbNode.Margin = new Padding(3, 4, 3, 4);
            rtbNode.Name = "rtbNode";
            rtbNode.Size = new Size(559, 137);
            rtbNode.TabIndex = 5;
            rtbNode.Text = "";
            // 
            // dgvAttribute
            // 
            dgvAttribute.BackgroundColor = SystemColors.Control;
            dgvAttribute.BorderStyle = BorderStyle.None;
            dgvAttribute.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvAttribute.Location = new Point(11, 50);
            dgvAttribute.Name = "dgvAttribute";
            dgvAttribute.RowHeadersWidth = 51;
            dgvAttribute.Size = new Size(571, 167);
            dgvAttribute.TabIndex = 2;
            //dgvAttribute.CellValueChanged += DgvAttribute_CellValueChanged;
            // 
            // gbComment
            // 
            gbComment.Controls.Add(rtbNode);
            gbComment.Location = new Point(11, 300);
            gbComment.Name = "gbComment";
            gbComment.Size = new Size(571, 171);
            gbComment.TabIndex = 10;
            gbComment.TabStop = false;
            gbComment.Text = "groupBox1";
            // 
            // rtbParam
            // 
            rtbParam.Location = new Point(11, 10);
            rtbParam.Name = "rtbParam";
            rtbParam.Size = new Size(572, 100);
            rtbParam.TabIndex = 11;
            rtbParam.Text = "";
            // 
            // FrmParameters
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoScroll = true;
            ClientSize = new Size(607, 561);
            Controls.Add(rtbParam);
            Controls.Add(dgvCData);
            Controls.Add(dgvAttribute);
            Controls.Add(gbComment);
            Margin = new Padding(3, 4, 3, 4);
            Name = "FrmParameters";
            Text = "FormParameters";
            Load += FrmParameters_Load;
            ((System.ComponentModel.ISupportInitialize)dgvCData).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvAttribute).EndInit();
            gbComment.ResumeLayout(false);
            ResumeLayout(false);
        }

        private void FrmParameters_FormClosing(object sender, FormClosingEventArgs e)
        {
            findCnl.Click -= Search_Click; // Отписка от кнопки поиска, чтобы не появлялось окно несколько раз
            throw new NotImplementedException();
        }
        #endregion

        private DataGridView dgvCData;
        private RichTextBox rtbNode;
        private DataGridView dgvAttribute;
        private GroupBox gbComment;
        private RichTextBox rtbParam;
    }
}