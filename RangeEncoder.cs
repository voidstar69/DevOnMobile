using System;

namespace DevOnMobile
{
    public class RangeEncoder
    {
        private const ulong MaxRange = 0x10000000;
        private const ulong FirstDigitScalar = 0x100000;
        private const ulong RangeBase = 0x100;
        private const char EndOfMessageChar = '\0';

        private ulong low;
        private ulong range;
        private string output;

        public string encode(string data, out RangeCodingTable table)
        {
            table = null;

            if(string.IsNullOrEmpty(data))
                return data;

            low = 0;
            range = MaxRange;
            ulong total = RoundToNextPower((ulong)data.Length + 1, RangeBase); // count including invented EOM char
            if (total > FirstDigitScalar / RangeBase)
            {
                total = FirstDigitScalar / RangeBase;
            }

            table = new RangeCodingTable {TotalRange = total};
            ulong[] charStart = table.CharStart;
            ulong[] charSize = table.CharSize;

            foreach (char ch in data)
            {
                charSize[ch]++;
            }
            charSize[EndOfMessageChar]++;

            // when making TotalRange a power of 10, char sizes need to be adjusted to cover this full range
            for (var ch = 0; ch < 256; ch++)
            {
                ulong size = charSize[ch];
                if (size > 0)
                {
                    charSize[ch] = Math.Max(1, size * total / (ulong)(data.Length + 1));
                }
            }

            // Here we make the EOM char the first char not the last char. This changes the final result from this method.
            ulong pos = 0;
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
            while (range < FirstDigitScalar)
                emitDigit();

            low += FirstDigitScalar;
            emitDigit();

            Console.WriteLine("Encoder output: {0}", output);
            Console.WriteLine();

            return output;
        }

        private static ulong RoundToNextPower(ulong num, ulong numBase)
        {
            ulong nextPower = numBase;
            while (nextPower < num)
            {
                nextPower *= numBase;
            }
            return nextPower;
        }

        private int tooManyCallsGuard;

        private void emitDigit()
        {
            if (tooManyCallsGuard++ > 5000)
            {
                throw new ApplicationException("Too many calls to emitDigit!");
            }

            Console.WriteLine("Range: {0}-{1}, digit: {2}", low, low + range, low / FirstDigitScalar);
            output += low / FirstDigitScalar;
            output += ',';
            //Console.Write(low / FirstDigitScalar);
            low = (low % FirstDigitScalar) * RangeBase;
            range = range * RangeBase;
        }

        private void encodeChar(ulong start, ulong size, ulong total, char ch)
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
            while (low / FirstDigitScalar == (low + range) / FirstDigitScalar)
                emitDigit();

            // readjust range - see reason for this below
            if (range < FirstDigitScalar / RangeBase)
            {
                emitDigit();
                emitDigit();
                range = MaxRange - low;

                Console.WriteLine("Range: {0}-{1}", low, low + range);
            }
        }
    }
}