using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System;
using System.Diagnostics;
using CMS;

public class CppExpressionConverter
{
    public List<string> cppExpressions = new List<string>();
    public List<string> cmsv2Expression = new List<string>();
    public List<string> validReturnType;
    public List<CECEntry> cecEntry;
    public List<CMSV2Var> translationFileVars = new List<CMSV2Var>();

    public string VarToCPPExpr(int tfv)
    {
        return cppExpressions[tfv];
    }

    public void InitReturnTypes()
    {
        validReturnType = new List<string>();
        validReturnType.Add("void");
        validReturnType.Add("int");
        validReturnType.Add("string");
        validReturnType.Add("float");
        validReturnType.Add("double");
        validReturnType.Add("bool");
    }

    public string ConvertCMSv2ToCpp(string CMSV2Exp)
    {
        for (int i = 0; i < cmsv2Expression.Count; i++)
        {
            if (cmsv2Expression[i] == CMSV2Exp)
            {
                return cppExpressions[i];
            }
        }
        return null;
    }
/*
    private string CCMSR(string CMSV2, string ciC)
    {
        List<string> names = SplitExpression(CMSV2);
        DefinedVar chosenVar = defineTarget(ciC, CMSV2, names);
        DefinedVar currentTarget = defineTarget(ciC, CMSV2, names);

        // Algorithm to find DefinedVar's

        return chosenVar.makeAccessPath

    }

    private DefinedVar defineTarget(string ciC, string cms, List<string> names)
    {
        DefinedVar currentTarget = new DefinedVar();

        foreach (DefinedVar dv in nodeTree)
        {
            if (names.Count == 1)
            {
                currentTarget.partOfClass = ciC;
            }

            if (dv.partOfClass == ciC && names.Count == 1)
            {
                if (dv.name == names[0])
                {
                    return dv;
                }
            }

            if (names.Count >= 2)
            {
                if (dv.makeCMSPath() == cms) return dv;
            }
        }

        return currentTarget;
    }

    private static List<string> SplitExpression(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return new List<string>();

        return expression.Split('.').ToList();
    }

*/
    public bool isValidReturnType(string t)
    {
        foreach (string r in validReturnType)
        {
            if (r == t)
            {
                return true;
            }
        }
        return false;
    }
}

public enum CECType
{
    Function,
    Enum,
    Enumerator,
    Variable,
    Class
}

public enum CECProtection
{
    Public,
    Protected,
    Private,
    None
}

public enum CECDefined
{
    Compiled,
    Local
}

public enum CECRefType
{
    Local,
    Static,
    Pointer
}

public enum CECVarType
{
    Class,
    Single,
    Namespace
}

public class DefinedVar
{
    public CECDefined made;
    public CECProtection prot;
    public CECRefType refType;
    public CECVarType varType;
    public string type;
    public string name;
    public string Namespace;
    public string partOfClass;
    public List<DefinedVar> children;
    public int parent;

    public DefinedVar(CECDefined m, CECProtection p, CECRefType rt, CECVarType vt, string t, string n, string ns, string poc)
    {
        made = m; prot = p; refType = rt; varType = vt; type = t; name = n; Namespace = ns; partOfClass = poc;
    }

    public string makeCMSPath()
    {
        if (partOfClass != "" && Namespace != "" && refType == CECRefType.Static)
        {
            return Namespace+"."+partOfClass+"."+name;
        }
        if (partOfClass != "" && refType == CECRefType.Static)
        {
            return partOfClass+"."+name;
        }
        return name;
    }

    public string RefToString(CECRefType c)
    {
        switch (c)
        {
            case CECRefType.Local:
            return ".";
            break;
            case CECRefType.Static:
            return "::";
            break;
            case CECRefType.Pointer:
            return "->";
            break;
        }

        return ".";
    }
/*
    public string makeCPPPath(List<DefinedVar> nodeTree)
    {
        if (parent == -1)
        {
            return name;
        }
        else
        {
            return getcppName();
        }
        return "";
    }

    string getcppName()
    {
        string curname = name;

        DefinedVar curVar = nodeTree[parent];

        string curname = "";

        bool keepMove = true;

        while (keepMove)
        {
            
        }
    }

    DefinedVar getcppmovenext(DefinedVar curVar, ref string curname)
    {
        int i = 0;
        foreach (DefinedVar dv in nodeTree)
        {

            if (curVar.parent == i)
            {
                curname = addToBack(curname, curVar.name+RefToString(curVar.refType));
                curVar = dv;
            }

            i++;

        }
    }*/

    private string addToBack(string c, string d)
    {
        return d+c;
    }
}

public class CECEntry
{
    public string        FullAccess  { get; init; }       // MyNs::MyClass::Foo
    public string        Namespace   { get; init; }       // MyNs::Sub
    public string        Name        { get; init; }       // Foo
    public string?       ReturnType  { get; init; }       // only for funcs/vars
    public CECType       Type        { get; init; }
    public CECProtection Protection  { get; init; }
    public List<CECEntry> SubEntries { get; } = new();    // class members, enum items
}

public static class CEC
{
    public static CppExpressionConverter FromSource(string buildDir)
    {
        CppExpressionConverter curC = new CppExpressionConverter();
        string rootPath = buildDir;
        string[] headerFiles = Directory.GetFiles(rootPath, "*.h", SearchOption.AllDirectories);

        Console.WriteLine("Parsing C++ Headers from source...");
        curC.InitReturnTypes();

        string translationFile = Path.Combine(buildDir, "TRANSLATOR.cmsv2t");
        Console.WriteLine("Checking for Translation file: " + translationFile);
        if (File.Exists(translationFile))
        {
            Console.WriteLine("Parsing Translation file...");
            string input = File.ReadAllText(translationFile);
            List<string> lines = input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).ToList();
            foreach (string line in lines)
            {
                int spaceIndex = line.IndexOf(' ');

                if (spaceIndex != -1)
                {
                    string firstPart = line.Substring(0, spaceIndex);
                    string secondPart = line.Substring(spaceIndex + 1);

                    curC.translationFileVars.Add(new CMSV2Var(secondPart, firstPart));
                }
            }
            for (int i = 0; i < lines.Count; i++)
            {
                int spaceIndex = lines[i].IndexOf(' ');

                if (spaceIndex != -1)
                {
                    string firstPart = lines[i].Substring(0, spaceIndex);
                    string secondPart = lines[i].Substring(spaceIndex + 1);

                    Console.WriteLine(secondPart);

                    curC.cmsv2Expression.Add(secondPart);
                }

                spaceIndex = lines[i + 1].IndexOf(' ');

                if (spaceIndex != -1)
                {
                    string firstPart = lines[i + 1].Substring(0, spaceIndex);
                    string secondPart = lines[i + 1].Substring(spaceIndex + 1);

                    curC.cppExpressions.Add(secondPart);
                    if (curC.validReturnType.Contains(firstPart) == false)
                    {
                        curC.validReturnType.Add(firstPart);
                    }
                }
                i++;
            }
        }

        foreach (var file in headerFiles)
        {
            Console.WriteLine("File: " + file);
            HandleHeader(File.ReadAllText(file), curC);
        }
        return curC;
    }

    private static void HandleHeader(string header, CppExpressionConverter cec)
    {
        var enums = NamespaceAwareEnumExtractor.ExtractEnumsWithNamespaces(header);
        foreach (var kvp in enums)
        {
            Console.WriteLine(kvp.Namespace != "" ? kvp.Namespace + "::" + kvp.Name : kvp.Name);
            foreach (var entry in kvp.Members)
            {
                string fullName = (kvp.Namespace != "" ? kvp.Namespace + "::" + kvp.Name : kvp.Name) + "::" + entry;
                Console.WriteLine(fullName);
                cec.cppExpressions.Add(fullName);
                cec.cmsv2Expression.Add(fullName.Replace("::", "."));
            }
            Console.WriteLine();
        }
    }
}

public class NamespaceAwareEnumExtractor
{
    public class EnumInfo
    {
        public string Namespace;
        public string Name;
        public List<string> Members;
    }

    public static List<EnumInfo> ExtractEnumsWithNamespaces(string code)
    {
        var enums = new List<EnumInfo>();
        var namespaceStack = new Stack<string>();

        // Remove comments first (as before)
        code = Regex.Replace(code, @"/\*.*?\*/", "", RegexOptions.Singleline);
        code = Regex.Replace(code, @"//.*?$", "", RegexOptions.Multiline);

        // Split code by lines for easier processing
        var lines = code.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        // Regexes
        var nsStartRegex = new Regex(@"^\s*namespace\s+(\w+)\s*\{");
        var nsEndRegex = new Regex(@"^\s*\}");
        var enumRegex = new Regex(@"\benum\s+(class\s+)?(\w+)\s*{");

        // Temporary variables to capture enum content
        EnumInfo currentEnum = null;
        bool insideEnum = false;
        int braceDepth = 0;

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();

            // Check namespace start
            var nsMatch = nsStartRegex.Match(line);
            if (nsMatch.Success)
            {
                namespaceStack.Push(nsMatch.Groups[1].Value);
                continue;
            }

            // Check namespace or enum block end
            if (line == "}")
            {
                if (insideEnum)
                {
                    braceDepth--;
                    if (braceDepth == 0)
                    {
                        enums.Add(currentEnum);
                        currentEnum = null;
                        insideEnum = false;
                    }
                }
                else if (namespaceStack.Count > 0)
                {
                    namespaceStack.Pop();
                }
                continue;
            }

            // Check enum start
            if (!insideEnum)
            {
                var enumMatch = enumRegex.Match(line);
                if (enumMatch.Success)
                {
                    insideEnum = true;
                    braceDepth = 1;
                    currentEnum = new EnumInfo
                    {
                        Namespace = string.Join("::", namespaceStack),
                        Name = enumMatch.Groups[2].Value,
                        Members = new List<string>()
                    };
                    // Remove everything up to '{' so we can parse members in next lines
                    var braceIndex = line.IndexOf('{');
                    line = line.Substring(braceIndex + 1).Trim();
                    if (!string.IsNullOrEmpty(line))
                    {
                        // Process this line as first member(s)
                        foreach (var part in line.Split(','))
                        {
                            var member = part.Trim();
                            if (!string.IsNullOrEmpty(member) && member != "}")
                            {
                                // Strip assigned values and comments as needed
                                int eqIdx = member.IndexOf('=');
                                if (eqIdx >= 0) member = member.Substring(0, eqIdx).Trim();
                                currentEnum.Members.Add(member);
                            }
                        }
                    }
                    continue;
                }
            }
            else
            {
                // We are inside enum, accumulate members
                if (line.Contains("{")) braceDepth++;
                if (line.Contains("}")) braceDepth--;

                var cleanLine = line.Replace("}", "").Trim();
                if (!string.IsNullOrEmpty(cleanLine))
                {
                    foreach (var part in cleanLine.Split(','))
                    {
                        var member = part.Trim();

                        // Remove assignment if present (e.g., Green = 1)
                        int eqIdx = member.IndexOf('=');
                        if (eqIdx >= 0)
                            member = member.Substring(0, eqIdx).Trim();

                        // Remove trailing semicolons or braces
                        member = member.TrimEnd(';', '}');

                        if (!string.IsNullOrEmpty(member))
                            currentEnum.Members.Add(member);
                    }
                }

                if (braceDepth == 0)
                {
                    enums.Add(currentEnum);
                    currentEnum = null;
                    insideEnum = false;
                }
            }
        }

        return enums;
    }
}

