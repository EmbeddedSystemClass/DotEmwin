/*
 * The MIT License (MIT)
 * 
 * Copyright (c) 2015 Jonathan Bradshaw <jonathan@nrgup.net>
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Spatial;
using Emwin.Core.Contracts;

namespace Emwin.Core.Parsers
{
    public class SpatialParser
    {
        #region Private Fields

        private static readonly Regex PolygonRegex = new Regex(@"^LAT\.{3}LON(?:\s(?<points>[0-9]{4}\s[0-9]{4}))+", RegexOptions.ExplicitCapture | RegexOptions.Multiline);

        #endregion Private Fields

        #region Public Methods

        /// <summary>
        /// Creates the polygon.
        /// </summary>
        /// <param name="points">The geographical points.</param>
        /// <returns>GeographyLineString.</returns>
        public static GeographyPolygon CreatePolygon(IEnumerable<GeographyPosition> points)
        {
            var queue = new Queue<GeographyPosition>(points);
            var startPoint = queue.Dequeue();
            var builder = SpatialBuilder.Create();
            var pipeline = builder.GeographyPipeline;
            pipeline.SetCoordinateSystem(CoordinateSystem.DefaultGeography);
            pipeline.BeginGeography(SpatialType.Polygon);
            pipeline.BeginFigure(startPoint);
            while (queue.Count > 0)
                pipeline.LineTo(queue.Dequeue());

            pipeline.LineTo(startPoint);
            pipeline.EndFigure();
            pipeline.EndGeography();
            return (GeographyPolygon) builder.ConstructedGeography;
        }

        /// <summary>
        /// Converts Geography to well known text format.
        /// </summary>
        /// <param name="geography">The geography.</param>
        /// <returns>System.String.</returns>
        public static string ConvertToWellKnownText(Geography geography) => WellKnownTextSqlFormatter.Create(true).Write(geography);

        /// <summary>
        /// Parses the product and creates the polygon.
        /// </summary>
        /// <param name="product">The product.</param>
        /// <returns>IEnumerable&lt;GeographyLineString&gt;.</returns>
        public static IEnumerable<GeographyPolygon> ParseProduct(ITextProduct product)
        {
            var polygonMatches = PolygonRegex.Matches(product.Content);

            return polygonMatches.Cast<Match>()
                .Select(match => match.Groups["points"].Captures.Cast<Capture>()
                .Select(points => points.Value.Split(' '))
                .Select(split => new GeographyPosition(double.Parse(split[0]) / 100.0, - double.Parse(split[1]) / 100.0)))
                .Select(CreatePolygon);
        }

        #endregion Public Methods

    }
}
