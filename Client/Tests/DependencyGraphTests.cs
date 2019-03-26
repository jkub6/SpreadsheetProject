using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpreadsheetUtilities;

namespace UnitTests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class DependencyGraphUnitTests
    {
        private DependencyGraph graph;

        public DependencyGraphUnitTests()
        {
            graph = new DependencyGraph();
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void SizeZero()
        {
            Assert.AreEqual(0, graph.Size);
        }

        [TestMethod]
        public void SizeZeroAddRemove()
        {
            graph.AddDependency("A", "B");
            Assert.AreEqual(1, graph.Size);
            graph.RemoveDependency("A", "B");
            Assert.AreEqual(0, graph.Size);
        }

        [TestMethod]
        public void SizeZeroAddDuplicate()
        {
            graph.AddDependency("A", "B");
            graph.AddDependency("A", "B");
            graph.AddDependency("A", "B");
            Assert.AreEqual(1, graph.Size);
        }

        [TestMethod]
        public void SizeFive()
        {
            graph.AddDependency("A", "B");
            graph.AddDependency("B", "C");
            graph.AddDependency("C", "D");
            graph.AddDependency("D", "E");
            graph.AddDependency("E", "F");
            Assert.AreEqual(5, graph.Size);
        }

        [TestMethod]
        public void SizeTenThousand()
        {
            for (int i = 0; i < 10000; i++)
                graph.AddDependency(i.ToString(), (i+1).ToString());
            Assert.AreEqual(10000, graph.Size);
            for (int i = 0; i < 10000; i++)
                graph.RemoveDependency(i.ToString(), (i + 1).ToString());
            Assert.AreEqual(0, graph.Size);
        }

        [TestMethod]
        public void IndexerZero()
        {
            Assert.AreEqual(0, graph["A"]);
        }

        [TestMethod]
        public void IndexerOne()
        {
            graph.AddDependency("A", "B");
            Assert.AreEqual(1, graph["B"]);
        }

        [TestMethod]
        public void IndexerOne2()
        {
            graph.AddDependency("A", "B");
            graph.AddDependency("B", "C");
            Assert.AreEqual(1, graph["B"]);
        }

        [TestMethod]
        public void IndexerTwo()
        {
            graph.AddDependency("A", "B");
            graph.AddDependency("C", "B");
            Assert.AreEqual(2, graph["B"]);
        }

        [TestMethod]
        public void HasDependentsZero()
        {
            Assert.IsFalse(graph.HasDependents("A"));
        }

        [TestMethod]
        public void HasDependentsZero2()
        {
            graph.AddDependency("A", "B");
            Assert.IsFalse(graph.HasDependents("B"));
        }

        [TestMethod]
        public void HasDependentsOne()
        {
            graph.AddDependency("A", "B");
            Assert.IsTrue(graph.HasDependents("A"));
        }

        [TestMethod]
        public void HasDependeesZero()
        {
            Assert.IsFalse(graph.HasDependees("A"));
        }

        [TestMethod]
        public void HasDependeesZero2()
        {
            graph.AddDependency("A", "B");
            Assert.IsFalse(graph.HasDependees("A"));
        }

        [TestMethod]
        public void HasDependeesOne()
        {
            graph.AddDependency("A", "B");
            Assert.IsTrue(graph.HasDependees("B"));
        }

        [TestMethod]
        public void GetDependentsZero()
        {
            var e = graph.GetDependents("A").GetEnumerator();
            Assert.IsFalse(e.MoveNext());
        }

        [TestMethod]
        public void GetDependentsZero2()
        {
            graph.AddDependency("A", "B");
            var e = graph.GetDependents("B").GetEnumerator();
            Assert.IsFalse(e.MoveNext());
        }

        [TestMethod]
        public void GetDependentsOne()
        {
            graph.AddDependency("A", "B");
            var e = graph.GetDependents("A").GetEnumerator();
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("B", e.Current);
            Assert.IsFalse(e.MoveNext());
        }

        [TestMethod]
        public void GetDependentsOne2()
        {
            graph.AddDependency("A", "B");
            graph.AddDependency("A", "B");
            var e = graph.GetDependents("A").GetEnumerator();
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("B", e.Current);
            Assert.IsFalse(e.MoveNext());
        }

        [TestMethod]
        public void GetDependentsTwo()
        {
            graph.AddDependency("A", "B");
            graph.AddDependency("A", "C");
            graph.AddDependency("C", "B");
            var e = graph.GetDependents("A").GetEnumerator();
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("B", e.Current);
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("C", e.Current);
            Assert.IsFalse(e.MoveNext());
        }

        [TestMethod]
        public void GetDependeesZero()
        {
            var e = graph.GetDependees("A").GetEnumerator();
            Assert.IsFalse(e.MoveNext());
        }

        [TestMethod]
        public void GetDependeesZero2()
        {
            graph.AddDependency("A", "B");
            var e = graph.GetDependees("A").GetEnumerator();
            Assert.IsFalse(e.MoveNext());
        }

        [TestMethod]
        public void GetDependeesOne()
        {
            graph.AddDependency("A", "B");
            var e = graph.GetDependees("B").GetEnumerator();
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("A", e.Current);
            Assert.IsFalse(e.MoveNext());
        }

        [TestMethod]
        public void GetDependeesOne2()
        {
            graph.AddDependency("A", "B");
            graph.AddDependency("A", "B");
            var e = graph.GetDependees("B").GetEnumerator();
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("A", e.Current);
            Assert.IsFalse(e.MoveNext());
        }

        [TestMethod]
        public void GetDependeesTwo()
        {
            graph.AddDependency("A", "B");
            graph.AddDependency("C", "B");
            graph.AddDependency("A", "C");
            var e = graph.GetDependees("B").GetEnumerator();
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("A", e.Current);
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("C", e.Current);
            Assert.IsFalse(e.MoveNext());
        }

        [TestMethod]
        public void AddDependencyCircular()
        {
            graph.AddDependency("A", "B");
            graph.AddDependency("B", "C");
            graph.AddDependency("C", "A");

            Assert.AreEqual(3, graph.Size);
        }

        [TestMethod]
        public void removeDependencyZero()
        {
            graph.RemoveDependency("A", "B");
        }

        [TestMethod]
        public void ReplaceDependentsNoneWithNone()
        {
            List<string> list = new List<string> {};
            graph.ReplaceDependents("A", list);
            Assert.AreEqual(0, graph.Size);
        }

        [TestMethod]
        public void ReplaceDependentsNoneWithSome()
        {
            List<string> list = new List<string> { "B", "C", "D" };
            graph.ReplaceDependents("A", list);
            Assert.AreEqual(3, graph.Size);
        }

        [TestMethod]
        public void ReplaceDependentsSomeWithSome()
        {
            List<string> list = new List<string> { "B", "C", "D" };
            graph.AddDependency("A", "Q");
            graph.AddDependency("A", "R");
            graph.ReplaceDependents("A", list);
            Assert.AreEqual(3, graph.Size);
        }

        [TestMethod]
        public void ReplaceDependentsSomeWithSome2()
        {
            List<string> list = new List<string> { "B", "C", "D" };
            graph.AddDependency("B", "A");
            graph.AddDependency("A", "R");
            graph.ReplaceDependents("A", list);
            Assert.AreEqual(4, graph.Size);
        }

        [TestMethod]
        public void ReplaceDependeesNoneWithNone()
        {
            List<string> list = new List<string> { };
            graph.ReplaceDependees("A", list);
            Assert.AreEqual(0, graph.Size);
        }

        [TestMethod]
        public void ReplaceDependeesNoneWithSome()
        {
            List<string> list = new List<string> { "B", "C", "D" };
            graph.ReplaceDependees("A", list);
            Assert.AreEqual(3, graph.Size);
        }

        [TestMethod]
        public void ReplaceDependeesSomeWithSome()
        {
            List<string> list = new List<string> { "B", "C", "D" };
            graph.AddDependency("G", "A");
            graph.AddDependency("H", "A");
            graph.ReplaceDependees("A", list);
            Assert.AreEqual(3, graph.Size);
        }

        [TestMethod]
        public void ReplaceDependeesSomeWithSome2()
        {
            List<string> list = new List<string> { "B", "C", "D" };
            graph.AddDependency("A", "B");
            graph.AddDependency("Q", "A");
            graph.ReplaceDependees("A", list);
            Assert.AreEqual(4, graph.Size);
        }

        [TestMethod()]
        public void SuperStressTest()
        {
            // Dependency graph
            DependencyGraph t = new DependencyGraph();

            // A bunch of strings to use
            const int SIZE = 1000;
            string[] letters = new string[SIZE];
            for (int i = 0; i < SIZE; i++)
            {
                letters[i] = ("" + (char)('a' + i));
            }

            // The correct answers
            HashSet<string>[] dents = new HashSet<string>[SIZE];
            HashSet<string>[] dees = new HashSet<string>[SIZE];
            for (int i = 0; i < SIZE; i++)
            {
                dents[i] = new HashSet<string>();
                dees[i] = new HashSet<string>();
            }

            // Add a bunch of dependencies
            for (int i = 0; i < SIZE; i++)
            {
                for (int j = i + 1; j < SIZE; j++)
                {
                    t.AddDependency(letters[i], letters[j]);
                    dents[i].Add(letters[j]);
                    dees[j].Add(letters[i]);
                }
            }

            // Remove a bunch of dependencies
            for (int i = 0; i < SIZE; i++)
            {
                for (int j = i + 4; j < SIZE; j += 4)
                {
                    t.RemoveDependency(letters[i], letters[j]);
                    dents[i].Remove(letters[j]);
                    dees[j].Remove(letters[i]);
                }
            }

            // Add some back
            for (int i = 0; i < SIZE; i++)
            {
                for (int j = i + 1; j < SIZE; j += 2)
                {
                    t.AddDependency(letters[i], letters[j]);
                    dents[i].Add(letters[j]);
                    dees[j].Add(letters[i]);
                }
            }

            // Remove some more
            for (int i = 0; i < SIZE; i += 2)
            {
                for (int j = i + 3; j < SIZE; j += 3)
                {
                    t.RemoveDependency(letters[i], letters[j]);
                    dents[i].Remove(letters[j]);
                    dees[j].Remove(letters[i]);
                }
            }

            // Make sure everything is right
            for (int i = 0; i < SIZE; i++)
            {
                Assert.IsTrue(dents[i].SetEquals(new HashSet<string>(t.GetDependents(letters[i]))));
                Assert.IsTrue(dees[i].SetEquals(new HashSet<string>(t.GetDependees(letters[i]))));
            }
        }
    }
}
