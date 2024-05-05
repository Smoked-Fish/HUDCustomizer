using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace HUDCustomizer
{
    internal class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public KeybindList ToggleHudKey {  get; set; } = new KeybindList(SButton.Q);
    }
}
