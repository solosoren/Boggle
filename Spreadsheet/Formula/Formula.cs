// Skeleton written by Joe Zachary for CS 3500, January 2017
// Kunaal Kumar
// u1063137
// January 24, 2018.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Formulas
{
    /// <summary>
    /// Represents formulas written in standard infix notation using standard precedence
    /// rules.  Provides a means to evaluate Formulas.  Formulas can be composed of
    /// non-negative floating-point numbers, variables, left and right parentheses, and
    /// the four binary operator symbols +, -, *, and /.  (The unary operators + and -
    /// are not allowed.)
    /// </summary>
    public class Formula
    {
        private string formula;

        // This stack consists of the numbers from the formula.
        private Stack<double> valueStack;

        // This stack consists of the operators from the formula.
        private Stack<string> operatorStack;

        /// <summary>
        /// Creates a Formula from a string that consists of a standard infix expression composed
        /// from non-negative floating-point numbers (using C#-like syntax for double/int literals),
        /// variable symbols (a letter followed by zero or more letters and/or digits), left and right
        /// parentheses, and the four binary operator symbols +, -, *, and /.  White space is
        /// permitted between tokens, but is not required.
        ///
        /// Examples of a valid parameter to this constructor are:
        ///     "2.5e9 + x5 / 17"
        ///     "(5 * 2) + 8"
        ///     "x*y-2+35/9"
        ///
        /// Examples of invalid parameters are:
        ///     "_"
        ///     "-5.3"
        ///     "2 5 + 3"
        ///
        /// If the formula is syntacticaly invalid, throws a FormulaFormatException with an
        /// explanatory Message.
        /// </summary>
        public Formula(String formula)
        {
            if (formula.Length < 1)
            {
                throw new FormulaFormatException("Formula is empty.");
            }

            if (!ParenthesisEval(formula))
            {
                throw new FormulaFormatException("Formula format is invalid.");
            }

            operatorStack = new Stack<string>();
            valueStack = new Stack<double>();
            this.formula = formula;
        }

        private bool ParenthesisEval(string formula)
        {
            int open = 0;
            int close = 0;
            char firstChar = formula[0];
            List<string> operators = new List<string> {"+", "-", "*", "/"};
            char lastChar = formula[formula.Length - 1];
            string lastToken = "-1";
            if (!Char.IsLetter(firstChar))
            {
                if (!int.TryParse(firstChar.ToString(), out int tempFirstChar))
                {
                    if (!firstChar.Equals('('))
                    {
                        return false;
                    }
                }
            }

            if (!Char.IsLetter(lastChar))
            {
                if (!int.TryParse(lastChar.ToString(), out int tempLastChar))
                {
                    if (!lastChar.Equals(')'))
                    {
                        return false;
                    }
                }
            }

            foreach (string token in GetTokens(formula))
            {
                if (!lastToken.Equals("-1"))
                {
                    if (lastToken.Equals("(") || operators.Contains(lastToken))
                    {
                        if (!int.TryParse(token, out int tempIntNext))
                        {
                            if (!token.Equals("("))
                            {
                                if (!Char.IsLetter(token.ToCharArray()[0]))
                                    return false;
                            }
                        }
                    }

                    if (lastToken.Equals(")") || Char.IsLetter(lastToken.ToCharArray()[0]) ||
                        int.TryParse(lastToken, out int tempLastToken))
                    {
                        if (!operators.Contains(token))
                        {
                            if (!token.Equals(")"))
                            {
                                return false;
                            }
                        }
                    }
                }


                if (close > open)
                {
                    return false;
                }

                if (token.Equals("(")) open++;
                if (token.Equals(")")) close++;
                lastToken = token;
            }

            return open == close;
        }

        /// <summary>
        /// Evaluates this Formula, using the Lookup delegate to determine the values of variables.  (The
        /// delegate takes a variable name as a parameter and returns its value (if it has one) or throws
        /// an UndefinedVariableException (otherwise).  Uses the standard precedence rules when doing the evaluation.
        ///
        /// If no undefined variables or divisions by zero are encountered when evaluating
        /// this Formula, its value is returned.  Otherwise, throws a FormulaEvaluationException
        /// with an explanatory Message.
        /// </summary>
        public double Evaluate(Lookup lookup)
        {
            double number;
            foreach (string var in GetTokens(formula))
            {
                if (Double.TryParse(var, out number))
                {
                    if (operatorStack.Count != 0)
                    {
                        if (operatorStack.Peek().Equals("*") ||
                            operatorStack.Peek().Equals("/"))
                        {
                            valueStack.Push(PopOpStackForSolution(valueStack.Pop(), number));
                        }
                        else
                        {
                            valueStack.Push(number);
                        }
                    }

                    else
                    {
                        valueStack.Push(number);
                    }
                }
                else if (var.Equals("+") || var.Equals("-"))
                {
                    if (operatorStack.Count != 0)
                    {
                        if (operatorStack.Peek().Equals("+") ||
                            operatorStack.Peek().Equals("-"))
                        {
                            valueStack.Push(PopOpStackForSolution(valueStack.Pop(), valueStack.Pop()));
                        }
                    }


                    operatorStack.Push(var);
                }
                else if (var.Equals("*") || var.Equals("/"))
                {
                    operatorStack.Push(var);
                }
                else if (var.Equals("("))
                {
                    operatorStack.Push(var);
                }
                else if (var.Equals(")"))
                {
                    if (operatorStack.Count != 0)
                    {
                        if (operatorStack.Peek().Equals("+") ||
                            operatorStack.Peek().Equals("-"))
                        {
                            valueStack.Push(PopOpStackForSolution(valueStack.Pop(), valueStack.Pop()));
                        }
                    }


                    operatorStack.Pop();

                    if (operatorStack.Count != 0)
                    {
                        if (operatorStack.Peek().Equals("*") ||
                            operatorStack.Peek().Equals("/"))
                        {
                            valueStack.Push(PopOpStackForSolution(valueStack.Pop(), valueStack.Pop()));
                        }
                    }
                }
                else if (Char.IsLetter(var.ToCharArray()[0]))
                {
                    if (operatorStack.Count != 0)
                    {
                        if (operatorStack.Peek().Equals("*") ||
                            operatorStack.Peek().Equals("/"))
                        {
                            try
                            {
                                valueStack.Push(PopOpStackForSolution(valueStack.Pop(), lookup.Invoke(var)));
                            }
                            catch (UndefinedVariableException e)
                            {
                                Console.WriteLine(e);
                                throw;
                            }
                        }
                        else
                        {
                            try
                            {
                                valueStack.Push(lookup.Invoke(var));
                            }
                            catch (UndefinedVariableException e)
                            {
                                Console.WriteLine(e);
                                throw;
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            valueStack.Push(lookup.Invoke(var));
                        }
                        catch (UndefinedVariableException e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }
                    }
                }
            }

            // This means that the operator stack is empty
            if (operatorStack.Count == 0)
            {
                return valueStack.Pop();
            }

            // ... else operator stack isn't empty
            return PopOpStackForSolution(valueStack.Pop(), valueStack.Pop());
        }

        /// <summary>
        /// Pops the operator stack and applies popped operator to the two given values and returns the result.
        /// Throws exception if operator is not +, -, * or /.
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private double PopOpStackForSolution(double val1, double val2)
        {
            switch (operatorStack.Pop())
            {
                case "+": return val1 + val2;
                case "-": return val1 - val2;
                case "*": return val1 * val2;
                case "/":
                    if (val2 == 0)
                        throw new FormulaEvaluationException("Can't divide by 0");
                    return val1 / val2;
                default: throw new Exception("Invalid operator");
            }
        }

        /// <summary>
        /// Given a formula, enumerates the tokens that compose it.  Tokens are left paren,
        /// right paren, one of the four operator symbols, a string consisting of a letter followed by
        /// zero or more digits and/or letters, a double literal, and anything that doesn't
        /// match one of those patterns.  There are no empty tokens, and no token contains white space.
        /// </summary>
        private static IEnumerable<string> GetTokens(String formula)
        {
            // Patterns for individual tokens.
            // NOTE:  These patterns are designed to be used to create a pattern to split a string into tokens.
            // For example, the opPattern will match any string that contains an operator symbol, such as
            // "abc+def".  If you want to use one of these patterns to match an entire string (e.g., make it so
            // the opPattern will match "+" but not "abc+def", you need to add ^ to the beginning of the pattern
            // and $ to the end (e.g., opPattern would need to be @"^[\+\-*/]$".)
            String lpPattern = @"\(";
            String rpPattern = @"\)";
            String opPattern = @"[\+\-*/]";
            String varPattern = @"[a-zA-Z][0-9a-zA-Z]*";

            // PLEASE NOTE:  I have added white space to this regex to make it more readable.
            // When the regex is used, it is necessary to include a parameter that says
            // embedded white space should be ignored.  See below for an example of this.
            String doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: e[\+-]?\d+)?";
            String spacePattern = @"\s+";

            // Overall pattern.  It contains embedded white space that must be ignored when
            // it is used.  See below for an example of this.  This pattern is useful for
            // splitting a string into tokens.
            String splittingPattern = String.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})",
                lpPattern, rpPattern, opPattern, varPattern, doublePattern, spacePattern);

            // Enumerate matching tokens that don't consist solely of white space.
            // PLEASE NOTE:  Notice the second parameter to Split, which says to ignore embedded white space
            /// in the pattern.
            foreach (String s in Regex.Split(formula, splittingPattern, RegexOptions.IgnorePatternWhitespace))
            {
                if (!Regex.IsMatch(s, @"^\s*$", RegexOptions.Singleline))
                {
                    yield return s;
                }
            }
        }
    }

    /// <summary>
    /// A Lookup method is one that maps some strings to double values.  Given a string,
    /// such a function can either return a double (meaning that the string maps to the
    /// double) or throw an UndefinedVariableException (meaning that the string is unmapped
    /// to a value. Exactly how a Lookup method decides which strings map to doubles and which
    /// don't is up to the implementation of the method.
    /// </summary>
    public delegate double Lookup(string var);

    /// <summary>
    /// Used to report that a Lookup delegate is unable to determine the value
    /// of a variable.
    /// </summary>
    [Serializable]
    public class UndefinedVariableException : Exception
    {
        /// <summary>
        /// Constructs an UndefinedVariableException containing whose message is the
        /// undefined variable.
        /// </summary>
        /// <param name="variable"></param>
        public UndefinedVariableException(String variable)
            : base(variable)
        {
        }
    }

    /// <summary>
    /// Used to report syntactic errors in the parameter to the Formula constructor.
    /// </summary>
    [Serializable]
    public class FormulaFormatException : Exception
    {
        /// <summary>
        /// Constructs a FormulaFormatException containing the explanatory message.
        /// </summary>
        public FormulaFormatException(String message) : base(message)
        {
        }
    }

    /// <summary>
    /// Used to report errors that occur when evaluating a Formula.
    /// </summary>
    [Serializable]
    public class FormulaEvaluationException : Exception
    {
        /// <summary>
        /// Constructs a FormulaEvaluationException containing the explanatory message.
        /// </summary>
        public FormulaEvaluationException(String message) : base(message)
        {
        }
    }
}