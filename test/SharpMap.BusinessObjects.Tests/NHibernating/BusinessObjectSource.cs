using System;
using System.Collections.Generic;
using System.Linq;
using GeoAPI.Geometries;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Loader;
using SharpMap.Data.Providers.Business;

namespace SharpMap.Business.Tests.NHibernating
{
    public class BusinessObjectSource<T> : BaseBusinessObjectSource<T>
        where T:class
    {
        private readonly string _title;

        public BusinessObjectSource()
        {
            _title = typeof (T).Name;
        }
        private static NHibernate.ISession GetSession()
        {
            return SessionProvider.SessionFactory.OpenSession();
        }

        public override string Title
        {
            get { return _title; }
        }

        public override IEnumerable<T> Select(Envelope box)
        {
            using (var session = GetSession())
            {
                foreach (var bo in session.CreateCriteria<T>().List<T>())
                {
                    if (box.Intersects(GetGeometry(bo).EnvelopeInternal))
                        yield return bo;
                }
            }
        }

        public override IEnumerable<T> Select(IGeometry geom)
        {
            var p = NetTopologySuite.Geometries.Prepared.PreparedGeometryFactory.Prepare(geom);

            using (var session = GetSession())
            {
                foreach (var bo in session.CreateCriteria<T>().List<T>())
                {
                    if (p.Intersects(GetGeometry(bo)))
                        yield return bo;
                }
            }
        }

        public override IEnumerable<T> Select(Predicate<T> match)
        {
            using (var session = GetSession())
            {
                
                foreach (var bo in session.CreateCriteria<T>().List<T>().Where(u => match(u)))
                        yield return bo;
            }
        }

        public override T Select(uint id)
        {
            using (var session = GetSession())
            {
                return session.Get<T>(id);
            }
        }

        public override void Update(IEnumerable<T> businessObjects)
        {
            using (var session = GetSession())
            {
                var t = session.BeginTransaction();
                foreach (var businessObject in businessObjects)
                {
                    session.Update(businessObject);
                }
                t.Commit();
            }
        }

        public override void Delete(IEnumerable<T> businessObjects)
        {
            using (var session = GetSession())
            {
                var t = session.BeginTransaction();
                foreach (var businessObject in businessObjects)
                {
                    session.Delete(businessObject);
                }
                t.Commit();
            }
        }

        public override void Insert(T businessObject)
        {
            using (var session = GetSession())
            {
                session.SaveOrUpdate(businessObject);
            }
        }

        public override void Insert(IEnumerable<T> businessObjects)
        {
            using (var session = GetSession())
            {
                var t = session.BeginTransaction();
                foreach (var businessObject in businessObjects)
                {
                    session.SaveOrUpdate(businessObject);
                }
                t.Commit();
            }
        }

        public override int Count
        {
            get
            {
                using (var session = GetSession())
                {
                    return session.CreateCriteria<T>()
                        .SetProjection(Projections.RowCount())
                        .UniqueResult<int>();
                }
            }
        }

        private Envelope _cached;
        public override Envelope GetExtents()
        {
            if (_cached == null)
            {
                _cached = new Envelope();
                using (var session = GetSession())
                {
                    foreach (var bo in session.CreateCriteria<T>().List<T>())
                    {
                        _cached.ExpandToInclude(GetGeometry(bo).EnvelopeInternal);
                    }
                }
            }
            return _cached;
        }
    }
}
