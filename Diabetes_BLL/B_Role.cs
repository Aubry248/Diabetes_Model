using System.Collections.Generic;
using DAL;
using Model;

namespace BLL
{
    public class B_Role
    {
        private readonly D_Role dal = new D_Role();

        /// <summary>
        /// 获取所有角色列表
        /// </summary>
        public List<Role> GetAllRoleList()
        {
            return dal.GetAllRoleList();
        }

        /// <summary>
        /// 根据ID获取角色
        /// </summary>
        public Role GetRoleById(int roleId)
        {
            return dal.GetRoleById(roleId);
        }

        /// <summary>
        /// 新增角色
        /// </summary>
        public string AddRole(Role role)
        {
            // 校验重复
            if (dal.CheckRoleCodeExist(role.role_code))
            {
                return "角色编码已存在，请勿重复添加";
            }
            if (dal.CheckRoleNameExist(role.role_name))
            {
                return "角色名称已存在，请勿重复添加";
            }
            int roleId = dal.AddRole(role);
            return roleId > 0 ? "ok" : "新增角色失败，请重试";
        }

        /// <summary>
        /// 修改角色
        /// </summary>
        public string UpdateRole(Role role)
        {
            // 校验重复
            if (dal.CheckRoleNameExist(role.role_name, role.role_id))
            {
                return "角色名称已被其他角色使用";
            }
            bool result = dal.UpdateRole(role);
            return result ? "ok" : "修改角色失败，请重试";
        }

        /// <summary>
        /// 删除角色
        /// </summary>
        public string DeleteRole(int roleId, int updateBy)
        {
            // 校验是否被用户关联
            if (dal.CheckRoleHasUser(roleId))
            {
                return "该角色已被用户关联，无法删除";
            }
            bool result = dal.DeleteRole(roleId, updateBy);
            return result ? "ok" : "删除角色失败，请重试";
        }

        /// <summary>
        /// 校验角色是否被用户关联
        /// </summary>
        public bool CheckRoleHasUser(int roleId)
        {
            return dal.CheckRoleHasUser(roleId);
        }
    }
}