using System;

namespace DevOnMobile
{
    public class RangeEncoder
    {
        private const char EndOfMessageChar = '\0';
        private int low;
        private int range;
        private string output;

        public string encode(string data, out RangeCodingTable table)
        {
            table = null;

            if(string.IsNullOrEmpty(data))
                return data;

            low = 0;
            range = 100000;
            int total = RoundToNextPowerOf10(data.Length + 1); // count including invented EOM char
            if (total > 1000)
            {
                total = 1000;
            }

            table = new RangeCodingTable {TotalRange = total};
            int[] charStart = table.CharStart;
            int[] charSize = table.CharSize;

            foreach (char ch in data)
            {
                charSize[ch]++;
            }
            charSize[EndOfMessageChar]++;

            // when making TotalRange a power of 10, char sizes need to be adjusted to cover this full range
            for (var ch = 0; ch < 256; ch++)
            {
                int size = charSize[ch];
                if (size > 0)
                {
                    charSize[ch] = Math.Max(1, size * total / (data.Length + 1));
                }
            }

            // Here we make the EOM char the first char not the last char. This changes the final result from this method.
            int pos = 0;
            for(int ch = 0; ch < 256; ch++)
            {
                if (charSize[ch] > 0)
                {
                    charStart[ch] = pos;
                    pos += charSize[ch];
                }
            }

            output = string.Empty;

            Console.WriteLine("Encoder: total: {0}, input length: {1}, input: {2}", total, data.Length, data);

            for(int i = 0; i <= data.Length; i++)
            {
                char ch = (i == data.Length ? '\0' : data[i]);
                encodeChar(charStart[ch], charSize[ch], total, ch);
            }

            // emit final digits - see below
            while (range < 10000)
                emitDigit();

            low += 10000;
            emitDigit();

            Console.WriteLine("Encoder output: {0}", output);
            Console.WriteLine();

            return output;
        }

        private static int RoundToNextPowerOf10(int num)
        {
            int pow10 = 10;
            while (pow10 < num)
            {
                pow10 *= 10;
            }
            return pow10;
        }

        private int tooManyCallsGuard;

        private void emitDigit()
        {
            if (tooManyCallsGuard++ > 5000)
            {
                throw new ApplicationException("Too many calls to emitDigit!");
            }

            Console.WriteLine("Range: {0}-{1}, digit: {2}", low, low + range, low / 10000);
            output += low / 10000;
            //Console.Write(low / 10000);
            low = (low % 10000) * 10;
            range *= 10;
        }

        private void encodeChar(int start, int size, int total, char ch)
        {
            Console.WriteLine("Range: {0}-{1}, start: {2}, size: {3}, total: {4}, char: {5} ({6})", low, low + range, start, size, total, ch, (int)ch);

            if(range < total)
            {
                throw new ApplicationException($"Range {range} less than Total {total} => range will become zero!");
            }

            // adjust the range based on the symbol interval
            range /= total;
            low += start * range;
            range *= size;

            // check if left-most digit is same throughout range
            while (low / 10000 == (low + range) / 10000)
                emitDigit();

            // readjust range - see reason for this below
            if (range < 1000)
            {
                emitDigit();
                emitDigit();
                range = 100000 - low;

                Console.WriteLine("Range: {0}-{1}", low, low + range);
            }
        }
    }
}