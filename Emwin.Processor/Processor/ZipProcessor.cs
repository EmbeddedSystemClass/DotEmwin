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
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks.Dataflow;
using Emwin.Core.Interfaces;
using Emwin.Core.Models;
using Emwin.Core.Parsers;
using Emwin.Core.Products;
using Emwin.Core.Types;
using Emwin.Processor.Pipeline;

namespace Emwin.Processor.Processor
{
    internal sealed class ZipProcessor
    {
        #region Public Properties

        /// <summary>
        /// Gets the block.
        /// </summary>
        /// <value>The block.</value>
        public TransformBlock<IEmwinContent, IEmwinContent> Block { get; } = new TransformBlock<IEmwinContent, IEmwinContent>(
            product => Unzip((CompressedProduct) product),
            new ExecutionDataflowBlockOptions {MaxDegreeOfParallelism = -1});

        #endregion Public Properties

        /// <summary>
        /// Filters the specified content.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns>System.Boolean.</returns>
        public bool Predicate(IEmwinContent content) => content is ICompressedContent;

        #region Private Methods

        /// <summary>
        /// Unzips the product and returns the first contained product in the zip. 
        /// Assumes a single product is contained inside the zip file.
        /// </summary>
        /// <param name="product">The compressed product.</param>
        /// <returns>WeatherProduct.</returns>
        private static IEmwinContent Unzip(CompressedProduct product)
        {
            using (var zip = new ZipArchive(product.GetStream(), ZipArchiveMode.Read))
            {
                var file = zip.Entries.First();
                using (var fileStream = file.Open())
                {
                    var content = ReadAllBytes(fileStream);

                    switch (ContentTypeParser.GetFileContentType(file.Name))
                    {
                        case ContentFileType.Text:
                            return ProductFactory.CreateTextProduct(file.Name.ToUpperInvariant(), file.LastWriteTime,
                                content, DateTimeOffset.UtcNow);

                        case ContentFileType.Image:
                            return ProductFactory.CreateImageProduct(file.Name.ToUpperInvariant(), file.LastWriteTime,
                                content, DateTimeOffset.UtcNow);
                    }
                }
            }

            return product;
        }

        private static byte[] ReadAllBytes(Stream stream)
        {
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }


        #endregion Private Methods
    }
}