using System;
using System.Collections;
using System.Collections.Generic;
using Dependencies;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DependencyGraphTestCases
{
    [TestClass]
    public class UnitTest1
    {
        private DependencyGraph graph;

        /// <summary>
        /// Tests to see if the graph size is zero when it contains no dependencies.
        /// </summary>
        [TestMethod]
        public void TestNoDependency()
        {
            graph = new DependencyGraph();
            Assert.IsTrue(graph.Size == 0);
        }

        /// <summary>
        /// Tests to see if the graph size is one when it contains on dependency.
        /// </summary>
        [TestMethod]
        public void TestAddOneDependency()
        {
            graph = new DependencyGraph();
            graph.AddDependency("A", "B");
            Assert.IsTrue(graph.Size == 1);
        }

        /// <summary>
        /// Tests to see if the graph size is reduced when the dependency is removed.
        /// </summary>
        [TestMethod]
        public void TestRemoveDependency()
        {
            graph = new DependencyGraph();
            graph.AddDependency("A", "B");
            graph.RemoveDependency("A", "B");
            Assert.IsTrue(graph.Size == 0);
        }

        /// <summary>
        /// Tests to see if all dependees of B are returned and if they are correct.
        /// Also tests to see if B is a dependent of A and C.
        /// </summary>
        [TestMethod]
        public void TestAddTwoDependencies()
        {
            graph = new DependencyGraph();
            graph.AddDependency("A", "B");
            graph.AddDependency("C", "B");
            foreach (string dependee in graph.GetDependees("B"))
            {
                Assert.IsTrue(dependee.Equals("A") || dependee.Equals("C"));
                Assert.IsFalse(dependee.Equals("D"));
                Assert.IsTrue(graph.HasDependees("B"));
                Assert.IsTrue(graph.HasDependents("A"));
                Assert.IsTrue(graph.HasDependents("C"));
            }

            foreach (string dependent in graph.GetDependents("A"))
            {
                Assert.IsTrue(dependent.Equals("B"));
            }

            foreach (string dependent in graph.GetDependents("C"))
            {
                Assert.IsTrue(dependent.Equals("B"));
            }
        }

        /// <summary>
        /// Test method called ReplaceDependents()
        /// </summary>
        [TestMethod]
        public void TestReplaceDependents()
        {
            graph = new DependencyGraph();
            graph.AddDependency("A", "B");
            graph.AddDependency("A", "C");
            graph.AddDependency("A", "D");
            graph.ReplaceDependents("A", GetList());
            foreach (string dependent in graph.GetDependents("A"))
            {
                Assert.IsTrue(dependent.Equals("T") || dependent.Equals("W") || dependent.Equals("S") ||
                              dependent.Equals("G"));
            }

            Assert.IsTrue(graph.Size == 4);
        }

        /// <summary>
        /// Test method called ReplaceDependee()
        /// </summary>
        [TestMethod]
        public void TestReplaceDependee()
        {
            graph = new DependencyGraph();
            graph.AddDependency("A", "B");
            graph.AddDependency("C", "B");
            graph.AddDependency("D", "B");
            graph.ReplaceDependees("B", GetList());
            foreach (string dependee in graph.GetDependees("B"))
            {
                Assert.IsTrue(dependee.Equals("T") || dependee.Equals("W") || dependee.Equals("S") ||
                              dependee.Equals("G"));
            }

            Assert.IsTrue(graph.Size == 4);
        }

        /// <summary>
        /// Test to see that nothing happens if dependency that alreadys exists is added.
        /// </summary>
        [TestMethod]
        public void TestAddDependencyForExisting()
        {
            graph = new DependencyGraph();
            graph.AddDependency("A", "B");
            graph.AddDependency("A", "B");
            Assert.IsTrue(graph.Size == 1);
        }

        /// <summary>
        /// Test to see if connect to seperate depenedencies works as intended.
        /// </summary>
        [TestMethod]
        public void TestConnectingTwoSeperateDependencies()
        {
            graph = new DependencyGraph();
            graph.AddDependency("A", "B");
            graph.AddDependency("C", "D");
            graph.AddDependency("B", "D");
            Assert.IsTrue(graph.Size == 3);
        }

        /// <summary>
        /// Test to see if (t, s) is not added while (s, t) exists in the graph
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public void TestAddOppositeDependency()
        {
            graph = new DependencyGraph();
            graph.AddDependency("A", "B");

            try
            {
                graph.AddDependency("B", "A");
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(true);
            }

            Assert.IsTrue(graph.Size == 1);
        }

        /// <summary>
        /// Test to see if adding (s, t), (t, u) and (s, u) works
        /// </summary>
        [TestMethod]
        public void TestCircularDependency()
        {
            graph = new DependencyGraph();
            try
            {
                graph.AddDependency("A", "B");
                graph.AddDependency("B", "C");
                graph.AddDependency("A", "C");
                Assert.IsTrue(true);
            }
            catch (Exception e)
            {
                Assert.Fail();
            }

            Assert.IsTrue(graph.Size == 3);
        }

        /// <summary>
        /// Private method to help generate an IEnumerable object.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<string> GetList()
        {
            string[] names = {"T", "W", "S", "G"};
            foreach (string name in names)
            {
                yield return name;
            }
        }
    }
}