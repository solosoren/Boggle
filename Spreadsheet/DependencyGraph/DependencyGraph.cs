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
        private Dictionary<string, GraphNode> nodeList;
        private int NumberOfDependencies;

        /// <summary>
        /// Creates a DependencyGraph containing no dependencies.
        /// </summary>
        public DependencyGraph()
        {
            nodeList = new Dictionary<string, GraphNode>();
        }

        public DependencyGraph(DependencyGraph dependencyGraph)
        {
            nodeList = dependencyGraph.nodeList;
            NumberOfDependencies = dependencyGraph.NumberOfDependencies;
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
        /// If s is null, an ArgumentNullException is thrown.
        /// </summary>
        public bool HasDependents(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException();
            }

            if (nodeList.Count == 0)
            {
                return false;
            }

            if (!nodeList.ContainsKey(s))
            {
                return false;
            }

            return nodeList[s].NumberOfDependents() != 0;
        }


        /// <summary>
        /// Reports whether dependees(s) is non-empty.  Requires s != null.
        /// If s is null, an ArgumentNullException is thrown.
        /// </summary>
        public bool HasDependees(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException();
            }

            if (nodeList.Count == 0)
            {
                return false;
            }

            if (!nodeList.ContainsKey(s))
            {
                return false;
            }

            return nodeList[s].NumberOfDependees() != 0;
        }

        /// <summary>
        /// Enumerates dependents(s).  Requires s != null.
        /// If s is null, an ArgumentNullException is thrown.
        /// </summary>
        public IEnumerable<string> GetDependents(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException();
            }

            if (nodeList.Count == 0)
            {
                yield break;
            }

            foreach (GraphNode dependent in nodeList[s].Dependents)
            {
                yield return dependent.Name;
            }
        }

        /// <summary>
        /// Enumerates dependees(s).  Requires s != null.
        /// If s is null, an ArgumentNullException is thrown.
        /// </summary>
        public IEnumerable<string> GetDependees(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException();
            }

            if (nodeList.Count == 0)
            {
                yield break;
            }

            foreach (GraphNode dependee in nodeList[s].Dependees)
            {
                yield return dependee.Name;
            }
        }

        /// <summary>
        /// Adds the dependency (s,t) to this DependencyGraph.
        /// This has no effect if (s,t) already belongs to this DependencyGraph.
        /// Requires s != null and t != null.
        /// If s or t is null, an ArgumentNullException is thrown.
        /// </summary>
        public void AddDependency(string s, string t)
        {
            if (s == null || t == null)
            {
                throw new ArgumentNullException();
            }
//            if (s.Equals(t))
//            {
//                throw new Exception(s + " can't be a dependent and a dependee of itself.");
//            }

            GraphNode dependentToAdd = new GraphNode(t);
            GraphNode dependeeToAdd = new GraphNode(s);
            // This means the graph is empty

            // Add s to the graph
            if (!nodeList.ContainsKey(s))
                nodeList.Add(s, dependeeToAdd);
            if (!nodeList.ContainsKey(t))
                nodeList.Add(t, dependentToAdd);


            if (nodeList[s].HasDependent(t) && nodeList[t].HasDependee(s))
            {
                return;
            }

//            if (nodeList[t].HasDependent(s) && nodeList[s].HasDependee(t))
//            {
//                throw new Exception(t + " is a dependee of " + s + " and " + s + " is a dependent of " + t +
//                                    ". To conintue with (" + t + ", " + s + ") remove (" + s + ", " + t + ") first.");
//            }

            // s.Dependents does not contain t
            if (!nodeList[s].HasDependent(t))
            {
                // Add t to s.Dependents
                nodeList[s].Dependents.Add(nodeList[t]);
            }

            // t.Dependee does not contain s
            if (!nodeList[t].HasDependee(s))
            {
                // Add s to t.Dependees
                nodeList[t].Dependees.Add(nodeList[s]);
            }

            NumberOfDependencies++;

            // dependees(t) = s
            // dependent(s) = t
        }

        /// <summary>
        /// Removes the dependency (s,t) from this DependencyGraph.
        /// Does nothing if (s,t) doesn't belong to this DependencyGraph.
        /// Requires s != null and t != null.
        /// If s or t is null, an ArgumentNullException is thrown.
        /// </summary>
        public void RemoveDependency(string s, string t)
        {
            if (s == null || t == null)
            {
                throw new ArgumentNullException();
            }

            if (nodeList.Count == 0)
            {
                return;
            }

            // s exists in nodeList and so does (s,t)
            if (nodeList.ContainsKey(s) && nodeList.ContainsKey(t) && nodeList[s].HasDependent(t) &&
                nodeList[t].HasDependee(s))
            {
                nodeList[s].Dependents.Remove(nodeList[t]);
                nodeList[t].Dependees.Remove(nodeList[s]);
                NumberOfDependencies--;
            }

            if (nodeList[s].NumberOfDependents() == 0 && nodeList[s].NumberOfDependees() == 0)
            {
                nodeList.Remove(s);
            }

            if (nodeList[t].NumberOfDependents() == 0 && nodeList[t].NumberOfDependees() == 0)
            {
                nodeList.Remove(t);
            }
        }

        /// <summary>
        /// Removes all existing dependencies of the form (s,r).  Then, for each
        /// t in newDependents, adds the dependency (s,t).
        /// Requires s != null and t != null.
        /// If s or newDependents is null, an ArgumentNullException is thrown.
        /// If newDependents contains a null string, an ArgumentNullException is thrown.
        /// </summary>
        public void ReplaceDependents(string s, IEnumerable<string> newDependents)
        {
            if (s == null || newDependents == null)
            {
                throw new ArgumentNullException();
            }

            if (nodeList.Count == 0)
            {
                return;
            }

            // Empty Dependents(s)
            foreach (GraphNode dependent in nodeList[s].Dependents.ToList())
            {
                RemoveDependency(s, dependent.Name);
            }

            // Add new Dependents(s)
            foreach (string dependent in newDependents.ToList())
            {
                if (dependent == null)
                {
                    throw new ArgumentNullException();
                }

                AddDependency(s, dependent);
            }
        }

        /// <summary>
        /// Removes all existing dependencies of the form (r,t).  Then, for each
        /// s in newDependees, adds the dependency (s,t).
        /// Requires s != null and t != null.
        /// If t or newDependees is null, an ArgumentNullException is thrown.
        /// If newDependees contains a null string, an ArgumentNullException is thrown.
        /// </summary>
        public void ReplaceDependees(string t, IEnumerable<string> newDependees)
        {
            if (t == null || newDependees == null)
            {
                throw new ArgumentNullException();
            }

            if (nodeList.Count == 0)
            {
                return;
            }

            // Empty Dependees(t)
            foreach (GraphNode dependee in nodeList[t].Dependees.ToList())
            {
                RemoveDependency(dependee.Name, t);
            }

            // Add new Dependees(t)
            foreach (string dependee in newDependees.ToList())
            {
                if (dependee == null)
                {
                    throw new ArgumentNullException();
                }

                AddDependency(dependee, t);
            }
        }
    }

    class GraphNode
    {
        public string Name { get; set; }
        public List<GraphNode> Dependents;
        public List<GraphNode> Dependees;

        public GraphNode()
        {
            Name = null;
            Dependents = new List<GraphNode>();
            Dependees = new List<GraphNode>();
        }


        public GraphNode(string name)
        {
            this.Name = name;
            Dependents = new List<GraphNode>();
            Dependees = new List<GraphNode>();
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

        /// <summary>
        /// Checks to see if s is a dependent of the object
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public bool HasDependent(string s)
        {
            foreach (GraphNode dependent in Dependents)
            {
                if (dependent.Name.Equals(s))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks to see if s is a dependee of the object
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public bool HasDependee(string s)
        {
            foreach (GraphNode dependee in Dependees)
            {
                if (dependee.Name.Equals(s))
                {
                    return true;
                }
            }

            return false;
        }
    }
}