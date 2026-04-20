using DAL;
using Diabetes_DAL;
using Model;
using System.Collections.Generic;

namespace BLL
{
    public class B_Permission
    {
        private readonly D_Permission dal = new D_Permission();

        /// <summary>
        /// 获取所有权限列表
        /// </summary>
        public List<Permission> GetAllPermissionList()
        {
            return dal.GetAllPermissionList();
        }
    }
}