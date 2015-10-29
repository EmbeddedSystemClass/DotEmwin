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
using Emwin.Core.Types;

namespace Emwin.Core.Interfaces
{
    /// <summary>
    /// Interface IEmwinContent. Represents the base interface for all received content from EMWIN.
    /// </summary>
    public interface IEmwinContent
    {
        #region Public Properties

        /// <summary>
        /// Gets the type of the content.
        /// </summary>
        /// <value>The type of the content.</value>
        ContentFileType ContentType { get; }

        /// <summary>
        /// Gets the filename.
        /// </summary>
        /// <value>The filename.</value>
        string Filename { get; }

        /// <summary>
        /// Gets the received at time.
        /// </summary>
        /// <value>The received at.</value>
        DateTimeOffset ReceivedAt { get; }

        /// <summary>
        /// Gets the content time stamp.
        /// </summary>
        /// <value>The time stamp.</value>
        DateTimeOffset TimeStamp { get; }

        #endregion Public Properties
    }

    /// <summary>
    /// Interface IEmwinContent. Represents EMWIN received content of a specified generic type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IEmwinContent<out T> : IEmwinContent where T: class
    {
        #region Public Properties

        /// <summary>
        /// Gets the content.
        /// </summary>
        /// <value>The content.</value>
        T Content { get; }

        #endregion Public Properties
    }
}