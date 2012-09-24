using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JMBucknall.Containers;

namespace Five
{
    interface ISolving<T>
    {
        bool IsFin();
        List<T> GetPaths();
        T GetPrev();
        bool Equals(T p);

    }
    class Solver<T> where T : class, ISolving<T>
    {
        /// <summary>
        /// Gets path from start node to last node, using GetPrev() function of node.
        /// </summary>
        /// <param name="fin">last node</param>
        protected static List<T> GetPathToFin(T fin)
        {
            var res = new List<T>();
            while (fin!=null)
            {
                res.Insert(0,fin);
                fin = fin.GetPrev();
            }
           
            return res;
        }
        /// <summary>
        /// Breadth-first search
        /// </summary>
        /// <param name="start"></param>
        /// <param name="pathsViewed"></param>
        /// <returns></returns>
        public static List<T> BFS(T start, out int pathsViewed)
        {
            var hashes = new Hashtable(); //hashes of seen nodes
            var queue = new Queue<T>(); //Queue of nodes to analyze
            queue.Enqueue(start); //add first node
            hashes.Add(start.GetHashCode(),start);
            pathsViewed = 0;
            while (queue.Count!=0)
            {
                var current = queue.Dequeue();
                pathsViewed++;
                if (current.IsFin())
                {
                    return GetPathToFin(current); //if finish have been found - return
                }
                //if (queue.Count > 50) continue;
                
                var paths = current.GetPaths(); //get different paths to go
                foreach (var path in paths)
                {
                    var hash = path.GetHashCode();
                    if (hashes.ContainsKey(hash)) //if path have been seen - skip it.
                        continue;
                    hashes.Add(hash, path); //add next node to queue
                    queue.Enqueue(path);  
                }

            }
            return null;
        }
        /// <summary>
        /// A* solving
        /// </summary>
        /// <param name="start">Start node</param>
        /// <param name="pathsViewed">count of visited paths</param>
        /// <param name="heuristics">Heuristics function. Takes node returns heuristics</param>
        /// <param name="cost">Cost function. Takes 2 nodes returns cost of moving beetween them</param>
        /// <returns></returns>
        public static List<T> AStar(T start, out int pathsViewed, Func<T,int> heuristics, Func<T,T,int> cost)
        {
            var hashes = new Hashtable();//hashes of seen nodes

            var dic = new PriorityQueue(); //priority queue, based on sum of heuristics and cost. Stores tuple<node,cost,heuristics>
            var newHeu = heuristics(start); //calc heuristics
            dic.Enqueue(new Tuple<T, int, int>(start,0,newHeu),int.MaxValue - newHeu ); //add node
            hashes.Add(start.GetHashCode(), start);
            pathsViewed = 0;
            while (dic.Count != 0)
            {
                Tuple<T, int, int> currentTuple = (Tuple<T, int, int>)dic.Dequeue(); //get node with biggest priority (smallest sum  of heuristics and cost)
                var current = currentTuple.Item1;
                var acost = currentTuple.Item2;
                var heu = currentTuple.Item3;
                pathsViewed++;
                if (current.IsFin())
                {
                    return GetPathToFin(current); //if finish have been found - return
                }
 
                var paths = current.GetPaths(); //get different paths to go
                foreach (var path in paths)
                {
                    var hash = path.GetHashCode();
                    if (hashes.ContainsKey(hash))
                        continue;
                    hashes.Add(hash, path);   //add next node to queue
                    newHeu = heuristics(path);
                    var newCost = cost(current, path);
                    dic.Enqueue(new Tuple<T, int, int>(path, acost + newCost, newHeu), int.MaxValue - (newHeu + acost + newCost));
                }

            }
            return null;
        }
    }
}
