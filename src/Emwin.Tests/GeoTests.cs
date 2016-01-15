using Emwin.Core.DataObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Emwin.Tests
{
    [TestClass]
    public class GeoTests
    {
        [TestMethod]
        public void GeoPointTests()
        {
            var actual = new GeoPoint("3870 10017");
            Assert.AreEqual(38.70, actual.Latitude, "Latitude 1");
            Assert.AreEqual(-100.17, actual.Longitude, "Longitude 1");
            Assert.AreEqual("3870 10017", actual.ToRaw(), "Raw 1");

            actual = new GeoPoint("9000 18000");
            Assert.AreEqual(90, actual.Latitude, "Latitude 2");
            Assert.AreEqual(-180, actual.Longitude, "Longitude 2");
            Assert.AreEqual("9000 18000", actual.ToRaw(), "Raw 2");

            actual = new GeoPoint("3870 28000");
            Assert.AreEqual(38.70, actual.Latitude, "Latitude 3");
            Assert.AreEqual(100, actual.Longitude, "Longitude 3");
            Assert.AreEqual("3870 28000", actual.ToRaw(), "Raw 3");
        }
    }
}
