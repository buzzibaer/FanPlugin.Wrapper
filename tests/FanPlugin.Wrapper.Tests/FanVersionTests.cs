using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FanPlugin.Wrapper.Tests
{
    [TestClass]
    public class FanVersionTests
    {
        [TestMethod]
        public void AssemblyVersion_Is0400()
        {
            Assert.AreEqual("0.4.0.0", typeof(Fan).Assembly.GetName().Version.ToString());
        }
    }
}
