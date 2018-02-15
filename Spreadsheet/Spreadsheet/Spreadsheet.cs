using System;
using System.Collections.Generic;
using Dependencies;
using Formulas;

namespace SS
{
    public struct Cell
    {
        public string name;
        private object content;

        public Cell(string name, object content)
        {
            this.name = name;
            this.content = content;
        }

        /// <summary>
        /// Unimplemented for now.
        /// </summary>
        /// <returns></returns>
        public object GetValue()
        {
            return new object();
        }

        public string GetName()
        {
            return name;
        }

        public void SetContent(object newContent)
        {
            content = newContent;
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
            if (!Cells.ContainsKey(name))
            {
                throw new InvalidNameException();
            }

            // TODO: Need to see where to check whether or not return value is string, double or a Formula
            return Cells[name];
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
            //TODO: If name does not exist, add it.
            if (!Cells.ContainsKey(name))
            {
                throw new InvalidNameException();
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

            //TODO: If name does not exist, add it.
            if (!Cells.ContainsKey(name))
            {
                throw new InvalidNameException();
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
            //TODO: If name does not exist, add it.
            if (!Cells.ContainsKey(name))
            {
                throw new InvalidNameException();
            }

            // Set consisting of variables in formula
            ISet<string> variables = new HashSet<string>();
            foreach (string variable in formula.GetVariables())
            {
                if (!Cells.ContainsKey(variable))
                {
                    throw new InvalidNameException();
                }

                variables.Add(variable);
            }

            Cells[name].SetContent(formula);
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

            if (!Cells.ContainsKey(name))
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