using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JosephM.Core.Extentions
{
    public static class ByteExtentions
    {
        public static Stream ToStream(this byte[] bytes)
        {
            var stream = new MemoryStream();
            stream.Write(bytes, 0, bytes.Length);
            return stream;
        }
    }
}
