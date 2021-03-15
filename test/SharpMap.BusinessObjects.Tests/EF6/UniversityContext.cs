using System.Data.Entity;

namespace SharpMap.Business.Tests.EF6
{
    public class UniversityContext : DbContext
    {
        public DbSet<University> Universities { get; set; }
    }
}