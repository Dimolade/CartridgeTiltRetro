using System.Collections;
using System.Collections.Generic;
using CMS;
using org.matheval;
using System.Text.RegularExpressions;

namespace CMS.Commands
{
    public static class Internal
    {
        public static void RegisterCommands()
        {
            // . commands \\\

            AddCommandWithHandler("get", GetHandler);
            AddCommandWithHandler("int", IntHandler);
            AddCommandWithHandler("setNamespace", setNamespaceHandler);
            AddCommandWithHandler("setClass", setClassHandler, ".");
            AddCommandWithHandler("if", ifHandler, ".",true);
            AddCommandWithHandler("setVoid", setVoidHandler, ".",true);
            AddCommandWithHandler("new", newClassHandler, ".");

            // - commands \\\

            AddCommandWithHandler("finish", finishHandler, "-", false);


            // [ commands \\\

            AddCommandWithHandler("Attributes", AttributesHandler, "[", false);

            // _ commands \\\

            AddCommandWithHandler("Variables", VarChangeHandler, "_");
            AddCommandWithHandler("Functions", VoidHandler, ">");
        }

        public static void AddCommandWithHandler(string name, Action<object, CMS.FireEventHandler> handler, string prefix = ".", bool isFunction = false)
        {
            InternalFunctionHandler command = new InternalFunctionHandler(name, prefix);
            Interpreter.RegisterCommand(ref command);

            if (handler != null)
            {
                command.onFire += (sender, args) => handler(sender, args);  // Attach the handler
            }

            if (isFunction)
            {
                Interpreter.FunctionInstructions.Add(prefix+name);
                Interpreter.FunctionInstructions.Add(name);
            }
        }
  
        // . commands \\\

        static void GetHandler(object sender, CMS.FireEventHandler e)
        {
            Console.WriteLine("Internal Command get fired!");
            string NamespaceName = e.fullInstructions.Remove(0,5);
            Console.WriteLine("Namespace for Get is: "+NamespaceName);
            e.script.registeredNamespaces.Add(NamespaceName);
            foreach (CMSScript script in Interpreter.RunningCMSScripts)
            {
                if (script == e.script)
                    continue;
                Console.WriteLine("Script GetNamespace: "+script.GetNamespace());
                //e.script.intVars.AddRange();
                int i = 0;
                foreach (CMSInt Int in script.intVars)
                {
                    if (Int.Namespace.StartsWith(NamespaceName))
                    {
                        e.script.namespaceIntVars.Add(script.intVars[i]);
                    }
                    i++;
                }
                i = 0;
                foreach (CMSVoid Int in script.voidVars)
                {
                    if (Int.Namespace.StartsWith(NamespaceName))
                    {
                        e.script.namespaceVoidVars.Add(script.voidVars[i]);
                    }
                    i++;
                }
                i = 0;
                foreach (CMSClass Int in script.classVars)
                {
                    if (Int.Namespace.StartsWith(NamespaceName))
                    {
                        e.script.namespaceClassVars.Add(script.classVars[i]);
                    }
                    i++;
                }
            }
            foreach (CSCMSScript script in Interpreter.RunningCSCMSScripts)
            {
                int i = 0;
                foreach (CSCMSVoid Int in script.Functions)
                {
                    if (Int.Namespace.StartsWith(NamespaceName))
                    {
                        e.script.csVoidVars.Add(script.Functions[i]);
                    }
                    i++;
                }
                i = 0;
                foreach (CSCMSInt Int in script.Ints)
                {
                    if (Int.Namespace.StartsWith(NamespaceName))
                    {
                        e.script.csIntVars.Add(script.Ints[i]);
                    }
                    i++;
                }
                i = 0;
                foreach (CSCMSClass Int in script.Classes)
                {
                    if (Int.Namespace.StartsWith(NamespaceName))
                    {
                        e.script.csClassVars.Add(script.Classes[i]);
                    }
                    i++;
                }
            }
            Console.WriteLine("get finish.");
        }

        static void setVoidHandler(object sender, CMS.FireEventHandler e)
        {
            string instruction = e.fullInstructions;
            var pattern = @"\.setVoid\s+""(.*?)""\s*\((.*?)\)";
            var match = Regex.Match(instruction, pattern);

            if (match.Success)
            {
                string voidName = match.Groups[1].Value;
                string parameters = match.Groups[2].Value;

                Console.WriteLine($"Void Name: {voidName}");
                Console.WriteLine($"Parameters: {parameters}");
                List<object> parsedparams = new List<object>();

                e.script.AddCMSVoid(new CMSVoid(voidName, e.line+1, e.script.GetNamespace(), parsedparams, e.script));
            }
        }

        static void newClassHandler(object sender, CMS.FireEventHandler e)
        {
            string instruction = e.fullInstructions;
            Console.WriteLine("Trying to make new Class.");

            var match = Regex.Match(instruction, @"\.new\s+""([^""]*)""\s+""([^""]*)""");

            if (match.Success)
            {
                string name = match.Groups[1].Value;
                CMSClass cl = e.script.GetCMSClass(name);
                CSCMSClass cscl = null;
                if (cl == null)
                {
                    cscl = e.script.GetCSCMSClass(name);
                }
                if (cscl != null || cl != null)
                {
                    if (cl != null)
                    {
                        e.script.madeClassVars.Add(cl);
                    }
                    if (cscl != null)
                    {
                        e.script.madeCsClassVars.Add(cscl);
                    }
                }
            }
        }

        static void setNamespaceHandler(object sender, CMS.FireEventHandler e)
        {
            string instruction;
            instruction = e.fullInstructions.Remove(0,14);
            string param = instruction;
            string name = param.Remove(0,1);
            name = name.Substring(0, name.Length - 1);
            Console.WriteLine("set namespace to: "+name);
            e.script.Namespace.Add(name);
            e.script.currentFinishLine.Add(e.line);
        }

        static void setClassHandler(object sender, CMS.FireEventHandler e)
        {
            string instruction;
            instruction = e.fullInstructions.Remove(0,10);
            string param = instruction;
            string name = param.Remove(0,1);
            name = name.Substring(0, name.Length - 1);
            Console.WriteLine("set class to: "+name);
            e.script.currentFinishLine.Add(e.line);
            e.script.inClass = true;
            e.script.currentClassName = name;
        }

        static void IntHandler(object sender, CMS.FireEventHandler e)
        {
            Console.WriteLine(".int executed");
            string instruction = e.fullInstructions.Remove(0, 5).Trim();

            // Pattern to match: .int "Name" = Value
            var match = Regex.Match(instruction, "^\"([^\"]+)\"\\s*=\\s*(.+)$");

            if (match.Success)
            {
                string name = match.Groups[1].Value;
                string valueExpression = match.Groups[2].Value.Trim();

                Console.WriteLine($".int with params: {instruction} | Name: {name} | Value Expression: {valueExpression}");

                int value = Convert.ToInt32(e.script.ParseExpression(valueExpression));
                CMSInt result = new CMSInt(name, value, e.script.GetNamespace(), e.script);
                e.script.AddCMSInt(result);
            }
            else if (instruction.StartsWith("\"") && instruction.EndsWith("\""))
            {
                // Handle case where there's no assignment (e.g., .int "Hello")
                string name = instruction.Substring(1, instruction.Length - 2);

                Console.WriteLine($".int with params: {instruction} | Name: {name} | Value: 0 (Default)");

                e.script.AddCMSInt(new CMSInt(name, 0, e.script.GetNamespace(), e.script));
            }
            else
            {
                Console.WriteLine("Invalid .int syntax: " + instruction);
            }
        }

        static void ifHandler(object sender, CMS.FireEventHandler e)
        {
            Console.WriteLine("If triggered.");
            string fullInstruction = e.fullInstructions;
            fullInstruction = fullInstruction.Remove(0,5);
            fullInstruction = fullInstruction.Substring(0,fullInstruction.Length - 1);
            Console.WriteLine("If check = "+fullInstruction);
            bool letThrough = CMS.Commands.If.HandleFullIf(fullInstruction, e.script);
            if (letThrough)
            {
                Console.WriteLine("If successful! Running Instruction List from If");
                e.script.RunInstructionList(e.line+1);
            }
        }

        // - commands \\\

        static void finishHandler(object sender, CMS.FireEventHandler e)
        {
            string instruction = e.fullInstructions;
            if (!instruction.StartsWith("-"))
                return;
            
            string instructionName = e.script.GetInstructionName(e.script.GetInstruction(e.script.currentFinishLine[e.script.currentFinishLine.Count-1]));
            Console.WriteLine("Finish instruct name: "+instructionName);
            if (instructionName == "setNamespace")
            {
                e.script.Namespace.RemoveAt(e.script.Namespace.Count-1);
                e.script.currentFinishLine.RemoveAt(e.script.currentFinishLine.Count-1);
            }
            else if (instructionName == "setClass")
            {
                Console.WriteLine("Adding Class, ClassInt length: "+e.script.classIntVars.Count);
                List<CMSInt> ints = new List<CMSInt>();
                List<CMSVoid> voids = new List<CMSVoid>();
                foreach (CMSInt Int in e.script.classIntVars)
                {
                    ints.Add(Int.Clone());
                }
                foreach (CMSVoid Int in e.script.classVoidVars)
                {
                    voids.Add(Int.Clone());
                }
                e.script.classVars.Add(new CMSClass(e.script.currentClassName, e.script.GetNamespace(), e.script, ints, voids));
                e.script.inClass = false;
                e.script.currentFinishLine.RemoveAt(e.script.currentFinishLine.Count-1);
                e.script.classIntVars.Clear();
                e.script.classVoidVars.Clear();
            }
        }

        // [ commands \\\

        static void AttributesHandler(object sender, CMS.FireEventHandler e)
        {
            string instruction = e.fullInstructions;
            if (!instruction.StartsWith("["))
                return;

            instruction = instruction.Remove(0,1);
            instruction = instruction.Substring(0, instruction.Length - 1);
            Console.WriteLine("Handling FuncVar "+instruction);
            if (instruction == "OnStart")
            {
                e.script.attributes.executeNextOnStart = true;
            }
        }

        // _ commands \\\

        static void VarChangeHandler(object sender, CMS.FireEventHandler e)
        {
            Console.WriteLine("Trying to change var with _ command");
            var pattern = @"_""(.*?)""\s*=\s*(.*)";

            Match match = Regex.Match(e.fullInstructions, pattern);

            if (match.Success)
            {
                string variableName = match.Groups[1].Value;
                string value = match.Groups[2].Value;

                e.script.SetVariableValue(variableName, value);
            }
        }

        // > commands \\\

        static void VoidHandler(object sender, CMS.FireEventHandler e)
        {
            var pattern = @">""([^""]+)""\s*\((.*?)\)";

            Match match = Regex.Match(e.fullInstructions, pattern);

            if (match.Success)
            {
                string variableName = match.Groups[1].Value;
                string value = match.Groups[2].Value;

                if (e.script.GetCMSVoid(variableName) != null)
                {
                    e.script.RunInstructionList(e.script.GetCMSVoid(variableName).instructionStart);
                }
            }
            else
            {
                Console.WriteLine("Void REGEX failed!");
            }
        }
    }
}