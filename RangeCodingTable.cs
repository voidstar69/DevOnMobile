namespace DevOnMobile
{
    public class RangeCodingTable
    {
        public ulong[] CharStart { get; private set; } 
        public ulong[] CharSize { get; private set; } 
        public ulong TotalRange { get; set; }

        public RangeCodingTable()
        {
            CharStart = new ulong[256];
            CharSize = new ulong[256];
        }
    }
}