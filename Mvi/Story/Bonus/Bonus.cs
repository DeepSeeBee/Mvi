using CharlyBeck.Mvi.Story.Value;
using CharlyBeck.Utils3.ServiceLocator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharlyBeck.Mvi.Story.Bonus
{
    enum CGemClassEnum
    {
        Offense,
        Defense,
        Navigation,
    }

    internal enum CGemEnum
    {
        /////////////////////// Class .........Text...............................................................Hilfetext
        ExtraLifeGem,        // Defense       <EXLF> // Extraleben.
        ShellRepair,         // Defense       <SHRP> // Hülle reparieren, hülle geht bei kollisionen kaputt
        ShieldGem,           // Defense       <SHLD> // Schutzschild, kamikatze flug auf planeten
        AmmoSpeed,           // Offense       <AMSP> // Schüsse flieen schneller
        FireRate,            // Offense       <FIRR> // Erhöht feuerrate
        LaserEnergyGem,      // Offense       <LSEN> // zum zerstören von asteroiden                              
        NuclearMissileGem,   // Offense       <NUKE> // zum zerstören von sonnen                                  Destroy a sun. (Dont do when planets remain)
        GuidedMissile,       // Offense       <GDEM> // Hilfeich zum zsertören kleiner monde.                     Shoot to fire, press again to follow target.
        ThermalShieldGem,    // Navigation    <THSH> // Zum landen auf planeten                                   Land on a planet.
        KruskalScannerGem,   // Navigation    <WHLE> // Zum öffnen von wurmlöchern                                Turn a sun into a wormhole to teleport and teleport..
        SlowMotion,          // Navigation    <SLMO> // Verlangsamt alles                                         Turn down the speed of time.
        Antigravity,         // Navigation    <AGRA> // Gravitation von planeten hat keinen/weniger einfluss.     Turn off or lower gravity.
        AutoPilot,           // Navigation    <AUTP> // Zum automatischen folgen von umlaufbahnen.                Follow path of orb. Press when orb is focused.

        //
        //FuelGem,
        //QuestGem,
    }



}
