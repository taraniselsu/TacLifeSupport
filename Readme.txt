A Life Support system from Thunder Aerospace Corporation (TAC),
designed by Taranis Elsu.

For use with the Kerbal Space Program, http://kerbalspaceprogram.com/

This mod is made available under the Attribution-NonCommercial-ShareAlike 3.0 (CC
BY-NC-SA 3.0) creative commons license. See the LICENSE.txt file.

Source code is available from: https://github.com/taraniselsu/TacLifeSupport


Includes HexCan parts from Greys, licensed CC-BY 3.0. Used with permission. See
Readme-HexCans.txt and LICENSE-HexCans.txt in the HexCans directory.
Forum thread: http://forum.kerbalspaceprogram.com/showthread.php/33754


===== Features =====

* Kerbals require resources to survive whether in a vessel or on EVA: Food, Water,
      Oxygen, Electricity (for air quality and climate control)
* They produce waste resources: CO2, Waste, WasteWater
* They will die if they go without resources for too long: 30 days without food, 3 days
      without water, 5 minutes without Oxygen, and 2 hours without Electricity
* Crewed pods come stocked with 3 days of resources.
* When a Kerbal goes on EVA, he takes a half day of each resource with him in the EVA
      suit, taking from the pod that he was in.
* Kerbals do require resources even when their vessel is not active, and they can die if
      you leave them alone for too long.
* The system tries to stop Time Warp when resources get low and again when resources
      run out. No guarantees.
* Includes Hex Can containers for Food, Water, and Oxygen.


===== How to use =====

Add the following lines to any part that you want to have the Life Support functionality:

MODULE
{
	name = LifeSupportModule
}

And add the resources.


===== Installation procedure =====

1) Copy everything in the GameData directory to the {KSP}/GameData directory.
