using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HUDCustomizer.Framework.Patches.Menus
{
    internal class BuffsDisplayPatch : PatchTemplate
    {
        internal BuffsDisplayPatch(Harmony harmony) : base(harmony, typeof(BuffsDisplay)) { }

        internal void Apply()
        {
            //Patch(PatchType.Prefix, nameof(BuffsDisplay.draw), nameof(DrawPrefix), [typeof(SpriteBatch)]);
        }

        public static bool DrawPrefix(BuffsDisplay __instance, SpriteBatch b)
        {
            if (!ModEntry.modConfig.EnableMod) return true;

            MethodInfo updatePosition = typeof(BuffsDisplay).GetMethod("updatePosition", BindingFlags.NonPublic | BindingFlags.Instance);
            updatePosition.Invoke(__instance, null);

            Dictionary<ClickableTextureComponent, Buff> buffs = (Dictionary<ClickableTextureComponent, Buff>)typeof(BuffsDisplay).GetField("buffs", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            foreach (KeyValuePair<ClickableTextureComponent, Buff> pair in buffs)
            {
                pair.Key.draw(b, Color.White * ((pair.Value.displayAlphaTimer > 0f) ? ((float)(Math.Cos(pair.Value.displayAlphaTimer / 100f) + 3.0) / 4f) : 1f), 0.8f);
                pair.Value.alreadyUpdatedIconAlpha = false;
            }
            if (__instance.hoverText.Length != 0 && __instance.isWithinBounds(Game1.getOldMouseX(), Game1.getOldMouseY()))
            {
                __instance.performHoverAction(Game1.getOldMouseX(), Game1.getOldMouseY());
                IClickableMenu.drawHoverText(b, __instance.hoverText, Game1.smallFont);
            }

            return false;
        }
    }
}
