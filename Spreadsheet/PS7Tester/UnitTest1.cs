using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpreadsheetGUI;

namespace PS7Tester
{
    [TestClass]
    public class UnitTest1
    {

        /// <summary>
        /// Tests close window functionality
        /// </summary>
        [TestMethod]
        public void TestCloseWindow()
        {
            SpreadsheetViewStub stub = new SpreadsheetViewStub();
            Controller controller = new Controller(stub);
            stub.CloseWindow();
            Assert.IsTrue(stub.CalledCloseWindow);
        }
    }
}
