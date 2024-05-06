using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework;
using StardewValley.Extensions;
using StardewModdingAPI;
using Microsoft.CodeAnalysis.Text;
using StardewValley.BellsAndWhistles;
using StardewValley.Logging;
using System.Reflection;
using System.Text;
using System.Collections;

namespace HUDCustomizer.Framework.Patches
{
    internal class DrawPatches : PatchTemplate
    {
        internal DrawPatches(Harmony harmony) : base(harmony) { }
        internal void Apply()
        {
            //Patch(PatchType.Prefix, typeof(Game1), "drawHUD", nameof(DrawHudPrefix));
            Patch(PatchType.Prefix, typeof(DayTimeMoneyBox), nameof(DayTimeMoneyBox.draw), nameof(DrawDayTimeMoneyBoxPrefix), [typeof(SpriteBatch)]);
            Patch(PatchType.Prefix, typeof(Toolbar), nameof(Toolbar.draw), nameof(DrawToolbarPrefix), [typeof(SpriteBatch)]);
        }
        private static bool DrawHudPrefix(Game1 __instance)
        {
            if (!ModEntry.modConfig.EnableMod) return true;

            if (Game1.eventUp || Game1.farmEvent != null)
            {
                return false;
            }



            // Calculate the top position of the stamina bar
            float modifier = 0.625f;
            Vector2 topOfBar = new Vector2(Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Right - 48 - 8, Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Bottom - 224 - 16 - (int)((float)(Game1.player.MaxStamina - 270) * modifier));

            // Adjust topOfBar if the outdoor map is smaller than the viewport
            if (Game1.isOutdoorMapSmallerThanViewport())
            {
                topOfBar.X = Math.Min(topOfBar.X, -Game1.viewport.X + Game1.currentLocation.map.Layers[0].LayerWidth * 64 - 48);
            }

            // Shake the stamina bar if required
            if (Game1.staminaShakeTimer > 0)
            {
                topOfBar.X += Game1.random.Next(-3, 4);
                topOfBar.Y += Game1.random.Next(-3, 4);
            }

            // Draw stamina bar components
            Game1.spriteBatch.Draw(Game1.mouseCursors, topOfBar, new Rectangle(256, 408, 12, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
            Game1.spriteBatch.Draw(Game1.mouseCursors, new Rectangle((int)topOfBar.X, (int)(topOfBar.Y + 64f), 48, Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Bottom - 64 - 16 - (int)(topOfBar.Y + 64f - 8f)), new Rectangle(256, 424, 12, 16), Color.White);
            Game1.spriteBatch.Draw(Game1.mouseCursors, new Vector2(topOfBar.X, topOfBar.Y + 224f + (float)(int)((float)(Game1.player.MaxStamina - 270) * modifier) - 64f), new Rectangle(256, 448, 12, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);

            // Draw the stamina bar
            Rectangle r = new Rectangle((int)topOfBar.X + 12, (int)topOfBar.Y + 16 + 32 + (int)((float)Game1.player.MaxStamina * modifier) - (int)(Math.Max(0f, Game1.player.Stamina) * modifier), 24, (int)(Game1.player.Stamina * modifier) - 1);
            if ((float)Game1.getOldMouseX() >= topOfBar.X && (float)Game1.getOldMouseY() >= topOfBar.Y)
            {
                Game1.drawWithBorder((int)Math.Max(0f, Game1.player.Stamina) + "/" + Game1.player.MaxStamina, Color.Black * 0f, Color.White, topOfBar + new Vector2(0f - Game1.dialogueFont.MeasureString("999/999").X - 16f - (float)(Game1.showingHealth ? 64 : 0), 64f));
            }
            Color c = Utility.getRedToGreenLerpColor(Game1.player.stamina / (float)(int)Game1.player.MaxStamina);
            Game1.spriteBatch.Draw(Game1.staminaRect, r, c);

            // Draw additional stamina bar components
            r.Height = 4;
            c.R = (byte)Math.Max(0, c.R - 50);
            c.G = (byte)Math.Max(0, c.G - 50);
            Game1.spriteBatch.Draw(Game1.staminaRect, r, c);

            // Draw exhaustion indicator if player is exhausted
            if ((bool)Game1.player.exhausted.Value)
            {
                Game1.spriteBatch.Draw(Game1.mouseCursors, topOfBar - new Vector2(0f, 11f) * 4f, new Rectangle(191, 406, 12, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
                if ((float)Game1.getOldMouseX() >= topOfBar.X && (float)Game1.getOldMouseY() >= topOfBar.Y - 44f)
                {
                    Game1.drawWithBorder(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3747"), Color.Black * 0f, Color.White, topOfBar + new Vector2(0f - Game1.dialogueFont.MeasureString(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3747")).X - 16f - (float)(Game1.showingHealth ? 64 : 0), 96f));
                }
            }

            // Check if health bar needs to be shown
            if (Game1.currentLocation is MineShaft || Game1.currentLocation is Woods || Game1.currentLocation is SlimeHutch || Game1.currentLocation is VolcanoDungeon || Game1.player.health < Game1.player.maxHealth)
            {
                Game1.showingHealthBar = true;
                Game1.showingHealth = true;
                int bar_full_height = 168 + (Game1.player.maxHealth - 100);
                int height = (int)((float)Game1.player.health / (float)Game1.player.maxHealth * (float)bar_full_height);
                topOfBar.X -= 56 + ((Game1.hitShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0);
                topOfBar.Y = Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Bottom - 224 - 16 - (Game1.player.maxHealth - 100);
                Game1.spriteBatch.Draw(Game1.mouseCursors, topOfBar, new Rectangle(268, 408, 12, 16), (Game1.player.health < 20) ? (Color.Pink * ((float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / (double)((float)Game1.player.health * 50f)) / 4f + 0.9f)) : Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
                Game1.spriteBatch.Draw(Game1.mouseCursors, new Rectangle((int)topOfBar.X, (int)(topOfBar.Y + 64f), 48, Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Bottom - 64 - 16 - (int)(topOfBar.Y + 64f)), new Rectangle(268, 424, 12, 16), (Game1.player.health < 20) ? (Color.Pink * ((float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / (double)((float)Game1.player.health * 50f)) / 4f + 0.9f)) : Color.White);
                Game1.spriteBatch.Draw(Game1.mouseCursors, new Vector2(topOfBar.X, topOfBar.Y + 224f + (float)(Game1.player.maxHealth - 100) - 64f), new Rectangle(268, 448, 12, 16), (Game1.player.health < 20) ? (Color.Pink * ((float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / (double)((float)Game1.player.health * 50f)) / 4f + 0.9f)) : Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
                Rectangle health_bar_rect = new Rectangle((int)topOfBar.X + 12, (int)topOfBar.Y + 16 + 32 + bar_full_height - height, 24, height);
                c = Utility.getRedToGreenLerpColor((float)Game1.player.health / (float)Game1.player.maxHealth);
                Game1.spriteBatch.Draw(Game1.staminaRect, health_bar_rect, Game1.staminaRect.Bounds, c, 0f, Vector2.Zero, SpriteEffects.None, 1f);
                c.R = (byte)Math.Max(0, c.R - 50);
                c.G = (byte)Math.Max(0, c.G - 50);
                if ((float)Game1.getOldMouseX() >= topOfBar.X && (float)Game1.getOldMouseY() >= topOfBar.Y && (float)Game1.getOldMouseX() < topOfBar.X + 32f)
                {
                    Game1.drawWithBorder(Math.Max(0, Game1.player.health) + "/" + Game1.player.maxHealth, Color.Black * 0f, Color.Red, topOfBar + new Vector2(0f - Game1.dialogueFont.MeasureString("999/999").X - 32f, 64f));
                }
                health_bar_rect.Height = 4;
                Game1.spriteBatch.Draw(Game1.staminaRect, health_bar_rect, Game1.staminaRect.Bounds, c, 0f, Vector2.Zero, SpriteEffects.None, 1f);
            }
            else
            {
                Game1.showingHealth = false;
            }

            // Draw other menus on screen
            foreach (IClickableMenu menu in Game1.onScreenMenus)
            {
                if (menu != Game1.chatBox)
                {
                    //DayTimeMoneyBox
                    //Toolbar
                    //BuffsDisplay
                    //ModEntry.monitor.Log($"Menu Name: {menu.GetType().Name}", LogLevel.Debug);
                    menu.update(Game1.currentGameTime);
                    menu.draw(Game1.spriteBatch);
                }
            }

            // Check if player has the Tracker profession and is outdoors
            if (!Game1.player.professions.Contains(17) || !Game1.currentLocation.IsOutdoors)
            {
                return false;
            }



            // Draw objects for Tracker profession
            foreach (KeyValuePair<Vector2, SObject> v in Game1.currentLocation.objects.Pairs)
            {
                if (((bool)v.Value.IsSpawnedObject || v.Value.QualifiedItemId == "(O)590") && !Utility.isOnScreen(v.Key * 64f + new Vector2(32f, 32f), 64))
                {
                    Rectangle vpbounds = Game1.graphics.GraphicsDevice.Viewport.Bounds;
                    Vector2 onScreenPosition2 = default(Vector2);
                    float rotation2 = 0f;
                    if (v.Key.X * 64f > (float)(Game1.viewport.MaxCorner.X - 64))
                    {
                        onScreenPosition2.X = vpbounds.Right - 8;
                        rotation2 = (float)Math.PI / 2f;
                    }
                    else if (v.Key.X * 64f < (float)Game1.viewport.X)
                    {
                        onScreenPosition2.X = 8f;
                        rotation2 = -(float)Math.PI / 2f;
                    }
                    else
                    {
                        onScreenPosition2.X = v.Key.X * 64f - (float)Game1.viewport.X;
                    }
                    if (v.Key.Y * 64f > (float)(Game1.viewport.MaxCorner.Y - 64))
                    {
                        onScreenPosition2.Y = vpbounds.Bottom - 8;
                        rotation2 = (float)Math.PI;
                    }
                    else if (v.Key.Y * 64f < (float)Game1.viewport.Y)
                    {
                        onScreenPosition2.Y = 8f;
                    }
                    else
                    {
                        onScreenPosition2.Y = v.Key.Y * 64f - (float)Game1.viewport.Y;
                    }
                    if (onScreenPosition2.X == 8f && onScreenPosition2.Y == 8f)
                    {
                        rotation2 += (float)Math.PI / 4f;
                    }
                    if (onScreenPosition2.X == 8f && onScreenPosition2.Y == (float)(vpbounds.Bottom - 8))
                    {
                        rotation2 += (float)Math.PI / 4f;
                    }
                    if (onScreenPosition2.X == (float)(vpbounds.Right - 8) && onScreenPosition2.Y == 8f)
                    {
                        rotation2 -= (float)Math.PI / 4f;
                    }
                    if (onScreenPosition2.X == (float)(vpbounds.Right - 8) && onScreenPosition2.Y == (float)(vpbounds.Bottom - 8))
                    {
                        rotation2 -= (float)Math.PI / 4f;
                    }

                    // Draw the object
                    Rectangle srcRect = new Rectangle(412, 495, 5, 4);
                    float renderScale = 4f;
                    Vector2 safePos = Utility.makeSafe(renderSize: new Vector2((float)srcRect.Width * renderScale, (float)srcRect.Height * renderScale), renderPos: onScreenPosition2);
                    Game1.spriteBatch.Draw(Game1.mouseCursors, safePos, srcRect, Color.White, rotation2, new Vector2(2f, 2f), renderScale, SpriteEffects.None, 1f);
                }
            }

            // Draw ore panning point if off-screen
            if (!Game1.currentLocation.orePanPoint.Equals(Point.Zero) && !Utility.isOnScreen(Utility.PointToVector2(Game1.currentLocation.orePanPoint.Value) * 64f + new Vector2(32f, 32f), 64))
            {
                // Calculate position and rotation for ore panning point
                Vector2 onScreenPosition = default(Vector2);
                float rotation = 0f;
                if (Game1.currentLocation.orePanPoint.X * 64 > Game1.viewport.MaxCorner.X - 64)
                {
                    onScreenPosition.X = Game1.graphics.GraphicsDevice.Viewport.Bounds.Right - 8;
                    rotation = (float)Math.PI / 2f;
                }
                else if (Game1.currentLocation.orePanPoint.X * 64 < Game1.viewport.X)
                {
                    onScreenPosition.X = 8f;
                    rotation = -(float)Math.PI / 2f;
                }
                else
                {
                    onScreenPosition.X = Game1.currentLocation.orePanPoint.X * 64 - Game1.viewport.X;
                }
                if (Game1.currentLocation.orePanPoint.Y * 64 > Game1.viewport.MaxCorner.Y - 64)
                {
                    onScreenPosition.Y = Game1.graphics.GraphicsDevice.Viewport.Bounds.Bottom - 8;
                    rotation = (float)Math.PI;
                }
                else if (Game1.currentLocation.orePanPoint.Y * 64 < Game1.viewport.Y)
                {
                    onScreenPosition.Y = 8f;
                }
                else
                {
                    onScreenPosition.Y = Game1.currentLocation.orePanPoint.Y * 64 - Game1.viewport.Y;
                }
                if (onScreenPosition.X == 8f && onScreenPosition.Y == 8f)
                {
                    rotation += (float)Math.PI / 4f;
                }
                if (onScreenPosition.X == 8f && onScreenPosition.Y == (float)(Game1.graphics.GraphicsDevice.Viewport.Bounds.Bottom - 8))
                {
                    rotation += (float)Math.PI / 4f;
                }
                if (onScreenPosition.X == (float)(Game1.graphics.GraphicsDevice.Viewport.Bounds.Right - 8) && onScreenPosition.Y == 8f)
                {
                    rotation -= (float)Math.PI / 4f;
                }
                if (onScreenPosition.X == (float)(Game1.graphics.GraphicsDevice.Viewport.Bounds.Right - 8) && onScreenPosition.Y == (float)(Game1.graphics.GraphicsDevice.Viewport.Bounds.Bottom - 8))
                {
                    rotation -= (float)Math.PI / 4f;
                }
                // Draw the ore panning point
                Game1.spriteBatch.Draw(Game1.mouseCursors, onScreenPosition, new Rectangle(412, 495, 5, 4), Color.Cyan, rotation, new Vector2(2f, 2f), 4f, SpriteEffects.None, 1f);
            }
            return false;
        }

        private static bool DrawDayTimeMoneyBoxPrefix(DayTimeMoneyBox __instance, SpriteBatch b)
        {
            if (!ModEntry.modConfig.EnableMod) return true;
            if (ModEntry.modConfig.DisableDayTimeMoneyBox) return false;


            // Get the appropriate font based on the game's language
            SpriteFont font = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? Game1.smallFont : Game1.dialogueFont);

            // Invoke a private method to update the position of the UI element
            MethodInfo updatePosition = typeof(DayTimeMoneyBox).GetMethod("updatePosition", BindingFlags.NonPublic | BindingFlags.Instance);
            updatePosition.Invoke(__instance, null);

            // Decrease timers related to animation/effects
            if (__instance.timeShakeTimer > 0)
            {
                __instance.timeShakeTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
            }
            if (__instance.questPulseTimer > 0)
            {
                __instance.questPulseTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
            }
            if (__instance.whenToPulseTimer >= 0)
            {
                __instance.whenToPulseTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
                if (__instance.whenToPulseTimer <= 0)
                {
                    __instance.whenToPulseTimer = 3000;
                    if (Game1.player.hasNewQuestActivity())
                    {
                        __instance.questPulseTimer = 1000;
                    }
                }
            }


            // Get necessary field values via reflection
            Rectangle sourceRect = (Rectangle)typeof(DayTimeMoneyBox).GetField("sourceRect", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            int _lastDayOfMonth = (int)typeof(DayTimeMoneyBox).GetField("_lastDayOfMonth", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            string _lastDayOfMonthString = (string)typeof(DayTimeMoneyBox).GetField("_lastDayOfMonthString", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            StringBuilder _dateText = (StringBuilder)typeof(DayTimeMoneyBox).GetField("_dateText", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);



            //sourceRect = new Rectangle(333, 431, 71/2, 43/2);



            b.Draw(Game1.mouseCursors, __instance.position, sourceRect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);

            // Update day-related text
            if (Game1.dayOfMonth != _lastDayOfMonth)
            {
                _lastDayOfMonth = Game1.dayOfMonth;
                _lastDayOfMonthString = Game1.shortDayDisplayNameFromDayOfSeason(_lastDayOfMonth);
            }
            _dateText.Clear();

            // Based on language, construct date text
            switch (LocalizedContentManager.CurrentLanguageCode)
            {
                case LocalizedContentManager.LanguageCode.ja:
                    _dateText.AppendEx(Game1.dayOfMonth);
                    _dateText.Append("日 (");
                    _dateText.Append(_lastDayOfMonthString);
                    _dateText.Append(")");
                    break;
                case LocalizedContentManager.LanguageCode.zh:
                    _dateText.AppendEx(Game1.dayOfMonth);
                    _dateText.Append("日 ");
                    _dateText.Append(_lastDayOfMonthString);
                    break;
                case LocalizedContentManager.LanguageCode.mod:
                    _dateText.Append(LocalizedContentManager.CurrentModLanguage.ClockDateFormat.Replace("[DAY_OF_WEEK]", _lastDayOfMonthString).Replace("[DAY_OF_MONTH]", Game1.dayOfMonth.ToString()));
                    break;
                default:
                    _dateText.Append(_lastDayOfMonthString);
                    _dateText.Append(". ");
                    _dateText.AppendEx(Game1.dayOfMonth);
                    break;
            }

            // Draw date text
            Vector2 daySize = font.MeasureString(_dateText);
            Vector2 dayPosition = new Vector2((float)sourceRect.X * 0.5625f - daySize.X / 2f, (float)sourceRect.Y * (LocalizedContentManager.CurrentLanguageLatin ? 0.1f : 0.1f) - daySize.Y / 2f);
            Utility.drawTextWithShadow(b, _dateText, font, __instance.position + dayPosition, Game1.textColor);

            // Draw weather icon
            b.Draw(Game1.mouseCursors, __instance.position + new Vector2(212f, 68f), new Rectangle(406, 441 + Game1.seasonIndex * 8, 12, 8), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
            if (Game1.weatherIcon == 999)
            {
                b.Draw(Game1.mouseCursors_1_6, __instance.position + new Vector2(116f, 68f), new Rectangle(243, 293, 12, 8), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
            }
            else
            {
                b.Draw(Game1.mouseCursors, __instance.position + new Vector2(116f, 68f), new Rectangle(317 + 12 * Game1.weatherIcon, 421, 12, 8), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
            }

            // Get necessary field values via reflection
            StringBuilder _padZeros = (StringBuilder)typeof(DayTimeMoneyBox).GetField("_padZeros", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            StringBuilder _hours = (StringBuilder)typeof(DayTimeMoneyBox).GetField("_hours", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            StringBuilder _temp = (StringBuilder)typeof(DayTimeMoneyBox).GetField("_temp", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            StringBuilder _timeText = (StringBuilder)typeof(DayTimeMoneyBox).GetField("_timeText", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            StringBuilder _hoverText = (StringBuilder)typeof(DayTimeMoneyBox).GetField("_hoverText", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            string _amString = (string)typeof(DayTimeMoneyBox).GetField("_amString", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            string _pmString = (string)typeof(DayTimeMoneyBox).GetField("_pmString", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            int questNotificationTimer = (int)typeof(DayTimeMoneyBox).GetField("questNotificationTimer", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            string questPingString = (string)typeof(DayTimeMoneyBox).GetField("questPingString", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            Texture2D questPingTexture = (Texture2D)typeof(DayTimeMoneyBox).GetField("questPingTexture", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            Rectangle questPingSourceRect = (Rectangle)typeof(DayTimeMoneyBox).GetField("questPingSourceRect", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            int goldCoinTimer = (int)typeof(DayTimeMoneyBox).GetField("goldCoinTimer", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            string goldCoinString = (string)typeof(DayTimeMoneyBox).GetField("goldCoinString", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);

            // Update and time text
            _padZeros.Clear();
            if (Game1.timeOfDay % 100 == 0)
            {
                _padZeros.Append("0");
            }
            _hours.Clear();
            switch (LocalizedContentManager.CurrentLanguageCode)
            {
                case LocalizedContentManager.LanguageCode.ru:
                case LocalizedContentManager.LanguageCode.zh:
                case LocalizedContentManager.LanguageCode.pt:
                case LocalizedContentManager.LanguageCode.es:
                case LocalizedContentManager.LanguageCode.de:
                case LocalizedContentManager.LanguageCode.th:
                case LocalizedContentManager.LanguageCode.fr:
                case LocalizedContentManager.LanguageCode.tr:
                case LocalizedContentManager.LanguageCode.hu:
                    _temp.Clear();
                    _temp.AppendEx(Game1.timeOfDay / 100 % 24);
                    if (Game1.timeOfDay / 100 % 24 <= 9)
                    {
                        _hours.Append("0");
                    }
                    _hours.AppendEx(_temp);
                    break;
                default:
                    if (Game1.timeOfDay / 100 % 12 == 0)
                    {
                        if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ja)
                        {
                            _hours.Append("0");
                        }
                        else
                        {
                            _hours.Append("12");
                        }
                    }
                    else
                    {
                        _hours.AppendEx(Game1.timeOfDay / 100 % 12);
                    }
                    break;
            }
            _timeText.Clear();
            _timeText.AppendEx(_hours);
            _timeText.Append(":");
            _timeText.AppendEx(Game1.timeOfDay % 100);
            _timeText.AppendEx(_padZeros);
            switch (LocalizedContentManager.CurrentLanguageCode)
            {
                case LocalizedContentManager.LanguageCode.en:
                case LocalizedContentManager.LanguageCode.it:
                    _timeText.Append(" ");
                    if (Game1.timeOfDay < 1200 || Game1.timeOfDay >= 2400)
                    {
                        _timeText.Append(_amString);
                    }
                    else
                    {
                        _timeText.Append(_pmString);
                    }
                    break;
                case LocalizedContentManager.LanguageCode.ko:
                    if (Game1.timeOfDay < 1200 || Game1.timeOfDay >= 2400)
                    {
                        _timeText.Append(_amString);
                    }
                    else
                    {
                        _timeText.Append(_pmString);
                    }
                    break;
                case LocalizedContentManager.LanguageCode.ja:
                    _temp.Clear();
                    _temp.AppendEx(_timeText);
                    _timeText.Clear();
                    if (Game1.timeOfDay < 1200 || Game1.timeOfDay >= 2400)
                    {
                        _timeText.Append(_amString);
                        _timeText.Append(" ");
                        _timeText.AppendEx(_temp);
                    }
                    else
                    {
                        _timeText.Append(_pmString);
                        _timeText.Append(" ");
                        _timeText.AppendEx(_temp);
                    }
                    break;
                case LocalizedContentManager.LanguageCode.mod:
                    _timeText.Clear();
                    _timeText.Append(LocalizedContentManager.FormatTimeString(Game1.timeOfDay, LocalizedContentManager.CurrentModLanguage.ClockTimeFormat));
                    break;
            }

            // Draw the time
            Vector2 txtSize = font.MeasureString(_timeText);
            Vector2 timePosition = new Vector2((float)sourceRect.X * 0.55f - txtSize.X / 2f + (float)((__instance.timeShakeTimer > 0) ? Game1.random.Next(-2, 3) : 0), (float)sourceRect.Y * (LocalizedContentManager.CurrentLanguageLatin ? 0.31f : 0.31f) - txtSize.Y / 2f + (float)((__instance.timeShakeTimer > 0) ? Game1.random.Next(-2, 3) : 0));
            bool nofade = Game1.shouldTimePass() || Game1.fadeToBlack || Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 2000.0 > 1000.0;
            Utility.drawTextWithShadow(b, _timeText, font, __instance.position + timePosition, (Game1.timeOfDay >= 2400) ? Color.Red : (Game1.textColor * (nofade ? 1f : 0.5f)));
            int adjustedTime = (int)((float)(Game1.timeOfDay - Game1.timeOfDay % 100) + (float)(Game1.timeOfDay % 100 / 10) * 16.66f);


            // Draw quest related elements
            if (Game1.player.hasVisibleQuests)
            {
                __instance.questButton.draw(b);
                if (__instance.questPulseTimer > 0)
                {
                    float scaleMult = 1f / (Math.Max(300f, Math.Abs(__instance.questPulseTimer % 1000 - 500)) / 500f);
                    b.Draw(Game1.mouseCursors, new Vector2(__instance.questButton.bounds.X + 24, __instance.questButton.bounds.Y + 32) + ((scaleMult > 1f) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), new Rectangle(395, 497, 3, 8), Color.White, 0f, new Vector2(2f, 4f), 4f * scaleMult, SpriteEffects.None, 0.99f);
                }
                if (__instance.questPingTimer > 0)
                {
                    b.Draw(Game1.mouseCursors, new Vector2(Game1.dayTimeMoneyBox.questButton.bounds.Left - 16, Game1.dayTimeMoneyBox.questButton.bounds.Bottom + 8), new Rectangle(128 + ((__instance.questPingTimer / 200 % 2 != 0) ? 16 : 0), 208, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
                }
            }


            // Draw zoom buttons if enabled
            if (Game1.options.zoomButtons)
            {
                __instance.zoomInButton.draw(b, Color.White * ((Game1.options.desiredBaseZoomLevel >= 2f) ? 0.5f : 1f), 1f);
                __instance.zoomOutButton.draw(b, Color.White * ((Game1.options.desiredBaseZoomLevel <= 0.75f) ? 0.5f : 1f), 1f);
            }

            // Draw money box
            __instance.drawMoneyBox(b);
            if (_hoverText.Length > 0 && __instance.isWithinBounds(Game1.getOldMouseX(), Game1.getOldMouseY()))
            {
                IClickableMenu.drawHoverText(b, _hoverText, Game1.dialogueFont);
            }



            // Draws the clock hand
            b.Draw(Game1.mouseCursors, __instance.position + new Vector2(88f, 88f), new Rectangle(324, 477, 7, 19), Color.White, (float)(Math.PI + Math.Min(Math.PI, (double)(((float)adjustedTime + (float)Game1.gameTimeInterval / (float)Game1.realMilliSecondsPerGameTenMinutes * 16.6f - 600f) / 2000f) * Math.PI)), new Vector2(3f, 17f), 4f, SpriteEffects.None, 0.9f);
            if (questNotificationTimer > 0)
            {
                Vector2 basePosition = __instance.position + new Vector2(27f, 76f) * 4f;
                b.Draw(Game1.mouseCursors_1_6, basePosition, new Rectangle(257, 228, 39, 18), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
                b.Draw(questPingTexture, basePosition + new Vector2(1f, 1f) * 4f, questPingSourceRect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.91f);
                if (questPingString != null)
                {
                    Utility.drawTextWithShadow(b, questPingString, Game1.smallFont, basePosition + new Vector2(27f, 9.5f) * 4f - Game1.smallFont.MeasureString(questPingString) * 0.5f, Game1.textColor);
                }
                else
                {
                    b.Draw(Game1.mouseCursors_1_6, basePosition + new Vector2(22f, 5f) * 4f, new Rectangle(297, 229, 9, 8), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.91f);
                }
            }



            // Draw gold coin text bubble
            if (goldCoinTimer > 0)
            {
                SpriteText.drawSmallTextBubble(b, goldCoinString, __instance.position + new Vector2(5f, 73f) * 4f, -1, 0.99f, drawPointerOnTop: true);
            }
            return false;
        }

        private static bool DrawToolbarPrefix(Toolbar __instance, SpriteBatch b)
        {
            if (!ModEntry.modConfig.EnableMod || !ModEntry.modConfig.ToggleHudKey.IsDown()) return true;
            if (Game1.activeClickableMenu != null) return false;

            float scaleX = 0.5f;
            float scaleY = 0.5f;

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
            //if (num != __instance.yPositionOnScreen)
            for (int k = 0; k < 12; k++)
            {
                __instance.buttons[k].bounds.Y = (int)desiredY;
                __instance.buttons[k].bounds.X = (int)(startX + k * 64 * scaleX);

                __instance.buttons[k].bounds.Width = (int)(64 * scaleX);
                __instance.buttons[k].bounds.Height = (int)(64 * scaleY);

                Rectangle buttonBounds = __instance.buttons[k].bounds;
                b.Draw(Game1.staminaRect, new Rectangle(buttonBounds.X, buttonBounds.Y, buttonBounds.Width, buttonBounds.Height), new Color(255, 0, 0, 100));
            }

            IClickableMenu.drawTextureBox(
                b,
                Game1.menuTexture,
                __instance.toolbarTextSource,
                adjustedX,
                (int)desiredY - 8,
                (int)(800 * scaleX),
                (int)(96 * scaleY),
                Color.White * __instance.transparency,
                scaleX,
                drawShadow: false
            );

            for (int j = 0; j < 12; j++)
            {
                Vector2 buttonPosition = new Vector2(startX + j * 64 * scaleX, desiredY);

                // Calculate the position of the item centered inside the button
                Vector2 itemPosition = buttonPosition + new Vector2((64 * scaleX - 64) / 2, (64 * scaleY - 64) / 2);
                float itemScale = Math.Min(scaleX, scaleY);

                // Calculate the position of the quality indicator
                Vector2 qualityPosition = buttonPosition + new Vector2(8 * scaleX, 52 * scaleY);


                if (Game1.player.Items.Count > j && Game1.player.Items[j] != null)
                {
                    if (Game1.player.Items[j].Category == -96) // If the item is a ring
                    {
                        // Adjust item position for rings
                        itemPosition += new Vector2(16 * itemScale, 24 * itemScale);
                    }
                    Game1.player.Items[j].drawInMenu(b, 
                        itemPosition, 
                        (Game1.player.CurrentToolIndex == j) ? (itemScale * 0.9f) : (itemScale * 0.8f), 
                        __instance.transparency, 
                        0.88f, 
                        StackDrawType.Hide);
                }

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
                    if (Game1.player.Items[j].Quality > 0)
                    {
                        Rectangle qualityRect = (((int)Game1.player.Items[j].Quality < 4) ? new Rectangle(338 + ((int)Game1.player.Items[j].Quality - 1) * 8, 400, 8, 8) : new Rectangle(346, 392, 8, 8));
                        Texture2D qualitySheet = Game1.mouseCursors;
                        float yOffset = (((int)Game1.player.Items[j].Quality < 4) ? 0f : (((float)Math.Cos((double)Game1.currentGameTime.TotalGameTime.Milliseconds * Math.PI / 512.0) + 1f) * 0.05f));
                        b.Draw(qualitySheet, qualityPosition, qualityRect, Color.White * __instance.transparency, 0f,
                            new Vector2(4f, 4f), 3f * itemScale * (1f + yOffset), SpriteEffects.None, 0.88f);
                    }

                    int drawnStack = Game1.player.Items[j].Stack;
                    bool shouldDrawStackNumber = ((Game1.player.Items[j].maximumStackSize() > 1 && drawnStack > 1)) && (double)itemScale > 0.3 && drawnStack != int.MaxValue;
                    if (Game1.player.Items[j].IsRecipe)
                    {
                        shouldDrawStackNumber = false;
                    }
                    if (shouldDrawStackNumber)
                    {
                        float scale_size = (Game1.player.CurrentToolIndex == j) ? (itemScale * 0.9f) : (itemScale * 0.8f);
                        Vector2 stackPosition = (itemPosition + new Vector2((float)((64 + (widthDifference / 12)) - Utility.getWidthOfTinyDigitString(drawnStack, 3f * scale_size)) + 3f * scale_size, (64f + heightDifference) - 18 * scale_size + 1f) * itemScale);

                        Utility.drawTinyDigits(drawnStack, b, stackPosition, 3f * scale_size, Math.Min(1f, 0.88f + 1E-06f), Color.White * __instance.transparency);
                    }
                }


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
