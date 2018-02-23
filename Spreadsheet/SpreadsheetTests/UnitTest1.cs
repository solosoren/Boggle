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
                Assert.IsTrue(changed.Equals("A1") || changed.Equals("B1") || changed.Equals("C1") || changed.Equals("D1"));
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

        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadException))]
        public void TestRegexConstructor3()
        {
            TextReader source = new StreamReader("Test1.xml");
            AbstractSpreadsheet spreadsheet = new Spreadsheet(source, new Regex("[A-B][1-3]"));
            Assert.IsFalse(spreadsheet.Changed);
        }

        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadException))]
        public void TestRegexConstructor4()
        {
            TextReader source = new StreamReader("Test2.xml");
            AbstractSpreadsheet spreadsheet = new Spreadsheet(source, new Regex("[A-B][1-3]"));
            Assert.IsFalse(spreadsheet.Changed);
        }

        [TestMethod]
        [ExpectedException(typeof(SpreadsheetVersionException))]
        public void TestRegexConstructor5()
        {
            TextReader source = new StreamReader("SampleSavedSpreadsheet.xml");
            AbstractSpreadsheet spreadsheet = new Spreadsheet(source, new Regex("[A][1-3]"));
            Assert.IsFalse(spreadsheet.Changed);
        }

        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadException))]
        public void TestRegexConstructor6()
        {
            TextReader source = new StreamReader("Test3.xml");
            AbstractSpreadsheet spreadsheet = new Spreadsheet(source, new Regex("[A-B][1-3]"));
            Assert.IsFalse(spreadsheet.Changed);
        }

        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadException))]
        public void TestRegexConstructor7()
        {
            TextReader source = new StreamReader("Test4.xml");
            AbstractSpreadsheet spreadsheet = new Spreadsheet(source, new Regex("[A-B][1-3]"));
            Assert.IsFalse(spreadsheet.Changed);
        }

        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadException))]
        public void TestRegexConstructor8()
        {
            TextReader source = new StreamReader("Test5.xml");
            AbstractSpreadsheet spreadsheet = new Spreadsheet(source, new Regex("[A-B][1-3]"));
            Assert.IsFalse(spreadsheet.Changed);
        }

        // The following tests are to test GetCellValue
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestGetCellValue1()
        {
            AbstractSpreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.SetContentsOfCell("A1", "1");
            spreadsheet.GetCellValue(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestGetCellValue2()
        {
            AbstractSpreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.SetContentsOfCell("A1", "1");
            spreadsheet.GetCellValue("A01");
        }

        [TestMethod]
        public void TestGetCellValue3()
        {
            AbstractSpreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.SetContentsOfCell("A1", "1");
            spreadsheet.SetContentsOfCell("B1", "Test");
            Assert.IsTrue(spreadsheet.GetCellValue("A1").Equals((double) 1));
            Assert.IsTrue(spreadsheet.GetCellValue("B1").Equals("Test"));
        }

        [TestMethod]
        public void TestGetCellValue4()
        {
            AbstractSpreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.SetContentsOfCell("A1", "1");
            spreadsheet.SetContentsOfCell("B1", "=A1 * 2");
            Assert.IsTrue(spreadsheet.GetCellValue("A1").Equals((double) 1));
            Assert.IsTrue(spreadsheet.GetCellValue("B1").Equals((double) 2));
        }

        [TestMethod]
        public void TestGetCellValue5()
        {
            AbstractSpreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.SetContentsOfCell("A1", "1");
            spreadsheet.SetContentsOfCell("B1", "=A2");
            Assert.IsTrue(spreadsheet.GetCellValue("A1").Equals((double) 1));
            Assert.IsTrue(spreadsheet.GetCellValue("B1") is FormulaError);
        }

        /// Used to make assertions about set equality.  Everything is converted first to
        /// upper case.
        /// </summary>
        public static void AssertSetEqualsIgnoreCase(IEnumerable<string> s1, IEnumerable<string> s2)
        {
            var set1 = new HashSet<String>();
            foreach (string s in s1)
            {
                set1.Add(s.ToUpper());
            }

            var set2 = new HashSet<String>();
            foreach (string s in s2)
            {
                set2.Add(s.ToUpper());
            }

            Assert.IsTrue(new HashSet<string>(set1).SetEquals(set2));
        }

        // EMPTY SPREADSHEETS
        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void Test1()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.GetCellContents(null);
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void Test2()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.GetCellContents("AA");
        }

        [TestMethod()]
        public void Test3()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            Assert.AreEqual("", s.GetCellContents("A2"));
        }

        // SETTING CELL TO A DOUBLE
        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void Test4()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell(null, "1.5");
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void Test5()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1A", "1.5");
        }

        [TestMethod()]
        public void Test6()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("Z7", "1.5");
            Assert.AreEqual(1.5, (double) s.GetCellContents("Z7"), 1e-9);
        }

        // SETTING CELL TO A STRING
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Test7()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A8", (string) null);
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void Test8()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell(null, "hello");
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void Test9()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("AZ", "hello");
        }

        [TestMethod()]
        public void Test10()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("Z7", "hello");
            Assert.AreEqual("hello", s.GetCellContents("Z7"));
        }

        // SETTING CELL TO A FORMULA
        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void Test11()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell(null, "=2");
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void Test12()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("AZ", "=2");
        }

        [TestMethod()]
        public void Test13()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("Z7", "=3");
            Formula f = (Formula) s.GetCellContents("Z7");
            Assert.AreEqual(3, f.Evaluate(x => 0), 1e-6);
        }

        // CIRCULAR FORMULA DETECTION
        [TestMethod()]
        [ExpectedException(typeof(CircularException))]
        public void Test14()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", ("=A2"));
            s.SetContentsOfCell("A2", ("=A1"));
        }

        [TestMethod()]
        [ExpectedException(typeof(CircularException))]
        public void Test15()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", ("=A2+A3"));
            s.SetContentsOfCell("A3", ("=A4+A5"));
            s.SetContentsOfCell("A5", ("=A6+A7"));
            s.SetContentsOfCell("A7", ("=A1+A1"));
        }

        [TestMethod()]
        [ExpectedException(typeof(CircularException))]
        public void Test16()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=A2+A3");
            s.SetContentsOfCell("A2", "15");
            s.SetContentsOfCell("A3", "30");
            s.SetContentsOfCell("A2", "=A3*A1");
        }

        // NONEMPTY CELLS
        [TestMethod()]
        public void Test17()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            Assert.IsFalse(s.GetNamesOfAllNonemptyCells().GetEnumerator().MoveNext());
        }

        [TestMethod()]
        public void Test18()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("B1", "");
            Assert.IsFalse(s.GetNamesOfAllNonemptyCells().GetEnumerator().MoveNext());
        }

        [TestMethod()]
        public void Test19()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("C2", "hello");
            s.SetContentsOfCell("C2", "");
            Assert.IsFalse(s.GetNamesOfAllNonemptyCells().GetEnumerator().MoveNext());
        }

        [TestMethod()]
        public void Test20()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("B1", "hello");
            AssertSetEqualsIgnoreCase(s.GetNamesOfAllNonemptyCells(), new string[] {"B1"});
        }

        [TestMethod()]
        public void Test21()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("B1", "52.25");
            AssertSetEqualsIgnoreCase(s.GetNamesOfAllNonemptyCells(), new string[] {"B1"});
        }

        [TestMethod()]
        public void Test22()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("B1", "=3.5");
            AssertSetEqualsIgnoreCase(s.GetNamesOfAllNonemptyCells(), new string[] {"B1"});
        }

        [TestMethod()]
        public void Test23()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "17.2");
            s.SetContentsOfCell("C1", "hello");
            s.SetContentsOfCell("B1", ("=3.5"));
            AssertSetEqualsIgnoreCase(s.GetNamesOfAllNonemptyCells(), new string[] {"A1", "B1", "C1"});
        }

        // RETURN VALUE OF SET CELL CONTENTS
        [TestMethod()]
        public void Test24()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("B1", "hello");
            s.SetContentsOfCell("C1", ("=5"));
            AssertSetEqualsIgnoreCase(s.SetContentsOfCell("A1", "17.2"), new string[] {"A1"});
        }

        [TestMethod()]
        public void Test25()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "17.2");
            s.SetContentsOfCell("C1", "=5");
            foreach (var VARIABLE in s.SetContentsOfCell("B1", "hello"))
            {
                Assert.IsTrue(VARIABLE.Equals("B1"));
            }
        }

        [TestMethod()]
        public void Test26()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "17.2");
            s.SetContentsOfCell("B1", "hello");
            AssertSetEqualsIgnoreCase(s.SetContentsOfCell("C1", ("=5")), new string[] {"C1"});
        }

        [TestMethod()]
        public void Test27()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=A2+A3");
            s.SetContentsOfCell("A2", "6");
            s.SetContentsOfCell("A3", "=A2+A4");
            s.SetContentsOfCell("A4", "=A2+A5");
            HashSet<string> result = new HashSet<string>(s.SetContentsOfCell("A5", "82.5"));
            AssertSetEqualsIgnoreCase(result, new string[] {"A5", "A4", "A3", "A1"});
        }

        [TestMethod()]
        public void Test29()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=A2+A3");
            s.SetContentsOfCell("A1", "Hello");
            Assert.AreEqual("Hello", (string) s.GetCellContents("A1"));
        }

        [TestMethod()]
        public void Test30()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "Hello");
            s.SetContentsOfCell("A1", ("=23"));
            Assert.AreEqual(23, ((Formula) s.GetCellContents("A1")).Evaluate(x => 0));
        }

        // STRESS TESTS
        [TestMethod()]
        public void Test31()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", ("=B1+B2"));
            s.SetContentsOfCell("B1", ("=C1-C2"));
            s.SetContentsOfCell("B2", ("=C3*C4"));
            s.SetContentsOfCell("C1", ("=D1*D2"));
            s.SetContentsOfCell("C2", ("=D3*D4"));
            s.SetContentsOfCell("C3", ("=D5*D6"));
            s.SetContentsOfCell("C4", ("=D7*D8"));
            s.SetContentsOfCell("D1", ("=E1"));
            s.SetContentsOfCell("D2", ("=E1"));
            s.SetContentsOfCell("D3", ("=E1"));
            s.SetContentsOfCell("D4", ("=E1"));
            s.SetContentsOfCell("D5", ("=E1"));
            s.SetContentsOfCell("D6", ("=E1"));
            s.SetContentsOfCell("D7", ("=E1"));
            s.SetContentsOfCell("D8", ("=E1"));
            ISet<String> cells = s.SetContentsOfCell("E1", "0");
            AssertSetEqualsIgnoreCase(
                new HashSet<string>()
                {
                    "A1",
                    "B1",
                    "B2",
                    "C1",
                    "C2",
                    "C3",
                    "C4",
                    "D1",
                    "D2",
                    "D3",
                    "D4",
                    "D5",
                    "D6",
                    "D7",
                    "D8",
                    "E1"
                }, cells);
        }

        [TestMethod()]
        public void Test32()
        {
            Test31();
        }

        [TestMethod()]
        public void Test33()
        {
            Test31();
        }

        [TestMethod()]
        public void Test34()
        {
            Test31();
        }

        public void RunRandomizedTest(int seed, int size)
        {
            AbstractSpreadsheet s = new Spreadsheet();
            Random rand = new Random(seed);
            for (int i = 0; i < 10000; i++)
            {
                try
                {
                    switch (rand.Next(3))
                    {
                        case 0:
                            s.SetContentsOfCell(randomName(rand), "3.14");
                            break;
                        case 1:
                            s.SetContentsOfCell(randomName(rand), "hello");
                            break;
                        case 2:
                            s.SetContentsOfCell(randomName(rand), "=" + randomFormula(rand));
                            break;
                    }
                }
                catch (CircularException)
                {
                }
            }

            ISet<string> set = new HashSet<string>(s.GetNamesOfAllNonemptyCells());
            Assert.AreEqual(size, set.Count);
        }

        private String randomName(Random rand)
        {
            return "ABCDEFGHIJKLMNOPQRSTUVWXYZ".Substring(rand.Next(26), 1) + (rand.Next(99) + 1);
        }

        private String randomFormula(Random rand)
        {
            String f = randomName(rand);
            for (int i = 0; i < 10; i++)
            {
                switch (rand.Next(4))
                {
                    case 0:
                        f += "+";
                        break;
                    case 1:
                        f += "-";
                        break;
                    case 2:
                        f += "*";
                        break;
                    case 3:
                        f += "/";
                        break;
                }

                switch (rand.Next(2))
                {
                    case 0:
                        f += 7.2;
                        break;
                    case 1:
                        f += randomName(rand);
                        break;
                }
            }

            return f;
        }

        // The following tests are to test the Save method.
        [TestMethod]
        public void TestSave1()
        {
            TextWriter textWriter = new StreamWriter("TestSave1.txt");
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "Hello");
            s.SetContentsOfCell("B1", "Bye");
            s.Save(textWriter);
        }
    }
}