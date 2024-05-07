using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;
using StardewValley;
using StardewValley.Menus;
using System.Reflection;
using System.Text;
using System;
using Microsoft.Xna.Framework;

namespace HUDCustomizer.Framework.Patches.Menus
{
    internal class DayTimeMoneyBoxPatch : PatchTemplate
    {
        internal DayTimeMoneyBoxPatch(Harmony harmony) : base(harmony, typeof(DayTimeMoneyBox)) { }

        internal void Apply()
        {
            //Patch(PatchType.Prefix, nameof(DayTimeMoneyBox.draw), nameof(DrawPrefix), [typeof(SpriteBatch)]);
        }

        private static bool DrawPrefix(DayTimeMoneyBox __instance, SpriteBatch b)
        {
            if (!ModEntry.modConfig.EnableMod) return true;
            if (ModEntry.modConfig.DisableDTMBoxHUD) return false;


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
    }
}
