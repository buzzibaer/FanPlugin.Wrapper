using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FanPlugin.Wrapper.Tests
{
    [TestClass]
    public class FanVideoIdTests
    {
        [DataTestMethod]
        [DataRow("0", 0)]
        [DataRow("9", 9)]
        [DataRow("10", 10)]
        [DataRow("99", 99)]
        public void TryParse_AcceptsTwoDigitIds(string value, int expected)
        {
            int actual;

            Assert.IsTrue(FanVideoId.TryParse(value, out actual));
            Assert.AreEqual(expected, actual);
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("abc")]
        [DataRow("-1")]
        [DataRow("100")]
        [DataRow("1.5")]
        public void TryParse_RejectsInvalidOrOutOfRangeIds(string value)
        {
            int actual;

            Assert.IsFalse(FanVideoId.TryParse(value, out actual));
        }
    }
}
