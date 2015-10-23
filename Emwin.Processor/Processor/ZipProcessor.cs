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
using System.IO;
using System.IO.Compression;
using System.Linq;
using Emwin.Core.Models;
using Emwin.Processor.Pipeline;

namespace Emwin.Processor.Processor
{
    internal sealed class ZipProcessor
    {
        /// <summary>
        /// Unzip the product if required.
        /// </summary>
        /// <param name="product">The product.</param>
        /// <returns>WeatherProduct.</returns>
        public WeatherProduct Execute(WeatherProduct product)
        {
            return product.IsCompressed() ? Unzip(product) : product;
        }

        /// <summary>
        /// Unzips the product and returns the first contained product in the zip. 
        /// Assumes a single product is contained inside the zip file.
        /// </summary>
        /// <param name="product">The compressed product.</param>
        /// <returns>WeatherProduct.</returns>
        private static WeatherProduct Unzip(WeatherProduct product)
        {
            using (var zip = new ZipArchive(product.GetStream(), ZipArchiveMode.Read))
            {
                var file = zip.Entries.First();
                using (var ms = new MemoryStream())
                using (var fileStream = file.Open())
                {
                    fileStream.CopyTo(ms);
                    var content = ms.ToArray();

                    return new WeatherProduct
                    {
                        Filename = file.Name.ToUpperInvariant(),
                        TimeStamp = file.LastWriteTime,
                        Content = content,
                        Hash = content.ComputeHash(),
                        ReceivedAt = DateTimeOffset.UtcNow
                    };
                }
            }
        }

    }
}