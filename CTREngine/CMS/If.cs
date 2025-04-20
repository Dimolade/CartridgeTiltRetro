using System.Collections;
using System.Collections.Generic;
using CMS;
using org.matheval;
using System.Text.RegularExpressions;

namespace CMS.Commands
{
    public static class If
    {
        public static bool HandleFullIf(string expressionWithoutParanthesis, CMSScript cms)
        {
            if (expressionWithoutParanthesis.Contains("=="))
            {
                return EqualsEqualsOperator(expressionWithoutParanthesis, cms);
            }
            return false;
        }
        public static bool EqualsEqualsOperator(string expression, CMSScript cms)
        {
            // Match pattern with or without spaces: "50 == 30" or "50==30"
            var match = Regex.Match(expression, @"^\s*(\S+)\s*==\s*(\S+)\s*$");

            Console.WriteLine("Val 1: "+match.Groups[1].Value+" Val 2: "+match.Groups[2].Value);

            float leftValue = 0f;
            bool b1 = false;
            if (cms.GetExpressionErrors(match.Groups[1].Value).Count == 0)
                leftValue = (float)cms.ParseExpressionD(match.Groups[1].Value);
                b1 = true;

            float rightValue = 0f;
            bool b2 = false;
            if (cms.GetExpressionErrors(match.Groups[2].Value).Count == 0)
                rightValue = (float)cms.ParseExpressionD(match.Groups[2].Value);
                b2 = true;

            bool sameValue = ((rightValue != null && leftValue != null) || CompareValues(match.Groups[1].Value, match.Groups[2].Value));

            if (match.Success)
            {
                if (sameValue)
                {
                    if (b1 && b2)
                    {
                        return leftValue == rightValue;
                    }
                    return match.Groups[1].Value == match.Groups[2].Value;
                }
            }
            
            return false;
        }

        public static bool CompareValues(string left, string right)
        {
            string l1 = Interpreter.GetTypeFromString(left);
            string l2 = Interpreter.GetTypeFromString(right);

            return l1 == l2;
        }
    }
}