// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace Emwin.ByteBlaster.Instrumentation
{
    internal sealed class PerformanceCounterCategoryInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceCounterCategoryInfo"/> class.
        /// </summary>
        /// <param name="categoryName">Name of the category.</param>
        /// <param name="categoryType">Type of the category.</param>
        /// <param name="categoryHelp">The category help.</param>
        public PerformanceCounterCategoryInfo(string categoryName, PerformanceCounterCategoryType categoryType,
            string categoryHelp)
        {
            CategoryName = categoryName;
            CategoryType = categoryType;
            CategoryHelp = categoryHelp;
        }

        /// <summary>
        /// Gets the name of the category.
        /// </summary>
        /// <value>The name of the category.</value>
        public string CategoryName { get; }

        /// <summary>
        /// Gets the type of the category.
        /// </summary>
        /// <value>The type of the category.</value>
        public PerformanceCounterCategoryType CategoryType { get; }

        /// <summary>
        /// Gets the category help.
        /// </summary>
        /// <value>The category help.</value>
        public string CategoryHelp { get; }
    }
}