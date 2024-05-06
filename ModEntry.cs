global using SObject = StardewValley.Object;
using HarmonyLib;
using HUDCustomizer.Framework.Interfaces;
using HUDCustomizer.Framework.Managers;
using HUDCustomizer.Framework.Patches;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Reflection;

namespace HUDCustomizer
{
    public class ModEntry : Mod
    {
        // Shared static helpers
        internal static IMonitor monitor;
        internal static IModHelper modHelper;
        internal static ModConfig modConfig;
        internal static Multiplayer multiplayer;

        // Managers
        internal static ApiManager apiManager;

        public override void Entry(IModHelper helper)
        {
            // Setup i18n
            I18n.Init(helper.Translation);

            // Setup the monitor, helper, config and multiplayer
            monitor = Monitor;
            modHelper = helper;
            modConfig = Helper.ReadConfig<ModConfig>();
            multiplayer = helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();

            // Setup the manager
            apiManager = new ApiManager();

            // Apply the patches
            var harmony = new Harmony(ModManifest.UniqueID);

            new DrawPatches(harmony).Apply();


            // Hook into GameLoop events
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            //helper.Events.Input.ButtonPressed += OnButtonPressed;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var configApi = apiManager.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu", false);
            if (Helper.ModRegistry.IsLoaded("spacechase0.GenericModConfigMenu") && configApi != null)
            {
                configApi.Register(ModManifest, () => modConfig = new ModConfig(), () => Helper.WriteConfig(modConfig));

                AddOption(configApi, nameof(modConfig.EnableMod));
                AddOption(configApi, nameof(modConfig.ToggleHudKey));
                AddOption(configApi, nameof(modConfig.DisableDayTimeMoneyBox));
            }
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e) 
        {
            if (!Context.IsWorldReady) return;

            if (modConfig.ToggleHudKey.JustPressed())
            {
                if (modConfig.DisableDayTimeMoneyBox)
                {
                    modConfig.DisableDayTimeMoneyBox = false;
                }
                else
                {
                    modConfig.DisableDayTimeMoneyBox = true;
                }
            }
        }

        private void AddOption(IGenericModConfigMenuApi configApi, string name)
        {
            PropertyInfo propertyInfo = typeof(ModConfig).GetProperty(name);
            if (propertyInfo == null)
            {
                Monitor.Log($"Error: Property '{name}' not found in ModConfig.", LogLevel.Error);
                return;
            }

            Func<string> getName = () => I18n.GetByKey($"Config.{typeof(ModEntry).Namespace}.{name}.Name");
            Func<string> getDescription = () => I18n.GetByKey($"Config.{typeof(ModEntry).Namespace}.{name}.Description");

            if (getName == null || getDescription == null)
            {
                Monitor.Log($"Error: Localization keys for '{name}' not found.", LogLevel.Error);
                return;
            }

            var getterMethod = propertyInfo.GetGetMethod();
            var setterMethod = propertyInfo.GetSetMethod();

            if (getterMethod == null || setterMethod == null)
            {
                Monitor.Log($"Error: The get/set methods are null for property '{name}'.", LogLevel.Error);
                return;
            }

            var getter = Delegate.CreateDelegate(typeof(Func<>).MakeGenericType(propertyInfo.PropertyType), modConfig, getterMethod);
            var setter = Delegate.CreateDelegate(typeof(Action<>).MakeGenericType(propertyInfo.PropertyType), modConfig, setterMethod);

            switch (propertyInfo.PropertyType.Name)
            {
                case nameof(Boolean):
                    configApi.AddBoolOption(ModManifest, (Func<bool>)getter, (Action<bool>)setter, getName, getDescription);
                    break;
                case nameof(Int32):
                    configApi.AddNumberOption(ModManifest, (Func<int>)getter, (Action<int>)setter, getName, getDescription);
                    break;
                case nameof(Single):
                    configApi.AddNumberOption(ModManifest, (Func<float>)getter, (Action<float>)setter, getName, getDescription);
                    break;
                case nameof(String):
                    configApi.AddTextOption(ModManifest, (Func<string>)getter, (Action<string>)setter, getName, getDescription);
                    break;
                case nameof(SButton):
                    configApi.AddKeybind(ModManifest, (Func<SButton>)getter, (Action<SButton>)setter, getName, getDescription);
                    break;
                case nameof(KeybindList):
                    configApi.AddKeybindList(ModManifest, (Func<KeybindList>)getter, (Action<KeybindList>)setter, getName, getDescription);
                    break;
                default:
                    Monitor.Log($"Error: Unsupported property type '{propertyInfo.PropertyType.Name}' for '{name}'.", LogLevel.Error);
                    break;
            }
        }
    }
}