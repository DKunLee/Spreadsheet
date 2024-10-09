/// <summary>
/// Author:    DK Lee
/// Partner:   None
/// Date:      Jan 11, 2024
/// Course:    CS 3500, University of Utah, School of Computing
/// Copyright: CS 3500 and DK Lee - This work may not 
///            be copied for use in Academic Coursework.
///
/// I, DK, certify that I wrote this code from scratch and
/// did not copy it in part or whole from another source.  All 
/// references used in the completion of the assignments are cited 
/// in my README file.
///
/// File Contents
///
///   This file is the formula evaluator that contains the method that evaluate
///   the expression and return the result of the expression. If there are variable
///   in the expression, it automatically look for the Lookup delegate and check
///   if there is any assigned integer for the variable and returns to use it for
///   expression.
///    
/// </summary>


using System;
using System.Text.RegularExpressions;

namespace FormulaEvaluator;
public static class Evaluator
{
    /// <summary>
    /// This delegate allows to deal with the various variable that given to Evaluate
    /// function. If evaluate function needs to look for the integer for variable,
    /// this is where it look for.
    /// - How to use:
    /// Evaluator.Lookup lookUpForVariable = variableName => {
    ///     if (input == "D1")
    ///         return int;
    ///     else if (...)
    ///         return ...;
    ///         ...
    /// };
    /// </summary>
    /// <param name="expression"> Expression to be evaluated. </param>
    /// <param name="variableEvaluator"> A delegate that can use to look up the value of a variable. </param>
    /// <returns> The value of expression or throw ArgumentException(ArgumentArgumentException) </returns>
    public delegate int Lookup(String variable_name);


    /// <summary>
    /// This method calculate the given expression. If the expression contains
    /// the variable, it also looks for the given variableEvaluator to convert
    /// the variable to integers.
    /// Available Format of Variable: letters + integers (ex. A1, a1, aa1, AA1, ...)
    /// </summary>
    /// <param name="expression"> Expression to be evaluated. </param>
    /// <param name="variableEvaluator"> A delegate that can use to look up the value of a variable. </param>
    /// <returns> The value of expression or throw ArgumentException(ArgumentArgumentException) </returns>
    public static int Evaluate(String expression, Lookup variableEvaluator)
    {
        // Create new stacks for values and operators.
        Stack<String> valueStack = new();
        Stack<String> operatorStack = new();

        // Splits the expression by arithmetic operations and parenthesis.
        String[] splittedExpression = Regex.Split(expression, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");
        // These are the only operators that are available.
        String[] operators = { "+", "-", "*", "/", "(", ")" };

        // Check all the splitted expression to find the answer of the expression
        for (int i = 0; i < splittedExpression.Length; i++)
        {
            String token = splittedExpression[i];

            // Skip processing if the token is null value or only include whitespace
            if (string.IsNullOrWhiteSpace(token))
                continue;

            // Divide the case into two parts
            // 1. Token is operator.
            // 2. Token is value.
            if (operators.Contains(token))
            {
                if (token.Equals("+") || token.Equals("-"))
                {
                    // Check if the top of the operator stack is '+' or '-'.
                    if (operatorStack.TryPeek(out String op) && (op.Equals("+") || op.Equals("-")))
                        oneExpressionProcess(valueStack, operatorStack);

                    operatorStack.Push(token);

                }
                else if (token.Equals(")"))
                {

                    if (operatorStack.TryPeek(out String op) && (op.Equals("+") || op.Equals("-")))
                        oneExpressionProcess(valueStack, operatorStack);

                    // Throws if the top of the operator stack is not '('.
                    operatorStack.TryPeek(out String op1);
                    if (op1 == null || op1.Trim() != "(")
                        throw new ArgumentException("Invalid Expression: the given expression is wrong ('(' is missing)");
                    else
                        operatorStack.Pop();

                    if (operatorStack.TryPeek(out String op2) && (op2.Equals("*") || op2.Equals("/")))
                        oneExpressionProcess(valueStack, operatorStack);

                }
                // The case for '*', '/', '('.
                else
                    operatorStack.Push(token);
            }
            else
            {
                String newToken;
                // Divide the case into three parts
                // 1. If the token is integer
                // 2. If the token is variable with the format letters+integers.
                // 3. If the token is none of the above
                if (int.TryParse(token, out int tk) && tk >= 0)
                {
                    newToken = tk.ToString();
                }
                else
                {
                    // Trim the token for the cases that the token is including the whitespace
                    String trimmedToken = token.Trim();

                    // Check if the token is named with the format.
                    if (Regex.IsMatch(trimmedToken, @"^[a-zA-z]+\d+$"))
                    {
                        // If there is no given variable evaluator, throws an ArgumentException
                        if (variableEvaluator == null)
                            throw new ArgumentException("Null Variable Evaluator: No given variable evaluator.");

                        // Variable evaluator returns the integer, so convert it to add the token to
                        // value stack.
                        newToken = variableEvaluator(trimmedToken).ToString();

                    }
                    else
                        throw new ArgumentException("Invalid Expression: Wrong format of variables or intgers. (Correct format of variable: letters + digits)");

                }

                if (operatorStack.TryPeek(out String op) && (op.Equals("*") || op.Equals("/")))
                {
                    oneExpressionProcess(newToken, valueStack, operatorStack);
                }
                else
                {
                    valueStack.Push(newToken);
                }
            }

        }

        // Empty operator stack
        if (operatorStack.Count < 1)
        {
            if (valueStack.Count != 1)
                throw new ArgumentException("Invalid Expression: the given expression is wrong.");
        }
        // One more operator left to be calculated
        else
        {
            if (operatorStack.Count != 1)
                throw new ArgumentException("Invalid Expression: the given expression is wrong.");

            if (valueStack.Count != 2)
                throw new ArgumentException("Invalid Expression: the given expression is wrong.");

            oneExpressionProcess(valueStack, operatorStack);
        }

        return int.Parse(valueStack.Pop());
    }

    // Helper methods

    /// <summary>
    /// This helper method do the calculation with the given token and the value that
    /// on the top of the given value stack with the operator on the top of the given
    /// operator stack. Then, add it back to the given value stack.
    /// </summary>
    /// <param name="token"> value that user want to calculate  </param>
    /// <param name="vaStack"> stack of the values from the whole expression </param>
    /// <param name="opStack"> stack of the operators from the whole expression </param>
    private static void oneExpressionProcess(String token, Stack<String> vaStack, Stack<String> opStack)
    {
        if (vaStack.Count < 1)
            throw new ArgumentException("Invalid Expression: the given expression is wrong. (Missing values)");

        // Prepare all the ingredients to calculate the value.
        int backValue = int.Parse(token);
        int frontValue = int.Parse(vaStack.Pop());
        String op = opStack.Pop();

        if (op == "*")
        {
            vaStack.Push((frontValue * backValue).ToString());
        }
        else if (op == "/")
        {
            if (backValue == 0)
                throw new ArgumentException("Ivalid Expression: Division by zero is undefined");

            vaStack.Push((frontValue / backValue).ToString());
        }
    }

    /// <summary>
    /// This helper method do the calculation with the two values that on the top of
    /// the given value stack with the operator on the top of the given operator stack.
    /// Then, add it back to the given value stack.
    /// </summary>
    /// <param name="vaStack"> stack of the values from the whole expression </param>
    /// <param name="opStack"> stack of the operators from the whole expression </param>
    private static void oneExpressionProcess(Stack<String> vaStack, Stack<String> opStack)
    {
        if (vaStack.Count < 2)
            throw new ArgumentException("Invalid Expression: the given expression is wrong. (Missing values)");

        // Prepare all the ingredients to calculate the value.
        int backValue = int.Parse(vaStack.Pop());
        int frontValue = int.Parse(vaStack.Pop());
        String op = opStack.Pop();

        if (op == "+")
        {
            vaStack.Push((frontValue + backValue).ToString());
        }
        else if (op == "-")
        {
            vaStack.Push((frontValue - backValue).ToString());
        }
        else if (op == "*")
        {
            vaStack.Push((frontValue * backValue).ToString());
        }
        else if (op == "/")
        {
            if (backValue == 0)
                throw new ArgumentException("Ivalid Expression: Division by zero is undefined");

            vaStack.Push((frontValue / backValue).ToString());
        }
    }
}

