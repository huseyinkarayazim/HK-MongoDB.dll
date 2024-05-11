using System;
using System.Collections.Generic;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Threading.Tasks;

namespace HK_MongoDB
{
    public class MongoManager 
    {
        #region Fields

        public MongoClientSettings setting = new MongoClientSettings();

        public MongoClient client = null;

        public IMongoDatabase db = null;

        public string Host { get { return "localhost"; } }

        public int Port { get { return 27017; } }

        //private string Username { get { return ""; } }

        //private string Password { get { return ""; } }

        //private string AuthMechanism { get { return "SCRAM-SHA-256"; } }

        //private string AuthDbName { get { return "admin"; } }

        #endregion


        //public void SetMongoCredentials()
        //{
        //    MongoIdentity identity = new MongoInternalIdentity(AuthDbName, Username);
        //    MongoIdentityEvidence evidence = new PasswordEvidence(Password);
        //    setting.Credential = new MongoCredential(AuthMechanism, identity, evidence);
        //}
        public void InitializeDatabase(string dbName)
        {
            try
            {
                db = client.GetDatabase(dbName);
            }
            catch (Exception ex)
            {
                db = null;
                throw ex;
            }
        }

        public List<string> GetCollectionsName()
        {
            try
            {
                var collections = new List<string>();
                foreach (var collection in db.ListCollectionsAsync().Result.ToListAsync<BsonDocument>().Result)
                {
                    if (!collections.Contains(collection["name"].AsString))
                        collections.Add(collection["name"].AsString);
                }

                return collections;
            }
            catch { return null; }
        }

        public void CreateConnection()
        {
            try
            {
               // SetMongoCredentials();
                client = new MongoClient(setting);
            }
            catch (Exception ex)
            {
                client = null;
                throw ex;
            }
        }

        public void CreateConnection(string connectionAddress)
        {
            try
            {
               // SetMongoCredentials();
                setting.Server = new MongoServerAddress(connectionAddress);
                client = new MongoClient(setting);
            }
            catch (Exception ex)
            {
                client = null;
                throw ex;
            }
        }

        public void CreateConnection(MongoClientSettings setting)
        {
            try
            {
                this.setting = setting;
               // SetMongoCredentials();
                client = new MongoClient(this.setting);
            }
            catch (Exception ex)
            {
                client = null;
                throw ex;
            }
        }

        public void CreateConnection(string host, int port)
        {
            try
            {
                //SetMongoCredentials();
                setting.Server = new MongoServerAddress(host, port);
                client = new MongoClient(setting);
            }
            catch (Exception ex)
            {
                client = null;
                throw ex;
            }
        }

        public void CreateCollection(string collectionName)
        {
            try
            {
                db.CreateCollection(collectionName);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task CreateCollectionAsync(string collectionName)
        {
            try
            {
                await db.CreateCollectionAsync(collectionName);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Dispose()
        {
            if (db != null)
                db = null;

            if (client != null)
                client = null;
        }

       

        public MongoManager()
        {
            try
            {
                //SetMongoCredentials();
                setting.Server = new MongoServerAddress(this.Host, this.Port);
                this.CreateConnection();
            }
            catch (Exception ex)
            {
                client = null;
                throw ex;
            }
        }




    }

}
