using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Android___MusicController
{
    public static class StaticHelpers
    {
        public static byte[] CombineImageDataChunks(List<byte[]> imageDataChunks)
        {
            var combinedImageData = new byte[imageDataChunks.Sum(chunk => chunk.Length)];
            var offset = 0;

            foreach (var chunk in imageDataChunks)
            {
                Buffer.BlockCopy(chunk, 0, combinedImageData, offset, chunk.Length);
                offset += chunk.Length;
            }

            return combinedImageData;
        }
    }
}
