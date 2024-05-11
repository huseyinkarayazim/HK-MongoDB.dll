using HK_MongoDB.Collections;
using HK_MongoDB.Enum;
using System;
using System.Collections.Generic;

namespace HK_MongoDB
{
    public static class Builders
    {
        private static List<string> collections = null;
        /// <summary>
        ///  This method build all SmartBase Station NOSQL Collections with Indexes
        /// </summary>
        /// <param name="RequestSource">If you want to do something special for Request Source, you can re-configure this method.</param>
        public static void BuildCollections(RequestSource RequestSource)
        {
            try
            {
                MongoProcessor.RequestSource = RequestSource; // If you want to do something special for Request Source, you can re-configure this method.

                MongoProcessor.DBName = DBName.HK_Database;

                if (MongoProcessor.CheckMongoDBConnection())
                {
                    if (MongoProcessor.mongoManager != null)
                    {
                        collections = MongoProcessor.mongoManager.GetCollectionsName();

                        if (collections != null)
                        {
                            
                            BuildCollectionDefaults<ExampleCollection>();
                            MongoProcessor.CreateIndex<ExampleCollection>("ExampleField", "Example Value");
                            
                        }
                    }
                }
            }
            catch { }
        }
        /// <summary>
        /// This method build all DB Collections Without Indexes
        /// </summary>
        /// <typeparam name="T">Generic Class</typeparam>
        private static void BuildCollectionDefaults<T>()
        {
            try
            {
                if (!collections.Contains(typeof(T).Name))
                    MongoProcessor.mongoManager.CreateCollection(typeof(T).Name);
            }
            catch (Exception ex) { throw ex; }
        }
        /// <summary>
        /// This method build all DB Collections Without Indexes
        /// </summary>
        /// <typeparam name="T">Generic Class</typeparam>
        /// <param name="indexFieldName">Index field name</param>
        /// <param name="indexName">Index name</param>
        private static void BuildCollectionDefaults<T>(string indexFieldName, string indexName)
        {
            try
            {
                if (!collections.Contains(typeof(T).Name))
                {
                    MongoProcessor.mongoManager.CreateCollection(typeof(T).Name);
                    MongoProcessor.CreateIndex<T>(indexFieldName, indexName);
                }
            }
            catch (Exception ex) { throw ex; }
        }


    }
}
