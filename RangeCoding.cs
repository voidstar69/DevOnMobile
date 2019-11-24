using System;

namespace DevOnMobile
{
    public class RangeCoding : ITextCodec
    {
        private const char EndOfMessageChar = '\0';
        private int low;
        private int range;
        private string output;

        public string encode(string data)
        {
            if(string.IsNullOrEmpty(data))
                return data;

            low = 0;
            range = 100000;

            //byte[] charProbability = new byte[256];
            //const int total = 10;
            var charStart = new int[256];
            var charSize = new int[256];

            // TODO: derive this data based on frequencies of characters in input
            int total = data.Length + 1; // count invented EOM char
            foreach (char ch in data)
            {
                charSize[ch]++;
            }
            charSize[EndOfMessageChar]++;

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

            //total = 5;
            //charStart['A'] = 0;
            //charSize['A'] = 3;
            //charStart['B'] = 3;
            //charSize['B'] = 1;
            //charStart['\0'] = 4;
            //charSize['\0'] = 1;

            //total = 10;
            //charStart['A'] = 0;
            //charSize['A'] = 6;
            //charStart['B'] = 6;
            //charSize['B'] = 2;
            //charStart['\0'] = 8;
            //charSize['\0'] = 2;

            output = string.Empty;

            for(int i = 0; i <= data.Length; i++)
            {
                char ch = (i == data.Length ? '\0' : data[i]);
                encodeChar(charStart[ch], charSize[ch], total);
            }

            // emit final digits - see below
            while (range < 10000)
                emitDigit();

            low += 10000;
            emitDigit();

            return output;
        }

        private int tooManyCallsGuard;

        private void emitDigit()
        {
            if (tooManyCallsGuard++ > 10)
            {
                throw new StackOverflowException("Oopsie Daisy!");
            }

            output += low / 10000;
            Console.Write(low / 10000);
            low = (low % 10000) * 10;
            range *= 10;
        }

        private void encodeChar(int start, int size, int total)
        {
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
            }
        }

        public string decode(string data)
        {
            if(string.IsNullOrEmpty(data))
                return data;

            return new RangeDecoder().decode(data);
        }
    }
}
