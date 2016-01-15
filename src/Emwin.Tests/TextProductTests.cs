using System;
using System.Linq;
using System.Text;
using Emwin.Core.Extensions;
using Emwin.Core.Parsers;
using Emwin.Core.Products;
using Emwin.Tests.Content;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Emwin.Tests
{
    [TestClass]
    public class TextProductTests
    {
        #region Public Methods

        [TestMethod]
        public void TornadoWarningHeaderTests()
        {
            var product = GetTornadoWarning();
            Assert.IsNotNull(product, "Unable to create text product");
            Assert.IsNotNull(product.Content, "Content != null");
            Assert.AreEqual("WFUS53 KDDC 050056\r\nTORDDC\r\n", product.Content.RawHeader, "Raw Header");

            var header = product.GetWmoHeader();
            Assert.AreEqual("TOR", header.ProductCategory, "ProductCategory");
            Assert.AreEqual("DDC", header.LocationIdentifier, "LocationIdentifier");
            Assert.AreEqual("WF", header.DataType, "DataType");
            Assert.AreEqual("US53", header.Distribution, "Distribution");
            Assert.AreEqual("KDDC", header.WmoId, "WmoId");
            Assert.AreEqual(string.Empty, header.Designator, "Designator");
            Assert.AreEqual(new TimeSpan(0, 56, 0), header.IssuedAt.TimeOfDay);
            Assert.IsFalse(header.IsCorrection(), "header.IsCorrection()");
            Assert.IsFalse(header.IsReissuance(), "header.IsReissuance()");
        }

        [TestMethod]
        public void TornadoWarningSegmentTests()
        {
            var product = GetTornadoWarning();
            Assert.IsNotNull(product, "Unable to create text product");
            Assert.IsNotNull(product.Content, "Content != null");
            Assert.IsFalse(product.IsResent(), "product.IsResent()");
            Assert.AreEqual("FINCH", product.GetSignature());

            var segments = product.GetSegments().ToList();
            Assert.AreEqual(2, segments.Count, "Segment Count");
            var bullets = segments[0].GetBullets().ToList();
        }

        [TestMethod]
        public void FloodWarningHeaderTests()
        {
            var product = GetFloodWarning();
            Assert.IsNotNull(product, "Unable to create text product");
            Assert.IsNotNull(product.Content, "Content != null");
            Assert.AreEqual("WGUS43 KOAX 070445\r\nFLWOAX\r\n", product.Content.RawHeader, "Raw Header");

            var header = product.GetWmoHeader();
            Assert.AreEqual("FLW", header.ProductCategory, "ProductCategory");
            Assert.AreEqual("OAX", header.LocationIdentifier, "LocationIdentifier");
            Assert.AreEqual("WG", header.DataType, "DataType");
            Assert.AreEqual("US43", header.Distribution, "Distribution");
            Assert.AreEqual("KOAX", header.WmoId, "WmoId");
            Assert.AreEqual(string.Empty, header.Designator, "Designator");
            Assert.AreEqual(new TimeSpan(4, 45, 0), header.IssuedAt.TimeOfDay);
            Assert.IsFalse(header.IsCorrection(), "header.IsCorrection()");
            Assert.IsFalse(header.IsReissuance(), "header.IsReissuance()");
        }

        [TestMethod]
        public void FloodWarningSegmentTests()
        {
            var product = GetFloodWarning();
            Assert.IsNotNull(product, "Unable to create text product");
            Assert.IsNotNull(product.Content, "Content != null");
            Assert.IsFalse(product.IsResent(), "product.IsResent()");
            Assert.AreEqual("FOBERT", product.GetSignature());

            var segments = product.GetSegments().ToList();
            Assert.AreEqual(2, segments.Count, "Segment Count");
        }

        #endregion Public Methods

        #region Private Methods

        private static TextProduct GetTornadoWarning() => TextProduct.Create(
            "TORDDCXXXX.TXT",
            new DateTimeOffset(2015, 6, 5, 0, 56, 0, TimeSpan.Zero),
            Encoding.ASCII.GetBytes(WarningProducts.TornadoWarning),
            DateTimeOffset.UtcNow, string.Empty);

        private static TextProduct GetFloodWarning() => TextProduct.Create(
            "FLWOAXXXXX.TXT",
            new DateTimeOffset(2015, 6, 5, 23, 45, 0, TimeSpan.Zero),
            Encoding.ASCII.GetBytes(WarningProducts.FloodWarning),
            DateTimeOffset.UtcNow, string.Empty);

        #endregion Private Methods
    }
}
