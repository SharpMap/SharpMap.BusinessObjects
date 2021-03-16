using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.Entity.Spatial;
using System.IO;
using System.Linq;
using System.Reflection;
using GeoAPI.Geometries;

namespace SharpMap.Data.Providers.Business
{
    /// <summary>
    /// A utility class to initialize
    /// </summary>
    public class EF6BusinessObjectSource
    {
        private static bool _initialized;

        internal static void Configure()
        {
            if (_initialized)
                return;

            _initialized = true;
            try
            {
                SqlServerTypes.Utilities.LoadNativeAssemblies(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            }
            catch (Exception ex)
            {
                throw new TypeInitializationException("EF6BusinessObjectSource", ex);
            }
        }
    }

    /// <summary>
    /// A business object source dealing with EF6
    /// </summary>
    /// <typeparam name="T">The type of the business objects.</typeparam>
    public class EF6BusinessObjectSource<T> : BaseBusinessObjectSource<T>
        where T:class, IEF6SpatialGeometryObject
    {
        static EF6BusinessObjectSource()
        {
            EF6BusinessObjectSource.Configure();
        }

        private readonly Func<DbContext> _createContext;
        private IGeometryFactory _factory;

        /// <summary>
        /// Gets a value indicating the <see cref="DbContext"/>
        /// </summary>
        public DbContext Context { get { return _createContext(); } }

        /// <summary>
        /// Creates an instance of this class using the provided context creation method.
        /// </summary>
        /// <param name="createContext">A context creation method</param>
        public EF6BusinessObjectSource(Func<DbContext> createContext)
        {

            _createContext = createContext;
        }

        /// <summary>
        /// Gets a value indication all the entities
        /// </summary>
        public IDbSet<T> Entities { get { return Context.Set<T>(); } }

        /// <summary>
        /// Gets or sets a factory
        /// </summary>
        public IGeometryFactory Factory   
        {
            get
            {
                if (_factory == null)
                {
                    lock (_createContext)
                    {
                        if (_factory == null)
                        {
                            using (var c = _createContext())
                            {
                                var f = c.Set<T>().FirstOrDefault();
                                int srid = f == null ? 0 : f.DbGeometry.CoordinateSystemId;
                                _factory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(srid);
                            }
                        }
                    }
                }
                return _factory;
            }
            set { _factory = value; }
        }

        /// <inheritdoc />
        public override IEnumerable<T> Select(Envelope box)
        {
            var envGeom = Factory.ToGeometry(box);
            return Select(envGeom);
        }

        /// <inheritdoc />
        public override IEnumerable<T> Select(IGeometry geom)
        {
            var dbGeometry = geom.ToDbGeometry();
            using (var c = Context)
            {
                var qry = from u in c.Set<T>()
                    where u.DbGeometry.Intersects(dbGeometry)
                    select u;

                return Select(qry);
            }
            
        }

        /// <inheritdoc />
        public override IEnumerable<T> Select(Predicate<T> match)
        {
            using (var c = Context)
            {
                var qry = from u in c.Set<T>()
                    where match(u)
                    select u;
                return Select(qry);
            }
        }

        /// <inheritdoc />
        public override T Select(uint id)
        {
            using (var c = Context)
            {
                return c.Set<T>().Find((int)id);
            }
        }


        /// <inheritdoc />
        public override Envelope GetExtents()
        {
            if (CachedExtents == null)
            {
                using (var c = Context)
                {
                    var q = from t in c.Set<T>()
                        select t.DbGeometry.Envelope;
                    
                    CachedExtents = ToEnvelope(q.AsEnumerable());
                }
            }
            return CachedExtents;
        }

        private static Envelope ToEnvelope(IEnumerable<DbGeometry> dbGeometries)
        {
            var res = new Envelope();
            foreach (var dbGeometry in dbGeometries)
            {
                res.ExpandToInclude(dbGeometry.ToGeometry().EnvelopeInternal);
            }
            return res;
        }

        /// <inheritdoc />
        public override void Update(IEnumerable<T> businessObjects)
        {
            using (var c = Context)
            {
                c.Set<T>().AddOrUpdate(new List<T>(businessObjects).ToArray());
                c.SaveChanges();
            }
        }

        /// <inheritdoc />
        public override void Delete(IEnumerable<T> businessObjects)
        {
            using (var c = Context)
            {
                c.Set<T>().RemoveRange(businessObjects);
                c.SaveChanges();
            }
        }

        /// <inheritdoc />
        public override void Delete(Predicate<T> match)
        {
            Delete(Select(match));
        }

        /// <inheritdoc />
        public override void Insert(T businessObject)
        {
            using (var c = Context)
            {
                c.Set<T>().Add(businessObject);
                c.SaveChanges();
            }
        }

        /// <inheritdoc />
        public override void Insert(IEnumerable<T> businessObjects)
        {
            using (var c = Context)
            {
                c.Set<T>().AddRange(businessObjects);
                c.SaveChanges();
            }
        }

        /// <inheritdoc />
        public override int Count
        {
            get
            {
                using (var c = Context)
                {
                    return c.Set<T>().Count();
                }
            }
        }
    }
}
