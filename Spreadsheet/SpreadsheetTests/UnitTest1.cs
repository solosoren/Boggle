using System.Collections.Generic;
using Formulas;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SS;

namespace SpreadsheetTests
{
    [TestClass]
    public class UnitTest1
    {
        /// <summary>
        /// Testing iterating over an empty spreadsheet
        /// </summary>
        [TestMethod]
        public void TestEmptySpreadsheet()
        {
            AbstractSpreadsheet spreadsheet = new Spreadsheet();
            foreach (string cell in spreadsheet.GetNamesOfAllNonemptyCells())
            {
                Assert.Fail();
            }

            Assert.IsTrue(true);
        }

        /// <summary>
        /// Testing setting cell contents to non empty and iterating over non empty cell names
        /// </summary>
        [TestMethod]
        public void TestNonEmptySpreadsheet()
        {
            AbstractSpreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.SetCellContents("A1", 1);
            spreadsheet.SetCellContents("B1", 1);
            foreach (string cell in spreadsheet.GetNamesOfAllNonemptyCells())
            {
                Assert.IsTrue(cell.Equals("A1") || cell.Equals("B1"));
            }
        }

        /// <summary>
        /// Testing to see if excpetion is thrown when trying to get cell contents from an
        /// invalid cell name.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestGetNonValidCellContent()
        {
            AbstractSpreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.GetCellContents("3A");
        }

        /// <summary>
        /// Another test to test if excpetion is thrown for invalid cell name.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestGetNonValidCellContent1()
        {
            AbstractSpreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.GetCellContents("Hello");
        }

        /// <summary>
        /// Another test to test if excpetion is thrown for invalid cell name.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestGetNonValidCellContent2()
        {
            AbstractSpreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.GetCellContents("A0");
        }

        /// <summary>
        /// Testing to see if a valid cell returns an empty string for content.
        /// </summary>
        [TestMethod]
        public void TestGetValidEmptyCellContent()
        {
            AbstractSpreadsheet spreadsheet = new Spreadsheet();
            Assert.IsTrue(spreadsheet.GetCellContents("A3").Equals(""));
        }

        /// <summary>
        /// Testing to see if a valid non empty cell returns the correct content.
        /// </summary>
        [TestMethod]
        public void TestGetValidNonEmptyCellContentDouble()
        {
            AbstractSpreadsheet spreadsheet = new Spreadsheet();
            double num = 1.0;
            spreadsheet.SetCellContents("A12", num);
            Assert.AreEqual(spreadsheet.GetCellContents("A12"), num);
        }

        /// <summary>
        /// Testing to see if exception is thrown when set contents of an invalid
        /// cell to a double
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestInvalidCellSetContentDouble()
        {
            AbstractSpreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.SetCellContents("A0", 1.0);
        }

        /// <summary>
        /// Testing to see if the dependents are returned too when content is changed.
        /// </summary>
        [TestMethod]
        public void TestDependentsCellSetContentFormula()
        {
            AbstractSpreadsheet spreadsheet = new Spreadsheet();
            Formula formula = new Formula("A1 + 2.0");
            spreadsheet.SetCellContents("A1", 1.2);
            spreadsheet.SetCellContents("A2", formula);

            ISet<string> changed = spreadsheet.SetCellContents("A1", 2);
            Assert.AreEqual(spreadsheet.GetCellContents("A2"), formula);
            Assert.IsTrue(changed.Contains("A1"));
            Assert.IsTrue(changed.Contains("A2"));
        }

        /// <summary>
        /// Testing to see if circular exception is thrown for self dependency.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        [ExpectedException(typeof(CircularException))]
        public void TestSelfDependency()
        {
            AbstractSpreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.SetCellContents("A1", 1.2);
            spreadsheet.SetCellContents("A4", new Formula("A1 + 2.3"));
            spreadsheet.SetCellContents("A1", new Formula("A1 + 2"));
        }

        /// <summary>
        /// Testing to see if circular exception is thrown for circular dependency.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        [ExpectedException(typeof(CircularException))]
        public void TestCircularDependency()
        {
            AbstractSpreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.SetCellContents("A1", 1.2);
            spreadsheet.SetCellContents("A4", new Formula("A1 + 2"));
            Formula formula = new Formula("A4");
            spreadsheet.SetCellContents("B1", formula);
            spreadsheet.SetCellContents("A1", new Formula("B1"));
        }


        /// <summary>
        /// Testing to see that no exception is thrown for removing variables from
        /// a formula.
        /// </summary>
        [TestMethod]
        public void TestAddMultipleDependencyFormula()
        {
            AbstractSpreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.SetCellContents("A1", 1);
            spreadsheet.SetCellContents("B1", new Formula("A1 + 1"));
            spreadsheet.SetCellContents("C1", new Formula("A1 + B1"));
            spreadsheet.SetCellContents("C1", new Formula("B1 + 2"));
        }
    }
}