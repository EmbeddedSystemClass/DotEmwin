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

using System.Diagnostics;

namespace Emwin.Processor.Instrumentation
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