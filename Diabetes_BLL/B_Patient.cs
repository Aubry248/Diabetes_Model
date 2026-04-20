using DAL;
using Model;

namespace BLL
{
    public class B_Patient
    {
        private static readonly D_Patient dalPatient = new D_Patient();

        /// <summary>
        /// 新增患者信息
        /// </summary>
        public static bool AddPatient(Patient patient)
        {
            return dalPatient.AddPatient(patient);
        }

        /// <summary>
        /// 修改患者信息
        /// </summary>
        public static bool UpdatePatient(Patient patient)
        {
            return dalPatient.UpdatePatient(patient);
        }

        /// <summary>
        /// 根据用户ID获取患者信息
        /// </summary>
        public static Patient GetPatientById(int patientId)
        {
            return dalPatient.GetPatientById(patientId);
        }
    }
}