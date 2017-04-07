using System;
using System.Threading;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;

namespace SharpMap.Data.Providers.Business.MongoDB.Gtfs.Import
{
    internal class UintIdGenerator : IIdGenerator
    {
        private static readonly object Lock = new object();
        private static MongoCollection<UintKeyTracker> _uintKeyTracker;

        public static void SetKeyTracker(MongoCollection<UintKeyTracker> keyTracker)
        {
            if (_uintKeyTracker != null)
                throw new InvalidOperationException();

            _uintKeyTracker = keyTracker;
        }
        public UintIdGenerator()
        {
            if (_uintKeyTracker == null)
                throw new InvalidOperationException();
        }

        public object GenerateId(object container, object document)
        {
            var mc = container as MongoCollection;
            if (mc == null)
                throw new ArgumentException("container must be a mongo collection");

            UintKeyTracker keyTracker = null;
            Monitor.Enter(Lock);
            keyTracker = _uintKeyTracker.FindOneByIdAs<UintKeyTracker>(mc.Name);
            if (keyTracker != null)
            {
                keyTracker.LastKey++;
            }
            else
            {
                keyTracker = new UintKeyTracker {CollectionName = mc.Name, LastKey = 1};
            }
            _uintKeyTracker.Save(keyTracker);
            Monitor.Exit(Lock);

            return keyTracker.LastKey;

        }

        public bool IsEmpty(object id)
        {
            return id == null || ((uint)id).Equals(new uint());
        }

        internal class UintKeyTracker
        {
            [BsonId(IdGenerator = typeof(NullIdChecker))]
            [BsonElement("collection_name")]
            public string CollectionName { get; set; }

            [BsonElement("last_key")]
            public uint LastKey { get; set; }
        }
    }
}