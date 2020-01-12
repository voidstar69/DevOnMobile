namespace DevOnMobile
{
    public class RangeCoding : ITextCodec
    {
        private RangeCodingTable table;

        public RangeCoding()
        {
        }

        public RangeCoding(RangeCodingTable table)
        {
            this.table = table;
        }

        public string encode(string data)
        {
            if(string.IsNullOrEmpty(data))
                return data;

            return new RangeEncoder().encode(data, out table);
        }

        public string decode(string data)
        {
            if(string.IsNullOrEmpty(data))
                return data;

            return new RangeDecoder().decode(data, table);
        }
    }
}