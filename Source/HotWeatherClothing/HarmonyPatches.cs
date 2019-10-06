using Harmony;
using RimWorld;
using System.Reflection;
using Verse;


namespace EquipHotWeatherClothing
{
    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {
        // this static constructor runs to create a HarmonyInstance and install a patch.
        static HarmonyPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.equiphotweatherclothing");

            // find the ApparelScoreRaw method of the class RimWorld.JobGiver_OptimizeApparel
            MethodInfo targetmethod = AccessTools.Method(typeof(RimWorld.JobGiver_OptimizeApparel), "ApparelScoreRaw");

            // find the static method to call before (i.e. Prefix) the targetmethod
            HarmonyMethod postfixmethod = new HarmonyMethod(typeof(EquipHotWeatherClothing.HarmonyPatches).GetMethod("ApparelScoreRaw_Postfix"));

            // patch the targetmethod, by calling (targetmethod, prefixmethod, postfixmethod) 
            // There is no prefixmethod, so it's null
            harmony.Patch(targetmethod, null, postfixmethod);

        }

        public static void ApparelScoreRaw_Postfix(Pawn pawn, Apparel ap, ref float __result)
        {
            Log.Message("Checking apparel");

            NeededWarmth neededCold = PawnApparelGenerator.CalculateNeededWarmth(pawn, pawn.Map.Tile, GenLocalDate.Twelfth(pawn));

            SimpleCurve InsulationHeatScoreFactorCurve_NeedCold = new SimpleCurve
            {
                {
                    new CurvePoint(0f, 1f),
                    true
                },
                {
                    new CurvePoint(30f, 8f),
                    true
                }
            };

            if (neededCold == NeededWarmth.Cool)
            {
                Log.Message("Cold is needed");
                float statValue = ap.GetStatValue(StatDefOf.Insulation_Heat, true);
                float coldFactor = 1f;
                coldFactor *= InsulationHeatScoreFactorCurve_NeedCold.Evaluate(statValue);

                __result *= coldFactor;
            }

        }


    }
}