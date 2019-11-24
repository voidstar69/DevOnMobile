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

        [TestMethod, Timeout(100)]
        public void TestDecodeWithShortText()
        {
            const string input = "251";
            var coder = new RangeCoding();
            string output = coder.decode(input);
            Assert.AreEqual("AABA", output); // According to Wikipedia this is the correct output
        }
    }
}