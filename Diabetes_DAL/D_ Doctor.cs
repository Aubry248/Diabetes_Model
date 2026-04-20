using Model;
using System;
using System.Data;
using System.Data.SqlClient;
using Tools;

namespace DAL
{
    public class D_Doctor
    {
        /// <summary>
        /// 根据ID查询医生信息
        /// </summary>
        public static Doctor GetDoctorById(int doctorId)
        {
            string sql = @"
                SELECT u.*,d.* 
                FROM t_user u
                INNER JOIN t_doctor d ON u.user_id = d.doctor_id
                WHERE u.user_id=@DoctorId AND u.user_type=2";

            SqlParameter[] paras = {
                new SqlParameter("@DoctorId",doctorId)
            };

            DataTable dt = SqlHelper.ExecuteDataTable(sql, paras);
            if (dt.Rows.Count == 0) return null;

            DataRow row = dt.Rows[0];
            Doctor doctor = new Doctor();
            doctor.doctor_id = Convert.ToInt32(row["doctor_id"]);
            doctor.title = row["title"]?.ToString();
            doctor.department = row["department"]?.ToString();
            doctor.hospital = row["hospital"]?.ToString();
            doctor.qualification_no = row["qualification_no"]?.ToString();
            doctor.specialty = row["specialty"]?.ToString();
            doctor.create_time = Convert.ToDateTime(row["create_time"]);
            doctor.update_time = Convert.ToDateTime(row["update_time"]);
            doctor.data_version = Convert.ToInt32(row["data_version"]);

            return doctor;
        }

        /// <summary>
        /// 更新医生信息
        /// </summary>
        public static int UpdateDoctor(Doctor doctor)
        {
            string sql = @"
                UPDATE t_doctor SET
                title=@Title,
                department=@Department,
                hospital=@Hospital,
                qualification_no=@QualificationNo,
                specialty=@Specialty,
                update_time=GETDATE(),
                data_version=data_version+1
                WHERE doctor_id=@DoctorId";

            SqlParameter[] paras = {
                new SqlParameter("@Title",(object)doctor.title??DBNull.Value),
                new SqlParameter("@Department",(object)doctor.department??DBNull.Value),
                new SqlParameter("@Hospital",(object)doctor.hospital??DBNull.Value),
                new SqlParameter("@QualificationNo",(object)doctor.qualification_no??DBNull.Value),
                new SqlParameter("@Specialty",(object)doctor.specialty??DBNull.Value),
                new SqlParameter("@DoctorId",doctor.doctor_id)
            };

            return SqlHelper.ExecuteNonQuery(sql, paras);
        }
    }
}