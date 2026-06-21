using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ICPlatformTools
{
    public class TypeResolve<T>
    {
        public static List<T> GetInstanceList()
        {
            var list = new List<T>();
            foreach (var type in Assembly.GetCallingAssembly().GetTypes())
            {
                if (type.GetInterface(typeof(T).Name) != null && type.IsClass)
                {
                    try
                    {
                        list.Add((T)Activator.CreateInstance(type));
                    }
                    catch { }
                }
            }
            return list;
        }

        public static List<Type> GetTypeList()
        {
            var list = new List<Type>();
            foreach (var assem in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (var type in assem.GetTypes())
                    {
                        if (type.GetInterface(typeof(T).Name) != null && type.IsClass)
                        {
                            list.Add(type);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Log.Error(ex.Message, ex);
                }
            }
            return list;
        }

        public static T Resolve(string typeName)
        {
            var assem = typeof(T).Assembly;
            var type = assem.GetType(typeName);
            return GetIns(type);
        }

        public static T Resolve(Type type)
        {
            return GetIns(type);
        }

        private static T GetIns(Type type)
        {
            if (type != null)
            {
                try
                {
                    var tType = typeof(T);
                    if (type == tType || type.IsAssignableFrom(tType) || (tType.IsInterface && type.GetInterface(tType.Name) != null))
                    {
                        var ins = (T)Activator.CreateInstance(type);
                        return ins;
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Log.Error(ex);
                }
            }
            return default(T);
        }
    }
}
