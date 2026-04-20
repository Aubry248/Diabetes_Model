using System;
using System.Drawing;
using System.Windows.Forms;
using Model;

namespace PatientUI
{
    /// <summary>
    /// 食物详情弹窗（独立窗体，构造函数正确，继承Form）
    /// </summary>
    public partial class FrmFoodDetail : Form
    {
        // 字段仅声明一次，无重复
        private readonly FoodNutrition _food;
        private readonly int _fontContent = 10;
        private readonly int _fontTitle = 12;

        // ✅ 【核心修复】必须保留这个带FoodNutrition参数的public构造函数！
        /// <summary>
        /// 带食物数据的构造函数（供FrmDietRecommend调用）
        /// </summary>
        public FrmFoodDetail(FoodNutrition food)
        {
            InitializeComponent();
            // 空值校验，避免崩溃
            _food = food ?? throw new ArgumentNullException(nameof(food), "食物数据不能为空");
            BuildLayout();
        }

        // ✅ 【设计器兼容】保留无参构造函数（VS窗体设计器必须依赖它，不影响带参使用）
        /// <summary>
        /// 设计器用无参构造函数（不要删除）
        /// </summary>
        public FrmFoodDetail()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 构建页面布局
        /// </summary>
        private void BuildLayout()
        {
            if (_food == null)
            {
                MessageBox.Show("食物数据为空，无法加载详情", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }

            this.Controls.Clear();
            this.SuspendLayout();

            // 根容器
            TableLayoutPanel root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 4,
                ColumnCount = 1,
                Padding = new Padding(20),
                BackColor = Color.White
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 60));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 30));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

            // 标题
            Label lblTitle = new Label
            {
                Text = $"🥗 {_food.FoodName} 营养详情",
                Font = new Font("微软雅黑", _fontTitle, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            root.Controls.Add(lblTitle, 0, 0);

            // 营养成分面板
            Panel pnlNutrition = new Panel
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(248, 250, 252)
            };
            string nutritionContent = $@"
■ 基础信息
食物分类：{_food.FoodCategory}
GI值：{_food.GI:F1}    升糖负荷GL：{_food.GL:F1}
可食部比例：{_food.EdibleRate:F1}%

■ 核心营养成分（每100g可食部）
热量：{_food.Energy_kcal:F1} kcal    能量：{_food.Energy_kJ:F1} kJ
蛋白质：{_food.Protein:F1} g    脂肪：{_food.Fat:F1} g
碳水化合物：{_food.Carbohydrate:F1} g    膳食纤维：{_food.DietaryFiber:F1} g

■ 微量营养素
胆固醇：{_food.Cholesterol:F1} mg    维生素C：{_food.VitaminC:F1} mg
钠：{_food.Sodium:F1} mg    钾：{_food.Potassium:F1} mg
胡萝卜素：{_food.Carotene:F1} μg
";
            Label lblNutrition = new Label
            {
                Text = nutritionContent,
                Font = new Font("微软雅黑", _fontContent),
                ForeColor = Color.FromArgb(64, 64, 64),
                Dock = DockStyle.Fill,
                AutoSize = false,
                TextAlign = ContentAlignment.TopLeft
            };
            pnlNutrition.Controls.Add(lblNutrition);
            root.Controls.Add(pnlNutrition, 0, 1);

            // 推荐食用建议面板
            Panel pnlSuggest = new Panel
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(240, 248, 255)
            };
            string suggestContent = $@"
■ 推荐食用量
糖尿病患者单次推荐食用量：{GetRecommendAmount()}g
每日食用不超过2次，避免过量摄入导致血糖波动

■ 推荐烹饪方式
{GetCookMethod()}
避免油炸、红烧、糖醋等高油高糖烹饪方式

■ 食用提示
{GetEatTip()}
";
            Label lblSuggest = new Label
            {
                Text = suggestContent,
                Font = new Font("微软雅黑", _fontContent),
                ForeColor = Color.FromArgb(64, 64, 64),
                Dock = DockStyle.Fill,
                AutoSize = false,
                TextAlign = ContentAlignment.TopLeft
            };
            pnlSuggest.Controls.Add(lblSuggest);
            root.Controls.Add(pnlSuggest, 0, 2);

            // 关闭按钮
            Button btnClose = new Button
            {
                Text = "关闭",
                Font = new Font("微软雅黑", _fontContent, FontStyle.Regular),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Right,
                Width = 100,
                Height = 35
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();
            Panel pnlBtn = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            pnlBtn.Controls.Add(btnClose);
            root.Controls.Add(pnlBtn, 0, 3);

            this.Controls.Add(root);
            this.ResumeLayout(true);
        }

        #region 辅助方法
        private string GetRecommendAmount()
        {
            if (_food.FoodCategory == "谷薯类") return "50-75";
            if (_food.FoodCategory == "蔬菜类") return "200-300";
            if (_food.FoodCategory == "肉类") return "50-100";
            if (_food.FoodCategory == "蛋类") return "50-100";
            if (_food.FoodCategory == "乳类") return "200-300";
            if (_food.FoodCategory == "豆类及制品") return "25-50";
            if (_food.FoodCategory == "水果类") return "100-200";
            return "50-100";
        }

        private string GetCookMethod()
        {
            if (_food.FoodCategory == "谷薯类") return "蒸、煮、焖，避免熬粥（糊化后升糖加快）";
            if (_food.FoodCategory == "蔬菜类") return "清炒、白灼、蒜蓉、凉拌，先洗后切避免营养流失";
            if (_food.FoodCategory == "肉类") return "清炖、清蒸、快炒，去除可见肥肉";
            if (_food.FoodCategory == "蛋类") return "水煮、蒸蛋羹，避免煎蛋、油炒";
            if (_food.FoodCategory == "乳类") return "直接饮用、无糖酸奶发酵，避免添加糖";
            return "蒸、煮、清炒，保留食物原有营养";
        }

        private string GetEatTip()
        {
            if (_food.GI < 30) return "该食物为极低GI食物，可放心食用，搭配蛋白质食物可进一步延缓升糖";
            if (_food.GI < 45) return "该食物为低GI食物，适合糖尿病患者日常食用，控制单次摄入量即可";
            if (_food.GI < 55) return "该食物为中低GI食物，建议搭配绿叶蔬菜食用，避免单次大量摄入";
            return "建议搭配膳食纤维丰富的食物食用，延缓血糖上升";
        }
        #endregion
    }
}