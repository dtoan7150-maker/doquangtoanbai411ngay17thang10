using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Bai411_QLDKDVSC
{
    public partial class Form1 : Form
    {
        private readonly BindingList<RepairOrder> _masterList = new BindingList<RepairOrder>();
        private readonly BindingSource _bindingSource = new BindingSource();
        private readonly ErrorProvider _errorProvider = new ErrorProvider { BlinkStyle = ErrorBlinkStyle.NeverBlink };
        private string _selectedImagePath = string.Empty;

        private MenuStrip menuStrip1 = null!;
        private StatusStrip statusStrip1 = null!;
        private ToolStripStatusLabel lblCountStatus = null!;
        private ToolStripStatusLabel lblPendingCostStatus = null!;
        private TableLayoutPanel mainTableLayout = null!;
        private GroupBox gbInput = null!;
        private TableLayoutPanel inputTableLayout = null!;
        private TextBox txtOrderCode = null!, txtCustomerName = null!, txtPhoneNumber = null!, txtErrorDescription = null!, txtSearch = null!;
        private ComboBox cboDeviceType = null!, cboStatus = null!, cboFilterStatus = null!;
        private DateTimePicker dtpReceivedDate = null!;
        private NumericUpDown nudEstimatedCost = null!;
        private Button btnAdd = null!, btnUpdate = null!, btnDelete = null!, btnClear = null!, btnSelectImage = null!;
        private GroupBox gbImage = null!;
        private PictureBox picDeviceImage = null!;
        private DataGridView dgvOrders = null!;

        public Form1()
        {
            InitializeComponent();
            BuildUI();
            LoadInitialData();
            ApplyFilterAndSearch();
        }

        private void BuildUI()
        {
            this.SuspendLayout();

            // Menu
            menuStrip1 = new MenuStrip();
            var mnuFile = new ToolStripMenuItem("Tệp tin");
            mnuFile.DropDownItems.Add(new ToolStripMenuItem("Xuất CSV", null, mnuExportCsv_Click));
            mnuFile.DropDownItems.Add(new ToolStripSeparator());
            mnuFile.DropDownItems.Add(new ToolStripMenuItem("Thoát", null, mnuExit_Click));
            menuStrip1.Items.Add(mnuFile);
            this.Controls.Add(menuStrip1);
            this.MainMenuStrip = menuStrip1;

            // Status
            statusStrip1 = new StatusStrip();
            lblCountStatus = new ToolStripStatusLabel("Số đơn: 0");
            lblPendingCostStatus = new ToolStripStatusLabel("Tổng chi phí: 0 VNĐ") { Margin = new Padding(20, 3, 0, 2) };
            statusStrip1.Items.Add(lblCountStatus);
            statusStrip1.Items.Add(lblPendingCostStatus);
            this.Controls.Add(statusStrip1);

            // Main Layout
            mainTableLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 3 };
            mainTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68F));
            mainTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32F));
            mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 280F));
            mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45F));
            mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this.Controls.Add(mainTableLayout);
            mainTableLayout.BringToFront();

            // Input Controls
            gbInput = new GroupBox { Text = "Thông tin đơn sửa chữa", Dock = DockStyle.Fill };
            inputTableLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 6 };
            inputTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            inputTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            inputTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            inputTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            inputTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            inputTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            inputTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            inputTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            inputTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
            inputTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            txtOrderCode = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            txtCustomerName = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            txtPhoneNumber = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };

            cboDeviceType = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Anchor = AnchorStyles.Left | AnchorStyles.Right };
            cboDeviceType.Items.AddRange(new object[] { "Điện thoại", "Laptop", "Máy tính bảng", "Máy tính bàn", "Khác" });
            cboDeviceType.SelectedIndex = 0;

            dtpReceivedDate = new DateTimePicker { Format = DateTimePickerFormat.Short, Anchor = AnchorStyles.Left | AnchorStyles.Right };
            nudEstimatedCost = new NumericUpDown { Maximum = 1000000000m, Increment = 50000m, ThousandsSeparator = true, Anchor = AnchorStyles.Left | AnchorStyles.Right };

            string[] statusList = new string[] { "Mới tiếp nhận", "Đang sửa", "Chờ linh kiện", "Hoàn tất", "Đã hủy" };
            cboStatus = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Anchor = AnchorStyles.Left | AnchorStyles.Right };
            cboStatus.Items.AddRange(statusList);
            cboStatus.SelectedIndex = 0;

            txtErrorDescription = new TextBox { Multiline = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill };

            inputTableLayout.Controls.Add(new Label { Text = "Mã đơn:", Anchor = AnchorStyles.Left }, 0, 0);
            inputTableLayout.Controls.Add(txtOrderCode, 1, 0);
            inputTableLayout.Controls.Add(new Label { Text = "Khách hàng:", Anchor = AnchorStyles.Left }, 2, 0);
            inputTableLayout.Controls.Add(txtCustomerName, 3, 0);

            inputTableLayout.Controls.Add(new Label { Text = "Số điện thoại:", Anchor = AnchorStyles.Left }, 0, 1);
            inputTableLayout.Controls.Add(txtPhoneNumber, 1, 1);
            inputTableLayout.Controls.Add(new Label { Text = "Loại thiết bị:", Anchor = AnchorStyles.Left }, 2, 1);
            inputTableLayout.Controls.Add(cboDeviceType, 3, 1);

            inputTableLayout.Controls.Add(new Label { Text = "Ngày nhận:", Anchor = AnchorStyles.Left }, 0, 2);
            inputTableLayout.Controls.Add(dtpReceivedDate, 1, 2);
            inputTableLayout.Controls.Add(new Label { Text = "Chi phí:", Anchor = AnchorStyles.Left }, 2, 2);
            inputTableLayout.Controls.Add(nudEstimatedCost, 3, 2);

            inputTableLayout.Controls.Add(new Label { Text = "Trạng thái:", Anchor = AnchorStyles.Left }, 0, 3);
            inputTableLayout.Controls.Add(cboStatus, 1, 3);

            inputTableLayout.Controls.Add(new Label { Text = "Mô tả lỗi:", Anchor = AnchorStyles.Left }, 0, 4);
            inputTableLayout.Controls.Add(txtErrorDescription, 1, 4);
            inputTableLayout.SetColumnSpan(txtErrorDescription, 3);

            var pnlBtns = new FlowLayoutPanel { Dock = DockStyle.Fill };
            btnAdd = new Button { Text = "Thêm", Size = new Size(80, 32) };
            btnUpdate = new Button { Text = "Cập nhật", Size = new Size(80, 32) };
            btnDelete = new Button { Text = "Xóa", Size = new Size(80, 32) };
            btnClear = new Button { Text = "Làm mới", Size = new Size(80, 32) };

            btnAdd.Click += btnAdd_Click;
            btnUpdate.Click += btnUpdate_Click;
            btnDelete.Click += btnDelete_Click;
            btnClear.Click += btnClear_Click;

            pnlBtns.Controls.Add(btnAdd);
            pnlBtns.Controls.Add(btnUpdate);
            pnlBtns.Controls.Add(btnDelete);
            pnlBtns.Controls.Add(btnClear);
            inputTableLayout.Controls.Add(pnlBtns, 0, 5);
            inputTableLayout.SetColumnSpan(pnlBtns, 4);

            gbInput.Controls.Add(inputTableLayout);
            mainTableLayout.Controls.Add(gbInput, 0, 0);

            // Image Section
            gbImage = new GroupBox { Text = "Ảnh thiết bị", Dock = DockStyle.Fill };
            var imgLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            imgLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            imgLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));

            picDeviceImage = new PictureBox { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom };
            btnSelectImage = new Button { Text = "Chọn ảnh...", Size = new Size(100, 30), Anchor = AnchorStyles.None };
            btnSelectImage.Click += btnSelectImage_Click;

            imgLayout.Controls.Add(picDeviceImage, 0, 0);
            imgLayout.Controls.Add(btnSelectImage, 0, 1);
            gbImage.Controls.Add(imgLayout);
            mainTableLayout.Controls.Add(gbImage, 1, 0);

            // Filter Section
            var pnlFilter = new Panel { Dock = DockStyle.Fill };
            pnlFilter.Controls.Add(new Label { Text = "Tìm kiếm:", Location = new Point(10, 12), AutoSize = true });
            txtSearch = new TextBox { Location = new Point(80, 9), Width = 200 };
            txtSearch.TextChanged += (s, e) => ApplyFilterAndSearch();
            pnlFilter.Controls.Add(txtSearch);

            pnlFilter.Controls.Add(new Label { Text = "Lọc trạng thái:", Location = new Point(310, 12), AutoSize = true });
            cboFilterStatus = new ComboBox { Location = new Point(410, 8), Width = 160, DropDownStyle = ComboBoxStyle.DropDownList };
            cboFilterStatus.Items.Add("Tất cả");
            cboFilterStatus.Items.AddRange(statusList);
            cboFilterStatus.SelectedIndex = 0;
            cboFilterStatus.SelectedIndexChanged += (s, e) => ApplyFilterAndSearch();
            pnlFilter.Controls.Add(cboFilterStatus);

            mainTableLayout.Controls.Add(pnlFilter, 0, 1);
            mainTableLayout.SetColumnSpan(pnlFilter, 2);

            // Grid Section
            dgvOrders = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "OrderCode", HeaderText = "Mã đơn", Width = 90 });
            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CustomerName", HeaderText = "Khách hàng", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "PhoneNumber", HeaderText = "SĐT", Width = 110 });
            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "DeviceType", HeaderText = "Loại TB", Width = 110 });
            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ReceivedDate", HeaderText = "Ngày nhận", Width = 100, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" } });
            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "EstimatedCost", HeaderText = "Chi phí", Width = 120, DefaultCellStyle = new DataGridViewCellStyle { Format = "N0", Alignment = DataGridViewContentAlignment.MiddleRight } });
            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "Trạng thái", Width = 110 });

            dgvOrders.DataSource = _bindingSource;
            dgvOrders.SelectionChanged += dgvOrders_SelectionChanged;

            mainTableLayout.Controls.Add(dgvOrders, 0, 2);
            mainTableLayout.SetColumnSpan(dgvOrders, 2);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void LoadInitialData()
        {
            _masterList.Add(new RepairOrder { OrderCode = "HD001", CustomerName = "Nguyễn Văn A", PhoneNumber = "0901234567", DeviceType = "Laptop", ErrorDescription = "Máy không lên nguồn", ReceivedDate = DateTime.Now.AddDays(-2), EstimatedCost = 500000, Status = "Mới tiếp nhận" });
            _masterList.Add(new RepairOrder { OrderCode = "HD002", CustomerName = "Trần Thị B", PhoneNumber = "0987654321", DeviceType = "Điện thoại", ErrorDescription = "Vỡ màn hình cảm ứng", ReceivedDate = DateTime.Now.AddDays(-1), EstimatedCost = 1200000, Status = "Đang sửa" });
        }

        private void ApplyFilterAndSearch()
        {
            string kw = txtSearch.Text.Trim().ToLower();
            string st = cboFilterStatus.SelectedItem?.ToString() ?? "Tất cả";

            var filtered = _masterList.Where(o =>
                (string.IsNullOrEmpty(kw) || o.OrderCode.ToLower().Contains(kw) || o.CustomerName.ToLower().Contains(kw) || o.PhoneNumber.ToLower().Contains(kw)) &&
                (st == "Tất cả" || o.Status == st)
            ).ToList();

            _bindingSource.DataSource = new BindingList<RepairOrder>(filtered);

            decimal pendingSum = _masterList.Where(o => o.Status != "Hoàn tất").Sum(o => o.EstimatedCost);
            lblCountStatus.Text = "Số đơn hiển thị: " + filtered.Count;
            lblPendingCostStatus.Text = "Tổng chi phí chưa hoàn tất: " + pendingSum.ToString("N0") + " VNĐ";
        }

        private void dgvOrders_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvOrders.CurrentRow?.DataBoundItem is RepairOrder selected)
            {
                txtOrderCode.Text = selected.OrderCode;
                txtCustomerName.Text = selected.CustomerName;
                txtPhoneNumber.Text = selected.PhoneNumber;
                cboDeviceType.SelectedItem = selected.DeviceType;
                txtErrorDescription.Text = selected.ErrorDescription;
                dtpReceivedDate.Value = selected.ReceivedDate;
                nudEstimatedCost.Value = selected.EstimatedCost;
                cboStatus.SelectedItem = selected.Status;
                _selectedImagePath = selected.ImagePath;
                LoadImageToPictureBox(_selectedImagePath);
                _errorProvider.Clear();
            }
        }

        private void LoadImageToPictureBox(string path)
        {
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                picDeviceImage.Image = Image.FromStream(fs);
            }
            else picDeviceImage.Image = null;
        }

        private void btnSelectImage_Click(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog { Filter = "Hình ảnh (*.jpg;*.png)|*.jpg;*.png", Title = "Chọn ảnh thiết bị" };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _selectedImagePath = ofd.FileName;
                LoadImageToPictureBox(_selectedImagePath);
            }
        }

        private bool ValidateData(bool isUpdating)
        {
            bool ok = true;
            _errorProvider.Clear();
            string code = txtOrderCode.Text.Trim();

            if (string.IsNullOrEmpty(code)) { _errorProvider.SetError(txtOrderCode, "Mã đơn không để trống!"); ok = false; }
            else if (!isUpdating && _masterList.Any(o => o.OrderCode.Equals(code, StringComparison.OrdinalIgnoreCase))) { _errorProvider.SetError(txtOrderCode, "Mã đơn đã tồn tại!"); ok = false; }

            if (string.IsNullOrWhiteSpace(txtCustomerName.Text)) { _errorProvider.SetError(txtCustomerName, "Tên không để trống!"); ok = false; }
            if (string.IsNullOrWhiteSpace(txtPhoneNumber.Text)) { _errorProvider.SetError(txtPhoneNumber, "SĐT không để trống!"); ok = false; }
            if (string.IsNullOrWhiteSpace(txtErrorDescription.Text)) { _errorProvider.SetError(txtErrorDescription, "Mô tả lỗi bắt buộc!"); ok = false; }
            return ok;
        }

        private void btnAdd_Click(object? sender, EventArgs e)
        {
            if (!ValidateData(false)) return;
            _masterList.Add(new RepairOrder
            {
                OrderCode = txtOrderCode.Text.Trim(),
                CustomerName = txtCustomerName.Text.Trim(),
                PhoneNumber = txtPhoneNumber.Text.Trim(),
                DeviceType = cboDeviceType.SelectedItem?.ToString() ?? "",
                ErrorDescription = txtErrorDescription.Text.Trim(),
                ReceivedDate = dtpReceivedDate.Value,
                EstimatedCost = nudEstimatedCost.Value,
                Status = cboStatus.SelectedItem?.ToString() ?? "",
                ImagePath = _selectedImagePath
            });
            ApplyFilterAndSearch();
            MessageBox.Show("Thêm thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnUpdate_Click(object? sender, EventArgs e)
        {
            if (dgvOrders.CurrentRow?.DataBoundItem is not RepairOrder selectedOrder) return;
            if (!ValidateData(true)) return;

            selectedOrder.CustomerName = txtCustomerName.Text.Trim();
            selectedOrder.PhoneNumber = txtPhoneNumber.Text.Trim();
            selectedOrder.DeviceType = cboDeviceType.SelectedItem?.ToString() ?? "";
            selectedOrder.ErrorDescription = txtErrorDescription.Text.Trim();
            selectedOrder.ReceivedDate = dtpReceivedDate.Value;
            selectedOrder.EstimatedCost = nudEstimatedCost.Value;
            selectedOrder.Status = cboStatus.SelectedItem?.ToString() ?? "";
            selectedOrder.ImagePath = _selectedImagePath;

            _bindingSource.ResetBindings(false);
            ApplyFilterAndSearch();
            MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnDelete_Click(object? sender, EventArgs e)
        {
            if (dgvOrders.CurrentRow?.DataBoundItem is not RepairOrder selectedOrder) return;
            if (MessageBox.Show("Bạn có chắc muốn xóa đơn này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _masterList.Remove(selectedOrder);
                ApplyFilterAndSearch();
                ClearInputs();
            }
        }

        private void btnClear_Click(object? sender, EventArgs e) => ClearInputs();

        private void ClearInputs()
        {
            txtOrderCode.Clear();
            txtCustomerName.Clear();
            txtPhoneNumber.Clear();
            txtErrorDescription.Clear();
            dtpReceivedDate.Value = DateTime.Now;
            nudEstimatedCost.Value = 0;
            cboDeviceType.SelectedIndex = 0;
            cboStatus.SelectedIndex = 0;
            _selectedImagePath = string.Empty;
            picDeviceImage.Image = null;
            _errorProvider.Clear();
        }

        private void mnuExportCsv_Click(object? sender, EventArgs e)
        {
            if (_bindingSource.List.Count == 0) return;
            using var sfd = new SaveFileDialog { Filter = "CSV File (*.csv)|*.csv", FileName = "RepairOrders.csv" };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                var sb = new StringBuilder();
                sb.AppendLine("Mã đơn,Tên khách hàng,Số điện thoại,Loại thiết bị,Mô tả lỗi,Ngày nhận,Chi phí dự kiến,Trạng thái");
                foreach (RepairOrder item in _bindingSource.List)
                {
                    sb.AppendLine($"\"{item.OrderCode}\",\"{item.CustomerName}\",\"{item.PhoneNumber}\",\"{item.DeviceType}\",\"{item.ErrorDescription.Replace("\"", "\"\"")}\",\"{item.ReceivedDate:dd/MM/yyyy}\",\"{item.EstimatedCost}\",\"{item.Status}\"");
                }
                File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                MessageBox.Show("Xuất CSV thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void mnuExit_Click(object? sender, EventArgs e) => Close();
    }
}
