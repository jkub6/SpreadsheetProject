using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerAdmin.Models
{
    public class SpreadsheetList
    {
        public List<String> Sheets { get; set; }
        public String type { get; set; }

        /// <summary>
        /// Checks equality of the object by equivalence of the spreadsheet list sent back
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(Object obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                //Checks if the elements are equal
                SpreadsheetList p = (SpreadsheetList)obj;
                if (p.Sheets == null && this.Sheets == null)
                    return true;
                else if (p.Sheets != null && this.Sheets != null)
                    return !p.Sheets.Except(this.Sheets).Any();
                else
                    return false;
            }
        }

        /// <summary>
        /// Overrides HashCode by List of spreadsheet's Hash
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.Sheets.GetHashCode();
        }
    }
}
