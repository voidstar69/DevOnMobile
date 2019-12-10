using System;

namespace DevOnMobile
{
    public class RangeDecoder
    {
        private const int MaxRange = 100000;
        private const int FirstDigitScalar = 10000;
        private const int RangeBase = 10;
        private const char EndOfMessageChar = '\0';

        private string input;
        private string output;
        private int code = 0;
        private int low = 0;
        private int range = 1;

        public string decode(string data, RangeCodingTable table)
        {
            input = data;

            Console.WriteLine("Decoder: data len: {0}, total: {1}, input: {2}", data.Length, table.TotalRange, data);

            //AppendDigit(); // need to get range/total >0
            while (range < table.TotalRange)
            {
                AppendDigit();
            }

            byte[] posToChar = BuildRangePointToCharMap(table);

            while (true)                      
            {
                // code is in what symbol range?
                int pos = GetValue(table.TotalRange);

                // convert position in range into symbol
                byte ch = posToChar[pos];

                Console.WriteLine("Range: {0}-{1}, code: {2}, pos: {3}/{4}, char: {5} ({6})", low, low + range, code, pos, table.TotalRange, (char) ch, ch);

                // stop when receive EOM
                if (ch == EndOfMessageChar)
                    break;

                output += (char) ch;

                Decode(table.CharStart[ch], table.CharSize[ch], table.TotalRange);
            }

            Console.WriteLine("Decoder output: {0}", output);
            Console.WriteLine();

            return output;
        }

        private static byte[] BuildRangePointToCharMap(RangeCodingTable table)
        {
            var posToChar = new byte[table.TotalRange];
            for(int ch = 0; ch < table.CharStart.Length; ch++)
            {
                int size = table.CharSize[ch];
                if (size > 0)
                {
                    int start = table.CharStart[ch];
                    for (int offset = 0; offset < size; offset++)
                    {
                        posToChar[start + offset] = (byte)ch;
                    }
                }
            }

            return posToChar;
        }

        private int GetValue(int total)
        {
            return (code - low) / (range / total);
            //return (code - low) * total / range;
        }

        private int ReadNextDigit()
        {
            if (input.Length == 0)
                return 0;

            char nextChar = input[0];
            input = input.Substring(1);
            return int.Parse(nextChar.ToString());
        }

        private void AppendDigit()
        {
            code = (code % FirstDigitScalar) * RangeBase + ReadNextDigit();
            low = (low % FirstDigitScalar) * RangeBase;
            range *= RangeBase;
        }

        private void Decode(int start, int size, int total)  // Decode is same as Encode with EmitDigit replaced by AppendDigit
        {
            // adjust the range based on the symbol interval
            range /= total;
            low += start * range;
            range *= size;

            if(range < 1)
                throw new ApplicationException("range must never be zero");

            // check if left-most digit is same throughout range
            while (low / FirstDigitScalar == (low + range) / FirstDigitScalar)
                AppendDigit();

            // readjust range - see reason for this below
            if (range < FirstDigitScalar / RangeBase)
            {
                AppendDigit();
                AppendDigit();
                range = MaxRange - low;
            }
        }
    }
}