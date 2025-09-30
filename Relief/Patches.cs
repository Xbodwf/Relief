using HarmonyLib;
using UnityEngine.SceneManagement;
using Relief.UI;

// TODO: Rename this namespace to your mod's name.
namespace Relief
{
    /// <summary>
    /// Add all of your <see cref="HarmonyPatch"/> classes here. If you find
    /// this file getting too large, you may want to consider separating the
    /// patches into several different classes.
    /// </summary>
    internal static class Patches
    {
        /// <summary>
        /// Example patch that logs anytime the user presses a key.
        /// </summary>
        [HarmonyPatch(typeof(scrController), "CountValidKeysPressed")]
        private static class KeyPatch
        {
            
            public static void Postfix(int __result) {
                //MainClass.Logger.Log($"User pressed {__result} key(s).");
            }
        }

        [HarmonyPatch(typeof(ADOBase), nameof(ADOBase.LoadScene))]
        private static class ScenePatch
        {
            
            private static void Postfix(string scene)
            {
                MainClass.Logger.Log($"Loading scene: {scene}");
                NotificationManager.Instance.ShowNotification($"Loading scene: {scene}");
            }
        }
    }
}
