using MongoDB.Driver;
using System;
using System.Collections.Concurrent;
namespace MongoDBTest
{
	public class MongoClientService
	{
		public static string DefaultMongodbConnString = "mongodb://192.168.225.110:27017";

		public ConcurrentDictionary<string, MongoClient> MongoClients { get; set; }
		public ConcurrentDictionary<Tuple<string, string, string>, dynamic> MongoCollections;
		public ConcurrentDictionary<Tuple<string, string>, IMongoDatabase> MongoDatabases;
		public MongoClientService()
		{
			this.MongoClients = new ConcurrentDictionary<string, MongoClient>();
			this.MongoDatabases = new ConcurrentDictionary<Tuple<string, string>, IMongoDatabase>();
			this.MongoCollections = new ConcurrentDictionary<Tuple<string, string, string>, dynamic>();
		}
		public IMongoCollection<T> GetMongoCollection<T>(string connectionString, string databaseName, string collectionName)
		{
			if (string.IsNullOrWhiteSpace(connectionString))
				connectionString = DefaultMongodbConnString;

			var collection = new Tuple<string, string, string>(connectionString, databaseName, collectionName);

			//check for and fill any needed clients/dbs/collections
			if (!MongoCollections.ContainsKey(collection))
			{
				var db = new Tuple<string, string>(connectionString, databaseName);

				if (!MongoDatabases.ContainsKey(db))
				{
					if (!MongoClients.ContainsKey(connectionString))
					{
						var client = CreateMongoClient(connectionString);
						MongoClients[connectionString] = client;
					}
					var newDb = MongoClients[connectionString].GetDatabase(databaseName);
					MongoDatabases[db] = newDb;
				}
				var newCollection = MongoDatabases[db].GetCollection<T>(collectionName);
				MongoCollections[collection] = newCollection;
			}
			return MongoCollections[collection] as IMongoCollection<T>;
		}
		private MongoClient CreateMongoClient(string connectionstring)
		{
			MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl(connectionstring));
			settings.ConnectTimeout = TimeSpan.FromMinutes(20);
			settings.SocketTimeout = TimeSpan.FromMinutes(20);
			settings.ServerSelectionTimeout = TimeSpan.FromMinutes(20);
			settings.MaxConnectionLifeTime = TimeSpan.FromMinutes(840);
			settings.MaxConnectionPoolSize = 10000;
			settings.WaitQueueSize = 10000;
			settings.WaitQueueTimeout = TimeSpan.FromMinutes(20);

			if (settings.UseTls)
			{
				settings.SslSettings = new SslSettings()
				{
					EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12
				};
			}
			// Create a MongoClient object by using the connection string
			return new MongoClient(settings);
		}
	}
}
