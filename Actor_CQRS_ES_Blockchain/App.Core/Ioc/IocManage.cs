using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;

namespace App.Core.Ioc
{
    public class IocManage
    {
        static ContainerBuilder builder = new ContainerBuilder();
        static IContainer container = null;
        public static void Build()
        {
            builder.RegisterTypeFromCurrentDomain();
            container = builder.Build();
        }
        public static ContainerBuilder Builder
        {
            get
            {
                return builder;
            }
        }
        public static IContainer Container
        {
            get
            {
                if (container != null)
                {
                    return container;
                }
                else
                {
                    throw new Exception("在使用前请先调用Build方法完成Ioc的构建");
                }
            }
        }
    }
}
