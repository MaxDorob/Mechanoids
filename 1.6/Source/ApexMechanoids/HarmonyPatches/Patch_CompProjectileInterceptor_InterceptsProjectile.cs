using HarmonyLib;
using RimWorld;
using Verse;

namespace ApexMechanoids
{
    [HarmonyPatch(typeof(CompProjectileInterceptor), nameof(CompProjectileInterceptor.InterceptsProjectile))]
    internal static class Patch_CompProjectileInterceptor_InterceptsProjectile_AnyShieldStopsMarkedProjectiles
    {
        private static void Postfix(CompProperties_ProjectileInterceptor props, Projectile projectile, ref bool __result)
        {
            if (__result || props == null || projectile?.def == null)
            {
                return;
            }

            if (!props.interceptGroundProjectiles && !props.interceptAirProjectiles)
            {
                return;
            }

            __result = projectile.def.HasModExtension<DefModExtension_InterceptedByAnyShield>();
        }
    }
}
