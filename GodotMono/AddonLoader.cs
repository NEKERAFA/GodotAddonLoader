using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;

/// <summary>
/// Class that loads addons written in GDScript and C# (Mono) for Godot
/// 
/// <para>
/// Under MIT License
/// Copyright (c) 2023 Rafael Alcalde Azpiazu
/// </para>
/// </summary>
public class AddonLoader : Node
{
    [Signal]
    delegate void AddonLoaded(string addonName);

    [Export]
    private string _addonsPath = "user://addons";

    [Export]
    private string _addonMainClass = "AddonClass";

    static readonly IReadOnlyList<string> ResourcePackExtensions = new List<string> { "pck", "zip" };
    static readonly string MonoAssemblyExtension = "dll";

    /// <summary>
    /// Loads the addons contained in "user://addons" folder
    /// </summary>
    public override void _Ready()
    {
        var dir = new Directory();

        // Opens addons folder
        if (dir.Open(_addonsPath) == Error.Ok)
        {
            // Start to iterate through addons folder
            if (dir.ListDirBegin(true, true) == Error.Ok)
            {
                var fileName = dir.GetNext();
                while (!fileName.Empty())
                {
                    LoadAddon(fileName);
                    fileName = dir.GetNext();
                }

                // Ends addons folder iteration
                dir.ListDirEnd();
            }
        }
    }

    /// <summary>
    /// Loads the addon.
    /// </summary>
    /// <param name="fileName">File name.</param>
    private void LoadAddon(string fileName)
    {
        var addonPath = _addonsPath.PlusFile(fileName);

        if (ResourcePackExtensions.Contains(fileName.Extension()))
        {
            // Loads the resource pack and instantiate the addon class as GDScript
            if (LoadAddonResourcePack(addonPath))
            {
                var addonScript = GD.Load<GDScript>("res://".PlusFile(fileName.BaseName()).PlusFile($"{_addonMainClass}.gd"));
                if (addonScript != null)
                {
                    var addonNode = (Node)addonScript.New();
                    if (addonNode != null)
                    {
                        AddAddonNode(addonNode, fileName);
                        EmitSignal(nameof(AddonLoaded), fileName.BaseName());
                    }
                }
            }
        }
        else if (fileName.Extension().Equals(MonoAssemblyExtension))
        {
            // Loads the assembly and instantiate the addon class as C# object
            var addonAssembly = LoadAddonAssembly(addonPath);
            if (addonAssembly != null)
            {
                var addonNode = (Node)addonAssembly.CreateInstance(_addonMainClass);
                if (addonNode != null)
                {
                    AddAddonNode(addonNode, fileName);
                    EmitSignal(nameof(AddonLoaded), fileName.BaseName());
                }
            }
        }
    }

    /// <summary>
    /// Loads the addon assembly and returns it.
    /// </summary>
    /// <returns>The addon assembly.</returns>
    /// <param name="assemblyFile">Assembly file.</param>
    private Assembly LoadAddonAssembly(string assemblyFile)
    {
        var assembly = Assembly.LoadFile(ProjectSettings.GlobalizePath(assemblyFile));

        if (assembly != null)
        {
            GD.Print($"Assembly {assemblyFile} loaded.");
        }
        else
        {
            GD.Print($"Error loading assembly {assemblyFile}.");
        }

        return assembly;
    }

    /// <summary>
    /// Loads the addon resource pack and prints confirmation/error messages.
    /// </summary>
    /// <returns><c>true</c>, if addon resource pack was loaded, <c>false</c> otherwise.</returns>
    /// <param name="resourcePackFile">Resource pack file.</param>
    private bool LoadAddonResourcePack(string resourcePackFile)
    {
        var resourcePackLoaded = ProjectSettings.LoadResourcePack(resourcePackFile);

        if (resourcePackLoaded)
        {
            GD.Print($"Resource pack {resourcePackFile} loaded.");
        }
        else
        {
            GD.Print($"Error loading resource pack {resourcePackFile}.");
        }

        return resourcePackLoaded;
    }

    /// <summary>
    /// Adds the addon node as child node using as name the addon name.
    /// </summary>
    /// <param name="addonNode">Addon node.</param>
    /// <param name="addonName">Addon name.</param>
    private void AddAddonNode(Node addonNode, string addonName)
    {
        addonNode.Name = addonName.BaseName();
        AddChild(addonNode, true);
        var nodeType = addonName.Extension().Equals(MonoAssemblyExtension) ? "Mono" : "GDScript";
        GD.Print($"{nodeType} add-on {addonName.BaseName()} loaded.");
    }
}
