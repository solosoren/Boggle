using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
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

        // The following tests are to test the method SetContentsOfCell
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestSetContentsOfCell1()
        {
            AbstractSpreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.SetContentsOfCell("A1", null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestSetContentsOfCell2()
        {
            AbstractSpreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.SetContentsOfCell(null, "1");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestSetContentsOfCell3()
        {
            AbstractSpreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.SetContentsOfCell("B03", "1");
        }

        [TestMethod]
        public void TestSetContentsOfCell4()
        {
            AbstractSpreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.SetContentsOfCell("A1", "1");
            Assert.AreEqual((double) 1, spreadsheet.GetCellContents("A1"));
            Assert.IsTrue(spreadsheet.Changed);
        }

        [TestMethod]
        public void TestSetContentsOfCell4a()
        {
            AbstractSpreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.SetContentsOfCell("A1", "1");
            foreach (string changed in spreadsheet.SetContentsOfCell("A1", "1"))
            {
                Assert.IsTrue(changed.Equals("A1"));
            }

            Assert.AreEqual((double) 1, spreadsheet.GetCellContents("A1"));
            Assert.IsTrue(spreadsheet.Changed);
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestSetContentsOfCell5()
        {
            AbstractSpreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.SetContentsOfCell("A1", "=X");
        }

        [TestMethod]
        public void TestSetContentsOfCell6()
        {
            AbstractSpreadsheet spreadsheet = new Spreadsheet();
            foreach (string changed in spreadsheet.SetContentsOfCell("A1", "=B1"))
            {
                Assert.IsTrue(changed.Equals("A1"));
            }

            Formula formula = new Formula("B1");
            Assert.AreEqual(formula.ToString(), spreadsheet.GetCellContents("A1").ToString());
            Assert.IsTrue(spreadsheet.Changed);
        }

        [TestMethod]
        public void TestSetContentsOfCell7()
        {
            AbstractSpreadsheet spreadsheet = new Spreadsheet();
            foreach (string changed in spreadsheet.SetContentsOfCell("A1", "Test"))
            {
                Assert.IsTrue(changed.Equals("A1"));
            }

            Assert.IsTrue("Test".Equals(spreadsheet.GetCellContents("A1")));
            Assert.IsTrue(spreadsheet.Changed);
        }

        // The following tests are to test the spreadsheet constructors
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestRegexConstructor()
        {
            AbstractSpreadsheet spreadsheet = new Spreadsheet(new Regex("[A-C][1-2]"));
            spreadsheet.SetContentsOfCell("B4", "1.0");
        }

        [TestMethod]
        public void TestRegexConstructor1()
        {
            AbstractSpreadsheet spreadsheet = new Spreadsheet(new Regex("[A-C][1-2]"));
            Assert.IsFalse(spreadsheet.Changed);
            spreadsheet.SetContentsOfCell("A2", "1");
            spreadsheet.SetContentsOfCell("A1", "=A2");
            spreadsheet.SetContentsOfCell("C2", "2");
            Assert.IsTrue(spreadsheet.Changed);
        }

        [TestMethod]
        public void TestRegexConstructor2()
        {
            TextReader source = new StreamReader("SampleSavedSpreadsheet.xml");
            AbstractSpreadsheet spreadsheet = new Spreadsheet(source, new Regex("[A-B][1-3]"));
            Assert.IsFalse(spreadsheet.Changed);
        }
    }
}