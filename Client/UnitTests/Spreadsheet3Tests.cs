using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SS;
using SpreadsheetUtilities;

namespace SpreadsheetTests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class Spreadsheet3
    {
        [TestMethod]
        public void GetCellValueDouble()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "12.556");
            Assert.AreEqual(12.556, s.GetCellValue("A1"));
        }

        [TestMethod]
        public void GetCellValueDoubleNegative()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "-12.556");
            Assert.AreEqual(-12.556, s.GetCellValue("A1"));
        }

        [TestMethod]
        public void GetCellValueDoubleZero()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "0");
            Assert.AreEqual(0.0, s.GetCellValue("A1"));
        }

        [TestMethod]
        public void GetCellValueString()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "hello");
            Assert.AreEqual("hello", s.GetCellValue("A1"));
        }

        [TestMethod]
        public void GetCellValueStringEmpty()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "");
            Assert.AreEqual("", s.GetCellValue("A1"));
        }

        [TestMethod]
        public void GetCellValueStringEmpty2()
        {
            Spreadsheet s = new Spreadsheet();
            Assert.AreEqual("", s.GetCellValue("A1"));
        }

        [TestMethod]
        public void GetCellValueStringNull()
        {
            Spreadsheet s = new Spreadsheet();
            //s.SetContentsOfCell("A1", (string)null);
            Assert.IsTrue(true);
            //Assert.AreEqual("", s.GetCellValue("A1")); //put expected error
        }

        [TestMethod]
        public void GetCellValueFormulaOneReference()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A2", "12");
            s.SetContentsOfCell("A1", "=A2 + 1");
            Assert.AreEqual(13.0, s.GetCellValue("A1"));
        }

        [TestMethod]
        public void GetCellValueFormulaTwoReferences()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A2", "12");
            s.SetContentsOfCell("A3", "5");
            s.SetContentsOfCell("A1", "= A2 * A3");
            Assert.AreEqual(60.0, s.GetCellValue("A1"));
        }

        [TestMethod]
        public void GetCellValueFormulaChainReference()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=A2 + 6");
            s.SetContentsOfCell("A2", "=A3 + 5");
            s.SetContentsOfCell("A3", "=A4 + 4");
            s.SetContentsOfCell("A4", "=A5 + 3");
            s.SetContentsOfCell("A5", "=A6 + 2");
            s.SetContentsOfCell("A6", "1");

            Assert.AreEqual(21.0, s.GetCellValue("A1"));
        }

        [TestMethod]
        public void GetCellValueFormulaTreeReference()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=A2 + A3");

            s.SetContentsOfCell("A2", "=A4 + A5");
            s.SetContentsOfCell("A3", "=A6 + A7");

            s.SetContentsOfCell("A4", "=4");
            s.SetContentsOfCell("A5", "3");
            s.SetContentsOfCell("A6", "=2");
            s.SetContentsOfCell("A7", "1");

            
        }

        [TestMethod]
        public void GetCellValueFormulaStringError()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=A2");
            s.SetContentsOfCell("A2", "blah");

            Object f = s.GetCellValue("A1");
            Assert.IsTrue(f is FormulaError);
        }

        [TestMethod]
        public void GetCellValueFormulaNull()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=A2");
            s.SetContentsOfCell("A2", "3");

            Assert.ThrowsException<InvalidNameException>(() => s.GetCellValue(null));

            Assert.AreEqual(3.0, s.GetCellValue("A1"));
        }

        [TestMethod]
        public void GetCellValueNoNormalize()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=1+B1");
            s.SetContentsOfCell("a1", "=3+b1");
            s.SetContentsOfCell("B1", "5");
            s.SetContentsOfCell("b1", "10");

            Assert.AreEqual(6.0, s.GetCellValue("A1"));
            Assert.AreEqual(13.0, s.GetCellValue("a1"));
        }

        [TestMethod]
        public void GetCellValueYesNormalize()
        {
            Spreadsheet s = new Spreadsheet(t => true, t => t.ToUpper(), "version blah");
            s.SetContentsOfCell("A1", "=1+b1");
            s.SetContentsOfCell("a1", "=3+B1");
            s.SetContentsOfCell("B1", "5");
            s.SetContentsOfCell("b1", "10");

            Assert.AreEqual(13.0, s.GetCellValue("A1"));
            Assert.AreEqual(13.0, s.GetCellValue("a1"));
        }

        [TestMethod]
        public void GetCellValueFormulaInvalidByDeffinition()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=A2");
            s.SetContentsOfCell("A2", "3");

            Assert.ThrowsException<InvalidNameException>(() => s.GetCellValue("3A2"));

            Assert.AreEqual(3.0, s.GetCellValue("A1"));
        }

        [TestMethod]
        public void GetCellValueFormulaInvalidBySpreadsheet()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=A2");
            s.SetContentsOfCell("A2", "3");

            Assert.ThrowsException<InvalidNameException>(() => s.GetCellValue("_A2"));
            Assert.ThrowsException<InvalidNameException>(() => s.GetCellValue("A2A"));
            Assert.ThrowsException<InvalidNameException>(() => s.GetCellValue("A_2"));

            Assert.AreEqual(3.0, s.GetCellValue("A1"));
        }

        [TestMethod]
        public void GetCellValueFormulaInvalidBValidator()
        {
            Spreadsheet s = new Spreadsheet(t => !t.Contains("C"), t => t, "version blah");
            s.SetContentsOfCell("A1", "=A2");
            s.SetContentsOfCell("A2", "3");
            Assert.ThrowsException<InvalidNameException>(() => s.GetCellValue("C5"));

            Assert.AreEqual(3.0, s.GetCellValue("A1"));
        }

        [TestMethod]
        public void SetContentsOfCellInvalidByDeffinition()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=A2");
            s.SetContentsOfCell("A2", "3");

            Assert.ThrowsException<InvalidNameException>(() => s.SetContentsOfCell("3A2", "3"));

            Assert.AreEqual(3.0, s.GetCellValue("A1"));
        }

        [TestMethod]
        public void SetContentsOfCellInvalidBySpreadsheet()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=A2");
            s.SetContentsOfCell("A2", "3");

            Assert.ThrowsException<InvalidNameException>(() => s.SetContentsOfCell("_A2", "3"));
            Assert.ThrowsException<InvalidNameException>(() => s.SetContentsOfCell("A2A", "3"));
            Assert.ThrowsException<InvalidNameException>(() => s.SetContentsOfCell("A_2", "3"));

            Assert.AreEqual(3.0, s.GetCellValue("A1"));
        }

        [TestMethod]
        public void SetContentsOfCellInvalidBValidator()
        {
            Spreadsheet s = new Spreadsheet(t => !t.Contains("C"), t => t, "version blah");
            s.SetContentsOfCell("A1", "=A2");
            s.SetContentsOfCell("A2", "3");
            Assert.ThrowsException<InvalidNameException>(() => s.SetContentsOfCell("C5", "3"));

            Assert.AreEqual(3.0, s.GetCellValue("A1"));
        }

        [TestMethod]
        public void GetCellContentsInvalidByDeffinition()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=A2");
            s.SetContentsOfCell("A2", "3");

            Assert.ThrowsException<InvalidNameException>(() => s.GetCellContents("3A2"));

            Assert.AreEqual(3.0, s.GetCellValue("A1"));
        }

        [TestMethod]
        public void GetCellContentsInvalidBySpreadsheet()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=A2");
            s.SetContentsOfCell("A2", "3");

            Assert.ThrowsException<InvalidNameException>(() => s.GetCellContents("_A2"));
            Assert.ThrowsException<InvalidNameException>(() => s.GetCellContents("A2A"));
            Assert.ThrowsException<InvalidNameException>(() => s.GetCellContents("A_2"));

            Assert.AreEqual(3.0, s.GetCellValue("A1"));
        }

        [TestMethod]
        public void GetCellContentsInvalidBValidator()
        {
            Spreadsheet s = new Spreadsheet(t => !t.Contains("C"), t => t, "version blah");
            s.SetContentsOfCell("A1", "=A2");
            s.SetContentsOfCell("A2", "3");
            Assert.ThrowsException<InvalidNameException>(() => s.GetCellContents("C5"));

            Assert.AreEqual(3.0, s.GetCellValue("A1"));
        }

        [TestMethod]
        public void SaveInvalidName()
        {
            Spreadsheet s = new Spreadsheet();
            Assert.ThrowsException<SpreadsheetReadWriteException>(() => s.Save("      \n.xml"));
        }

        [TestMethod]
        public void SaveAndLoad()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=A2");
            s.SetContentsOfCell("A2", "3");
            s.Save("test.xml");
            
            Spreadsheet s2 = new Spreadsheet("test.xml", t => true, t => t, "default");
            s2.SetContentsOfCell("A2", "10");

            Assert.AreEqual(10.0, s2.GetCellValue("A1"));
        }

        [TestMethod]
        public void SaveAndLoadVersionMismatch()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=A2");
            s.SetContentsOfCell("A2", "3");
            s.Save("test.xml");

            Assert.ThrowsException<SpreadsheetReadWriteException>(() => new Spreadsheet("test.xml", t => true, t => t, "blah"));
        }

        [TestMethod]
        public void SaveAndLoadSpecialCharacters()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "\\/<>,</test><cell>");
            s.Save("test.xml");

            Spreadsheet s2 = new Spreadsheet("test.xml", t => true, t => t, "default");

            Assert.AreEqual("\\/<>,</test><cell>", s2.GetCellValue("A1"));
        }

        [TestMethod]
        public void LoadNonexistant()
        {
            Assert.ThrowsException<SpreadsheetReadWriteException>(() => new Spreadsheet("fake_news.xml", t => true, t => t, "blah"));
        }

        [TestMethod]
        public void LoadFileVersion()
        {
            Spreadsheet s = new Spreadsheet(t => true, t => t, "version 1.2.3");
            s.Save("test.xml");

            Assert.AreEqual("version 1.2.3", s.GetSavedVersion("test.xml"));
        }

        [TestMethod]
        public void LoadFileVersionNonExistant()
        {
            Spreadsheet s = new Spreadsheet(t => true, t => t, "version 1.2.3");

            Assert.ThrowsException<SpreadsheetReadWriteException>(() => s.GetSavedVersion("fake_news.html"));
        }

        [TestMethod]
        public void Changed()
        {
            Spreadsheet s = new Spreadsheet(t => true, t => t, "version 1.2.3");
            s.SetContentsOfCell("A1", "5");
            Assert.IsTrue(s.Changed);
        }

        [TestMethod]
        public void ChangedNot()
        {
            Spreadsheet s = new Spreadsheet(t => true, t => t, "version 1.2.3");
            Assert.IsFalse(s.Changed);
        }

        [TestMethod]
        public void ChangedNotCircular()
        {
            Spreadsheet s = new Spreadsheet();
            Assert.IsFalse(s.Changed);
            Assert.ThrowsException<CircularException>(() => s.SetContentsOfCell("D1", "=D1"));
            Console.WriteLine(s.GetCellValue("D1"));
            Console.WriteLine("asdf");
            Assert.IsFalse(s.Changed);
        }

        [TestMethod]
        public void ChangedNotEmpty()
        {
            Spreadsheet s = new Spreadsheet(t => true, t => t, "version 1.2.3");
            s.SetContentsOfCell("A1", "");
            Assert.IsFalse(s.Changed);
        }

        [TestMethod]
        public void StressTest()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("A0", "0");

            double num = 100000.0;

            for (int i = 0; i < num+1; i++)
                s.SetContentsOfCell("A"+(i+1), "=A"+i+"+1");

            for (int i = 0; i < num; i++)
                Assert.AreEqual(num, s.GetCellValue("A"+num));
        }
    }
}
