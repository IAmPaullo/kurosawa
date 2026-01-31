using System;
using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// A utility class for working with predefined assemblies.
/// </summary>
public static class PredefinedAssemblyUtil
{
    // Enum representing different types of assemblies.
    // TODO: Add custom assemblies here.
    enum AssemblyType
    {
        AssemblyCSharp,
        AssemblyCSharpEditor,
        AssemblyCSharpEditorFirstPass,
        AssemblyCSharpFirstPass,
        EventBusAssembly,
        Main
    }

    /// <summary>
    /// Gets the assembly type based on the assembly name.
    /// </summary>
    /// <param name="assemblyName">The name of the assembly.</param>
    /// <returns>The assembly type, or null if the assembly name does not ma
    static AssemblyType? GetAssemblyType(string assemblyName)
    {
        return assemblyName switch
        {
            "Assembly-CSharp" => AssemblyType.AssemblyCSharp,
            "Assembly-CSharp-Editor" => AssemblyType.AssemblyCSharpEditor,
            "Assembly-CSharp-Editor-firstpass" => AssemblyType.AssemblyCSharpEditorFirstPass,
            "Assembly-CSharp-firstpass" => AssemblyType.AssemblyCSharpFirstPass,
            "EventBusAssembly" => AssemblyType.EventBusAssembly,
            "Main" => AssemblyType.Main,
            _ => null
        };
    }

    /// <summary>
    /// Gets all types from the predefined assemblies that implement the specified interface type.
    /// </summary>
    /// <param name="interfaceType">The interface type to search for.</param>
    /// <returns>A list of types that implement the specified interface type.<
    public static List<Type> GetTypes(Type interfaceType)
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        Dictionary<AssemblyType, Type[]> assemblyTypes = new();
        List<Type> types = new();
        for (int i = 0; i < assemblies.Length; i++)
        {
            Assembly assembly = assemblies[i];
            AssemblyType? assemblyType = GetAssemblyType(assembly.GetName().Name);

            if (assemblyType != null)
            {
                assemblyTypes.Add((AssemblyType)assemblyType, assembly.GetTypes());
            }
        }

        //AddTypesFromAssembly(assemblyTypes[AssemblyType.AssemblyCSharp], types, interfaceType);
        //AddTypesFromAssembly(assemblyTypes[AssemblyType.AssemblyCSharpFirstPass], types, interfaceType);

        return types;
    }
    /// <summary>
    /// Adds types from the specified assembly that implement the specified interface type to the collection of types.
    /// </summary>
    /// <param name="assembly">The assembly to search for types.</param>
    /// <param name="types">The collection of types to add to.</param>
    /// <param name="interfaceType">The interface type to search for.</param>
    private static void AddTypesFromAssembly(Type[] assembly, ICollection<Type> types, Type interfaceType)
    {
        if (assembly == null) return;

        for (int i = 0; i < assembly.Length; i++)
        {
            Type type = assembly[i];
            if (type != interfaceType && interfaceType.IsAssignableFrom(type))
            {
                types.Add(type);
            }
        }
    }
}
