// PatientUI/FrmMedicineReminder.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BLL;
using Model;

namespace PatientUI
{
    public partial class FrmMedicineReminder : Form
    {
        private readonly int _userId;
        private readonly B_MedicineReminder _bllReminder = new B_MedicineReminder();
        private DataGridView _dgvReminder;

        public FrmMedicineReminder(int userId)
        {
            InitializeComponent();
            _userId = userId;
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Text = "用药提醒管理";
            this.Size = new Size(720, 520);
            this.BackColor = Color.White;
            this.Font = new Font("微软雅黑", 9F);
            this.Load += FrmMedicineReminder_Load;
        }

        private void FrmMedicineReminder_Load(object sender, EventArgs e)
        {
            InitializeControls();
            LoadReminderData();
        }

        private void InitializeControls()
        {
            // 表格
            _dgvReminder = new DataGridView
            {
                Name = "dgvReminder",
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                GridColor = Color.FromArgb(220, 220, 220),
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 40,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };

            _dgvReminder.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "reminder_id", HeaderText = "ID", DataPropertyName = "reminder_id", Visible = false },
                new DataGridViewTextBoxColumn { Name = "colDrugName", HeaderText = "药物名称", DataPropertyName = "drug_name", Width = 120 },
                new DataGridViewTextBoxColumn { Name = "colDosage", HeaderText = "剂量", DataPropertyName = "drug_dosage", Width = 80 },
                new DataGridViewTextBoxColumn { Name = "colTakeWay", HeaderText = "用药方式", DataPropertyName = "take_way", Width = 80 },
                new DataGridViewTextBoxColumn { Name = "colTime", HeaderText = "提醒时间", DataPropertyName = "reminder_time", Width = 80 },
                new DataGridViewCheckBoxColumn { Name = "colEnabled", HeaderText = "启用", DataPropertyName = "is_enabled", Width = 60 },
                new DataGridViewButtonColumn { Name = "colEdit", HeaderText = "编辑", Text = "编辑", UseColumnTextForButtonValue = true, Width = 60 },
                new DataGridViewButtonColumn { Name = "colDelete", HeaderText = "删除", Text = "删除", UseColumnTextForButtonValue = true, Width = 60 }
            });

            _dgvReminder.DefaultCellStyle.Font = new Font("微软雅黑", 9F);
            _dgvReminder.ColumnHeadersDefaultCellStyle.Font = new Font("微软雅黑", 9F, FontStyle.Bold);
            _dgvReminder.RowTemplate.Height = 34;
            _dgvReminder.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            _dgvReminder.CellContentClick += (s, e) =>
            {
                if (e.RowIndex < 0) return;

                if (e.ColumnIndex == _dgvReminder.Columns["colEdit"].Index)
                {
                    EditReminder(e.RowIndex);
                }
                else if (e.ColumnIndex == _dgvReminder.Columns["colDelete"].Index)
                {
                    DeleteReminder(e.RowIndex);
                }
            };

            // 按钮区
            var btnPanel = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 56, Padding = new Padding(0, 6, 0, 6), FlowDirection = FlowDirection.RightToLeft };
            var btnClose = new Button { Text = "关闭", Width = 100, Height = 35, Margin = new Padding(10) };
            var btnAdd = new Button { Text = "添加提醒", Width = 100, Height = 35, Margin = new Padding(10), BackColor = Color.FromArgb(0, 122, 204), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };

            btnAdd.Click += (s, e) => { AddReminder(); };
            btnClose.Click += (s, e) => { this.Close(); };

            btnPanel.Controls.Add(btnClose);
            btnPanel.Controls.Add(btnAdd);

            this.Controls.Add(_dgvReminder);
            this.Controls.Add(btnPanel);
        }

        private void LoadReminderData()
        {
            try
            {
                var reminderList = _bllReminder.GetUserReminders(_userId);
                _dgvReminder.DataSource = null;
                _dgvReminder.DataSource = reminderList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载用药提醒失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddReminder()
        {
            using (var frm = new FrmAddEditReminder(_userId))
            {
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    LoadReminderData();
                }
            }
        }

        private void EditReminder(int rowIndex)
        {
            var row = _dgvReminder.Rows[rowIndex];
            if (!TryGetReminderId(row, out int reminderId))
            {
                MessageBox.Show("提醒ID无效，无法编辑该提醒记录", "操作提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var reminder = new MedicineReminder
            {
                reminder_id = reminderId,
                user_id = _userId,
                drug_name = row.Cells["colDrugName"].Value.ToString(),
                drug_dosage = row.Cells["colDosage"].Value.ToString(),
                take_way = row.Cells["colTakeWay"].Value.ToString(),
                reminder_time = row.Cells["colTime"].Value.ToString(),
                is_enabled = Convert.ToBoolean(row.Cells["colEnabled"].Value)
            };

            using (var frm = new FrmAddEditReminder(_userId, reminder))
            {
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    LoadReminderData();
                }
            }
        }

        private void DeleteReminder(int rowIndex)
        {
            var row = _dgvReminder.Rows[rowIndex];
            if (!TryGetReminderId(row, out int reminderId))
            {
                MessageBox.Show("提醒ID无效，无法删除该提醒记录", "操作提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string drugName = row.Cells["colDrugName"].Value.ToString();

            if (MessageBox.Show($"确定要删除\"{drugName}\"的用药提醒吗？", "确认删除",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            var result = _bllReminder.DeleteReminder(reminderId, _userId);
            if (result.Success)
            {
                MessageBox.Show(result.Msg, "删除成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadReminderData();
            }
            else
            {
                MessageBox.Show(result.Msg, "删除失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool TryGetReminderId(DataGridViewRow row, out int reminderId)
        {
            reminderId = 0;
            if (row == null)
            {
                return false;
            }

            object idValue = row.Cells["reminder_id"]?.Value;
            if (idValue == null || !int.TryParse(idValue.ToString(), out reminderId) || reminderId <= 0)
            {
                return false;
            }

            return true;
        }
    }
}