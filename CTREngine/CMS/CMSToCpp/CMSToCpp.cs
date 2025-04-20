using System;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;

namespace CMS
{
    public class VariableCont
    {
        public string rawVar;
        public string Namespace;

        public static string RemoveFirstOccurrence(string input, string toRemove)
        {
            int index = input.IndexOf(toRemove);
            if (index != -1)
            {
                return input.Remove(index, toRemove.Length);
            }
            return input; // Return original if substring not found
        }

        public VariableCont(string rawVar, string Namespace)
        {
            this.rawVar = rawVar;
            this.Namespace = Namespace;
        }
    }
    public class ConvertResult
    {
        public string OutputScript;
        public List<VariableCont> functions;
        public List<VariableCont> variables;
        public List<string> includes  = new List<string>();
        public List<string> namespaces= new List<string>();
        public List<string> anamespaces= new List<string>();
        public ConvertProcess CP;

        public ConvertResult(ConvertProcess CP)
        {
            this.CP = CP;
            variables  = new List<VariableCont>();
            functions  = new List<VariableCont>();
        }
        public string GenerateHFile()
        {
            string currentH = "";
            string includeName = CP.scriptName.ToUpper().Replace(" ","_")+"_H";
            currentH += "#ifndef "+includeName+"\n";
            currentH += "#define "+includeName+"\n\n";

            foreach (string inc in includes)
            {
                if (inc.Contains("CTR/ScriptScene")) continue;
                currentH += "#include "+inc;
                currentH += "\n";
            }
            currentH += "\n";
            foreach (string inc in CP.toUse)
            {
                currentH += "using "+inc+";";
                currentH += "\n";
            }

            currentH += "class "+CP.scriptName+" {\npublic:\n";

            foreach (VariableCont var in variables)
            {
                if (var.Namespace == "")
                {
                    currentH += var.rawVar+"\n";
                }
            }
            foreach (VariableCont var in functions)
            {
                if (var.Namespace == "")
                {
                    var.rawVar = VariableCont.RemoveFirstOccurrence(var.rawVar, CP.scriptName+"::");
                    currentH += var.rawVar+"\n";
                }
            }

            currentH += "};\n";

            foreach (string n in namespaces)
            {
                currentH += "namespace "+n+" {\n";
                foreach (VariableCont var in variables)
                {
                    if (var.Namespace == n)
                    {
                        currentH += "extern "+var.rawVar+"\n";
                    }
                }
                foreach (VariableCont var in functions)
                {
                    if (var.Namespace == n)
                    {
                        var.rawVar = VariableCont.RemoveFirstOccurrence(var.rawVar, CP.scriptName+"::");
                        currentH += "extern "+var.rawVar+"\n";
                    }
                }
                currentH += "}\n";
            }

            currentH += "\n#endif";
            return currentH;
        }
    }
    public class ConvertProcess
    {
        public int currentTabs;
        public int currentLine;
        public int spacesBetween;
        public string cmsScript;
        public string cppScript;
        public List<string> toInclude;
        public List<string> toUse;
        public List<string> variablesToAdd;
        public bool makeLocalVars = false;
        public string[] instructList;
        public List<string> globalInstrucList;
        public bool canClearVarOptions = true;
        public string scriptName;
        public ConvertResult CV;

        public List<string> nextVarOptions;
        public bool NextIsVar = false;
        public bool NextIsFunc = false;
        public bool NextIsLVar = false;
        public bool NextIsFCall = false;
        public bool NextIsSetVar = false;
        public bool NextIsRef = false;
        public bool NextIsSetFunc = false;
        public bool isInFunction = false;
        public bool cppOverride;
        public int endBracketCount = 0;
        public List<int> currentFinishLines = new List<int>();
        public List<string> namespaces = new List<string>();

        public string GetNamespace()
        {
            if (namespaces.Count > 0)
            {
                return namespaces[namespaces.Count-1];
            }
            return "";
        }

        public void ClearOptions()
        {
            if (canClearVarOptions)
            {
                nextVarOptions = new List<string>();
                            NextIsVar = false;
                            NextIsFunc = false;
                            NextIsFCall = false;
                            NextIsLVar = false;
                            NextIsSetVar = false;
                            NextIsRef = false;
                            NextIsSetFunc = false;
            }
        }

        public void MakeSpace()
        {
            for (int i = 0; i < spacesBetween; i++)
            {
                cppScript += "\n";
                currentLine++;
            }
        }

        public string GetCurrentVarOptions()
        {
            string c = "";
            foreach (string s in nextVarOptions)
            {
                c += s+" ";
            }
            return c;
        }
        
        public ConvertProcess(string cmsScript, int spacesBetween)
        {
            this.cmsScript = cmsScript;
            this.spacesBetween = spacesBetween;
            CV = new ConvertResult(this);
        }
    }
    public static class Cpp
    {
        public static List<string> defaultIncludes = new List<string>()
        {
            "<memory>",
            "<string>",
            "<vector>",
            "<iostream>"
        };
        public static List<string> defaultUsing = new List<string>()
        {
            "namespace std"
        };
        public static ConvertResult ConvertToCpp(string CMS, string scriptName, int spacesBetween = 2)
        {
            ConvertProcess CP = new ConvertProcess(CMS, spacesBetween);
            CP.scriptName = scriptName;
            CP.instructList = GetInstructions(CMS);
            List<string> globalInstrucs = new List<string>();
            for (int i = 0; i < CP.instructList.Length-1; i++)
            {
                string inst = CP.instructList[i];
                if (isInstructionFunction(GetInstructionName(inst)))
                {
                    globalInstrucs.Add(inst);
                    if (GetNextEndInstruction(i+1, ref CP) != -1)
                    {
                        i = GetNextEndInstruction(i+1, ref CP); 
                    }
                    else
                    {
                        Console.WriteLine("GetNextEnd broke");
                        break;
                    }
                }
                else
                {
                    globalInstrucs.Add(inst);
                }
            }
            Console.WriteLine("Global instrucs:");
            foreach (string s in globalInstrucs)
            {
                Console.WriteLine(s);
            }
            CP.globalInstrucList = globalInstrucs;
            CP.cppScript = "// Generated with CMS-To-Cpp ============================ Made with Cartridge Tilt Retro";
            CP.MakeSpace();

            RegisterIncludes(ref CP);

            CP.MakeSpace();

            RegisterUsings(ref CP);

            CP.MakeSpace();

            RegisterVariables(ref CP);

            CP.MakeSpace();

            CP.CV.OutputScript = CP.cppScript;
            return CP.CV;
        }

        public static string ModifyInstruction(string instruction)
        {
            instruction = instruction.Replace("\r\n", "").Replace("\n", "");
            instruction = RemoveLeadingCharacters(instruction,' ');
            instruction = instruction.Replace("\r\n", "").Replace("\n", "");
            instruction = RemoveLeadingCharacters(instruction,' ');
            return instruction;
        }

        static string RemoveLeadingCharacters(string input, char character)
        {
            return input.TrimStart(character);
        }

        public static string[] GetInstructions(string script)
        {
            string[] instrucs = script.Split(new string[] { ";" }, StringSplitOptions.None);
            for (int i = 0; i < instrucs.Length; i++)
            {
                instrucs[i] = ModifyInstruction(instrucs[i]);
            }
            return instrucs;
        }

        // CMS to CPP functions

        public static void RegisterIncludes (ref ConvertProcess CP)
        {
            CP.toInclude = new List<string>();
            CP.toInclude.AddRange(Cpp.defaultIncludes);
            CP.toUse = new List<string>();
            CP.toUse.AddRange(Cpp.defaultUsing);
            CP.toInclude.Add($"\"{CP.scriptName+".h"}\"");
            foreach (string instruc in CP.instructList)
            {
                if (instruc.StartsWith(".get "))
                {
                    var match = Regex.Match(instruc, @"\.get\s+(.*)");
                    if (match.Success)
                    {
                        Console.WriteLine(match.Groups[1].Value);
                        CP.toInclude.Add($"\"{match.Groups[1].Value}.h\"");
                        //CP.toUse.Add("namespace "+match.Groups[1].Value);
                    }
                }
            }

            string includeString = "";

            foreach (string inc in CP.toInclude)
            {
                includeString += "#include "+inc;
                includeString += "\n";
            }

            CP.CV.includes = CP.toInclude;

            CP.cppScript += includeString;
        }

        public static void RegisterUsings (ref ConvertProcess CP)
        {
            string includeString = "";

            foreach (string instruc in CP.instructList)
            {
                if (instruc.StartsWith(".using "))
                {
                    var match = Regex.Match(instruc, @"\.using\s+(.*)");
                    if (match.Success)
                    {
                        Console.WriteLine(match.Groups[1].Value);
                        CP.toUse.Add($"namespace {match.Groups[1].Value}");
                        //CP.toUse.Add("namespace "+match.Groups[1].Value);
                    }
                }
            }

            foreach (string inc in CP.toUse)
            {
                includeString += "using "+inc+";";
                includeString += "\n";
            }

            CP.CV.anamespaces = CP.toUse;

            CP.cppScript += includeString;
        }
    
        public static bool isInstructionFunction(string instruction)
        {
            foreach (string voidInst in CMS.Interpreter.FunctionInstructions)
            {
                if (instruction.ToLower().StartsWith(voidInst))
                {
                    return true;
                }
            }
            return false;
        }

        public static string GetInstructionName(string instruction)
        {
            instruction = instruction.Remove(0,1);
            string internalInstruction = instruction.Split(' ')[0];
            return internalInstruction;
        }

        public static void InsertFunctionContent(ref ConvertProcess CP, int index)
        {
            CP.isInFunction = true;
            Console.WriteLine("Insert Function Content  START!!!!");
            CP.makeLocalVars = true;
            int nextEnd = -1;

            int skipCount = 0;

            for (int i = index; i < CP.instructList.Length-1; i++)
            {
                Console.WriteLine("Insert Function Instruc: "+CP.instructList[i]);
                string instruction = CP.instructList[i];
                string instructionName = GetInstructionName(instruction);

                if (isInstructionFunction(instructionName))
                {
                    Console.WriteLine("Is a function from IFC");
                    skipCount++;
                }

                if (instruction.StartsWith("-end"))
                {
                    Console.WriteLine("End Witnessed!");
                    skipCount--;
                }

                if (instruction.StartsWith("-end") && skipCount == -1)
                {
                    nextEnd = i;
                    break;
                }
            }

            if (nextEnd == -1)
                return;

            Console.WriteLine("Found End on instruction: "+nextEnd);
            int endsNeeded = 0;

            for (int i = index; i < nextEnd; i++)
            {
                string inst = CP.instructList[i];
                Console.WriteLine("Function is handling: "+CP.instructList[i]);
                if (CP.instructList[i].StartsWith("["))
                {
                    RegisterVarOption(ref CP, CP.instructList[i], i);
                }
                else if (CP.NextIsLVar && !CP.cppOverride)
                {
                    CP.cppScript += GetCppVarFromInstruc(CP.instructList[i], ref CP)+"\n";

                    CP.ClearOptions();
                }
                else if (CP.NextIsVar && !CP.cppOverride)
                {
                    CP.makeLocalVars = false;
                    CP.cppScript += GetCppVarFromInstruc(CP.instructList[i], ref CP)+"\n";

                    CP.ClearOptions();
                    CP.makeLocalVars = true;
                }
                else if (CP.NextIsFCall && !CP.NextIsRef && !CP.cppOverride)
                {
                    Console.WriteLine("Adding "+CP.instructList[i]+" from instruction "+i);
                    CP.cppScript += CP.instructList[i].Replace(".", "::")+";\n";

                    CP.ClearOptions();
                }
                else if (CP.NextIsFCall && CP.NextIsRef && !CP.cppOverride)
                {
                    Console.WriteLine("Adding "+CP.instructList[i]+" from instruction "+i);
                    CP.cppScript += CP.instructList[i].Replace(".", "->")+";\n";

                    CP.ClearOptions();
                }
                else if (CP.NextIsSetVar && !CP.NextIsRef)
                {
                    CP.cppScript += CP.instructList[i]+";\n"/*.Replace(".", "->")+";\n"*/;

                    CP.ClearOptions();
                }
                else if (CP.NextIsSetVar && CP.NextIsRef)
                {
                    CP.cppScript += CP.instructList[i].Replace(".", "->")+";\n";

                    CP.ClearOptions();
                }
                else if (CP.NextIsSetFunc && !CP.NextIsRef)
                {
                    CP.cppScript += CP.instructList[i].Replace(".", "::")+";\n";
                    CP.ClearOptions();
                }
                else if (inst.StartsWith("-return") && !CP.cppOverride)
                {
                    var match = Regex.Match(inst, "-return\\s*(.*)");

                    Console.WriteLine("Trying to add return");
                    if (match.Success)
                    {
                        string toR = match.Groups[1].Value;
                        Console.WriteLine("Return added, toR: "+toR);
                        CP.cppScript += "return "+toR+";\n";
                    }
                }
                else if (inst.StartsWith(".if") && !CP.cppOverride)
                {
                    string instruction = inst.Remove(0,1);
                    CP.cppScript += instruction+" {\n";
                    endsNeeded++;
                }
                else if (inst.StartsWith("-end") && !CP.cppOverride && endsNeeded > 0)
                {
                    endsNeeded--;
                    CP.cppScript += "}\n";
                }
            }

            CP.cppScript += "}\n";
            CP.makeLocalVars = false;
            CP.isInFunction = false;
        }

        public static string GetCppVarFromInstruc(string inst, ref ConvertProcess CP, bool isCheck = true)
        {
            if (inst.StartsWith(".")&& CP.cppOverride == false)
            {
                string name = "";
                string value = "";
                string type = "";
                var match = Regex.Match(inst, @"\.(\S+)\s+""(.*?)""(\s?.*)");
                if (match.Success)
                {
                    name = match.Groups[2].Value;
                    value = match.Groups[3].Value;
                    type = match.Groups[1].Value;
                }
                if (CP.makeLocalVars)
                {
                    //unique_ptr<int> num = make_unique<int>(i);
                    string rtype = type;
                    type = $"unique_ptr<{type}>";
                    if (value == "")
                    {
                        value = $"make_unique<{rtype}>(0)";
                    }
                    else
                    {
                        var pattern = @"\s*=\s*(.*)";
                        var match2 = Regex.Match(value, pattern);
                        if (match2.Success)
                        {
                            value = $" = make_unique<{rtype}>({match2.Groups[1].Value})";
                        }
                    }
                }
                string toAdd =
                CP.GetCurrentVarOptions()+
                    type+" "+
                    name+
                    value+
                    ";";

                if (!CP.isInFunction && !isCheck)
                {
                    CP.CV.variables.Add(new VariableCont(CP.GetCurrentVarOptions()+type+" "+name+";", CP.GetNamespace()));
                    Console.WriteLine("VERY IMPORTANT !!!!!!!!!!!!!!!!!!!! ADDED A NEW VARIABLE CONT FOR HEADER FILE GENERATION!");
                }

                return toAdd;
            }
            return null;
        }

        public static int GetNextEndInstruction(int startInstruction, ref ConvertProcess CP, string toSearch = "-end")
        {
            Console.WriteLine("Trying to find next " + toSearch + " from instruction " + startInstruction);
            int skipCount = 0;

            for (int i = startInstruction; i < CP.instructList.Length-1; i++)
            {
                string instruction = CP.instructList[i];
                string instructionName = GetInstructionName(instruction);

                if (isInstructionFunction(instructionName))
                {
                    skipCount++;
                    Console.WriteLine("Skipped: " + instructionName);
                }

                if (instruction.StartsWith("-end"))
                {
                    Console.WriteLine("End Witnessed");
                    skipCount--;
                }

                if (instruction.StartsWith(toSearch) && skipCount <= 0)
                {
                    Console.WriteLine("Next " + toSearch + " is on Instruction " + i);
                    return i;
                }
            }

            Console.WriteLine("No " + toSearch + " was found.");
            return -1;
        }

        public static void RegisterVariables(ref ConvertProcess CP)
        {
            CP.variablesToAdd = new List<string>();
            CP.nextVarOptions = new List<string>();
            for (int i = 0; i < CP.instructList.Length-1; i++)
            {
                string inst = CP.instructList[i];
                int nextI = i;
                if (isInstructionFunction(GetInstructionName(inst)))
                {
                    if (GetNextEndInstruction(i+1, ref CP) != -1)
                    {
                        nextI = GetNextEndInstruction(i+1, ref CP);
                        Console.WriteLine("IS A FUNCTION FROM VARIABLES, NEW INDEX IS "+i+", FUNCTION IS: "+inst);
                    }
                    else
                    {
                        Console.WriteLine("GetNextEnd broke");
                        break;
                    }
                }
                Console.WriteLine("Editing Isntruction: "+inst);
                if (inst.StartsWith("["))
                {
                    RegisterVarOption(ref CP, inst, i);
                }
                else if (CP.NextIsVar) {
                    if (GetCppVarFromInstruc(inst, ref CP, true) != null)
                    {
                        string toAdd = GetCppVarFromInstruc(inst, ref CP, false);
                        CP.cppScript += toAdd+"\n";
                        CP.ClearOptions();
                    }
                }
                else if (inst.StartsWith(".set") && CP.NextIsFunc && CP.cppOverride == false)
                {
                    var pattern = @"\.set(.*?)\s+""(.*?)""\s*\((.*?)\)";
                    var match = Regex.Match(inst, pattern);

                    if (match.Success)
                    {
                        string typeName = match.Groups[1].Value;
                        string name = match.Groups[2].Value;
                        string parameters = match.Groups[3].Value;

                        name = CP.scriptName+"::"+name;

                        string toAdd = CP.GetCurrentVarOptions()+typeName+" "+name+" ("+parameters+") "+" {";
                        toAdd = VariableCont.RemoveFirstOccurrence(toAdd, "static");
                        CP.CV.functions.Add(new VariableCont(CP.GetCurrentVarOptions()+typeName+" "+name+" ("+parameters+");", CP.GetNamespace()));
                        CP.cppScript += toAdd+"\n";
                        CP.ClearOptions();
                        Console.WriteLine("Inserting function at "+(i+1));
                        InsertFunctionContent(ref CP, i+1);
                    }
                }
                else if (inst.StartsWith(".setNamespace") && CP.cppOverride == false)
                {
                    string instruction;
                    instruction = inst.Remove(0,14);
                    string param = instruction;
                    string name = param.Remove(0,1);
                    name = name.Substring(0, name.Length - 1);
                    Console.WriteLine("Setting Namespace to"+name+" from ins "+i);
                    CP.MakeSpace();
                    CP.cppScript += "namespace "+name+" {\n";
                    CP.endBracketCount++;
                    CP.namespaces.Add(name);
                    CP.CV.namespaces.Add(name);
                }
                else if (inst.StartsWith("-finish") && !CP.cppOverride)
                {
                    if (CP.endBracketCount > 0)
                    {
                        CP.endBracketCount--;
                        if (CP.namespaces.Count > 0)
                        {
                            CP.namespaces.RemoveAt(CP.namespaces.Count-1);
                        }
                        CP.cppScript+= "}\n";
                    }
                }

                i = nextI;
            }
        }

        public static string GetCppOverride(ref ConvertProcess CP, int index)
        {
            string cppOverride = "";
            int lastIndex = -1;

            for (int i = index+1; i < CP.instructList.Length; i++)
            {
                string instruction = CP.instructList[i];

                if (ModifyInstruction(instruction).StartsWith("[CMS]"))
                {
                    lastIndex = i;
                    break;
                }
            }

            if (lastIndex != -1)
            {
                for (int i = index+1; i < lastIndex; i++)
                {
                    cppOverride += CP.instructList[i]+";"+"\n";
                }
            }

            return cppOverride;
        }

        public static void InsertCppOverride(ref ConvertProcess CP, int index)
        {
            string cppOverride = "";
            int lastIndex = -1;

            for (int i = index+1; i < CP.instructList.Length; i++)
            {
                string instruction = CP.instructList[i];

                if (ModifyInstruction(instruction).StartsWith("[CMS]"))
                {
                    lastIndex = i;
                    break;
                }
            }

            if (lastIndex != -1)
            {
                for (int i = index+1; i < lastIndex; i++)
                {
                    cppOverride += CP.instructList[i]+";"+"\n";
                }
            }

            CP.cppScript += cppOverride;
        }

        public static void RegisterVarOption(ref ConvertProcess CP, string inst, int index)
        {
            var match = Regex.Match(inst, @"\[(.*?)\]");

            if (match.Success)
            {
                Console.WriteLine(match.Groups[1].Value);

                switch (match.Groups[1].Value)
                {
                    case "Const":
                    CP.nextVarOptions.Add("constexpr");
                    break;
                    case "Public":
                    CP.nextVarOptions.Add("public");
                    break;
                    case "Private":
                    CP.nextVarOptions.Add("private");
                    break;
                    case "Protected":
                    CP.nextVarOptions.Add("protected");
                    break;
                    case "Inline":
                    CP.nextVarOptions.Add("inline");
                    break;
                    case "Static":
                    CP.nextVarOptions.Add("static");
                    break;
                    case "Mutable":
                    CP.nextVarOptions.Add("mutable");
                    break;
                    case "Virtual":
                    CP.nextVarOptions.Add("virtual");
                    break;
                    case "Override":
                    CP.nextVarOptions.Add("override");
                    break;
                    case "Final":
                    CP.nextVarOptions.Add("final");
                    break;
                    case "Break Options":
                    CP.canClearVarOptions = true;
                    CP.ClearOptions();
                    break;
                    case "Keep Options":
                    CP.canClearVarOptions = false;
                    break;
                    case "Variable":
                    CP.NextIsVar = true;
                    break;
                    case "Local":
                    CP.NextIsLVar = true;
                    break;
                    case "Function":
                    CP.NextIsFunc = true;
                    break;
                    case "FCall":
                    CP.NextIsFCall = true;
                    break;
                    case "SVariable":
                    CP.NextIsSetVar = true;
                    break;
                    case "SFunc":
                    CP.NextIsSetFunc = true;
                    break;
                    case "Ref":
                    CP.NextIsRef = true;
                    break;
                    case "C++":
                    CP.cppOverride = true;
                    InsertCppOverride(ref CP, index);
                    break;
                    case "CMS":
                    CP.cppOverride = false;
                    break;
                }
            }
        }
    }
}