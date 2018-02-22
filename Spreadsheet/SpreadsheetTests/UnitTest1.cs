using System;
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
            spreadsheet.SetContentsOfCell("A1", "1");
            spreadsheet.SetContentsOfCell("B1", "1");
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
            spreadsheet.SetContentsOfCell("A12", "1.0");
            Assert.AreEqual(spreadsheet.GetCellContents("A12"), 1.0);
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
            spreadsheet.SetContentsOfCell("A0", "1.0");
        }

        /// <summary>
        /// Testing to see if the dependents are returned too when content is changed.
        /// </summary>
        [TestMethod]
        public void TestDependentsCellSetContentFormula()
        {
            AbstractSpreadsheet spreadsheet = new Spreadsheet();
            string formula = "=A1+2.0";
            Formula expectedFormula = new Formula("A1+2.0");
            spreadsheet.SetContentsOfCell("A1", "1.2");
            spreadsheet.SetContentsOfCell("A2", formula);

            ISet<string> changed = spreadsheet.SetContentsOfCell("A1", "2");
            Assert.AreEqual(expectedFormula.ToString(), spreadsheet.GetCellContents("A2").ToString());
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
            spreadsheet.SetContentsOfCell("A1", "1.2");
            spreadsheet.SetContentsOfCell("A4", "=A1 + 2.3");
            spreadsheet.SetContentsOfCell("A1", "=A1 + 2");
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
            spreadsheet.SetContentsOfCell("A1", "1.2");
            spreadsheet.SetContentsOfCell("A4", "=A1 + 2");
            spreadsheet.SetContentsOfCell("B1", "=A4");
            spreadsheet.SetContentsOfCell("A1", "=B1");
        }


        /// <summary>
        /// Testing to see that no exception is thrown for removing variables from
        /// a formula.
        /// </summary>
        [TestMethod]
        public void TestAddMultipleDependencyFormula()
        {
            AbstractSpreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.SetContentsOfCell("A1", "1.1");
            string formulaB = "=A1+1";
            Formula expectedFormulaB = new Formula("A1+1");
            string formulaC = "=B1+2";
            Formula expectedFormulaC = new Formula("B1+2");
            spreadsheet.SetContentsOfCell("B1", formulaB);
            spreadsheet.SetContentsOfCell("C1", "=A1+B1");
            spreadsheet.SetContentsOfCell("C1", formulaC);

            Assert.AreEqual(spreadsheet.GetCellContents("A1"), 1.1);
            Assert.AreEqual(spreadsheet.GetCellContents("B1").ToString(), expectedFormulaB.ToString());
            Assert.AreEqual(spreadsheet.GetCellContents("C1").ToString(), expectedFormulaC.ToString());
        }

        // The following methods test SetContent for strings
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestSetContentString()
        {
            AbstractSpreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.GetCellContents("A0");
        }

        [TestMethod]
        public void TestSetContentString1()
        {
            AbstractSpreadsheet spreadsheet = new Spreadsheet();
            Assert.AreEqual(spreadsheet.GetCellContents("A1"), "");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestSetContentString2()
        {
            AbstractSpreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.SetContentsOfCell("A1", null);
        }

        [TestMethod]
        public void TestSetContentString3()
        {
            AbstractSpreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.SetContentsOfCell("A1", "1.0");
            spreadsheet.SetContentsOfCell("B1", "=A1+1");
            spreadsheet.SetContentsOfCell("C1", "=A1");
            spreadsheet.SetContentsOfCell("D1", "=C1");
            foreach (string changed in spreadsheet.SetContentsOfCell("A1", "Test"))
            {
                Assert.IsTrue(changed.Equals("A1") || changed.Equals("B1") || changed.Equals("C1"));
            }
        }
    }
}