using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHIP8_VM.Utility
{
    static class Extensions
    {
        public static int ReadStreamToBuf(this Stream stream, byte[] buffer, int offset, int count, int chunkSize = 8196)
        {
            if (offset < 0 || count < 0)
                throw new ArgumentOutOfRangeException("offset or count is less than zero", (Exception)null);
            if (stream.Length < offset + count)
                throw new ArgumentOutOfRangeException("offset plus count denote not valid position in stream", (Exception)null);

            if (buffer.Length < offset + count)
                throw new ArgumentOutOfRangeException("buffer", "buffer length is not enough to store stream data");

            long leftToRead = stream.Length;
            int bytesRead = 0;
            while (leftToRead > 0)
            {
                byte[] tempBuf = new byte[chunkSize];
                int n = stream.Read(tempBuf, offset + bytesRead, chunkSize);
                leftToRead -= n;
                if (n > 0)
                {
                    Array.Copy(tempBuf, 0, buffer, bytesRead, n);
                    bytesRead += n;
                }
            }
            return bytesRead;
        }

    }
}
