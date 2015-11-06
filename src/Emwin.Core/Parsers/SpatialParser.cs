﻿/*
 * Microsoft Public License (MS-PL)
 * Copyright (c) 2015 Jonathan Bradshaw <jonathan@nrgup.net>
 *     
 * This license governs use of the accompanying software. If you use the software, you
 * accept this license. If you do not accept the license, do not use the software.
 *     
 * 1. Definitions
 *     The terms "reproduce," "reproduction," "derivative works," and "distribution" have the
 *     same meaning here as under U.S. copyright law.
 *     A "contribution" is the original software, or any additions or changes to the software.
 *     A "contributor" is any person that distributes its contribution under this license.
 *     "Licensed patents" are a contributor's patent claims that read directly on its contribution.
 *     
 * 2. Grant of Rights
 *     (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
 *     (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
 *     
 * 3. Conditions and Limitations
 *     (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
 *     (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from such contributor to the software ends automatically.
 *     (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present in the software.
 *     (D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a license that complies with this license.
 *     (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement.
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

        private static readonly Regex PolygonRegex = new Regex(@"^LAT\.{3}LON(?:\s(?<points>[0-9]{4}\s[0-9]{4}))+", RegexOptions.ExplicitCapture | RegexOptions.Multiline | RegexOptions.Compiled);

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