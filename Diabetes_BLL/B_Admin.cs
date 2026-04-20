using DAL;
using Model;

namespace BLL
{
    public class B_Admin
    {
        public static Admin GetAdminById(int adminId)
        {
            return D_Admin.GetAdminById(adminId);
        }

        public static int UpdateAdmin(Admin admin)
        {
            return D_Admin.UpdateAdmin(admin);
        }
    }
}