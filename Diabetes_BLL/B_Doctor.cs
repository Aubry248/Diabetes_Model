using DAL;
using Model;

namespace BLL
{
    public class B_Doctor
    {
        public static Doctor GetDoctorById(int doctorId)
        {
            return D_Doctor.GetDoctorById(doctorId);
        }

        public static int UpdateDoctor(Doctor doctor)
        {
            return D_Doctor.UpdateDoctor(doctor);
        }
    }
}