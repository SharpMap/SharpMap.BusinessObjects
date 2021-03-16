// Copyright 2013 - 2014 Felix Obermaier (www.ivv-aachen.de)
//
// This file is part of SharpMap.Data.Providers.Business.
// SharpMap.Data.Providers.Business is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap.Data.Providers.Business is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using System.Collections;

namespace SharpMap.Data.Providers.Business
{
    /// <summary>
    /// Quick and dirty implementation of an in-memory business object store
    /// </summary>
    /// <typeparam name="T">The type of the business object</typeparam>
    public class InMemoryBusinessObjectSource<T> : BaseBusinessObjectSource<T>
    {
        private readonly Dictionary<uint, T> _businessObjects;

        private readonly string _title;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        public InMemoryBusinessObjectSource()
            : this(typeof(T).Name)
        { }

        /// <summary>
        /// Creates an instance of this class, assigning a <see cref="Title"/>
        /// </summary>
        public InMemoryBusinessObjectSource(string title)
        {
            _businessObjects = new Dictionary<uint, T>();
            _title = title;
        }
        /// <summary>
        /// Gets a value identifying the business object
        /// </summary>
        public override string Title
        {
            get { return _title; }
        }
        ///// <summary>
        ///// Collection of business objects for querying using Select(IQueryable<T> query)
        ///// or to directly edit attributes of one or more objects
        /////</summary>
        //public ICollection<T> Context
        //{
        //    get
        //    {
        //        lock (((ICollection)_businessObjects).SyncRoot)
        //            return _businessObjects.Values;
        //    }
        //}

        /// <summary>
        /// Select a set of features based on <paramref name="box"/>
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        public override IEnumerable<T> Select(Envelope box)
        {
            return Select(new GeometryFactory().ToGeometry(box));
        }

        /// <summary>
        /// Select a set of features based on <paramref name="geom"/>
        /// </summary>
        /// <param name="geom">A geometry</param>
        /// <returns></returns>
        public override IEnumerable<T> Select(IGeometry geom)
        {
            var prep = NetTopologySuite.Geometries.Prepared.PreparedGeometryFactory.Prepare(geom);

            lock (((ICollection)_businessObjects).SyncRoot)
            {
                foreach (T value in _businessObjects.Values)
                {
                    var g = _getGeometry(value);
                    if (g != null && prep.Intersects(g))
                    {
                        yield return value;
                    }
                }
            }
        }

        /// <summary>
        /// Select a feature by its id
        /// </summary>
        /// <param name="id">the id of the feature</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public override T Select(uint id)
        {
            T res;
            lock (((ICollection)_businessObjects).SyncRoot)
            {
                if (_businessObjects.TryGetValue(id, out res))
                    return res;
            }
            throw new ArgumentException("No feature with this id", id.ToString());
        }

        /// <summary>
        /// Update the provided <paramref name="features"/>
        /// </summary>
        /// <param name="features">The features that need to be updated</param>
        public override void Update(IEnumerable<T> features)
        {
            Delete(features);

            lock (((ICollection)_businessObjects).SyncRoot)
            {
                foreach (T feature in features)
                {
                    _businessObjects.Add(_getId(feature), feature);
                }
            }
        }

        /// <summary>
        /// Delete the provided <paramref name="features"/>
        /// </summary>
        /// <param name="features">The features that need to be deleted</param>
        public override void Delete(IEnumerable<T> features)
        {
            lock (((ICollection)_businessObjects).SyncRoot)
            {
                foreach (T feature in features)
                {
                    _businessObjects.Remove(_getId(feature));
                }
                CachedExtents = null;
            }
        }

        /// <summary>
        /// Attribute-based deletion according to provided <paramref name="match"/>
        /// </summary>
        /// <param name="match"><typeparamref name="Predicate<T>"/> identifying business objects to be deleted</param>
        public override void Delete(Predicate<T> match)
        {
            lock (((ICollection)_businessObjects).SyncRoot)
            {
                foreach (T bo in FindAll(match))
                {
                    _businessObjects.Remove(_getId(bo));
                }
                CachedExtents = null;
            }
        }

        /// <summary>
        /// Delete objects by provided <paramref name="oids"/>
        /// </summary>
        /// <param name="oids">ObjectIds of features to be deleted</param>
        public void Delete(IEnumerable<uint> oids)
        {
            lock (((ICollection)_businessObjects).SyncRoot)
            {
                foreach (uint oid in oids)
                {
                    _businessObjects.Remove(oid);
                }
                CachedExtents = null;
            }
        }

        public void Clear()
        {
            lock (((ICollection)_businessObjects).SyncRoot)
            {
                _businessObjects.Clear();
                CachedExtents = null;
            }

        }

        /// <summary>
        /// Insert the provided <paramref name="features"/>
        /// </summary>
        /// <param name="features">The features that need to be inserted</param>
        public override void Insert(IEnumerable<T> features)
        {
            lock (((ICollection)_businessObjects).SyncRoot)
            {
                foreach (T feature in features)
                {
                    _businessObjects.Add(_getId(feature), feature);
                }
                CachedExtents = null;
            }
        }

        /// <summary>
        /// Insert provided <paramref name="businessObject"/> and expand extents
        /// </summary>
        /// <param name="businessObject">The business object to be inserted</param>
        public override void Insert(T businessObject)
        {
            lock (((ICollection)_businessObjects).SyncRoot)
            {
                _businessObjects.Add(_getId(businessObject), businessObject);

                // expand to include
                var res = CachedExtents ?? new Envelope();

                var g = _getGeometry(businessObject);
                if (g != null && !g.IsEmpty)
                    res.ExpandToInclude(g.EnvelopeInternal);
                CachedExtents = res;
            }
        }

        [Obsolete("Use Insert(T businessObject)")]
        public void InsertFeature(T feature)
        {
            Insert(feature);
        }

        public override int Count
        {
            get
            {
                lock (((ICollection)_businessObjects).SyncRoot)
                    return _businessObjects.Count;
            }
        }

        public override Envelope GetExtents()
        {
            return CachedExtents ?? (CachedExtents = ComputeExtents());
        }

        private Envelope ComputeExtents()
        {
            lock (((ICollection)_businessObjects).SyncRoot)
            {
                var res = new Envelope();
                foreach (var bo in _businessObjects.Values)
                {
                    var g = _getGeometry(bo);
                    if (g != null && !g.IsEmpty)
                        res.ExpandToInclude(g.EnvelopeInternal);
                }
                return res;
            }
        }

        /// <summary>
        /// Attribute-based selection according to <paramref name="match"/>
        /// </summary>
        /// <param name="match">The predicate</param>
        /// <returns>The the first business object matching the predicate</returns>
        public override T[] FindAll(Predicate<T> match)
        {
            T[] objList;
            lock (((ICollection)_businessObjects).SyncRoot)
            {
                objList = new T[_businessObjects.Count];
                _businessObjects.Values.CopyTo(objList, 0);
            }
            return Array.FindAll(objList, match);
        }

        /// <summary>
        /// Attribute-based selection according to <paramref name="match"/>
        /// </summary>
        /// <param name="match">The predicate</param>
        /// <returns>The the first business object matching the predicate</returns>
        public override T Find(Predicate<T> match)
        {
            T[] objList;
            lock (((ICollection)_businessObjects).SyncRoot)
            {
                objList = new T[_businessObjects.Count];
                _businessObjects.Values.CopyTo(objList, 0);
            }
            return Array.Find(objList, match);
        }

        /// <summary>
        /// The business objects contained by the Source
        /// </summary>
        /// <returns>A shallow copy of the business objects</returns>
        public override T[] AsReadOnly()
        {
            T[] objList;
            lock (((ICollection)_businessObjects).SyncRoot)
            {
                objList = new T[_businessObjects.Count];
                _businessObjects.Values.CopyTo(objList, 0);
            }
            return objList;

        }
    }
}