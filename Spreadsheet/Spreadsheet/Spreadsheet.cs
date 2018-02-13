using System.Collections.Generic;
using Formulas;

namespace SS
{
    public class Spreadsheet : AbstractSpreadsheet
    {
        public Spreadsheet()
        {

        }

        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {
            throw new System.NotImplementedException();
        }

        public override object GetCellContents(string name)
        {
            throw new System.NotImplementedException();
        }

        public override ISet<string> SetCellContents(string name, double number)
        {
            throw new System.NotImplementedException();
        }

        public override ISet<string> SetCellContents(string name, string text)
        {
            throw new System.NotImplementedException();
        }

        public override ISet<string> SetCellContents(string name, Formula formula)
        {
            throw new System.NotImplementedException();
        }

        protected override IEnumerable<string> GetDirectDependents(string name)
        {
            throw new System.NotImplementedException();
        }
    }

}