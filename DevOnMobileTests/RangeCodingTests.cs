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
            Assert.AreEqual("251", output); // According to Wikipedia this is the correct output
        }

        [TestMethod, Timeout(100)]
        public void TestEncodeWithShortText2()
        {
            const string input = "ABAA";
            var coder = new RangeCoding();
            string output = coder.encode(input);
            Assert.AreEqual("395", output); // We do not know if this is the correct output
        }

        [TestMethod, Timeout(100)]
        public void TestEncodeWithShortText3()
        {
            const string input = "AAAA";
            var coder = new RangeCoding();
            string output = coder.encode(input);
            Assert.AreEqual("Not 11", output); // TODO: The correct output is probably not "11" since symbol probability is not yet measured
        }

        [TestMethod, Timeout(100)]
        public void TestDencodeWithShortText()
        {
            const string input = "251";
            var coder = new RangeCoding();
            string output = coder.decode(input); // TODO
            Assert.AreEqual("AABA", output); // According to Wikipedia this is the correct output
        }
    }
}