using MongoDB.Driver;

namespace App.Framework.Db
{
    public class MongoStorage
    {
        static MongoStorage()
        {
            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["mongodbs"].ConnectionString;
            _Client = new MongoClient(connStr);
        }
        static MongoClient _Client;
        protected static MongoClient Client
        {
            get
            {
                return _Client;
            }
        }
        public static IMongoDatabase GetDatabase(string name)
        {
            return Client.GetDatabase(name);
        }
        public static IMongoCollection<T> GetCollection<T>(string databaseName, string collectionName)
        {
            return Client.GetDatabase(databaseName).GetCollection<T>(collectionName);
        }
    }
}
