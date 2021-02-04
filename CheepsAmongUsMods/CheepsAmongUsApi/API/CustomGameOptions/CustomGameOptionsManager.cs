using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using GameOptionsMenu = PHCKLDDNJNP;
using Scroller = CIAACBCIDFI;
using NumberOption = PCGDGFIAJJI;
using ToggleOption = BCLDBBKFJPK;

namespace CheepsAmongUsApi.API.CustomGameOptions
{
    public class CustomGameOptionsManager
    {
        public static List<CustomNumberOption> AllCustomNumberOptions = new List<CustomNumberOption>();
        public static List<CustomToggleOption> AllCustomToggleOptions = new List<CustomToggleOption>();

        internal static int StringNameId = 1000;
        internal static int IndexId = 0;

        #region ------------------------------ Patch GameOptionsMenu ------------------------------
        [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Start))]
        public static class Patch_GameOptionsMenu_Start
        {
            public static void Postfix(GameOptionsMenu __instance)
            {
                foreach (var customNoOption in AllCustomNumberOptions)
                {
                    try
                    {
                        Scroller componentInParent = __instance.GetComponentInParent<Scroller>();

                        componentInParent.YBounds = new FloatRange(componentInParent.YBounds.min, 20f);
                        float num = -8.5f;
                        float num2 = -0.5f;

                        NumberOption option = UnityEngine.Object.Instantiate(__instance.GetComponentsInChildren<NumberOption>().First(), __instance.transform); // Instantiate new option

                        option.transform.localPosition = new Vector3(option.transform.localPosition.x, num + num2 * customNoOption.IndexId, option.transform.localPosition.z);  // Update position

                        option.Title = (StringNames)customNoOption.StringNameId;    // Set custom string name id
                        option.TitleText.Text = customNoOption.TitleText;   // Set the title text

                        option.Value = customNoOption.Value;    // Set the current value

                        option.gameObject.AddComponent<NumberOption>();
                        option.ValidRange = customNoOption.FloatRange;

                        customNoOption.GameNumberOption = option;
                    }
                    catch { }
                }

                foreach (var customToggleOption in AllCustomToggleOptions)
                {
                    try
                    {
                        Scroller componentInParent = __instance.GetComponentInParent<Scroller>();

                        componentInParent.YBounds = new FloatRange(componentInParent.YBounds.min, 20f);
                        float num = -8.5f;
                        float num2 = -0.5f;

                        ToggleOption option = UnityEngine.Object.Instantiate(__instance.GetComponentsInChildren<ToggleOption>().First(), __instance.transform); // Instantiate new option

                        option.transform.localPosition = new Vector3(option.transform.localPosition.x, num + num2 * customToggleOption.IndexId, option.transform.localPosition.z);  // Update position

                        option.Title = (StringNames)customToggleOption.StringNameId;    // Set custom string name id
                        option.TitleText.Text = customToggleOption.TitleText;   // Set the title text

                        option.CheckMark.enabled = customToggleOption.Value;    // Set the current value

                        option.gameObject.AddComponent<NumberOption>();

                        customToggleOption.GameToggleOption = option;
                    }
                    catch { }
                }
            }
        }
        #endregion

        #region ------------------------------ Patch Number Option Update ------------------------------
        [HarmonyPatch(typeof(NumberOption), nameof(NumberOption.FixedUpdate))]
        public static class Patch_NumberOption_Fixed_Update
        {
            public static void Postfix(NumberOption __instance)
            {
                try
                {
                    var options = AllCustomNumberOptions.Where(x => x.StringNameId == (int)__instance.Title);

                    if (options.Count() != 1)
                        return;

                    var option = options.First();

                    if (option.Value != __instance.Value)
                    {
                        option.Value = Mathf.RoundToInt(__instance.Value);
                        __instance.ValueText.Text = option.ValueStrings[option.Value];

                        option.ExecuteValueChanged();
                    }

                    if (__instance.ValueText.Text != option.ValueStrings[option.Value])
                        __instance.ValueText.Text = option.ValueStrings[option.Value];

                } catch { }
            }
        }
        #endregion

        #region ------------------------------ Patch Number Option Update ------------------------------
        [HarmonyPatch(typeof(ToggleOption), nameof(ToggleOption.FixedUpdate))]
        public static class Patch_ToggleOption_FixedUpdate
        {
            public static void Postfix(ToggleOption __instance)
            {
                try
                {
                    var options = AllCustomToggleOptions.Where(x => x.StringNameId == (int)__instance.Title);

                    if (options.Count() != 1)
                        return;

                    var option = options.First();

                    if (option.Value != __instance.CheckMark.enabled)
                    {
                        option.Value = __instance.CheckMark.enabled;

                        option.ExecuteValueChanged();
                    }
                }
                catch { }
            }
        }
        #endregion
    }

    public class CustomToggleOption
    {
        public event EventHandler<CustomToggleOptionEventArgs> ValueChanged;

        internal void ExecuteValueChanged()
        {
            ValueChanged?.Invoke(null, new CustomToggleOptionEventArgs() { ToggleOption = this });
        }

        public string TitleText { get; }

        public bool Value { get; set; }

        internal int StringNameId { get; }
        internal int IndexId { get; }

        public ToggleOption GameToggleOption { get; set; }

        public CustomToggleOption(string titleText)
        {
            TitleText = titleText;
            Value = false;

            StringNameId = CustomGameOptionsManager.StringNameId;
            CustomGameOptionsManager.StringNameId++;

            IndexId = CustomGameOptionsManager.IndexId;
            CustomGameOptionsManager.IndexId++;

            CustomGameOptionsManager.AllCustomToggleOptions.Add(this);
        }

        public void Remove()
        {
            CustomGameOptionsManager.AllCustomToggleOptions.Remove(this);
        }

        public class CustomToggleOptionEventArgs : EventArgs
        {
            public CustomToggleOption ToggleOption { get; set; }
        }
    }

    public class CustomNumberOption
    {
        public event EventHandler<CustomNumberOptionEventArgs> ValueChanged;

        internal void ExecuteValueChanged()
        {
            ValueChanged?.Invoke(null, new CustomNumberOptionEventArgs() { NumberOption = this });
        }

        public string Selected
        {
            get
            {
                return ValueStrings[Value];
            }
        }

        public string TitleText { get; }

        public int Value { get; set; }

        public List<string> ValueStrings { get; set; }

        internal int StringNameId { get; }
        internal int IndexId { get; }

        public NumberOption GameNumberOption { get; set; }

        public FloatRange FloatRange
        {
            get
            {
                return new FloatRange(0, ValueStrings.Count - 1);
            }
        }

        public CustomNumberOption(string titleText, List<string> valueStrings)
        {
            TitleText = titleText;
            Value = 0;
            ValueStrings = valueStrings;

            StringNameId = CustomGameOptionsManager.StringNameId;
            CustomGameOptionsManager.StringNameId++;

            IndexId = CustomGameOptionsManager.IndexId;
            CustomGameOptionsManager.IndexId++;

            CustomGameOptionsManager.AllCustomNumberOptions.Add(this);
        }

        public CustomNumberOption(string titleText, string[] valueStrings) : this(titleText, valueStrings.ToList()) { }

        public void Remove()
        {
            CustomGameOptionsManager.AllCustomNumberOptions.Remove(this);
        }

        public class CustomNumberOptionEventArgs : EventArgs
        {
            public CustomNumberOption NumberOption { get; set; }
        }
    }
}
