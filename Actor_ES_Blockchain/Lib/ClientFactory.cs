using Orleans;
using System;
using System.Threading.Tasks;

namespace Lib
{
    public class ClientFactory : IClientFactory
    {
#pragma warning disable CS8618 
        private static Func<IClientBuilder> builderFunc;
        private static IClusterClient client;
#pragma warning restore CS8618
        private static bool needReBuild = false;
        public static async Task<IClusterClient> Build(Func<IClientBuilder> builderFunc)
        {
            ClientFactory.builderFunc = builderFunc;
            client = builderFunc().Build();
            await client.Connect();
            return client;
        }
        public static void ReBuild()
        {
            if (client != null)
            {
                needReBuild = true;
            }
        }
        readonly object connectLock = new object();
        public IClusterClient GetClient()
        {
            if (!client.IsInitialized || needReBuild)
            {
                lock (connectLock)
                {
                    if (!client.IsInitialized || needReBuild)
                    {
                        if (needReBuild)
                        {
                            client.Close();
                            client.Dispose();
                        }
                        client = builderFunc().Build();
                        client.Connect().GetAwaiter().GetResult();
                        needReBuild = false;
                    }
                }
            }
            return client;
        }



    }
}
