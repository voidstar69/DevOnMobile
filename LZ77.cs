using System.IO;

namespace DevOnMobile
{
    public class LempelZiv77Codec : IStreamCodec
    {
        public void encode(Stream inputStream, Stream outputStream)
        {
            int byteOrFlag;
            while (-1 != (byteOrFlag = inputStream.ReadByte()))
            {
                var num = (byte) byteOrFlag;
                outputStream.WriteByte(num);
            }
        }

        public void decode(Stream inputStream, Stream outputStream)
        {
            int byteOrFlag;
            while (-1 != (byteOrFlag = inputStream.ReadByte()))
            {
                var num = (byte) byteOrFlag;
                outputStream.WriteByte(num);
            }
        }
    }
}