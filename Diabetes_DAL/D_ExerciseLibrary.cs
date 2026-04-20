using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Model;
using Tools;

namespace DAL
{
    /// <summary>
    /// 运动库管理数据访问层（管理端字典表专用）
    /// </summary>
    public class D_ExerciseLibrary
    {
        #region 核心：多条件分页查询运动库列表
        /// <summary>
        /// 管理端多条件分页查询运动库数据
        /// </summary>
        public DataTable GetExerciseListByPage(string searchKey, string exerciseCategory, string intensityCategory, string suitablePeople, string enableStatus, DateTime updateStart, DateTime updateEnd, out int totalCount)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(@"
                SELECT 
                    ExerciseID, ExerciseCode, ExerciseName, ExerciseCategory, MET_Value, IntensityCategory, 
                    IsDiabetesFriendly, RecommendDuration, RecommendFrequency, SuitablePeople, EnableStatus, 
                    CreateTime, UpdateTime, UpdateUser, AuditStatus, Version
                FROM Diabetes_Exercise_Energy_Expenditure 
                WHERE 1=1 ");

            List<SqlParameter> paramList = new List<SqlParameter>();

            // 动态拼接筛选条件
            if (!string.IsNullOrWhiteSpace(searchKey) && searchKey != "输入运动名称/唯一编码检索")
            {
                sql.Append(" AND (ExerciseName LIKE @SearchKey OR ExerciseCode LIKE @SearchKey OR Alias LIKE @SearchKey) ");
                paramList.Add(new SqlParameter("@SearchKey", $"%{searchKey}%"));
            }
            if (!string.IsNullOrWhiteSpace(exerciseCategory) && exerciseCategory != "全部")
            {
                sql.Append(" AND ExerciseCategory = @ExerciseCategory ");
                paramList.Add(new SqlParameter("@ExerciseCategory", exerciseCategory));
            }
            if (!string.IsNullOrWhiteSpace(intensityCategory) && intensityCategory != "全部")
            {
                sql.Append(" AND IntensityCategory = @IntensityCategory ");
                paramList.Add(new SqlParameter("@IntensityCategory", intensityCategory));
            }
            if (!string.IsNullOrWhiteSpace(suitablePeople) && suitablePeople != "全部")
            {
                sql.Append(" AND SuitablePeople LIKE @SuitablePeople ");
                paramList.Add(new SqlParameter("@SuitablePeople", $"%{suitablePeople}%"));
            }
            if (!string.IsNullOrWhiteSpace(enableStatus) && enableStatus != "全部")
            {
                sql.Append(" AND EnableStatus = @EnableStatus ");
                paramList.Add(new SqlParameter("@EnableStatus", enableStatus));
            }
            // 时间范围筛选
            if (updateStart > DateTime.Now.AddYears(-10) || updateEnd < DateTime.Now.AddDays(1))
            {
                sql.Append(" AND UpdateTime BETWEEN @UpdateStart AND @UpdateEnd ");
                paramList.Add(new SqlParameter("@UpdateStart", updateStart.Date));
                paramList.Add(new SqlParameter("@UpdateEnd", updateEnd.Date.AddDays(1).AddSeconds(-1)));
            }

            // 查询总条数
            string countSql = $"SELECT COUNT(1) FROM ({sql.ToString()}) AS t";
            totalCount = Convert.ToInt32(SqlHelper.ExecuteScalar(countSql, paramList.ToArray()));

            // 排序返回数据
            sql.Append(" ORDER BY CreateTime DESC ");
            return SqlHelper.ExecuteDataTable(sql.ToString(), paramList.ToArray());
        }
        #endregion

        #region 基础CRUD操作
        /// <summary>
        /// 根据ID获取运动详情
        /// </summary>
        public ExerciseEnergyExpenditure GetExerciseById(int exerciseId)
        {
            string sql = @"SELECT * FROM Diabetes_Exercise_Energy_Expenditure WHERE ExerciseID = @ExerciseID";
            SqlParameter[] param = { new SqlParameter("@ExerciseID", exerciseId) };
            DataTable dt = SqlHelper.ExecuteDataTable(sql, param);
            if (dt == null || dt.Rows.Count == 0) return null;

            DataRow dr = dt.Rows[0];
            return new ExerciseEnergyExpenditure
            {
                ExerciseID = Convert.ToInt32(dr["ExerciseID"]),
                ExerciseCode = dr["ExerciseCode"].ToString(),
                ExerciseCategory = dr["ExerciseCategory"].ToString(),
                ExerciseName = dr["ExerciseName"].ToString(),
                MET_Value = Convert.ToDecimal(dr["MET_Value"]),
                IntensityCategory = dr["IntensityCategory"].ToString(),
                IsDiabetesFriendly = dr["IsDiabetesFriendly"].ToString(),
                ExerciseDesc = dr["ExerciseDesc"].ToString(),
                StandardSource = dr["StandardSource"].ToString(),
                Remark = dr["Remark"].ToString(),
                EnableStatus = dr["EnableStatus"].ToString(),
                AuditStatus = dr["AuditStatus"].ToString(),
                CreateTime = Convert.ToDateTime(dr["CreateTime"]),
                CreateUser = dr["CreateUser"].ToString(),
                UpdateTime = dr["UpdateTime"] != DBNull.Value ? Convert.ToDateTime(dr["UpdateTime"]) : (DateTime?)null,
                UpdateUser = dr["UpdateUser"].ToString(),
                Version = dr["Version"].ToString(),
                UpdateLog = dr["UpdateLog"].ToString(),
                AuditRecord = dr["AuditRecord"].ToString(),
                Alias = dr["Alias"].ToString(),
                RecommendDuration = dr["RecommendDuration"].ToString(),
                RecommendFrequency = dr["RecommendFrequency"].ToString(),
                SuitablePeople = dr["SuitablePeople"].ToString(),
                ForbiddenPeople = dr["ForbiddenPeople"].ToString(),
                SafetyTip = dr["SafetyTip"].ToString()
            };
        }

        /// <summary>
        /// 新增运动数据
        /// </summary>
        public int AddExercise(ExerciseEnergyExpenditure model)
        {
            string sql = @"
                INSERT INTO Diabetes_Exercise_Energy_Expenditure (
                    ExerciseCode, ExerciseCategory, ExerciseName, Alias, MET_Value, IntensityCategory, 
                    IsDiabetesFriendly, ExerciseDesc, StandardSource, RecommendDuration, RecommendFrequency, 
                    SuitablePeople, ForbiddenPeople, SafetyTip, Remark, EnableStatus, AuditStatus, 
                    CreateTime, CreateUser, UpdateTime, UpdateUser, Version, UpdateLog, AuditRecord
                ) VALUES (
                    @ExerciseCode, @ExerciseCategory, @ExerciseName, @Alias, @MET_Value, @IntensityCategory, 
                    @IsDiabetesFriendly, @ExerciseDesc, @StandardSource, @RecommendDuration, @RecommendFrequency, 
                    @SuitablePeople, @ForbiddenPeople, @SafetyTip, @Remark, @EnableStatus, @AuditStatus, 
                    @CreateTime, @CreateUser, @UpdateTime, @UpdateUser, @Version, @UpdateLog, @AuditRecord
                ); SELECT SCOPE_IDENTITY();";

            SqlParameter[] param = {
                new SqlParameter("@ExerciseCode", model.ExerciseCode ?? (object)DBNull.Value),
                new SqlParameter("@ExerciseCategory", model.ExerciseCategory),
                new SqlParameter("@ExerciseName", model.ExerciseName),
                new SqlParameter("@Alias", model.Alias ?? (object)DBNull.Value),
                new SqlParameter("@MET_Value", model.MET_Value),
                new SqlParameter("@IntensityCategory", model.IntensityCategory),
                new SqlParameter("@IsDiabetesFriendly", model.IsDiabetesFriendly),
                new SqlParameter("@ExerciseDesc", model.ExerciseDesc ?? (object)DBNull.Value),
                new SqlParameter("@StandardSource", model.StandardSource),
                new SqlParameter("@RecommendDuration", model.RecommendDuration ?? (object)DBNull.Value),
                new SqlParameter("@RecommendFrequency", model.RecommendFrequency ?? (object)DBNull.Value),
                new SqlParameter("@SuitablePeople", model.SuitablePeople ?? (object)DBNull.Value),
                new SqlParameter("@ForbiddenPeople", model.ForbiddenPeople ?? (object)DBNull.Value),
                new SqlParameter("@SafetyTip", model.SafetyTip ?? (object)DBNull.Value),
                new SqlParameter("@Remark", model.Remark ?? (object)DBNull.Value),
                new SqlParameter("@EnableStatus", model.EnableStatus),
                new SqlParameter("@AuditStatus", model.AuditStatus),
                new SqlParameter("@CreateTime", model.CreateTime),
                new SqlParameter("@CreateUser", model.CreateUser),
                new SqlParameter("@UpdateTime", model.UpdateTime ?? (object)DBNull.Value),
                new SqlParameter("@UpdateUser", model.UpdateUser ?? (object)DBNull.Value),
                new SqlParameter("@Version", model.Version),
                new SqlParameter("@UpdateLog", model.UpdateLog ?? (object)DBNull.Value),
                new SqlParameter("@AuditRecord", model.AuditRecord ?? (object)DBNull.Value)
            };

            object res = SqlHelper.ExecuteScalar(sql, param);
            return res != null && res != DBNull.Value ? Convert.ToInt32(res) : 0;
        }

        /// <summary>
        /// 修改运动数据
        /// </summary>
        public int UpdateExercise(ExerciseEnergyExpenditure model)
        {
            string sql = @"
                UPDATE Diabetes_Exercise_Energy_Expenditure SET
                    ExerciseName = @ExerciseName, Alias = @Alias, ExerciseCategory = @ExerciseCategory, 
                    MET_Value = @MET_Value, IntensityCategory = @IntensityCategory, IsDiabetesFriendly = @IsDiabetesFriendly,
                    ExerciseDesc = @ExerciseDesc, StandardSource = @StandardSource, RecommendDuration = @RecommendDuration,
                    RecommendFrequency = @RecommendFrequency, SuitablePeople = @SuitablePeople, ForbiddenPeople = @ForbiddenPeople,
                    SafetyTip = @SafetyTip, Remark = @Remark, EnableStatus = @EnableStatus, AuditStatus = @AuditStatus,
                    UpdateTime = @UpdateTime, UpdateUser = @UpdateUser, Version = @Version, UpdateLog = @UpdateLog,
                    AuditRecord = @AuditRecord
                WHERE ExerciseID = @ExerciseID";

            SqlParameter[] param = {
                new SqlParameter("@ExerciseID", model.ExerciseID),
                new SqlParameter("@ExerciseName", model.ExerciseName),
                new SqlParameter("@Alias", model.Alias ?? (object)DBNull.Value),
                new SqlParameter("@ExerciseCategory", model.ExerciseCategory),
                new SqlParameter("@MET_Value", model.MET_Value),
                new SqlParameter("@IntensityCategory", model.IntensityCategory),
                new SqlParameter("@IsDiabetesFriendly", model.IsDiabetesFriendly),
                new SqlParameter("@ExerciseDesc", model.ExerciseDesc ?? (object)DBNull.Value),
                new SqlParameter("@StandardSource", model.StandardSource),
                new SqlParameter("@RecommendDuration", model.RecommendDuration ?? (object)DBNull.Value),
                new SqlParameter("@RecommendFrequency", model.RecommendFrequency ?? (object)DBNull.Value),
                new SqlParameter("@SuitablePeople", model.SuitablePeople ?? (object)DBNull.Value),
                new SqlParameter("@ForbiddenPeople", model.ForbiddenPeople ?? (object)DBNull.Value),
                new SqlParameter("@SafetyTip", model.SafetyTip ?? (object)DBNull.Value),
                new SqlParameter("@Remark", model.Remark ?? (object)DBNull.Value),
                new SqlParameter("@EnableStatus", model.EnableStatus),
                new SqlParameter("@AuditStatus", model.AuditStatus),
                new SqlParameter("@UpdateTime", model.UpdateTime ?? (object)DBNull.Value),
                new SqlParameter("@UpdateUser", model.UpdateUser ?? (object)DBNull.Value),
                new SqlParameter("@Version", model.Version),
                new SqlParameter("@UpdateLog", model.UpdateLog ?? (object)DBNull.Value),
                new SqlParameter("@AuditRecord", model.AuditRecord ?? (object)DBNull.Value)
            };

            return SqlHelper.ExecuteNonQuery(sql, param);
        }

        /// <summary>
        /// 逻辑删除运动数据
        /// </summary>
        public int DeleteExercise(int exerciseId)
        {
            string sql = "DELETE FROM Diabetes_Exercise_Energy_Expenditure WHERE ExerciseID = @ExerciseID";
            SqlParameter[] param = { new SqlParameter("@ExerciseID", exerciseId) };
            return SqlHelper.ExecuteNonQuery(sql, param);
        }
        #endregion

        #region 批量操作
        /// <summary>
        /// 批量更新启用状态
        /// </summary>
        public int BatchUpdateEnableStatus(List<int> exerciseIdList, string enableStatus, string updateUser)
        {
            if (exerciseIdList == null || exerciseIdList.Count == 0) return 0;
            string exerciseIds = string.Join(",", exerciseIdList);
            string sql = $@"
                UPDATE Diabetes_Exercise_Energy_Expenditure 
                SET EnableStatus = @EnableStatus, UpdateTime = GETDATE(), UpdateUser = @UpdateUser 
                WHERE ExerciseID IN ({exerciseIds})";
            SqlParameter[] param = {
                new SqlParameter("@EnableStatus", enableStatus),
                new SqlParameter("@UpdateUser", updateUser)
            };
            return SqlHelper.ExecuteNonQuery(sql, param);
        }

        /// <summary>
        /// 批量审核操作
        /// </summary>
        public int BatchAuditExercise(List<int> exerciseIdList, string auditStatus, string auditRecord, string auditUser)
        {
            if (exerciseIdList == null || exerciseIdList.Count == 0) return 0;
            string exerciseIds = string.Join(",", exerciseIdList);
            string sql = $@"
                UPDATE Diabetes_Exercise_Energy_Expenditure 
                SET AuditStatus = @AuditStatus, AuditRecord = @AuditRecord, UpdateTime = GETDATE(), UpdateUser = @AuditUser 
                WHERE ExerciseID IN ({exerciseIds})";
            SqlParameter[] param = {
                new SqlParameter("@AuditStatus", auditStatus),
                new SqlParameter("@AuditRecord", auditRecord),
                new SqlParameter("@AuditUser", auditUser)
            };
            return SqlHelper.ExecuteNonQuery(sql, param);
        }
        #endregion

        #region 辅助方法
        /// <summary>
        /// 获取所有运动分类列表
        /// </summary>
        public DataTable GetAllExerciseCategory()
        {
            string sql = @"SELECT DISTINCT ExerciseCategory FROM Diabetes_Exercise_Energy_Expenditure ORDER BY ExerciseCategory";
            return SqlHelper.ExecuteDataTable(sql);
        }

        /// <summary>
        /// 获取所有运动名称列表（版本管理用）
        /// </summary>
        public DataTable GetAllExerciseNameList()
        {
            string sql = @"SELECT ExerciseID, ExerciseName FROM Diabetes_Exercise_Energy_Expenditure WHERE EnableStatus = '启用' ORDER BY ExerciseName";
            return SqlHelper.ExecuteDataTable(sql);
        }

        /// <summary>
        /// 生成唯一运动编码
        /// </summary>
        public string GenerateExerciseCode()
        {
            string prefix = "E";
            string date = DateTime.Now.ToString("yyMMdd");
            string sql = $"SELECT MAX(ExerciseCode) FROM Diabetes_Exercise_Energy_Expenditure WHERE ExerciseCode LIKE '{prefix}{date}%'";
            object maxCode = SqlHelper.ExecuteScalar(sql);
            int serialNo = 1;
            if (maxCode != null && maxCode != DBNull.Value)
            {
                string maxCodeStr = maxCode.ToString();
                if (maxCodeStr.Length >= 8)
                {
                    string serialStr = maxCodeStr.Substring(7);
                    if (int.TryParse(serialStr, out int temp))
                    {
                        serialNo = temp + 1;
                    }
                }
            }
            return $"{prefix}{date}{serialNo:D3}";
        }

        /// <summary>
        /// 校验运动名称是否重复
        /// </summary>
        public bool CheckExerciseNameExists(string exerciseName, int excludeId = 0)
        {
            string sql = "SELECT COUNT(1) FROM Diabetes_Exercise_Energy_Expenditure WHERE ExerciseName = @ExerciseName";
            List<SqlParameter> paramList = new List<SqlParameter> { new SqlParameter("@ExerciseName", exerciseName) };
            if (excludeId > 0)
            {
                sql += " AND ExerciseID != @ExcludeId";
                paramList.Add(new SqlParameter("@ExcludeId", excludeId));
            }
            return Convert.ToInt32(SqlHelper.ExecuteScalar(sql, paramList.ToArray())) > 0;
        }
        #endregion

        #region 审核/版本管理专用查询
        /// <summary>
        /// 查询待审核数据列表
        /// </summary>
        public DataTable GetAuditList(string auditStatus, string uploader)
        {
            StringBuilder sql = new StringBuilder(@"
                SELECT 
                    ExerciseID, ExerciseCode, ExerciseName, CreateUser AS Uploader, CreateTime AS UploadTime, 
                    AuditStatus, UpdateLog AS CheckTip
                FROM Diabetes_Exercise_Energy_Expenditure 
                WHERE 1=1 ");

            List<SqlParameter> paramList = new List<SqlParameter>();
            if (!string.IsNullOrWhiteSpace(auditStatus) && auditStatus != "全部")
            {
                sql.Append(" AND AuditStatus = @AuditStatus ");
                paramList.Add(new SqlParameter("@AuditStatus", auditStatus));
            }
            if (!string.IsNullOrWhiteSpace(uploader) && uploader != "全部")
            {
                sql.Append(" AND CreateUser LIKE @Uploader ");
                paramList.Add(new SqlParameter("@Uploader", $"%{uploader}%"));
            }

            sql.Append(" ORDER BY CreateTime DESC ");
            return SqlHelper.ExecuteDataTable(sql.ToString(), paramList.ToArray());
        }

        /// <summary>
        /// 查询运动版本历史记录
        /// </summary>
        public DataTable GetVersionHistory(int exerciseId)
        {
            string sql = @"
                SELECT Version, UpdateTime, UpdateUser, UpdateLog, AuditStatus
                FROM Diabetes_Exercise_Energy_Expenditure 
                WHERE ExerciseID = @ExerciseID
                ORDER BY UpdateTime DESC";
            SqlParameter[] param = { new SqlParameter("@ExerciseID", exerciseId) };
            return SqlHelper.ExecuteDataTable(sql, param);
        }
        #endregion
    }
}