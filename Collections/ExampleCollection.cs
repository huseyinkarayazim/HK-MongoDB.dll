using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using HK_MongoDB.Enum;
using MongoDB.Bson;

namespace HK_MongoDB.Collections
{
    [DataContract]
    public class ExampleCollection
    {
        [DataMember]
        [MongoDB.Bson.Serialization.Attributes.BsonId]
        [MongoDB.Bson.Serialization.Attributes.BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        #region Object Field
        [DataMember]
        [MongoDB.Bson.Serialization.Attributes.BsonDictionaryOptions(MongoDB.Bson.Serialization.Options.DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<CustomFields, object> CustomFields = new Dictionary<CustomFields, object>();
        #endregion
        #region Fields
        [DataMember]
        [MongoDB.Bson.Serialization.Attributes.BsonElement]
        public string ExampleField { get; set; }

        [DataMember]
        [MongoDB.Bson.Serialization.Attributes.BsonElement]
        public string MustBeEncryptedField { get; set; } // If you want encrypted fields, you can add more, but remember, you have to re-configure the encryption and decryption methods in MongoProcessor!
        #endregion
        #region Date Fields
        [DataMember]
        [MongoDB.Bson.Serialization.Attributes.BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreationDate { get; set; }
        [DataMember]
        [MongoDB.Bson.Serialization.Attributes.BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime ModifiedDate { get; set; }
        #endregion
    }
}
