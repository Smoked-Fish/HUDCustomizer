using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace HUDCustomizer
{
    internal class ModConfig
    {
        public bool EnableMod { get; set; } = true;

        public KeybindList ToggleDTMBoxHUD {  get; set; } = new KeybindList(SButton.None);
        public bool DisableDTMBoxHUD { get; set; } = false;
        public float DTMBoxScale { get; set; } = 1f;

        public KeybindList ToggleToolbarHUD { get; set; } = new KeybindList(SButton.None);
        public bool DisableToolbarHUD { get; set; } = false;
        public float ToolbarScale { get; set; } = 1f;

        public KeybindList ToggleResourceBarsHUD { get; set; } = new KeybindList(SButton.None);
        public bool DisableResourceBarsHUD { get; set; } = false;
        public float ResourceBarsScale { get; set; } = 1f;

        public KeybindList ToggleBuffsHUD { get; set; } = new KeybindList(SButton.None);
        public bool DisableBuffsHUD { get; set; } = false;
        public float BuffsScale { get; set; } = 1f;

        public ModConfig() 
        {
            DTMBoxScale = 1f;
            ToolbarScale = 1f;
            ResourceBarsScale = 1f;
            BuffsScale = 1f;
        }

        public void ToggleDTMBoxHUDState()
        {
            DisableDTMBoxHUD = !DisableDTMBoxHUD;
        }

        public void ToggleToolbarHUDState()
        {
            DisableToolbarHUD = !DisableToolbarHUD;
        }

        public void ToggleResourceBarsHUDState()
        {
            DisableResourceBarsHUD = !DisableResourceBarsHUD;
        }

        public void ToggleBuffsHUDState()
        {
            DisableBuffsHUD = !DisableBuffsHUD;
        }
    }
}
