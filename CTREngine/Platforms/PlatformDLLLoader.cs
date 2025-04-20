using System;
using System.IO;
using System.Reflection;

public static class PlatformDLLLoader
{
    public static bool GetBuildSupported(string dllPath)
    {
        if (!File.Exists(dllPath))
            throw new FileNotFoundException($"DLL not found at: {dllPath}");

        try
        {
            // Load the DLL
            Assembly assembly = Assembly.LoadFrom(dllPath);

            // Get the type of the static class
            Type type = assembly.GetType("PlatformFunctions");

            // Try to find a static property or field named 'buildSupported'
            PropertyInfo property = type.GetProperty("buildSupported", BindingFlags.Public | BindingFlags.Static);
            FieldInfo field = type.GetField("buildSupported", BindingFlags.Public | BindingFlags.Static);

            if (property != null && property.PropertyType == typeof(bool))
            {
                return (bool)property.GetValue(null);
            }
            else if (field != null && field.FieldType == typeof(bool))
            {
                return (bool)field.GetValue(null);
            }
            else
            {
                throw new Exception("'buildSupported' not found or is not of type bool.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return false;
        }
    }

    public static bool GetBuildROMSupported(string dllPath)
    {
        if (!File.Exists(dllPath))
            throw new FileNotFoundException($"DLL not found at: {dllPath}");

        try
        {
            // Load the DLL
            Assembly assembly = Assembly.LoadFrom(dllPath);

            // Get the type of the static class
            Type type = assembly.GetType("PlatformFunctions");

            // Try to find a static property or field named 'buildSupported'
            PropertyInfo property = type.GetProperty("buildRomSupported", BindingFlags.Public | BindingFlags.Static);
            FieldInfo field = type.GetField("buildRomSupported", BindingFlags.Public | BindingFlags.Static);

            if (property != null && property.PropertyType == typeof(bool))
            {
                return (bool)property.GetValue(null);
            }
            else if (field != null && field.FieldType == typeof(bool))
            {
                return (bool)field.GetValue(null);
            }
            else
            {
                throw new Exception("'buildRomSupported' not found or is not of type bool.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return false;
        }
    }

    public static object GetValueFromDll(string dllPath, string valueName)
    {
        if (!File.Exists(dllPath))
            throw new FileNotFoundException($"DLL not found at: {dllPath}");

        try
        {
            // Load the DLL
            Assembly assembly = Assembly.LoadFrom(dllPath);

            // Get the type of the static class
            Type type = assembly.GetType("PlatformFunctions");
            if (type == null)
                throw new Exception("Type 'PlatformFunctions' not found in assembly.");

            // Try to find a static property
            PropertyInfo property = type.GetProperty(valueName, BindingFlags.Public | BindingFlags.Static);
            if (property != null)
                return property.GetValue(null);

            // Try to find a static field
            FieldInfo field = type.GetField(valueName, BindingFlags.Public | BindingFlags.Static);
            if (field != null)
                return field.GetValue(null);

            throw new Exception($"Property or field '{valueName}' not found in type 'PlatformFunctions'.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return null;
        }
    }

    public static void CallBuildPlatform(string pathToDll, string pathToCtrProj)
    {
        if (!File.Exists(pathToDll))
        {
            Console.WriteLine("DLL not found at the specified path.");
            return;
        }

        try
        {
            var assembly = Assembly.LoadFrom(pathToDll);

            var platformFunctionsType = assembly.GetType("PlatformFunctions");

            if (platformFunctionsType == null)
            {
                Console.WriteLine("PlatformFunctions type not found in the DLL.");
                return;
            }

            var buildPlatformMethod = platformFunctionsType.GetMethod("BuildPlatform");

            if (buildPlatformMethod == null)
            {
                Console.WriteLine("BuildPlatform method not found in PlatformFunctions.");
                return;
            }

            object instance = null;
            if (!buildPlatformMethod.IsStatic)
            {
                instance = Activator.CreateInstance(platformFunctionsType);
            }

            buildPlatformMethod.Invoke(instance, new object[] { pathToCtrProj });

            Console.WriteLine("BuildPlatform called successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}\n{ex.Source}\n{ex.StackTrace}");
        }
    }

    public static void CallOnBuiltROM(string pathToDll, string pathToBuiltROM)
    {
        if (!File.Exists(pathToDll))
        {
            Console.WriteLine("DLL not found at the specified path.");
            return;
        }

        try
        {
            var assembly = Assembly.LoadFrom(pathToDll);

            var platformFunctionsType = assembly.GetType("PlatformFunctions");

            if (platformFunctionsType == null)
            {
                Console.WriteLine("PlatformFunctions type not found in the DLL.");
                return;
            }

            var buildPlatformMethod = platformFunctionsType.GetMethod("OnBuiltROM");

            if (buildPlatformMethod == null)
            {
                Console.WriteLine("OnBuiltROM method not found in PlatformFunctions.");
                return;
            }

            object instance = null;
            if (!buildPlatformMethod.IsStatic)
            {
                instance = Activator.CreateInstance(platformFunctionsType);
            }

            buildPlatformMethod.Invoke(instance, new object[] { pathToBuiltROM });

            Console.WriteLine("OnBuiltROM called successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}
