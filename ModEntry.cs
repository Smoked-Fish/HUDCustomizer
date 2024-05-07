global using SObject = StardewValley.Object;
using HarmonyLib;
using HUDCustomizer.Framework.Interfaces;
using HUDCustomizer.Framework.Managers;
using HUDCustomizer.Framework.Patches.G1;
using HUDCustomizer.Framework.Patches.Menus;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;

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

            new Game1Patch(harmony).Apply();
            new DayTimeMoneyBoxPatch(harmony).Apply();
            new ToolbarPatch(harmony).Apply();


            // Hook into GameLoop events
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.Input.ButtonPressed += OnButtonPressed;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var configApi = apiManager.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu", false);
            if (Helper.ModRegistry.IsLoaded("spacechase0.GenericModConfigMenu") && configApi != null)
            {
                configApi.Register(ModManifest, () => modConfig = new ModConfig(), () => Helper.WriteConfig(modConfig));

                configApi.AddPageLink(ModManifest, "DTMBoxPage", () => String.Concat("> ", I18n.Config_HUDCustomizer_DTMBox_Title()));
                configApi.AddPageLink(ModManifest, "ToolbarPage", () => String.Concat("> ", I18n.Config_HUDCustomizer_Toolbar_Title()));
                configApi.AddPageLink(ModManifest, "ResourceBarsPage", () => String.Concat("> ", I18n.Config_HUDCustomizer_ResourceBars_Title()));
                configApi.AddPageLink(ModManifest, "BuffsPage", () => String.Concat("> ", I18n.Config_HUDCustomizer_Buffs_Title()));
                configApi.AddPageLink(ModManifest, "OtherPage", () => String.Concat("> ", I18n.Config_HUDCustomizer_Other_Title()));

                configApi.AddPage(ModManifest, "DTMBoxPage", I18n.Config_HUDCustomizer_DTMBox_Title);
                configApi.AddKeybindList(ModManifest, () => modConfig.ToggleDTMBoxHUD, value => modConfig.ToggleDTMBoxHUD = value, I18n.Config_HUDCustomizer_ToggleDTMBoxHUD_Name, I18n.Config_HUDCustomizer_ToggleDTMBoxHUD_Description);
                configApi.AddBoolOption(ModManifest, () => modConfig.DisableDTMBoxHUD, value => modConfig.DisableDTMBoxHUD = value, I18n.Config_HUDCustomizer_DisableDTMBoxHUD_Name, I18n.Config_HUDCustomizer_DisableDTMBoxHUD_Description);
                configApi.AddNumberOption(ModManifest, () => modConfig.DTMBoxScale, value => modConfig.DTMBoxScale = value, I18n.Config_HUDCustomizer_DTMBoxScale_Name, I18n.Config_HUDCustomizer_DTMBoxScale_Description, 0f, 2f, 0.05f, value => value.ToString("0.00"));

                configApi.AddPage(ModManifest, "ToolbarPage", I18n.Config_HUDCustomizer_DTMBox_Title);
                configApi.AddKeybindList(ModManifest, () => modConfig.ToggleToolbarHUD, value => modConfig.ToggleToolbarHUD = value, I18n.Config_HUDCustomizer_ToggleToolbarHUD_Name, I18n.Config_HUDCustomizer_ToggleToolbarHUD_Description);
                configApi.AddBoolOption(ModManifest, () => modConfig.DisableToolbarHUD, value => modConfig.DisableToolbarHUD = value, I18n.Config_HUDCustomizer_DisableToolbarHUD_Name, I18n.Config_HUDCustomizer_DisableToolbarHUD_Description);
                configApi.AddNumberOption(ModManifest, () => modConfig.ToolbarScale, value => modConfig.ToolbarScale = value, I18n.Config_HUDCustomizer_ToolbarScale_Name, I18n.Config_HUDCustomizer_ToolbarScale_Description, 0f, 2f, 0.05f, value => value.ToString("0.00"));

                configApi.AddPage(ModManifest, "ResourceBarsPage", I18n.Config_HUDCustomizer_ResourceBars_Title);
                configApi.AddKeybindList(ModManifest, () => modConfig.ToggleResourceBarsHUD, value => modConfig.ToggleResourceBarsHUD = value, I18n.Config_HUDCustomizer_ToggleResourceBarsHUD_Name, I18n.Config_HUDCustomizer_ToggleResourceBarsHUD_Description);
                configApi.AddBoolOption(ModManifest, () => modConfig.DisableResourceBarsHUD, value => modConfig.DisableResourceBarsHUD = value, I18n.Config_HUDCustomizer_DisableResourceBarsHUD_Name, I18n.Config_HUDCustomizer_DisableResourceBarsHUD_Description);
                configApi.AddNumberOption(ModManifest, () => modConfig.ResourceBarsScale, value => modConfig.ResourceBarsScale = value, I18n.Config_HUDCustomizer_ResourceBarsScale_Name, I18n.Config_HUDCustomizer_ResourceBarsScale_Description, 0f, 2f, 0.05f, value => value.ToString("0.00"));

                configApi.AddPage(ModManifest, "BuffsPage", I18n.Config_HUDCustomizer_Buffs_Title);
                configApi.AddKeybindList(ModManifest, () => modConfig.ToggleBuffsHUD, value => modConfig.ToggleBuffsHUD = value, I18n.Config_HUDCustomizer_ToggleBuffsHUD_Name, I18n.Config_HUDCustomizer_ToggleBuffsHUD_Description);
                configApi.AddBoolOption(ModManifest, () => modConfig.DisableBuffsHUD, value => modConfig.DisableBuffsHUD = value, I18n.Config_HUDCustomizer_DisableBuffsHUD_Name, I18n.Config_HUDCustomizer_DisableBuffsHUD_Description);
                configApi.AddNumberOption(ModManifest, () => modConfig.BuffsScale, value => modConfig.BuffsScale = value, I18n.Config_HUDCustomizer_BuffsScale_Name, I18n.Config_HUDCustomizer_BuffsScale_Description, 0f, 2f, 0.05f, value => value.ToString("0.00"));

                configApi.AddPage(ModManifest, "OtherPage", I18n.Config_HUDCustomizer_Other_Title);
                configApi.AddBoolOption(ModManifest, () => modConfig.EnableMod, value => modConfig.EnableMod = value, I18n.Config_HUDCustomizer_EnableMod_Name, I18n.Config_HUDCustomizer_EnableMod_Description);
            }
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e) 
        {
            if (!Context.IsWorldReady) return;

            if (modConfig.ToggleDTMBoxHUD.JustPressed())
            {
                modConfig.ToggleDTMBoxHUDState();
            }
            if (modConfig.ToggleToolbarHUD.JustPressed())
            {
                modConfig.ToggleToolbarHUDState();
            }
            if (modConfig.ToggleResourceBarsHUD.JustPressed())
            {
                modConfig.ToggleResourceBarsHUDState();
            }
            if (modConfig.ToggleBuffsHUD.JustPressed())
            {
                modConfig.ToggleBuffsHUDState();
            }
        }
    }
}