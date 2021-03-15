// Copyright 2013-2014 - Felix Obermaier (www.ivv-aachen.de)
//
// This file is part of SharpMap.Business.MongoDB.
// SharpMap.Business.MongoDB is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap.Business.MongoDB is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System.Collections.Generic;
using System.Linq;
using GeoAPI.Geometries;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
//using MongoDB.Driver.Builders;
using MongoDB.Driver.GeoJsonObjectModel;
using SharpMap.Converters;

namespace SharpMap.Data.Providers.Business
{
    /// <summary>
    /// Abstract base class for MongDB based repositories
    /// </summary>
    /// <typeparam name="T">The type of the business object</typeparam>
    /// <typeparam name="TCoordinate">The type of the <see cref="GeoJsonCoordinates"/> to use.</typeparam>
    public abstract class MongoDbBusinessObjectSource<T, TCoordinate> : BaseBusinessObjectSource<T>
        where TCoordinate: GeoJsonCoordinates
    {
        private readonly IMongoCollection<T> _collection;
        protected readonly GeoJsonConverter<TCoordinate> Converter;

        private MongoDbBusinessObjectSource(GeoJsonConverter<TCoordinate> converter)
        {
            base.Title = typeof (T).Name;
            
            Converter = converter;
            if (!BsonClassMap.IsClassMapRegistered(typeof (T)))
            {
                BsonClassMap.RegisterClassMap<T>(cm =>
                {
                    cm.AutoMap();
                    //cm.GetMemberMap("BsonGeometry").SetSerializer(new GeoJson2DCoordinatesSerializer());
                }
                    );
            }
        }

        protected MongoDbBusinessObjectSource(GeoJsonConverter<TCoordinate> converter, MongoClientSettings settings, string database, string collection)
            : this(converter,new MongoClient(settings), database, collection)
        {
        }

        protected MongoDbBusinessObjectSource(GeoJsonConverter<TCoordinate> converter, string connectionString, string database, string collection)
            : this(converter,new MongoClient(connectionString), database, collection)
        {
        }

        private MongoDbBusinessObjectSource(GeoJsonConverter<TCoordinate> converter, MongoClient mongoClient, string database, string collection)
            :this(converter)
        {
            var mongoDatabase = mongoClient.GetDatabase(database);
            _collection = mongoDatabase.GetCollection<T>(collection);
        }

        /// <summary>
        /// Gets the extents of the 
        /// </summary>
        /// <returns>The extents</returns>
        public override Envelope GetExtents()
        {
            if (CachedExtents != null)
                return CachedExtents;

            var extent = new Envelope();
            var crsr = _collection.FindSync(Builders<T>.Filter.Empty);
            while (crsr.MoveNext())
            {
                foreach (var itm in crsr.Current)
                    extent.ExpandToInclude(GetGeometry(itm).EnvelopeInternal);
            }
            return CachedExtents = extent;
        }

        public override IEnumerable<T> Select(Envelope box)
        {
            box = GetExtents().Intersection(box);
            return _collection.Find(BuildEnvelopeQuery(box)).ToEnumerable();
        }

        /// <summary>
        /// Method to create the mongo query for the bounding box search
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        protected abstract FilterDefinition<T> BuildEnvelopeQuery(Envelope box);

        /// <summary>
        /// Select a set of features based on <paramref name="geom"/>
        /// </summary>
        /// <param name="geom">A geometry</param>
        /// <returns></returns>
        public override IEnumerable<T> Select(IGeometry geom)
        {
            var candidates = Select(geom.EnvelopeInternal);
            var p = NetTopologySuite.Geometries.Prepared.PreparedGeometryFactory.Prepare(geom);
            return candidates.Where(candidate => p.Intersects(GetGeometry(candidate)));
        }

        /// <summary>
        /// Select a business object by its id
        /// </summary>
        /// <param name="id">the id of the business object</param>
        /// <returns></returns>
        public override T Select(uint id)
        {
            return _collection.FindSync(Builders<T>.Filter.Eq(t => GetId(t) == id, true)).FirstOrDefault();
        }

        /// <summary>
        /// Update the provided <paramref name="businessObjects"/>
        /// </summary>
        /// <param name="businessObjects">The business objects that need to be updated</param>
        public override void Update(IEnumerable<T> businessObjects)
        {
            foreach (var businessObject in businessObjects)
            {
                uint id = GetId(businessObject);
                var filter = Builders<T>.Filter.Eq(t => GetId(t), id);
                _collection.ReplaceOne(filter, businessObject);
            }
        }

        /// <summary>
        /// Delete the provided <paramref name="businessObjects"/>
        /// </summary>
        /// <param name="businessObjects">The <typeparamref name="T"/>s that need to be deleted</param>
        public override void Delete(IEnumerable<T> businessObjects)
        {
            foreach (var businessObject in businessObjects)
            {
                uint id = GetId(businessObject);
                var query = Builders<T>.Filter.Eq(t  => GetId(t), id);
                _collection.DeleteOne(query);
            }
        }

        /// <summary>
        /// Insert the provided <paramref name="businessObjects"/>
        /// </summary>
        /// <param name="businessObjects">The features that need to be inserted</param>
        public override void Insert(IEnumerable<T> businessObjects)
        {
            _collection.InsertMany(businessObjects);
        }

        /// <summary>
        /// Gets the number of business objects in the store
        /// </summary>
        public override int Count
        {
            get { return (int)_collection.CountDocuments(Builders<T>.Filter.Empty); }
        }
    }
}
