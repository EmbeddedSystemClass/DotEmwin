using System;
using System.Linq;
using System.Text;
using Emwin.Core.Parsers;
using Emwin.Core.Products;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Emwin.Core.Contracts;

namespace Emwin.Tests
{
    [TestClass]
    public class BulletinTests
    {
        #region Public Methods

        public void TornadoWarningHeaderTests()
        {
            var product = GetTornadoWarning();
            Assert.IsNotNull(product, "Unable to create text product");

            var bulletin = BulletinParser.ParseProduct(product).FirstOrDefault();
            Assert.IsNotNull(bulletin, "Unable to parse text product into bulletin");

            Assert.IsNotNull(product.Header, "product.Header != null");
            Assert.AreEqual("TORDDC", product.Header.AfosPil, "AfosPil");
            Assert.AreEqual("WFUS53", product.Header.AwipsId, "AwipsId");
            Assert.AreEqual("KDDC", product.Header.WmoId, "WmoId");
            Assert.IsNull(product.Header.Indicator, "Indicator");
            Assert.AreEqual(new TimeSpan(0, 56, 0), product.Header.Time.TimeOfDay);
        }

        [TestMethod]
        public void TornadoWarningPolygonTest()
        {
            var product = GetTornadoWarning();
            Assert.IsNotNull(product, "Unable to create text product");

            var bulletin = BulletinParser.ParseProduct(product).FirstOrDefault();
            Assert.IsNotNull(bulletin, "Unable to parse text product into bulletin");

            var polygon = bulletin.Polygons.FirstOrDefault();
            Assert.IsNotNull(polygon, "Unable to parse polygon");
            Assert.AreEqual(4, polygon.Length, "Count");
            Assert.AreEqual(38.7, polygon[0].Latitude, "Latitude");
            Assert.AreEqual(-100.17, polygon[0].Longitude, "Longitude");
        }

        [TestMethod]
        public void TornadoWarningTrackingTest()
        {
            var product = GetTornadoWarning();
            Assert.IsNotNull(product, "Unable to create text product");

            var bulletin = BulletinParser.ParseProduct(product).FirstOrDefault();
            Assert.IsNotNull(bulletin, "Unable to parse text product into bulletin");

            var trackingLine = bulletin.TrackingLine;
            Assert.IsNotNull(trackingLine, "Unable to parse tracking line");
            Assert.AreEqual(new TimeSpan(0, 55, 0), trackingLine.TimeStamp.TimeOfDay, "Time");
            Assert.AreEqual(227, trackingLine.DirectionDeg);
            Assert.AreEqual(17, trackingLine.WindSpeedKts);

            var line = trackingLine.Line.ToList();
            Assert.AreEqual(1, line.Count, "Count");
            Assert.AreEqual(38.69, line[0].Latitude, "Latitude");
            Assert.AreEqual(-100.33, line[0].Longitude, "Longitude");
        }


        [TestMethod]
        public void TornadoWarningUgcTest()
        {
            var product = GetTornadoWarning();
            Assert.IsNotNull(product, "Unable to create text product");

            var bulletin = BulletinParser.ParseProduct(product).FirstOrDefault();
            Assert.IsNotNull(bulletin, "Unable to parse text product into bulletin");

            var geoCodes = bulletin.GeoCodes.ToList();
            Assert.AreEqual(1, geoCodes.Count, "Count");
            Assert.IsTrue(geoCodes[0].Counties.Contains(101), "County 101");
            Assert.IsTrue(geoCodes[0].Counties.Contains(135), "County 135");
            Assert.IsFalse(geoCodes[0].Zones.Any(), "Zones");
            Assert.AreEqual("KS", geoCodes[0].State, "State");
            Assert.AreEqual(new DateTimeOffset(2015, 6, 5, 1, 30, 00, TimeSpan.Zero), geoCodes[0].PurgeTime, "PurgeTime");
        }

        [TestMethod]
        public void TornadoWarningVTecTest()
        {
            var product = GetTornadoWarning();
            Assert.IsNotNull(product, "Unable to create text product");

            var bulletin = BulletinParser.ParseProduct(product).FirstOrDefault();
            Assert.IsNotNull(bulletin, "Unable to parse text product into bulletin");

            var vtec = bulletin.PrimaryVtec;
            Assert.IsNotNull(vtec, "Unable to parse primary VTEC");
            Assert.AreEqual(45, vtec.EventNumber, "VTEC EventNumber");
            Assert.AreEqual(new DateTimeOffset(2015, 6, 5, 00, 56, 00, TimeSpan.Zero), vtec.Begin, "VTEC Begin");
            Assert.AreEqual(new DateTimeOffset(2015, 6, 5, 1, 30, 00, TimeSpan.Zero), vtec.End, "VTEC End");
            Assert.AreEqual("KDDC", vtec.WmoId, "VTEC WmoId");
            Assert.AreEqual("NEW", vtec.ActionCode, "VTEC ActionCode");
            Assert.AreEqual("TO", vtec.PhenomenonCode, "VTEC PhenomenonCode");
            Assert.AreEqual('W', vtec.SignificanceCode, "VTEC SignificanceCode");
        }

        #endregion Public Methods

        #region Private Methods

        private static ITextProduct GetTornadoWarning()
        {
            return TextProduct.Create(
                "TORDDCXXXX.TXT",
                new DateTimeOffset(2015, 6, 5, 0, 56, 0, TimeSpan.Zero),
                Encoding.ASCII.GetBytes(BulletinContent.TornadoWarning),
                DateTimeOffset.UtcNow, String.Empty);
        }

        #endregion Private Methods
    }
}
