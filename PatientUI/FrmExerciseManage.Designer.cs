using System;
using System.ComponentModel; // 新增：IContainer 来自这里
using System.Drawing;        // 新增：Size 来自这里
using System.Windows.Forms;  // 关键：Form 类来自这里

namespace PatientUI
{
    // 修复：添加 : Form 继承
    partial class FrmExerciseManage : Form
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
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
            this.SuspendLayout();
            // 
            // FrmExerciseManage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Name = "FrmExerciseManage";
            this.Text = "FrmExercisePlan";
            this.Load += new System.EventHandler(this.FrmExerciseManage_Load);
            this.ResumeLayout(false);

        }

        #endregion
    }
}