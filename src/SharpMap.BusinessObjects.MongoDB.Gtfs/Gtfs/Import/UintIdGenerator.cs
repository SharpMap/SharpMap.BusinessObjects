/*
 * Copyright © 2017 - Felix Obermaier, Ingenieurgruppe IVV GmbH & Co. KG
 * 
 * This file is part of SharpMap.BusinessObjects.MongoDB.Gtfs.
 *
 * SharpMap.BusinessObjects.MongoDB.Gtfs is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 * 
 * SharpMap.BusinessObjects.MongoDB.Gtfs is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.

 * You should have received a copy of the GNU Lesser General Public License
 * along with SharpMap; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 
 *
 */
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
        private static IMongoCollection<UintKeyTracker> _uintKeyTracker;

        public static void SetKeyTracker(IMongoCollection<UintKeyTracker> keyTracker)
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
            if (!(container is IMongoCollection<UintKeyTracker> mc))
                throw new ArgumentException("container must be a mongo collection");

            string mcName = mc.CollectionNamespace.FullName;
            var filter = Builders<UintKeyTracker>.Filter.Exists(t => t.CollectionName == mcName);
            Monitor.Enter(Lock);
            var keyTracker = _uintKeyTracker.Find(filter).FirstOrDefault();
            if (keyTracker != null)
            {
                keyTracker.LastKey++;
                _uintKeyTracker.ReplaceOne(filter, keyTracker);
            }
            else
            {
                keyTracker = new UintKeyTracker {CollectionName = mc.CollectionNamespace.CollectionName, LastKey = 1};
                _uintKeyTracker.InsertOne(keyTracker);
            }
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
