// PatientUI/FrmSelectBloodSugar.cs
using BLL;
using Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PatientUI
{
    public partial class FrmSelectBloodSugar : Form
    {
        private readonly int _userId;
        private readonly B_BloodSugar _bllBloodSugar = new B_BloodSugar();

        public int SelectedBloodSugarId { get; private set; }

        public FrmSelectBloodSugar(int userId)
        {
            InitializeComponent();
            _userId = userId;
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Text = "选择血糖记录";
            this.Size = new Size(600, 400);
            this.Load += FrmSelectBloodSugar_Load;
        }

        private void FrmSelectBloodSugar_Load(object sender, EventArgs e)
        {
            InitializeControls();
            LoadBloodSugarData();
        }

        private void InitializeControls()
        {
            var dgv = new DataGridView
            {
                Name = "dgvBloodSugar",
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };

            dgv.Columns.AddRange(new[]
            {
                new DataGridViewTextBoxColumn { Name = "blood_sugar_id", HeaderText = "ID", DataPropertyName = "blood_sugar_id", Visible = false },
                new DataGridViewTextBoxColumn { Name = "colValue", HeaderText = "血糖值(mmol/L)", DataPropertyName = "blood_sugar_value", Width = 120 },
                new DataGridViewTextBoxColumn { Name = "colTime", HeaderText = "测量时间", DataPropertyName = "measurement_time", Width = 150 },
                new DataGridViewTextBoxColumn { Name = "colScenario", HeaderText = "测量场景", DataPropertyName = "measurement_scenario", Width = 100 }
            });

            dgv.DefaultCellStyle.Font = new Font("微软雅黑", 9F);
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("微软雅黑", 9F, FontStyle.Bold);
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgv.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                ConfirmSelection(e.RowIndex);
            };

            var btnPanel = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 50, FlowDirection = FlowDirection.RightToLeft };
            var btnCancel = new Button { Text = "取消", Width = 100, Height = 35, Margin = new Padding(10) };
            var btnConfirm = new Button { Text = "确定", Width = 100, Height = 35, Margin = new Padding(10) };

            btnConfirm.Click += (s, e) =>
            {
                if (dgv.SelectedRows.Count == 0)
                {
                    MessageBox.Show("请选择一条血糖记录", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                ConfirmSelection(dgv.SelectedRows[0].Index);
            };

            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; };

            btnPanel.Controls.Add(btnCancel);
            btnPanel.Controls.Add(btnConfirm);

            this.Controls.Add(dgv);
            this.Controls.Add(btnPanel);
        }

        private void LoadBloodSugarData()
        {
            try
            {
                var bloodSugarList = _bllBloodSugar.GetUserBloodSugarList(_userId);
                var dgv = this.Controls.Find("dgvBloodSugar", true).FirstOrDefault() as DataGridView;

                if (dgv == null) return;

                dgv.DataSource = bloodSugarList.Select(bs => new
                {
                    bs.blood_sugar_id,
                    bs.blood_sugar_value,
                    measurement_time = bs.measurement_time?.ToString("yyyy-MM-dd HH:mm") ?? "",
                    bs.measurement_scenario
                }).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载血糖记录失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.Cancel;
            }
        }

        private void ConfirmSelection(int rowIndex)
        {
            var dgv = this.Controls.Find("dgvBloodSugar", true).FirstOrDefault() as DataGridView;
            if (dgv == null) return;
            var row = dgv.Rows[rowIndex];

            SelectedBloodSugarId = Convert.ToInt32(row.Cells["blood_sugar_id"].Value);
            this.DialogResult = DialogResult.OK;
        }
    }
}