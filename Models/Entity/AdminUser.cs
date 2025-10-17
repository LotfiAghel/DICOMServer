using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
namespace Models
{
    [GeneratedControllerAttribute]
    [SelectAccess(AdminUserRole.SUPER_USER)]
    [ViewAccess(AdminUserRole.SUPER_USER)]
    [UpdateAccess(AdminUserRole.SUPER_USER)]    
    [InsertAccess(AdminUserRole.SUPER_USER)]    
    public class AdminUser : BaseEntity, IAdminUser
    {

        public string firstName { get; set; }
        public string lastName { get; set; }
        public string username { get; set; }
        

        public string password { get; set; }

        [MultiSelect]
        public List<AdminUserRole> roles{ get; set; }

        public bool checkPermission<T2>(Type T) where T2 : ACLAtr
        {
            //return true;
            
            try
            {
                var md=MDTypeInfo.get(T);
                var adminWriteBan=md.getAttrs().OfType<T2>().FirstOrDefault();
                //var adminWriteBan = T.GetCustomAttributes(typeof(T2), true).OfType<T2>().FirstOrDefault();
                if (adminWriteBan == null || adminWriteBan.kinds==null || !adminWriteBan.kinds.Intersect(roles).Any() )
                    return false;
            }
            catch
            {
                return false;
            };
            return true;

        }

        public bool checkPermission<T2>(MemberInfo prop) where T2 : ACLAtr
        {
            if (prop is PropertyInfo memInfo)
                return checkPermission<T2>(memInfo);
            if (prop is MethodInfo methodInfo)
                return checkPermission<T2>(methodInfo);
            return false;
        }
        public bool checkPermission<T2>(PropertyInfo prop) where T2 : ACLAtr
        {
           
            if (checkPermission<T2>(prop.PropertyType.GetGenericArguments()[0]))
                return true;
            
            try
            {
                var adminWriteBan = prop.GetCustomAttributes(typeof(T2), true).OfType<T2>().FirstOrDefault();
                if (adminWriteBan == null || adminWriteBan.kinds==null  || adminWriteBan.kinds.Intersect(roles).Count() == 0 )
                    return false;
            }
            catch
            {
                return false;
            };
            return true;

        }
        public bool checkPermission<T2>(MethodInfo prop) where T2 : ACLAtr
        {
          
            if (checkPermission<T2>(prop.ReturnType.GetGenericArguments()[0]))
                return true;
            try
            {
                var adminWriteBan = prop.GetCustomAttributes(typeof(T2), true).OfType<T2>().FirstOrDefault();
                if (adminWriteBan == null || adminWriteBan.kinds==null ||  adminWriteBan.kinds.Intersect(roles).Count() == 0 )
                    return false;
            }
            catch
            {
                return false;
            };
            return true;

        }

    }







}
