using System.Collections.Generic;
using DAL;

namespace BLL
{
    public class B_RolePermission
    {
        private readonly D_RolePermission dal = new D_RolePermission();

        /// <summary>
        /// 获取角色的权限ID列表
        /// </summary>
        public List<int> GetPermissionIdListByRoleId(int roleId)
        {
            return dal.GetPermissionIdListByRoleId(roleId);
        }

        /// <summary>
        /// 保存角色权限
        /// </summary>
        public string SaveRolePermission(int roleId, List<int> permissionIdList, int operateUserId)
        {
            if (roleId <= 0) return "角色ID无效";
            bool result = dal.SaveRolePermissionByTrans(roleId, permissionIdList, operateUserId);
            return result ? "ok" : "保存角色权限失败，请重试";
        }
    }
}