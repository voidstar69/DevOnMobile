namespace DevOnMobile
{
    public class RangeCodingTable
    {
        public ulong[] CharStart { get; } = new ulong[256];
        public ulong[] CharSize { get; } = new ulong[256];
        public ulong TotalRange { get; set; }
    }
}