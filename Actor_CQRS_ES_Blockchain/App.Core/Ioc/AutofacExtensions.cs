using System;
using System.Linq;
using System.Collections.Generic;
using Autofac;
using System.IO;
using System.Reflection;

namespace App.Core.Ioc
{
    public static class AutofacExtensions
    {
        /// <summary>
        /// 注册指定文件目录下面的所有文件
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="fileFilter"></param>
        /// <param name="directoryPaths"></param>
        public static void RegisterTypeFromCurrentDomain(this ContainerBuilder builder)
        {
            var assemblyList = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.IndexOf("App") != -1);
            foreach (var assembly in assemblyList)
            {
                RegisterTypeFromAssembly(builder, assembly);
            }
        }
        /// <summary>
        /// 注册指定文件路径的文件
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="fileFullName"></param>
        public static void RegisterTypeFromAssembly(this ContainerBuilder builder,Assembly assembly)
        {
            var allClass = from types in assembly.GetExportedTypes()
                           where types.IsClass
                           select types;
            foreach (var c in allClass)
            {
                var exportAttrs = c.GetCustomAttributes(typeof(IocExportAttribute), false);
                if (exportAttrs.Length > 0)
                {
                    var exportAttr = exportAttrs[0] as IocExportAttribute;
                    if (null != exportAttr.ContractKey)
                    {
                        if (exportAttr.SingleInstance)
                            builder.RegisterType(c).Keyed(exportAttr.ContractKey, exportAttr.ContractType).SingleInstance();
                        else
                            builder.RegisterType(c).Keyed(exportAttr.ContractKey, exportAttr.ContractType);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(exportAttr.ContractName))
                        {
                            if (exportAttr.SingleInstance)
                                builder.RegisterType(c).Named(exportAttr.ContractName, exportAttr.ContractType).SingleInstance();
                            else
                                builder.RegisterType(c).Named(exportAttr.ContractName, exportAttr.ContractType);
                        }
                        else
                        {
                            if (exportAttr.SingleInstance)
                                builder.RegisterType(c).As(exportAttr.ContractType).SingleInstance();
                            else
                                builder.RegisterType(c).As(exportAttr.ContractType);
                        }
                    }
                }
            }
        }
    }
}
