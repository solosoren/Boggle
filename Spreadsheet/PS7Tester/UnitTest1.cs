using System;
using Formulas;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpreadsheetGUI;
using SS;

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
            stub.TestClose();
            Assert.IsTrue(stub.CalledCloseWindow);
        }

        /// <summary>
        /// Tests close window functionality
        /// </summary>
        [TestMethod]
        public void TestCloseDisplay()
        {
            SpreadsheetViewStub stub = new SpreadsheetViewStub();
            Controller controller = new Controller(stub);
            stub.TestSetContentEvent(0, 1, "45");
            stub.TestClose();
            Assert.IsTrue(stub.CalledCloseWindow);
        }

        /// <summary>
        /// Tests Set Content
        /// </summary>
        [TestMethod]
        public void TestSetContent1()
        {
            SpreadsheetViewStub stub = new SpreadsheetViewStub();
            Controller controller = new Controller(stub);
            stub.TestSetContentEvent(0, 1, "45");
            Assert.AreEqual("45", stub.setCellValue);
            Assert.AreEqual(0, stub.setColumn);
            Assert.AreEqual(1, stub.setRow);
        }

        /// <summary>
        /// Tests Set Content
        /// </summary>
        [TestMethod]
        public void TestSetContent2()
        {
            SpreadsheetViewStub stub = new SpreadsheetViewStub();
            Controller controller = new Controller(stub);
            stub.TestSetContentEvent(0, 1, "45");
            stub.TestSetContentEvent(0, 0, "=A2");

            Assert.AreEqual("45", stub.setCellValue);
            Assert.AreEqual(0, stub.setColumn);
            Assert.AreEqual(0, stub.setRow);
        }

        /// <summary>
        /// Tests Set Content
        /// </summary>
        [TestMethod]
        public void TestSetContent3()
        {
            SpreadsheetViewStub stub = new SpreadsheetViewStub();
            Controller controller = new Controller(stub);
            stub.TestSetContentEvent(1, 0, "=A2");
            Assert.IsTrue(stub.displayMessage.Contains("Formula error"));
        }

        /// <summary>
        /// Tests Circular Exception
        /// </summary>
        [TestMethod]
        public void TestSetContentCircular()
        {
            SpreadsheetViewStub stub = new SpreadsheetViewStub();
            Controller controller = new Controller(stub);
            stub.TestSetContentEvent(0, 0, "=a1");
            Assert.AreEqual("Circular exception", stub.displayMessage);
        }

        /// <summary>
        /// Tests SpreadsheetHelp
        /// </summary>
        [TestMethod]
        public void TestSpreadsheetHelp()
        {
            SpreadsheetViewStub stub = new SpreadsheetViewStub();
            Controller controller = new Controller(stub);
            stub.TestHandleSpreadsheetHelp();
        }

        /// <summary>
        /// Tests Handle New
        /// </summary>
        [TestMethod]
        public void TestNew()
        {
            SpreadsheetViewStub stub = new SpreadsheetViewStub();
            Controller controller = new Controller(stub);
            stub.TestNew();
        }

        /// <summary>
        /// Tests File Help
        /// </summary>
        [TestMethod]
        public void TestFileHelp()
        {
            SpreadsheetViewStub stub = new SpreadsheetViewStub();
            Controller controller = new Controller(stub);
            stub.TestHandleFileHelp();
        }

        /// <summary>
        /// Tests Did Change
        /// </summary>
        [TestMethod]
        public void TestChange()
        {
            SpreadsheetViewStub stub = new SpreadsheetViewStub();
            Controller controller = new Controller(stub);
            stub.TestSetContentEvent(0, 1, "45");
            stub.TestDidChange();
        }

        /// <summary>
        /// Tests Did Change
        /// </summary>
        [TestMethod]
        public void TestHandleSave()
        {
            SpreadsheetViewStub stub = new SpreadsheetViewStub();
            Controller controller = new Controller(stub);
            stub.TestSave();
        }

        /// <summary>
        /// Tests Did Change
        /// </summary>
        [TestMethod]
        public void TestHandleOpen()
        {
            SpreadsheetViewStub stub = new SpreadsheetViewStub();
            Controller controller = new Controller(stub);
            stub.TestOpen();
        }


    }
}
