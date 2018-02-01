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
            }
        }
    }
}