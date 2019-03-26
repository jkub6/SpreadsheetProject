using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SS;
using SpreadsheetUtilities;

namespace UnitTests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class PS4Tests
    {
        [TestMethod]
        public void GetNamesOfAllNonemptyCellsNone()
        {
            // For example, if name is A1, B1 contains A1 * 2, and C1 contains B1+A1, the
            // set {A1, B1, C1} is returned.
            SS.Spreadsheet s = new Spreadsheet();
            Assert.AreEqual(0, s.GetNamesOfAllNonemptyCells().Count());
        }

        [TestMethod]
        public void GetNamesOfAllNonemptyCellsOne()
        {
            // For example, if name is A1, B1 contains A1 * 2, and C1 contains B1+A1, the
            // set {A1, B1, C1} is returned.
            SS.Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("B1", "=A1*2");
            Assert.AreEqual(1, s.GetNamesOfAllNonemptyCells().Count());
            Assert.IsTrue(s.GetNamesOfAllNonemptyCells().Contains("B1"));
        }

        [TestMethod]
        public void GetNamesOfAllNonemptyCellsTwo()
        {
            // For example, if name is A1, B1 contains A1 * 2, and C1 contains B1+A1, the
            // set {A1, B1, C1} is returned.
            SS.Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("B1", "=A1*2");
            s.SetContentsOfCell("C1", "=D1*2");
            Assert.AreEqual(2, s.GetNamesOfAllNonemptyCells().Count());
            Assert.IsTrue(s.GetNamesOfAllNonemptyCells().Contains("B1"));
            Assert.IsTrue(s.GetNamesOfAllNonemptyCells().Contains("C1"));
        }

        [TestMethod]
        public void SetContentsOfCellOneFormula()
        {
            // For example, if name is A1, B1 contains A1 * 2, and C1 contains B1+A1, the
            // set {A1, B1, C1} is returned.
            SS.Spreadsheet s = new Spreadsheet();
            ISet<string> set = s.SetContentsOfCell("A1", "B1 + 2");
            Assert.AreEqual(1, set.Count);
            Assert.IsTrue(set.Contains("A1"));

            Assert.AreEqual(1, s.GetNamesOfAllNonemptyCells().Count());
            Assert.IsTrue(s.GetNamesOfAllNonemptyCells().Contains("A1"));
        }

        [TestMethod]
        public void SetContentsOfCellTwoFormulas()
        {
            SS.Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("C1", "=B1*2");
            ISet<string> set = s.SetContentsOfCell("B1", "=A1+A2");
            Assert.AreEqual(2, set.Count);
            Assert.IsTrue(set.Contains("C1"));
            Assert.IsTrue(set.Contains("B1"));

            Assert.AreEqual(2, s.GetNamesOfAllNonemptyCells().Count());
            Assert.IsTrue(s.GetNamesOfAllNonemptyCells().Contains("B1"));
            Assert.IsTrue(s.GetNamesOfAllNonemptyCells().Contains("C1"));
        }

        [TestMethod]
        public void SetContentsOfCellTwoFormulasReset()
        {
            SS.Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("C1", "=B1");
            s.SetContentsOfCell("C1", "2.0");
            ISet<string> set = s.SetContentsOfCell("B1", "=A1");
            Assert.AreEqual(1, set.Count);
            Assert.IsTrue(set.Contains("B1"));
        }

        [TestMethod]
        public void SetContentsOfCellThreeFormulasOneDouble()
        {
            // For example, if name is A1, B1 contains A1 * 2, and C1 contains B1+A1, the
            // set {A1, B1, C1} is returned.
            SS.Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("B1", "=A1*2");
            s.SetContentsOfCell("C1", "=B1+A1");
            s.SetContentsOfCell("D1", "=H1+F1");
            ISet<string> set = s.SetContentsOfCell("A1", "4");
            Assert.AreEqual(3, set.Count);
            Assert.IsTrue(set.Contains("A1"));
            Assert.IsTrue(set.Contains("B1"));
            Assert.IsTrue(set.Contains("C1"));

            Assert.AreEqual(4, s.GetNamesOfAllNonemptyCells().Count());
            Assert.IsTrue(s.GetNamesOfAllNonemptyCells().Contains("A1"));
            Assert.IsTrue(s.GetNamesOfAllNonemptyCells().Contains("B1"));
            Assert.IsTrue(s.GetNamesOfAllNonemptyCells().Contains("C1"));
            Assert.IsTrue(s.GetNamesOfAllNonemptyCells().Contains("D1"));
        }

        [TestMethod]
        public void SetContentsOfCellFirstNull()
        {
            SS.Spreadsheet s = new Spreadsheet();
            Assert.ThrowsException<InvalidNameException>(() => s.SetContentsOfCell(null, "=A1*2"));
        }

        [TestMethod]
        public void SetContentsOfCellSecondNull()
        {
            SS.Spreadsheet s = new Spreadsheet();
            Assert.ThrowsException<ArgumentNullException>(() => s.SetContentsOfCell("A5", null));
        }

        [TestMethod]
        public void SetContentsOfCellBothNull()
        {
            SS.Spreadsheet s = new Spreadsheet();
            Assert.ThrowsException<InvalidNameException>(() => s.SetContentsOfCell(null, null));
        }

        [TestMethod]
        public void SetContentsOfCellFirstInvalid()
        {
            SS.Spreadsheet s = new Spreadsheet();
            Assert.ThrowsException<InvalidNameException>(() => s.SetContentsOfCell("$b4", "=12 + 3"));
        }

        [TestMethod]
        public void GetCellContentsDouble()
        {
            // For example, if name is A1, B1 contains A1 * 2, and C1 contains B1+A1, the
            // set {A1, B1, C1} is returned.
            SS.Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "3");
            s.SetContentsOfCell("B1", "-1.0");
            s.SetContentsOfCell("C1", "10e+15");
            s.SetContentsOfCell("D1", ".0001");

            Assert.AreEqual(3.0, s.GetCellContents("A1"));
            Assert.AreEqual(-1.0, s.GetCellContents("B1"));
            Assert.AreEqual(10e+15, s.GetCellContents("C1"));
            Assert.AreEqual(.0001, s.GetCellContents("D1"));
        }

        [TestMethod]
        public void GetCellContentsString()
        {
            SS.Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "test");
            s.SetContentsOfCell("B1", "asdk395@#%$");
            s.SetContentsOfCell("C1", " ");
            s.SetContentsOfCell("D1", "");

            Assert.AreEqual("test", s.GetCellContents("A1"));
            Assert.AreEqual("asdk395@#%$", s.GetCellContents("B1"));
            Assert.AreEqual(" ", s.GetCellContents("C1"));
            Assert.AreEqual("", s.GetCellContents("D1"));
            Assert.AreEqual("", s.GetCellContents("E1"));
        }

        [TestMethod]
        public void GetCellContentsInvalid()
        {
            SS.Spreadsheet s = new Spreadsheet();
            Assert.ThrowsException<InvalidNameException>(() => s.GetCellContents("$A1"));
        }

        [TestMethod]
        public void GetCellContentsNull()
        {
            SS.Spreadsheet s = new Spreadsheet();
            Assert.ThrowsException<InvalidNameException>(() => s.GetCellContents(null));
        }

        [TestMethod]
        public void CircularExceptionSelf()
        {
            SS.Spreadsheet s = new Spreadsheet();
            Assert.ThrowsException<CircularException>(() => s.SetContentsOfCell("A1", "=A1"));
        }

        [TestMethod]
        public void CircularExceptionDirect()
        {
            SS.Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=B1");
            Assert.ThrowsException<CircularException>( () => s.SetContentsOfCell("B1", "=A1"));
        }

        [TestMethod]
        public void CircularExceptionLessDirect()
        {
            SS.Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=14 * B1 - 2* (12 / C4)");
            Assert.ThrowsException<CircularException>(() => s.SetContentsOfCell("B1", "=12 + E4 * (A1 - 3)"));
        }

        [TestMethod]
        public void CircularExceptionIndirect()
        {
            SS.Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=B1");
            s.SetContentsOfCell("B1", "=C1");
            s.SetContentsOfCell("C1", "=D1");
            Assert.ThrowsException<CircularException>(() => s.SetContentsOfCell("D1", "=A1"));
        }

        [TestMethod]
        public void CircularExceptionChangesNothing()
        {
            SS.Spreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("A1", "test");
            s.SetContentsOfCell("B1", "2.0");
            s.SetContentsOfCell("C1", "");
            s.SetContentsOfCell("D1", "=E1");
            Assert.ThrowsException<CircularException>(() => s.SetContentsOfCell("E1", "=D1"));

            var set = s.GetNamesOfAllNonemptyCells();
            Assert.AreEqual(3, set.Count());
            Assert.IsTrue(set.Contains("A1"));
            Assert.IsTrue(set.Contains("B1"));
            Assert.IsTrue(set.Contains("D1"));

            Assert.AreEqual("test", s.GetCellContents("A1"));
            Assert.AreEqual(2.0, s.GetCellContents("B1"));
            Assert.AreEqual("", s.GetCellContents("C1"));
            Assert.AreEqual(new Formula("E1"), s.GetCellContents("D1"));
        }

        [TestMethod]
        public void CircularExceptionChangesFew()
        {
            SS.Spreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("A1", "=C1");
            s.SetContentsOfCell("A1", "=D1");
            s.SetContentsOfCell("A1", "=E1");
            s.SetContentsOfCell("A1", "=F1");
            var a = s.SetContentsOfCell("F1", "=3+3");

            Assert.AreEqual(2, a.Count);
            
        }
    }
}
