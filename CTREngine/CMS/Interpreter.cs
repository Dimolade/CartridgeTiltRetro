using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using org.matheval;
using System.Globalization;

namespace CMS
{
    // Function Handlers
    public class FireEventHandler : EventArgs
    {
        public CMSScript script;
        public int line;
        public string fullInstructions;
        public char operat;
        
        public FireEventHandler(CMSScript script, int line, string fullInstructions, char operat)
        {
            this.script = script;
            this.line = line;
            this.fullInstructions = fullInstructions;
            this.operat = operat;
        }
    }
    public class InternalFunctionHandler
    {
        public string instruction;
        public string prefix = ".";
        public delegate void FireEvent(object sender, FireEventHandler e);

        public event FireEvent onFire;

        public void Fire(FireEventHandler FEH)
        {
            onFire?.Invoke(this, FEH);
        }

        public InternalFunctionHandler(string instruction, string prefix = ".")
        {
            this.instruction = instruction;
            this.prefix = prefix;
        }
    }
    // CMS Attributes
    public class CMSAttributes
    {
        public bool executeNextOnStart = false;

        public CMSAttributes(bool executeNextOnStart = false)
        {
            this.executeNextOnStart = executeNextOnStart;
        }
    }
    // CMS Vars
    public class CMSInt
    {
        public string name;
        public int value;
        public string Namespace;
        public CMSScript Owner;

        public CMSInt Clone()
        {
            return new CMSInt(name,value,Namespace,Owner);
        }
        
        public CMSInt(string name, int value, string Namespace, CMSScript Owner)
        {
            this.name = name;
            this.value = value;
            this.Namespace = Namespace;
            this.Owner = Owner;
        }
    }
    public class CMSVoid
    {
        public string name;
        public int instructionStart;
        public string Namespace;
        public CMSScript Owner;
        public List<object> Params;

        public CMSVoid Clone()
        {
            return new CMSVoid(name,instructionStart,Namespace,Params, Owner);
        }
        
        public CMSVoid(string name, int instructionStart, string Namespace, List<object> Params, CMSScript Owner)
        {
            this.name = name;
            this.instructionStart = instructionStart;
            this.Namespace = Namespace;
            this.Owner = Owner;
            this.Params = Params;
        }
    }
    public class CMSClass
    {
        public string name;
        public string varName;
        public string Namespace;
        public CMSScript Owner;
        public List<CMSInt> intVars;
        public List<CMSVoid> voidVars;
        
        public CMSClass(string name, string Namespace, CMSScript Owner, List<CMSInt> intVars, List<CMSVoid> voidVars)
        {
            this.name = name;
            this.Namespace = Namespace;
            this.Owner = Owner;
            this.intVars = intVars;
            this.voidVars = voidVars;
        }
    }
    // CMS Script
    public class CMSScript
    {
        public string script;
        public List<string> Namespace = new List<string>() {"DefaultCMS"};
        public bool running = false;
        //vars
        public CMSAttributes attributes = new CMSAttributes();
        public List<CMSInt> intVars = new List<CMSInt>();
        public List<CMSInt> localIntVars = new List<CMSInt>();
        public List<CMSInt> classIntVars = new List<CMSInt>();
        public List<CSCMSInt> csIntVars = new List<CSCMSInt>();
        public List<CMSVoid> voidVars = new List<CMSVoid>();
        public List<CMSVoid> namespaceVoidVars = new List<CMSVoid>();
        public List<CMSVoid> classVoidVars = new List<CMSVoid>();
        public List<CSCMSVoid> csVoidVars = new List<CSCMSVoid>();
        public List<CMSClass> classVars = new List<CMSClass>();
        public List<CMSClass> namespaceClassVars = new List<CMSClass>();
        public List<CSCMSClass> csClassVars = new List<CSCMSClass>();
        public List<CMSClass> madeClassVars = new List<CMSClass>();
        public List<CSCMSClass> madeCsClassVars = new List<CSCMSClass>();
        public List<string> registeredNamespaces = new List<string>();
        public List<int> currentFinishLine = new List<int>();
        public bool inClass = false;
        public string currentClassName = "";
        //instruct
        public string[] instructions;
        bool makeLocalVariables = false;
        bool inLimbo = false;
        int currentLimboMaster = -1;

        //namespace vars
        public List<CMSInt> namespaceIntVars = new List<CMSInt>();

        public CMSScript(string s, string defaultNamespace = "DefaultCMS")
        {
            script = s;
            Namespace[0] = defaultNamespace;
        }

        public string GetNamespace()
        {
            return Namespace[Namespace.Count-1];
        }

        public CMSInt GetCMSInt(string name)
        {
            foreach (CMSInt Int in intVars)
            {
                if (Int.name == name)
                {
                    return Int;
                }
            }
            foreach (CMSInt Int in localIntVars)
            {
                if (Int.name == name)
                {
                    return Int;
                }
            }
            foreach (CMSInt Int in classIntVars)
            {
                if (Int.name == name)
                {
                    return Int;
                }
            }
            return null;
        }

        public CMSVoid GetCMSVoid(string name)
        {
            foreach (CMSVoid Int in voidVars)
            {
                if (Int.name == name)
                {
                    return Int;
                }
            }
            foreach (CMSVoid Int in namespaceVoidVars)
            {
                if (Int.name == name)
                {
                    return Int;
                }
            }
            return null;
        }

        public CMSClass GetCMSClass(string name)
        {
            foreach (CMSClass Int in classVars)
            {
                if (Int.name == name)
                {
                    return Int;
                }
            }
            foreach (CMSClass Int in namespaceClassVars)
            {
                if (Int.name == name)
                {
                    return Int;
                }
            }
            return null;
        }

        public CSCMSClass GetCSCMSClass(string name)
        {
            foreach (CSCMSClass Int in csClassVars)
            {
                if (Int.name == name)
                {
                    return Int;
                }
            }
            return null;
        }

        public static async Task StartCoroutine(IEnumerator routine)
        {
            while (routine.MoveNext())
            {
                if (routine.Current is Task task)
                {
                    await task; // Waits if Current is an async Task
                }
                else
                {
                    await Task.Delay(0); // Continue on the next "frame"
                }
            }
        }

        //Interpreting

        public async Task Run()
        {
            running = true;
            Interpreter();
        }

        string RemoveLeadingCharacters(string input, char character)
        {
            return input.TrimStart(character);
        }

        public void PrintInstructions(string[] instructions)
        {
            foreach (string s in instructions)
            {
                Console.WriteLine(ModifyInstruction(s));
            }
        }

        public void SetVariableValue(string name, string value)
        {
            foreach (CMSInt Int in intVars)
            {
                if (Int.name == name)
                {
                    Int.value = Convert.ToInt32(ParseExpressionD(value));
                }
            }
            foreach (CMSInt Int in localIntVars)
            {
                if (Int.name == name)
                {
                    Int.value = Convert.ToInt32(ParseExpressionD(value));
                }
            }
        }

        public string GetInstruction(int index)
        {
            return ModifyInstruction(instructions[index]);
        }

        public string GetInstructionName(string instruction)
        {
            instruction = instruction.Remove(0,1);
            string internalInstruction = instruction.Split(' ')[0];
            return internalInstruction;
        }

        public List<string> GetExpressionErrors(string expression)
        {
            Expression exp = new Expression(expression);
            foreach (CMSInt Int in intVars)
            {
                exp.Bind(Int.name, Int.value);
            }
            List<String> errors = exp.GetError();
            return errors;
        }

        public void BindExpressionCMS(Expression exp)
        {
            foreach (CMSInt Int in intVars)
                {
                    exp.Bind(Int.name, Int.value);
                }
                foreach (CMSInt Int in localIntVars)
                {
                    exp.Bind(Int.name, Int.value);
                }
                foreach (CMSInt Int in namespaceIntVars)
                {
                    exp.Bind(Int.name, Int.value);
                }
                foreach (CMSInt Int in classIntVars)
                {
                    exp.Bind(Int.name, Int.value);
                }
                foreach (CSCMSInt Int in csIntVars)
                {
                    exp.Bind(Int.name, Int.value);
                }
                foreach (CMSClass Int in madeClassVars)
                {
                    foreach (CMSInt INT in Int.intVars)
                    {
                        exp.Bind(Int.name+"."+INT.name, INT.value);
                    }
                }
        }

        public Decimal ParseExpressionD(string expression)
        {
            try
            {
                Expression exp = new Expression(expression);
                BindExpressionCMS(exp);
                return exp.Eval<Decimal>();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error parsing expression: " + ex.Message + " Source: "+ex.Source+" StackTrace: "+ex.StackTrace);
                return -1;
            }
        }

        public Object ParseExpression(string expression)
        {
            try
            {
                Expression exp = new Expression(expression);
                BindExpressionCMS(exp);
                return exp.Eval();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error parsing expression: " + ex.Message + " Source: "+ex.Source+" StackTrace: "+ex.StackTrace);
                return null;
            }
        }

        public string ModifyInstruction(string instruction)
        {
            instruction = instruction.Replace("\r\n", "").Replace("\n", "");
            instruction = RemoveLeadingCharacters(instruction,' ');
            instruction = instruction.Replace("\r\n", "").Replace("\n", "");
            instruction = RemoveLeadingCharacters(instruction,' ');
            return instruction;
        }

        public void HandleInternalInstruction(string instruction, CMSScript script, int line)
        {
            string origInstruc = instruction;
            char operat = instruction[0];
            instruction = instruction.Remove(0,1);
            string internalInstruction = instruction.Split(' ')[0];
            foreach (InternalFunctionHandler ifh in CMS.Interpreter.InternalFunctions)
            {
                if (ifh.instruction == internalInstruction && ifh.prefix == ".")
                {
                    ifh.Fire(new CMS.FireEventHandler(script,line,origInstruc, operat));
                }
            }
        }

        public void HandleAttributeInstruction(string instruction, CMSScript script, int line)
        {
            foreach (InternalFunctionHandler ifh in CMS.Interpreter.AttributeHandlers)
            {
                if (ifh.prefix == "[")
                {
                    ifh.Fire(new CMS.FireEventHandler(script,line,instruction, '['));
                }
            }
        }

        public void HandleVariableInstruction(string instruction, CMSScript script, int line)
        {
            foreach (InternalFunctionHandler ifh in CMS.Interpreter.VarHandlers)
            {
                if (ifh.prefix == "_")
                {
                    ifh.Fire(new CMS.FireEventHandler(script,line,instruction, '_'));
                }
            }
        }

        public void HandleVoidInstruction(string instruction, CMSScript script, int line)
        {
            foreach (InternalFunctionHandler ifh in CMS.Interpreter.VoidHandlers)
            {
                if (ifh.prefix == ">")
                {
                    ifh.Fire(new CMS.FireEventHandler(script,line,instruction, '>'));
                }
            }
            foreach (CSCMSScript ifh in CMS.Interpreter.RunningCSCMSScripts)
            {
                foreach (CSCMSVoid v in ifh.Functions)
                {
                    v.func(this, new CMS.FireEventHandler(script,line,instruction, '>'));
                }
            }
        }

        public void HandleMinusInstruction(string instruction, CMSScript script, int line)
        {
            string origInstruc = instruction;
            char operat = instruction[0];
            instruction = instruction.Remove(0,1);
            string internalInstruction = instruction.Split(' ')[0];
            foreach (InternalFunctionHandler ifh in CMS.Interpreter.MinusFunctions)
            {
                if (ifh.instruction == internalInstruction && ifh.prefix == "-")
                {
                    ifh.Fire(new CMS.FireEventHandler(script,line,origInstruc, operat));
                }
            }
        }

        public void AddCMSInt(CMSInt Int)
        {   
            if (inClass)
            {
                classIntVars.Add(Int);
            }
            else if (makeLocalVariables)
            {
                localIntVars.Add(Int);
            }
            else
            {
                intVars.Add(Int);
            }
        }

        public void AddCMSVoid(CMSVoid Int)
        {
            if (inClass)
            {
                classVoidVars.Add(Int);
            }
            else
            {
                voidVars.Add(Int);
            }
        }

        public void RunInstructionList(int startInstruction)
        {
            makeLocalVariables = true;
            Console.WriteLine("Running Instruction List from Instruction: " + startInstruction);

            int currentInstruction = startInstruction; 
            int nextFinishInstructionCount = GetNextEndInstruction(startInstruction);

            string conditionLine = GetInstruction(currentInstruction);
            for (int i = startInstruction; i < nextFinishInstructionCount; i++)
            {
                if (isInstructionFunction(GetInstruction(i)))
                {
                    if (!inLimbo)
                    {
                        inLimbo = true;
                        currentLimboMaster = startInstruction;
                    }
                    InterpretInstruction(i);
                    i = GetNextEndInstruction(i);
                }
                InterpretInstruction(i);
            }
            if (startInstruction == currentLimboMaster)
            {
                inLimbo = false;
                currentLimboMaster = -1;
                localIntVars.Clear();
            }
            if (!inLimbo)
            {
                makeLocalVariables = false;
            }
        }

        public int GetNextEndInstruction(int startInstruction, string toSearch = "-end")
        {
            Console.WriteLine("Trying to find next " + toSearch + " from instruction " + startInstruction);
            int skipCount = 0;

            for (int i = startInstruction; i < instructions.Length; i++)
            {
                string instruction = GetInstruction(i);
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

        public bool isInstructionFunction(string instruction)
        {
            foreach (string voidInst in CMS.Interpreter.FunctionInstructions)
            {
                if (instruction.StartsWith(voidInst))
                {
                    return true;
                }
            }
            return false;
        }

        public void InterpretInstruction(int instructionCount)
        {
            if (instructionCount >= instructions.Length || instructionCount < 0)
            {
                running = false;
                Console.WriteLine("Intepreting outside of Bounds!");
                return;
            }
            try {
                int currentExpressionType = -1;
                
                string currentInstruction = ModifyInstruction(instructions[instructionCount]);
                Console.WriteLine(currentInstruction+" - Being Ran");
                if (currentInstruction.StartsWith("."))
                {
                    currentExpressionType = 0;
                    HandleInternalInstruction(currentInstruction, this, instructionCount);
                }
                else if (currentInstruction.StartsWith("-"))
                {
                    currentExpressionType = 1;
                    HandleMinusInstruction(currentInstruction, this, instructionCount);
                }
                else if (currentInstruction.StartsWith("["))
                {
                    currentExpressionType = 2;
                    HandleAttributeInstruction(currentInstruction, this, instructionCount);
                }
                else if (currentInstruction.StartsWith("_"))
                {
                    currentExpressionType = 3;
                    HandleVariableInstruction(currentInstruction, this, instructionCount);
                }
                else if (currentInstruction.StartsWith(">"))
                {
                    currentExpressionType = 4;
                    HandleVoidInstruction(currentInstruction, this, instructionCount);
                }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error Interpreting: " + ex.Message + " Source: "+ex.Source+" StackTrace: "+ex.StackTrace);
                    running = false;
                }
        }

        public void Interpreter()
        {
            int currentLine = 0;
            instructions = script.Split(new string[] { ";" }, StringSplitOptions.None);
            PrintInstructions(instructions);

            while (currentLine < instructions.Length)
            {
                if (currentLine >= instructions.Length || currentLine < 0) 
                {
                    running = false;
                    continue;
                }

                string currentInstruction = GetInstruction(currentLine);
                bool skipElseInterpret = false;
                if (attributes.executeNextOnStart)
                {
                    Console.WriteLine("Force Executing Instruction "+currentLine);
                    InterpretInstruction(currentLine);
                    currentLine++; // DO NOT DELETE
                    attributes.executeNextOnStart = false;
                    skipElseInterpret = true;
                }
                if (isInstructionFunction(GetInstructionName(currentInstruction)))
                {
                    if (currentInstruction.StartsWith(".set"))
                    {InterpretInstruction(currentLine);}
                    Console.WriteLine("Is a function! Handing over to RunInstructionList.");
                    int blockEndIndex;
                    blockEndIndex = GetNextEndInstruction(currentLine+1);
                    currentLine = blockEndIndex-1;
                    Console.WriteLine("Next Instruction: " + currentLine);
                }
                else if (!skipElseInterpret)
                {
                    Console.WriteLine("Interpreting Instruction from Interpreter: "+currentLine);
                    InterpretInstruction(currentLine);
                }
                currentLine++;
            }
        }
    }
    

    public class CSCMSVoid
    {
        public string name;
        public Action<object, CMS.FireEventHandler> func;
        public string Namespace;
        public CSCMSScript Owner;
        public string Params;

        public CSCMSVoid Clone()
        {
            return new CSCMSVoid(name,func,Namespace,Params, Owner);
        }
        
        public CSCMSVoid(string name, Action<object, CMS.FireEventHandler> func, string Namespace, string Params, CSCMSScript Owner)
        {
            this.name = name;
            this.func = func;
            this.Namespace = Namespace;
            this.Owner = Owner;
            this.Params = Params;
        }
    }

    public class CSCMSInt
    {
        public string name;
        public int value;
        public string Namespace;
        public CSCMSScript Owner;

        /*public CMSInt Clone()
        {
            return new CMSInt(name,value,Namespace,Owner);
        }*/
        
        public CSCMSInt(string name, int value, string Namespace, CSCMSScript Owner)
        {
            this.name = name;
            this.value = value;
            this.Namespace = Namespace;
            this.Owner = Owner;
        }
    }

    public class CSCMSClass
    {
        public string name;
        public string varName;
        public string Namespace;
        public CSCMSScript Owner;
        public List<CSCMSInt> intVars;
        public List<CSCMSVoid> voidVars;
        
        public CSCMSClass(string name, string Namespace, CSCMSScript Owner, List<CSCMSInt> intVars, List<CSCMSVoid> voidVars)
        {
            this.name = name;
            this.Namespace = Namespace;
            this.Owner = Owner;
            this.intVars = intVars;
            this.voidVars = voidVars;
        }

        public void AddInt(string name, int value)
        {
            intVars.Add(new CSCMSInt(name,value,Namespace,Owner));
        }

        public void AddFunction(string name, string vars, Action<object, CMS.FireEventHandler> handler)
        {
            voidVars.Add(new CSCMSVoid(name, handler, Namespace, vars, Owner));
        }
    }

    public class CSCMSScript
    {
        public string Namespace;
        public List<CSCMSVoid> Functions;
        public List<CSCMSClass> Classes;
        public List<CSCMSInt> Ints;

        public void AddFunction(string name, string vars, Action<object, CMS.FireEventHandler> handler)
        {
            Functions.Add(new CSCMSVoid(name, handler, Namespace, vars, this));
        }

        public CSCMSClass AddClass(string name)
        {
            CSCMSClass m = new CSCMSClass(name, Namespace, this, new List<CSCMSInt>(), new List<CSCMSVoid>());
            Classes.Add(m);
            return m;
        }

        public void AddInt(string name, int value)
        {
            Ints.Add(new CSCMSInt(name,value,Namespace,this));
        }

        public CSCMSScript(string Namespace)
        {
            this.Namespace = Namespace;

            Functions = new List<CSCMSVoid>();
            Classes = new List<CSCMSClass>();
            Ints = new List<CSCMSInt>();
        }
    }

    public static class Interpreter
    {
        public static List<CMSScript> RunningCMSScripts = new List<CMSScript>();
        public static List<CSCMSScript> RunningCSCMSScripts = new List<CSCMSScript>();
        public static List<InternalFunctionHandler> InternalFunctions = new List<InternalFunctionHandler>();
        public static List<InternalFunctionHandler> MinusFunctions = new List<InternalFunctionHandler>();
        public static List<InternalFunctionHandler> AttributeHandlers = new List<InternalFunctionHandler>();
        public static List<InternalFunctionHandler> VarHandlers = new List<InternalFunctionHandler>();
        public static List<InternalFunctionHandler> VoidHandlers = new List<InternalFunctionHandler>();
        public static List<string> FunctionInstructions = new List<string>() {"setvoid", "setint", "if"};
        public static void RegisterCommand(ref InternalFunctionHandler IFH)
        {
            if (IFH.prefix == ".")
            {
                InternalFunctions.Add(IFH);
            }
            else if (IFH.prefix == "-")
            {
                MinusFunctions.Add(IFH);
            }
            else if (IFH.prefix == "[")
            {
                AttributeHandlers.Add(IFH);
            }
            else if (IFH.prefix == "_")
            {
                VarHandlers.Add(IFH);
            }
            else if (IFH.prefix == ">")
            {
                VoidHandlers.Add(IFH);
            }
        }
        public static CMSScript InterpretCMS(string CMS)
        {
            CMSScript cms = new CMSScript(CMS);
            RunningCMSScripts.Add(cms);
            return (cms);
        }
        public static string GetTypeFromString(string value)
        {
            if (value.EndsWith("f", StringComparison.OrdinalIgnoreCase) &&
                float.TryParse(value.TrimEnd('f', 'F'), NumberStyles.Float, CultureInfo.InvariantCulture, out _))
            {
                return "float";
            }
            
            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
            {
                return "double";
            }
            
            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
            {
                return "int";
            }
            
            return "string";
        }
    }
}