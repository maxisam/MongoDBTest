using MongoDB.Driver;
using MongoDBTest;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;

namespace NUnitTest
{
	public class ConnectionTest
	{
		private IMongoCollection<ThreadEntry> Collection;
		private MongoClientService ClientService;
		private int Exceptions = 0;
		private int MaxThreads = 7000;
		private int FinishedThread = 0;
		private readonly Random Random = new Random();
		private int MaxWait = 1000;
		private int Iterations = 10;

		public ConnectionTest()
		{
		}

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			ClientService = new MongoClientService();
			Collection = ClientService.GetMongoCollection<ThreadEntry>(connectionString: MongoClientService.DefaultMongodbConnString, databaseName: "test", collectionName: "nunit_test");
		}

		[Test]
		public void TestConnection()
		{
			Assert.AreEqual(Collection.CollectionNamespace.FullName, "test.nunit_test");
		}

		[Test]
		public void TestThreadAsync()
		{

			for (int i = 0; i < MaxThreads; i++)
			{
				var thread = new Thread(ThreadInsertMethodAsync)
				{
					IsBackground = true,
					Name = string.Format("Thread {0}", i)
				};

				thread.Start(string.Format("Thread {0}", i));
			}

			//wait for all of them to finish
			while (FinishedThread < MaxThreads - 1 && Exceptions == 0)
			{
				Thread.Sleep(MaxWait);
			}
			Assert.That(ClientService.MongoCollections.Count(), Is.EqualTo(1));
			Assert.AreEqual(0, Exceptions, "Expected no exceptions on threads");
		}

		private async void ThreadInsertMethodAsync(object threadName)
		{
			try
			{
				for (int i = 0; i < Iterations; i++)
				{
					var name = threadName + " Test " + i;
					var entry = new ThreadEntry { Iteration = i, Name = name };
					Collection.InsertOne(entry);
					IAsyncCursor<ThreadEntry> cursor = await Collection.FindAsync(Builders<ThreadEntry>.Filter.Eq("Name", entry.Name));
					var result = await cursor.ToListAsync();
					Assert.That(result.FirstOrDefault().Id, Is.EqualTo(entry.Id));
					var deleteResult = await Collection.DeleteOneAsync(Builders<ThreadEntry>.Filter.Eq("Id", entry.Id));
					Assert.That(deleteResult.DeletedCount, Is.EqualTo(1));
					Thread.Sleep(Random.Next(MaxWait));
				}
			}
			catch (Exception exception)
			{
				Console.Out.WriteLine("Ërror in Method2 Thread {0}", exception);
				Exceptions++;
			}
			finally
			{
				FinishedThread++;
			}
		}
	}
}