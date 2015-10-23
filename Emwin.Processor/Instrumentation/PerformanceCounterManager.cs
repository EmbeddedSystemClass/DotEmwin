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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Emwin.Processor.Instrumentation
{
    internal class PerformanceCounterManager
    {
        private readonly Dictionary<string, SafePerformanceCounter> _counterMap = new Dictionary<string, SafePerformanceCounter>();

        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceCounterManager" /> class.
        /// </summary>
        /// <param name="categoryInfo">The category information.</param>
        /// <param name="counterDefinitions">The counter definitions.</param>
        protected PerformanceCounterManager(PerformanceCounterCategoryInfo categoryInfo, CounterCreationData[] counterDefinitions)
        {
            var category = GetOrCreateCounterCategory(categoryInfo, counterDefinitions);
            foreach (var counter in category.GetCounters())
            {
                counter.ReadOnly = false;
                _counterMap.Add(counter.CounterName, new SafePerformanceCounter(counter));
            }
        }

        /// <summary>
        /// Gets the or create counter category.
        /// </summary>
        /// <param name="categoryInfo">The category information.</param>
        /// <param name="counters">The counters.</param>
        /// <returns>PerformanceCounterCategory.</returns>
        private static PerformanceCounterCategory GetOrCreateCounterCategory(
            PerformanceCounterCategoryInfo categoryInfo, CounterCreationData[] counters)
        {
            var creationPending = true;
            var categoryExists = false;
            var categoryName = categoryInfo.CategoryName;
            var counterNames = new HashSet<string>(counters.Select(info => info.CounterName));
            PerformanceCounterCategory category = null;

            if (PerformanceCounterCategory.Exists(categoryName))
            {
                categoryExists = true;
                category = new PerformanceCounterCategory(categoryName);
                var counterList = category.GetCounters();
                if (category.CategoryType == categoryInfo.CategoryType && counterList.Length == counterNames.Count)
                {
                    creationPending = counterList.Any(x => !counterNames.Contains(x.CounterName));
                }
            }

            if (!creationPending) return category;

            if (categoryExists)
                PerformanceCounterCategory.Delete(categoryName);

            var counterCollection = new CounterCreationDataCollection(counters);

            category = PerformanceCounterCategory.Create(
                categoryInfo.CategoryName,
                categoryInfo.CategoryHelp,
                categoryInfo.CategoryType,
                counterCollection);

            return category;
        }

        /// <summary>
        /// Gets the counter.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>SafePerformanceCounter.</returns>
        /// <exception cref="System.InvalidOperationException">$Counter named '{name}' could not be found</exception>
        public SafePerformanceCounter GetCounter(string name)
        {
            SafePerformanceCounter counter;
            if (!_counterMap.TryGetValue(name, out counter))
            {
                throw new InvalidOperationException($"Counter named '{name}' could not be found");
            }

            return counter;
        }
    }
}