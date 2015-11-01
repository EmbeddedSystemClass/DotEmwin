/*
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

using System.Text.RegularExpressions;
using Emwin.Core.Contracts;
using Emwin.Core.DataObjects;

namespace Emwin.Core.Parsers
{
    /// <summary>
    /// AwipsHeader represents the header information on the first line of the product.
    /// </summary>
    public static class HeadingParser
    {

        #region Private Fields

        /// <summary>
        /// The WMO Abbreviated Header
        /// http://www.nws.noaa.gov/tg/head.php
        /// </summary>
        private static readonly Regex WmoHeaderRegex = new Regex(
            @"^(?<id>[A-Z]{4}[0-9]{2})\s(?<station>[A-Z]{4})\s(?<time>[0-9]{6})(\s(?<indicator>[A-Z]{3}))?(\r\r\n(?<pil>[A-Z ]{6}))?\r\r\n", 
            RegexOptions.ExplicitCapture | RegexOptions.Singleline | RegexOptions.Compiled);

        #endregion Private Fields

        #region Public Methods

        /// <summary>
        /// Creates the header from the content.
        /// </summary>
        /// <param name="product">The product.</param>
        /// <returns>Header.</returns>
        /// <exception cref="System.ArgumentException">Invalid content header</exception>
        public static ICommsHeader ParseProduct(ITextProduct product)
        {
            var match = WmoHeaderRegex.Match(product.Content);
            if (!match.Success) return new CommsHeader();

            return new CommsHeader
            {
                AwipsId = match.Groups["id"].Value,
                WmoId = match.Groups["station"].Value,
                Time = TimeParser.ParseDayHourMinute(product.TimeStamp, match.Groups["time"].Value),
                Indicator = match.Groups["indicator"].Success ? match.Groups["indicator"].Value : null,
                AfosPil = match.Groups["pil"].Success ? match.Groups["pil"].Value.TrimEnd() : null
            };
        }

        #endregion Public Methods

    }
}
