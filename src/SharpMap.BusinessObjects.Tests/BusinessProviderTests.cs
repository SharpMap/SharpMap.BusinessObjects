using NUnit.Framework;
using SharpMap.Data.Providers.Business;

namespace SharpMap.Business.Tests
{
    [TestFixture]
    public class TypeUtilityTest
    {
        private class Entity
        {
            public int Field;
            public long Field64;
            public int Property { get; set; }
            public long Property64 { get; set; }
        }

        [Test]
        public void TestMemberType()
        {
            var df = TypeUtility<Entity>.GetMemberGetDelegate<int>("Field");
            var df64 = TypeUtility<Entity>.GetMemberGetDelegate<long>("Field64");
            var dp = TypeUtility<Entity>.GetMemberGetDelegate<int>("Property");
            var dp64 = TypeUtility<Entity>.GetMemberGetDelegate<long>("Property64");

            var e = new Entity {Field = 1, Field64 = long.MaxValue-100, Property = 2, Property64 = long.MinValue+100};

            Assert.AreEqual(e.Field, df(e));
            Assert.AreEqual(e.Field64, df64(e));
            Assert.AreEqual(e.Property, dp(e));
            Assert.AreEqual(e.Property64, dp64(e));
        }
    }
}
