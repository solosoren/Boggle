using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Dependencies;
using Formulas;

namespace SS
{
    public class Cell
    {
        public string name;
        private object content;
        public bool hasFormula;

        public Cell(string name)
        {
            this.name = name;
            this.content = "";
            hasFormula = false;
        }

        public void SetContent(object content)
        {
            if (content is Formula)
            {
                hasFormula = true;
            }

            this.content = content;
        }

        public object GetContent()
        {
            return content;
        }
    }

    public class Spreadsheet : AbstractSpreadsheet
    {
        // Cells will contain the values of each cell
        private Dictionary<string, Cell> Cells;

        // Graph will contain the dependencies of the cells
        private DependencyGraph Graph;

        private Regex IsValid;


        /// Creates an empty Spreadsheet whose IsValid regular expression accepts every string.
        public Spreadsheet()
        {
            Graph = new DependencyGraph();
            Cells = new Dictionary<string, Cell>();
            IsValid = new Regex(@".*");
        }

        /// Creates an empty Spreadsheet whose IsValid regular expression is provided as the parameter
        public Spreadsheet(Regex isValid)
        {
            Graph = new DependencyGraph();
            Cells = new Dictionary<string, Cell>();
            IsValid = isValid;
        }

        /// Creates a Spreadsheet that is a duplicate of the spreadsheet saved in source.
        ///
        /// See the AbstractSpreadsheet.Save method and Spreadsheet.xsd for the file format
        /// specification.
        ///
        /// If there's a problem reading source, throws an IOException.
        ///
        /// Else if the contents of source are not consistent with the schema in Spreadsheet.xsd,
        /// throws a SpreadsheetReadException.
        ///
        /// Else if the IsValid string contained in source is not a valid C# regular expression, throws
        /// a SpreadsheetReadException.  (If the exception is not thrown, this regex is referred to
        /// below as oldIsValid.)
        ///
        /// Else if there is a duplicate cell name in the source, throws a SpreadsheetReadException.
        /// (Two cell names are duplicates if they are identical after being converted to upper case.)
        ///
        /// Else if there is an invalid cell name or an invalid formula in the source, throws a
        /// SpreadsheetReadException.  (Use oldIsValid in place of IsValid in the definition of
        /// cell name validity.)
        ///
        /// Else if there is an invalid cell name or an invalid formula in the source, throws a
        /// SpreadsheetVersionException.  (Use newIsValid in place of IsValid in the definition of
        /// cell name validity.)
        ///
        /// Else if there's a formula that causes a circular dependency, throws a SpreadsheetReadException.
        ///
        /// Else, create a Spreadsheet that is a duplicate of the one encoded in source except that
        /// the new Spreadsheet's IsValid regular expression should be newIsValid.
        public Spreadsheet(TextReader source, Regex newIsValid)
        {
        }

        /// A string is a valid cell name if and only if (1) s consists of one or more letters,
        /// followed by a non-zero digit, followed by zero or more digits AND (2) the C#
        /// expression IsValid.IsMatch(s.ToUpper()) is true.
        ///
        /// For example, "A15", "a15", "XY32", and "BC7" are valid cell names, so long as they also
        /// are accepted by IsValid.  On the other hand, "Z", "X07", and "hello" are not valid cell
        /// names, regardless of IsValid.
        private bool IsValidCellName(string name)
        {
            if (name == null)
            {
                return false;
            }

            bool lastCharWasNum = false;

            for (int i = 0; i < name.Length; i++)
            {
                if (i == 0)
                {
                    if (Char.IsLetter(name[i]))
                    {
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if (Char.IsLetter(name[i]))
                    {
                        if (lastCharWasNum == false)
                        {
                        }
                        else
                        {
                            return false;
                        }
                    }

                    else if (Char.IsNumber(name[i]))
                    {
                        if (name[i].Equals('0'))
                        {
                            return false;
                        }


                        lastCharWasNum = true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            if (!lastCharWasNum)
            {
                return false;
            }

            return true;
        }

        public override bool Changed { get; protected set; }

        /// <summary>
        /// Writes the contents of this spreadsheet to dest using an XML format.
        /// The XML elements should be structured as follows:
        ///
        /// <spreadsheet IsValid="IsValid regex goes here">
        ///   <cell name="cell name goes here" contents="cell contents go here"></cell>
        ///   <cell name="cell name goes here" contents="cell contents go here"></cell>
        ///   <cell name="cell name goes here" contents="cell contents go here"></cell>
        /// </spreadsheet>
        ///
        /// The value of the IsValid attribute should be IsValid.ToString()
        ///
        /// There should be one cell element for each non-empty cell in the spreadsheet.
        /// If the cell contains a string, the string (without surrounding double quotes) should be written as the contents.
        /// If the cell contains a double d, d.ToString() should be written as the contents.
        /// If the cell contains a Formula f, f.ToString() with "=" prepended should be written as the contents.
        ///
        /// If there are any problems writing to dest, the method should throw an IOException.
        /// </summary>
        public override void Save(TextWriter dest)
        {
            try
            {
                dest.WriteLine("<spreadsheet IsValid=\"" + IsValid.ToString() + "\">");
                foreach (KeyValuePair<string, Cell> entry in Cells)
                {
                    if (entry.Value.hasFormula)
                    {
                        dest.WriteLine("\t<cell name=\"" + entry.Key + "\" contents=\"=" + entry.Value.ToString() +
                                       "\"></cell>");
                    }

                    else
                    {
                        dest.WriteLine("\t<cell name=\"" + entry.Key + "\" contents=\"" + entry.Value.ToString() +
                                       "\"></cell>");
                    }
                }
                dest.WriteLine("</spreadsheet>");
                dest.Flush();
            }
            catch (IOException e)
            {
                throw e;
            }
        }

        public override object GetCellValue(string name)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Enumerates the names of all the non-empty cells in the spreadsheet.
        /// </summary>
        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {
            foreach (var cell in Cells)
            {
                yield return cell.Key;
            }
        }

        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        ///
        /// Otherwise, returns the contents (as opposed to the value) of the named cell.  The return
        /// value should be either a string, a double, or a Formula.
        /// </summary>
        public override object GetCellContents(string name)
        {
            if (!IsValidCellName(name))
            {
                throw new InvalidNameException();
            }

            if (!Cells.ContainsKey(name))
            {
                Cells.Add(name, new Cell(name));
            }

            return Cells[name].GetContent();
        }

        public override ISet<string> SetContentsOfCell(string name, string content)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        ///
        /// Otherwise, the contents of the named cell becomes number.  The method returns a
        /// set consisting of name plus the names of all other cells whose value depends,
        /// directly or indirectly, on the named cell.
        ///
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        protected override ISet<string> SetCellContents(string name, double number)
        {
            if (!IsValidCellName(name))
            {
                throw new InvalidNameException();
            }

            if (!Cells.ContainsKey(name))
            {
                Cells.Add(name, new Cell(name));
            }

            Cells[name].SetContent(number);

            ISet<string> changedSet = new HashSet<string>();
            changedSet.Add(name);
            if (Graph.HasDependents(name))
            {
                foreach (string dependent in Graph.GetDependents(name))
                {
                    changedSet.Add(dependent);
                }
            }

            return changedSet;
        }

        /// <summary>
        /// If text is null, throws an ArgumentNullException.
        ///
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        ///
        /// Otherwise, the contents of the named cell becomes text.  The method returns a
        /// set consisting of name plus the names of all other cells whose value depends,
        /// directly or indirectly, on the named cell.
        ///
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        protected override ISet<string> SetCellContents(string name, string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException();
            }

            if (!IsValidCellName(name))
            {
                throw new InvalidNameException();
            }

            if (!Cells.ContainsKey(name))
            {
                Cells.Add(name, new Cell(name));
            }

            Cells[name].SetContent(text);

            ISet<string> changedSet = new HashSet<string>();
            changedSet.Add(name);
            if (Graph.HasDependents(name))
            {
                foreach (string dependent in Graph.GetDependents(name))
                {
                    changedSet.Add(dependent);
                }
            }

            return changedSet;
        }

        /// <summary>
        /// Requires that all of the variables in formula are valid cell names.
        ///
        /// If name is null or invalid, throws an InvalidNameException.
        ///
        /// Otherwise, if changing the contents of the named cell to be the formula would cause a
        /// circular dependency, throws a CircularException.
        ///
        /// Otherwise, the contents of the named cell becomes formula.  The method returns a
        /// Set consisting of name plus the names of all other cells whose value depends,
        /// directly or indirectly, on the named cell.
        ///
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        protected override ISet<string> SetCellContents(string name, Formula formula)
        {
            if (!IsValidCellName(name))
            {
                throw new InvalidNameException();
            }

            if (!Cells.ContainsKey(name))
            {
                Cells.Add(name, new Cell(name));
            }

            // Set consisting of variables in formula
            ISet<string> variables = formula.GetVariables();

            Cells[name].SetContent(formula);

            // Remove dependess that aren't there in the formula
            if (Graph.HasDependees(name))
            {
                foreach (string dependee in Graph.GetDependees(name).ToList())
                {
                    if (!variables.Contains(dependee))
                    {
                        Graph.RemoveDependency(dependee, name);
                    }
                }
            }

            // Add dependees that don't already exist
            foreach (string variable in variables)
            {
                Graph.AddDependency(variable, name);
            }


            ISet<string> changedSet = new HashSet<string>();
            changedSet.Add(name);
            if (Graph.HasDependents(name))
            {
                foreach (string dependent in Graph.GetDependents(name))
                {
                    changedSet.Add(dependent);
                }
            }

            // To check for Circular Dependency
            GetCellsToRecalculate(changedSet);
            return changedSet;
        }


        /// <summary>
        /// If name is null, throws an ArgumentNullException.
        ///
        /// Otherwise, if name isn't a valid cell name, throws an InvalidNameException.
        ///
        /// Otherwise, returns an enumeration, without duplicates, of the names of all cells whose
        /// values depend directly on the value of the named cell.  In other words, returns
        /// an enumeration, without duplicates, of the names of all cells that contain
        /// formulas containing name.
        ///
        /// For example, suppose that
        /// A1 contains 3
        /// B1 contains the formula A1 * A1
        /// C1 contains the formula B1 + A1
        /// D1 contains the formula B1 - C1
        /// The direct dependents of A1 are B1 and C1
        /// </summary>
        protected override IEnumerable<string> GetDirectDependents(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException();
            }

            if (!IsValidCellName(name))
            {
                throw new InvalidNameException();
            }

            if (Graph.HasDependents(name))
            {
                foreach (string dependent in Graph.GetDependents(name))
                {
                    yield return dependent;
                }
            }
        }
    }
}