namespace System.Reflection;

/// <summary>
/// Provides information about the specified assembly.
/// </summary>
public class AssemblyInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AssemblyInfo"/> class with the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly to get information from.</param>
    public AssemblyInfo(Assembly assembly)
    {
        Assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
        AssemblyName = Assembly.GetName();
        Version = AssemblyName.Version;
        Company = Assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company;
        Copyright = Assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright;
        Product = Assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product;
        Title = Assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AssemblyInfo"/> class using the entry or executing assembly.
    /// </summary>
    public AssemblyInfo(): this(Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly())
    {
    }

    /// <summary>
    /// Gets the current assembly info for the entry or executing assembly.
    /// </summary>
    public static AssemblyInfo Current { get; } = new();

    /// <summary>
    /// Gets the assembly.
    /// </summary>
    public Assembly Assembly { get; }

    /// <summary>
    /// Gets the company name from the assembly's attributes.
    /// </summary>
    public string? Company { get; }

    /// <summary>
    /// Gets the copyright information from the assembly's attributes.
    /// </summary>
    public string? Copyright { get; }

    /// <summary>
    /// Gets the product name from the assembly's attributes.
    /// </summary>
    public string? Product { get; }

    /// <summary>
    /// Gets the title of the assembly from the assembly's attributes.
    /// </summary>
    public string? Title { get; }

    /// <summary>
    /// Gets the name of the assembly.
    /// </summary>
    public AssemblyName AssemblyName { get; }

    /// <summary>
    /// Gets the version of the assembly.
    /// </summary>
    public Version? Version { get; }
}