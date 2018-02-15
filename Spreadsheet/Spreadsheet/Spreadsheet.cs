using System;
using System.Collections.Generic;
using System.Linq;
using Dependencies;
using Formulas;

namespace SS
{
    public class Cell
    {
        public string name;
        private object content;

        public Cell(string name)
        {
            this.name = name;
            this.content = "";
        }

        public void SetContent(object content)
        {
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

        public Spreadsheet()
        {
            Graph = new DependencyGraph();

            Cells = new Dictionary<string, Cell>();
        }

        /// A string s is a valid cell name if and only if it consists of one or more letters,
        /// followed by a non-zero digit, followed by zero or more digits.
        ///
        /// For example, "A15", "a15", "XY32", and "BC7" are valid cell names.  On the other hand,
        /// "Z", "X07", and "hello" are not valid cell names.
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
        public override ISet<string> SetCellContents(string name, double number)
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
        public override ISet<string> SetCellContents(string name, string text)
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
        public override ISet<string> SetCellContents(string name, Formula formula)
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