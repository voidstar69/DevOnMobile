namespace DevOnMobile
{
    public class RangeCoding : ITextCodec
    {
        public string encode(string data)
        {
            if(string.IsNullOrEmpty(data))
                return data;

            return new RangeEncoder().encode(data);
        }

        public string decode(string data)
        {
            if(string.IsNullOrEmpty(data))
                return data;

            return new RangeDecoder().decode(data);
        }
    }
}