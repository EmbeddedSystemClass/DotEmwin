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
using System.Runtime.Serialization;
using System.Text;

namespace Emwin.Core.Models
{
    [DataContract]
    public abstract class AbstractContent
    {
        #region Private Fields

        private readonly Lazy<string> _contentStr;

        #endregion Private Fields

        #region Protected Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractContent"/> class.
        /// </summary>
        protected AbstractContent()
        {
            _contentStr = new Lazy<string>(() => Encoding.ASCII.GetString(Content));
        }

        #endregion Protected Constructors

        #region Public Enums

        /// <summary>
        /// Data Content File Type
        /// </summary>
        public enum FileContent
        {
            Unknown,
            Text,
            Image,
            Compressed
        }

        #endregion Public Enums

        #region Public Properties

        /// <summary>
        /// Gets or sets the body content.
        /// </summary>
        /// <value>The body.</value>
        [DataMember]
        public byte[] Content { get; set; }

        /// <summary>
        /// Gets the type of the file.
        /// </summary>
        /// <returns>WeatherProductFileType.</returns>
        [IgnoreDataMember]
        public FileContent ContentType
        {
            get
            {
                switch (Path.GetExtension(Filename))
                {
                    case ".TXT":
                        return FileContent.Text;
                    case ".ZIS":
                        return FileContent.Compressed;

                    case ".JPG":
                    case ".GIF":
                        return FileContent.Image;
                }

                return FileContent.Unknown;
            }
        }

        /// <summary>
        /// Gets or sets the filename.
        /// </summary>
        /// <value>The filename.</value>
        [DataMember]
        public string Filename { get; set; }

        /// <summary>
        /// Gets or sets the received at time.
        /// </summary>
        /// <value>The received at.</value>
        [DataMember]
        public DateTimeOffset ReceivedAt { get; set; }

        /// <summary>
        /// Gets or sets the time stamp.
        /// </summary>
        /// <value>The time stamp.</value>
        [DataMember]
        public DateTimeOffset TimeStamp { get; set; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Gets the content as a stream.
        /// </summary>
        /// <returns>System.IO.Stream.</returns>
        public Stream GetStream() => new MemoryStream(Content, false);

        /// <summary>
        /// Gets the content as a string.
        /// </summary>
        /// <returns>System.String.</returns>
        public string GetString() => _contentStr.Value;

        /// <summary>
        /// Determines whether this content is compressed.
        /// </summary>
        /// <returns>System.Boolean.</returns>
        public bool IsCompressed() => ContentType == FileContent.Compressed;

        /// <summary>
        /// Determines whether this content is image.
        /// </summary>
        /// <returns>System.Boolean.</returns>
        public bool IsImage() => ContentType == FileContent.Image;

        /// <summary>
        /// Determines whether this content is text.
        /// </summary>
        /// <returns>System.Boolean.</returns>
        public bool IsText() => ContentType == FileContent.Text;

        #endregion Public Methods

    }
}