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
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Emwin.Core.References
{
    public sealed class FipsCode
    {

        #region Private Fields

        private const string ResourceName = "Emwin.Core.Resources.FipsCodes.txt";
        private static readonly Lazy<Dictionary<string, FipsCode>> LookupTable = new Lazy<Dictionary<string, FipsCode>>(GetTable);

        #endregion Private Fields

        #region Private Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FipsCode" /> class.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="stateFips">The state fips.</param>
        /// <param name="countyFips">The county fips.</param>
        /// <param name="county">The county.</param>
        /// <param name="classFips">The class fips.</param>
        private FipsCode(string state, string stateFips, string countyFips, string county, string classFips)
        {
            State = state;
            StateFipsCode = stateFips;
            CountyFipsCode = countyFips;
            CountyName = county;
            ClassFipsCode = classFips;
        }

        #endregion Private Constructors

        #region Public Properties

        /// <summary>
        /// Gets the class fips code.
        /// </summary>
        /// <value>The class fips code.</value>
        public string ClassFipsCode { get; }

        /// <summary>
        /// Gets the county code.
        /// </summary>
        /// <value>The county code.</value>
        public string CountyFipsCode { get; }

        /// <summary>
        /// Gets the county name.
        /// </summary>
        /// <value>The county.</value>
        public string CountyName { get; }
        /// <summary>
        /// Gets the 2 letter state.
        /// </summary>
        /// <value>The state.</value>
        public string State { get; }

        /// <summary>
        /// Gets the state FIPS code.
        /// </summary>
        /// <value>The state code.</value>
        public string StateFipsCode { get; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Gets the FIPS code.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="countyFips">The county fips.</param>
        /// <returns>Emwin.Core.References.SameCode.</returns>
        public static FipsCode Get(string state, string countyFips)
        {
            FipsCode result;
            return LookupTable.Value.TryGetValue(state + countyFips, out result) ? result : null;
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Gets the table from the resource file.
        /// </summary>
        /// <returns>System.Collections.Generic.Dictionary&lt;System.String, Emwin.Core.References.SameCode&gt;.</returns>
        private static Dictionary<string, FipsCode> GetTable()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var table = new Dictionary<string, FipsCode>();

            using (var stream = assembly.GetManifestResourceStream(ResourceName))
            using (var reader = new StreamReader(stream ?? Stream.Null))
            {
                while (!reader.EndOfStream)
                {
                    var columns = reader.ReadLine()?.Split(',');
                    if (columns != null && columns.Length == 5)
                    {
                        table.Add(columns[0] + columns[2], 
                            new FipsCode(columns[0], columns[1], columns[2], columns[3], columns[4]));
                    }
                }
            }

            return table;
        }

        #endregion Private Methods

    }
}
