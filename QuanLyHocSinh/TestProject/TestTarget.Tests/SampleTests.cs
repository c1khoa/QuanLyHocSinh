using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestTarget.Tests
{
    [TestClass]
    public class SampleTests
    {
        [TestMethod]
        public void SimpleTest()
        {
            Assert.AreEqual(1, 1);
        }
    }
}