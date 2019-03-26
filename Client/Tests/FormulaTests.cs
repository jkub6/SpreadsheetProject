using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpreadsheetUtilities;

namespace UnitTests
{
    [TestClass]
    public class FormulaTests
    {
        private double TestLookup(String v)
        {
            switch (v)
            {
                case "a1":
                    return 1;
                case "A1":
                    return 2;
                case "B143":
                    return 3;
                case "ab1":
                    return 4;
                case "Abr114":
                    return 5;
                case "aBr114":
                    return 6;
                default:
                    throw new Exception();
            }
        }

        [TestMethod]
        public void Addition()
        {
            Assert.AreEqual(9.0, new Formula("4+5").Evaluate(TestLookup));
        }

        [TestMethod]
        public void Subtraction()
        {
            Assert.AreEqual(2.0, new Formula("6-4").Evaluate(TestLookup));
        }

        [TestMethod]
        public void SubtractionNegative()
        {
            Assert.AreEqual(-4.0, new Formula("3-7").Evaluate(TestLookup));
        }

        [TestMethod]
        public void Multiplication()
        {
            Assert.AreEqual(56.0, new Formula("7*8").Evaluate(TestLookup));
        }

        [TestMethod]
        public void Division()
        {
            Assert.AreEqual(4.0, new Formula("8/2").Evaluate(TestLookup));
        }

        [TestMethod]
        public void DivisionInteger()
        {
            Assert.AreNotEqual(2.0, new Formula("5/2").Evaluate(TestLookup));
            Assert.AreEqual(2.5, new Formula("5/2").Evaluate(TestLookup));
        }

        [TestMethod]
        public void OrderOfOperations()
        {
            Assert.AreEqual(8.0, new Formula("5+6/2").Evaluate(TestLookup));
        }

        [TestMethod]
        public void ParenthesisAddition()
        {
            Assert.AreEqual(15.0, new Formula("(12+3)").Evaluate(TestLookup));
        }

        [TestMethod]
        public void DoubleParenthesisAddition()
        {
            Assert.AreEqual(15.0, new Formula("((12+(3)))").Evaluate(TestLookup));
        }

        [TestMethod]
        public void ParenthesisDivision()
        {
            Assert.AreEqual(4.0, new Formula("(12/3)").Evaluate(TestLookup));
        }

        [TestMethod]
        public void DoubleParenthesisDivision()
        {
            Assert.AreEqual(4.0, new Formula("((12/3))").Evaluate(TestLookup));
        }

        [TestMethod]
        public void OrderOfOperationsParenthesis()
        {
            Assert.AreEqual(24.0, new Formula("60/5*(7-5)").Evaluate(TestLookup));
        }

        [TestMethod]
        public void BigArithmeticCombo1()
        {
            Assert.AreEqual(-3.0, new Formula("2 + 3 - (4 * 2) / 2 * (1 + 1)").Evaluate(TestLookup));
        }

        [TestMethod]
        public void BigArithmeticCombo2()
        {
            Assert.AreEqual(-466908.0, new Formula("(2*((12*4+13)+3)+(22+77)*12-234809+(12*3+2-6)+(4/2+8/4+16/8)+1)*2").Evaluate(TestLookup));
        }

        [TestMethod]
        public void VariableAloneLower()
        {
            Assert.AreEqual(1.0, new Formula("a1").Evaluate(TestLookup));
        }

        [TestMethod]
        public void VariableAloneUpper()
        {
            Assert.AreEqual(2.0, new Formula("A1").Evaluate(TestLookup));
        }

        [TestMethod]
        public void VariableAloneMultiLetter()
        {
            Assert.AreEqual(4.0, new Formula("ab1").Evaluate(TestLookup));
        }

        [TestMethod]
        public void VariableAloneMultiNumber()
        {
            Assert.AreEqual(3.0, new Formula("B143").Evaluate(TestLookup));
        }

        [TestMethod]
        public void VariableAloneMultiLetterNumber()
        {
            Assert.AreEqual(5.0, new Formula("Abr114").Evaluate(TestLookup));
        }

        [TestMethod]
        public void VariableSingleOperation()
        {
            Assert.AreEqual(3.0, new Formula("A1+a1").Evaluate(TestLookup));
        }

        [TestMethod]
        public void VariableArithmetic()
        {
            Assert.AreEqual(-3.0, new Formula("A1 + B143 - (ab1 * A1) / A1 * (a1 + a1)").Evaluate(TestLookup));
        }

        [TestMethod]
        public void VariableNonExistant()
        {
            FormulaError a = (FormulaError)new Formula("c4").Evaluate(s => throw new Exception());
            Assert.AreEqual(a.Reason, "Unknown variable \"c4\"");
        }

        [TestMethod]
        public void ParenthesisEmpty()
        {
            var ex = Assert.ThrowsException<FormulaFormatException>(() => new Formula("()").Evaluate(TestLookup));
            Assert.AreEqual(ex.Message, "token: \")\" cannot immediately follow an opening parenthesis or operator");
        }

        [TestMethod]
        public void ParenthesisEmptyDouble()
        {
            var ex = Assert.ThrowsException<FormulaFormatException>(() => new Formula("(())").Evaluate(TestLookup));
            Assert.AreEqual(ex.Message, "token: \")\" cannot immediately follow an opening parenthesis or operator");
        }

        [TestMethod]
        public void ParenthesisEmptyStatement()
        {
            var ex = Assert.ThrowsException<FormulaFormatException>(() => new Formula("()1+1").Evaluate(TestLookup));
            Assert.AreEqual(ex.Message, "token: \")\" cannot immediately follow an opening parenthesis or operator");
        }

        [TestMethod]
        public void ParenthesisUnevenLeft()
        {
            var ex = Assert.ThrowsException<FormulaFormatException>(() => new Formula("(4+3").Evaluate(TestLookup));
            Assert.AreEqual(ex.Message, "Balanced parenthesis rule: the number of opening and closing parentheses must be equal");
        }

        [TestMethod]
        public void ParenthesisUnevenRight()
        {
            var ex = Assert.ThrowsException<FormulaFormatException>(() => new Formula("4+3)").Evaluate(TestLookup));
            Assert.AreEqual(ex.Message, "Right parenthesis rule: unbalanced parenthesis");
        }

        [TestMethod]
        public void JustInteger()
        {
            Assert.AreEqual(12345.0, new Formula("12345").Evaluate(TestLookup));
        }

        [TestMethod]
        public void JustFloat()
        {
            Assert.AreEqual(1235.31, new Formula("1235.31").Evaluate(TestLookup));
        }

        [TestMethod]
        public void ZeroDivision()
        {
            //var a = (double)new Formula("15/0").Evaluate(s => throw new Exception());
            //Assert.IsTrue(double.IsInfinity(a));
            FormulaError b = (FormulaError)new Formula("15/0").Evaluate(TestLookup);
            Assert.AreEqual(b.Reason, "Division by zero error");
        }

        [TestMethod]
        public void Empty()
        {
            var ex = Assert.ThrowsException<FormulaFormatException>(() => new Formula("").Evaluate(TestLookup));
            Assert.AreEqual(ex.Message, "One Token Rule: formula must contain a minimum of one token");
        }

        [TestMethod]
        public void AdjacentNumbers()
        {
            var ex = Assert.ThrowsException<FormulaFormatException>(() => new Formula("1 2 + 3").Evaluate(TestLookup));
            Assert.AreEqual(ex.Message, "Extra Following Rule: \"2\" cannot follow \"1\"");
        }

        [TestMethod]
        public void AdjacentOperators()
        {
            var ex = Assert.ThrowsException<FormulaFormatException>(() => new Formula("1 ++ 3").Evaluate(TestLookup));
            Assert.AreEqual(ex.Message, "token: \"+\" cannot immediately follow an opening parenthesis or operator");
        }

        [TestMethod]
        public void MiscInvalidStatments1()
        {
            var ex = Assert.ThrowsException<FormulaFormatException>(() => new Formula("-1+2").Evaluate(TestLookup));
            Assert.AreEqual(ex.Message, "First token must be a number variable, or opening parenthesis, not \"-\"");

        }

        [TestMethod]
        public void MiscInvalidStatments2()
        {
            var ex = Assert.ThrowsException<FormulaFormatException>(() => new Formula("11+33/3*").Evaluate(TestLookup));
            Assert.AreEqual(ex.Message, "The last token of an expression must be a number, a variable, or a closing parenthesis, not \"*\"");
        }

        [TestMethod]
        public void MiscInvalidStatments3()
        {
            var ex = Assert.ThrowsException<FormulaFormatException>(() => new Formula("5 + * 5 5").Evaluate(TestLookup));
            Assert.AreEqual(ex.Message, "token: \"*\" cannot immediately follow an opening parenthesis or operator");
        }

        [TestMethod]
        public void InvalidToken1()
        {
            var ex = Assert.ThrowsException<FormulaFormatException>(() => new Formula("4$ + 3").Evaluate(TestLookup));
            Assert.AreEqual(ex.Message, "Unrecognized symbol \"$\"");
        }

        [TestMethod]
        public void InvalidToken2()
        {
            var ex = Assert.ThrowsException<FormulaFormatException>(() => new Formula("4 +   3 @").Evaluate(TestLookup));
            Assert.AreEqual(ex.Message, "Unrecognized symbol \"@\"");
        }

        [TestMethod]
        public void Overflow()
        {
            double d = (double)new Formula("5E200 * 5E200 * 5E200 * 5E200").Evaluate(TestLookup);
            Assert.IsTrue(double.IsInfinity(d));
        }

        [TestMethod]
        public void OverflowGiven()
        {
            var ex = Assert.ThrowsException<FormulaFormatException>(() => new Formula("5E400").Evaluate(TestLookup));
            Assert.AreEqual(ex.Message, "Error reading double: \"5E400\"");
        }

    [TestMethod]
        public void ToStringBasic()
        {
            var a = new Formula("4+3");
            string answer = "4+3";
            Assert.AreEqual(answer, a.ToString());
        }

        [TestMethod]
        public void ToStringParenthesis()
        {
            var a = new Formula("4+(3*9)");
            string answer = "4+(3*9)";
            Assert.AreEqual(answer, a.ToString());
        }

        [TestMethod]
        public void ToStringVariable()
        {
            var a = new Formula("4+(3*9)+_aA44");
            string answer = "4+(3*9)+_aA44";
            Assert.AreEqual(answer, a.ToString());
        }

        [TestMethod]
        public void ToStringLong()
        {
            var a = new Formula("1+3*(4+A3-abf33*12) + 15e-14/16E2+0.0 +   .0 + 0.001 + 10.1 + 15E2");
            string answer = "1+3*(4+A3-abf33*12)+1.5E-13/1600+0+0+0.001+10.1+1500";
            Assert.AreEqual(answer, a.ToString());
        }

        [TestMethod]
        public void ToStringNormalize()
        {
            var a = new Formula("A3 + a3 + 44 / 5.0000", s => s.ToUpper(), s => true);
            string answer = "A3+A3+44/5";
            Assert.AreEqual(answer, a.ToString());
        }

        [TestMethod]
        public void EqualsBasic()
        {
            var a = new Formula("12+13");
            var b = new Formula("12+13");
            Assert.AreEqual(a, b);
            Assert.IsTrue(a == b);
            Assert.IsFalse(a != b);
        }

        [TestMethod]
        public void EqualsSpace()
        {
            var a = new Formula("12+           13");
            var b = new Formula("       12 +13   ");
            Assert.AreEqual(a, b);
            Assert.IsTrue(a == b);
            Assert.IsFalse(a != b);
        }

        [TestMethod]
        public void EqualsParenthesis()
        {
            var a = new Formula("12+13*(3*4)");
            var b = new Formula("12+13*(3*4)");
            Assert.AreEqual(a, b);
            Assert.IsTrue(a == b);
            Assert.IsFalse(a != b);
        }

        [TestMethod]
        public void EqualsDecimal()
        {
            var a = new Formula("12.1+13.0");
            var b = new Formula("12.1+13");
            Assert.AreEqual(a, b);
            Assert.IsTrue(a == b);
            Assert.IsFalse(a != b);
        }        

        [TestMethod]
        public void EqualsDoubleFormat()
        {
            var a = new Formula("0+0.1+5E-2");
            var b = new Formula("0.0+.1+0.05");
            Assert.AreEqual(a, b);
            Assert.IsTrue(a == b);
            Assert.IsFalse(a != b);
        }

        [TestMethod]
        public void EqualsNull()
        {
            Formula a = null;
            Formula b = null;
            Assert.IsTrue(a == b);
            Assert.IsFalse(a != b);
        }

        [TestMethod]
        public void EqualsNullFirst()
        {
            Formula a = null;
            Formula b = new Formula("3+7");
            Assert.IsFalse(a == b);
            Assert.IsTrue(a != b);
        }

        [TestMethod]
        public void EqualsNullSecond()
        {
            Formula a = new Formula("3+7");
            Formula b = null;
            Assert.IsFalse(a.Equals(b));
            Assert.IsFalse(a == b);
            Assert.IsTrue(a != b);
        }

        [TestMethod]
        public void EqualsVariablesNormalize()
        {
            var a = new Formula("A3", s => s.ToUpper(), s => true);
            var b = new Formula("a3", s => s.ToUpper(), s => true);
            Assert.AreEqual(a, b);
            Assert.IsTrue(a == b);
            Assert.IsFalse(a != b);
        }

        [TestMethod]
        public void EqualsNotBasic()
        {
            var a = new Formula("4+3");
            var b = new Formula("4+5");
            Assert.AreNotEqual(a, b);
            Assert.IsFalse(a == b);
            Assert.IsTrue(a != b);
        }

        [TestMethod]
        public void EqualsNotVariables()
        {
            var a = new Formula("A3");
            var b = new Formula("a3");
            Assert.AreNotEqual(a, b);
            Assert.IsFalse(a == b);
            Assert.IsTrue(a != b);
        }

        [TestMethod]
        public void EqualsNotNegative() //make for -1
        {
            var a = new Formula("2");
            var b = new Formula("1");
            Assert.AreNotEqual(a, b);
            Assert.IsFalse(a == b);
            Assert.IsTrue(a != b);
        }


        [TestMethod]
        public void GetVariablesNone()
        {
            var a = new Formula("5+3");
            IEnumerable<string> variables = a.GetVariables();
            Assert.IsFalse(variables.Any());
        }

        [TestMethod]
        public void GetVariablesNoneNormalize()
        {
            var a = new Formula("5+3", s => s.ToUpper(), s => true);
            IEnumerable<string> variables = a.GetVariables();
            Assert.IsFalse(variables.Any());
        }

        [TestMethod]
        public void GetVariablesOne()
        {
            var a = new Formula("5+3-A3");
            IEnumerable<string> variables = a.GetVariables();
            Assert.AreEqual(1, variables.Count());
            Assert.IsTrue(variables.Contains("A3"));
        }

        [TestMethod]
        public void GetVariablesOneNormalize()
        {
            var a = new Formula("A3 + a3", s => s.ToUpper(), s => true);
            IEnumerable<string> variables = a.GetVariables();
            Assert.AreEqual(1, variables.Count());
            Assert.IsTrue(variables.Contains("A3"));
        }

        [TestMethod]
        public void GetVariablesTwo()
        {
            var a = new Formula("b2 / 5+3-A3");
            IEnumerable<string> variables = a.GetVariables();
            Assert.AreEqual(2, variables.Count());
            Assert.IsTrue(variables.Contains("b2"));
            Assert.IsTrue(variables.Contains("A3"));
        }

        [TestMethod]
        public void HashCodeCollisionCountLowEnough()
        {
            int equations = 50000;
            int maxCollisionsAllowed = 5;

            Random rnd = new Random(12345);
            List<int> hashCodes = new List<int>();
            char[] operators = { '+', '-', '/', '*' };

            for (int i = 0; i < equations; i++)
            {
                int terms = rnd.Next() % 10; //equations can be up to 10 terms long
                string f = "";

                for (int j = 0; j < terms; j++)
                {
                    f += (rnd.NextDouble()).ToString();
                    f += operators[rnd.Next(4)];
                }
                f += (rnd.NextDouble()).ToString();

                Formula form = new Formula(f);
                hashCodes.Add(form.GetHashCode());
            }

            HashSet<int> set = new HashSet<int>(hashCodes);
            
            int collisions = hashCodes.Count - set.Count;
            Assert.IsTrue(collisions < maxCollisionsAllowed);
        }

        [TestMethod]
        public void InvalidStatmentStartingOperator()
        {
            var ex = Assert.ThrowsException<FormulaFormatException>(() => new Formula("+1+2").Evaluate(s => 3));
            Assert.AreEqual(ex.Message, "First token must be a number variable, or opening parenthesis, not \"+\"");

        }
    }
}