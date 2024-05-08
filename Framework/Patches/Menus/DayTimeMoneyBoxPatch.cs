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


        public const int digitHeight = 8;

        public static int numDigits = 8;

        public static int currentValue;

        public static int previousTargetValue;

        public static TemporaryAnimatedSpriteList animations = new TemporaryAnimatedSpriteList();

        private static int speed;

        private static int soundTimer;

        private static int moneyMadeAccumulator;

        private static int moneyShineTimer;

        private static bool playSounds = true;

        public static Action<int> onPlaySound;

        public static bool ShouldShakeMainMoneyBox = true;

        internal DayTimeMoneyBoxPatch(Harmony harmony) : base(harmony, typeof(DayTimeMoneyBox)) 
        {
            onPlaySound = playDefaultSound;
        }

        internal void Apply()
        {
            Patch(PatchType.Prefix, nameof(DayTimeMoneyBox.draw), nameof(DrawPrefix), [typeof(SpriteBatch)]);
            Patch(PatchType.Prefix, "updatePosition", nameof(UpdatePositionPrefix));
        }

        private static bool DrawPrefix(DayTimeMoneyBox __instance, SpriteBatch b)
        {
            if (!ModEntry.modConfig.EnableMod) return true;
            if (ModEntry.modConfig.DisableDTMBoxHUD) return false;

            Vector2 scale = new Vector2(ModEntry.modConfig.DTMBoxScale);


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





            b.Draw(Game1.mouseCursors, __instance.position, sourceRect, Color.White, 0f, Vector2.Zero, 4f * scale.X, SpriteEffects.None, 0.9f);

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
            Vector2 dayPosition = new Vector2((sourceRect.X * 0.5625f - daySize.X / 2f) * scale.X, (sourceRect.Y * (LocalizedContentManager.CurrentLanguageLatin ? 0.1f : 0.1f) - daySize.Y / 2f) * scale.Y);
            drawScaledTextWithShadow(b, _dateText, font, __instance.position + dayPosition, Game1.textColor, 1f * scale.X);
            //Utility.drawBoldText(b, _dateText.ToString(), font, __instance.position + dayPosition, Game1.textColor, 1f * scale.X);

            // Draw weather icon
            b.Draw(Game1.mouseCursors, __instance.position + new Vector2(212f * scale.X, 68f * scale.Y), new Rectangle(406, 441 + Game1.seasonIndex * 8, 12, 8), Color.White, 0f, Vector2.Zero, 4f * scale.X, SpriteEffects.None, 0.9f);
            if (Game1.weatherIcon == 999)
            {
                b.Draw(Game1.mouseCursors_1_6, __instance.position + new Vector2(116f * scale.X, 68f * scale.Y), new Rectangle(243, 293, 12, 8), Color.White, 0f, Vector2.Zero, 4f * scale.X, SpriteEffects.None, 0.9f);
            }
            else
            {
                b.Draw(Game1.mouseCursors, __instance.position + new Vector2(116f * scale.X, 68f * scale.Y), new Rectangle(317 + 12 * Game1.weatherIcon, 421, 12, 8), Color.White, 0f, Vector2.Zero, 4f * scale.X, SpriteEffects.None, 0.9f);
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
            drawScaledTextWithShadow(b, _timeText, font, __instance.position + (timePosition * scale.X), (Game1.timeOfDay >= 2400) ? Color.Red : (Game1.textColor * (nofade ? 1f : 0.5f)), 1 * scale.X);
            int adjustedTime = (int)((float)(Game1.timeOfDay - Game1.timeOfDay % 100) + (float)(Game1.timeOfDay % 100 / 10) * 16.66f);


            // Draw quest related elements
            if (Game1.player.hasVisibleQuests)
            {
                __instance.questButton.scale = 4f * scale.X;
                __instance.questButton.setPosition(new Vector2(__instance.questButton.bounds.X, __instance.questButton.bounds.Y));
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
                __instance.zoomInButton.scale = 4f * scale.X;
                __instance.zoomInButton.draw(b, Color.White * ((Game1.options.desiredBaseZoomLevel >= 2f) ? 0.5f : 1f), 1f);


                //__instance.zoomOutButton.bounds.Y = (int)(__instance.zoomOutButton.bounds.Y * scale.Y);
                __instance.zoomOutButton.scale = 4f * scale.X;
                __instance.zoomOutButton.draw(b, Color.White * ((Game1.options.desiredBaseZoomLevel <= 0.75f) ? 0.5f : 1f), 1f);
            }

            // drawMoneyBox

            updatePosition.Invoke(__instance, null);

            b.Draw(Game1.mouseCursors, __instance.position + (new Vector2(28 + ((__instance.moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0), 172 + ((__instance.moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0) ) * scale.X), new Rectangle(340, 472, 65, 17), Color.White, 0f, Vector2.Zero, 4f * scale.X, SpriteEffects.None, 0.9f);



            drawBox(b, __instance.position + new Vector2((68 * scale.X) + ((__instance.moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0), (196 * scale.Y) + ((__instance.moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0)), Game1.player.Money, scale.X);
            if (__instance.moneyShakeTimer > 0)
            {
                __instance.moneyShakeTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
            }

            /*            int test = (int)(__instance.position.X - (__instance.position.X * scale.X));
                        int x = (int)(__instance.position.X - test);

                        int test2 = (int)(__instance.position.Y - (__instance.position.Y * scale.Y));
                        int y = (int)(__instance.position.Y - test2);
                        Vector2 drawPosition = new Vector2(x, y);

                        int shakeX = (__instance.moneyShakeTimer > 0) ? Game1.random.Next((int)(-3 * scale.X), (int)(4 * scale.X)) : 0;
                        int shakeY = (__instance.moneyShakeTimer > 0) ? Game1.random.Next((int)(-3 * scale.X), (int)(4 * scale.X)) : 0;

                        b.Draw(Game1.mouseCursors, drawPosition + new Vector2(28 + shakeX, 172 + shakeY), new Rectangle(340, 472, 65, 17), Color.White, 0f, Vector2.Zero, 4f * scale.X, SpriteEffects.None, 0.9f);*/

            // TODO FIX
            //__instance.moneyDial.draw(b, drawPosition + new Vector2(68 + shakeX, 196 + shakeY), Game1.player.Money);

            if (__instance.moneyShakeTimer > 0)
            {
                __instance.moneyShakeTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
            }

            if (_hoverText.Length > 0 && __instance.isWithinBounds(Game1.getOldMouseX(), Game1.getOldMouseY()))
            {
                IClickableMenu.drawHoverText(b, _hoverText, Game1.dialogueFont);
            }



            // Draws the clock hand
            b.Draw(Game1.mouseCursors, __instance.position + new Vector2(88f * scale.X, 88f * scale.X), new Rectangle(324, 477, 7, 19), Color.White, (float)(Math.PI + Math.Min(Math.PI, (double)(((float)adjustedTime + (float)Game1.gameTimeInterval / (float)Game1.realMilliSecondsPerGameTenMinutes * 16.6f - 600f) / 2000f) * Math.PI)), new Vector2(3f, 17f), 4f * scale.X, SpriteEffects.None, 0.9f);



            if (questNotificationTimer > 0)
            {
                Vector2 basePosition = __instance.position + new Vector2(27f, 76f) * 4f;
                b.Draw(Game1.mouseCursors_1_6, basePosition, new Rectangle(257, 228, 39, 18), Color.White, 0f, Vector2.Zero, 4f * scale.X, SpriteEffects.None, 0.9f);
                b.Draw(questPingTexture, basePosition + new Vector2(1f, 1f) * 4f, questPingSourceRect, Color.White, 0f, Vector2.Zero, 4f * scale.X, SpriteEffects.None, 0.91f);
                if (questPingString != null)
                {
                    drawScaledTextWithShadow(b, questPingString, Game1.smallFont, basePosition + new Vector2(27f, 9.5f) * 4f - Game1.smallFont.MeasureString(questPingString) * 0.5f, Game1.textColor, 1 * scale.X);
                }
                else
                {
                    b.Draw(Game1.mouseCursors_1_6, basePosition + new Vector2(22f, 5f) * 4f, new Rectangle(297, 229, 9, 8), Color.White, 0f, Vector2.Zero, 4f * scale.X, SpriteEffects.None, 0.91f);
                }
            }



            // Draw gold coin text bubble
            if (goldCoinTimer > 0)
            {
                drawScaledSmallTextBubble(b, goldCoinString, __instance.position + new Vector2(5f, 73f) * 4f, -1, 0.99f, drawPointerOnTop: true, scale: 1 * scale.X);
            }
            return false;
        }

        private static bool UpdatePositionPrefix(DayTimeMoneyBox __instance)
        {
            if (!ModEntry.modConfig.EnableMod) return true;

            Vector2 scale = new Vector2(ModEntry.modConfig.DTMBoxScale);

            // Scale width difference
            float widthDifference = (300 - (300 * scale.X));

            // Scale position
            __instance.position = new Vector2(Game1.uiViewport.Width - 300 + widthDifference, 8f * scale.Y);

            // Scale position for outdoor map
            if (Game1.isOutdoorMapSmallerThanViewport())
            {
                __instance.position = new Vector2(Math.Min(__instance.position.X, -Game1.uiViewport.X + Game1.currentLocation.map.Layers[0].LayerWidth * 64 - 300 * scale.X), 8f * scale.Y);
            }

            // Scale safe area
            Utility.makeSafe(ref __instance.position, (int)(300 - widthDifference), 284);

            // Scale x position on screen
            __instance.xPositionOnScreen = (int)(__instance.position.X);

            // Scale y position on screen
            __instance.yPositionOnScreen = (int)(__instance.position.Y);

            // Scale quest button bounds
            __instance.questButton.bounds = new Rectangle(__instance.xPositionOnScreen + (int)(212 * scale.X), __instance.yPositionOnScreen + (int)(240 * scale.Y), (int)(44 * scale.X), (int)(46 * scale.Y));

            // Scale zoom out button bounds
            __instance.zoomOutButton.bounds = new Rectangle(__instance.xPositionOnScreen + (int)(92 * scale.X), __instance.yPositionOnScreen + (int)(244 * scale.Y), (int)(28 * scale.X), (int)(32 * scale.Y));

            // Scale zoom in button bounds
            __instance.zoomInButton.bounds = new Rectangle(__instance.xPositionOnScreen + (int)(124 * scale.X), __instance.yPositionOnScreen + (int)(244 * scale.Y), (int)(28 * scale.X), (int)(32 * scale.Y));

            return false;
        }




        public static void drawScaledTextWithShadow(SpriteBatch b, string text, SpriteFont font, Vector2 position, Color color, float scale = 1f, float layerDepth = -1f, int horizontalShadowOffset = -1, int verticalShadowOffset = -1, float shadowIntensity = 1f, int numShadows = 3)
        {
            if (layerDepth == -1f)
            {
                layerDepth = position.Y / 10000f;
            }

            bool flag = Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.ru || Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.de || Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.ko;
            if (horizontalShadowOffset == -1)
            {
                horizontalShadowOffset = ((font.Equals(Game1.smallFont) || flag) ? (int)(-2 * scale) : (int)(-3 * scale));
            }

            if (verticalShadowOffset == -1)
            {
                verticalShadowOffset = ((font.Equals(Game1.smallFont) || flag) ? (int)(2 * scale) : (int)(3 * scale));
            }

            if (text == null)
            {
                text = "";
            }

            b.DrawString(font, text, position + new Vector2(horizontalShadowOffset, verticalShadowOffset), Game1.textShadowDarkerColor * shadowIntensity, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth - 0.0001f);
            switch (numShadows)
            {
                case 2:
                    b.DrawString(font, text, position + new Vector2(horizontalShadowOffset, 0f), Game1.textShadowDarkerColor * shadowIntensity, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth - 0.0002f);
                    break;
                case 3:
                    b.DrawString(font, text, position + new Vector2(0f, verticalShadowOffset), Game1.textShadowDarkerColor * shadowIntensity, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth - 0.0003f);
                    break;
            }

            b.DrawString(font, text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
        }

        public static void drawScaledTextWithShadow(SpriteBatch b, StringBuilder text, SpriteFont font, Vector2 position, Color color, float scale = 1f, float layerDepth = -1f, int horizontalShadowOffset = -1, int verticalShadowOffset = -1, float shadowIntensity = 1f, int numShadows = 3)
        {
            if (layerDepth == -1f)
            {
                layerDepth = position.Y / 10000f;
            }

            bool flag = Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.ru || Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.de;
            if (horizontalShadowOffset == -1)
            {
                horizontalShadowOffset = ((font.Equals(Game1.smallFont) || flag) ? (int)(-2 * scale) : (int)(-3 * scale));
            }

            if (verticalShadowOffset == -1)
            {
                verticalShadowOffset = ((font.Equals(Game1.smallFont) || flag) ? (int)(2 * scale) : (int)(3 * scale));
            }

            if (text == null)
            {
                throw new ArgumentNullException("text");
            }

            b.DrawString(font, text, position + new Vector2(horizontalShadowOffset, verticalShadowOffset), Game1.textShadowDarkerColor * shadowIntensity, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth - 0.0001f);
            switch (numShadows)
            {
                case 2:
                    b.DrawString(font, text, position + new Vector2(horizontalShadowOffset, 0f), Game1.textShadowDarkerColor * shadowIntensity, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth - 0.0002f);
                    break;
                case 3:
                    b.DrawString(font, text, position + new Vector2(0f, verticalShadowOffset), Game1.textShadowDarkerColor * shadowIntensity, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth - 0.0003f);
                    break;
            }

            b.DrawString(font, text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
        }

        public static void drawScaledSmallTextBubble(SpriteBatch b, string s, Vector2 positionOfBottomCenter, int maxWidth = -1, float layerDepth = -1f, float scale = 1f, bool drawPointerOnTop = false)
        {
            if (maxWidth != -1)
            {
                s = Game1.parseText(s, Game1.smallFont, maxWidth - 16);
            }

            s = s.Trim();
            Vector2 vector = Game1.smallFont.MeasureString(s);
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors_1_6, new Rectangle(241, 503, 9, 9), (int)(positionOfBottomCenter.X - vector.X / 2f - 4f), (int)(positionOfBottomCenter.Y - vector.Y), (int)vector.X + 16, (int)vector.Y + 12, Color.White, 4f * scale, drawShadow: false, layerDepth);
            if (drawPointerOnTop)
            {
                b.Draw(Game1.mouseCursors_1_6, positionOfBottomCenter + new Vector2(-4f, -3f) * 4f + new Vector2(vector.X / 2f, 0f - vector.Y), new Rectangle(251, 506, 5, 5), Color.White, 0f, Vector2.Zero, 4f * scale, SpriteEffects.FlipVertically, layerDepth + 1E-05f);
            }
            else
            {
                b.Draw(Game1.mouseCursors_1_6, positionOfBottomCenter + new Vector2(-2.5f, 1f) * 4f, new Rectangle(251, 506, 5, 5), Color.White, 0f, Vector2.Zero, 4f * scale, SpriteEffects.None, layerDepth + 1E-05f);
            }

            drawScaledTextWithShadow(b, s, Game1.smallFont, positionOfBottomCenter - vector + new Vector2(4f + vector.X / 2f, 8f), Game1.textColor, 1f * scale, layerDepth + 2E-05f, -1, -1, 0.5f);
        }


        public void playDefaultSound(int direction)
        {
            if (direction > 0)
            {
                Game1.playSound("moneyDial");
            }
        }

        public static void drawBox(SpriteBatch b, Vector2 position, int target, float scale)
        {
            if (previousTargetValue != target)
            {
                speed = (target - currentValue) / 100;
                previousTargetValue = target;
                soundTimer = Math.Max(6, 100 / (Math.Abs(speed) + 1));
            }
            if (moneyShineTimer > 0 && currentValue == target)
            {
                moneyShineTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
            }
            if (moneyMadeAccumulator > 0)
            {
                moneyMadeAccumulator -= (Math.Abs(speed / 2) + 1) * ((animations.Count > 0) ? 1 : 100);
                if (moneyMadeAccumulator <= 0)
                {
                    moneyShineTimer = numDigits * 60;
                }
            }
            if (ShouldShakeMainMoneyBox && moneyMadeAccumulator > 2000)
            {
                Game1.dayTimeMoneyBox.moneyShakeTimer = 100;
            }
            if (currentValue != target)
            {
                currentValue += speed + ((currentValue < target) ? 1 : (-1));
                if (currentValue < target)
                {
                    moneyMadeAccumulator += Math.Abs(speed);
                }
                soundTimer--;
                if (Math.Abs(target - currentValue) <= speed + 1 || (speed != 0 && Math.Sign(target - currentValue) != Math.Sign(speed)))
                {
                    currentValue = target;
                }
                if (soundTimer <= 0)
                {
                    if (playSounds)
                    {
                        onPlaySound?.Invoke(Math.Sign(target - currentValue));
                    }
                    soundTimer = Math.Max(6, 100 / (Math.Abs(speed) + 1));
                    if (Game1.random.NextDouble() < 0.4)
                    {
                        if (target > currentValue)
                        {
                            animations.Add(TemporaryAnimatedSprite.GetTemporaryAnimatedSprite(Game1.random.Next(10, 12), position + new Vector2(Game1.random.Next(30, 190), Game1.random.Next(-32, 48)), Color.Gold));
                        }
                        else if (target < currentValue)
                        {
                            TemporaryAnimatedSprite sprite = TemporaryAnimatedSprite.GetTemporaryAnimatedSprite(
                                "LooseSprites\\Cursors", 
                                new Rectangle((int)(356 * scale), (int)(449 * scale), 1, 1), 
                                999999f, 
                                1, 
                                44, 
                                (position + new Vector2(Game1.random.Next((int)(160 * scale)), Game1.random.Next((int)(-32 * scale), (int)(32 * scale)))), 
                                flicker: false, 
                                flipped: false, 
                                1f, 
                                0.01f, 
                                Color.White, 
                                Game1.random.Next(1, 3) * 4 * scale, -0.001f, 0f, 0f);
                            sprite.motion = new Vector2((float)Game1.random.Next(-30, 40) / 10f, (float)Game1.random.Next(-30, -5) / 10f);
                            sprite.acceleration = new Vector2(0f, 0.25f);
                            animations.Add(sprite);
                        }
                    }
                }
            }
            for (int j = animations.Count - 1; j >= 0; j--)
            {
                if (animations[j].update(Game1.currentGameTime))
                {
                    animations.RemoveAt(j);
                }
                else
                {
                    //animations[j].position *= scale;
                    animations[j].draw(b, localPosition: true);
                }
            }

            int xPosition = (int)(position.X - 24 * scale) + (int)(24 * scale);
            int digitStrip = (int)Math.Pow(10.0, numDigits - 1);
            bool significant = false;

            for (int i = 0; i < numDigits; i++)
            {
                int currentDigit = currentValue / digitStrip % 10;
                if (currentDigit > 0 || i == numDigits - 1)
                {
                    significant = true;
                }
                if (significant)
                {
                    float yOffset = (Game1.activeClickableMenu is ShippingMenu && currentValue >= 1000000) ? (float)(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 100.53096771240234 + i) * (currentValue / 1000000) * scale) : 0f;

                    Vector2 digitPosition = new Vector2(xPosition, position.Y + yOffset);

                    b.Draw(Game1.mouseCursors, digitPosition, new Rectangle(286, 502 - currentDigit * 8, 5, 8), Color.Maroon, 0f, Vector2.Zero, 4f * scale + ((moneyShineTimer / 60 == numDigits - i) ? 0.3f : 0f), SpriteEffects.None, 1f);
                }
                xPosition += (int)(24 * scale);
                digitStrip /= 10;
            }
        }
    }
}
