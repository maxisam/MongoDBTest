using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoDBTest
{
	public class ThreadEntry
	{
		[BsonRepresentation(BsonType.ObjectId)]
		public string Id { get; set; }
		public string Name { get; set; }
		public int Iteration { get; set; }
		public string Note { get; set; }
	}
}
