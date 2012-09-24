using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Five
{
    /// <summary>
    /// position shift
    /// </summary>
    struct Pos
    {
        public int dx;
        public int dy;
        public Pos(int adx, int ady)
        {
            dx = adx;
            dy = ady;
        }
    }
    /// <summary>
    /// What have been changed after move
    /// </summary>
    class Diff
    {
        public int movedNum;
        public int dx;
        public int dy;

        public Diff(int amovedNum, int adx, int ady)
        {
            dx = adx;
            dy = ady;
            movedNum = amovedNum;
        }
    }
    /// <summary>
    /// Main class of game field
    /// </summary>
    internal class Field : ISolving<Field>
    {
        /// <summary>
        /// Current state
        /// </summary>
        public byte[,] Positions = new byte[3,3] {{1, 2, 3}, {4, 5, 6}, {7, 8, 0}};
        /// <summary>
        /// Flag of empty state in positions
        /// </summary>
        public const int emptyCell = 0;
        /// <summary>
        /// 4 different ways of moving a square
        /// </summary>
        private static readonly int[,] directions = new int[4, 2] { { -1, 0 }, { 1, 0 }, { 0, -1 }, { 0, 1 } };

        /// <summary>
        /// Previous position (for solving)
        /// </summary>
        protected Field Prev;

        /// <summary>
        /// Returns a position of square with number num
        /// </summary>
        /// <param name="num">Piece number </param>
        /// <returns>Tupple x,y</returns>
        protected Tuple<int,int> GetPosByNum(int num)
        {
            Tuple<int, int> res = null;
            Traverse((i, j, el) =>
                         {
                             if (el==num)
                                 res = new Tuple<int, int>(i,j);
                             return el == num;
                         });
            return res;
        }
        /// <summary>
        /// Walks thouth the whole array of pieces and runs a function for each piece
        /// </summary>
        /// <param name="func">Function that looks like «(x,y,element)». Returns true if you need to stop iteration, false otherwise </param>
        /// <returns>True is function was stoped by delegate, false otherwise</returns>
        public bool Traverse(Func<int, int, byte, bool> func)
        {
            bool isExit;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {

                    isExit = func(j, i, Positions[i, j]);
                    if (isExit) return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Walks thouth the whole array of pieces and runs a procedure for each piece
        /// </summary>
        /// <param name="act">Procedure that looks like «(x,y,element)». </param>
        public void Traverse(Action<int, int, byte> act)
        {
            Traverse((i, j, el) =>
                         {
                             act(i, j, el);
                             return false;
                         });
        }

        /// <summary>
        /// Compare two Fields
        /// </summary>
        /// <returns>True, if positions are equal</returns>
        public bool Equals(Field p)
        {
            // If parameter is null return false:
            if ((object)p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return !p.Traverse((i, j, el) =>Positions[j,i]!=el);
        }
        
        /// <summary>
        /// Moves piece with number clickedNum
        /// </summary>
        /// <param name="clickedNum">A piece to move</param>
        /// <returns>new field or null, if move can't be done</returns>
        public Field MovePiece(int clickedNum)
        {
            var clickPos = GetPosByNum(clickedNum); //Position of clicked piece
            var emptyPos = GetPosByNum(emptyCell); //Position of empty cell
            if (clickPos == null || emptyPos == null)
                return null; 
            var dx = clickPos.Item1 - emptyPos.Item1; //offset of 2 positions
            var dy = clickPos.Item2 - emptyPos.Item2;
            if (dx*dy==0 && (Math.Abs(dx)==1 || Math.Abs(dy)==1)) //1 should be zero and other is -1 or 1
                return Swap(dx, dy); //return swapped field
            return null;
        }
        /// <summary>
        /// Get difference of two fields. Returns a piece, that have been moved
        /// </summary>
        /// <param name="a">Previous field</param>
        /// <param name="b">New field</param>
        /// <returns>Difference or null, if there is no path beetween two fields</returns>
        public static Diff GetDifference(Field a, Field b)
        {
            var diffCount = 0;
            a.Traverse((i, j, el) => diffCount += el != b.Positions[j, i] ? 1 : 0); //get count of pieces, whose positions differ from each other
            if (diffCount != 2) // if not 2, than there is no path beetween 2 fields
                return null;
            var emptyPosA = a.GetPosByNum(emptyCell); //same as in MovePiece function
            var emptyPosB = b.GetPosByNum(emptyCell);
            if (emptyPosA == null || emptyPosB == null)
                return null;
            var dx = emptyPosA.Item1 - emptyPosB.Item1;
            var dy = emptyPosA.Item2 - emptyPosB.Item2;
            if (dx * dy == 0 && (Math.Abs(dx) == 1 || Math.Abs(dy) == 1))
                return new Diff(b.Positions[emptyPosA.Item2, emptyPosA.Item1],dx,dy);
            return null; 
        }
        /// <summary>
        /// Moves empty place.
        /// </summary>
        /// <param name="dx">X offset of empty place</param>
        /// <param name="dy">Y offset of empty place</param>
        /// <returns>new Filed or null, if move can't be done</returns>
        public Field Swap(int dx, int dy)
        {
            if (dx * dy != 0) return null;
            Field res = null;
            Traverse((i, j, el) =>
                         {
                             if (el != emptyCell) return false; //find empty cell
                             i += dx;
                             j += dy;
                             if (j < 0 || j >= 3 || i < 0 || i >= 3) //check bounds
                                 return true;
                             res = new Field(this);
                             res.Positions[j - dy, i - dx] = Positions[j, i]; //swap
                             res.Positions[j, i] = Positions[j - dy, i - dx];
                             return true; //exit traverse

                         });
            return res;
        }
        /// <summary>
        /// Generates random field
        /// </summary>
        /// <returns>Random field</returns>
        public static Field RandomField()
        {
            var field = new Field();
            Random rand = new Random();
            //Move pieces 100 times
            for (int n = 100; n > 0; n--)
            {
                    //Get possible move
                    int pos = rand.Next(directions.GetLength(0));
                    //try swap
                    Field swapped = field.Swap(directions[pos, 0], directions[pos, 1]);
                    if (swapped == null) continue; //if swapping failed - repeat it.
                    field = swapped;
            }
            return field;
        }

        private Field()
        {
        }

        private Field(Field cop)
        {
            cop.Traverse((i, j, el) =>
                             {
                                 Positions[j, i] = el; //copy positions
                             });
        }

        /// <summary>
        /// Get count of pieces on right places. Skips empty space. (max 8)
        /// </summary>
        /// <returns>Count of pieces on right places</returns>
        public int RightPosCount()
        {
            var res = 0;
            Traverse((i, j, el) =>
                         {
                             if (el == emptyCell) return;
                             res += (el == (i + j*3) + 1) ? 1 : 0; //calc element on position i,j and compare to element on this position
                         });
            return res;
        }
        
        /// <summary>
        /// returns square of values (0 - empty place)
        /// </summary>
        /// <returns></returns>
        public override String ToString()
        {
            string s = "";
            Traverse((i, j, el) =>
                         {
                             if (i == 0 && j != 0)
                                 s += "\n";
                             if (el != emptyCell)
                                 s += el;
                             else
                                 s += "0";
                                            
                             if (i != 2)
                                 s += " ";
                         });
            return s;
        }
        /// <summary>
        /// Finds out is current position right or not.
        /// </summary>
        /// <returns>True, if all pieces are on their places</returns>
        public bool IsFin()
        {
            return RightPosCount() == 8;
        }

        /// <summary>
        /// Get possible move from current state
        /// </summary>
        /// <returns>List of moves</returns>
        public List<Field> GetPaths()
        {
            var res = new List<Field>();
      
            for (int i = 0; i < directions.GetLength(0); i++)
            {
                var dx = directions[i, 0];
                var dy = directions[i, 1];
                Field swapped = Swap(dx, dy); //try swap
                if (swapped != null)
                {
                    //can swap this direction
                    swapped.Prev = this;
                    res.Add(swapped);
                }
            }
            return res;
        }
        /// <summary>
        /// Returns prevoius state (for solving)
        /// </summary>
        public Field GetPrev()
        {
            return Prev;
        }
        /// <summary>
        /// Calcs pieces that are not on their places
        /// </summary>
        public int GetHeuristics1()
        {
            return 8-RightPosCount();
        }
        /// <summary>
        /// Calcs Manhattan lenth heuristics. Sums paths of each piece to reach their right place.
        /// </summary>
        public int GetHeuristics2()
        {
            var res = 0;
                Traverse((i, j, el) =>
                         {
                             if (el == emptyCell) return;
                             int realI = (el-1) % 3; 
                             int realJ = (el-1) / 3;
                             res += Math.Abs(realI - i) + Math.Abs(realJ - j);
                         });
            return res;
        }

        /// <summary>
        /// Gets hash code, based on positions.
        /// </summary>
        public override int GetHashCode()
        {
            var res = 0;
            int n = 1;
            Traverse((i, j, el) =>
                         {
                             if (el == emptyCell)
                                 el = 0;
                             res += el*n;
                             n*=10;
                         });
            return res;
        }
    }
}
