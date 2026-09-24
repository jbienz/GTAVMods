using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;
using GTA;
using LemonUI;
using LemonUI.Menus;

public sealed class ExntrcMenu : Script
{
    private readonly ObjectPool menuPool = new ObjectPool();
    private readonly NativeMenu mainMenu;
    private readonly Keys openKey;
    private readonly GTA.Control[] controllerChord;
    private bool menusBuilt;

    public ExntrcMenu()
    {
        ScriptSettings settings = ScriptSettings.Load(@"scripts\ExntrcMenu\ExntrcMenu.ini");
        openKey = ReadKey(settings, "OpenKey", Keys.F5);
        string controllerChordError;
        controllerChord = ReadControllerChord(settings, out controllerChordError);

        mainMenu = new NativeMenu("eXntrc's Mods", "Installed Mods");
        mainMenu.KeepNameCasing = true;
        menuPool.Add(mainMenu);

        if (controllerChordError != null)
        {
            GTA.UI.Notification.PostTicker("ExntrcMenu controller chord disabled: " + controllerChordError, false, false);
        }

        KeyDown += OnKeyDown;
        Tick += OnTick;
    }

    private static Keys ReadKey(ScriptSettings settings, string settingName, Keys defaultKey)
    {
        string configuredKey = settings.GetValue("ExntrcMenu", settingName, defaultKey.ToString());
        Keys parsedKey;

        if (Enum.TryParse(configuredKey, true, out parsedKey))
        {
            return parsedKey;
        }

        return defaultKey;
    }

    private static GTA.Control[] ReadControllerChord(ScriptSettings settings, out string error)
    {
        string configuredChord = settings.GetValue("ExntrcMenu", "ControllerChord", "FrontendLb,FrontendRb,FrontendLt,FrontendRt");
        error = null;

        if (string.IsNullOrWhiteSpace(configuredChord))
        {
            return new GTA.Control[0];
        }

        string[] configuredControls = configuredChord.Split(',');
        if (configuredControls.Length > 4)
        {
            error = "specify no more than four controls.";
            return new GTA.Control[0];
        }

        List<GTA.Control> controls = new List<GTA.Control>();
        for (int index = 0; index < configuredControls.Length; index++)
        {
            string configuredControl = configuredControls[index].Trim();
            GTA.Control parsedControl;

            if (configuredControl.Length == 0 ||
                !Enum.TryParse(configuredControl, true, out parsedControl) ||
                !Enum.IsDefined(typeof(GTA.Control), parsedControl))
            {
                error = "'" + configuredControl + "' is not a valid GTA control name.";
                return new GTA.Control[0];
            }

            if (controls.Contains(parsedControl))
            {
                error = "'" + configuredControl + "' is listed more than once.";
                return new GTA.Control[0];
            }

            controls.Add(parsedControl);
        }

        return controls.ToArray();
    }

    private void BuildMenus()
    {
        // Discover once per SHVDN script domain; a script reset creates a new menu instance and cache.
        List<DiscoveredMod> mods = DiscoverMods();
        mods.Sort(delegate(DiscoveredMod left, DiscoveredMod right)
        {
            return StringComparer.OrdinalIgnoreCase.Compare(left.Name, right.Name);
        });

        for (int modIndex = 0; modIndex < mods.Count; modIndex++)
        {
            AddModMenu(mods[modIndex]);
        }

        menusBuilt = true;
    }

    private static List<DiscoveredMod> DiscoverMods()
    {
        List<DiscoveredMod> mods = new List<DiscoveredMod>();
        object[] scriptInstances = GetRunningScriptInstances();

        for (int scriptIndex = 0; scriptIndex < scriptInstances.Length; scriptIndex++)
        {
            object scriptInstance = scriptInstances[scriptIndex];
            if (scriptInstance == null || scriptInstance is ExntrcMenu)
            {
                continue;
            }

            List<MenuAction> actions = DiscoverActions(scriptInstance);
            if (actions.Count > 0)
            {
                mods.Add(new DiscoveredMod(GetModDisplayName(scriptInstance.GetType()), actions));
            }
        }

        return mods;
    }

    private static string GetModDisplayName(Type scriptType)
    {
        DescriptionAttribute description = Attribute.GetCustomAttribute(scriptType, typeof(DescriptionAttribute), false) as DescriptionAttribute;
        return description == null || string.IsNullOrWhiteSpace(description.Description)
            ? scriptType.Name
            : description.Description;
    }

    private static object[] GetRunningScriptInstances()
    {
        // SHVDN owns the live script instances, so mods do not need to register with or reference this menu.
        List<object> instances = new List<object>();
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        for (int assemblyIndex = 0; assemblyIndex < assemblies.Length; assemblyIndex++)
        {
            Type domainType = assemblies[assemblyIndex].GetType("SHVDN.ScriptDomain", false);
            if (domainType == null)
            {
                continue;
            }

            PropertyInfo currentDomainProperty = domainType.GetProperty("CurrentDomain", BindingFlags.Public | BindingFlags.Static);
            PropertyInfo runningScriptsProperty = domainType.GetProperty("RunningScripts", BindingFlags.Public | BindingFlags.Instance);
            if (currentDomainProperty == null || runningScriptsProperty == null)
            {
                continue;
            }

            object currentDomain = currentDomainProperty.GetValue(null, null);
            Array runningScripts = currentDomain == null ? null : runningScriptsProperty.GetValue(currentDomain, null) as Array;
            if (runningScripts == null)
            {
                return instances.ToArray();
            }

            for (int scriptIndex = 0; scriptIndex < runningScripts.Length; scriptIndex++)
            {
                object runningScript = runningScripts.GetValue(scriptIndex);
                PropertyInfo instanceProperty = runningScript == null
                    ? null
                    : runningScript.GetType().GetProperty("ScriptInstance", BindingFlags.Public | BindingFlags.Instance);

                if (instanceProperty != null)
                {
                    instances.Add(instanceProperty.GetValue(runningScript, null));
                }
            }

            return instances.ToArray();
        }

        return instances.ToArray();
    }

    private static List<MenuAction> DiscoverActions(object scriptInstance)
    {
        List<MenuAction> actions = new List<MenuAction>();
        MethodInfo[] methods = scriptInstance.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        for (int methodIndex = 0; methodIndex < methods.Length; methodIndex++)
        {
            MethodInfo method = methods[methodIndex];
            BrowsableAttribute browsable = Attribute.GetCustomAttribute(method, typeof(BrowsableAttribute), false) as BrowsableAttribute;
            if (browsable == null || !browsable.Browsable || method.ReturnType != typeof(void) || method.GetParameters().Length != 0)
            {
                continue;
            }

            DescriptionAttribute description = Attribute.GetCustomAttribute(method, typeof(DescriptionAttribute), false) as DescriptionAttribute;
            CategoryAttribute category = Attribute.GetCustomAttribute(method, typeof(CategoryAttribute), false) as CategoryAttribute;
            string displayName = description == null || string.IsNullOrWhiteSpace(description.Description)
                ? method.Name
                : description.Description;
            string categoryName = category == null ? string.Empty : category.Category;

            actions.Add(new MenuAction(scriptInstance, method, displayName, categoryName));
        }

        actions.Sort(delegate(MenuAction left, MenuAction right)
        {
            int categoryComparison = StringComparer.OrdinalIgnoreCase.Compare(left.Category, right.Category);
            return categoryComparison != 0
                ? categoryComparison
                : StringComparer.OrdinalIgnoreCase.Compare(left.DisplayName, right.DisplayName);
        });

        return actions;
    }

    private void AddModMenu(DiscoveredMod mod)
    {
        NativeMenu modMenu = new NativeMenu("eXntrc's Mods", mod.Name);
        modMenu.KeepNameCasing = true;
        menuPool.Add(modMenu);
        mainMenu.AddSubMenu(modMenu, ">");

        string currentCategory = null;

        for (int index = 0; index < mod.Actions.Count; index++)
        {
            MenuAction action = mod.Actions[index];
            if (!string.IsNullOrEmpty(action.Category) && !string.Equals(currentCategory, action.Category, StringComparison.OrdinalIgnoreCase))
            {
                currentCategory = action.Category;
                modMenu.Add(new NativeSeparatorItem(currentCategory));
            }

            NativeItem item = new NativeItem(action.DisplayName);
            item.Activated += delegate
            {
                InvokeAction(mod.Name, action);
            };
            modMenu.Add(item);
        }
    }

    private static void InvokeAction(string modName, MenuAction action)
    {
        try
        {
            action.Method.Invoke(action.ScriptInstance, null);
        }
        catch (Exception exception)
        {
            TargetInvocationException invocationException = exception as TargetInvocationException;
            Exception reportedException = invocationException != null && invocationException.InnerException != null
                ? invocationException.InnerException
                : exception;
            GTA.UI.Notification.PostTicker(modName + " action failed: " + reportedException.Message, false, false);
        }
    }

    private void OnKeyDown(object sender, KeyEventArgs eventArgs)
    {
        if (eventArgs.KeyCode != openKey)
        {
            return;
        }

        ToggleMenu();
    }

    private void ToggleMenu()
    {
        if (menuPool.AreAnyVisible)
        {
            menuPool.HideAll();
        }
        else
        {
            if (!menusBuilt)
            {
                BuildMenus();
            }

            mainMenu.Visible = true;
        }
    }

    private void OnTick(object sender, EventArgs eventArgs)
    {
        if (IsControllerChordActivated())
        {
            ToggleMenu();
        }

        menuPool.Process();
    }

    private bool IsControllerChordActivated()
    {
        if (controllerChord.Length == 0)
        {
            return false;
        }

        // Earlier controls are modifiers; the last control edge-triggers the chord once per press.
        for (int index = 0; index < controllerChord.Length - 1; index++)
        {
            if (!Game.IsControlPressed(controllerChord[index]))
            {
                return false;
            }
        }

        GTA.Control trigger = controllerChord[controllerChord.Length - 1];
        if (!Game.IsControlJustPressed(trigger))
        {
            return false;
        }

        Game.DisableControlThisFrame(trigger);
        return true;
    }

    private sealed class MenuAction
    {
        public MenuAction(object scriptInstance, MethodInfo method, string displayName, string category)
        {
            ScriptInstance = scriptInstance;
            Method = method;
            DisplayName = displayName;
            Category = category;
        }

        public object ScriptInstance { get; private set; }
        public MethodInfo Method { get; private set; }
        public string DisplayName { get; private set; }
        public string Category { get; private set; }
    }

    private sealed class DiscoveredMod
    {
        public DiscoveredMod(string name, List<MenuAction> actions)
        {
            Name = name;
            Actions = actions;
        }

        public string Name { get; private set; }
        public List<MenuAction> Actions { get; private set; }
    }
}