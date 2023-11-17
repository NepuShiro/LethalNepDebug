
using HarmonyLib;
using UnityEngine;
using GameNetcodeStuff;

namespace LethalNepDebug.Patches
{
    class LethalNepDebug_Patches
    {
        [HarmonyPatch(typeof(PlayerControllerB), "KillPlayer")]
        public static class NoKillPatch
        {
            public static bool Prefix()
            {
                if (Plugin.toggles["NoKill"])
                {
                    //LoggerManager.LogToggle("NoKill");
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), "DamagePlayer")]
        public static class InfiniteHealthPatch
        {
            public static void Prefix(ref PlayerControllerB __instance, ref int damageNumber)
            {
                if (Plugin.toggles["NoHurty"])
                {
                    //LoggerManager.LogToggle("NoHurty");
                    damageNumber = Mathf.Max(100 - __instance.health, 0);
                }
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), "LateUpdate")]
        public static class InfiniteStaminaPatch
        {
            public static void Prefix(ref PlayerControllerB __instance)
            {
                if (Plugin.toggles["Speed"])
                {
                    //LoggerManager.LogToggle("Speed");
                    __instance.sprintMeter = 1f;
                }

                if (Plugin.toggles["Heavy"])
                {
                    //LoggerManager.LogToggle("Heavy");
                    __instance.carryWeight = 1;
                }
            }
        }

        [HarmonyPatch(typeof(TimeOfDay), "Update")]
        public static class QuotaFullfilledPatch
        {
            public static void Prefix(ref TimeOfDay __instance)
            {
                if (Plugin.toggles["Quota"])
                {
                    //LoggerManager.LogToggle("Quota");
                    Plugin.QuotaToggledOnce = true;
                    __instance.quotaFulfilled = __instance.profitQuota;
                    __instance.UpdateProfitQuotaCurrentTime();
                }
                if (!Plugin.toggles["Quota"] && Plugin.QuotaToggledOnce)
                {
                    //LoggerManager.LogToggle("Quota");
                    __instance.quotaFulfilled -= __instance.profitQuota;
                    __instance.UpdateProfitQuotaCurrentTime();
                    Plugin.QuotaToggledOnce = false;
                }
            }
        }

        [HarmonyPatch(typeof(GrabbableObject), "Update")]
        public static class InfiniteBatteryPatch
        {
            public static void Postfix(ref GrabbableObject __instance)
            {
                if (Plugin.toggles["Charge"])
                {
                    //LoggerManager.LogToggle("Charge");
                    if (__instance.isBeingUsed && __instance.itemProperties.requiresBattery)
                    {
                        if (__instance.insertedBattery.charge > 0f)
                        {
                            if (!__instance.itemProperties.itemIsTrigger)
                            {
                                // this.insertedBattery.charge -= Time.deltaTime / this.itemProperties.batteryUsage;
                                __instance.insertedBattery.charge = 1f;
                            }
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(MenuManager), "Update")]
        public static class MenuManagerUpdatePatch
        {
            public static void Prefix(MenuManager __instance)
            {
                if (Plugin.toggles["VersionSpoof"] && GameNetworkManager.Instance != null && !Plugin.VersionToggledOnce)
                {
                    //LoggerManager.LogToggle("VersionSpoof");
                    Plugin.VersionToggledOnce = true;
                    Plugin.Version = GameNetworkManager.Instance.gameVersionNum;
                    GameNetworkManager.Instance.gameVersionNum += 16440;
                    if (__instance != null && __instance.versionNumberText != null)
                    {
                        __instance.versionNumberText.text = $"v{GameNetworkManager.Instance.gameVersionNum - 16440}\n[NEP]";
                    }
                }
                if (!Plugin.toggles["VersionSpoof"] && GameNetworkManager.Instance != null && Plugin.VersionToggledOnce)
                {
                    //LoggerManager.LogToggle("VersionSpoof");
                    Plugin.VersionToggledOnce = false;
                    GameNetworkManager.Instance.gameVersionNum = Plugin.Version;
                    if (__instance != null && __instance.versionNumberText != null)
                    {
                        __instance.versionNumberText.text = $"v{Plugin.Version}";
                    }
                }
            }
        }
    }
}
