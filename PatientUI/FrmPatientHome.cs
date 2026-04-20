using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using BLL;

namespace PatientUI
{
    public partial class FrmPatientHome : Form
    {
        // ==============================================
        // 【1. 页面可调节参数】——直接改数值调整页面样式
        // ==============================================
        private readonly int _pageTopMargin = 80;
        private readonly int _pageLeftMargin = 250;
        private readonly int _pageRightMargin = 20;
        private readonly int _pageBottomMargin = 20;

        private readonly int _cardAreaHeight = 180;
        private readonly int _chartAreaHeight = 1200; // 大幅增加，支持滚动

        private readonly float _columnWidthScale = 0.9f;

        private readonly B_BloodSugar _bllBloodSugar = new B_BloodSugar();
        private readonly B_Diet _bllDiet = new B_Diet();
        private readonly B_Exercise _bllExercise = new B_Exercise();
        private readonly B_MedicineReminder _bllReminder = new B_MedicineReminder();
        private readonly B_PatientHealthOverview _bllOverview = new B_PatientHealthOverview();

        private TodayHealthData _healthData;

        public class TodayHealthData
        {
            public double TodayBloodSugar { get; set; }
            public int TodayCalories { get; set; }
            public int TodayExerciseMinutes { get; set; }
            public Dictionary<string, int> DietStructure { get; set; }
            public List<double> BloodSugarTrend { get; set; }
            public List<string> AlertMessages { get; set; }
            public string AssessmentSummary { get; set; }
        }

        public FrmPatientHome()
        {
            _healthData = CreateEmptyHealthData();
            InitializeComponent();
            InitBaseSettings();
        }

        private void InitBaseSettings()
        {
            this.TopLevel = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;
            this.Font = new Font("微软雅黑", 10f); // 增大基础字体
            this.AutoScaleMode = AutoScaleMode.Font;
            this.DoubleBuffered = true;

            this.VisibleChanged += (s, e) =>
            {
                if (this.Visible)
                {
                    LoadHealthData();
                    CreateCenterLayout();
                }
            };
        }

        private TodayHealthData CreateEmptyHealthData()
        {
            return new TodayHealthData
            {
                TodayBloodSugar = 0,
                TodayCalories = 0,
                TodayExerciseMinutes = 0,
                DietStructure = new Dictionary<string, int>(),
                BloodSugarTrend = new List<double>(),
                AlertMessages = new List<string>(),
                AssessmentSummary = "暂无最近健康评估记录"
            };
        }

        private void LoadHealthData()
        {
            _healthData = CreateEmptyHealthData();
            if (Program.LoginUser == null || Program.LoginUser.user_id <= 0)
            {
                _healthData.AlertMessages.Add("当前未获取到登录用户信息，请重新登录后查看首页数据。");
                return;
            }

            int userId = Program.LoginUser.user_id;
            try
            {
                _healthData.TodayBloodSugar = Convert.ToDouble(_bllBloodSugar.GetTodayLatestBloodSugar(userId));
                _healthData.TodayCalories = Convert.ToInt32(Math.Round(_bllDiet.GetTodayTotalCalorie(userId), 0));
                int todayExerciseMinutes = _bllExercise.GetTodayExerciseMinutes(userId);
                _healthData.TodayExerciseMinutes = todayExerciseMinutes < 0 ? 0 : todayExerciseMinutes;
                _healthData.DietStructure = _bllDiet.Get7DayMealCalorieDistribution(userId) ?? new Dictionary<string, int>();
                _healthData.BloodSugarTrend = Build7DayBloodSugarTrend(userId);

                var overview = _bllOverview.GetPatientFullHealthData(userId);
                _healthData.AssessmentSummary = BuildAssessmentSummary(overview);
                _healthData.AlertMessages = BuildAlertMessages(userId, overview);
            }
            catch (Exception ex)
            {
                _healthData = CreateEmptyHealthData();
                _healthData.AlertMessages.Add($"首页数据加载失败：{ex.Message}");
            }
        }

        private List<double> Build7DayBloodSugarTrend(int userId)
        {
            DataTable dt = _bllBloodSugar.Get30DayTrendData(userId);
            if (dt == null || dt.Rows.Count == 0)
                return new List<double>();

            DateTime startDate = DateTime.Today.AddDays(-6);
            return dt.AsEnumerable()
                .Where(row => row["record_date"] != DBNull.Value && Convert.ToDateTime(row["record_date"]).Date >= startDate)
                .GroupBy(row => Convert.ToDateTime(row["record_date"]).Date)
                .OrderBy(group => group.Key)
                .Select(group => Math.Round(group.Average(row => Convert.ToDouble(row["avg_value"])), 1))
                .ToList();
        }

        private List<string> BuildAlertMessages(int userId, Model.PatientHealthOverview overview)
        {
            List<string> alerts = new List<string>();
            int abnormalCount = overview?.BloodSugarList?.Count(item => item.measurement_time.HasValue && item.measurement_time.Value >= DateTime.Now.AddDays(-7) && item.is_abnormal == 1) ?? 0;
            if (abnormalCount > 0)
            {
                alerts.Add($"近7天检测到 {abnormalCount} 次血糖异常，请及时复测并关注饮食、运动和用药情况。");
            }

            var reminderList = _bllReminder.GetUserReminders(userId)
                .Where(item => item.is_enabled)
                .OrderBy(item => item.reminder_time)
                .Take(3)
                .ToList();
            if (reminderList.Any())
            {
                alerts.Add("最近用药提醒：" + string.Join("；", reminderList.Select(item => $"{item.drug_name} {item.reminder_time}")));
            }

            if (!alerts.Any())
            {
                alerts.Add("暂无血糖异常和待执行用药提醒，请继续保持当前健康管理节奏。");
            }

            return alerts;
        }

        private string BuildAssessmentSummary(Model.PatientHealthOverview overview)
        {
            if (overview?.LatestAssessment == null || overview.LatestAssessment.assessment_id <= 0)
                return "暂无最近健康评估记录";

            var assessment = overview.LatestAssessment;
            List<string> lines = new List<string>
            {
                $"评估日期：{assessment.assessment_date:yyyy-MM-dd}",
                $"血糖控制：{(string.IsNullOrWhiteSpace(assessment.glycemic_control_status) ? "未填写" : assessment.glycemic_control_status)}",
                $"糖化血红蛋白：{(assessment.hba1c.HasValue ? assessment.hba1c.Value.ToString("0.0") + "%" : "未填写")}",
                $"评估得分：{(assessment.assessment_score.HasValue ? assessment.assessment_score.Value.ToString("0.0") : "未评分")}"
            };
            if (!string.IsNullOrWhiteSpace(assessment.health_suggestion))
            {
                lines.Add($"建议：{assessment.health_suggestion}");
            }
            return string.Join(Environment.NewLine, lines);
        }

        private void CreateCenterLayout()
        {
            this.Controls.Clear();
            this.SuspendLayout();

            var rootPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                BackColor = Color.White,
                Padding = new Padding(_pageLeftMargin, _pageTopMargin, _pageRightMargin, _pageBottomMargin),
                AutoScroll = true
            };
            rootPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            rootPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, _cardAreaHeight));
            rootPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 480));
            rootPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 360));

            rootPanel.Controls.Add(CreateTopCardPanel(), 0, 0);
            rootPanel.Controls.Add(CreateChartPanelGroup(), 0, 1);
            rootPanel.Controls.Add(CreateInfoPanelGroup(), 0, 2);

            this.Controls.Add(rootPanel);
            this.ResumeLayout(true);
            this.PerformLayout();
        }

        private TableLayoutPanel CreateTopCardPanel()
        {
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 0, 16)
            };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34f));

            panel.Controls.Add(CreateStatCard("今日血糖", _healthData.TodayBloodSugar > 0 ? _healthData.TodayBloodSugar.ToString("0.0") + " mmol/L" : "暂无数据", Color.FromArgb(59, 130, 246)), 0, 0);
            panel.Controls.Add(CreateStatCard("今日热量", _healthData.TodayCalories > 0 ? _healthData.TodayCalories + " kcal" : "暂无数据", Color.FromArgb(245, 158, 11)), 1, 0);
            panel.Controls.Add(CreateStatCard("今日运动", _healthData.TodayExerciseMinutes > 0 ? _healthData.TodayExerciseMinutes + " 分钟" : "暂无数据", Color.FromArgb(16, 185, 129)), 2, 0);
            return panel;
        }

        private Panel CreateStatCard(string title, string value, Color accentColor)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(8),
                BackColor = Color.White,
                Padding = new Padding(20)
            };

            panel.Paint += (s, e) =>
            {
                using (var borderPen = new Pen(Color.FromArgb(229, 231, 235), 1))
                {
                    e.Graphics.DrawRectangle(borderPen, 0, 0, panel.Width - 1, panel.Height - 1);
                }
                using (var accentBrush = new SolidBrush(accentColor))
                {
                    e.Graphics.FillRectangle(accentBrush, 0, 0, 6, panel.Height);
                }
            };

            var lblValue = new Label
            {
                Text = value,
                Dock = DockStyle.Fill,
                Font = new Font("微软雅黑", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 41, 55),
                TextAlign = ContentAlignment.MiddleLeft
            };
            var lblTitle = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = 36,
                Font = new Font("微软雅黑", 11F, FontStyle.Bold),
                ForeColor = accentColor,
                TextAlign = ContentAlignment.MiddleLeft
            };
            panel.Controls.Add(lblValue);
            panel.Controls.Add(lblTitle);
            return panel;
        }

        private TableLayoutPanel CreateChartPanelGroup()
        {
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 0, 16)
            };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            panel.Controls.Add(CreateChartCard("🍽 近7天饮食热量占比", GenerateDietPieChartImage(_healthData.DietStructure)), 0, 0);
            panel.Controls.Add(CreateChartCard("📈 近7天血糖趋势", GenerateBloodSugarTrendChartImage(_healthData.BloodSugarTrend)), 1, 0);
            return panel;
        }

        private TableLayoutPanel CreateInfoPanelGroup()
        {
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.White
            };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            panel.Controls.Add(CreateTextCard("🔔 用药与异常提醒", _healthData.AlertMessages), 0, 0);
            panel.Controls.Add(CreateTextCard("🩺 最近健康评估", new List<string> { _healthData.AssessmentSummary }), 1, 0);
            return panel;
        }

        private Panel CreateChartCard(string title, Image image)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = new Padding(8),
                Padding = new Padding(12, 12, 12, 20)
            };
            panel.Controls.Add(new PictureBox
            {
                Dock = DockStyle.Fill,
                Image = image,
                SizeMode = PictureBoxSizeMode.Zoom
            });
            panel.Controls.Add(new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = 42,
                Font = new Font("微软雅黑", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 41, 55),
                TextAlign = ContentAlignment.MiddleLeft
            });
            return panel;
        }

        private Panel CreateTextCard(string title, List<string> lines)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = new Padding(8),
                Padding = new Padding(16)
            };
            string content = lines != null && lines.Count > 0
                ? string.Join(Environment.NewLine + Environment.NewLine, lines.Select((line, index) => lines.Count == 1 ? line : $"{index + 1}. {line}"))
                : "暂无数据";
            panel.Controls.Add(new Label
            {
                Text = content,
                Dock = DockStyle.Fill,
                Font = new Font("微软雅黑", 10F),
                ForeColor = Color.FromArgb(55, 65, 81),
                AutoSize = false
            });
            panel.Controls.Add(new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = 42,
                Font = new Font("微软雅黑", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 41, 55),
                TextAlign = ContentAlignment.MiddleLeft
            });
            return panel;
        }

        private Image GenerateDietPieChartImage(Dictionary<string, int> dietData)
        {
            var bmp = new Bitmap(520, 360);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                if (dietData == null || dietData.Count == 0 || dietData.Values.Sum() <= 0)
                {
                    DrawNoDataText(g, bmp.Width, bmp.Height, "近7天暂无饮食热量数据");
                    return bmp;
                }

                Color[] colors =
                {
                    Color.FromArgb(59, 130, 246),
                    Color.FromArgb(16, 185, 129),
                    Color.FromArgb(245, 158, 11),
                    Color.FromArgb(239, 68, 68),
                    Color.FromArgb(168, 85, 247)
                };

                int total = dietData.Values.Sum();
                float startAngle = 0f;
                Rectangle pieRect = new Rectangle(28, 48, 220, 220);
                int index = 0;
                foreach (var item in dietData)
                {
                    float sweepAngle = total == 0 ? 0 : (float)item.Value / total * 360f;
                    using (var brush = new SolidBrush(colors[index % colors.Length]))
                    {
                        g.FillPie(brush, pieRect, startAngle, sweepAngle);
                    }
                    startAngle += sweepAngle;
                    index++;
                }

                using (var font = new Font("微软雅黑", 10F))
                {
                    index = 0;
                    int top = 52;
                    foreach (var item in dietData)
                    {
                        using (var brush = new SolidBrush(colors[index % colors.Length]))
                        {
                            g.FillRectangle(brush, 286, top, 14, 14);
                        }
                        g.DrawString($"{item.Key}：{item.Value} kcal", font, Brushes.Black, 308, top - 2);
                        top += 34;
                        index++;
                    }
                }
            }
            return bmp;
        }

        private Image GenerateBloodSugarTrendChartImage(List<double> trendData)
        {
            var bmp = new Bitmap(520, 360);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                if (trendData == null || trendData.Count == 0)
                {
                    DrawNoDataText(g, bmp.Width, bmp.Height, "近7天暂无血糖趋势数据");
                    return bmp;
                }

                int padLeft = 58;
                int padTop = 32;
                int padRight = 24;
                int padBottom = 54;
                Rectangle plot = new Rectangle(padLeft, padTop, bmp.Width - padLeft - padRight, bmp.Height - padTop - padBottom);
                double maxValue = Math.Max(10, trendData.Max() + 1);
                double minValue = Math.Max(0, Math.Min(3, trendData.Min() - 1));
                double range = Math.Max(1, maxValue - minValue);

                using (var gridPen = new Pen(Color.FromArgb(229, 231, 235), 1))
                {
                    for (int i = 0; i <= 4; i++)
                    {
                        float y = plot.Y + plot.Height / 4f * i;
                        g.DrawLine(gridPen, plot.X, y, plot.Right, y);
                        double value = maxValue - range / 4d * i;
                        g.DrawString(value.ToString("0.0"), new Font("微软雅黑", 8F), Brushes.Gray, 4, y - 8);
                    }
                }

                using (var axisPen = new Pen(Color.FromArgb(107, 114, 128), 1.5f))
                {
                    g.DrawLine(axisPen, plot.X, plot.Y, plot.X, plot.Bottom);
                    g.DrawLine(axisPen, plot.X, plot.Bottom, plot.Right, plot.Bottom);
                }

                PointF[] points = new PointF[trendData.Count];
                float stepX = trendData.Count == 1 ? 0 : plot.Width / (float)(trendData.Count - 1);
                for (int i = 0; i < trendData.Count; i++)
                {
                    float x = plot.X + stepX * i;
                    float y = plot.Bottom - (float)((trendData[i] - minValue) / range * plot.Height);
                    points[i] = new PointF(x, y);
                    g.DrawString($"D{i + 1}", new Font("微软雅黑", 8F), Brushes.Gray, x - 8, plot.Bottom + 8);
                }

                if (points.Length > 1)
                {
                    using (var linePen = new Pen(Color.FromArgb(59, 130, 246), 2.5f))
                    {
                        g.DrawLines(linePen, points);
                    }
                }

                using (var pointBrush = new SolidBrush(Color.FromArgb(59, 130, 246)))
                {
                    foreach (var point in points)
                    {
                        g.FillEllipse(pointBrush, point.X - 4, point.Y - 4, 8, 8);
                    }
                }
            }
            return bmp;
        }

        private void DrawNoDataText(Graphics g, int width, int height, string text)
        {
            using (var font = new Font("微软雅黑", 11F, FontStyle.Bold))
            {
                SizeF size = g.MeasureString(text, font);
                g.DrawString(text, font, Brushes.Gray, (width - size.Width) / 2f, (height - size.Height) / 2f);
            }
        }

        // ✅ 优化版柱状图：增大字体 + 完善布局
        private Image GenerateScoreBarChartImage(Dictionary<string, int> scoreData)
        {
            var bmp = new Bitmap(400, 300);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                if (scoreData == null || scoreData.Count == 0)
                {
                    DrawNoDataText(g, bmp.Width, bmp.Height, "暂无评分数据");
                    return bmp;
                }

                int pad = 40;
                Rectangle plot = new Rectangle(pad, pad, bmp.Width - 2 * pad, bmp.Height - 2 * pad);
                var list = scoreData.ToList();
                float step = plot.Width / (float)Math.Max(1, list.Count);
                float barWidth = step * 0.6f;

                using (var axisPen = new Pen(Color.FromArgb(209, 213, 219), 1.5f))
                {
                    g.DrawLine(axisPen, plot.X, plot.Bottom, plot.Right, plot.Bottom);
                }

                for (int i = 0; i < list.Count; i++)
                {
                    float x = plot.X + i * step + (step - barWidth) / 2f;
                    float barHeight = plot.Height * list[i].Value / 100f;
                    float y = plot.Bottom - barHeight;

                    using (var brush = new SolidBrush(Color.FromArgb(16, 185, 129)))
                    {
                        g.FillRectangle(brush, x, y, barWidth, barHeight);
                    }

                    g.DrawString(list[i].Key, new Font("微软雅黑", 8F), Brushes.Gray, x, plot.Bottom + 6);
                    g.DrawString(list[i].Value.ToString(), new Font("微软雅黑", 8F, FontStyle.Bold), Brushes.Black, x, y - 16);
                }
            }
            return bmp;
        }

        // ✅ 修复版热力图：完美对齐 + 解决错位问题
        private Image GenerateTargetHeatmapImage(List<int> targetData)
        {
            var bmp = new Bitmap(420, 320); // 微调尺寸，适配7天+标签
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // ✅ 关键：统一内边距，确保坐标系对齐
                int padTop = 50;
                int padLeft = 60;   // 左侧留足Y轴标签空间
                int padRight = 30;
                int padBottom = 60; // 底部留足X轴标签空间

                Rectangle plot = new Rectangle(
                    padLeft,
                    padTop,
                    bmp.Width - padLeft - padRight,
                    bmp.Height - padTop - padBottom
                );

                // 绘制网格线（Y轴）
                using (var gridPen = new Pen(Color.FromArgb(240, 240, 240), 1))
                {
                    for (int i = 0; i <= 5; i++)
                    {
                        float y = plot.Y + i * (plot.Height / 5f);
                        g.DrawLine(gridPen, plot.X, y, plot.Right, y);

                        // Y轴标签（0%-100%）
                        using (var font = new Font("微软雅黑", 9F))
                        using (var textBrush = new SolidBrush(Color.Gray))
                        {
                            string label = (100 - i * 20).ToString() + "%";
                            g.DrawString(label, font, textBrush, plot.X - 45, y - 8);
                        }
                    }
                }

                // ✅ 绘制坐标轴（严格对齐）
                using (var axisPen = new Pen(Color.Black, 2))
                {
                    g.DrawLine(axisPen, plot.X, plot.Y, plot.X, plot.Bottom);      // Y轴左线
                    g.DrawLine(axisPen, plot.X, plot.Bottom, plot.Right, plot.Bottom); // X轴底线
                }

                // ✅ 关键修复：计算每根柱子的精确位置（7天 → 7个柱子）
                int n = targetData.Count;
                float barWidth = plot.Width / (float)n * 0.7f; // 每根柱子宽度
                float spacing = plot.Width / (float)n * 0.3f;  // 间距

                // ✅ 从左到右绘制，确保第一根柱子左边缘 = plot.X
                for (int i = 0; i < n; i++)
                {
                    // ✅ 精确计算柱子左上角 X 坐标（左对齐）
                    float x = plot.X + i * (barWidth + spacing);

                    // ✅ 柱子高度 = 百分比 × 高度，从 plot.Bottom 向上画
                    float barHeight = (float)targetData[i] / 100f * plot.Height;
                    float y = plot.Bottom - barHeight; // ✅ 严格以 plot.Bottom 为基线

                    // ✅ 修正：柱子宽度居中于间隔区间（视觉更平衡）
                    float actualX = x + spacing / 2f - barWidth / 2f;

                    // 透明度随数值变化
                    int alpha = Math.Min(255, (int)(targetData[i] * 2.55));
                    using (var brush = new SolidBrush(Color.FromArgb(alpha, 239, 68, 68)))
                        g.FillRectangle(brush, actualX, y, barWidth, barHeight);

                    // 边框（加粗）
                    using (var pen = new Pen(Color.FromArgb(239, 68, 68), 2))
                        g.DrawRectangle(pen, actualX, y, barWidth, barHeight);

                    // 数据标签（顶部居中）
                    using (var font = new Font("微软雅黑", 9F, FontStyle.Bold))
                    using (var textBrush = new SolidBrush(Color.White))
                    {
                        string percent = targetData[i].ToString() + "%";
                        SizeF size = g.MeasureString(percent, font);
                        g.DrawString(percent, font, textBrush,
                            actualX + barWidth / 2f - size.Width / 2f,
                            y - 18f); // 上移18px，避免遮挡
                    }

                    // X轴标签（底部居中）
                    using (var font = new Font("微软雅黑", 9F))
                    using (var textBrush = new SolidBrush(Color.Black))
                    {
                        string dayLabel = $"Day{i + 1}";
                        SizeF size = g.MeasureString(dayLabel, font);
                        g.DrawString(dayLabel, font, textBrush,
                            actualX + barWidth / 2f - size.Width / 2f,
                            plot.Bottom + 10f); // 严格在 plot.Bottom 下方10px
                    }
                }

                // 添加标题（居中）
                using (var font = new Font("微软雅黑", 11F, FontStyle.Bold))
                using (var textBrush = new SolidBrush(Color.FromArgb(37, 99, 235)))
                {
                    string title = "🎯 目标完成热力图";
                    SizeF titleSize = g.MeasureString(title, font);
                    g.DrawString(title, font, textBrush,
                        bmp.Width / 2f - titleSize.Width / 2f,
                        padTop - 15f);
                }
            }
            return bmp;
        }

        private void FrmPatientHome_Load(object sender, EventArgs e) { }

        private void FrmPatientHome_Load_1(object sender, EventArgs e)
        {

        }
    }
}