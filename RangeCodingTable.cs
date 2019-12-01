namespace DevOnMobile
{
    public class RangeCodingTable
    {
        public int[] CharStart { get; } = new int[256];
        public int[] CharSize { get; } = new int[256];
        public int TotalRange { get; set; }
    }
}