// PatientUI/FrmAddEditReminder.cs
using System;
using System.Drawing;
using System.Windows.Forms;
using BLL;
using Model;

namespace PatientUI
{
    public partial class FrmAddEditReminder : Form
    {
        private readonly int _userId;
        private readonly MedicineReminder _editReminder;
        private readonly B_MedicineReminder _bllReminder = new B_MedicineReminder();
        private readonly B_Medicine _bllMedicine = new B_Medicine();

        private ComboBox _cboDrugName;
        private TextBox _txtDosage;
        private ComboBox _cboTakeWay;
        private DateTimePicker _dtpReminderTime;
        private CheckBox _chkEnabled;
        private TextBox _txtRemark;

        public FrmAddEditReminder(int userId, MedicineReminder editReminder = null)
        {
            InitializeComponent();
            _userId = userId;
            _editReminder = editReminder;
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Text = editReminder == null ? "添加用药提醒" : "编辑用药提醒";
            this.Size = new Size(350, 300);
            this.BackColor = Color.White;
            this.Font = new Font("微软雅黑", 9F);
            this.Load += FrmAddEditReminder_Load;
        }

        private void FrmAddEditReminder_Load(object sender, EventArgs e)
        {
            InitializeControls();
            BindDrugDictionary();
            if (_editReminder != null)
            {
                LoadEditData();
            }
        }

        private void InitializeControls()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                RowCount = 6,
                ColumnCount = 2
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // 药物名称
            layout.Controls.Add(new Label { Text = "药物名称：", TextAlign = ContentAlignment.MiddleRight }, 0, 0);
            _cboDrugName = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            layout.Controls.Add(_cboDrugName, 1, 0);

            // 用药剂量
            layout.Controls.Add(new Label { Text = "用药剂量：", TextAlign = ContentAlignment.MiddleRight }, 0, 1);
            _txtDosage = new TextBox { Dock = DockStyle.Fill };
            layout.Controls.Add(_txtDosage, 1, 1);

            // 用药方式
            layout.Controls.Add(new Label { Text = "用药方式：", TextAlign = ContentAlignment.MiddleRight }, 0, 2);
            _cboTakeWay = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            _cboTakeWay.Items.AddRange(new[] { "口服", "皮下注射", "静脉注射", "外用", "雾化" });
            _cboTakeWay.SelectedIndex = 0;
            layout.Controls.Add(_cboTakeWay, 1, 2);

            // 提醒时间
            layout.Controls.Add(new Label { Text = "提醒时间：", TextAlign = ContentAlignment.MiddleRight }, 0, 3);
            _dtpReminderTime = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Time, ShowUpDown = true };
            layout.Controls.Add(_dtpReminderTime, 1, 3);

            // 启用状态
            layout.Controls.Add(new Label { Text = "启用提醒：", TextAlign = ContentAlignment.MiddleRight }, 0, 4);
            _chkEnabled = new CheckBox { Text = "启用", Checked = true };
            layout.Controls.Add(_chkEnabled, 1, 4);

            // 备注
            layout.Controls.Add(new Label { Text = "备注：", TextAlign = ContentAlignment.MiddleRight }, 0, 5);
            _txtRemark = new TextBox { Dock = DockStyle.Fill, Multiline = true, Height = 60 };
            layout.Controls.Add(_txtRemark, 1, 5);

            // 按钮区
            var btnPanel = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 50, FlowDirection = FlowDirection.RightToLeft };
            var btnCancel = new Button { Text = "取消", Width = 80, Height = 30, Margin = new Padding(10) };
            var btnSave = new Button { Text = "保存", Width = 80, Height = 30, Margin = new Padding(10), BackColor = Color.FromArgb(0, 122, 204), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };

            btnSave.Click += (s, e) => { SaveReminder(); };
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; };

            btnPanel.Controls.Add(btnCancel);
            btnPanel.Controls.Add(btnSave);

            this.Controls.Add(layout);
            this.Controls.Add(btnPanel);
        }

        private void BindDrugDictionary()
        {
            try
            {
                var drugList = _bllMedicine.GetDrugDictionary();
                _cboDrugName.DataSource = null;
                _cboDrugName.Items.Clear();
                if (drugList != null && drugList.Count > 0)
                {
                    _cboDrugName.DisplayMember = "DrugGenericName";
                    _cboDrugName.ValueMember = "DrugCode";
                    _cboDrugName.DataSource = drugList;
                }
                else
                {
                    _cboDrugName.Items.AddRange(new[] { "二甲双胍", "阿卡波糖", "西格列汀", "胰岛素" });
                }

                if (_cboDrugName.Items.Count > 0)
                {
                    _cboDrugName.SelectedIndex = 0;
                }
            }
            catch
            {
                _cboDrugName.DataSource = null;
                _cboDrugName.Items.Clear();
                _cboDrugName.Items.AddRange(new[] { "二甲双胍", "阿卡波糖", "西格列汀", "胰岛素" });
                if (_cboDrugName.Items.Count > 0)
                {
                    _cboDrugName.SelectedIndex = 0;
                }
            }
        }

        private void LoadEditData()
        {
            if (!string.IsNullOrWhiteSpace(_editReminder.drug_name))
            {
                int index = _cboDrugName.FindStringExact(_editReminder.drug_name);
                if (index >= 0)
                {
                    _cboDrugName.SelectedIndex = index;
                }
                else
                {
                    _cboDrugName.DataSource = null;
                    _cboDrugName.Items.Insert(0, _editReminder.drug_name);
                    _cboDrugName.SelectedIndex = 0;
                }
            }
            _txtDosage.Text = _editReminder.drug_dosage;
            _cboTakeWay.Text = _editReminder.take_way;
            _dtpReminderTime.Value = DateTime.Parse(_editReminder.reminder_time);
            _chkEnabled.Checked = _editReminder.is_enabled;
            _txtRemark.Text = _editReminder.remark;
        }

        private void SaveReminder()
        {
            if (_cboDrugName.SelectedIndex < 0 || string.IsNullOrWhiteSpace(_cboDrugName.Text))
            {
                MessageBox.Show("请选择药物名称", "输入校验", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _cboDrugName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(_txtDosage.Text))
            {
                MessageBox.Show("请输入用药剂量", "输入校验", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtDosage.Focus();
                return;
            }

            var reminder = new MedicineReminder
            {
                user_id = _userId,
                drug_name = _cboDrugName.Text.Trim(),
                drug_dosage = _txtDosage.Text.Trim(),
                take_way = _cboTakeWay.Text,
                reminder_time = _dtpReminderTime.Value.ToString("HH:mm"),
                is_enabled = _chkEnabled.Checked,
                remark = _txtRemark.Text.Trim()
            };

            ResultModel result;
            if (_editReminder == null)
            {
                result = _bllReminder.AddReminder(reminder);
            }
            else
            {
                reminder.reminder_id = _editReminder.reminder_id;
                result = _bllReminder.UpdateReminder(reminder);
            }

            if (result.Success)
            {
                MessageBox.Show(result.Msg, "保存成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show(result.Msg, "保存失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}