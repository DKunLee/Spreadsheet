// Skeleton written by Joe Zachary for CS 3500, September 2013
// Read the entire skeleton carefully and completely before you
// do anything else!

// Version 1.1 (9/22/13 11:45 a.m.)

// Change log:
//  (Version 1.1) Repaired mistake in GetTokens
//  (Version 1.1) Changed specification of second constructor to
//                clarify description of how validation works

// (Daniel Kopta) 
// Version 1.2 (9/10/17) 

// Change log:
//  (Version 1.2) Changed the definition of equality with regards
//                to numeric tokens

/// <summary>
/// Author:    DK Lee
/// Partner:   None
/// Date:      Jan 27, 2024
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
/// This Formula object will store the expression to evaluate (e.g., "1+A2") along with any
/// other needed information, and can then be used to "re-evaluate" the expression whenever
/// necessary (e.g., when a dependee changes).
/// The key implementation in this Formula class is once you create the expression with
/// normalizer and validator, you can't edit or change variable. It means we could not directly
/// use this in spreadsheet project.
/// </summary>
/// <remarks>
/// <para>
/// Represents formulas written in standard infix notation using standard precedence
/// rules.  The allowed symbols are non-negative numbers written using double-precision 
/// floating-point syntax (without unary preceeding '-' or '+'); 
/// variables that consist of a letter or underscore followed by 
/// zero or more letters, underscores, or digits; parentheses; and the four operator 
/// symbols +, -, *, and /.
/// </para>
/// <para>
/// Spaces are significant only insofar that they delimit tokens.  For example, "xy" is
/// a single variable, "x y" consists of two variables "x" and y; "x23" is a single variable; 
/// and "x 23" consists of a variable "x" and a number "23".
/// </para>
/// <para>
/// Associated with every formula are two delegates:  a normalizer and a validator.  The
/// normalizer is used to convert variables into a canonical form, and the validator is used
/// to add extra restrictions on the validity of a variable (beyond the standard requirement 
/// that it consist of a letter or underscore followed by zero or more letters, underscores,
/// or digits.)  Their use is described in detail in the constructor and method comments.
/// </para>
/// <para>
///  The lookup function: The variable lookup function serves the same purpose as before
///                          and is provided at Evaluation time.
///
///  The Normalize function: This function takes in a string (representing a variable) and
///                          normalizes it.
///
///  The Validate function: This function will return true if the given variable name is
///                          valid for the context of the program.
/// </para>
/// </remarks>


using System.Text.RegularExpressions;

namespace SpreadsheetUtilities;
public class Formula
{
    private string correctFormula;
    private string[] operators = { "+", "-", "*", "/", "(", ")" };
    private FormulaError FEmessage;

    /// <summary>
    /// Creates a Formula from a string that consists of an infix expression written as
    /// described in the class comment.  If the expression is syntactically invalid,
    /// throws a FormulaFormatException with an explanatory Message.
    /// 
    /// The associated normalizer is the identity function, and the associated validator
    /// maps every string to true.  
    /// </summary>
    /// <param name="formula">Expression that you want to make</param>
    public Formula(String formula) : this(formula, s => s, s => true) { }

    /// <summary>
    /// Creates a Formula from a string that consists of an infix expression written as
    /// described in the class comment.  If the expression is syntactically incorrect,
    /// throws a FormulaFormatException with an explanatory Message.
    /// 
    /// The associated normalizer and validator are the second and third parameters,
    /// respectively.  
    /// 
    /// If the formula contains a variable v such that normalize(v) is not a legal variable, 
    /// throws a FormulaFormatException with an explanatory message. 
    /// 
    /// If the formula contains a variable v such that isValid(normalize(v)) is false,
    /// throws a FormulaFormatException with an explanatory message.
    /// 
    /// Suppose that N is a method that converts all the letters in a string to upper case, and
    /// that V is a method that returns true only if a string consists of one letter followed
    /// by one digit.  Then:
    /// 
    /// new Formula("x2+y3", N, V) should succeed
    /// new Formula("x+y3", N, V) should throw an exception, since V(N("x")) is false
    /// new Formula("2x+y3", N, V) should throw an exception, since "2x+y3" is syntactically incorrect.
    /// </summary>
    /// <param name="formula">Expression that you want to make</param>
    /// <param name="normalize">Provide normalize function</param>
    /// <param name="isValid">Provide validate function</param>
    public Formula(String formula, Func<string, string> normalize, Func<string, bool> isValid)
    {
        if (IsSyntacticallyCorrect(formula) == false)
            throw new FormulaFormatException("FormulaFormatException: the given formula is syntactically incorrect.");

        correctFormula = "";
        string rtnToken;
        foreach (string token in GetTokens(formula))
        {
            if (string.IsNullOrWhiteSpace(token)) continue;

            rtnToken = token.Trim();
            if (!operators.Contains(rtnToken))
            {
                rtnToken = normalize(token).Trim();
                if (double.TryParse(token, out _))
                {
                    correctFormula += rtnToken;
                }
                else if (!isValid(rtnToken))
                {
                    throw new FormulaFormatException("FormulaFormatException: the tokens are invalid.");
                }
                else if (!Regex.IsMatch(rtnToken, @"^[a-zA-Z_](?:[a-zA-Z_]*|\d*)$"))
                {
                    throw new FormulaFormatException("FormulaFormatException: one of the variable is invalid.");
                }
                else
                {
                    correctFormula += rtnToken;
                }
            }
            else
            {
                correctFormula += rtnToken;
            }
        }
    }

    /// <summary>
    /// Evaluates this Formula, using the lookup delegate to determine the values of
    /// variables.  When a variable symbol v needs to be determined, it should be looked up
    /// via lookup(normalize(v)). (Here, normalize is the normalizer that was passed to 
    /// the constructor.)
    /// 
    /// For example, if L("x") is 2, L("X") is 4, and N is a method that converts all the letters 
    /// in a string to upper case:
    /// 
    /// new Formula("x+7", N, s => true).Evaluate(L) is 11
    /// new Formula("x+7").Evaluate(L) is 9
    /// 
    /// Given a variable symbol as its parameter, lookup returns the variable's value 
    /// (if it has one) or throws an ArgumentException (otherwise).
    /// 
    /// If no undefined variables or divisions by zero are encountered when evaluating 
    /// this Formula, the value is returned.  Otherwise, a FormulaError is returned.  
    /// The Reason property of the FormulaError should have a meaningful explanation.
    ///
    /// This method should never throw an exception.
    /// </summary>
    /// <param name="lookup">Provide a lookup function</param>
    /// <returns>Result of the expression, or FormulaError if there is any error in formula</returns>
    public object Evaluate(Func<string, double> lookup)
    {
        Stack<object> vaStack = new();
        Stack<string> opStack = new();
        string token;

        foreach (string tk in GetTokens(correctFormula))
        {
            if (string.IsNullOrWhiteSpace(tk)) continue;
            token = tk.Trim();
            if (operators.Contains(token))
            {
                if (token.Equals("+") || token.Equals("-"))
                {
                    // If + and - on top of the operator
                    if (opStack.TryPeek(out string top) && (top.Equals("+") || top.Equals("-")))
                    {
                        if (!operationStep(vaStack, opStack)) return FEmessage;
                    }
                    opStack.Push(token);
                }
                else if (token.Equals(")"))
                {
                    // If + and - on top of the operator
                    if (opStack.TryPeek(out string pORm) && (pORm.Equals("+") || pORm.Equals("-")))
                    {
                        if (!operationStep(vaStack, opStack)) return FEmessage;
                    }

                    if (!opStack.Pop().Equals("("))
                        return new FormulaError("FormulaError: missing '(' operator.");

                    if (opStack.TryPeek(out string mORd) && (mORd.Equals("*") || mORd.Equals("/")))
                    {
                        if (!operationStep(vaStack, opStack)) return FEmessage;
                    }
                }
                else
                {
                    opStack.Push(token);
                }
            }
            else
            {
                if (!double.TryParse(token, out _))
                {
                    try
                    {
                        token = lookup(token).ToString();
                    }
                    catch
                    {
                        return new FormulaError("FormulaError: unknown variable.");
                    }
                }

                vaStack.Push(token);

                if (opStack.TryPeek(out string mORd) && (mORd.Equals("*") || mORd.Equals("/")))
                {
                    if (!operationStep(vaStack, opStack)) return FEmessage;
                }
            }
        }
        if (opStack.Count == 0)
        {
            if (vaStack.Count != 1) return new FormulaError("FormulaError: there is not exactly one value left.");
        }
        else
        {
            if (opStack.Count != 1) return new FormulaError("FormulaError: there is not exactly one operator left.");
            if (vaStack.Count != 2) return new FormulaError("FormulaError: there is not exactly two values left.");

            popVaStackTwiceAndCalculate(vaStack, opStack);
        }

        return double.Parse(vaStack.Pop().ToString());
    }

    /// <summary>
    /// Enumerates the normalized versions of all of the variables that occur in this 
    /// formula.  No normalization may appear more than once in the enumeration, even 
    /// if it appears more than once in this Formula.
    /// 
    /// For example, if N is a method that converts all the letters in a string to upper case:
    /// 
    /// new Formula("x+y*z", N, s => true).GetVariables() should enumerate "X", "Y", and "Z"
    /// new Formula("x+X*z", N, s => true).GetVariables() should enumerate "X" and "Z".
    /// new Formula("x+X*z").GetVariables() should enumerate "x", "X", and "z".
    /// </summary>
    /// <returns>IEnumerable variables in formula</returns>
    public IEnumerable<string> GetVariables()
    {
        HashSet<string> uniqueVariables = new HashSet<string>();
        foreach (string variable in GetTokens(correctFormula))
        {
            if (!double.TryParse(variable, out _) && !operators.Contains(variable))
            {
                if (!uniqueVariables.Contains(variable))
                {
                    yield return variable;
                }
                uniqueVariables.Add(variable);
            }
        }
    }


    /// <summary>
    /// Returns a string containing no spaces which, if passed to the Formula
    /// constructor, will produce a Formula f such that this.Equals(f).  All of the
    /// variables in the string should be normalized.
    /// 
    /// For example, if N is a method that converts all the letters in a string to upper case:
    /// 
    /// new Formula("x + y", N, s => true).ToString() should return "X+Y"
    /// new Formula("x + Y").ToString() should return "x+Y"
    /// </summary>
    /// <returns>The string type of expression</returns>
    public override string ToString() { return correctFormula; }

    /// <summary>
    ///  <change> make object nullable </change>
    ///
    /// If obj is null or obj is not a Formula, returns false.  Otherwise, reports
    /// whether or not this Formula and obj are equal.
    /// 
    /// Two Formulae are considered equal if they consist of the same tokens in the
    /// same order.  To determine token equality, all tokens are compared as strings 
    /// except for numeric tokens and variable tokens.
    /// Numeric tokens are considered equal if they are equal after being "normalized" 
    /// by C#'s standard conversion from string to double, then back to string. This 
    /// eliminates any inconsistencies due to limited floating point precision.
    /// Variable tokens are considered equal if their normalized forms are equal, as 
    /// defined by the provided normalizer.
    /// 
    /// For example, if N is a method that converts all the letters in a string to upper case:
    ///  
    /// new Formula("x1+y2", N, s => true).Equals(new Formula("X1  +  Y2")) is true
    /// new Formula("x1+y2").Equals(new Formula("X1+Y2")) is false
    /// new Formula("x1+y2").Equals(new Formula("y2+x1")) is false
    /// new Formula("2.0 + x7").Equals(new Formula("2.000 + x7")) is true
    /// </summary>
    /// <param name="obj">Have to be a Formula object or return false</param>
    /// <returns>Whether the given formula is equal to this(In expression wise)</returns>
    public override bool Equals(object? obj)
    {
        if (obj == null || !(obj is Formula))
            return false;

        var enumerator1 = GetTokens(correctFormula).GetEnumerator();
        var enumerator2 = GetTokens(((Formula)obj).ToString()).GetEnumerator();

        while (enumerator1.MoveNext() && enumerator2.MoveNext())
        {
            string token1 = enumerator1.Current;
            string token2 = enumerator2.Current;

            if (double.TryParse(token1, out double num1) && double.TryParse(token2, out double num2))
            {
                if (Math.Abs(num1 - num2) > double.Epsilon)
                    return false;
            }
            else if (!token1.Equals(token2))
            {
                return false;
            }
        }

        return !(enumerator1.MoveNext() || enumerator2.MoveNext());
    }

    /// <summary>
    ///   <change> We are now using Non-Nullable objects.  Thus neither f1 nor f2 can be null!</change>
    /// Reports whether f1 == f2, using the notion of equality from the Equals method.
    /// 
    /// </summary>
    /// <param name="f1">Formula you want to compare</param>
    /// <param name="f2">Formula you want to compare</param>
    /// <returns>True if they are equal, false if they are not equal</returns>
    public static bool operator ==(Formula f1, Formula f2)
    {
        if (ReferenceEquals(f1, f2))
            return true;

        return f1.Equals(f2);
    }

    /// <summary>
    ///   <change> We are now using Non-Nullable objects.  Thus neither f1 nor f2 can be null!</change>
    ///   <change> Note: != should almost always be not ==, if you get my meaning </change>
    ///   Reports whether f1 != f2, using the notion of equality from the Equals method.
    /// </summary>
    /// <param name="f1">Formula you want to compare</param>
    /// <param name="f2">Formula you want to compare</param>
    /// <returns>True if they are not equal, false if they are equal</returns>
    public static bool operator !=(Formula f1, Formula f2) { return !(f1 == f2); }

    /// <summary>
    /// Returns a hash code for this Formula.  If f1.Equals(f2), then it must be the
    /// case that f1.GetHashCode() == f2.GetHashCode().  Ideally, the probability that two 
    /// randomly-generated unequal Formulae have the same hash code should be extremely small.
    /// </summary>
    /// <returns>The hashcode of expression. Same expression but different Formula have to return
    ///          same hashcode</returns>
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 13;
            foreach (string token in GetTokens(correctFormula))
            {
                if (double.TryParse(token, out double number))
                    hash = hash * 17 + number.GetHashCode();
                else if (!operators.Contains(token))
                    hash = hash * 15 + token.GetHashCode();
                else
                    hash = hash * 17 + token.GetHashCode();
            }
            return hash;
        }
    }

    /// <summary>
    /// Given an expression, enumerates the tokens that compose it.  Tokens are left paren;
    /// right paren; one of the four operator symbols; a string consisting of a letter or underscore
    /// followed by zero or more letters, digits, or underscores; a double literal; and anything that doesn't
    /// match one of those patterns.  There are no empty tokens, and no token contains white space.
    /// </summary>
    /// <param name="formula">Formula that you want to IEnumerate with specific patterns</param>
    /// <returns>Tokens according to the pattern rules</returns>
    private static IEnumerable<string> GetTokens(String formula)
    {
        // Patterns for individual tokens
        String lpPattern = @"\(";
        String rpPattern = @"\)";
        String opPattern = @"[\+\-*/]";
        String varPattern = @"[a-zA-Z_](?: [a-zA-Z_]|\d)*";
        String doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: [eE][\+-]?\d+)?";
        String spacePattern = @"\s+";

        // Overall pattern
        String pattern = String.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})",
                                    lpPattern, rpPattern, opPattern, varPattern, doublePattern, spacePattern);

        // Enumerate matching tokens that don't consist solely of white space.
        foreach (String s in Regex.Split(formula, pattern, RegexOptions.IgnorePatternWhitespace))
        {
            if (!Regex.IsMatch(s, @"^\s*$", RegexOptions.Singleline))
            {
                yield return s;
            }
        }
    }

    // Helper methods

    /// <summary>
    ///   <para>
    ///   This helper method check if the given formula is syntactically correct
    ///   For example, does formula have the right pairs of ( and ), or does it
    ///   have the appropriate amount of values and operators?
    ///   </para>
    /// </summary>
    /// <param name="formula">mathematical expression</param>
    /// <returns>True if formula is syntactically correct, False if it's incorrext</returns>
    /// <exception cref="FormulaFormatException">If the parantheses is missing</exception>
    private bool IsSyntacticallyCorrect(string formula)
    {
        Stack<string> paranOpStack = new();
        int valueCount = 0;
        int operatorCount = 0;
        foreach (string token in GetTokens(formula))
        {
            if (string.IsNullOrWhiteSpace(token)) continue;

            if (!operators.Contains(token)) { valueCount++; }
            else if (token.Equals("(")) { paranOpStack.Push(token); }
            else if (token.Equals(")"))
            {
                if (paranOpStack.Count < 1)
                    throw new FormulaFormatException("FormulaFormatException: missing parentheses.");
                else if (!paranOpStack.Pop().Equals("("))
                    throw new FormulaFormatException("FormulaFormatException: missing parentheses.");
            }
            else { operatorCount++; }
        }

        return valueCount.Equals(operatorCount + 1) && paranOpStack.Count == 0;
    }

    /// <summary>
    /// This helper method do the basic operation steps that check the given
    /// value stack has less than 2 value, and if the popVaStackTwiceAndCalculate
    /// returns false, it means there is an error. So return false.
    /// </summary>
    /// <param name="vaStack">stack that holding values</param>
    /// <param name="opStack">stack that holding operators</param>
    /// <returns>True if evaluatable, False if it's not</returns>
    private bool operationStep(Stack<object> vaStack, Stack<string> opStack)
    {
        if (vaStack.Count < 2)
        {
            FEmessage = new FormulaError("FormulaError: value stack is fewer than 2");
            return false;
        }

        if (!popVaStackTwiceAndCalculate(vaStack, opStack)) return false;

        return true;
    }

    /// <summary>
    /// This helper method do the actual calculation of the given situation.
    /// It pop two values from vaStack and one operator from opStack, and
    /// calculate it. If the denominator is 0, it throws and exception
    /// </summary>
    /// <param name="vaStack">stack that holding values</param>
    /// <param name="opStack">stack that holding operators</param>
    /// <returns>Ture, if it succeed to calculate, otherwise false</returns>
    private bool popVaStackTwiceAndCalculate(Stack<object> vaStack, Stack<string> opStack)
    {
        double sVa = double.Parse(vaStack.Pop().ToString());
        double fVa = double.Parse(vaStack.Pop().ToString());
        string op = opStack.Pop();

        if (op.Equals("+"))
            vaStack.Push((fVa + sVa).ToString());
        else if (op.Equals("-"))
            vaStack.Push((fVa - sVa).ToString());
        else if (op.Equals("*"))
            vaStack.Push((fVa * sVa).ToString());
        else if (op.Equals("/"))
        {
            if (sVa.Equals(0))
            {
                FEmessage = new FormulaError("FormulaError: value can't be divided by 0.");
                return false;
            }
            vaStack.Push((fVa / sVa).ToString());
        }
        return true;
    }
}

/// <summary>
/// Used to report syntactic errors in the argument to the Formula constructor.
/// </summary>
public class FormulaFormatException : Exception
{
    /// <summary>
    /// Constructs a FormulaFormatException containing the explanatory message.
    /// </summary>
    /// <param name="message">Message you want to alert to user</param>
    public FormulaFormatException(String message) : base(message) { }
}

/// <summary>
/// Used as a possible return value of the Formula.Evaluate method.
/// </summary>
public struct FormulaError
{
    /// <summary>
    /// Constructs a FormulaError containing the explanatory reason.
    /// </summary>
    /// <param name="reason">The reason why this expression hasn't evaluated</param>
    public FormulaError(String reason) : this() { Reason = reason; }

    /// <summary>
    ///  The reason why this FormulaError was created.
    /// </summary>
    public string Reason { get; private set; }
}


// <change>
//   If you are using Extension methods to deal with common stack operations (e.g., checking for
//   an empty stack before peeking) you will find that the Non-Nullable checking is "biting" you.
//
//   To fix this, you have to use a little special syntax like the following:
//
//       public static bool OnTop<T>(this Stack<T> stack, T element1, T element2) where T : notnull
//
//   Notice that the "where T : notnull" tells the compiler that the Stack can contain any object
//   as long as it doesn't allow nulls!
// </change>
