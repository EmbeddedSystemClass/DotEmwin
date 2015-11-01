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

namespace Emwin.Processor.EventAggregator
{
    /// <summary>
    /// Class Pipeline.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Pipeline<T> : IPipelineFilter<T>, IHandle<T>
    {

        #region Private Fields

        /// <summary>
        /// The list of filters
        /// </summary>
        private readonly IList<IPipelineFilter<T>> _filters = new List<IPipelineFilter<T>>();

        /// <summary>
        /// The _current
        /// </summary>
        private int _current;

        /// <summary>
        /// The get next action
        /// </summary>
        private Func<Action<T>> _getNext;

        #endregion Private Fields

        #region Public Properties

        /// <summary>
        /// The count
        /// </summary>
        public int Count => _filters.Count;

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Adds the specified filter.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <returns>Emwin.Core.Pipeline.Pipeline&lt;T&gt;.</returns>
        public Pipeline<T> AddFilter(IPipelineFilter<T> filter)
        {
            _filters.Add(filter);
            return this;
        }

        /// <summary>
        /// Adds the specified filter type.
        /// </summary>
        /// <param name="filterType">Type of the filter.</param>
        /// <returns>Emwin.Core.Pipeline.Pipeline&lt;T&gt;.</returns>
        public Pipeline<T> AddFilter(Type filterType)
        {
            AddFilter((IPipelineFilter<T>)Activator.CreateInstance(filterType));
            return this;
        }

        /// <summary>
        /// Adds this instance.
        /// </summary>
        /// <typeparam name="TFilter">The type of the t filter.</typeparam>
        /// <returns>Emwin.Core.Pipeline.Pipeline&lt;T&gt;.</returns>
        public Pipeline<T> AddFilter<TFilter>() where TFilter : IPipelineFilter<T>
        {
            AddFilter(typeof(TFilter));
            return this;
        }

        /// <summary>
        /// Executes the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        public void Execute(T input)
        {
            ((IPipelineFilter<T>)this).Execute(input, x => { });
        }

        /// <summary>
        /// Handles the specified message by executing the pipeline with it.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="ctx">The CTX.</param>
        public void Handle(T message, IEventAggregator ctx)
        {
            Execute(message);
        }

        /// <summary>
        /// Executes the pipeline.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="executeNext">The execute next.</param>
        void IPipelineFilter<T>.Execute(T input, Action<T> executeNext)
        {
            _current = 0;

            _getNext = () => _current < _filters.Count
                ? x => _filters[_current++].Execute(x, _getNext())
                : executeNext;

            _getNext().Invoke(input);
        }

        #endregion Public Methods

    }
}