// Skeleton implementation written by Joe Zachary for CS 3500, September 2013.
// Version 1.1 (Fixed error in comment for RemoveDependency.)
// Version 1.2 - Daniel Kopta 
//               (Clarified meaning of dependent and dependee.)
//               (Clarified names in solution/project structure.)
// Version 1.3 - Jacob (completed the project 9/11/2018)

using System.Collections.Generic;
using System.Linq;

namespace SpreadsheetUtilities
{
    /// <summary>
    /// (s1,t1) is an ordered pair of strings
    /// t1 depends on s1; s1 must be evaluated before t1
    /// 
    /// A DependencyGraph can be modeled as a set of ordered pairs of strings.  Two ordered pairs
    /// (s1,t1) and (s2,t2) are considered equal if and only if s1 equals s2 and t1 equals t2.
    /// Recall that sets never contain duplicates.  If an attempt is made to add an element to a 
    /// set, and the element is already in the set, the set remains unchanged.
    /// 
    /// Given a DependencyGraph DG:
    /// 
    ///    (1) If s is a string, the set of all strings t such that (s,t) is in DG is called dependents(s).
    ///        (The set of things that depend on s)    
    ///        
    ///    (2) If s is a string, the set of all strings t such that (t,s) is in DG is called dependees(s).
    ///        (The set of things that s depends on) 
    //
    // For example, suppose DG = {("a", "b"), ("a", "c"), ("b", "d"), ("d", "d")}
    //     dependents("a") = {"b", "c"}
    //     dependents("b") = {"d"}
    //     dependents("c") = {}
    //     dependents("d") = {"d"}
    //     dependees("a") = {}
    //     dependees("b") = {"a"}
    //     dependees("c") = {"a"}
    //     dependees("d") = {"b", "d"}
    /// </summary>
    public class DependencyGraph
    {
        private Dictionary<string, HashSet<string>> dependents = new Dictionary<string, HashSet<string>>();
        private Dictionary<string, HashSet<string>> dependees = new Dictionary<string, HashSet<string>>();
        private int size;

        /// <summary>
        /// The number of ordered pairs in the DependencyGraph.
        /// </summary>
        public int Size => size;

        /// <summary>
        /// The size of dependees(s).
        /// This property is an example of an indexer.  If dg is a DependencyGraph, you would
        /// invoke it like this:
        /// dg["a"]
        /// It should return the size of dependees("a")
        /// </summary>
        public int this[string s] => dependees.TryGetValue(s, out var set) ? set.Count : 0;

        /// <summary>
        /// Reports whether dependents(s) is non-empty.
        /// </summary>
        public bool HasDependents(string s) => dependents.ContainsKey(s) && dependents[s].Any();

        /// <summary>
        /// Reports whether dependees(s) is non-empty.
        /// </summary>
        public bool HasDependees(string s) => dependees.ContainsKey(s) && dependees[s].Any();

        /// <summary>
        /// Enumerates dependents(s).
        /// </summary>
        public IEnumerable<string> GetDependents(string s)
            => HasDependents(s) ? new HashSet<string>(dependents[s]) : new HashSet<string>(); //return dependents of s or new hashset if empty

        /// <summary>
        /// Enumerates dependees(s).
        /// </summary>
        public IEnumerable<string> GetDependees(string s)
            => HasDependees(s) ? new HashSet<string>(dependees[s]) : new HashSet<string>(); //return dependees of s or new hashset if empty

        /// <summary>
        /// <para>Adds the ordered pair (s,t), if it doesn't exist</para>
        /// 
        /// <para>This should be thought of as:</para>   
        /// 
        ///   t depends on s
        ///
        /// </summary>
        /// <param name="s"> s must be evaluated first. T depends on S</param>
        /// <param name="t"> t cannot be evaluated until s is</param>        /// 
        public void AddDependency(string s, string t)
        {
            if (!dependents.ContainsKey(s))
                dependents.Add(s, new HashSet<string>());
            if (!dependees.ContainsKey(t))
                dependees.Add(t, new HashSet<string>());
            if (dependents[s].Add(t) && dependees[t].Add(s)) size++;
        }

        /// <summary>
        /// Removes the ordered pair (s,t), if it exists
        /// </summary>
        /// <param name="s"></param>
        /// <param name="t"></param>
        public void RemoveDependency(string s, string t)
        {
            if (!HasDependents(s)) return;
            if (dependents[s].Remove(t) && dependees[t].Remove(s)) size--;
        }

        /// <summary>
        /// Removes all existing ordered pairs of the form (s,r).  Then, for each
        /// t in newDependents, adds the ordered pair (s,t).
        /// </summary>
        public void ReplaceDependents(string s, IEnumerable<string> newDependents)
        {
            if (HasDependents(s))
                foreach (string t in new HashSet<string>(dependents[s])) //loop through a copy to allow editing
                    RemoveDependency(s, t);
            foreach (string t in newDependents)
                AddDependency(s, t);
        }

        /// <summary>
        /// Removes all existing ordered pairs of the form (r,s).  Then, for each 
        /// t in newDependees, adds the ordered pair (t,s).
        /// </summary>
        public void ReplaceDependees(string s, IEnumerable<string> newDependees)
        {
            if (HasDependees(s))
                foreach (string t in new HashSet<string>(dependees[s])) //loop through a copy to allow editing
                    RemoveDependency(t, s);
            foreach (string t in newDependees)
                AddDependency(t, s);
        }
    }
}