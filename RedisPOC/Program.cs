using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Net;

namespace RedisPOC
{
    class Program
    {
        static void Main(string[] args)
        {
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost,allowAdmin=true");
            IDatabase db = redis.GetDatabase();

            db.StringSet("operationTime", DateTime.Now.ToString(), when: When.Always);

            string key = DateTime.Now.ToString("yy-MM-dd_hh-mm-ss"); //new Random().Next(10, 100).ToString();
            string value = Guid.NewGuid().ToString();

            bool isSave = db.StringSet(key, value, expiry: TimeSpan.FromSeconds(20), when: When.NotExists); //Save(db, key, value, TimeSpan.FromSeconds(10));

            Console.WriteLine($"Added key '{key}' with value '{value}' -> isSaved = '{isSave}'\n");

            string result = db.StringGet(key);
            Console.WriteLine($"Get from Redis with key '{key}' -> result = '{result}'");

            IServer server = redis.GetServer("localhost", 6379);
            EndPoint[] endpoints = redis.GetEndPoints();
            DateTime lastSave = server.LastSave();
            ClientInfo[] clients = server.ClientList();

            Console.WriteLine($"endpoints:\n{JsonConvert.SerializeObject(endpoints)}");
            Console.WriteLine($"lastSave:\n{lastSave}");
            Console.WriteLine($"clients:\n{JsonConvert.SerializeObject(clients.Select(x=> x.Name))}");

            Console.WriteLine("\n\nEnd, press any key to exit...");
            Console.Read();
        }

        private static bool Save(IDatabase db, string key, string value, TimeSpan? expiry = null)
        {
            bool isSuccess = false;

            if (db.StringGet(key) == RedisValue.Null)
            {
                db.HashIncrement(key, value, 4);
                isSuccess = db.StringSet(key, value, expiry: expiry);
            }

            return isSuccess;
        }
    }
}
