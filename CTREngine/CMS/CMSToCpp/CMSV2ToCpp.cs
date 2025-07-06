using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CMS
{
    public static class CMSV2Tokenizer
    {
        public static List<string> Tokenize(string input)
        {
            if (input == null) return new List<string>();

            string noComments = Regex.Replace(
                input,
                @"/\*[\s\S]*?\*/|//.*?$",
                "",
                RegexOptions.Multiline);

            var tokens = Regex.Split(
                noComments,
                @"\s+
              (?= (?: [^""]* "" [^""]* "")*  [^""]*  $ )
              (?= (?: [^']*  ' [^']*  ' )*  [^']*  $ )",
                RegexOptions.IgnorePatternWhitespace);

            List<string> toks = new List<string>(tokens);
            for (int i = 0; i < toks.Count; i++)
            {
                if (toks[i] == "")
                    toks.RemoveAt(i);
            }
            return toks;
        }
    }
    
    public class ErrorReason
    {
        string type;
        string error;
        int tokenIndex;
        public List<string> Tokens;
        public string fullError()
        {
            return "" +
            type + "\n" + error + "\nAt Token: " + tokenIndex.ToString() + ";\n"+CoolTokenShow();
        }

        private string CoolTokenShow()
        {
            string cur = "";
            if (tokenIndex >= 2)
            {
                cur += Tokens[tokenIndex - 2] + " ";
            }
            if (tokenIndex >= 1)
            {
                cur += Tokens[tokenIndex - 1] + " ";
            }
            cur += ">>" + Tokens[tokenIndex] + "<< ";
            if (Tokens.Count > tokenIndex + 2)
            {
                cur += Tokens[tokenIndex + 1]+ " ";
            }
            if (Tokens.Count > tokenIndex + 3)
            {
                cur += Tokens[tokenIndex + 2];
            }
            return cur;
        }

        public ErrorReason(string t, string e, int i)
        {
            type = t; error = e; tokenIndex = i;
        }
    }

    public class CMSV2ConversionResult
    {
        public string Cpp;
        public string CMS;
        public ErrorReason whyFailed;
        public bool failed;
        public CMSV2Conversion conversion;
        public List<CMSV2Class> classes = new List<CMSV2Class>();

        public string SumUp()
        {
            whyFailed.Tokens = conversion.Tokens;
            string cur = "";
            cur += failed ? "CMSV2 To C++ Conversion FAILED!" : "CMSV2 To C++ Conversion SUCCESSFUL!";
            cur += "\n";
            cur += failed ? whyFailed.fullError() : "C++\n\n" + Cpp + "\n\n" + "CMS\n\n" + CMS;
            if (failed)
            {
                cur += "\n\nCaptured Tokens:\n";
                int i = 0;
                foreach (string Token in conversion.Tokens)
                {
                    cur += "Token " + i + ":" + "\"" + Token + "\"\n";
                    i++;
                }

                cur += Cpp + " <<<< HERE";
            }
            return cur;
        }
    }

    public class CMSV2BatchCR
    {
        public List<CMSV2ConversionResult> CMSV2CR = new List<CMSV2ConversionResult>();
        public List<CMSV2Conversion> CMSV2C = new List<CMSV2Conversion>();
        List<string> converts = new List<string>();
        List<string> paths = new List<string>();

        public void StartJob(string buildDir)
        {
            int i = 0;
            foreach (string c in converts)
            {
                CMSV2Conversion cc = new CMSV2Conversion(c);
                cc.fileName = Path.GetFileNameWithoutExtension(paths[i]);
                cc.DoSymbolRun(buildDir);
                CMSV2C.Add(cc);
                i++;
            }

            List<string> symbols = new List<string>();
            List<CMSV2Var> globalVar = new List<CMSV2Var>();

            foreach (CMSV2Conversion cms in CMSV2C)
            {
                globalVar.AddRange(cms.globalVar);
                symbols.AddRange(cms.validSymbols);
            }

            foreach (CMSV2Conversion cms in CMSV2C)
            {
                cms.globalVar = globalVar;
                cms.validSymbols = symbols;
                cms.converted = CMSV2C;
                CMSV2CR.Add(cms.StartJob(buildDir));
            }
        }

        public CMSV2BatchCR(List<string> tc, List<string> inc)
        {
            converts = tc;
            paths = inc;
        }
    }

    public static class CMSV2ToCpp
    {
        public static CMSV2ConversionResult Convert(string toConvert, string buildDir)
        {
            CMSV2Conversion cc = new CMSV2Conversion(toConvert);
            return cc.StartJob(buildDir);
        }

        public static CMSV2BatchCR ConvertBatch(List<string> toConvert, List<string> paths, string buildDir)
        {
            CMSV2BatchCR cc = new CMSV2BatchCR(toConvert, paths);
            cc.StartJob(buildDir);
            return cc;
        }
    }

    public class Token
    {
        public TokenType type;
        int index = 0;

        public Token(TokenType t, int i)
        {
            type = t;
            index = i;
        }
    }

    public enum TokenType
    {
        Class,
        If,
        Switch,
        Return,
        Other
    }

    public enum TokenAdditives
    {
        Static,
        Private,
        Public,
        Protected,
        Override
    }

    public class CMSV2Var
    {
        public string name;
        public string returnType;

        public CMSV2Var(string n, string rT)
        {
            name = n;
            returnType = rT;
        }
    }

    public class CMSV2Class
    {
        public string name;
        public List<string> inheritants;

        public CMSV2Class(string n, List<string> i)
        {
            name = n;
            inheritants = i;
        }
    }

    public class CMSV2Conversion
    {
        string cmsv2Script;
        string currentCpp;
        int currentIndex = 0;
        public List<string> Tokens;
        bool cancel = false;
        ErrorReason curError;
        List<TokenAdditives> currentAdds;
        public List<CMSV2Var> currentClassVar = new List<CMSV2Var>();
        public List<CMSV2Var> currentLocalVar = new List<CMSV2Var>();
        public List<CMSV2Var> globalVar = new List<CMSV2Var>();
        public List<CMSV2Var> currentClassFunc = new List<CMSV2Var>();
        public List<string> validSymbols = new List<string>();
        CMSV2ConversionResult result;
        CMSV2Class currentClass;
        public CppExpressionConverter currentCEC;
        int bodiesToClose = 0;
        bool inClass = false;
        bool inComment = false;
        bool inFunction;
        string addBeforeVariable = "";
        public List<string> validScriptNames;
        public List<string> classNames = new List<string>();
        public List<string> includeList = new List<string>();
        public List<CMSV2Conversion> converted;
        public string fileName;
        int run = 0; //run 0 is symbol collection, run 1 is actual conversion

        public void DoSymbolRun(string buildDir)
        {
            run = 0;
            Tokens = CMSV2Tokenizer.Tokenize(cmsv2Script);
            currentCpp = "#include \"CTR/AutoIncludes.h\"\nusing namespace std;";
            AddNLS();
            // Symbol Run
            result = new CMSV2ConversionResult();
            currentAdds = new List<TokenAdditives>();
            currentCEC = CEC.FromSource(buildDir);
            IdentifyNextExpression();
        }

        public CMSV2ConversionResult StartJob(string buildDir)
        {
            Tokens = CMSV2Tokenizer.Tokenize(cmsv2Script);
            inClass = false;
            inComment = false;
            inFunction = false;
            bodiesToClose = 0;
            cancel = false;
            currentIndex = 0;
            // Actual Run
            run = 1;
            currentCpp = "#include \"CTR/AutoIncludes.h\"\nusing namespace std;";
            currentAdds = new List<TokenAdditives>();
            currentCEC = CEC.FromSource(buildDir);

            result = new CMSV2ConversionResult();

            AddNLS();
            IdentifyNextExpression();

            result.Cpp = currentCpp;
            result.CMS = cmsv2Script;
            result.failed = false;
            if (cancel)
            {
                result.failed = true;
                result.whyFailed = curError;
                Console.WriteLine("Conversion Failed, Tried C++:\n"+currentCpp);
            }
            result.conversion = this;

            return result;
        }

        void CancelEarly(ErrorReason er)
        {
            if (run == 1)
            {
                cancel = true;   
            }
            curError = er;
        }

        void AddNLS()
        {
            currentCpp += "\n\n";
        }

        string getCurrentToken()
        {
            if (currentIndex >= 0 && currentIndex < Tokens.Count)
            {
                return Tokens[currentIndex];
            }
            else
            {
                CancelEarly(new ErrorReason("Error: ", "Exceeded Tokens while trying to parse.", currentIndex));
                return null;
            }
        }

        void IdentifyNextExpression()
        {
            if (currentIndex > Tokens.Count - 1) return;
            if (cancel) return;
            string thisToken = getCurrentToken();
            Console.WriteLine("Handling Token: "+thisToken);
            switch (thisToken)
            {
                case "static":
                    currentAdds.Add(TokenAdditives.Static);
                    break;
                case "override":
                    currentAdds.Add(TokenAdditives.Override);
                    break;
                case "public":
                    currentAdds.Add(TokenAdditives.Public);
                    break;
                case "protected":
                    currentAdds.Add(TokenAdditives.Protected);
                    break;
                case "private":
                    currentAdds.Add(TokenAdditives.Private);
                    break;
                case "class":
                    HandleClass();
                    break;

                case "Loop":
                    HandleLoop();
                break;

                case "If":
                    HandleIf();
                break;

                case "Else":
                    currentCpp += " else ";
                break;

                case "Use":
                    HandleUse();
                break;

                case "}":
                    currentCpp += "}";
                    bodiesToClose--;
                    //AddNLS();
                    break;

                case "/*":
                    inComment = true;
                    while (inComment)
                    {
                        currentIndex++;
                        if (getCurrentToken() == "*/")
                        {
                            inComment = false;
                        }
                        if (currentIndex > Tokens.Count - 1)
                        {
                            CancelEarly(new ErrorReason("Error: ", "Did not exit comment. Did you forget the syntax?", currentIndex));
                            break;
                        }
                    }
                    break;

                default:
                    bool canc = true;
                    foreach (string rType in currentCEC.validReturnType)
                    {
                        if (thisToken == rType && inClass)
                        {
                            Console.WriteLine("Detected Returner");
                            canc = false;
                            handleReturner();
                        }
                        else if (!inClass && thisToken == rType)
                        {
                            CancelEarly(new ErrorReason("Error: ", "Cannot declare function or variable outside of class.", currentIndex));
                            return;
                        }
                    }
                    if (inClass)
                    {
                        foreach (CMSV2Var var in currentClassVar)
                        {
                            if (thisToken == var.name)
                            {
                                canc = false;
                                handleVariableOption();
                                break;
                            }
                            else if (thisToken.StartsWith(var.name+"."))
                            {
                                string acc = "";
                                if (fixAccess(var, thisToken, out acc))
                                {
                                    canc = false;
                                    Tokens[currentIndex] = acc;
                                    handleVariableOption();
                                    break;
                                }
                            }
                        }
                        foreach (CMSV2Var var in globalVar)
                        {
                            if (thisToken == var.name)
                            {
                                canc = false;
                                handleVariableOption();
                                break;
                            }
                            else if (thisToken.StartsWith(var.name+"."))
                            {
                                string acc = "";
                                if (fixAccess(var, thisToken, out acc))
                                {
                                    canc = false;
                                    Tokens[currentIndex] = acc;
                                    handleVariableOption();
                                    break;
                                }
                            }
                        }
                        foreach (CMSV2Var var in currentLocalVar)
                        {
                            if (thisToken == var.name)
                            {
                                canc = false;
                                handleVariableOption();
                                break;
                            }
                            else if (thisToken.StartsWith(var.name + "."))
                            {
                                string acc = "";
                                if (fixAccess(var, thisToken, out acc))
                                {
                                    canc = false;
                                    Tokens[currentIndex] = acc;
                                    handleVariableOption();
                                    break;
                                }
                            }
                        }
                        foreach (CMSV2Var var in currentClassFunc)
                        {
                            if (thisToken == var.name)
                            {
                                canc = false;
                                handleFunctionOption();
                                break;
                            }
                            else if (thisToken.StartsWith(var.name + "."))
                            {
                                string acc = "";
                                if (fixAccess(var, thisToken, out acc))
                                {
                                    canc = false;
                                    Tokens[currentIndex] = acc;
                                    handleFunctionOption();
                                    break;
                                }
                            }
                        }
                        foreach (string expr in currentCEC.cmsv2Expression)
                        {
                            if (thisToken == expr)
                            {
                                handleVarOrFunc();
                                canc = false;
                                break;
                            }
                        }
                        foreach (string sym in validSymbols)
                        {
                            if (thisToken == sym)
                            {
                                handleVarOrFunc();
                                canc = false;
                                break;
                            }
                        }
                    }
                    if (canc) CancelEarly(new ErrorReason("Error: ", "Unrecognized Token: \"" + thisToken + "\"", currentIndex));
                    break;
            }
            currentIndex++;
            IdentifyNextExpression();
        }

        string TokenAdditivesToString()
        {
            string cur = "";
            foreach (TokenAdditives ta in currentAdds)
            {
                cur += ta.ToString().ToLower() + " ";
            }
            return cur;
        }

        // actual Conversion

        bool fixAccess(CMSV2Var var, string usage, out string Access)
        {
            string thisToken = usage;
            string toReplace = ".";
            string replacement = (var.name == var.returnType) ? ("::") : (var.returnType.EndsWith("*") ? "->" : ".");

            int index = thisToken.IndexOf(toReplace);
            if (index >= 0)
            {
                Access = thisToken.Substring(0, index) + replacement + thisToken.Substring(index + toReplace.Length);
                return true;
            }
            Access = usage;
            return false;
        }

        string HandlePossbileReturnerArgs(string thisToken)
        {
            foreach (CMSV2Var var in currentClassVar)
            {
                if (thisToken == var.name)
                {
                    return TryTranslate(thisToken);
                }
                else if (thisToken.StartsWith(var.name + ".") && Tokens[currentIndex + 1] != "(")
                {
                    string acc = "";
                    if (fixAccess(var, thisToken, out acc))
                    {
                        Tokens[currentIndex] = acc;
                        return TryTranslate(acc);
                    }
                }
                else if (thisToken.StartsWith(var.name + ".") && Tokens[currentIndex + 1] == "(")
                {
                    string acc = "";
                    if (fixAccess(var, thisToken, out acc))
                    {
                        Tokens[currentIndex] = acc;
                        return handleFunctionOption(false,false);
                    }
                }
            }
            foreach (string var in validSymbols)
            {
                if (thisToken == var)
                {
                    return TryTranslate(thisToken);
                }
                else if (thisToken.StartsWith(var + ".") && Tokens[currentIndex + 1] != "(")
                {
                    string acc = "";
                    if (fixAccess(new CMSV2Var(var,var), thisToken, out acc))
                    {
                        Tokens[currentIndex] = acc;
                        return TryTranslate(acc);
                    }
                }
                else if (thisToken.StartsWith(var + ".") && Tokens[currentIndex + 1] == "(")
                {
                    string acc = "";
                    if (fixAccess(new CMSV2Var(var,var), thisToken, out acc))
                    {
                        Tokens[currentIndex] = acc;
                        return handleFunctionOption(false,false);
                    }
                }
            }
            foreach (CMSV2Var var in globalVar)
            {
                if (thisToken == var.name)
                {
                    return TryTranslate(thisToken);
                }
                else if (thisToken.StartsWith(var.name + ".") && Tokens[currentIndex + 1] != "(")
                {
                    string acc = "";
                    if (fixAccess(var, thisToken, out acc))
                    {
                        Tokens[currentIndex] = acc;
                        return TryTranslate(acc);
                    }
                }
                else if (thisToken.StartsWith(var.name + ".") && Tokens[currentIndex + 1] == "(")
                {
                    string acc = "";
                    if (fixAccess(var, thisToken, out acc))
                    {
                        Tokens[currentIndex] = acc;
                        return handleFunctionOption(false,false);
                    }
                }
            }
            foreach (CMSV2Var var in currentLocalVar)
            {
                if (thisToken == var.name)
                {
                    return TryTranslate(thisToken);
                }
                else if (thisToken.StartsWith(var.name + ".") && Tokens[currentIndex + 1] != "(")
                {
                    string acc = "";
                    if (fixAccess(var, thisToken, out acc))
                    {
                        Tokens[currentIndex] = acc;
                        return TryTranslate(acc);
                    }
                }
                else if (thisToken.StartsWith(var.name + ".") && Tokens[currentIndex + 1] == "(")
                {
                    string acc = "";
                    if (fixAccess(var, thisToken, out acc))
                    {
                        Tokens[currentIndex] = acc;
                        return handleFunctionOption(false, false);
                    }
                }
            }
            foreach (CMSV2Var var in currentClassFunc)
            {
                if (thisToken == var.name)
                {
                    return handleFunctionOption(false, false);
                }
            }
            foreach (string expr in currentCEC.cmsv2Expression)
            {
                if (thisToken == expr)
                {
                    if (Tokens[currentIndex + 1] == "(")
                    {
                        return handleFunctionOption(false, false);
                    }
                }
            }
            if (thisToken == "not")
            {
                currentIndex++;
                return "!"+getCurrentToken();
            }
            if (thisToken == "(")
            {
                currentIndex++;
                string ts = "( ";
                while (getCurrentToken() != ")")
                {
                    string token = getCurrentToken();

                    if (token == ",")
                    {
                        ts += ", ";
                    }
                    else
                    {
                        ts += HandlePossbileReturnerArgs(token) + " ";
                    }
                    
                    if (getCurrentToken() != ")")
                    {
                        currentIndex++;
                    }
                    else break;
                }
                currentIndex++;
                return ts + " )";
            }
            return TryTranslate(thisToken);
        }

        void HandleUse()
        {
            currentIndex++;
            string toInclude = getCurrentToken();
            if (run == 0) includeList.Add(toInclude);
            if (run == 0) return;
            foreach (CMSV2Conversion con in converted)
            {
                Console.WriteLine("Looking for: " + toInclude + " while including, current is: " + con.fileName);
                if (con.fileName == toInclude)
                {
                    currentCpp += "// Forward Declerations for CMS: " + toInclude + "\n";
                    foreach (string c in con.classNames)
                    {
                        currentCpp += "class " + c + ";\n";
                    }
                    break;
                }
            }
            currentIndex++;
            if (getCurrentToken() == ";") { AddNLS(); }
            else CancelEarly(new ErrorReason("Error: ", "Syntax Error in Use, Usage: 'Use cmsFileName ;'", currentIndex));
        }

        void HandleIf()
        {
            currentIndex++;
            currentCpp += "if (";
            bool usedP = false;
            if (getCurrentToken() == "(")
            {
                currentIndex++;
                usedP = true;
            }
            while (getCurrentToken() != ")" && getCurrentToken() != "{")
            {
                currentCpp += HandlePossbileReturnerArgs(getCurrentToken()) + " ";
                if (getCurrentToken() != ")" && getCurrentToken() != "{")
                {
                    currentIndex++;
                }
            }
            if (getCurrentToken() == "{" && usedP)
            {
                CancelEarly(new ErrorReason("Error", "Are you being Intentionally dense? \"huh\"", currentIndex));
                return;
            }
            currentIndex++;

            currentCpp += ") ";
            handleFunctionContents();
        }

        void HandleClass()
        {
            if (currentAdds.Contains(TokenAdditives.Static))
            {
                addBeforeVariable = "static";
            }
            currentIndex++;
            string name = getCurrentToken();
            currentIndex++;
            currentCpp += "class " + name;
            CMSV2Class cl = new CMSV2Class(name, new List<string>() {});
            cl.name = name;
            result.classes.Add(cl);
            currentCEC.validReturnType.Add(name);
            Console.WriteLine("Made Class");
            currentClassVar.Add(new CMSV2Var(name, name));
            globalVar.Add(new CMSV2Var(name, name));
            currentClassFunc.Add(new CMSV2Var(name, name));
            currentAdds.Clear();
            HandleInheritants(cl);
            if (cancel)
            {
                return;
            }
        }

        enum HIState
        {
            expectsInheritant,
            expectsMultInheritant,
            ableToComplete,
            ableToCompleteAI
        }

        void HandleInheritants(CMSV2Class cl)
        {
            HIState currentstate = HIState.ableToComplete;
            bool sr = false;
            while (!sr)
            {
                string curToken = getCurrentToken();
                switch (curToken)
                {
                    case "{":
                        if (currentstate == HIState.ableToComplete || currentstate == HIState.ableToCompleteAI)
                        {
                            currentCpp += " {";
                            AddNLS();
                            sr = true;
                            inClass = true;
                            bodiesToClose++;
                            break;
                        }
                        else
                        {
                            CancelEarly(new ErrorReason("Error", "Syntax error in class construction.", currentIndex));
                            sr = true;
                            break;
                        }
                        break;
                    case "inherits":
                        if (currentstate == HIState.ableToComplete)
                        {
                            currentstate = HIState.expectsInheritant;
                            currentCpp += " : ";
                        }
                        else
                        {
                            CancelEarly(new ErrorReason("Error", "Syntax error in class construction. Was expecting to be able to complete and be first inherit.", currentIndex));
                            sr = true;
                            break;
                        }
                        break;

                    case ",":
                        if (currentstate == HIState.ableToCompleteAI)
                        {
                            currentCpp += ", ";
                            currentstate = HIState.expectsMultInheritant;
                        }
                        else
                        {
                            CancelEarly(new ErrorReason("Error", "Syntax error in class construction. Was expecting to end.", currentIndex));
                            sr = true;
                            break;
                        }
                        break;

                    default:
                        if (currentstate == HIState.expectsInheritant)
                        {
                            currentstate = HIState.ableToCompleteAI;
                            bool isP = true;
                            switch (curToken)
                            {
                                case "public":
                                    currentCpp += "public";
                                    break;
                                case "private":
                                    currentCpp += "private";
                                    break;
                                case "protected":
                                    currentCpp += "protected";
                                    break;
                                default:
                                    cl.inheritants.Add(curToken);
                                    currentCpp += curToken;
                                    isP = false;
                                    break;
                            }

                            if (isP)
                            {
                                currentIndex++;
                                currentCpp += " " + getCurrentToken();
                                cl.inheritants.Add(getCurrentToken());
                            }

                        }
                        else if (currentstate == HIState.expectsMultInheritant)
                        {
                            currentstate = HIState.ableToCompleteAI;
                            bool isP = true;
                            switch (curToken)
                            {
                                case "public":
                                    currentCpp += "public";
                                    break;
                                case "private":
                                    currentCpp += "private";
                                    break;
                                case "protected":
                                    currentCpp += "protected";
                                    break;
                                default:
                                    currentCpp += curToken;
                                    isP = false;
                                    break;
                            }

                            if (isP)
                            {
                                currentIndex++;
                                currentCpp += " " + getCurrentToken();
                            }
                        }
                        else
                        {
                            CancelEarly(new ErrorReason("Error", "Syntax error in class construction.", currentIndex));
                            sr = true;
                            break;
                        }

                        break;
                }
                if (!sr) currentIndex++;
            }
            HandleClassContents(cl);
        }

        void HandleClassContents(CMSV2Class cl)
        {
            currentClassVar = new List<CMSV2Var>();
            currentClassFunc = new List<CMSV2Var>();
            int startbodies = bodiesToClose;
            currentIndex++;
            currentClass = cl;
            if (run == 0) validSymbols.Add(cl.name);
            if (run == 0) classNames.Add(cl.name);
            while (startbodies <= bodiesToClose) // startbodies = 1; bodiesToClose = 1;
            {
                IdentifyNextExpression();
                if (startbodies > bodiesToClose)
                {
                    break;
                }
                if (cancel)
                {
                    break;
                }
                if (currentIndex > Tokens.Count - 1)
                {
                    CancelEarly(new ErrorReason("Error: ", "Didn't close out of Body.", currentIndex));
                    return;
                }
            }
            currentCpp = currentCpp.Remove(currentCpp.Length - 1);
            HandleClassClose();
            currentCpp += "};";
            AddNLS();
            currentClassVar.Clear();
            currentClassFunc.Clear();
            inClass = false;
            addBeforeVariable = "";
            currentClass = null;
        }

        void HandleClassClose()
        {
            currentCpp += "\npublic:\n" + currentClass.name + "() {}\n";
        }

        void handleVarOrFunc(bool useS = true)
        {
            if (Tokens[currentIndex + 1] == "(")
            {
                handleFunctionOption(useS);
            }
            else
            {
                handleVariableOption(useS);
            }
        }

        void handleReturner()
        {
            Console.WriteLine("Handling Returner.");
            string returnType = getCurrentToken();
            bool isFunc = false;
            bool addOV = false;
            if (currentAdds.Contains(TokenAdditives.Public))
            {
                currentCpp += "public:\n";
            }
            else if (currentAdds.Contains(TokenAdditives.Private))
            {
                currentCpp += "private:\n";
            }
            else if (currentAdds.Contains(TokenAdditives.Protected))
            {
                currentCpp += "protected:\n";
            }
            if (currentAdds.Contains(TokenAdditives.Override))
            {
                addOV = true;
            }
            currentIndex++;
            currentAdds.Clear();
            string varName = getCurrentToken();
            if (run == 0) validSymbols.Add(varName);

            currentIndex++;
            isFunc = getCurrentToken() == "(";

            currentCpp += ((addBeforeVariable != "" && !inFunction) ? addBeforeVariable + " " : "") + (isFunc ? "inline " : "") + returnType;
            currentCpp += " " + varName;
            Console.WriteLine("Created \"" + varName + "\" of Type \"" + returnType + "\"");
            if (!isFunc)
            {
                handleVariable();
                if (!inFunction)
                {
                    currentClassVar.Add(new CMSV2Var(varName, returnType));
                }
                else
                {
                    currentLocalVar.Add(new CMSV2Var(varName, returnType));
                }
            }
            else
            {
                currentClassFunc.Add(new CMSV2Var(varName, returnType));
                handleFunction(addOV);
            }

            if (cancel)
            {
                return;
            }
        }

        void handleFunction(bool addOV)
        {
            currentIndex++;
            bool hasParameters = getCurrentToken() == "parameter";
            currentCpp += "(";
            if (!hasParameters)
            {
                if (getCurrentToken() == ")")
                {
                    currentCpp += ")";
                    currentCpp += addOV ? " override " : "";
                    currentIndex++;
                    handleFunctionContents();
                }
                else
                {
                    CancelEarly(new ErrorReason("Error: ", "Expected Function Close, got: " + getCurrentToken() + ", did you forget to use parameter?", currentIndex));
                }
            }
            else
            {
                currentIndex++;
                int defaultStart = -1;
                int i = 0;
                while (getCurrentToken() != ")")
                {
                    if (cancel) return;
                    if (i != 0)
                    {
                        if (getCurrentToken() != ",")
                        {
                            CancelEarly(new ErrorReason("Error: ", "Was expecting ',', got: " + getCurrentToken(), currentIndex));
                            return;
                        }
                        currentIndex++;
                        currentCpp += ",";
                    }
                    string type = getCurrentToken();
                    currentIndex++;
                    string name = getCurrentToken();
                    currentLocalVar.Add(new CMSV2Var(name, type));
                    currentCpp += " " + type + " " + name;
                    currentIndex++;
                    if (getCurrentToken() == "=")
                    {
                        if (defaultStart == -1) defaultStart = i;
                        currentIndex++;
                        currentCpp += " = " + getCurrentToken();
                        currentIndex++;
                    }
                    else if (defaultStart != -1)
                    {
                        CancelEarly(new ErrorReason("Error: ", "Cannot have non-default parameter after a default parameter." + getCurrentToken(), currentIndex));
                    }
                    i++;
                }
                currentCpp += ")";
                currentCpp += addOV ? " override " : "";
                currentIndex++;
                handleFunctionContents();
            }
        }

        void handleFunctionContents()
        {
            string tok = getCurrentToken();
            currentCpp += " {\n";
            inFunction = true;
            if (tok == "{")
            {
                // handle body
                currentIndex++;
                handleBody();
            }
            else // handle singleCommand
            {
                handleInstruc();
                currentCpp += "}\n";
                currentIndex--;
            }
            currentIndex++;
        }

        void HandleLoop()
        {
            currentCpp += "\n";
            currentIndex++;
            if (getCurrentToken() != "(") { CancelEarly(new ErrorReason("Error: ", "Expected Loop Function Opening.", currentIndex)); return; }
            currentIndex++;
            string loopCount = "";
            string varName = "i";
            bool firstArgNum = false;
            if (int.TryParse(getCurrentToken(), out _))
            {
                loopCount = getCurrentToken();
                firstArgNum = true;
            }
            else
            {
                varName = getCurrentToken();
            }

            string startIndex = "0";
            currentIndex++;

            if (firstArgNum)
            {
                if (getCurrentToken() == ",")
                {
                    currentIndex++;
                }
                else if (getCurrentToken() != ")")
                {
                    startIndex = getCurrentToken();
                }
            }
            else
            {
                if (getCurrentToken() == ",")
                {
                    currentIndex++;
                }
                if (getCurrentToken() == ")")
                {
                    CancelEarly(new ErrorReason("Error: ", "Expected Loop Count in Loop. (Parameter 2)", currentIndex));
                    return;
                }
                else
                {
                    loopCount = getCurrentToken();
                }
            }

            currentIndex++;

            if (getCurrentToken() == "," && !firstArgNum)
            {
                currentIndex++;
                startIndex = getCurrentToken();
                currentIndex++;
            }

            currentCpp += $"for ( int {varName} = {startIndex}; {varName} < {loopCount}; {varName}++ )";
            if (getCurrentToken() != ")")
            {
                CancelEarly(new ErrorReason("Error: ", "Expected Loop Close.", currentIndex));
                return;
            }

            currentIndex++;
            handleFunctionContents();
        }

        void handleBody()
        {
            int startbodies = bodiesToClose;
            while (startbodies <= bodiesToClose) // startbodies = 1; bodiesToClose = 1;
            {
                IdentifyNextExpression();
                if (startbodies > bodiesToClose)
                {
                    return;
                }
                if (cancel)
                {
                    return;
                }
                if (currentIndex > Tokens.Count - 1)
                {
                    CancelEarly(new ErrorReason("Error: ", "Didn't close out of Body.", currentIndex));
                    return;
                }
            }
            if (inFunction)
            {
                inFunction = false;
                currentLocalVar.Clear();
            }
        }

        void handleInstruc()
        {
            string curTok = getCurrentToken();
            foreach (CMSV2Var clVar in currentClassVar)
            {
                if (curTok == clVar.name)
                {
                    handleVariableOption();
                    return;
                }
            }

            foreach (CMSV2Var clVar in globalVar)
            {
                if (curTok == clVar.name)
                {
                    handleVariableOption();
                    return;
                }
            }

            foreach (string clVar in validSymbols)
            {
                if (curTok == clVar)
                {
                    handleVarOrFunc();
                    return;
                }
            }

            foreach (string clVar in currentCEC.cmsv2Expression)
            {
                if (curTok == clVar)
                {
                    handleVarOrFunc();
                    return;
                }
            }

            foreach (CMSV2Var clVar in currentLocalVar)
            {
                if (curTok == clVar.name)
                {
                    handleVariableOption();
                    return;
                }
            }

            foreach (CMSV2Var clVar in currentClassFunc)
            {
                if (curTok == clVar.name)
                {
                    handleFunctionOption();
                    return;
                }
            }

            if (curTok == "return")
            {
                currentCpp += "return ";
                currentIndex++;
                curTok = getCurrentToken();

                while (getCurrentToken() != ";")
                {
                    currentCpp += HandlePossbileReturnerArgs(getCurrentToken()) + " ";
                    if (getCurrentToken() != ";")
                    {
                        currentIndex++;
                    } else break;
                }

                if (getCurrentToken() == ";") { currentCpp += ";"; return; }
                else
                {
                    CancelEarly(new ErrorReason("Error: ", "Expected semicolon, got: " + getCurrentToken(), currentIndex));
                    return;
                }
            }

            CancelEarly(new ErrorReason("Error: ", "Action not Identified.", currentIndex));
        }

        string TryTranslate(string toTrans)
        {
            string trans = currentCEC.ConvertCMSv2ToCpp(toTrans);
            if (trans != null)
            {
                return trans;
            }
            return toTrans;
        }

        string handleFunctionOption(bool useS = true, bool app = true)
        {
            Console.WriteLine("Handling a Function Call");
            string var = TryTranslate(getCurrentToken());
            currentIndex++;
            string f = var;
            //currentCpp += var;
            if (getCurrentToken() == "(")
            {
                //currentCpp += "(";
                f += "(";
                currentIndex++;
                int i = 0;
                while (getCurrentToken() != ")")
                {
                    if (i != 0)
                    {
                        if (getCurrentToken() == ",")
                        {
                            //currentCpp += ", ";
                            f += ", ";
                            currentIndex++;
                        }
                        else
                        {
                            CancelEarly(new ErrorReason("Error: ", "Was expecting ',', got: \"" + getCurrentToken() + "\"", currentIndex));
                            break;
                        }
                    }
                    while (getCurrentToken() != "," && getCurrentToken() != ")")
                    {
                        f += HandlePossbileReturnerArgs(getCurrentToken()) + " ";
                        if (getCurrentToken() != "," && getCurrentToken() != ")")
                        {
                            currentIndex++;
                        } else break;
                    }
                    i++;
                }
                if (cancel) return f;
                f += ")";
                currentIndex++;
                //currentCpp += ")";
                if (useS)
                {
                    if (getCurrentToken() == ";" && useS) {
                        if (app) currentCpp += f + ";\n";
                        return f;
                    }
                    else
                    {
                        CancelEarly(new ErrorReason("Error: ", "Expected Semicolon in function call, got:"+getCurrentToken(), currentIndex));
                    }
                }
                else
                {
                    if (getCurrentToken() == ")") {currentIndex++;}
                    if (Tokens[currentIndex - 2] == ")") { f += ")"; }
                    else
                    {
                        Console.WriteLine("In Function: "+var+" Tokens -2 = "+Tokens[currentIndex-2]);
                    }
                    if (getCurrentToken() == ";") return f;
                    /*else
                    {
                        while (getCurrentToken() != ")")
                        {
                            f += HandlePossbileReturnerArgs(getCurrentToken()) + " ";
                            if (getCurrentToken() != ")")
                            {
                                currentIndex++;
                            } else break;
                        }
                    }*/
                }
            }
            else
            {
                CancelEarly(new ErrorReason("Error: ", "Was expecting Function Call, got: \"" + getCurrentToken() + "\"", currentIndex));
            }

            Console.WriteLine("Function Option: "+f+" Append?: "+app.ToString()+" useS?: "+useS);

            if (app)
            {
                currentCpp += f;
            }
            return f;
        }

        void handleVariableOption(bool useS = true)
        {
            string var = TryTranslate(getCurrentToken());
            currentIndex++;
            handleVariableOptionOp(var, useS);
        }

        void handleVariableOptionOp(string var, bool useS = true)
        {
            string op = getCurrentToken();

            if (op != "++" && op != "--")
                currentCpp += var + " " + op + " ";
            else
                currentCpp += var + op;

            currentIndex++;
            if (getCurrentToken() == ";")
            {
                if (!useS) return;
                currentCpp += ";\n";
                return;
            }

            currentCpp += " ";

            Console.WriteLine("Handling a Variable Set");

            while (getCurrentToken() != ";")
            {
                currentCpp += HandlePossbileReturnerArgs(getCurrentToken()) + " ";
                if (getCurrentToken() != ";")
                {
                    currentIndex++;
                } else break;
            }

            if (getCurrentToken() == ";")
            {
                currentCpp += ";\n";
            }
            else
            {
                CancelEarly(new ErrorReason("Error: ", "Was expecting semicolon, got: \"" + getCurrentToken() + "\"", currentIndex));
            }
        }

        void handleVariable()
        {
            if (getCurrentToken() == ";")
            {
                currentCpp += ";";
            }
            else if (getCurrentToken() == "=")
            {
                currentIndex++;
                currentCpp += " = ";
                while (getCurrentToken() != ";")
                {
                    currentCpp += HandlePossbileReturnerArgs(getCurrentToken()) + " ";
                    if (getCurrentToken() != ";")
                    {
                        currentIndex++;
                    } else break;
                }
                if (getCurrentToken() == ";")
                {
                    currentCpp += ";";
                }
                else
                {
                    CancelEarly(new ErrorReason("Error: ", "Expected semicolon, got: " + getCurrentToken(), currentIndex));
                }
            }
            currentCpp += "\n";
        }

        public CMSV2Conversion(string script)
        {
            cmsv2Script = script;
        }
    }

}