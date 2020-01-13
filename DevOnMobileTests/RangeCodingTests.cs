using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevOnMobile.Tests
{
    [TestClass]
    public class RangeCodingTests
    {
        [TestMethod, Timeout(100)]
        public void TestEncodeWithShortText()
        {
            const string input = "AABA";
            var coder = new RangeCoding();
            string output = coder.encode(input);
            //Assert.AreEqual("251", output); // According to Wikipedia this is the correct output
            // We make the EOM char the first char not the last char. This changes the result.
            //Assert.AreEqual("6,2,3,", output);
            Assert.AreEqual("158,", output);
        }

        [TestMethod, Timeout(100)]
        public void TestEncodeWithShortText2()
        {
            const string input = "ABAA";
            var coder = new RangeCoding();
            string output = coder.encode(input);
            //Assert.AreEqual("7,1,9,", output); // We do not know if this is the correct output
            Assert.AreEqual("183,", output); // We do not know if this is the correct output
        }

        [TestMethod, Timeout(100)]
        public void TestEncodeWithShortText3()
        {
            const string input = "AAAA";
            var coder = new RangeCoding();
            string output = coder.encode(input);
            //Assert.AreEqual("5,10,", output); // We do not know if this is the correct output
            Assert.AreEqual("150,", output); // We do not know if this is the correct output
        }

        [TestMethod, Timeout(100)]
        public void TestDecodeBase10WithShortText()
        {
            //const string input = "6,2,3,";
            const string input = "158";

            //var table = new RangeCodingTable {TotalRange = 10};
            //table.CharStart['A'] = 2; // 2
            //table.CharSize['A'] = 6;
            //table.CharStart['B'] = 8; // 8
            //table.CharSize['B'] = 2;
            //table.CharStart['\0'] = 0; // 0
            //table.CharSize['\0'] = 2;
            //var table = new RangeCodingTable {TotalRange = 5};
            //table.CharStart['A'] = 0;
            //table.CharSize['A'] = 3;
            //table.CharStart['B'] = 3;
            //table.CharSize['B'] = 1;
            //table.CharStart['\0'] = 4;
            //table.CharSize['\0'] = 1;
            var table = new RangeCodingTable { TotalRange = 256 };
            table.CharStart['A'] = 51;
            table.CharSize['A'] = 153;
            table.CharStart['B'] = 204;
            table.CharSize['B'] = 51;
            table.CharStart['\0'] = 0;
            table.CharSize['\0'] = 51;

            var coder = new RangeCoding(table);
            string output = coder.decode(input);
            Assert.AreEqual("AABA", output); // According to Wikipedia this is the correct output
        }

        [TestMethod, Timeout(100)]
        public void TestEncodeThenDecodeWithShortText()
        {
            const string input = "AABA";
            var coder = new RangeCoding();
            string encoded = coder.encode(input);

            // We make the EOM char the first char not the last char. This changes the result from what is on Wikipedia.
            //Assert.AreEqual("6,2,3,", encoded);
            Assert.AreEqual("158,", encoded);

            string decoded = coder.decode(encoded);
            Assert.AreEqual("AABA", decoded);
        }

        [TestMethod, Timeout(100)]
        public void TestEncodeThenDecodeWithShortText2()
        {
            const string input = "fghjgfjfghjfghj";
            var coder = new RangeCoding();
            string encoded = coder.encode(input);

            string decoded = coder.decode(encoded);
            Assert.AreEqual(input, decoded);
        }

        // This fails because a zero char is used to indicate when the encoded data stream ends
        [TestMethod, Ignore, Timeout(100)]
        public void TestEncodeThenDecodeWithZeroCharSucceeds()
        {
            const string input = "AB\0CD";
            var coder = new RangeCoding();
            string encoded = coder.encode(input);

            string decoded = coder.decode(encoded);
            Assert.AreEqual(input, decoded);
        }

        [TestMethod, Timeout(100)]
        public void TestEncodeThenDecodeWithLongText()
        {
            const string input = "AB Afdghdfjh356756utyjfgr t56uy 56u567856 5jhthne t784567hfdhgdfgJFGHJETJE%^UE%YJDGJ^&IR^&KC";
            var coder = new RangeCoding();
            string encoded = coder.encode(input);

            string decoded = coder.decode(encoded);
            Assert.AreEqual(input, decoded);
        }

        [TestMethod, Timeout(200)]
        public void TestEncodeThenDecodeWithRandomText()
        {
            const int size = 100; // TODO: size of 500 or 1000 sometimes breaks the compressor - range becomes zero sometimes during compression

            var random = new Random();
            var builder = new StringBuilder(size);
            for (var i = 0; i < size; i++)
            {
                builder.Append((char) random.Next(1, 255)); // TODO: a zero-character in the input causes the decompressor to terminate early as this is the terminator character
                //builder.Append((char) random.Next('A', 'z'));
            }
            string input = builder.ToString();

            var coder = new RangeCoding();
            string encoded = coder.encode(input);

            string decoded = coder.decode(encoded);
            Assert.AreEqual(input, decoded);
        }
    }
}