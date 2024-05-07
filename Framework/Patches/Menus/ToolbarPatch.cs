using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System;

namespace HUDCustomizer.Framework.Patches.Menus
{
    internal class ToolbarPatch : PatchTemplate
    {
        internal ToolbarPatch(Harmony harmony) : base(harmony, typeof(Toolbar)) { }

        internal void Apply()
        {
            Patch(PatchType.Prefix, nameof(Toolbar.draw), nameof(DrawPrefix), [typeof(SpriteBatch)]);
        }




        private static bool DrawPrefix(Toolbar __instance, SpriteBatch b)
        {
            //if (!ModEntry.modConfig.EnableMod || !ModEntry.modConfig.ToggleToolbarHUD.IsDown()) return true;
            if (!ModEntry.modConfig.EnableMod) return true;
            if (ModEntry.modConfig.ToolbarScale == 1f) return true;
            if (Game1.activeClickableMenu != null) return false;

            float scaleX = ModEntry.modConfig.ToolbarScale;
            float scaleY = ModEntry.modConfig.ToolbarScale;

            float toolbarWidth = 12 * 64 * scaleX;
            float startX = (Game1.uiViewport.Width - toolbarWidth) / 2;
            int widthDifference = (int)((800 - (800 * scaleX)) / 2);
            int heightDifference = (int)((96 - (96 * scaleY)) / 2);
            float desiredY = __instance.yPositionOnScreen - (96 * scaleY);
            int adjustedX = (int)(Game1.uiViewport.Width / 2 - 384 - 16 + widthDifference);
            int adjustedY = (int)(__instance.yPositionOnScreen - heightDifference);



            Point playerGlobalPos = Game1.player.StandingPixel;
            Vector2 playerLocalVec = Game1.GlobalToLocal(globalPosition: new Vector2(playerGlobalPos.X, playerGlobalPos.Y), viewport: Game1.viewport);
            bool alignTop;
            if (Game1.options.pinToolbarToggle)
            {
                alignTop = false;
                __instance.transparency = Math.Min(1f, __instance.transparency + 0.075f);
                if (playerLocalVec.Y > (float)(Game1.viewport.Height - 192))
                {
                    __instance.transparency = Math.Max(0.33f, __instance.transparency - 0.15f);
                }
            }
            else
            {
                alignTop = ((playerLocalVec.Y > (float)(Game1.viewport.Height / 2 + 64)) ? true : false);
                __instance.transparency = 1f;
            }
            int margin = Utility.makeSafeMarginY(8);
            int num = __instance.yPositionOnScreen;
            if (!alignTop)
            {
                __instance.yPositionOnScreen = Game1.uiViewport.Height;
                __instance.yPositionOnScreen += 8;
                __instance.yPositionOnScreen -= margin;
            }
            else
            {
                __instance.yPositionOnScreen = (int)(112 * scaleY);
                __instance.yPositionOnScreen -= 8;
                __instance.yPositionOnScreen += margin;
            }
            if (num != __instance.yPositionOnScreen)
            {
                for (int k = 0; k < 12; k++)
                {
                    __instance.buttons[k].bounds.Y = (int)(desiredY + (8 * scaleY));
                }
            }
            for (int k = 0; k < 12; k++)
            {
                __instance.buttons[k].bounds.Y = (int)(desiredY + (8 * scaleY));
                __instance.buttons[k].bounds.X = (int)(startX + k * 64 * scaleX);

                __instance.buttons[k].bounds.Width = (int)(64 * scaleX);
                __instance.buttons[k].bounds.Height = (int)(64 * scaleY);


            }

            IClickableMenu.drawTextureBox(
                b,
                Game1.menuTexture,
                __instance.toolbarTextSource,
                adjustedX,
                (int)(desiredY - (8 * scaleX)),
                (int)(800 * scaleX),
                (int)(96 * scaleY),
                Color.White * __instance.transparency,
                scaleX,
                drawShadow: false
            );

            for (int j = 0; j < 12; j++)
            {
                Vector2 buttonPosition = new Vector2(startX + j * 64 * scaleX, desiredY + (8 * scaleY));

                // Calculate the position of the item centered inside the button
                Vector2 itemPosition = buttonPosition + new Vector2((64 * scaleX - 64) / 2, (64 * scaleY - 64) / 2);
                float itemScale = Math.Min(scaleX, scaleY);

                b.Draw(Game1.menuTexture,
                    buttonPosition,
                    Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, (Game1.player.CurrentToolIndex == j) ? 56 : 10),
                    Color.White * __instance.transparency,
                    0f,
                    Vector2.Zero,
                    new Vector2(scaleX, scaleY),
                    SpriteEffects.None,
                    1f);


                if (!Game1.options.gamepadControls)
                {
                    Vector2 textPosition = buttonPosition + new Vector2(4f, -8f);
                    Color textColor = Color.DimGray * __instance.transparency;
                    b.DrawString(Game1.tinyFont, __instance.slotText[j], textPosition, textColor);
                }

                if (Game1.player.Items.Count > j && Game1.player.Items[j] != null)
                {
                    if (Game1.player.Items[j].Category == -96) // If the item is a ring
                    {
                        // Calculate the adjustment based on itemScale
                        // If scale is greater than 1, don't change adjustment values
                        if (itemScale < 1)
                        {
                            float diminishingFactor = 1 - itemScale;

                            int ringX = 16;
                            int ringY = 24;


                            if (itemScale <= 0.4)
                            {
                                ringX = 32;
                                ringY = 32;
                            }

                            float ringAdjustmentX = ringX * diminishingFactor;
                            float ringdAdjustmentY = ringY * diminishingFactor;
                            // Adjust position for rings
                            itemPosition += new Vector2(ringAdjustmentX, ringdAdjustmentY);
                        }
                    }
                    Game1.player.Items[j].drawInMenu(b,
                        itemPosition,
                        (Game1.player.CurrentToolIndex == j) ? (scaleY * 0.9f) : (scaleY * 0.8f),
                        __instance.transparency,
                        0.88f,
                        StackDrawType.Hide);
                }






                if (Game1.player.Items.Count > j && Game1.player.Items[j] != null)
                {
                    if (Game1.player.Items[j].Quality > 0)
                    {
                        Rectangle qualityRect = (((int)Game1.player.Items[j].Quality < 4) ? new Rectangle(338 + ((int)Game1.player.Items[j].Quality - 1) * 8, 400, 8, 8) : new Rectangle(346, 392, 8, 8));
                        Texture2D qualitySheet = Game1.mouseCursors;
                        Vector2 qualityPosition = buttonPosition + new Vector2(12f * scaleX, 52f * scaleY);
                        float yOffset = (((int)Game1.player.Items[j].Quality < 4) ? 0f : (((float)Math.Cos((double)Game1.currentGameTime.TotalGameTime.Milliseconds * Math.PI / 512.0) + 1f) * 0.05f));
                        b.Draw(qualitySheet, qualityPosition, qualityRect, Color.White * __instance.transparency, 0f,
                            new Vector2(4f, 4f), 3f * ((Game1.player.CurrentToolIndex == j) ? (itemScale * 0.9f) : (itemScale * 0.8f)) * (1f + yOffset), SpriteEffects.None, 0.88f);
                    }
                    else if (Game1.player.Items[j].Category == -102 && Game1.player.stats.Get(Game1.player.Items[j].ItemId) != 0)
                    {
                        Vector2 bookPosition = buttonPosition + new Vector2(12 * scaleX, 44f * scaleY);

                        b.Draw(Game1.mouseCursors_1_6, bookPosition, new Rectangle(244, 271, 9, 11), Color.White * __instance.transparency, 0f, new Vector2(4f, 4f), 3f * ((Game1.player.CurrentToolIndex == j) ? (itemScale * 0.9f) : (itemScale * 0.8f)) * 1f, SpriteEffects.None, 0.88f);
                    }

                    // Draw item stack number
                    int drawnStack = Game1.player.Items[j].Stack;
                    bool shouldDrawStackNumber = ((Game1.player.Items[j].maximumStackSize() > 1 && drawnStack > 1)) && (double)itemScale > 0.3 && drawnStack != int.MaxValue && scaleY >= 0.6f;
                    if (Game1.player.Items[j].IsRecipe)
                    {
                        shouldDrawStackNumber = false;
                    }
                    if (shouldDrawStackNumber)
                    {
                        float scale_size = (Game1.player.CurrentToolIndex == j) ? (itemScale * 0.9f) : (itemScale * 0.8f);
                        float stackMarginHeight = 0; // Get the margin height

                        // Adjust the position for scaleY less than 1
                        if (scaleY < 1)
                        {
                            // When scaleY is less than 1, adjust the stackPositionY by scaling the margin height
                            stackMarginHeight = margin * scaleY;
                        }

                        Vector2 stackPosition = itemPosition +
                                                new Vector2((64 * scaleX + (widthDifference / 12) - Utility.getWidthOfTinyDigitString(drawnStack, 3f * scale_size)) + 3f * scale_size,
                                                64 * scaleY + heightDifference - 18 * scale_size + 1f - stackMarginHeight);

                        Utility.drawTinyDigits(drawnStack, b, stackPosition, 3f * scale_size, Math.Min(1f, 0.88f + 1E-06f), Color.White * __instance.transparency);
                    }
                }

                Rectangle buttonBounds = __instance.buttons[j].bounds;
                //b.Draw(Game1.staminaRect, new Rectangle(buttonBounds.X, buttonBounds.Y, buttonBounds.Width, buttonBounds.Height), new Color(255, 0, 0, 100));
            }

            if (__instance.hoverItem != null)
            {
                IClickableMenu.drawToolTip(b, __instance.hoverItem.getDescription(), __instance.hoverItem.DisplayName, __instance.hoverItem);
                __instance.hoverItem = null;
            }

            return false;
        }
    }
}
