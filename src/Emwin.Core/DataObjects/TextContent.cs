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

using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Emwin.Core.DataObjects
{
    [DataContract]
    public class TextContent
    {

        #region Private Fields

        /// <summary>
        /// The header regex, example:
        /// WFUS53 KDDC 050056
        /// TORDDC
        /// </summary>
        private static readonly Regex HeaderRegex = new Regex(
            @"^[A-Z]{4}[0-9]{2}\s[A-Z]{4}\s[0-9]{6}(\s[A-Z]{3})?(\r\n[A-Z0-9\s]{4,6}\r\n)?",
            RegexOptions.ExplicitCapture | RegexOptions.Singleline | RegexOptions.Compiled);

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TextContent"/> class.
        /// </summary>
        public TextContent()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextContent"/> class.
        /// </summary>
        /// <param name="content">The raw.</param>
        public TextContent(string content)
        {
            var match = HeaderRegex.Match(content);
            if (match.Success)
            {
                RawHeader = match.Value;
                RawBody = content.Substring(match.Length);
            }
            else
            {
                RawHeader = string.Empty;
                RawBody = content;
            }
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// Gets or sets the body.
        /// </summary>
        /// <value>The body.</value>
        [DataMember]
        public string RawBody { get; set; }

        /// <summary>
        /// Gets or sets the raw header.
        /// </summary>
        /// <value>The header.</value>
        [DataMember]
        public string RawHeader { get; set; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Gets the body without control characters.
        /// </summary>
        /// <returns>System.String.</returns>
        public string GetBodyText() => new string(RawBody.Where(c => char.IsWhiteSpace(c) || !char.IsControl(c)).ToArray());

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString() => string.Concat(RawHeader, RawBody);

        #endregion Public Methods
    }
}