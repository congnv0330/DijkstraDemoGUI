using System;
using System.Drawing;
using System.Windows.Forms;

namespace Dijkstra_Demo
{
    public class Welcome
    {
        public bool Value { get; set; }

        //Tạo form chọn kiểu đồ thị
        public DialogResult ChooseGraphType(string title)
        {
            #region Design
            Form form = new Form();
            Label lbTitle = new Label();
            Label lbNote = new Label();
            Label lbNoteHelp = new Label();
            RadioButton rbtnDirected = new RadioButton();
            RadioButton rbtnUndirected = new RadioButton();
            Button btnOK = new Button();

            // lbTitle 
            lbTitle.AutoSize = true;
            lbTitle.Font = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            lbTitle.Location = new Point(13, 9);
            lbTitle.Margin = new Padding(4, 0, 4, 0);
            lbTitle.Name = "label1";
            lbTitle.Size = new Size(164, 23);
            lbTitle.Text = "Please choose Graph Type:";

            // lbNote
            lbNote.AutoSize = true;
            lbNote.Font = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            lbNote.ForeColor = Color.Red;
            lbNote.Location = new Point(13, 101);
            lbNote.Margin = new Padding(4, 0, 4, 0);
            lbNote.Name = "label2";
            lbNote.Size = new Size(288, 23);
            lbNote.Text = "The graph used here is multi graph!"; 

            // lbNoteHelp
            lbNoteHelp.AutoSize = true;
            lbNoteHelp.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            lbNoteHelp.Location = new Point(13, 124);
            lbNoteHelp.Margin = new Padding(4, 0, 4, 0);
            lbNoteHelp.Name = "label2";
            lbNoteHelp.Size = new Size(288, 23);
            lbNoteHelp.Text = "Click Help - Guide to know how to use.";

            // rbtnDirected 
            rbtnDirected.AutoSize = true;
            rbtnDirected.Location = new Point(17, 35);
            rbtnDirected.Name = "radioButton1";
            rbtnDirected.Size = new Size(147, 27);
            rbtnDirected.Text = "Directed Graph";
            rbtnDirected.Checked = true;
            rbtnDirected.CheckedChanged += RbtnDirected_CheckedChanged;

            // rbtnUndirected
            rbtnUndirected.AutoSize = true;
            rbtnUndirected.Location = new Point(17, 70);
            rbtnUndirected.Name = "radioButton2";
            rbtnUndirected.Size = new Size(167, 27);
            rbtnUndirected.Text = "Undirected Graph";
            rbtnUndirected.CheckedChanged += RbtnUndirected_CheckedChanged;

            // btnOK 
            btnOK.AutoSize = true;
            btnOK.Location = new Point(89, 152);
            btnOK.Name = "button1";
            btnOK.Size = new Size(122, 33);
            btnOK.Text = "OK";
            btnOK.DialogResult = DialogResult.OK;
            btnOK.UseVisualStyleBackColor = true;

            // Form
            form.Text = title;
            form.ClientSize = new Size(311, 201);
            form.Controls.AddRange(new Control[] { lbTitle, lbNote, lbNoteHelp, rbtnDirected, rbtnUndirected, btnOK });
            form.Font = new Font("Segoe UI", 10.2F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            form.StartPosition = FormStartPosition.CenterParent;
            form.FormBorderStyle = FormBorderStyle.FixedSingle;
            form.BackColor = Color.GhostWhite;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = btnOK;
            form.StartPosition = FormStartPosition.CenterParent;
            form.ShowIcon = false;
            form.ShowInTaskbar = false;
            #endregion

            Value = true;

            DialogResult result = form.ShowDialog();

            return result;
        }

        //Sự kiện thay đổi giá trị của RbtnUndirected
        private void RbtnUndirected_CheckedChanged(object sender, EventArgs e)
        {
            Value = false;
        }

        //Sự kiện thay đổi giá trị của RbtnDirected
        private void RbtnDirected_CheckedChanged(object sender, EventArgs e)
        {
            Value = true;
        }
    }
}
