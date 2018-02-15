using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SS;

namespace SpreadsheetTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            Spreadsheet spreadsheet = new Spreadsheet();

            foreach (var VARIABLE in spreadsheet.GetNamesOfAllNonemptyCells())
            {
                Assert.AreEqual(VARIABLE, "");
            }
        }
    }
}