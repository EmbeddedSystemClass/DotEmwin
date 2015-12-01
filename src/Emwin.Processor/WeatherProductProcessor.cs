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
using Emwin.Core.Contracts;
using Emwin.Core.DataObjects;
using Emwin.Core.Products;
using Emwin.Processor.CoreHandlers;
using Emwin.Processor.EventAggregator;
using Emwin.Processor.Instrumentation;

namespace Emwin.Processor
{
    public class WeatherProductProcessor : ProcessorBase<QuickBlockTransferSegment>, IObserver<QuickBlockTransferSegment>
    {
        #region Private Fields

        private readonly ObservableListener<TextProductSegment> _segmentObservable = new ObservableListener<TextProductSegment>();
        private readonly ObservableListener<ImageProduct> _imageObservable = new ObservableListener<ImageProduct>();
        private readonly ObservableListener<TextProduct> _textObservable = new ObservableListener<TextProduct>();
        private readonly ObservableListener<XmlProduct> _xmlObservable = new ObservableListener<XmlProduct>();
        private readonly ObservableListener<alert> _capObservable = new ObservableListener<alert>();

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WeatherProductProcessor" /> class.
        /// </summary>
        /// <param name="observable">The observable.</param>
        public WeatherProductProcessor(IObservable<QuickBlockTransferSegment> observable = null)
        {
            observable?.Subscribe(this);
        }

        #endregion Public Constructors

        #region Public Methods

        /// <summary>
        /// Gets the product segment observable.
        /// </summary>
        /// <returns>System.IObservable&lt;Emwin.Core.Interfaces.BulletinProduct&gt;.</returns>
        public IObservable<TextProductSegment> GetSegmentObservable() => _segmentObservable;

        /// <summary>
        /// Gets the image observable.
        /// </summary>
        /// <returns>System.IObservable&lt;Emwin.Core.Interfaces.TextProduct&gt;.</returns>
        public IObservable<ImageProduct> GetImageObservable() => _imageObservable;

        /// <summary>
        /// Gets the text observable.
        /// </summary>
        /// <returns>System.IObservable&lt;Emwin.Core.Interfaces.TextProduct&gt;.</returns>
        public IObservable<TextProduct> GetTextObservable() => _textObservable;

        /// <summary>
        /// Gets the XML observable.
        /// </summary>
        /// <returns>System.IObservable&lt;Emwin.Core.Contracts.XmlProduct&gt;.</returns>
        public IObservable<XmlProduct> GetXmlObservable() => _xmlObservable;

        /// <summary>
        /// Gets the cap observable.
        /// </summary>
        /// <returns>System.IObservable&lt;Emwin.Core.Contracts.alert&gt;.</returns>
        public IObservable<alert> GetCapObservable() => _capObservable;

        /// <summary>
        /// Called when input is completed.
        /// </summary>
        public void OnCompleted()
        {
            ProcessorEventSource.Log.Info("WeatherProductProcessor", "Observable source indicated completion");
            Stop();
        }

        /// <summary>
        /// Called when error occurs.
        /// </summary>
        /// <param name="error">The error.</param>
        public void OnError(Exception error)
        {
            ProcessorEventSource.Log.Error("WeatherProductProcessor", "Observable source error: " + error);
            Stop();
        }

        /// <summary>
        /// Called when next block segment is available for processing.
        /// </summary>
        /// <param name="blockSegment">The value.</param>
        public void OnNext(QuickBlockTransferSegment blockSegment)
        {
            SegmentQueue.Add(blockSegment);
        }

        #endregion Public Methods

        #region Protected Methods

        /// <summary>
        /// Gets and wires up the event aggregator.
        /// </summary>
        /// <returns>Emwin.Processor.EventAggregator.IEventAggregator.</returns>
        protected override IEventAggregator GetAggregator()
        {
            var aggregator = new EventAggregator.EventAggregator();

            aggregator
                // Observable Listeners
                .AddListener(_xmlObservable)
                .AddListener(_textObservable)
                .AddListener(_imageObservable)
                .AddListener(_segmentObservable)
                //.AddListener(_capObservable)

                // Core Listener Handlers
                .AddListener<BlockSegmentBundler>()
                .AddListener<ProductAssembler>()
                .AddListener<ZipExtractor>()
                .AddListener<TextProductSplitter>()
                .AddListener<XmlProductSplitter>();

            // Product Based Handlers

            return aggregator;
        }

        #endregion Protected Methods

    }
}
