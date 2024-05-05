﻿using HarmonyLib;
using StardewModdingAPI;
using System;


namespace HUDCustomizer.Framework.Patches
{
    internal class PatchTemplate
    {
        internal static Harmony _harmony;

        internal PatchTemplate(Harmony modHarmony)
        {
            _harmony = modHarmony;
        }

        /// <summary>
        /// Applies method patches using Harmony for a specified target method.
        /// </summary>
        /// <param name="patchType">The type of patch to apply: Prefix, Postfix, or Transpiler.</param>
        /// <param name="originalMethod">The name of the original method to patch.</param>
        /// <param name="newMethod">The name of the method to be applied as a patch.</param>
        /// <param name="parameters">Optional parameters for the method.</param>
        public void Patch(PatchType patchType, Type objectType, string originalMethod, string newMethod, Type[] parameters = null)
        {
            try
            {
                switch (patchType)
                {
                    case PatchType.Prefix:
                        _harmony.Patch(AccessTools.Method(objectType, originalMethod, parameters), prefix: new HarmonyMethod(GetType(), newMethod));
                        break;
                    case PatchType.Postfix:
                        _harmony.Patch(AccessTools.Method(objectType, originalMethod, parameters), postfix: new HarmonyMethod(GetType(), newMethod));
                        break;
                    case PatchType.Transpiler:
                        _harmony.Patch(AccessTools.Method(objectType, originalMethod, parameters), transpiler: new HarmonyMethod(GetType(), newMethod));
                        break;
                    default:
                        ModEntry.monitor.Log($"Unknown patch type: {patchType}", LogLevel.Error);
                        return;
                }
            }
            catch (Exception e)
            {
                string errorMessage = $"Issue with Harmony patching for method {originalMethod} with {newMethod}: {e}";
                ModEntry.monitor.Log(errorMessage, LogLevel.Error);
            }
        }

        public enum PatchType
        {
            Prefix,
            Postfix,
            Transpiler
        }
    }
}
