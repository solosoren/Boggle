using System;
using Formulas;
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

        /// <summary>
        /// Tests SetVellValue
        /// </summary>
        [ExpectedException(typeof(FormulaFormatException))]
        [TestMethod]
        public void TestSetContent()
        {
            SpreadsheetViewStub stub = new SpreadsheetViewStub();
            Controller controller = new Controller(stub);
            stub.TestSetContentEvent(1, 1, "=a1");
        }
    }
}
