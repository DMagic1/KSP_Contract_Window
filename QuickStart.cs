#if DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QuickStart
{
    //This will kick us into the save called default and set the first vessel active
    
    
    [KSPAddon(KSPAddon.Startup.MainMenu, false)]

    public class Debug_AutoLoadPersistentSaveOnStartup : MonoBehaviour {

	public static bool first = true;
	public static int  vId = 0;

	public void Start () {
            
		if (first) {
			first = false;
			HighLogic.SaveFolder = "Contracts Ahoy";
			var game = GamePersistence.LoadGame ("persistent" , HighLogic.SaveFolder , true , false);
			if (game != null && game.flightState != null && game.compatible) {
                     
				List<ProtoVessel> allVessels = game.flightState.protoVessels;
				int suitableVessel = 0;

				for (vId = 0; vId < allVessels.Count; vId++) {
					switch (allVessels [vId].vesselType) {
						case VesselType.SpaceObject: 	continue;  // asteroids
						case VesselType.Unknown: 	continue;  // asteroids in facepaint
						case VesselType.EVA: continue;
						default:					suitableVessel = vId;
												break; // this one will do
                         }
					/* If you want a more stringent filter than
                      *   "vessel is not inert ball of space dirt", then you
                      *   will want to do it here.
                      */
                    }
				HighLogic.LoadScene(GameScenes.SPACECENTER);
				//FlightDriver.StartAndFocusVessel (game , suitableVessel);
               }
            CheatOptions.InfiniteFuel = true;
          }
     }
}
}
  

#endif