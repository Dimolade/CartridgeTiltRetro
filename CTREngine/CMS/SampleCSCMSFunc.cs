using CMS;

public static class SampleCSCMSFunc
{
    public static void Commence()
    {
        CSCMSScript main = new CSCMSScript("SampleCS");
        Interpreter.RunningCSCMSScripts.Add(main);
        
        main.AddFunction("Coolio", "", Coolio);
        main.AddInt("CoolValue", 69);
    }

    public static void Coolio(object sender, CMS.FireEventHandler e)
    {
        Console.WriteLine("Coolio! from line: "+e.line);
    }
}