using System;

namespace DevOnMobile
{
    public class RangeDecoder
    {
        private string input;
        private string output;
        private int code = 0;
        private int low = 0;
        private int range = 1;

        // TODO: need to pass in symbol probability table rather than hardcoding it
        public string decode(string data)
        {
            input = data;

            //InitializeDecoder();

            int start = 0;
            int size;
            int total = 10;
            AppendDigit();                    // need to get range/total >0
            while (start < 8)                 // stop when receive EOM
            {
                int v = GetValue(total);  // code is in what symbol range?
                switch (v)                // convert value to symbol
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5: start=0; size=6; Console.Write("A"); output += 'A'; break;
                    case 6:
                    case 7: start=6; size=2; Console.Write("B"); output += 'B'; break;
                    default: start=8; size=2; Console.WriteLine(""); break;
                }
                Decode(start, size, total);
            }

            return output;
        }

        private int GetValue(int total)
        {
            return (code - low) / (range / total);
        }

        //private void InitializeDecoder()
        //{
        //    AppendDigit();  // with this example code, only 1 of these is actually needed
        //    //AppendDigit();
        //    //AppendDigit();
        //    //AppendDigit();
        //    //AppendDigit();
        //}

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
            code = (code % 10000) * 10 + ReadNextDigit();
            low = (low % 10000) * 10;
            range *= 10;
        }

        private void Decode(int start, int size, int total)  // Decode is same as Encode with EmitDigit replaced by AppendDigit
        {
            // adjust the range based on the symbol interval
            range /= total;
            low += start * range;
            range *= size;

            // check if left-most digit is same throughout range
            while (low / 10000 == (low + range) / 10000)
                AppendDigit();

            // readjust range - see reason for this below
            if (range < 1000)
            {
                AppendDigit();
                AppendDigit();
                range = 100000 - low;
            }
        }
    }
}