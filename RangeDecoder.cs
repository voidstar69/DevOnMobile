using System;
using System.Collections.Generic;
using System.Linq;

namespace DevOnMobile
{
    public class RangeDecoder
    {
        //private const int MaxRange = 100000;
        //private const int FirstDigitScalar = 10000;
        //private const int RangeBase = 10;
        private const ulong MaxRange = 0x10000000;
        private const ulong FirstDigitScalar = 0x100000;
        private const ulong RangeBase = 0x100;
        private const char EndOfMessageChar = '\0';

        private List<string> input;
        private string output;
        private ulong code = 0;
        private ulong low = 0;
        private ulong range = 1;

        public string decode(string data, RangeCodingTable table)
        {
            var a = new List<string>();
            input = data.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).ToList();

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
                ulong pos = GetValue(table.TotalRange);

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
                ulong size = table.CharSize[ch];
                if (size > 0)
                {
                    ulong start = table.CharStart[ch];
                    for (ulong offset = 0; offset < size; offset++)
                    {
                        posToChar[start + offset] = (byte)ch;
                    }
                }
            }

            return posToChar;
        }

        private ulong GetValue(ulong total)
        {
            return (code - low) / (range / total);
            //return (code - low) * total / range;
        }

        private ulong ReadNextDigit()
        {
            if (input.Count == 0)
                return 0;

            string nextNum = input[0];
            input.RemoveAt(0);

            return ulong.Parse(nextNum);
        }

        private void AppendDigit()
        {
            code = (code % FirstDigitScalar) * RangeBase + ReadNextDigit();
            low = (low % FirstDigitScalar) * RangeBase;
            range *= RangeBase;
        }

        private void Decode(ulong start, ulong size, ulong total)  // Decode is same as Encode with EmitDigit replaced by AppendDigit
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