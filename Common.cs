﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPR381Project.Tokens;

namespace LPR381Project.Common
{
    /// <summary>
    /// This enum represents supported constraint types.
    ///
    /// <para>NOTE: Do not use this enum directly. Retrieve enum variants with the <see cref="Sign"/> class instead.</para>
    /// <para>This enum is used for specifying various types of constraints, including:</para>
    ///
    /// <list type="bullet">
    /// <item><description>Lesser: Represents a lesser than condition</description></item>
    /// <item><description>LesserEq: Represents a lesser than or equal to condition</description></item>
    /// <item><description>Greater: Represents a greater than condition</description></item>
    /// <item><description>GreaterEq: Represents a greater than or equal to condition</description></item>
    /// <item><description>Equal: Represents an equal to condition</description></item>
    /// <item><description>Urs: Represents a custom or special condition</description></item>
    /// </list>
    /// </summary>
    internal enum ConstraintEnum
    {
        LesserEq,
        GreaterEq,
        Equal,
        Bin,
        Int,
        NonNegative,
    }

    /// <summary>
    /// This class is used to get enum variants from <see cref="ConstraintEnum"/>
    /// </summary>
    internal class Constraint
    {
        public static ConstraintEnum LesserEq => ConstraintEnum.LesserEq;
        public static ConstraintEnum GreaterEq => ConstraintEnum.GreaterEq;
        public static ConstraintEnum Equal => ConstraintEnum.Equal;
        public static ConstraintEnum Bin => ConstraintEnum.Bin;
        public static ConstraintEnum Int => ConstraintEnum.Int;
        public static ConstraintEnum NonNegative => ConstraintEnum.NonNegative;
    }

    internal enum ProblemKindEnum
    {
        Min,
        Max,
    }

    internal class ProblemKind
    {
        public static ProblemKindEnum Min => ProblemKindEnum.Min;
        public static ProblemKindEnum Max => ProblemKindEnum.Max;
    }

    /// <summary>
    /// Class representing a tableau
    /// </summary>
    internal class Tableau
    {
        public ProblemKindEnum Kind { get; set; }

        // Dynamically sized matrix
        public List<List<double>> table;
        public Dictionary<string, ConstraintEnum> constraints;
        public Dictionary<string, int> varToIdx;
        public Dictionary<int, string> colToVar;
        public int ConCount { get; set; }

        public Tableau(List<Token> tokens)
        {
            List<List<Token>> problemTokens = new();
            this.ConCount = 1;

            int subListPtr = 0;
            problemTokens.Add(new List<Token>());
            foreach (Token token in tokens)
            {
                if (token.Kind != TokenKind.NewLine)
                {
                    problemTokens[subListPtr].Add(token);
                }
                else
                {
                    problemTokens.Add(new List<Token>());
                    subListPtr++;
                }
            }

            this.Kind = Tableau.GetObjectiveKind(problemTokens[0][0]);
            this.table = new List<List<double>>();

            int ptr = 0;
            this.table.Add(new List<double>());
            this.constraints = new Dictionary<string, ConstraintEnum>();
            foreach (Token token in problemTokens[0])
            {
                if (ptr == 0)
                {
                    ptr++;
                    continue;
                }

                this.constraints.Add($"x{ptr}", GetRestriction(problemTokens[^1][ptr - 1]));
                this.table[0].Add(double.Parse(token.Value));
                ptr++;
            }

            if (this.Kind == ProblemKind.Max)
            {
                for (int i = 0; i < this.table[0].Count; i++)
                {
                    this.table[0][i] *= -1;
                }
            }

            this.table[0].Add(0.0); // adding rhs value

            // skip index 0 and n-1
            for (int i = 1; i < problemTokens.Count - 1; i++)
            {
                // this.table.Add(new List<double>());
                List<double> nums = new();
                ConstraintEnum sign = Constraint.Equal; // default
                foreach (Token token in problemTokens[i])
                {
                    if (token.Kind != TokenKind.Number)
                    {
                        //this.constraints.Add(ConCount.ToString(), Tableau.GetConstraint(token));
                        //this.ConCount++;
                        sign = Tableau.GetConstraint(token);
                        continue;
                    }

                    nums.Add(double.Parse(token.Value));
                }

                this.AddConstraint(nums, sign);
            }

            // int numVars = this.CountVars();
            //
            // for (int i = 0; i < numVars; i++)
            // {
            //     if (this.constraints[$"x{i + 1}"] == ConstraintEnum.Bin)
            //     {
            //         List<double> numTmp = new();
            //         for (int var = 0; var < numVars; var++)
            //         {
            //             if (i == var)
            //             {
            //                 numTmp.Add(1.0);
            //             }
            //             else
            //             {
            //                 numTmp.Add(0.0);
            //             }
            //         }
            //
            //         numTmp.Add(1.0);
            //
            //         this.AddConstraint(numTmp, Constraint.LesserEq);
            //     }
            // }

            this.colToVar = new Dictionary<int, string>();
            this.varToIdx = new Dictionary<string, int>();

            int numVars = this.CountVars();
            int numCons = this.CountCons();
            int numVarCon = numVars + numCons;

            int varCount = 0;
            int conCount = 0;

            for (int col = 0; col < numVarCon; col++)
            {
                if (col < numVars)
                {
                    varCount++;
                    this.colToVar.Add(col, $"x{varCount}");
                    this.varToIdx.Add($"x{varCount}", col);
                }
                else
                {
                    conCount++;
                    this.colToVar.Add(col, $"con{conCount}");
                    this.varToIdx.Add($"con{conCount}", col);
                }
            }
        }

        private static ProblemKindEnum GetObjectiveKind(Token token)
        {
            return (token.Kind == TokenKind.Min) ? ProblemKindEnum.Min : ProblemKindEnum.Max;
        }

        private static ConstraintEnum GetRestriction(Token token)
        {
            return token.Kind switch
            {
                TokenKindEnum.Bin => Constraint.Bin,
                TokenKindEnum.Int => Constraint.Int,
                _ => Constraint.NonNegative,
            };
        }

        private static ConstraintEnum GetConstraint(Token token)
        {
            return token.Kind switch
            {
                TokenKindEnum.LessEq => Constraint.LesserEq,
                TokenKindEnum.GreaterEq => Constraint.GreaterEq,
                _ => Constraint.Equal,
            };
        }

        /// <summary>
        /// Add objective function to the tableau.
        /// </summary>
        /// <param name="nums">Numbers representign the objective function</param>
        public void AddObjective(List<double> nums)
        {
            // HACK: might not be the best way to do this
            if (this.table.Count != 0)
            {
                Console.Error.WriteLine(
                    "Error: Tableau already has elements cannot add another objective function"
                );
                Environment.Exit(1);
            }

            this.table.Add(nums);
        }

        /// <summary>
        /// Add constraint to the tableau.
        ///
        /// NOTE: this should only be used when creating the tableau
        /// </summary>
        /// <param name="nums">Numbers representing the constraint</param>
        /// <param name="sign">Sign of the constraint</param>
        public void AddConstraint(List<double> nums, ConstraintEnum sign)
        {
            // TODO: discuss if constraints should be stored as "con n" or just "n"

            // Check for "normal" amount of constraints
            if (this.constraints.Count > 100)
            {
                // lets be reasonable
                Console.Error.WriteLine("Error: Cannot add more than 100 constraints.");
                Environment.Exit(1);
            }

            this.constraints.Add(this.ConCount.ToString(), sign);

            // adds new constraint column to other rows
            int tableSize = this.table.Count;
            for (int i = 0; i < tableSize; i++)
            {
                // insert before rhs value
                this.table[i].Insert(this.table[i].Count - 1, 0.0);
            }

            // make constraint len match other rows
            int matchLen = this.table[0].Count;
            // number or other constraints (therefore -1 for how many to account for)
            int diff = matchLen - nums.Count;
            for (int i = 0; i < diff - 1; i++)
            {
                nums.Insert(nums.Count - 1, 0.0);
            }

            // if e constraint
            if (this.constraints[this.ConCount.ToString()] == ConstraintEnum.GreaterEq)
            {
                for (int i = 0; i < nums.Count; i++)
                {
                    nums[i] *= -1;
                }
            }
            this.ConCount++;

            nums.Insert(nums.Count - 1, 1.0);

            this.table.Add(nums);
        }

        // HACK: is adding the restrictions one at a time the best?
        /// <summary>
        /// Add sign restriction for decision variables
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="restriction"></param>
        public void AddRestriction(string variable, ConstraintEnum restriction)
        {
            if (this.constraints.ContainsKey(variable))
            {
                Console.Error.WriteLine($"Error: Restriction already set for {variable}.");
            }
            this.constraints.Add(variable, restriction);
        }

        /// <summary>
        /// if returned value is -1 then solution is optimal (probably)
        /// </summary>
        /// <returns></returns>
        public int GetPivotCol()
        {
            int pivotCol = 0;
            double val = 0;

            if (this.Kind == ProblemKind.Max)
            {
                bool hasNegative = false;
                for (int i = 0; i < this.table.Count - 1; i++)
                {
                    if (this.table[0][i] < val)
                    {
                        hasNegative = true;
                        val = this.table[0][i];
                        pivotCol = i;
                    }
                }

                if (!hasNegative)
                {
                    return -1;
                }

                return pivotCol;
            }
            else
            {
                for (int i = 0; i < this.table.Count - 1; i++)
                {
                    if (this.table[0][i] > val)
                    {
                        val = this.table[0][i];
                        pivotCol = i;
                    }
                }

                return pivotCol;
            }
        }

        public int GetPivotRow(int pivotCol)
        {
            // only 1 .. len(table)-1 are valid
            int pivotRow = 1;
            double val;
            List<double> ratios = new();

            if (this.Kind == ProblemKind.Max)
            {
                for (int i = 1; i < this.table.Count; i++)
                {
                    double a = this.table[i][^1];
                    double b = this.table[i][pivotCol];
                    double c = a / b;

                    if (Double.IsInfinity(c))
                    {
                        ratios.Add(Double.NaN);
                    }
                    else
                    {
                        ratios.Add(c);
                    }
                }
            }

            val = ratios.Max() + 1;

            for (int i = 0; i < ratios.Count; i++)
            {
                if (ratios[i] < 0)
                {
                    continue;
                }

                if (ratios[i] < val)
                {
                    val = ratios[i];
                    pivotRow = i + 1;
                }
            }

            if (val == 0)
            {
                return -1;
            }

            return pivotRow;
        }

        public int CountVars()
        {
            return this.table[0].Count - this.table.Count;
        }

        public int CountCons()
        {
            return this.table.Count - 1;
        }

        public int LenRows()
        {
            return this.table[0].Count;
        }

        public bool IsBasic(int colIndex)
        {
            int len = this.table.Count;

            int ones = 0;

            for (int row = 0; row < len; row++)
            {
                var val = this.table[row][colIndex];

                if (val == 0.0)
                {
                    continue;
                }
                else if (val == 1.0)
                {
                    if (ones >= 1)
                    {
                        return false;
                    }

                    ones += 1;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        public (int, int) GetSize()
        {
            int rows = this.table.Count;
            int cols = this.table[0].Count;

            return (rows, cols);
        }

        public void PrintTable(int iteration)
        {
            var headers = new List<string>();

            if (iteration == 0)
            {
                headers.Add("T-i");
            }
            else
            {
                headers.Add($"T-{iteration}");
            }

            int numDecisionVar = this.CountVars();
            int numConstraints = this.CountCons();

            for (int i = 0; i < numDecisionVar; i++)
            {
                headers.Add($"x{i + 1}");
            }

            for (int i = 0; i < numConstraints; i++)
            {
                headers.Add($"con{i + 1}");
            }

            headers.Add("RHS");

            foreach (var header in headers)
            {
                Console.Write(header.PadLeft(8));
                Console.Write(" ");
            }

            Console.WriteLine();

            var rows = new List<string>();
            rows.Add("z");

            for (int i = 0; i < numDecisionVar; i++)
            {
                rows.Add($"con{i + 1}");
            }

            for (var i = 0; i < this.table.Count; i++)
            {
                Console.Write(rows[i].PadLeft(8));
                Console.Write(" ");
                foreach (var num in this.table[i])
                {
                    Console.Write(num.ToString().PadLeft(8));
                    Console.Write(" ");
                }

                Console.WriteLine();
            }

            Console.WriteLine();
        }

        public void WriteTable(int iteration)
        {
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var outputFile = Path.Combine(desktopPath, "output.txt");

            try
            {
                using (StreamWriter writer = new StreamWriter(outputFile, true, Encoding.UTF8))
                {
                    int padding = 10;
                    var headers = new List<string>();

                    // Tableau iteration
                    if (iteration == 0)
                    {
                        headers.Add("T-i");
                    }
                    else
                    {
                        headers.Add($"T-{iteration}");
                    }

                    // get number of variables and constrainsts
                    int numDecisionVar = this.CountVars();
                    int numConstraints = this.CountCons();

                    // add decision variables to column headers
                    for (int i = 0; i < numDecisionVar; i++)
                    {
                        headers.Add($"x{i + 1}");
                    }

                    // add constraints to column headers
                    for (int i = 0; i < numConstraints; i++)
                    {
                        headers.Add($"con{i + 1}");
                    }

                    headers.Add("RHS");

                    foreach (var header in headers)
                    {
                        writer.Write(header.PadLeft(padding));
                        writer.Write(" ");
                    }

                    writer.WriteLine();

                    var rows = new List<string>();
                    rows.Add("z");

                    // add constraints to row header
                    for (int i = 0; i < numConstraints; i++)
                    {
                        rows.Add($"con{i + 1}");
                    }

                    for (var i = 0; i < this.table.Count; i++)
                    {
                        writer.Write(rows[i].PadLeft(padding));
                        writer.Write(" ");
                        foreach (var num in this.table[i])
                        {
                            writer.Write(Math.Round(num, 3).ToString().PadLeft(padding));
                            writer.Write(" ");
                        }

                        writer.WriteLine();
                    }

                    writer.WriteLine();
                }
            }
            catch (Exception err)
            {
                Console.WriteLine(err);
            }
        }
    }
}
