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
using System.IO;
using Emwin.Core.Types;

namespace Emwin.Core.Parsers
{
    /// <summary>
    /// Class ContentTypeParser. Responsible for determining the content type.
    /// </summary>
    public static class ContentTypeParser
    {
        public static ContentFileType GetFileContentType(string filename)
        {
            switch (Path.GetExtension(filename.ToUpperInvariant()))
            {
                case ".TXT":
                    return ContentFileType.Text;

                case ".ZIP":
                case ".ZIS":
                    return ContentFileType.Compressed;

                case ".JPG":
                case ".GIF":
                case ".PNG":
                    return ContentFileType.Image;
            }

            return ContentFileType.Unknown;
        }
    }
}