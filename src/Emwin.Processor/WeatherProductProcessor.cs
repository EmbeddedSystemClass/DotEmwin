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
using Emwin.Processor.EventAggregator;
using Emwin.Processor.Instrumentation;
using Emwin.Processor.Processor;

namespace Emwin.Processor
{
    public class WeatherProductProcessor : ProcessorBase<IQuickBlockTransferSegment>, IObserver<IQuickBlockTransferSegment>
    {

        #region Private Fields

        private readonly ObservableListener<IBulletinProduct> _bulletinObservable = new ObservableListener<IBulletinProduct>();
        private readonly ObservableListener<IImageProduct> _imageObservable = new ObservableListener<IImageProduct>();
        private readonly ObservableListener<ITextProduct> _textObservable = new ObservableListener<ITextProduct>();
        private readonly ObservableListener<IXmlProduct> _xmlObservable = new ObservableListener<IXmlProduct>();

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WeatherProductProcessor" /> class.
        /// </summary>
        /// <param name="observable">The observable.</param>
        public WeatherProductProcessor(IObservable<IQuickBlockTransferSegment> observable = null)
        {
            observable?.Subscribe(this);
        }

        #endregion Public Constructors

        #region Public Methods

        /// <summary>
        /// Gets the bulletin observable.
        /// </summary>
        /// <returns>System.IObservable&lt;Emwin.Core.Interfaces.IBulletinProduct&gt;.</returns>
        public IObservable<IBulletinProduct> GetBulletinObservable() => _bulletinObservable;

        /// <summary>
        /// Gets the image observable.
        /// </summary>
        /// <returns>System.IObservable&lt;Emwin.Core.Interfaces.ITextProduct&gt;.</returns>
        public IObservable<IImageProduct> GetImageObservable() => _imageObservable;

        /// <summary>
        /// Gets the text observable.
        /// </summary>
        /// <returns>System.IObservable&lt;Emwin.Core.Interfaces.ITextProduct&gt;.</returns>
        public IObservable<ITextProduct> GetTextObservable() => _textObservable;

        public IObservable<IXmlProduct> GetXmlObservable() => _xmlObservable;


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
        public void OnNext(IQuickBlockTransferSegment blockSegment)
        {
            SegmentQueue.Add(blockSegment);
        }

        #endregion Public Methods

        #region Protected Methods

        protected override IEventAggregator GetAggregator()
        {
            var aggregator = new EventAggregator.EventAggregator();

            aggregator
                .AddListener<SegmentBundler>()
                .AddListener<ProductAssembler>()
                .AddListener<ZipProcessor>()
                .AddListener<BulletinSplitter>()
                .AddListener(_xmlObservable)
                .AddListener(_textObservable)
                .AddListener(_imageObservable)
                .AddListener(_bulletinObservable);

            return aggregator;
        }

        #endregion Protected Methods

    }
}
