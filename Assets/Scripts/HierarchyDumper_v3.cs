using UnityEngine;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.IO;

[System.Serializable]
public enum DetailLevel
{
    Basic = 0,           // Just component names
    Standard = 1,        // Component names + custom script fields only
    Detailed = 2,        // All public fields/properties (current v2 behavior)
    BuiltInBasic = 3,    // Built-in components with basic info
    BuiltInDetailed = 4, // Built-in components with all details
    Everything = 5       // Everything including private fields
}

[System.Serializable]
public class GameObjectAnalysisTarget
{
    [Header("Target Settings")]
    public GameObject gameObject;
    public string gameObjectName = "";
    public DetailLevel detailLevel = DetailLevel.Standard;
    
    [Header("Options")]
    public bool includeChildren = true;
    public bool includeInactive = true;
    public int maxDepth = 3;
    
    [Header("Component Filtering")]
    public bool includeBuiltInComponents = false;
    public bool includeTransform = false;
    public List<string> specificComponentTypes = new List<string>();
    
    // Auto-populate name when GameObject is assigned
    public void OnValidate()
    {
        if (gameObject != null && string.IsNullOrEmpty(gameObjectName))
        {
            gameObjectName = gameObject.name;
        }
    }
}

public class HierarchyDumper_v3 : MonoBehaviour
{
    [Header("V3 - Advanced Analysis Targets")]
    [SerializeField] private List<GameObjectAnalysisTarget> analysisTargets = new List<GameObjectAnalysisTarget>();
    
    [Header("Legacy String-Based Targets (v2 compatibility)")]
    [SerializeField] private List<string> legacyGameObjectNames = new List<string>();
    [SerializeField] private DetailLevel defaultDetailLevel = DetailLevel.Standard;
    
    [Header("Global Settings")]
    [SerializeField] private bool dumpOnStart = false;
    [SerializeField] private bool showFullPaths = true;
    [SerializeField] private bool includeOverviewHierarchy = true;
    [SerializeField] private int overviewMaxDepth = 3;
    [SerializeField] private string overviewRootObject = "";
    
    [Header("Output")]
    [SerializeField] [TextArea(15, 40)] private string output = "";

    void Start()
    {
        if (dumpOnStart)
        {
            DumpSelectedHierarchy();
        }
    }

    [ContextMenu("Dump Selected GameObjects")]
    public void DumpSelectedHierarchy()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== UNITY HIERARCHY DUMP v3 (Advanced) ===");
        sb.AppendLine($"Scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        sb.AppendLine($"Timestamp: {System.DateTime.Now}");
        sb.AppendLine();

        // Step 1: Generate overview hierarchy for context
        if (includeOverviewHierarchy)
        {
            GenerateOverviewHierarchy(sb);
        }

        // Step 2: Process advanced analysis targets
        if (analysisTargets.Count > 0)
        {
            sb.AppendLine("=== ADVANCED ANALYSIS TARGETS ===");
            sb.AppendLine();

            foreach (var target in analysisTargets)
            {
                if (target.gameObject == null && string.IsNullOrEmpty(target.gameObjectName)) continue;
                
                GameObject targetObj = target.gameObject;
                if (targetObj == null && !string.IsNullOrEmpty(target.gameObjectName))
                {
                    targetObj = FindGameObjectByName(target.gameObjectName);
                }
                
                if (targetObj != null)
                {
                    string fullPath = showFullPaths ? GetFullPath(targetObj) : targetObj.name;
                    sb.AppendLine($"🎯 ANALYZING: {fullPath}");
                    sb.AppendLine($"📊 Detail Level: {target.detailLevel}");
                    sb.AppendLine($"👶 Include Children: {target.includeChildren} (Max Depth: {target.maxDepth})");
                    sb.AppendLine("".PadRight(70, '='));
                    
                    DumpGameObjectAdvanced(targetObj, target, sb, 0);
                    sb.AppendLine("".PadRight(70, '='));
                    sb.AppendLine();
                }
                else
                {
                    sb.AppendLine($"❌ GameObject '{target.gameObjectName}' not found!");
                    sb.AppendLine();
                }
            }
        }

        // Step 3: Process legacy string-based targets for backward compatibility
        if (legacyGameObjectNames.Count > 0)
        {
            sb.AppendLine("=== LEGACY STRING-BASED TARGETS ===");
            sb.AppendLine();

            foreach (string objName in legacyGameObjectNames)
            {
                if (string.IsNullOrEmpty(objName)) continue;
                
                GameObject targetObj = FindGameObjectByName(objName);
                if (targetObj != null)
                {
                    string fullPath = showFullPaths ? GetFullPath(targetObj) : objName;
                    sb.AppendLine($"🎯 ANALYZING: {fullPath}");
                    sb.AppendLine($"📊 Detail Level: {defaultDetailLevel} (Legacy Mode)");
                    sb.AppendLine("".PadRight(50, '='));
                    
                    // Create temporary target with default settings
                    var tempTarget = new GameObjectAnalysisTarget
                    {
                        gameObject = targetObj,
                        detailLevel = defaultDetailLevel,
                        includeChildren = true,
                        maxDepth = 10,
                        includeBuiltInComponents = (defaultDetailLevel >= DetailLevel.BuiltInBasic)
                    };
                    
                    DumpGameObjectAdvanced(targetObj, tempTarget, sb, 0);
                    sb.AppendLine("".PadRight(50, '='));
                    sb.AppendLine();
                }
                else
                {
                    sb.AppendLine($"❌ GameObject '{objName}' not found!");
                    sb.AppendLine();
                }
            }
        }

        // Step 4: Show instructions if no targets
        if (analysisTargets.Count == 0 && legacyGameObjectNames.Count == 0)
        {
            sb.AppendLine("❌ ERROR: No analysis targets specified!");
            sb.AppendLine("Please add GameObjects to the 'Analysis Targets' list or use legacy 'Legacy Game Object Names'.");
            sb.AppendLine();
            sb.AppendLine("📚 DETAIL LEVELS AVAILABLE:");
            sb.AppendLine("  • Basic: Just component names");
            sb.AppendLine("  • Standard: Component names + custom script fields");
            sb.AppendLine("  • Detailed: All public fields/properties");
            sb.AppendLine("  • BuiltInBasic: Built-in components with basic info");
            sb.AppendLine("  • BuiltInDetailed: Built-in components with all details");
            sb.AppendLine("  • Everything: Everything including private fields");
        }

        // Calculate statistics before finalizing output
        string finalOutput = sb.ToString();
        string[] lines = finalOutput.Split('\n');
        int totalLines = lines.Length;
        int totalChars = finalOutput.Length;
        
        // Add statistics header at the beginning
        StringBuilder finalSb = new StringBuilder();
        finalSb.AppendLine("=== DUMP STATISTICS ===");
        finalSb.AppendLine($"📊 Total Lines: {totalLines}");
        finalSb.AppendLine($"📏 Total Characters: {totalChars:N0}");
        finalSb.AppendLine($"📈 Context Window Status: {GetContextWindowStatus(totalLines)}");
        finalSb.AppendLine($"⏰ Generated: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        finalSb.AppendLine("".PadRight(50, '='));
        finalSb.AppendLine();
        finalSb.Append(finalOutput);
        
        output = finalSb.ToString();
        Debug.Log("Hierarchy dump v3 complete! Check the 'output' field in inspector.");
        Debug.Log($"📊 Dump Statistics: {totalLines} lines, {totalChars:N0} characters - {GetContextWindowStatus(totalLines)}");
    }

    private string GetFullPath(GameObject obj)
    {
        List<string> path = new List<string>();
        Transform current = obj.transform;
        
        while (current != null)
        {
            path.Insert(0, current.name);
            current = current.parent;
        }
        
        return string.Join("/", path.ToArray());
    }

    private void GenerateOverviewHierarchy(StringBuilder sb)
    {
        sb.AppendLine("=== CONTEXT OVERVIEW (Structure Only) ===");
        sb.AppendLine("This shows the overall hierarchy structure without detailed attributes.");
        sb.AppendLine();

        GameObject[] rootObjects;
        if (!string.IsNullOrEmpty(overviewRootObject))
        {
            GameObject specificRoot = FindGameObjectByName(overviewRootObject);
            rootObjects = specificRoot != null ? new GameObject[] { specificRoot } : new GameObject[0];
        }
        else
        {
            rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        }

        foreach (GameObject rootObj in rootObjects)
        {
            DumpGameObjectOverview(rootObj, sb, 0);
        }
        
        sb.AppendLine();
        sb.AppendLine();
    }

    private void DumpGameObjectOverview(GameObject obj, StringBuilder sb, int depth)
    {
        if (depth > overviewMaxDepth) return;

        string indent = new string(' ', depth * 2);
        string activeStatus = obj.activeInHierarchy ? "✓" : "✗";
        
        sb.AppendLine($"{indent}📦 [{activeStatus}] {obj.name}");
        
        // List components/scripts without details
        Component[] components = obj.GetComponents<Component>();
        foreach (Component comp in components)
        {
            if (comp == null) continue;
            if (comp is Transform) continue;
            
            System.Type compType = comp.GetType();
            string compName = compType.Name;
            string componentType = GetComponentTypeDescription(compType);
            
            sb.AppendLine($"{indent}  {componentType} {compName}");
        }

        // Recursively show children
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            GameObject child = obj.transform.GetChild(i).gameObject;
            DumpGameObjectOverview(child, sb, depth + 1);
        }
    }

    private void DumpGameObjectAdvanced(GameObject obj, GameObjectAnalysisTarget target, StringBuilder sb, int depth)
    {
        if (depth > target.maxDepth) return;

        string indent = new string(' ', depth * 2);
        string activeStatus = obj.activeInHierarchy ? "✓" : "✗";
        
        string fullPath = showFullPaths ? GetFullPath(obj) : obj.name;
        sb.AppendLine($"{indent}📦 [{activeStatus}] {obj.name} (GameObject)");
        if (showFullPaths && depth == 0)
        {
            sb.AppendLine($"{indent}  📍 Full Path: {fullPath}");
        }
        
        // Transform info
        if (target.includeTransform)
        {
            Transform t = obj.transform;
            sb.AppendLine($"{indent}  🎯 Transform Component:");
            sb.AppendLine($"{indent}     Position: {t.position}");
            sb.AppendLine($"{indent}     Rotation: {t.rotation.eulerAngles}");
            sb.AppendLine($"{indent}     Scale: {t.localScale}");
            sb.AppendLine($"{indent}     Child Count: {t.childCount}");
        }

        // Get all components
        Component[] components = obj.GetComponents<Component>();
        
        foreach (Component comp in components)
        {
            if (comp == null) continue;
            if (comp is Transform && !target.includeTransform) continue;
            
            System.Type compType = comp.GetType();
            string compName = compType.Name;
            
            // Apply component filtering
            if (!ShouldIncludeComponent(compType, target))
                continue;

            // Filter by specific component types if specified
            if (target.specificComponentTypes.Count > 0 && 
                !target.specificComponentTypes.Contains(compName))
                continue;

            // Enhanced component header with type and file path
            string componentType = GetComponentTypeDescription(compType);
            string filePath = GetScriptFilePath(compType);
            
            sb.AppendLine($"{indent}  {componentType} {compName}:");
            if (!string.IsNullOrEmpty(filePath))
            {
                sb.AppendLine($"{indent}     📁 File: {filePath}");
            }
            if (!string.IsNullOrEmpty(compType.Namespace))
            {
                sb.AppendLine($"{indent}     📦 Namespace: {compType.Namespace}");
            }
            
            // Dump component properties based on detail level
            DumpComponentPropertiesAdvanced(comp, target.detailLevel, sb, indent + "     ");
        }

        // Recursively dump children
        if (target.includeChildren)
        {
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                GameObject child = obj.transform.GetChild(i).gameObject;
                if (!target.includeInactive && !child.activeInHierarchy)
                    continue;
                    
                DumpGameObjectAdvanced(child, target, sb, depth + 1);
            }
        }
        
        sb.AppendLine(); // Empty line between GameObjects
    }

    private bool ShouldIncludeComponent(System.Type compType, GameObjectAnalysisTarget target)
    {
        bool isBuiltIn = IsBuiltInComponent(compType);
        
        switch (target.detailLevel)
        {
            case DetailLevel.Basic:
            case DetailLevel.Standard:
            case DetailLevel.Detailed:
            case DetailLevel.Everything:
                return target.includeBuiltInComponents || !isBuiltIn;
                
            case DetailLevel.BuiltInBasic:
            case DetailLevel.BuiltInDetailed:
                return true; // Include everything
                
            default:
                return !isBuiltIn;
        }
    }

    private void DumpComponentPropertiesAdvanced(Component comp, DetailLevel detailLevel, StringBuilder sb, string indent)
    {
        if (detailLevel == DetailLevel.Basic)
        {
            sb.AppendLine($"{indent}💡 Component loaded (Basic level - no details shown)");
            return;
        }

        System.Type type = comp.GetType();
        bool isBuiltIn = IsBuiltInComponent(type);
        
        // Handle built-in components specially
        if (isBuiltIn && detailLevel < DetailLevel.BuiltInBasic)
        {
            sb.AppendLine($"{indent}💡 Built-in component (increase detail level to see properties)");
            return;
        }

        // Get serialized fields
        FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        
        List<string> fieldOutputs = new List<string>();
        foreach (FieldInfo field in fields)
        {
            bool shouldInclude = false;
            
            switch (detailLevel)
            {
                case DetailLevel.Standard:
                    // Only custom script fields (public or [SerializeField])
                    shouldInclude = !isBuiltIn && (field.IsPublic || field.GetCustomAttribute<SerializeField>() != null);
                    break;
                    
                case DetailLevel.Detailed:
                case DetailLevel.BuiltInBasic:
                    // All public fields and serialized private fields
                    shouldInclude = field.IsPublic || field.GetCustomAttribute<SerializeField>() != null;
                    break;
                    
                case DetailLevel.BuiltInDetailed:
                case DetailLevel.Everything:
                    // Everything including private fields
                    shouldInclude = true;
                    break;
            }
            
            if (!shouldInclude) continue;
                
            // Skip compiler-generated fields
            if (field.Name.Contains("<") || field.Name.Contains("k__BackingField"))
                continue;

            try
            {
                object value = field.GetValue(comp);
                string fieldName = field.Name;
                string fieldType = field.FieldType.Name;
                
                // Handle different value types
                string valueStr = FormatValue(value, field.FieldType);
                
                fieldOutputs.Add($"{indent}🔹 {fieldName} ({fieldType}): {valueStr}");
            }
            catch (System.Exception e)
            {
                fieldOutputs.Add($"{indent}🔹 {field.Name}: [Error reading value: {e.Message}]");
            }
        }

        // Get public properties
        PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        foreach (PropertyInfo prop in properties)
        {
            // Skip indexers and properties that can't be read
            if (prop.GetIndexParameters().Length > 0 || !prop.CanRead)
                continue;
                
            // Filter based on detail level
            bool shouldInclude = false;
            
            switch (detailLevel)
            {
                case DetailLevel.Standard:
                    shouldInclude = !isBuiltIn && !IsUninterestingProperty(prop.Name);
                    break;
                    
                case DetailLevel.Detailed:
                case DetailLevel.BuiltInBasic:
                    shouldInclude = !IsUninterestingProperty(prop.Name);
                    break;
                    
                case DetailLevel.BuiltInDetailed:
                case DetailLevel.Everything:
                    shouldInclude = true;
                    break;
            }
            
            if (!shouldInclude) continue;

            try
            {
                object value = prop.GetValue(comp);
                string valueStr = FormatValue(value, prop.PropertyType);
                fieldOutputs.Add($"{indent}🔸 {prop.Name} (Property): {valueStr}");
            }
            catch (System.Exception)
            {
                // Skip properties that throw exceptions when accessed
            }
        }

        // Output all fields and properties
        foreach (string output in fieldOutputs)
        {
            sb.AppendLine(output);
        }
        
        if (fieldOutputs.Count == 0)
        {
            sb.AppendLine($"{indent}💡 No properties available at this detail level");
        }
    }

    private string GetComponentTypeDescription(System.Type type)
    {
        if (IsCustomScript(type))
        {
            return "📜 SCRIPT";
        }
        else if (IsBuiltInComponent(type))
        {
            return "🔧 BUILT-IN";
        }
        else
        {
            return "🔧 COMPONENT";
        }
    }

    private bool IsCustomScript(System.Type type)
    {
        return typeof(MonoBehaviour).IsAssignableFrom(type) && 
               (type.Namespace == null || 
                (!type.Namespace.StartsWith("UnityEngine") && 
                 !type.Namespace.StartsWith("UnityEditor")));
    }

    private string GetScriptFilePath(System.Type type)
    {
        try
        {
            if (typeof(MonoBehaviour).IsAssignableFrom(type))
            {
                string scriptName = type.Name + ".cs";
                
                // Search in Assets folder
                string[] foundFiles = Directory.GetFiles(Application.dataPath, scriptName, SearchOption.AllDirectories);
                
                if (foundFiles.Length > 0)
                {
                    string relativePath = foundFiles[0].Replace(Application.dataPath, "Assets");
                    return relativePath.Replace('\\', '/');
                }
                
                // If not found in Assets, check if it's in Packages
                string packagePath = FindInPackages(scriptName);
                if (!string.IsNullOrEmpty(packagePath))
                {
                    return packagePath;
                }
            }
        }
        catch (System.Exception)
        {
            // Ignore errors in file searching
        }
        
        return "";
    }

    private string FindInPackages(string scriptName)
    {
        try
        {
            string packagesPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Library", "PackageCache");
            if (Directory.Exists(packagesPath))
            {
                string[] foundFiles = Directory.GetFiles(packagesPath, scriptName, SearchOption.AllDirectories);
                if (foundFiles.Length > 0)
                {
                    string packageFile = foundFiles[0];
                    int packageIndex = packageFile.IndexOf("PackageCache");
                    if (packageIndex >= 0)
                    {
                        return "Packages/" + packageFile.Substring(packageIndex + "PackageCache/".Length).Replace('\\', '/');
                    }
                }
            }
        }
        catch (System.Exception)
        {
            // Ignore errors
        }
        
        return "";
    }

    private GameObject FindGameObjectByName(string name)
    {
        // First try direct find
        GameObject found = GameObject.Find(name);
        if (found != null) return found;
        
        // If not found, search through all objects (including inactive)
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == name && obj.scene.IsValid())
            {
                return obj;
            }
        }
        
        return null;
    }

    private string FormatValue(object value, System.Type type)
    {
        if (value == null)
            return "null";
            
        if (type == typeof(Vector3))
            return ((Vector3)value).ToString("F3");
        else if (type == typeof(Vector2))
            return ((Vector2)value).ToString("F3");
        else if (type == typeof(Quaternion))
            return $"({((Quaternion)value).eulerAngles.ToString("F1")})";
        else if (type == typeof(Color))
            return ((Color)value).ToString("F2");
        else if (typeof(UnityEngine.Object).IsAssignableFrom(type))
        {
            UnityEngine.Object unityObj = (UnityEngine.Object)value;
            return unityObj != null ? $"→ {unityObj.name} ({type.Name})" : "null";
        }
        else if (type.IsArray)
        {
            System.Array array = (System.Array)value;
            return $"Array[{array.Length}]";
        }
        else if (typeof(System.Collections.IList).IsAssignableFrom(type))
        {
            System.Collections.IList list = (System.Collections.IList)value;
            return $"List[{list.Count}]";
        }
        else if (type.IsEnum)
            return value.ToString();
        else if (type.IsPrimitive || type == typeof(string))
            return value.ToString();
        else
            return $"({type.Name})";
    }

    private string GetContextWindowStatus(int totalLines)
    {
        if (totalLines <= 1000)
            return "🟢 OPTIMAL (≤1000 lines) - Perfect for AI analysis";
        else if (totalLines <= 2500)
            return "🟡 GOOD (1001-2500 lines) - Still very manageable";
        else if (totalLines <= 4000)
            return "🟠 LARGE (2501-4000 lines) - Manageable but may need filtering for complex tasks";
        else if (totalLines <= 6000)
            return "🔴 VERY LARGE (4001-6000 lines) - Will work but context compression likely";
        else
            return "❌ EXCESSIVE (>6000 lines) - Likely to exceed context window, recommend splitting";
    }

    private bool IsBuiltInComponent(System.Type type)
    {
        return type.Namespace != null && 
               (type.Namespace.StartsWith("UnityEngine") && 
                !type.Namespace.Contains("Interaction"));
    }

    private bool IsUninterestingProperty(string propName)
    {
        string[] boring = { "hideFlags", "name", "tag", "gameObject", "transform" };
        return boring.Contains(propName);
    }

    #region Context Menu Actions
    
    [ContextMenu("Add Current GameObject to Advanced Targets")]
    public void AddCurrentGameObjectToAdvancedTargets()
    {
        var newTarget = new GameObjectAnalysisTarget
        {
            gameObject = this.gameObject,
            gameObjectName = this.gameObject.name,
            detailLevel = DetailLevel.Standard,
            includeChildren = true,
            maxDepth = 3
        };
        
        analysisTargets.Add(newTarget);
        Debug.Log($"Added '{this.gameObject.name}' to advanced analysis targets. Total targets: {analysisTargets.Count}");
    }

    [ContextMenu("Clear All Targets")]
    public void ClearAllTargets()
    {
        analysisTargets.Clear();
        legacyGameObjectNames.Clear();
        Debug.Log("Cleared all analysis targets.");
    }

    [ContextMenu("Clear Output")]
    public void ClearOutput()
    {
        output = "";
    }

    [ContextMenu("Show V3 Setup Guide")]
    public void ShowV3SetupGuide()
    {
        Debug.Log(@"
=== HIERARCHY DUMPER V3 SETUP GUIDE ===

🎯 ADVANCED ANALYSIS TARGETS:
- Drag GameObjects directly into 'Analysis Targets' list
- Set individual detail levels for each target
- Control children inclusion and depth per target
- Filter specific component types

📊 DETAIL LEVELS:
• Basic: Just component names
• Standard: Component names + custom script fields  
• Detailed: All public fields/properties
• BuiltInBasic: Built-in components with basic info
• BuiltInDetailed: Built-in components with all details
• Everything: Everything including private fields

🔧 COMPONENT FILTERING:
- Include/exclude built-in components per target
- Include/exclude Transform details
- Filter by specific component type names

📍 FULL PATHS:
- Enable 'Show Full Paths' for complete hierarchy paths
- Example: 'Interactables/SimpleGrab3Torch/Visuals/Root/torchParticleFlames'

💡 BACKWARD COMPATIBILITY:
- Legacy string-based targets still work
- Set 'Default Detail Level' for legacy mode

🚀 QUICK START:
1. Drag GameObjects into 'Analysis Targets'
2. Set detail level to 'BuiltInDetailed' for ParticleSystem analysis
3. Enable 'Include Built In Components'
4. Run dump!
        ");
    }
    
    #endregion
}
