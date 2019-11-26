namespace DevOnMobile
{
    public class RangeCodingTable
    {
        public int[] CharStart { get; set; } = new int[256];
        public int[] CharSize { get; set; } = new int[256];
        public int TotalRange { get; set; }
    }
}