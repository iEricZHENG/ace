using System;
using System.Configuration;
using Nest;
using Elasticsearch.Net;

namespace App.ESLog.ES
{
    public class ESClient
    {
        static ElasticClient client;
        public static ElasticClient Client
        {
            get
            {
                if (client == null)
                {
                    var connections = ConfigurationManager.ConnectionStrings["elasticsearch"].ConnectionString.Split(',');
                    var nodes = new Uri[connections.Length];
                    for (int i = 0; i < connections.Length; i++)
                    {
                        nodes[i] = new Uri(connections[i]);
                    };

                    var pool = new StaticConnectionPool(nodes);
                    var settings = new ConnectionSettings(pool);
                    client = new ElasticClient(settings);
                }
                return client;
            }
        }
    }
}
