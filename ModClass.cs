using GlobalEnums;
using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace Deathless
{
    public class Deathless : Mod, IMenuMod, IGlobalSettings<GlobalSettings>
    {
        internal static Deathless Instance;

        public Deathless() : base("DeathlessSteelSoul")
        {
            Instance = this;
        }
        public override string GetVersion() => "v0.1.0";


        private bool permadeath;
        private bool permadeathSubscribed;


        //Create and initalize a local variable to be able to access the settings
        public static GlobalSettings GS { get; set; } = new GlobalSettings();

        // First method to implement. The parameter is the read settings from the file
        public void OnLoadGlobal(GlobalSettings s)
        {
            GS = s; // save the read data into local variable;
        }

        public GlobalSettings OnSaveGlobal()
        {
            return GS;//return the local variable so it can be written in the json file
        }


        public bool ToggleButtonInsideMenu => throw new NotImplementedException();

        public List<IMenuMod.MenuEntry> GetMenuData(IMenuMod.MenuEntry? toggleButtonEntry)
        {
            return new List<IMenuMod.MenuEntry>
            {
                new IMenuMod.MenuEntry
                {
                    Name = "Permadeath:",
                    Description = null,
                    Values = new string[]
                    {
                        "Disabled",
                        "Enabled"
                    },
                    Saver = opt => this.permadeath = opt switch
                    {
                        0 => false,
                        1 => true,

                        _ => throw new InvalidOperationException()
                    },
                    Loader = () => this.permadeath switch
                    {
                        false => 0,
                        true => 1,
                    }

                }
            };
        }


        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Log("Initializing");

            Instance = this;

            Log("Initialized");

            permadeathSubscribed = false;

            this.permadeath = GS.permadeath;

            On.HeroController.Awake += UpdateGlobalSettings;

            On.HeroController.Awake += ConfigurePermadeathSetting;

        }

        private void ConfigurePermadeathSetting(On.HeroController.orig_Awake orig, HeroController self)
        {

            orig(self);

            Log("Configuring PERMADEATH");
            Log("Permadeath: " + permadeath);
            Log("Permadeath subscribed: " + permadeathSubscribed);

            if (permadeath == true && permadeathSubscribed == false)
            {
                Log("Enabling PERMADEATH");
                On.GameManager.GetCurrentMapZone += ForcePermadeath;
            }
            else if (permadeath == false && permadeathSubscribed == true)
            {
                Log("Removing PERMADEATH");
                On.GameManager.GetCurrentMapZone -= ForcePermadeath;

            }
        }

        private string ForcePermadeath(On.GameManager.orig_GetCurrentMapZone orig, GameManager self)
        {


            if (PlayerData.instance.health <= 0)
            {
                return "ABYSS";
            }
            else
            {
                return orig(self);
            }
        }

        private void UpdateGlobalSettings(On.HeroController.orig_Awake orig, HeroController self)
        {
            orig(self);

            Log("Updating Global Settings");

            GS.permadeath = this.permadeath;
        }
    }
}