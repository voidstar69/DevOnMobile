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
            Assert.AreEqual("623", output);
        }

        [TestMethod, Timeout(100)]
        public void TestEncodeWithShortText2()
        {
            const string input = "ABAA";
            var coder = new RangeCoding();
            string output = coder.encode(input);
            Assert.AreEqual("719", output); // We do not know if this is the correct output
        }

        [TestMethod, Timeout(100)]
        public void TestEncodeWithShortText3()
        {
            const string input = "AAAA";
            var coder = new RangeCoding();
            string output = coder.encode(input);
            Assert.AreEqual("510", output); // We do not know if this is the correct output
        }

        [TestMethod]//, Timeout(100)]
        public void TestDecodeWithShortText()
        {
            const string input = "251";

            var table = new RangeCodingTable {TotalRange = 5};
            table.CharStart['A'] = 0;
            table.CharSize['A'] = 3;
            table.CharStart['B'] = 3;
            table.CharSize['B'] = 1;
            table.CharStart['\0'] = 4;
            table.CharSize['\0'] = 1;

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
            Assert.AreEqual("623", encoded);

            string decoded = coder.decode(encoded);
            Assert.AreEqual("AABA", decoded);
        }

        [TestMethod]//, Timeout(1000)]
        public void TestEncodeThenDecodeWithShortText2()
        {
            const string input = "fghjgfjfghjfghj";
            var coder = new RangeCoding();
            string encoded = coder.encode(input);

            string decoded = coder.decode(encoded);
            Assert.AreEqual(input, decoded);
        }

        [TestMethod]//, Timeout(100)]
        public void TestEncodeThenDecodeWithLongText()
        {
            const string input = "ABAfdghdfjh356756utyjfgr t56uy 56u567856 5jhthne t784567hfdhgdfgJFGHJETJE%^UE%YJDGJ^&IR^&KC";
            var coder = new RangeCoding();
            string encoded = coder.encode(input);

            string decoded = coder.decode(encoded);
            Assert.AreEqual(input, decoded);
        }
    }
}