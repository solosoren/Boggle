// Skeleton implementation written by Joe Zachary for CS 3500, January 2018.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Dependencies
{
    /// <summary>
    /// A DependencyGraph can be modeled as a set of dependencies, where a dependency is an ordered 
    /// pair of strings.  Two dependencies (s1,t1) and (s2,t2) are considered equal if and only if 
    /// s1 equals s2 and t1 equals t2.
    /// 
    /// Given a DependencyGraph DG:
    /// 
    ///    (1) If s is a string, the set of all strings t such that the dependency (s,t) is in DG 
    ///    is called the dependents of s, which we will denote as dependents(s).
    ///        
    ///    (2) If t is a string, the set of all strings s such that the dependency (s,t) is in DG 
    ///    is called the dependees of t, which we will denote as dependees(t).
    ///    
    /// The notations dependents(s) and dependees(s) are used in the specification of the methods of this class.
    ///
    /// For example, suppose DG = {("a", "b"), ("a", "c"), ("b", "d"), ("d", "d")}
    ///     dependents("a") = {"b", "c"}
    ///     dependents("b") = {"d"}
    ///     dependents("c") = {}
    ///     dependents("d") = {"d"}
    ///     dependees("a") = {}
    ///     dependees("b") = {"a"}
    ///     dependees("c") = {"a"}
    ///     dependees("d") = {"b", "d"}
    ///     
    /// All of the methods below require their string parameters to be non-null.  This means that 
    /// the behavior of the method is undefined when a string parameter is null.  
    ///
    /// IMPORTANT IMPLEMENTATION NOTE
    /// 
    /// The simplest way to describe a DependencyGraph and its methods is as a set of dependencies, 
    /// as discussed above.
    /// 
    /// However, physically representing a DependencyGraph as, say, a set of ordered pairs will not
    /// yield an acceptably efficient representation.  DO NOT USE SUCH A REPRESENTATION.
    /// 
    /// You'll need to be more clever than that.  Design a representation that is both easy to work
    /// with as well acceptably efficient according to the guidelines in the PS3 writeup. Some of
    /// the test cases with which you will be graded will create massive DependencyGraphs.  If you
    /// build an inefficient DependencyGraph this week, you will be regretting it for the next month.
    /// </summary>
    public class DependencyGraph
    {
        private GraphNode Root;
        private int NumberOfDependencies;

        /// <summary>
        /// Creates a DependencyGraph containing no dependencies.
        /// </summary>
        public DependencyGraph()
        {
            Root = new GraphNode();
        }

        /// <summary>
        /// The number of dependencies in the DependencyGraph.
        /// </summary>
        public int Size
        {
            get { return NumberOfDependencies; }
        }

        /// <summary>
        /// Reports whether dependents(s) is non-empty.  Requires s != null.
        /// </summary>
        public bool HasDependents(string s)
        {
            GraphNode temp = TraverseGraphToFind(s);
            if (temp.Name == null)
            {
                return false;
            }

            return temp.NumberOfDependents() != 0;
        }

        private GraphNode TraverseGraphToFind(string s)
        {
            Queue<GraphNode> toVisit = new Queue<GraphNode>();
            HashSet<GraphNode> visited = new HashSet<GraphNode>();
            toVisit.Enqueue(Root);
            while (toVisit.Count != 0)
            {
                GraphNode currentNode = toVisit.Dequeue();

                if (currentNode.Name.Equals(s))
                {
                    return currentNode;
                }

                if (visited.Contains(currentNode))
                {
                    continue;
                }

                visited.Add(currentNode);

                foreach (GraphNode dependee in currentNode.Dependees)
                {
                    toVisit.Enqueue(dependee);
                }
            }

            return new GraphNode();
        }


        /// <summary>
        /// Reports whether dependees(s) is non-empty.  Requires s != null.
        /// </summary>
        public bool HasDependees(string s)
        {
            GraphNode temp = TraverseGraphToFind(s);
            if (temp.Name == null)
            {
                throw new Exception(s + " does not exist");
            }

            return temp.NumberOfDependees() != 0;
        }

        /// <summary>
        /// Enumerates dependents(s).  Requires s != null.
        /// </summary>
        public IEnumerable<string> GetDependents(string s)
        {
            GraphNode temp = TraverseGraphToFind(s);
            if (temp.Name == null)
            {
                throw new Exception(s + " does not exist");
            }

            foreach (GraphNode dependent in temp.Dependents)
            {
                yield return dependent.Name;
            }
        }

        /// <summary>
        /// Enumerates dependees(s).  Requires s != null.
        /// </summary>
        public IEnumerable<string> GetDependees(string s)
        {
            GraphNode temp = TraverseGraphToFind(s);
            if (temp.Name == null)
            {
                throw new Exception(s + " does not exist");
            }

            foreach (GraphNode dependee in temp.Dependees)
            {
                yield return dependee.Name;
            }
        }

        /// <summary>
        /// Adds the dependency (s,t) to this DependencyGraph.
        /// This has no effect if (s,t) already belongs to this DependencyGraph.
        /// Requires s != null and t != null.
        /// </summary>
        public void AddDependency(string s, string t)
        {
            // t is a dependent of s
            // s is a dependee of t
        }

        /// <summary>
        /// Removes the dependency (s,t) from this DependencyGraph.
        /// Does nothing if (s,t) doesn't belong to this DependencyGraph.
        /// Requires s != null and t != null.
        /// </summary>
        public void RemoveDependency(string s, string t)
        {
        }

        /// <summary>
        /// Removes all existing dependencies of the form (s,r).  Then, for each
        /// t in newDependents, adds the dependency (s,t).
        /// Requires s != null and t != null.
        /// </summary>
        public void ReplaceDependents(string s, IEnumerable<string> newDependents)
        {
        }

        /// <summary>
        /// Removes all existing dependencies of the form (r,t).  Then, for each 
        /// s in newDependees, adds the dependency (s,t).
        /// Requires s != null and t != null.
        /// </summary>
        public void ReplaceDependees(string t, IEnumerable<string> newDependees)
        {
        }
    }

    class GraphNode
    {
        public string Name { get; set; }
        public List<GraphNode> Dependents;
        public List<GraphNode> Dependees;
        public bool IsSelfDependent { get; set; }

        public GraphNode()
        {
            Name = null;
            Dependents = new List<GraphNode>();
            Dependees = new List<GraphNode>();
            IsSelfDependent = false;
        }

        public GraphNode(string name)
        {
            this.Name = name;
            Dependents = new List<GraphNode>();
            Dependees = new List<GraphNode>();
            IsSelfDependent = false;
        }

        /// <summary>
        /// Returns the number of dependents for the current GraphNode
        /// </summary>
        /// <returns></returns>
        public int NumberOfDependents()
        {
            return Dependents.Count;
        }

        /// <summary>
        /// Returns the number of dependees for the current GraphNode
        /// </summary>
        /// <returns></returns>
        public int NumberOfDependees()
        {
            return Dependees.Count;
        }
    }
}