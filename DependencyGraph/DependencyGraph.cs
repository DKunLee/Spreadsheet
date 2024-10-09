// Skeleton implementation written by Joe Zachary for CS 3500, September 2013.
// Version 1.1 (Fixed error in comment for RemoveDependency.)
// Version 1.2 - Daniel Kopta 
//               (Clarified meaning of dependent and dependee.)
//               (Clarified names in solution/project structure.)


using System.ComponentModel;

/// <summary>
/// Author:    DK Lee
/// Partner:   None
/// Date:      Jan 20, 2024
/// Course:    CS 3500, University of Utah, School of Computing
/// Copyright: CS 3500 and DK Lee - This work may not 
///            be copied for use in Academic Coursework.
///
///     I, DK, certify that I wrote this code from scratch and
///     did not copy it in part or whole from another source.  All 
///     references used in the completion of the assignments are cited 
///     in my README file.
/// </summary>
/// <summary>
/// File Contents (In summary)
///     This is the DependencyGraph.cs file, which can hold the information of dependency which contains
///     dependents and dependees.
/// </summary>
/// <remarks>
/// File Contents (In details)
/// <para>
///     We need the dependency informations to calculate each cells from spreadsheet. In this spreadsheet
///     project, we define the dependency in two groups. One is dependents, one is dependees. Dependent is
///     the cell that depends on another cell. Dependee is the cell that is depended upon.
/// </para>
/// <para>
///     In this DependencyGraph class, (s1,t1) is an ordered pair of strings.
///         - t1 depends on s1; s1 must be evaluated before t1
///         - (s1, t1) = (Dependee, Dependent)
/// </para>
/// <para>
///     A DependencyGraph can be modeled as a set of ordered pairs of strings. Two ordered pairs,
///     (s1,t1) and (s2,t2) are considered equal if and only if s1 equals s2 and t1 equals t2.
///     Recall that sets never contain duplicates. If an attempt is made to add an element to a 
///     set, and the element is already in the set, the set remains unchanged.
/// </para>
/// <para>
///     In this DependencyGraph, the DG dictionary has the key of cells name and each key, it includes
///     array that carries the two different Lists(Hash Set), which are List(Hash Set) of dependees and dependents.
/// </para>
/// <para>
///     Given a DependencyGraph DG:
/// 
///        (1) If s is a string, the set of all strings t such that (s,t) is in DG is called dependents(s).
///            (The set of things that depend on s)    
///        
///        (2) If s is a string, the set of all strings t such that (t,s) is in DG is called dependees(s).
///            (The set of things that s depends on t) 
///
///
///     For example, suppose DG = {("a", "b"), ("a", "c"), ("b", "d"), ("d", "d")}
///          dependents("a") = {"b", "c"}
///          dependents("b") = {"d"}
///          dependents("c") = {}
///          dependents("d") = {"d"}
///          dependees("a") = {}
///          dependees("b") = {"a"}
///          dependees("c") = {"a"}
///          dependees("d") = {"b", "d"}
/// </para>
/// </remarks>
namespace SpreadsheetUtilities
{
    public class DependencyGraph
    {
        /// <summary>
        /// This two dictionaries has the key of cell name(variable name) with
        /// List(Hash Set) of dependees and List(Hash Set) of dependents.
        /// </summary>
        private Dictionary<string, HashSet<string>> dependents;
        private Dictionary<string, HashSet<string>> dependees;

        /// <summary>
        /// Creates an empty DependencyGraph.
        /// </summary>
        public DependencyGraph()
        {
            dependents = new Dictionary<string, HashSet<string>>();
            dependees = new Dictionary<string, HashSet<string>>();
        }


        /// <summary>
        /// The size of Dependency Graph
        /// </summary>
        /// <returns>Size of DependencyGraph</returns>
        public int Size
        {
            get
            {
                int size = 0;
                foreach (HashSet<string> dees in dependents.Values)
                {
                    size += dees.Count;
                }
                return size;
            }
        }


        /// <summary>
        /// The size of dependees(s).
        /// </summary>
        /// <remarks>
        /// This property is an example of an indexer. If dg is a DependencyGraph, you would
        /// invoke it like this:
        /// dg["a"]
        /// </remarks>
        /// <param name="s">String(cell) for which dependee want to know about the size</param>
        /// <returns>The size of dependees("a")</returns>
        public int this[string s]
        {
            get
            {
                if (dependees.TryGetValue(s, out HashSet<string> rtn)) { return rtn.Count; }
                return 0;
            }
        }


        /// <summary>
        /// Reports whether dependent(s) is non-empty.
        /// </summary>
        /// <remarks>
        /// True - If the given string(cell) has the dependent(s).
        /// False - If the given string(cell) has NO dependent(s).
        /// </remarks>
        /// <param name="s">String(cell) for which dependee want to know whether it has dependent(s)</param>
        /// <returns>True is given cell has dependent(s)</returns>
        public bool HasDependents(string s)
        {
            return GetDependents(s).GetEnumerator().MoveNext();
        }


        /// <summary>
        /// Reports whether dependee(s) is non-empty.
        /// </summary>
        /// <remarks>
        /// True - If the given string(cell) has the dependee(s).
        /// False - If the given string(cell) has NO dependee(s).
        /// </remarks>
        /// <param name="s">String(cell) for which dependent want to know whether it has dependee(s)</param>
        /// <returns>True is given cell has dependee(s)</returns>
        public bool HasDependees(string s)
        {
            return GetDependees(s).GetEnumerator().MoveNext();

        }


        /// <summary>
        /// Enumerates dependent(s).
        /// </summary>
        /// <remarks>
        /// Enumerates dependent(s) of the specified string (cell).
        /// Returns an IEnumerable containing dependents of the given string (cell).
        /// </remarks>
        /// <param name="s">String(cell) for which dependents are to be enumerated.</param>
        /// <returns>IEnumerable<string> of dependents for the given cell.</returns>
        public IEnumerable<string> GetDependents(string s)
        {
            try
            {
                return dependents[s];
            }
            catch (Exception)
            {
                return new HashSet<String>();
            }
        }


        /// <summary>
        /// Enumerates dependee(s).
        /// </summary>
        /// <remarks>
        /// Enumerates dependee(s) of the specified string (cell).
        /// Returns an IEnumerable containing dependees of the given string (cell).
        /// </remarks>
        /// <param name="s">String(cell) for which dependees are to be enumerated.</param>
        /// <returns>IEnumerable<string> of dependees for the given cell.</returns>
        public IEnumerable<string> GetDependees(string s)
        {
            try
            {
                return dependees[s];
            }
            catch (Exception)
            {
                return new HashSet<String>();
            }
        }


        /// <summary>
        /// Add new pair of dependency to DependencyGraph.
        /// </summary>
        /// <remarks>
        /// <para>Adds the ordered pair (s,t), if it doesn't exist</para>
        /// <para>
        /// This should be thought of as:
        ///     t depends on s.
        /// </para>
        /// </remarks>
        /// <param name="s"> s must be evaluated first. T depends on S</param>
        /// <param name="t"> t cannot be evaluated until s is</param>
        public void AddDependency(string s, string t)
        {
            if (!dependents.ContainsKey(s)) dependents.Add(s, new HashSet<string>());
            if (!dependees.ContainsKey(s)) dependees.Add(s, new HashSet<string>());
            if (!dependents.ContainsKey(t)) dependents.Add(t, new HashSet<string>());
            if (!dependees.ContainsKey(t)) dependees.Add(t, new HashSet<string>());

            dependents[s].Add(t);
            dependees[t].Add(s);
        }


        /// <summary>
        /// Removes the ordered pair (s,t), if it exists
        /// </summary>
        /// <remarks>
        /// s and t have to be pair.
        /// For example, you can't remove s and t each from other pairs. They must
        /// be in that ordered pair.
        /// </remarks>
        /// <param name="s">Dependee that user want to remove</param>
        /// <param name="t">Dependent that user want to remove</param>
        public void RemoveDependency(string s, string t)
        {
            if (dependents.ContainsKey(s) && dependents[s] != null)
                dependents[s].Remove(t);
            if (dependees.ContainsKey(s) && dependees[s] != null)
                dependees[t].Remove(s);
        }


        /// <summary>
        /// Replace the dependents of s to newDependents.
        /// </summary>
        /// <remarks>
        /// Removes all existing ordered pairs of the form (s,-). Then, for each
        /// t in newDependents, adds the ordered pair (s,t).
        /// </remarks>
        /// <param name="s">Dependee that you want to select</param>
        /// <param name="newDependents">Dependents that you want to replace</param>
        public void ReplaceDependents(string s, IEnumerable<string> newDependents)
        {
            foreach (String r in GetDependents(s))
            {
                RemoveDependency(s, r);
            }
            foreach (String t in newDependents)
            {
                AddDependency(s, t);
            }
        }


        /// <summary>
        /// Replace the dependees of s to newDependees.
        /// </summary>
        /// <remarks>
        /// Removes all existing ordered pairs of the form (-,s). Then, for each 
        /// t in newDependees, adds the ordered pair (t,s).
        /// </remarks>
        /// <param name="s">Dependent that you want to select</param>
        /// <param name="newDependees">Dependees that you want to replace</param>
        public void ReplaceDependees(string s, IEnumerable<string> newDependees)
        {
            foreach (String r in GetDependees(s))
            {
                RemoveDependency(r, s);
            }
            foreach (String t in newDependees)
            {
                AddDependency(t, s);
            }
        }
    }
}
