using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Dijkstra_Demo
{
    public partial class Form1 : Form
    {
        //Danh sách các biến sử dụng
        #region Variables
        MyGraphic myGraphic = new MyGraphic();
        MyGraph myGraph = new MyGraph();
        Dijkstra dijkstra = new Dijkstra();

        private int i_th = 1;
        private int grid_gap = 30;
        private int radius = 12;
        private bool isPoint = false;
        private bool isDrawing = false;
        private bool dijkstra_step = true;
        private bool isTimeSleep = true;
        private bool isPaths = true;
        private bool isColor = false;        

        Point p = new Point(0, 0);
        List<Point> Pt = new List<Point>();
        List<PointColor> PtColor = new List<PointColor>();
        List<Segment> segment = new List<Segment>();
        List<Segment> segment_dijkstra = new List<Segment>();
        List<int> segment_dijkstra_save = new List<int>();
        List<int> segment_dijkstra_save_tmp = new List<int>();
        List<List<Segment>> segment_dijkstra_Review_tmp = new List<List<Segment>>();
        List<List<Segment>> segment_dijkstra_Review = new List<List<Segment>>();
        List<List<Segment>> tested = new List<List<Segment>>();

        ListView lv = new ListView();

        public class Segment
        {
            public int S { get; set; }
            public int E { get; set; }
            public string W { get; set; }

            public Segment(int s, int e, string w)
            {
                this.S = s;
                this.E = e;
                this.W = w;
            }
        }

        private class PointColor
        {
            public int _index { get; set; }
            public Color _color { get; set; }

            public PointColor(int _index, Color _color)
            {
                this._index = _index;
                this._color = _color;
            }
        }
        #endregion

        public Form1()
        {
            InitializeComponent();
            //Chỉnh size Maximum full màn hình
            //this.MaximumSize = new Size(this.Width, Screen.PrimaryScreen.Bounds.Height);
            //Dành cho thread
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        #region Menustrip
        //Sự kiện click menu close
        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Đóng chương trình
            Application.Exit();
        }

        //Sự kiện click menu about
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Hiển thị form About
            About about = new About();
            about.ShowDialog();
        }

        //Sự kiện click menu guide
        private void guideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Hiển thị Form Guide
            Guide f = new Guide();
            f.ShowDialog();
        }

        //Sự kiện hiện form Dijkstra FibonnaciHeaps
        private void dijkstraFibonnaciHeapsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Hiển thị Form Code Dijkstra FibonnaciHeaps
            DijkstraFibonnaciHeaps dijkstraCode = new DijkstraFibonnaciHeaps();
            dijkstraCode.Show();
        }

        //Sự kiện hiện form DijkstraHeap
        private void dijkstraHeapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Hiển thị Form DijkstraHeap
            DijkstraHeap dijkstraHeap = new DijkstraHeap();
            dijkstraHeap.Show();
        }

        //Sự kiện hiện form DijkstraNormal
        private void dijkstraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Hiển thị Form DijkstraNormal
            DijkstraNormal dijkstraNormal = new DijkstraNormal();
            dijkstraNormal.Show();
        }
        #endregion

        #region GridLine
        private void MakeBackgroundGrid()
        {
            if (cbxGridLine.Checked)
            {
                // tạo một image bitmap bằng graphic
                Bitmap bm = new Bitmap(picGraphView.ClientSize.Width, picGraphView.ClientSize.Height);
                var g = Graphics.FromImage(bm);
                Pen pen = new Pen(ChangeColorBrightness(Color.LightGray, 0.5F), 1);

                // Lần lượt vẽ các đường thẳng xuống và sang ngang
                for (int i = grid_gap; i < picGraphView.ClientSize.Width; i += grid_gap)
                {
                    g.DrawLine(pen, i, 0, i, picGraphView.ClientSize.Height);
                }

                for (int j = grid_gap; j < picGraphView.ClientSize.Height; j += grid_gap)
                {
                    g.DrawLine(pen, 0, j, picGraphView.ClientSize.Width, j);
                }

                // Set background bằng ảnh đó
                picGraphView.BackgroundImage = bm;
            }
            else picGraphView.BackgroundImage = null;
        }

        private void cbxSnapGrid_CheckedChanged(object sender, EventArgs e)
        {
            //Sự kiện khi tích vào checkbox 
            MakeBackgroundGrid();
        }

        private void picGraphView_Resize(object sender, EventArgs e)
        {
            MakeBackgroundGrid();
        }
        #endregion

        #region Graph
        //Hàm tính khu vực xác định Point a, b
        private int FindDistanceToPointSquared(Point a, Point b)
        {
            int dx = a.X - b.X;
            int dy = a.Y - b.Y;
            return dx * dx + dy * dy;
        }

        //Hàm check tọa độ mouse_pt có gần với điểm nào trong list Pt 
        private bool MouseIsOverEndPoint(Point mouse_pt, out int segment_number, out Point hit_pt)
        {
            int over_squared = radius * radius;
            for (int i = 0; i < Pt.Count; ++i)
            {
                if (FindDistanceToPointSquared(mouse_pt, Pt[i]) <= over_squared)
                {
                    //đánh dấu vị trí tọa độ gần nhất với mouse_pt lại bằng segment_number
                    segment_number = i;
                    hit_pt = Pt[i];
                    return true;
                }
            }

            //Nếu không có thì thiết lập false
            segment_number = -1;
            hit_pt = new Point(0, 0);
            return false;
        }

        //Sự kiện di chuyển con chuột
        private void picGraphView_MouseMove(object sender, MouseEventArgs e)
        {
            //khi không ở chế độ vẽ điểm thì thực thi
            if (!isPoint)
            {
                //Gán biểu tượng chuột mặc định
                Cursor my_cursor = Cursors.Default;
                Point hit_point;
                int segment_num;
                if (MouseIsOverEndPoint(e.Location, out segment_num, out hit_point)) my_cursor = Cursors.Hand;
                //Gán cho khu vực picGraphView có con chuột hình dạng như đã thiết lập
                if (picGraphView.Cursor != my_cursor) picGraphView.Cursor = my_cursor;
            }
            else lbStatus.Text = "Draw Point (" + e.X.ToString() + ", " + e.Y.ToString() + ")";
        }

        //Sự kiện nhấp chuột
        private void picGraphView_MouseDown(object sender, MouseEventArgs e)
        {
            //Nếu đang là chế độ vẽ Point
            if (e.Button == MouseButtons.Left)
            {
                if (isPoint)
                {
                    //Thêm tọa độ chuột hiện tại vào list Pt
                    Pt.Add(e.Location);
                    //Vẽ bảng ma trận
                    myGraph.MatrixCreate(lvMatrixView, i_th, ChangeColorBrightness(Color.LightGreen, 0.6F));
                    //Thêm giá trị vào danh sách combobox 
                    cbStartPoint.Items.Add(i_th.ToString());
                    cbEndPoint.Items.Add(i_th.ToString());
                    //Hiển thị log
                    txbLogs.Text += "[" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + "] Point " + i_th.ToString() + " (" + e.X.ToString() + ", " + e.Y.ToString() + ")" + " add!\n";
                    ++i_th;
                }
                else
                {
                    Point hit_pt;
                    int segment_number;
                    if (MouseIsOverEndPoint(e.Location, out segment_number, out hit_pt))
                    {
                        //Nếu đang ở chế độ vẽ đường thẳng
                        if (isDrawing)
                        {
                            //Tạo các sự kiện để thực hiện lệnh
                            picGraphView.MouseMove -= picGraphView_MouseMove;
                            picGraphView.MouseMove += picGraphView_MouseMove_Drawing;
                            picGraphView.MouseUp += picGraphView_MouseUp_Drawing;
                        }
                        //Nếu đang ở chế độ tô màu cho điểm
                        else if (isColor)
                        {
                            //Thêm Điểm được xác định với màu tương ứng lấy từ colordialog 
                            PtColor.Add(new PointColor(segment_number, MyColorDialog.Color));
                            txbLogs.Text += "[" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + "] Color Point " + (segment_number + 1).ToString() + " changed!\n";
                        }
                        else
                        {
                            //Tạo các sự kiện để thực hiện lệnh co chế độ moving point
                            picGraphView.MouseMove -= picGraphView_MouseMove;
                            picGraphView.MouseMove += picGraphView_MouseMove_Point;
                            picGraphView.MouseUp += picGraphView_MouseUp_Point;
                        }

                        //Gán moving_segment bằng điểm đã được đánh dấu
                        moving_segment = segment_number;
                    }
                }
            }

            //Refresh lại
            picGraphView.Invalidate();
        }

        #region MovingPoint
        //Khởi tạo moving_point
        private int moving_segment = -1;

        //Sự kiện di chuyển chuột trong chế độ moving point
        private void picGraphView_MouseMove_Point(object sender, MouseEventArgs e)
        {
            //cập nhật tọa độ mới cho điểm đã được đánh dấu
            Pt[moving_segment] = e.Location;
            //Refresh lại
            picGraphView.Invalidate();
            //Hiển thị logs
            lbStatus.Text = "Point " + (moving_segment + 1).ToString() + " (" + e.X.ToString() + ", " + e.Y.ToString() + ")";
        }

        //Sự kiện thả chuột trong chế độ moving point
        private void picGraphView_MouseUp_Point(object sender, MouseEventArgs e)
        {
            //Hiển thị logs
            txbLogs.Text += "[" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + "] Point " + (moving_segment + 1).ToString() + " new location (" + Pt[moving_segment].X.ToString() + ", " + Pt[moving_segment].Y.ToString() + ") updated!\n";
            //thay đổi trạng thái label status
            lbStatus.Text = "View and move";
            //Xóa các sự kiện của moving point đi và khôi phục lại sự kiện picGraphView_MouseMove
            picGraphView.MouseMove -= picGraphView_MouseMove_Point;
            picGraphView.MouseMove += picGraphView_MouseMove;
            picGraphView.MouseUp -= picGraphView_MouseUp_Point;
            //Refresh lại
            picGraphView.Invalidate();
        }
        #endregion

        #region DrawingMove
        //Sự kiện di chuyển chuột trong chế độ vẽ đường thẳng
        private void picGraphView_MouseMove_Drawing(object sender, MouseEventArgs e)
        {
            //Thiết lập bán kính
            int over_squared = radius * radius;
            //Gán tọa độ hiện tại cho biến tạm p để chạy sự kiện Paint
            p = e.Location;

            Point hit_pt = new Point();
            hit_pt = Pt[moving_segment];
            int segment_num;
            //Xác định xem con chuột có di chuyển tới gần một điểm nào hay không
            if (FindDistanceToPointSquared(e.Location, hit_pt) > over_squared && MouseIsOverEndPoint(e.Location, out segment_num, out hit_pt))
            {
                start:
                string value = "";
                //Hiển thị khung nhập giá trị
                if (InputGraph.InputBox("Log", "Enter Input", ref value) == DialogResult.OK)
                {
                    //Nếu trọng số là số và lớn hơn 0 (Dijkstra không chạy trên trọng số âm)
                    if (int.TryParse(value, out int tmp) && tmp > 0)
                    {
                        int temp;
                        //Nếu là điểm điểm đã được vẽ trc đó (tức là đường thẳng này đã có giá trị) thì cập nhật lại trọng số mới
                        if (!checkindexSimpleDirect(moving_segment, segment_num, out temp)) segment[temp].W = value;
                        //Tạo cạnh mới với trọng số
                        else segment.Add(new Segment(moving_segment, segment_num, value));
                        //Hiển thị logs
                        txbLogs.Text += "[" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + "] Point " + (moving_segment + 1).ToString() + " to Point " + (segment_num + 1).ToString() + " is " + value + "\n";
                    }
                    else
                    {
                        //Báo lỗi
                        MessageBox.Show("Input isn't correct", "Logs", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        //Tạo vòng lặp lại từ điểm start:
                        goto start;
                    }
                }
                //chạy tới sự kiện thả chuột
                picGraphView_MouseUp_Drawing(sender, e);
            }
            //Refresh lại
            picGraphView.Invalidate();
        }

        //Sự kiện thả chuột trong chế độ vẽ đường thẳng
        private void picGraphView_MouseUp_Drawing(object sender, MouseEventArgs e)
        {
            //Xóa các sự kiện vẽ đường thẳng và khôi phục sự kiện picGraphView_MouseMove
            picGraphView.MouseMove += picGraphView_MouseMove;
            picGraphView.MouseMove -= picGraphView_MouseMove_Drawing;
            picGraphView.MouseUp -= picGraphView_MouseUp_Drawing;
            //Thiết lập biến tạm p về lại mặc định
            p = new Point(0, 0);
            //Refresh
            picGraphView.Invalidate();
        }
        #endregion

        //Hàm kiểm tra xem đường thẳng có giá trị trọng số hay không và đánh dấu lại vị trí qua tmp
        private bool checkindexSimpleDirect(int s, int e, out int tmp)
        {
            if (segment.Count == 0) { tmp = -1; return true; }
            int temp = segment.Count;
            for (int i = 0; i < segment.Count / 2 + 1; ++i)
            {
                if (segment[i].S == s && segment[i].E == e) { tmp = i; return false; }
                if (segment[temp - i - 1].S == s && segment[temp - i - 1].E == e) { tmp = temp - i - 1; return false; }
            }
            tmp = -1;
            return true;
        }

        //Sự kiện Paint (Mỗi lần Refresh là sự kiện paint được chạy
        private void picGraphView_Paint(object sender, PaintEventArgs e)
        {
            //thiết lập chất lượng graphic là AntiAlias
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            //Nếu biến tạm p có giá trị khác 0 thì vẽ một đường thẳng tạm thời để hiển thị đường vẽ từ điểm bắt đầu
            if (p.X != 0) myGraphic.DrawLineNoInputGraph(e.Graphics, Pt[moving_segment], p, Brushes.DarkGray, 3, rbtnDirected.Checked);

            //Vẽ các đường thẳng có trong list segment đã add trên sự kiện vẽ đường thẳng
            for (int i = 0; i < segment.Count; ++i)
            {
                int temp;
                //Nếu có tồn tại hai dường thẳng song song vẽ thành đường còn để biểu diễn
                if (!checkindexSimpleDirect(segment[i].E, segment[i].S, out temp) && !checkindexSimpleDirect(segment[i].S, segment[i].E, out temp)) myGraphic.DrawCurve(e.Graphics, Pt[segment[i].S], Pt[segment[i].E], segment[i].W, this.Font, Brushes.Black, 3, rbtnDirected.Checked);
                else myGraphic.DrawLine(e.Graphics, Pt[segment[i].S], Pt[segment[i].E], segment[i].W, this.Font, Brushes.Black, 3, rbtnDirected.Checked);
                //Gán giá trị trọng số cho ma trận 
                myGraph.MatrixAdd(lvMatrixView, segment[i].S, segment[i].E, rbtnUnDirected.Checked, segment[i].W);
            }

            //Vẽ các điểm có trong Pt đã add ở sự kiện nhấp chuột
            for (int i = 0; i < Pt.Count; ++i)
                myGraphic.DrawPoint(e.Graphics, Pt[i], (i + 1).ToString(), 4, Brushes.LightYellow, this.Font);

            //Nếu segment_dijkstra_Review số phần tử khác 0 thì bắt đầu vẽ các đường thẳng biểu diễn tạm thời khi chạy lệnh run step hoặc run with timesleep
            if (segment_dijkstra_Review.Count != 0)
            {
                int temp;
                foreach (List<Segment> i in segment_dijkstra_Review)
                    foreach (Segment j in i)
                        if (!checkindexSimpleDirect(j.E, j.S, out temp) && !checkindexSimpleDirect(j.S, j.E, out temp)) myGraphic.DrawCurve(e.Graphics, Pt[j.S], Pt[j.E], j.W, this.Font, Brushes.Blue, 4, rbtnDirected.Checked);
                        else myGraphic.DrawLine(e.Graphics, Pt[j.S], Pt[j.E], j.W, this.Font, Brushes.Blue, 4, rbtnDirected.Checked);
            }

            //Nếu segment_dijkstra_save có sớ phần từ khác 0 thì vẽ chồng các điểm được biểu diễn dựa trên bảng dijkstra chạy tay
            if (segment_dijkstra_save.Count != 0)
            {
                foreach (int i in segment_dijkstra_save)
                    myGraphic.DrawPoint(e.Graphics, Pt[i], (i + 1).ToString(), 5, Brushes.Yellow, this.Font);
            }

            //segment_dijkstra_save_tmp có số phần từ bằng 0 thì vẽ ra đường đi ngắn nhất
            if (segment_dijkstra_save_tmp.Count == 0)
            {
                if (segment_dijkstra.Count != 0)
                {
                    int temp;
                    foreach (Segment i in segment_dijkstra)
                        if (!checkindexSimpleDirect(i.E, i.S, out temp) && !checkindexSimpleDirect(i.S, i.E, out temp)) myGraphic.DrawCurve(e.Graphics, Pt[i.S], Pt[i.E], i.W, this.Font, Brushes.Red, 4, rbtnDirected.Checked);
                        else myGraphic.DrawLine(e.Graphics, Pt[i.S], Pt[i.E], i.W, this.Font, Brushes.Red, 4, rbtnDirected.Checked);

                    foreach (Segment i in segment_dijkstra)
                    {
                        myGraphic.DrawPoint(e.Graphics, Pt[i.S], (i.S + 1).ToString(), 5, Brushes.Yellow, this.Font);
                        myGraphic.DrawPoint(e.Graphics, Pt[i.E], (i.E + 1).ToString(), 5, Brushes.Yellow, this.Font);
                    }
                }
            }

            //Nếu có tô màu điểm nào đó thì bắt đầu vẽ điểm đó chồng lên
            if(PtColor.Count != 0)
            {
                foreach(PointColor i in PtColor)
                    myGraphic.DrawPoint(e.Graphics, Pt[i._index], (i._index + 1).ToString(), 5, new SolidBrush(i._color), this.Font);
            }
        }
        #endregion

        #region TurnOnOffEditGraph
        //Sự kiện nhấn nút vẽ điểm
        private void btnDrawPoint_Click(object sender, EventArgs e)
        {
            //Nếu không ở chế độ vẽ điểm thì chuyển sang và ngược lại
            if (!isPoint) { isPoint = true; lbStatus.Text = "Draw point"; }
            else { isPoint = false; lbStatus.Text = "View and move"; }
            //Vô hiệu hóa các chế độ khác
            isDrawing = false;
            isColor = false;
        }

        //Sự kiện nhấn nút di chuyển điểm
        private void btnMove_Click(object sender, EventArgs e)
        {
            //Vô hiệu hóa các chế độ khác
            isPoint = false;
            isDrawing = false;
            isColor = false;
            lbStatus.Text = "View and move";
        }

        //Sự kiện vẽ đường thẳng
        private void btnDrawLine_Click(object sender, EventArgs e)
        {
            //Nếu không ở chế độ vẽ đường thẳng thì chuyển sang và ngược lại
            if (!isDrawing) { isDrawing = true; lbStatus.Text = "Draw Line"; }
            else { isDrawing = false; lbStatus.Text = "View and move"; }
            //Vô hiệu hóa các chế độ khác
            isPoint = false;
            isColor = false;
        }

        //Sự kiện vẽ màu cho điểm
        private void btnColor_Click(object sender, EventArgs e)
        {
            //Nếu chế độ tô màu đang vô hiệu hóa thì chuyển sang và ngược lại
            if (!isColor) { isColor = true; lbStatus.Text = "Draw Color"; }
            else { isColor = false; lbStatus.Text = "View and move"; }
            //Vô hiệu hóa các chế độ khác
            isPoint = false;
            isDrawing = false;
            //Hiển thị bảng chọn màu
            //Nếu không chon bất cứ màu nào thì vô hiệu hóa chế độ tô màu
            if (MyColorDialog.ShowDialog() != DialogResult.OK) { isColor = false; lbStatus.Text = "View and move"; }
            //Ngược lại
            else { isColor = true; lbStatus.Text = "Draw Color"; }
        }
        #endregion

        //Hàm chuyển về mặc định tất cả (tạo ra trang mới cho vẽ đồ thị)
        private void RestNew()
        {
            if (MessageBox.Show("Create New Graph?", "Logs", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                this.Cursor = Cursors.WaitCursor;
                isPoint = false;
                isDrawing = false;
                isTimeSleep = true;
                isPaths = true;
                isColor = false;
                lbStatus.Text = "View and move";
                p = new Point(0, 0);
                dijkstra_step = true;
                Pt.Clear();
                PtColor.Clear();
                segment.Clear();
                segment_dijkstra.Clear();
                segment_dijkstra_Review.Clear();
                segment_dijkstra_Review_tmp.Clear();
                segment_dijkstra_save.Clear();
                segment_dijkstra_save_tmp.Clear();
                tested.Clear();
                PtColor.Clear();
                picGraphView.Invalidate();
                txbLogs.Clear();
                txbLogDijkstra.Clear();
                txbTimeSleep.Clear();
                cbxTimeSleep.Checked = false;
                lvMatrixView.GridLines = false;
                lvMatrixView.Clear();
                lvMatrixView.Columns.Add("", 35);
                lvTableView.GridLines = false;
                lvTableView.Clear();
                cbStartPoint.Items.Clear();
                cbStartPoint.SelectedIndex = -1;
                cbStartPoint.SelectedItem = null;
                cbStartPoint.SelectedValue = null;
                cbStartPoint.SelectedText = "";
                cbStartPoint.Text = "";
                cbStartPoint.Refresh();
                cbEndPoint.Items.Clear();
                cbEndPoint.SelectedIndex = -1;
                cbEndPoint.SelectedItem = null;
                cbEndPoint.SelectedValue = null;
                cbEndPoint.SelectedText = "";
                cbEndPoint.Text = "";
                cbEndPoint.Refresh();
                cbEndPoint.Items.Add("All");
                cbDemo.Text = "";
                i_th = 1;
                txbStarting.Clear();
                txbListLocation.Clear();
                num_th = 0;
                tested.Clear();
                lbRecommed.Text = "Recommended";
                fpnlTested.Controls.Clear();
                this.Cursor = Cursors.Default;
            }
        }

        //Sự kiện contextmenu clear
        private void clearAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RestNew();
        }

        //Sự kiện New
        private void btnNew_Click(object sender, EventArgs e)
        {
            //Gọi hàm RestNew         
            RestNew();
        }

        //Sự kiện new trên toolstrip
        private void newToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            RestNew();
        }

        //Sự kiện thay đổi giá trị của rbtnUnDirected (chọn chế độ đồ thị vô hướng)
        private void rbtnUnDirected_CheckedChanged(object sender, EventArgs e)
        {
            //Mở các panel chứa các lệnh đi kèm
            pnlChoose.Enabled = true;
            pnlGraph.Enabled = true;
            pnlDijkstra.Enabled = true;
            pnlTested.Enabled = true;
        }

        //Sự kiện thay đổi giá trị của rbtnDirected (chọn chế độ đồ thị có hướng)
        private void rbtnDirected_CheckedChanged(object sender, EventArgs e)
        {
            //Mở các panel chứa các lệnh đi kèm
            pnlChoose.Enabled = true;
            pnlGraph.Enabled = true;
            pnlDijkstra.Enabled = true;
            pnlTested.Enabled = true;
        }

        //Sự kiện chọn sử dụng đồ thị mẫu
        private void rbtnDemo_CheckedChanged(object sender, EventArgs e)
        {
            //Mở panal chưa combobox chứa danh sách đồ thị mẫu
            pnlDemo.Enabled = true;
        }

        //Sự kiện lựa chọn đồ thị mẫu trong combobox
        private void cbDemo_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                PtColor.Clear();
                this.Cursor = Cursors.WaitCursor;

                //Tạo đường dẫn 
                string paths = @"Demo\" + (cbDemo.SelectedIndex + 1).ToString() + ".txt";
                //Đọc file lưa đồ thị
                myGraph.ReadGraph(Pt, segment, lvMatrixView, ChangeColorBrightness(Color.LightGreen, 0.6F), rbtnDirected, rbtnUnDirected, cbStartPoint, cbEndPoint, out i_th, paths);
                //Refresh lại list chứa đường đi ngắn nhất dijkstra
                segment_dijkstra.Clear();
                //Refresh lại bảng dijkstra chạy tay
                lvTableView.Clear();
                //Refresh lại hiển thị đường đi dijkstra
                txbLogDijkstra.Clear();
                //hiển thị logs
                txbLogs.Text += "Graph demo " + (cbDemo.SelectedIndex + 1).ToString() + "\n";

                this.Cursor = Cursors.Default;

                //Refresh lại
                picGraphView.Invalidate();
            }
            catch (Exception)
            {
                //Báo lỗi nếu đồ thị mẫu bị xóa hoặc không tồn tại
                MessageBox.Show("File demo isn't available !", "Logs", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //Sự kiện chọn tự vẽ
        private void rbtnDraw_CheckedChanged(object sender, EventArgs e)
        {
            //Khóa lại panal chứa combobox demo
            pnlDemo.Enabled = false;
        }

        #region OpenSave
        //Hàm lưu đồ thị
        private void Save()
        {
            if (MySaveFileDialog.ShowDialog() == DialogResult.OK)
            {                
                //Gọi hàm mở đồ thị
                myGraph.SaveGraph(Pt, segment, rbtnDirected.Checked, MySaveFileDialog.FileName);
                txbLogDijkstra.Clear();
                txbLogs.Text += "[" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + "] Graph save!\n";
            }
        }

        //sự kiện nhấn button lưu đồ thị
        private void btnSaveGraph_Click(object sender, EventArgs e)
        {
            Save();
        }

        //sự kiện nhấn toolstrip lưu đồ thị
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Save();
        }

        //Hàm mở để đọc file lưu đồ thị
        private void Open()
        {
            picGraphView.Invalidate();
            if (MyOpenFileDialog.ShowDialog() == DialogResult.OK)
            {
                //Gọi hàm lưu đồ thị
                PtColor.Clear();
                myGraph.ReadGraph(Pt, segment, lvMatrixView, ChangeColorBrightness(Color.LightGreen, 0.6F), rbtnDirected, rbtnUnDirected, cbStartPoint, cbEndPoint, out i_th, MyOpenFileDialog.FileName);
                txbLogs.Text += "[" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + "] Graph open!\n";
                lvTableView.Clear();
                segment_dijkstra.Clear();
            }
            picGraphView.Invalidate();
        }

        //Sự kiện nhấn button mở file lưu đồ thị
        private void btnOpenGraph_Click(object sender, EventArgs e)
        {
            Open();
        }

        //Sự kiện nhấn toolstrip mở file lưu đồ thị
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Open();
        }

        //Hàm update lại đồ thị
        private void Updates()
        {
            segment_dijkstra.Clear();
            segment_dijkstra_Review.Clear();
            segment_dijkstra_Review_tmp.Clear();
            segment_dijkstra_save.Clear();
            segment_dijkstra_save_tmp.Clear();
            picGraphView.Invalidate();
            lvTableView.GridLines = false;
            lvTableView.Clear();
            PtColor.Clear();
            myGraph.MatrixCreate(lvMatrixView, i_th - 1, ChangeColorBrightness(Color.LightGreen, 0.6F));
            txbLogDijkstra.Clear();
            txbLogs.Text += "[" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + "] Graph updated!\n";
        }

        //Sự kiện nhấn button cập nhật lại đồ thị
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            Updates();
        }

        //Sự kiện nhấn contextmenu cập nhật lại đồ thị
        private void updateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Updates();
        }

        //Sử dụng bitmap để lưu
        Bitmap SaveImage()
        {
            Bitmap bm = new Bitmap(picGraphView.Width, picGraphView.Height);
            Rectangle rect = new Rectangle(new Point(0, 0), new Size(bm.Width, bm.Height));
            picGraphView.DrawToBitmap(bm, rect);
            return bm;
        }

        //Sự kiện nhấn button để lưu đồ thị đươi dạng file ảnh .png
        private void saveAsImageToolStripMenuItem_Click(object sender, EventArgs e)
        {           
            if (SaveFileDialogImage.ShowDialog() == DialogResult.OK)
            {
                SaveImage().Save(SaveFileDialogImage.FileName, System.Drawing.Imaging.ImageFormat.Png);
                txbLogs.Text += "[" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + "] Graph save image!\n";
            }
        }

        //Sự kiện sao chép ảnh Graph vào clipboard
        private void copyGraphImageInClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetImage(SaveImage());
        }

        //Sự kiện lưu ma trận kề vào clipboard
        private void copyMatrixToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lvMatrixView.Items.Count == 0) MessageBox.Show("Nothing to Copy!", "Logs", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else myGraph.SaveMatrix(lvMatrixView, i_th - 1);
        }
        #endregion

        #region DijkstraSlove
        //Sự kiện tích và checkbox timesleep
        private void cbxTimeSleep_CheckedChanged(object sender, EventArgs e)
        {
            //Xử lý ẩn hiện các chức năng đi tho
            if (cbxTimeSleep.Checked) { pnlTimeSleep.Enabled = true; btnRunStepDijkstra.Enabled = false; }
            else { pnlTimeSleep.Enabled = false; btnRunStepDijkstra.Enabled = true; }
        }

        //Sự kiện nhấn button chạy dijkstra
        private void btnRunDijkstra_Click(object sender, EventArgs e)
        {
            //Nếu đều đã chọn giá trị bắt đầu và kết thúc
            if (cbStartPoint.SelectedIndex != -1 && cbEndPoint.SelectedIndex != -1)
            {
                //Nếu có sử dụng timesleep
                if (cbxTimeSleep.Checked)
                {
                    //Timesleep không áp dụng cho chạy từ một điểm đến tất cả
                    if (cbEndPoint.SelectedIndex != 0)
                    {
                        if (btnRunDijkstra.Text == "Run")
                        {
                            //Kiểm tra tính hợp lệ của giá trị timesleep
                            try
                            {
                                int timesleep = Convert.ToInt16(txbTimeSleep.Text);
                                if (timesleep >= 0)
                                {
                                    //Thiết lập tên button và hình ảnh
                                    btnRunDijkstra.Text = "Stop";
                                    btnRunDijkstra.Image = Properties.Resources.Pause;
                                    //Tạo vòng lặp
                                    isTimeSleep = true;
                                    //Tạo luồng xử lý riêng
                                    new Thread(
                                        () =>
                                        {
                                            while (isTimeSleep)
                                            {
                                                DijkstraStep();
                                                Thread.Sleep(timesleep);
                                            }
                                        }
                                        )
                                    { IsBackground = true }.Start();
                                }
                                else MessageBox.Show("Time Sleep not correct!", "Logs", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            catch (Exception)
                            {
                                MessageBox.Show("Time Sleep not correct!", "Logs", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        else
                        {
                            //Trả về mặc định khi nhấn lần nữa
                            btnRunDijkstra.Text = "Run";
                            btnRunDijkstra.Image = Properties.Resources.Play;
                            dijkstra_step = true;
                            isTimeSleep = false;
                            segment_dijkstra_save.Clear();
                            segment_dijkstra_Review_tmp.Clear();
                            segment_dijkstra_save_tmp.Clear();
                            segment_dijkstra_Review.Clear();
                            picGraphView.Invalidate();
                        }
                    }
                    else MessageBox.Show("Run with time sleep aren't available for Point to All", "Logs", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                //Nếu không sử dụng timesleep
                else
                {
                    try
                    {
                        //Chạy hàm dijkstra cho all hoặc đơn lẻ
                        if (cbEndPoint.SelectedIndex == 0) dijkstra.DijkstraAll(lvMatrixView, lvTableView, i_th, cbStartPoint.SelectedIndex, out isPaths, txbLogDijkstra, Pt, segment, segment_dijkstra, rbtnUnDirected.Checked);
                        else dijkstra.DijkstraSimple(lvMatrixView, lvTableView, i_th, cbStartPoint.SelectedIndex, cbEndPoint.SelectedIndex - 1, out isPaths, txbLogDijkstra, Pt, segment, segment_dijkstra, segment_dijkstra_save_tmp, segment_dijkstra_Review_tmp, rbtnUnDirected.Checked);
                        //Tạo bảng
                        myGraph.TableView(lvTableView, ChangeColorBrightness(Color.LightSkyBlue, 0.7F));
                        if (isPaths)
                        {
                            segment_dijkstra_Review_tmp.Clear();
                            segment_dijkstra_save_tmp.Clear();
                            segment_dijkstra_Review.Clear();
                            segment_dijkstra_save.Clear();
                            picGraphView.Invalidate();
                        }
                        else MessageBox.Show("Error!", "Logs", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Error!", "Logs", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else MessageBox.Show("Start point or End point isn't choose!", "Logs", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        //hàm chạy từng vược dijkstra
        private void DijkstraStep()
        {
            //Refresh lại
            picGraphView.Invalidate();
            if (dijkstra_step)
            {
                //Chỉ cần chạy mội lần lấy giá trị cho các biến list tạm
                segment_dijkstra.Clear();
                lvTableView.Clear();
                segment_dijkstra_Review.Clear();
                segment_dijkstra_save.Clear();
                dijkstra.DijkstraSimple(lvMatrixView, lv, i_th, cbStartPoint.SelectedIndex, cbEndPoint.SelectedIndex - 1, out isPaths, txbLogDijkstra, Pt, segment, segment_dijkstra, segment_dijkstra_save_tmp, segment_dijkstra_Review_tmp, rbtnUnDirected.Checked);
                for (int i = 1; i < i_th; ++i) lvTableView.Columns.Add(i.ToString(), 80);
            }

            //thêm hàng cho bảng
            if (lv.Items.Count != 0)
            {
                ListViewItem item = lv.Items[0];
                lv.Items.RemoveAt(0);
                lvTableView.Items.Add(item);
            }

            //Nếu có đường đi thì chạy từng bước theo lần lượt nhấn
            if (isPaths)
            {
                if (!dijkstra_step)
                {
                    if (segment_dijkstra_Review_tmp.Count != 0)
                    {
                        segment_dijkstra_Review.Clear();
                        segment_dijkstra_Review.Add(segment_dijkstra_Review_tmp[0]);
                        segment_dijkstra_Review_tmp.RemoveAt(0);
                    }
                }

                dijkstra_step = false;

                if (segment_dijkstra_save_tmp.Count != 0)
                {
                    segment_dijkstra_save.Add(segment_dijkstra_save_tmp[0]);
                    segment_dijkstra_save_tmp.RemoveAt(0);
                    picGraphView.Invalidate();
                }

                if (segment_dijkstra_save_tmp.Count == 0)
                {
                    segment_dijkstra_save.Clear();
                    segment_dijkstra_Review.Clear();
                    picGraphView.Invalidate();
                    //Khi chạy xong hiện thông báo
                    MessageBox.Show("Done!", "Logs", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    //khôi phục lại mặc định
                    dijkstra_step = true;
                    isTimeSleep = false;
                    btnRunDijkstra.Text = "Run";
                    btnRunDijkstra.Image = Properties.Resources.Play;
                }
            }
            else
            {
                //Báo lỗi và rest lại các thuộc tính
                MessageBox.Show("Error!", "Logs", MessageBoxButtons.OK, MessageBoxIcon.Error);
                segment_dijkstra_save.Clear();
                segment_dijkstra_Review.Clear();
                picGraphView.Invalidate();
                dijkstra_step = true;
                isTimeSleep = false;
            }

            //Tô màu cho lvTableView
            myGraph.TableView(lvTableView, ChangeColorBrightness(Color.LightSkyBlue, 0.7F));
        }

        //Sự kiện chạy dijkstra từng bước
        private void btnRunStepDijkstra_Click(object sender, EventArgs e)
        {
            //Kiểm tra đã chọn điểm bắt đàu và điểm kết thúc chưa
            if (cbStartPoint.SelectedIndex != -1 && cbEndPoint.SelectedIndex != -1)
            {
                try
                {
                    if (cbEndPoint.SelectedIndex == 0) MessageBox.Show("Run step aren't available for Point to All", "Logs", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    else if (isPaths) DijkstraStep();
                    else MessageBox.Show("Error!", "Logs", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception)
                {
                    MessageBox.Show("Error!", "Logs", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else MessageBox.Show("Start point or End point isn't choose!", "Logs", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        #endregion

        //Hàm chỉnh độ sáng màu sắc
        //Hàm này copy trên mạng :D
        public static Color ChangeColorBrightness(Color color, float correctionFactor)
        {
            float red = (float)color.R;
            float green = (float)color.G;
            float blue = (float)color.B;

            if (correctionFactor < 0)
            {
                correctionFactor = 1 + correctionFactor;
                red *= correctionFactor;
                green *= correctionFactor;
                blue *= correctionFactor;
            }
            else
            {
                red = (255 - red) * correctionFactor + red;
                green = (255 - green) * correctionFactor + green;
                blue = (255 - blue) * correctionFactor + blue;
            }

            return Color.FromArgb(color.A, (int)red, (int)green, (int)blue);
        }    

        //Sự kiện sau khi load xong form
        private void Form1_Shown(object sender, EventArgs e)
        {
            //Hiểm thị form nhỏ chọn kiểu đồ thị       
            Welcome wc = new Welcome();
            if (wc.ChooseGraphType(" Welcome") == DialogResult.OK)
            {
                if (wc.Value) rbtnDirected.Checked = true;
                else rbtnUnDirected.Checked = true;
                this.Enabled = true;
            }
            else this.Close(); //Tắt form nhỏ là tắt luôn chương trình       
        }

        #region Tested
        //Sự kiện xử lý lộ trình đi đề xuất
        private void btnProcessing_Click(object sender, EventArgs e)
        {
            segment_dijkstra.Clear();
            fpnlTested.Controls.Clear();
            num_th = 0;
            tested.Clear();
            lbRecommed.Text = "Recommended";

            picGraphView.Invalidate();

            //Kiểm tra xem xem dữ liệu có đủ hay không ?
            if (txbListLocation.Text == "" || txbStarting.Text == "")
                MessageBox.Show("Error", "Logs", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
            {
                this.Cursor = Cursors.WaitCursor;

                //Sử lý tạo một danh sách các điểm đi
                while (txbListLocation.Text.IndexOf("  ") != -1) txbListLocation.Text = txbListLocation.Text.Replace("  ", " ");

                string[] listlocation = txbListLocation.Text.Trim().Split(' ');

                List<int> listPointLocation = new List<int>();                
                List<int> road = new List<int>();              
                List<Segment> segment_tested = new List<Segment>();

                try
                {
                    bool isPathTested = true;
                    int s = int.Parse(txbStarting.Text) - 1;

                    foreach (string i in listlocation) listPointLocation.Add(int.Parse(i) - 1);

                    //Khởi tạo điểm bắt đầu và tổng trọng số
                    int nearpoint = s;
                    int res = 0;

                    //Chạy hàm thuật toán chạy cho đề tài thực tế
                    Tested(listPointLocation, road, nearpoint, res, isPathTested, segment_tested);

                    lbRecommed.Text += ": " + num_th.ToString() + " Routes";

                }
                catch (Exception)
                {
                    MessageBox.Show("Error!", "Logs", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                this.Cursor = Cursors.Default;
            }
        }

        private void btnClearTested_Click(object sender, EventArgs e)
        {
            txbStarting.Clear();
            txbListLocation.Clear();
            num_th = 0;
            tested.Clear();
            lbRecommed.Text = "Recommended";
            fpnlTested.Controls.Clear();
            segment_dijkstra.Clear();

            picGraphView.Invalidate();
        }

        //Hàm chuyển item có trong list b sang list a
        private void changelistInt(List<int> a, List<int> b)
        {
            foreach (int item in b)
                a.Add(item);
        }

        //Hàm chuyển item có trong list b sang list a
        private void changelistSegment(List<Segment> a, List<Segment> b)
        {
            foreach (Segment item in b)
                a.Add(item);
        }

        private int num_th = 0;

        //Hàm thuật toán xử lý cho đề tài thực tế 
        private void Tested(List<int> _listPointLocation, List<int> _road, int _nearpoint, int _res, bool _isPathTested, List<Segment> _segments)
        {
            _road.Add(_nearpoint + 1);

            //Danh sách điểm đã hết thì ngưng
            while (_listPointLocation.Count != 0)
            {
                int Mins = 99999999;

                int start = _nearpoint;

                int index = -1;

                List<int> tmpPoint = new List<int>();
                List<int> tmpLength = new List<int>();

                //Bắt đầu so sánh từng điểm để chọn đoạn đi gần nhất
                for(int i = 0; i < _listPointLocation.Count; ++i)
                {
                    int length = dijkstra.DijkstraForTested(lvMatrixView, out _isPathTested, i_th, start, _listPointLocation[i]);

                    tmpPoint.Add(_listPointLocation[i]);
                    tmpLength.Add(length);

                    if (Mins > length)
                    {
                        Mins = length;
                        _nearpoint = _listPointLocation[i];
                        index = i;
                    }
                }

                //Nếu có trùng về trọng số gần nhất của hai địa điểm khác nhau
                for(int i = 0; i < tmpPoint.Count; ++i)
                {
                    //Xét hết
                    if(tmpPoint[i] != _nearpoint && tmpLength[i] == Mins)
                    {
                        //Danh sách biến tạm cho đệ quy
                        List<int> tmpListpoint = new List<int>();
                        List<int> tmpRoad = new List<int>();
                        List<Segment> tmpSegments = new List<Segment>();
                        changelistInt(tmpListpoint, _listPointLocation);
                        changelistInt(tmpRoad, _road);
                        changelistSegment(tmpSegments, _segments);
                        tmpListpoint.RemoveAt(i);
                        int tmpRes = _res;
                        bool tmpIsPaths = _isPathTested;
                        tmpRes += tmpLength[i];
                        List<Segment> _segments_test = new List<Segment>();
                        dijkstra.DijkstraSimple(lvMatrixView, new ListView(), i_th, start, tmpPoint[i], out _isPathTested, new RichTextBox(), Pt, segment, _segments_test, segment_dijkstra_save_tmp, segment_dijkstra_Review_tmp, rbtnUnDirected.Checked);
                        lvTableView.Clear();
                        changelistSegment(tmpSegments, _segments_test);

                        _segments_test.Clear();
                        segment_dijkstra_save_tmp.Clear();
                        segment_dijkstra_Review_tmp.Clear();
               
                        Tested(tmpListpoint, tmpRoad, tmpPoint[i], tmpRes, tmpIsPaths, tmpSegments);
                    }
                }

                //Có được đỉnh gần nhất, xóa đỉnh đó ra khỏi danh sách rồi tiếp tục xét từ đỉnh đó đến các đỉnh còn lại
                _listPointLocation.RemoveAt(index);

                //Thêm đỉnh đó zô danh sách đường đi để xuất ra
                _road.Add(_nearpoint + 1);

                _res += Mins;

                //Chạy dijkstra để lấy danh sách lưu graphic vẽ
                List<Segment> segments_test = new List<Segment>();
                dijkstra.DijkstraSimple(lvMatrixView, new ListView(), i_th, start, _nearpoint, out _isPathTested, new RichTextBox(), Pt, segment, segments_test, segment_dijkstra_save_tmp, segment_dijkstra_Review_tmp, rbtnUnDirected.Checked);
                lvTableView.Clear();
                changelistSegment(_segments, segments_test);

                //Dọn dẹp
                segments_test.Clear();
                segment_dijkstra_save_tmp.Clear();
                segment_dijkstra_Review_tmp.Clear();
            }

            //Thêm vào danh sách các đề xuất lộ trình
            tested.Add(_segments);

            //Tạo một textbox và button để phục vụ hiển thị đề xuất
            #region createControl
            Panel pnl = new Panel();
            pnl.AutoSize = true;
            pnl.BorderStyle = BorderStyle.FixedSingle;
            RichTextBox txb = new RichTextBox();
            txb.Width = 327;
            txb.WordWrap = false;
            txb.BorderStyle = BorderStyle.None;
            txb.Location = new Point(3, 3);           
            txb.ReadOnly = true;
            txb.BackColor = Color.GhostWhite;
            txb.ScrollBars = RichTextBoxScrollBars.None;
            txb.MouseLeave += Txb_MouseLeave;
            txb.MouseHover += Txb_MouseHover;
            Button btn = new Button();
            btn.Tag = ++num_th - 1;
            btn.Size = new Size(27, 27);
            btn.Location = new Point(330, 3);
            btn.UseVisualStyleBackColor = true;
            btn.FlatStyle = FlatStyle.Standard;
            btn.ForeColor = Color.Black;
            btn.BackgroundImage = Properties.Resources.Eye;
            btn.BackgroundImageLayout = ImageLayout.Zoom;
            toolTip1.SetToolTip(btn, "Click to show Graph display!");
            toolTip1.SetToolTip(txb, "Recommended Route");
            btn.Click += Btn_Click;
            pnl.Controls.Add(txb);
            pnl.Controls.Add(btn);
            fpnlTested.Controls.Add(pnl);
            #endregion

            //Hiển thị đường đi trong textbox
            for (int i = 0; i < _road.Count - 1; ++i) txb.Text += _road[i].ToString() + " -> ";
            txb.Text += _road[_road.Count - 1].ToString();
            txb.Text += "\n" + "Length = " + _res.ToString();

            using (Graphics g = CreateGraphics())
                txb.Height = (int)g.MeasureString(txb.Text, txb.Font, txb.Width).Height;
        }

        //Sự kiện ẩn ScrollBars khi rời chuột khỏi 
        private void Txb_MouseLeave(object sender, EventArgs e)
        {
            (sender as RichTextBox).ScrollBars = RichTextBoxScrollBars.None;
        }

        //Sự kiện hiện ScrollBars khi rời chuột khỏi 
        private void Txb_MouseHover(object sender, EventArgs e)
        {
            (sender as RichTextBox).ScrollBars = RichTextBoxScrollBars.Horizontal;
        }

        //Sự kiện nhất button để hiển thị đường đi trên Graph
        private void Btn_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            segment_dijkstra.Clear();

            //Tham chiếu đề button đã tạo event
            Button btn = sender as Button;
            int index = (int)btn.Tag;

            //Lưu vào mảng dùng để vẽ đường đi
            changelistSegment(segment_dijkstra, tested[index]);
            segment_dijkstra_save_tmp.Clear();
            segment_dijkstra_Review_tmp.Clear();

            picGraphView.Invalidate();

            this.Cursor = Cursors.Default;
        }
        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {
            //Hiệu ứng thử nghiệm xíu :D
            WinAPI.AnimateWindow(this.Handle, 50, WinAPI.CENTER);
            cbxGridLine.Checked = true;
        } 
    }
}
