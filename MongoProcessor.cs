using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace HK_MongoDB
{
    public static class MongoProcessor
    {
        public static MongoManager mongoManager = null;
        public static Enum.DBName DBName = Enum.DBName.HK_Database;
        public static Enum.RequestSource RequestSource = Enum.RequestSource.Web;
        public static bool isLocalhost = true;

        #region General

        public static string GetObjectId()
        {
            try
            {
                return ObjectId.GenerateNewId(DateTime.Now).ToString();
            }
            catch { }

            return string.Empty;
        }

        public static void ReMongoDBConnection(string host)
        {
            try
            {
                MongoProcessor.mongoManager.CreateConnection(host, MongoProcessor.mongoManager.Port);
                mongoManager.InitializeDatabase(DBName.ToString());

                if (RequestSource == Enum.RequestSource.Client)
                {
                    if (host == mongoManager.Host)
                        isLocalhost = true;
                    else
                        isLocalhost = false;
                }
                else
                    isLocalhost = true;
            }
            catch (Exception ex) { throw ex; }
        }

        public static bool CheckMongoDBConnection()
        {
            try
            {
                if (mongoManager == null)
                {
                    mongoManager = new MongoManager();
                    mongoManager.InitializeDatabase(DBName.ToString());
                }

                else
                {
                    if (mongoManager.db == null)
                        mongoManager.InitializeDatabase(DBName.ToString());
                }

                return true;
            }
            catch (Exception ex) { mongoManager = null; throw ex; }
        }

        public static bool IsServerAvailable()
        {
            try
            {
                if (MongoProcessor.CheckMongoDBConnection())
                    return MongoProcessor.mongoManager.db.RunCommandAsync((Command<BsonDocument>)"{ping:1}").Wait(1000 * 20);
            }
            catch { }

            return false;
        }

        public static bool IsIndexExist<T>(string indexName)
        {
            try
            {
                if (MongoProcessor.mongoManager != null)
                {
                    var collectionIndex = MongoProcessor.mongoManager.db.GetCollection<T>(typeof(T).Name);

                    foreach (var item in collectionIndex.Indexes.List().ToList())
                    {
                        if (item["name"] == indexName)
                            return true;
                    }
                }
            }
            catch (Exception ex) { throw ex; }

            return false;
        }

        public static bool CreateIndex<T>(string fieldName, string indexName)
        {
            try
            {
                if (MongoProcessor.mongoManager != null)
                {
                    if (!IsIndexExist<T>(indexName))
                    {
                        var collectionIndex = MongoProcessor.mongoManager.db.GetCollection<T>(typeof(T).Name);
                        var options = new CreateIndexOptions() { Unique = true, Name = indexName };
                        var field = new StringFieldDefinition<T>(fieldName);
                        var indexDefinition = new IndexKeysDefinitionBuilder<T>().Ascending(field);
                        var collection = collectionIndex
                            .Indexes
                            .CreateOne(indexDefinition, options);

                        return true;
                    }
                }
            }
            catch (Exception ex) { throw ex; }

            return true;
        }

        public static void Dispose()
        {
            try
            {
                mongoManager.Dispose();
                mongoManager = null;
            }
            catch { }
        }

        public static void CreateDatetimeProperty<T>(ref T instance)
        {
            try
            {
                var property = instance.GetType().GetProperty("CreationDate");

                if (property != null)
                    property.SetValue(instance, DateTime.Now, null);

                property = instance.GetType().GetProperty("ModifiedDate");

                if (property != null)
                    property.SetValue(instance, DateTime.Now, null);
            }
            catch { }
        }

        public static void CreateDatetimeProperty<T>(ref IEnumerable<T> instanceList)
        {
            try
            {
                foreach (var instance in instanceList)
                {
                    var property = instance.GetType().GetProperty("CreationDate");

                    if (property != null)
                        property.SetValue(instance, DateTime.Now, null);

                    property = instance.GetType().GetProperty("ModifiedDate");

                    if (property != null)
                        property.SetValue(instance, DateTime.Now, null);
                }
            }
            catch { }
        }

        public static void CreateIdProperty<T>(ref T instance)
        {
            try
            {
                var property = instance.GetType().GetProperty("Id");

                if (property != null)
                {
                    var value = property.GetValue(instance);

                    if (value == null)
                        property.SetValue(instance, ObjectId.GenerateNewId(DateTime.Now).ToString(), null);

                    else
                    {
                        if (string.IsNullOrEmpty(value.ToString()))
                            property.SetValue(instance, ObjectId.GenerateNewId(DateTime.Now).ToString(), null);
                    }
                }
            }
            catch { }
        }

        public static void CreateIdProperty<T>(ref IEnumerable<T> instanceList)
        {
            try
            {
                foreach (var instance in instanceList)
                {
                    var property = instance.GetType().GetProperty("Id");

                    if (property != null)
                    {
                        var value = property.GetValue(instance);

                        if (value == null)
                            property.SetValue(instance, ObjectId.GenerateNewId(DateTime.Now).ToString(), null);

                        else
                        {
                            if (string.IsNullOrEmpty(value.ToString()))
                                property.SetValue(instance, ObjectId.GenerateNewId(DateTime.Now).ToString(), null);
                        }
                    }
                }
            }
            catch { }
        }

        public static void CheckCollectionExist<T>()
        {
            try
            {
                if (!MongoProcessor.mongoManager.GetCollectionsName().Contains(typeof(T).Name))
                    MongoProcessor.mongoManager.CreateCollection(typeof(T).Name);
            }
            catch { }
        }
        #endregion
        #region AddOrUpdate Process

        public static bool AddOrUpdateEncrypt<T>(T _instance, Expression<Func<T, bool>> LambdaExpression)
        {
            try
            {
                var instance = MongoProcessor.GetOne<T>(false, LambdaExpression);

                if (instance == null)
                {
                    if (MongoProcessor.Add<T>(_instance, false))
                        return true;
                }
                else
                {
                    if (MongoProcessor.Update<T>(LambdaExpression, _instance, false))
                        return true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return false;
        }

        public static bool AddOrUpdate<T>(T _instance, Expression<Func<T, bool>> LambdaExpression)
        {
            try
            {
                var instance = MongoProcessor.GetOne<T>(LambdaExpression);

                if (instance == null)
                {
                    if (MongoProcessor.Add<T>(_instance))
                        return true;
                }
                else
                {
                    if (MongoProcessor.Update<T>(LambdaExpression, _instance))
                        return true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return false;
        }

        #endregion
        #region Add Process

        public static bool Add<T>(T instance)
        {
            try
            {
                if (MongoProcessor.CheckMongoDBConnection())
                {
                    
                    CheckCollectionExist<T>();
                    CreateDatetimeProperty<T>(ref instance);
                    CreateIdProperty<T>(ref instance);
                    Encrypt<T>(ref instance);
                    var collection = MongoProcessor.mongoManager.db.GetCollection<T>(typeof(T).Name);
                    collection.InsertOne(instance);
                    return true;
                }
            }
            catch (Exception ex) { throw ex; }

            return false;
        }

        public static async Task<bool> AddAsync<T>(T instance)
        {
            try
            {
                if (MongoProcessor.CheckMongoDBConnection())
                {
                   
                    CheckCollectionExist<T>();
                    CreateDatetimeProperty<T>(ref instance);
                    CreateIdProperty<T>(ref instance);
                    Encrypt<T>(ref instance);
                    var collection = MongoProcessor.mongoManager.db.GetCollection<T>(typeof(T).Name);
                    await collection.InsertOneAsync(instance);
                    Decrypt<T>(ref instance);
                    return true;

                }
            }
            catch (Exception ex) { throw ex; }

            return false;
        }

        public static bool AddRange<T>(IEnumerable<T> instanceList)
        {
            try
            {
                if (MongoProcessor.CheckMongoDBConnection())
                {
                   
                    CheckCollectionExist<T>();
                    CreateDatetimeProperty<T>(ref instanceList);
                    CreateIdProperty<T>(ref instanceList);
                    Encrypt<T>(ref instanceList);
                    var collection = MongoProcessor.mongoManager.db.GetCollection<T>(typeof(T).Name);
                    collection.InsertMany(instanceList);
                    Decrypt<T>(ref instanceList);
                    return true;
                }
            }
            catch (Exception ex) { throw ex; }

            return false;
        }

        public static async Task<bool> AddRangeAsync<T>(IEnumerable<T> instanceList)
        {
            try
            {
                if (MongoProcessor.CheckMongoDBConnection())
                {
                    
                    CheckCollectionExist<T>();
                    CreateDatetimeProperty<T>(ref instanceList);
                    CreateIdProperty<T>(ref instanceList);
                    Encrypt<T>(ref instanceList);
                    var collection = MongoProcessor.mongoManager.db.GetCollection<T>(typeof(T).Name);
                    await collection.InsertManyAsync(instanceList);
                    Decrypt<T>(ref instanceList);
                    return true;
                }
            }
            catch (Exception ex) { throw ex; }

            return false;
        }

        public static bool Add<T>(T instance, bool encryptDecryptStatus)
        {
            try
            {
                if (MongoProcessor.CheckMongoDBConnection())
                {
                    
                    CheckCollectionExist<T>();
                    CreateDatetimeProperty<T>(ref instance);
                    CreateIdProperty<T>(ref instance);

                    if (encryptDecryptStatus)
                        Encrypt<T>(ref instance);
                    var collection = MongoProcessor.mongoManager.db.GetCollection<T>(typeof(T).Name);
                    collection.InsertOne(instance);

                    if (encryptDecryptStatus)
                        Decrypt<T>(ref instance);

                    return true;


                }
            }
            catch (Exception ex) { throw ex; }

            return false;
        }

        public static async Task<bool> AddAsync<T>(T instance, bool encryptDecryptStatus)
        {
            try
            {
                if (MongoProcessor.CheckMongoDBConnection())
                {
                    
                    CheckCollectionExist<T>();
                    CreateDatetimeProperty<T>(ref instance);
                    CreateIdProperty<T>(ref instance);

                    if (encryptDecryptStatus)
                        Encrypt<T>(ref instance);
                    var collection = MongoProcessor.mongoManager.db.GetCollection<T>(typeof(T).Name);
                    await collection.InsertOneAsync(instance);

                    if (encryptDecryptStatus)
                        Decrypt<T>(ref instance);

                    return true;


                }
            }
            catch (Exception ex) { throw ex; }

            return false;
        }

        public static bool AddRange<T>(IEnumerable<T> instanceList, bool encryptDecryptStatus)
        {
            try
            {
                if (MongoProcessor.CheckMongoDBConnection())
                {
                    
                    CheckCollectionExist<T>();
                    CreateDatetimeProperty<T>(ref instanceList);
                    CreateIdProperty<T>(ref instanceList);

                    if (encryptDecryptStatus)
                        Encrypt<T>(ref instanceList);
                    var collection = MongoProcessor.mongoManager.db.GetCollection<T>(typeof(T).Name);
                    collection.InsertMany(instanceList);

                    if (encryptDecryptStatus)
                        Decrypt<T>(ref instanceList);

                    return true;


                }
            }
            catch (Exception ex) { throw ex; }

            return false;
        }

        public static async Task<bool> AddRangeAsync<T>(IEnumerable<T> instanceList, bool encryptDecryptStatus)
        {
            try
            {
                if (MongoProcessor.CheckMongoDBConnection())
                {
                    
                    CheckCollectionExist<T>();
                    CreateDatetimeProperty<T>(ref instanceList);
                    CreateIdProperty<T>(ref instanceList);

                    if (encryptDecryptStatus)
                        Encrypt<T>(ref instanceList);
                    var collection = MongoProcessor.mongoManager.db.GetCollection<T>(typeof(T).Name);
                    await collection.InsertManyAsync(instanceList);

                    if (encryptDecryptStatus)
                        Decrypt<T>(ref instanceList);

                    return true;


                }
            }
            catch (Exception ex) { throw ex; }

            return false;
        }

        #endregion
        #region Delete Process
        public static bool Delete<T>(Expression<Func<T, bool>> LambdaExpression)
        {
            try
            {
                if (MongoProcessor.CheckMongoDBConnection())
                {
                    var collection = MongoProcessor.mongoManager.db.GetCollection<T>(typeof(T).Name);
                    var result = collection.DeleteOne(LambdaExpression);

                    if (result.DeletedCount > 0)
                        return true;
                }
            }
            catch (Exception ex) { throw ex; }

            return false;
        }

        public static async Task<bool> DeleteAsync<T>(Expression<Func<T, bool>> LambdaExpression)
        {
            try
            {
                if (MongoProcessor.CheckMongoDBConnection())
                {
                    var collection = MongoProcessor.mongoManager.db.GetCollection<T>(typeof(T).Name);
                    var result = await collection.DeleteOneAsync(LambdaExpression);

                    if (result.DeletedCount > 0)
                        return true;
                }
            }
            catch (Exception ex) { throw ex; }

            return false;
        }

        public static bool DeleteRange<T>(Expression<Func<T, bool>> LambdaExpression)
        {
            try
            {
                if (MongoProcessor.CheckMongoDBConnection())
                {
                    var collection = MongoProcessor.mongoManager.db.GetCollection<T>(typeof(T).Name);
                    var result = collection.DeleteMany(LambdaExpression);

                    if (result.DeletedCount > 0)
                        return true;
                }
            }
            catch (Exception ex) { throw ex; }

            return false;
        }

        public static async Task<bool> DeleteRangeAsync<T>(Expression<Func<T, bool>> LambdaExpression)
        {
            try
            {
                if (MongoProcessor.CheckMongoDBConnection())
                {
                    var collection = MongoProcessor.mongoManager.db.GetCollection<T>(typeof(T).Name);
                    var result = await collection.DeleteManyAsync(LambdaExpression);

                    if (result.DeletedCount > 0)
                        return true;
                }
            }
            catch (Exception ex) { throw ex; }

            return false;
        }

        #endregion
        #region Get Process

        public static List<T> Get<T>()
        {
            try
            {
                if (MongoProcessor.CheckMongoDBConnection())
                {
                    var collection = MongoProcessor.mongoManager.db.GetCollection<T>(typeof(T).Name);
                    var instanceList = collection.AsQueryable<T>().ToList();

                    if (instanceList != null && instanceList.Count > 0)
                        Decrypt<T>(ref instanceList);

                    return instanceList;
                }
            }
            catch (Exception ex) { throw ex; }

            return null;
        }

        public static async Task<List<T>> GetAsync<T>()
        {
            try
            {
                if (MongoProcessor.CheckMongoDBConnection())
                {
                    var collection = MongoProcessor.mongoManager.db.GetCollection<T>(typeof(T).Name);
                    var instanceList = await collection.AsQueryable<T>().ToListAsync();

                    if (instanceList != null && instanceList.Count > 0)
                        Decrypt<T>(ref instanceList);

                    return instanceList;
                }
            }
            catch (Exception ex) { throw ex; }

            return null;
        }

        public static List<T> Get<T>(Expression<Func<T, bool>> LambdaExpression)
        {
            try
            {
                if (MongoProcessor.CheckMongoDBConnection())
                {
                    var collection = MongoProcessor.mongoManager.db.GetCollection<T>(typeof(T).Name);
                    var instanceList = collection.Find<T>(LambdaExpression).ToList();

                    if (instanceList != null && instanceList.Count > 0)
                        Decrypt<T>(ref instanceList);

                    return instanceList;
                }
            }
            catch (Exception ex) { throw ex; }

            return null;
        }

        public static async Task<List<T>> GetAsync<T>(Expression<Func<T, bool>> LambdaExpression)
        {
            try
            {
                if (MongoProcessor.CheckMongoDBConnection())
                {
                    var collection = MongoProcessor.mongoManager.db.GetCollection<T>(typeof(T).Name);
                    var instanceList = await collection.FindAsync<T>(LambdaExpression).Result.ToListAsync();

                    if (instanceList != null && instanceList.Count > 0)
                        Decrypt<T>(ref instanceList);

                    return instanceList;
                }
            }
            catch (Exception ex) { throw ex; }

            return null;
        }

        public static List<T> GetIn<T>(string fieldName, IEnumerable<string> containList)
        {
            try
            {
                if (MongoProcessor.CheckMongoDBConnection())
                {
                    var filterBuilder = Builders<T>.Filter;
                    var field = new StringFieldDefinition<T>(fieldName);

                    var collection = MongoProcessor.mongoManager.db.GetCollection<T>(typeof(T).Name);
                    var instanceList = collection.Find(filterBuilder.AnyIn(field, containList)).ToList();

                    if (instanceList != null && instanceList.Count > 0)
                        Decrypt<T>(ref instanceList);

                    return instanceList;
                }
            }
            catch (Exception ex) { throw ex; }

            return null;
        }

        public static async Task<List<T>> GetInAsync<T>(string fieldName, IEnumerable<string> containList)
        {
            try
            {
                if (MongoProcessor.CheckMongoDBConnection())
                {
                    var filterBuilder = Builders<T>.Filter;
                    var field = new StringFieldDefinition<T>(fieldName);

                    var collection = MongoProcessor.mongoManager.db.GetCollection<T>(typeof(T).Name);

                    var instanceList = await collection.FindAsync(filterBuilder.AnyIn(field, containList)).Result.ToListAsync();

                    if (instanceList != null && instanceList.Count > 0)
                        Decrypt<T>(ref instanceList);

                    return instanceList;
                }
            }
            catch (Exception ex) { throw ex; }

            return null;
        }

        public static T GetOne<T>(Expression<Func<T, bool>> LambdaExpression)
        {
            try
            {
                if (MongoProcessor.CheckMongoDBConnection())
                {
                    var collection = MongoProcessor.mongoManager.db.GetCollection<T>(typeof(T).Name);
                    T instance = collection.Find<T>(LambdaExpression).FirstOrDefault();

                    if (instance != null)
                    {
                        Decrypt<T>(ref instance);

                    }

                    return instance;
                }
            }
            catch (Exception ex) { throw ex; }

            return default(T);
        }

        public static async Task<T> GetOneAsync<T>(Expression<Func<T, bool>> LambdaExpression)
        {
            try
            {
                if (MongoProcessor.CheckMongoDBConnection())
                {
                    var collection = MongoProcessor.mongoManager.db.GetCollection<T>(typeof(T).Name);
                    T instance = await collection.FindAsync<T>(LambdaExpression).Result.FirstOrDefaultAsync();

                    if (instance != null)
                    {
                        Decrypt<T>(ref instance);

                    }

                    return instance;
                }
            }
            catch (Exception ex) { throw ex; }

            return default(T);
        }

        public static T GetLastOne<T>(Expression<Func<T, bool>> LambdaExpression, Expression<Func<T, DateTime>> SortLambdaExpression)
        {
            try
            {
                if (MongoProcessor.CheckMongoDBConnection())
                {
                    var collection = MongoProcessor.mongoManager.db.GetCollection<T>(typeof(T).Name);
                    T instance = collection
                        .AsQueryable<T>()
                        .Where<T>(LambdaExpression)
                        .OrderByDescending(SortLambdaExpression)
                        .FirstOrDefault();

                    if (instance != null)
                    {
                        Decrypt<T>(ref instance);

                    }

                    return instance;
                }
            }
            catch (Exception ex) { throw ex; }

            return default(T);
        }

        public static async Task<T> GetLastOneAsync<T>(Expression<Func<T, bool>> LambdaExpression, Expression<Func<T, DateTime>> SortLambdaExpression)
        {
            try
            {
                if (MongoProcessor.CheckMongoDBConnection())
                {
                    var collection = MongoProcessor.mongoManager.db.GetCollection<T>(typeof(T).Name);
                    T instance = await collection
                        .AsQueryable<T>()
                        .Where<T>(LambdaExpression)
                        .OrderByDescending(SortLambdaExpression)
                        .FirstOrDefaultAsync();

                    if (instance != null)
                    {
                        Decrypt<T>(ref instance);

                    }

                    return instance;
                }
            }
            catch (Exception ex) { throw ex; }

            return default(T);
        }

        #endregion
        #region Get Without Encrypt Process

        public static List<T> Get<T>(bool decryptStatus)
        {
            try
            {
                if (MongoProcessor.CheckMongoDBConnection())
                {
                    var collection = MongoProcessor.mongoManager.db.GetCollection<T>(typeof(T).Name);
                    var instanceList = collection.AsQueryable<T>().ToList();

                    if (decryptStatus)
                        if (instanceList != null && instanceList.Count > 0)
                            Decrypt<T>(ref instanceList);

                    return instanceList;
                }
            }
            catch (Exception ex) { throw ex; }

            return null;
        }

        public static async Task<List<T>> GetAsync<T>(bool decryptStatus)
        {
            try
            {
                if (MongoProcessor.CheckMongoDBConnection())
                {
                    var collection = MongoProcessor.mongoManager.db.GetCollection<T>(typeof(T).Name);
                    var instanceList = await collection.AsQueryable<T>().ToListAsync();

                    if (decryptStatus)
                        if (instanceList != null && instanceList.Count > 0)
                            Decrypt<T>(ref instanceList);

                    return instanceList;
                }
            }
            catch (Exception ex) { throw ex; }

            return null;
        }

        public static List<T> Get<T>(bool decryptStatus, Expression<Func<T, bool>> LambdaExpression)
        {
            try
            {
                if (MongoProcessor.CheckMongoDBConnection())
                {
                    var collection = MongoProcessor.mongoManager.db.GetCollection<T>(typeof(T).Name);
                    var instanceList = collection.Find<T>(LambdaExpression).ToList();

                    if (decryptStatus)
                        if (instanceList != null && instanceList.Count > 0)
                            Decrypt<T>(ref instanceList);

                    return instanceList;
                }
            }
            catch (Exception ex) { throw ex; }

            return null;
        }

        public static async Task<List<T>> GetAsync<T>(bool decryptStatus, Expression<Func<T, bool>> LambdaExpression)
        {
            try
            {
                if (MongoProcessor.CheckMongoDBConnection())
                {
                    var collection = MongoProcessor.mongoManager.db.GetCollection<T>(typeof(T).Name);
                    var instanceList = await collection.FindAsync<T>(LambdaExpression).Result.ToListAsync();

                    if (decryptStatus)
                        if (instanceList != null && instanceList.Count > 0)
                            Decrypt<T>(ref instanceList);

                    return instanceList;
                }
            }
            catch (Exception ex) { throw ex; }

            return null;
        }

        public static List<T> GetIn<T>(bool decryptStatus, string fieldName, IEnumerable<string> containList)
        {
            try
            {
                if (MongoProcessor.CheckMongoDBConnection())
                {
                    var filterBuilder = Builders<T>.Filter;
                    var field = new StringFieldDefinition<T>(fieldName);

                    var collection = MongoProcessor.mongoManager.db.GetCollection<T>(typeof(T).Name);

                    var instanceList = collection.Find(filterBuilder.AnyIn(field, containList)).ToList();

                    if (decryptStatus)
                        if (instanceList != null && instanceList.Count > 0)
                            Decrypt<T>(ref instanceList);

                    return instanceList;
                }
            }
            catch (Exception ex) { throw ex; }

            return null;
        }

        public static async Task<List<T>> GetInAsync<T>(bool decryptStatus, string fieldName, IEnumerable<string> containList)
        {
            try
            {
                if (MongoProcessor.CheckMongoDBConnection())
                {
                    var filterBuilder = Builders<T>.Filter;
                    var field = new StringFieldDefinition<T>(fieldName);

                    var collection = MongoProcessor.mongoManager.db.GetCollection<T>(typeof(T).Name);

                    var instanceList = await collection.FindAsync(filterBuilder.AnyIn(field, containList)).Result.ToListAsync();

                    if (decryptStatus)
                        if (instanceList != null && instanceList.Count > 0)
                            Decrypt<T>(ref instanceList);

                    return instanceList;
                }
            }
            catch (Exception ex) { throw ex; }

            return null;
        }

        public static T GetOne<T>(bool decryptStatus, Expression<Func<T, bool>> LambdaExpression)
        {
            try
            {
                if (MongoProcessor.CheckMongoDBConnection())
                {
                    var collection = MongoProcessor.mongoManager.db.GetCollection<T>(typeof(T).Name);
                    T instance = collection.Find<T>(LambdaExpression).FirstOrDefault();

                    if (decryptStatus)
                        if (instance != null)
                            Decrypt<T>(ref instance);

                    return instance;
                }
            }
            catch (Exception ex) { throw ex; }

            return default(T);
        }

        public static async Task<T> GetOneAsync<T>(bool decryptStatus, Expression<Func<T, bool>> LambdaExpression)
        {
            try
            {
                if (MongoProcessor.CheckMongoDBConnection())
                {
                    var collection = MongoProcessor.mongoManager.db.GetCollection<T>(typeof(T).Name);
                    T instance = await collection.FindAsync<T>(LambdaExpression).Result.FirstOrDefaultAsync();

                    if (decryptStatus)
                        if (instance != null)
                            Decrypt<T>(ref instance);

                    return instance;
                }
            }
            catch (Exception ex) { throw ex; }

            return default(T);
        }

        #endregion
        #region Get AsQueryable Process

        public static IMongoQueryable<T> GetAsQueryableAllowDiskUse<T>()
        {
            try
            {
                if (MongoProcessor.CheckMongoDBConnection())
                {
                    var collection = MongoProcessor.mongoManager.db.GetCollection<T>(typeof(T).Name);
                    return collection.AsQueryable<T>(new AggregateOptions { AllowDiskUse = true });
                }
            }
            catch (Exception ex) { throw ex; }

            return null;
        }

        public static IMongoQueryable<T> GetAsQueryableAllowDiskUse<T>(Expression<Func<T, bool>> LambdaExpression)
        {
            try
            {
                if (MongoProcessor.CheckMongoDBConnection())
                {
                    var collection = MongoProcessor.mongoManager.db.GetCollection<T>(typeof(T).Name);
                    return collection.AsQueryable<T>(new AggregateOptions { AllowDiskUse = true }).Where<T>(LambdaExpression);
                }
            }
            catch (Exception ex) { throw ex; }

            return null;
        }


        public static IMongoQueryable<T> GetAsQueryable<T>()
        {
            try
            {
                if (MongoProcessor.CheckMongoDBConnection())
                {
                    var collection = MongoProcessor.mongoManager.db.GetCollection<T>(typeof(T).Name);
                    return collection.AsQueryable<T>();
                }
            }
            catch (Exception ex) { throw ex; }

            return null;
        }

        public static IMongoQueryable<T> GetAsQueryable<T>(Expression<Func<T, bool>> LambdaExpression)
        {
            try
            {
                if (MongoProcessor.CheckMongoDBConnection())
                {
                    var collection = MongoProcessor.mongoManager.db.GetCollection<T>(typeof(T).Name);
                    return collection.AsQueryable<T>().Where<T>(LambdaExpression);
                }
            }
            catch (Exception ex) { throw ex; }

            return null;
        }

        public static IQueryable<T> GetInQueryable<T>(string fieldName, IEnumerable<string> containList, Expression<Func<T, bool>> LambdaExpression)
        {
            try
            {
                if (MongoProcessor.CheckMongoDBConnection())
                {
                    var filterBuilder = Builders<T>.Filter;
                    var field = new StringFieldDefinition<T>(fieldName);

                    var collection = MongoProcessor.mongoManager.db.GetCollection<T>(typeof(T).Name);

                    var instanceList = collection
                        .Find(filterBuilder.AnyIn(field, containList))
                        .ToEnumerable()
                        .AsQueryable()
                        .Where<T>(LambdaExpression);

                    return instanceList;
                }
            }
            catch (Exception ex) { throw ex; }

            return null;
        }

        public static IQueryable<T> GetInQueryable<T>(string fieldName, IEnumerable<string> containList)
        {
            try
            {
                if (MongoProcessor.CheckMongoDBConnection())
                {
                    var filterBuilder = Builders<T>.Filter;
                    var field = new StringFieldDefinition<T, string>(fieldName);

                    var collection = MongoProcessor.mongoManager.db.GetCollection<T>(typeof(T).Name);

                    var instanceList = collection
                        .Find(filterBuilder.In(field, containList))
                        .ToEnumerable()
                        .AsQueryable();

                    return instanceList;
                }
            }
            catch (Exception ex) { throw ex; }

            return null;
        }

        public static IEnumerable<T> GetIEnumerable<T>()
        {
            try
            {
                if (MongoProcessor.CheckMongoDBConnection())
                {
                    var collection = MongoProcessor.mongoManager.db.GetCollection<T>(typeof(T).Name);
                    return collection.AsQueryable<T>();
                }
            }
            catch (Exception ex) { throw ex; }

            return null;
        }

        public static IMongoQueryable<T> GetIEnumerable<T>(Expression<Func<T, bool>> LambdaExpression)
        {
            try
            {
                if (MongoProcessor.CheckMongoDBConnection())
                {
                    var collection = MongoProcessor.mongoManager.db.GetCollection<T>(typeof(T).Name);
                    return collection.AsQueryable<T>().Where<T>(LambdaExpression);
                }
            }
            catch (Exception ex) { throw ex; }

            return null;
        }

        #endregion
        #region Update Process

        public static bool Update<T>(Expression<Func<T, bool>> LambdaExpression, T instance, bool encryptDecryptStatus)
        {
            try
            {
                if (MongoProcessor.CheckMongoDBConnection())
                {
                    
                    var property = instance.GetType().GetProperty("ModifiedDate");

                    if (property != null)
                        property.SetValue(instance, DateTime.Now, null);

                    var collection = MongoProcessor.mongoManager.db.GetCollection<T>(typeof(T).Name);

                    if (encryptDecryptStatus)
                        Encrypt<T>(ref instance);

                    
                }
            }
            catch (Exception ex) { throw ex; }

            return false;
        }

        public static bool Update<T>(Expression<Func<T, bool>> LambdaExpression, T instance)
        {
            try
            {
                if (MongoProcessor.CheckMongoDBConnection())
                {
                    
                    var property = instance.GetType().GetProperty("ModifiedDate");

                    if (property != null)
                        property.SetValue(instance, DateTime.Now, null);

                    var collection = MongoProcessor.mongoManager.db.GetCollection<T>(typeof(T).Name);
                    Encrypt<T>(ref instance);

                    
                }
            }
            catch (Exception ex) { throw ex; }

            return false;
        }

        public static bool UpdateWithoutModifiedDate<T>(Expression<Func<T, bool>> LambdaExpression, T instance, bool updateModifiedDate = true)
        {
            try
            {
                if (MongoProcessor.CheckMongoDBConnection())
                {
                    

                    var collection = MongoProcessor.mongoManager.db.GetCollection<T>(typeof(T).Name);
                    Encrypt<T>(ref instance);
                    
                }
            }
            catch (Exception ex) { throw ex; }

            return false;
        }

        public static async Task<bool> UpdateAsync<T>(Expression<Func<T, bool>> LambdaExpression, T instance, bool encryptDecryptStatus)
        {
            try
            {
                if (MongoProcessor.CheckMongoDBConnection())
                {
                    
                    var property = instance.GetType().GetProperty("ModifiedDate");

                    if (property != null)
                        property.SetValue(instance, DateTime.Now, null);

                    var collection = MongoProcessor.mongoManager.db.GetCollection<T>(typeof(T).Name);

                    if (encryptDecryptStatus)
                        Encrypt<T>(ref instance);

                   
                }
            }
            catch (Exception ex) { throw ex; }

            return false;
        }

        public static async Task<bool> UpdateAsync<T>(Expression<Func<T, bool>> LambdaExpression, T instance)
        {
            try
            {
                if (MongoProcessor.CheckMongoDBConnection())
                {
                   
                    var property = instance.GetType().GetProperty("ModifiedDate");

                    if (property != null)
                        property.SetValue(instance, DateTime.Now, null);

                    var collection = MongoProcessor.mongoManager.db.GetCollection<T>(typeof(T).Name);
                    Encrypt<T>(ref instance);
                    
                }
            }
            catch (Exception ex) { throw ex; }

            return false;
        }

        #endregion
        #region Count Process

        public static long Count<T>(Expression<Func<T, bool>> LambdaExpression)
        {
            try
            {
                if (MongoProcessor.CheckMongoDBConnection())
                {
                    var collectionCount = MongoProcessor.mongoManager.db.GetCollection<T>(typeof(T).Name).CountDocuments(LambdaExpression);

                    return collectionCount;
                }
            }
            catch (Exception ex) { throw ex; }

            return 0;
        }

        public static long Count<T>()
        {
            try
            {
                if (MongoProcessor.CheckMongoDBConnection())
                {
                    var collectionCount = MongoProcessor.mongoManager.db.GetCollection<T>(typeof(T).Name).CountDocuments(new BsonDocument());

                    return collectionCount;
                }
            }
            catch (Exception ex) { throw ex; }

            return 0;
        }

        public static async Task<long> CountAsync<T>(Expression<Func<T, bool>> LambdaExpression)
        {
            try
            {
                if (MongoProcessor.CheckMongoDBConnection())
                {
                    var collectionCount = await MongoProcessor.mongoManager.db.GetCollection<T>(typeof(T).Name).CountDocumentsAsync(LambdaExpression);

                    return collectionCount;
                }
            }
            catch (Exception ex) { throw ex; }

            return 0;
        }

        public static async Task<long> CountAsync<T>()
        {
            try
            {
                if (MongoProcessor.CheckMongoDBConnection())
                {
                    var collectionCount = await MongoProcessor.mongoManager.db.GetCollection<T>(typeof(T).Name).CountDocumentsAsync(new BsonDocument());

                    return collectionCount;
                }
            }
            catch (Exception ex) { throw ex; }

            return 0;
        }

        #endregion
        #region Encrypt & Decrypt Data

        #region Encrypt
        public static void Encrypt<T>(ref T instance)
        {
            try
            {
                var type = instance.GetType();

                if (type.FullName == "Collections.ExampleCollection")
                {
                    foreach (var property in type.GetProperties())
                    {
                        try
                        {
                            if (property.CanWrite && property.CanRead && property.PropertyType == typeof(System.String))
                            {
                                if (
                                    property.Name == "MustBeEncryptedField"   // You can add more
                                    )
                                {
                                    var value = property.GetValue(instance);

                                    if (value != null)
                                        property.SetValue(instance, HK.Security.StringCipher.Encrypt(value.ToString()), null);
                                }
                            }
                        }
                        catch { }
                    }
                }

            }

            catch { }

        }
        public static void Encrypt<T>(ref List<T> instanceList)
        {
            try
            {
                if (typeof(T).FullName == "Collections.ExampleCollection")
                {
                    foreach (var instance in instanceList)
                    {
                        var type = instance.GetType();
                        foreach (var property in type.GetProperties())
                        {
                            try
                            {
                                if (property.CanWrite && property.CanRead && property.PropertyType == typeof(System.String))
                                {
                                    if (
                                       property.Name == "MustBeEncryptedField"   // You can add more
                                        )
                                    {
                                        var value = property.GetValue(instance);

                                        if (value != null)
                                            property.SetValue(instance, HK.Security.StringCipher.Encrypt(value.ToString()), null);
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                }
            }
            catch { }
        }

        public static void Encrypt<T>(ref IEnumerable<T> instanceList)
        {
            try
            {
                if (typeof(T).FullName == "Collections.ExampleCollection")
                {
                    foreach (var instance in instanceList)
                    {
                        var type = instance.GetType();
                        foreach (var property in type.GetProperties())
                        {
                            try
                            {
                                if (property.CanWrite && property.CanRead && property.PropertyType == typeof(System.String))
                                {
                                    if (
                                       property.Name == "MustBeEncryptedField"   // You can add more

                                        )
                                    {
                                        var value = property.GetValue(instance);

                                        if (value != null)
                                            property.SetValue(instance, HK.Security.StringCipher.Encrypt(value.ToString()), null);
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                }
            }
            catch { }
        }
        public static void Encrypt<T>(ref IMongoQueryable<T> instanceList)
        {
            try
            {
                if (typeof(T).FullName == "Collections.ExampleCollection")
                {
                    foreach (var instance in instanceList)
                    {
                        var type = instance.GetType();
                        foreach (var property in type.GetProperties())
                        {
                            try
                            {
                                if (property.CanWrite && property.CanRead && property.PropertyType == typeof(System.String))
                                {
                                    if (
                                       property.Name == "MustBeEncryptedField"   // You can add more
                                        )
                                    {
                                        var value = property.GetValue(instance);

                                        if (value != null)
                                            property.SetValue(instance, HK.Security.StringCipher.Encrypt(value.ToString()), null);
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                }
            }
            catch { }
        }

        #endregion
        #region   Decrypt

        public static void Decrypt<T>(ref T instance)
        {
            try
            {
                var type = instance.GetType();

                if (type.FullName == "Collections.ExampleCollection")
                {
                    foreach (var property in type.GetProperties())
                    {
                        try
                        {
                            if (property.CanWrite && property.CanRead && property.PropertyType == typeof(System.String))
                            {
                                if (
                                      property.Name == "MustBeEncryptedField"   // You can add more
                                    )
                                {
                                    var value = property.GetValue(instance);

                                    if (value != null)
                                        property.SetValue(instance, HK.Security.StringCipher.Decrypt(value.ToString()), null);
                                }
                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }
        }
        public static void Decrypt<T>(ref List<T> instanceList)
        {
            try
            {
                if (typeof(T).FullName == "Collections.ExampleCollection")
                {
                    foreach (var instance in instanceList)
                    {
                        var type = instance.GetType();
                        foreach (var property in type.GetProperties())
                        {
                            try
                            {
                                if (property.CanWrite && property.CanRead && property.PropertyType == typeof(System.String))
                                {
                                    if (
                                       property.Name == "MustBeEncryptedField"   // You can add more
                                        )
                                    {
                                        var value = property.GetValue(instance);

                                        if (value != null)
                                            property.SetValue(instance, HK.Security.StringCipher.Decrypt(value.ToString()), null);
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                }
            }
            catch { }
        }
        public static void Decrypt<T>(ref IEnumerable<T> instanceList)
        {
            try
            {
                if (typeof(T).FullName == "Collections.ExampleCollection")
                {
                    foreach (var instance in instanceList)
                    {
                        var type = instance.GetType();
                        foreach (var property in type.GetProperties())
                        {
                            try
                            {
                                if (property.CanWrite && property.CanRead && property.PropertyType == typeof(System.String))
                                {
                                    if (
                                        property.Name == "MustBeEncryptedField"   // You can add more
                                        )
                                    {
                                        var value = property.GetValue(instance);

                                        if (value != null)
                                            property.SetValue(instance, HK.Security.StringCipher.Decrypt(value.ToString()), null);
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                }
            }
            catch { }
        }
        public static void Decrypt<T>(ref IMongoQueryable<T> instanceList)
        {
            try
            {
                if (typeof(T).FullName == "Collections.ExampleCollection")
                {
                    foreach (var instance in instanceList)
                    {
                        var type = instance.GetType();
                        foreach (var property in type.GetProperties())
                        {
                            try
                            {
                                if (property.CanWrite && property.CanRead && property.PropertyType == typeof(System.String))
                                {
                                    if (
                                       property.Name == "MustBeEncryptedField"   // You can add more
                                        )
                                    {
                                        var value = property.GetValue(instance);

                                        if (value != null)
                                            property.SetValue(instance, HK.Security.StringCipher.Decrypt(value.ToString()), null);
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                }
            }
            catch { }
        }
        #endregion

        #endregion

    }
}

