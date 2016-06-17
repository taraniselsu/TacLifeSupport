

using HighlightingSystem;
/**
* REPOSoftTech KSP Utilities
* (C) Copyright 2015, Jamie Leighton
*
* Kerbal Space Program is Copyright (C) 2013 Squad. See http://kerbalspaceprogram.com/. This
* project is in no way associated with nor endorsed by Squad.
* 
*
* Licensed under the Attribution-NonCommercial-ShareAlike (CC BY-NC-SA 4.0) creative commons license. 
* See <https://creativecommons.org/licenses/by-nc-sa/4.0/> for full details (except where else specified in this file).
*
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace RSTUtils
{
	public enum GameState
	{
		FLIGHT = 0,
		EDITOR = 1,
		EVA = 2,
		SPACECENTER = 3,
		OTHER = 4
	}

	internal static class Utilities
	{
		public static int randomSeed = new Random().Next();
		private static int _nextrandomInt = randomSeed;        

		public static int getnextrandomInt()
		{
			_nextrandomInt ++;
			return _nextrandomInt;
		}

		private static GameState state;

		//Set the Game State mode indicator, 0 = inflight, 1 = editor, 2 on EVA or F2
		public static bool GameModeisFlight
		{
			get
			{
				state = SetModeFlag();
				if (state == GameState.FLIGHT) return true;
				return false;
			}
		}

		public static bool GameModeisEditor
		{
			get
			{
				state = SetModeFlag();
				if (state == GameState.EDITOR) return true;
				return false;
			}
		}

		public static bool GameModeisEVA
		{
			get
			{
				state = SetModeFlag();
				if (state == GameState.EVA) return true;
				return false;
			}
		}

		public static bool GameModeisSpaceCenter
		{
			get
			{
				state = SetModeFlag();
				if (state == GameState.SPACECENTER) return true;
				return false;
			}
		}

		public static GameState GameMode
		{
			get
			{
				return SetModeFlag();
			} 
		}

		public static GameState SetModeFlag()
		{
			//Set the mode flag, 0 = inflight, 1 = editor, 2 on EVA or F2
			if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
			{
				return GameState.SPACECENTER;
			}
			//if (FlightGlobals.fetch != null && FlightGlobals.ActiveVessel != null)  // Check if in flight
			if (HighLogic.LoadedSceneIsFlight)
			{
				if (FlightGlobals.fetch != null)
				{
					if (FlightGlobals.ActiveVessel != null)
					{
						if (FlightGlobals.ActiveVessel.isEVA) // EVA kerbal
						{
							return GameState.EVA;
						}
					}
				}
				return GameState.FLIGHT;
			}
			if (EditorLogic.fetch != null) // Check if in editor
			{
				return GameState.EDITOR;
			}
			return GameState.OTHER;
		}
		
		#region GeometryandSpace
		//Geometry and space

		public static double DistanceFromHomeWorld(Vessel vessel)
		{
			Vector3d vslPos = vessel.GetWorldPos3D();
			CelestialBody HmePlanet = Planetarium.fetch.Home;
			Log_Debug("Home = " + HmePlanet.name + " Pos = " + HmePlanet.position);
			Log_Debug("Vessel Pos = " + vslPos);
			Vector3d hmeplntPos = HmePlanet.position;
			double DstFrmHome = Math.Sqrt(Math.Pow(vslPos.x - hmeplntPos.x, 2) + Math.Pow(vslPos.y - hmeplntPos.y, 2) + Math.Pow(vslPos.z - hmeplntPos.z, 2));
			Log_Debug("Distance from Home Planet = " + DstFrmHome);
			return DstFrmHome;
		}

		public static double DistanceFromHomeWorld(string bodyname)
		{
			CelestialBody body = FlightGlobals.Bodies.FirstOrDefault(a => a.name == bodyname);
			if (body == null) body = Planetarium.fetch.Home;
			Vector3d bodyPos = body.getPositionAtUT(0);
			CelestialBody HmePlanet = Planetarium.fetch.Home;
			Log_Debug("Home = " + HmePlanet.name + " Pos = " + HmePlanet.getPositionAtUT(0));
			Log_Debug("Body Pos = " + bodyPos);
			Vector3d hmeplntPos = HmePlanet.getPositionAtUT(0);
			double DstFrmHome = Math.Sqrt(Math.Pow(bodyPos.x - hmeplntPos.x, 2) + Math.Pow(bodyPos.y - hmeplntPos.y, 2) + Math.Pow(bodyPos.z - hmeplntPos.z, 2));
			Log_Debug("Distance from Home Planet = " + DstFrmHome);
			return DstFrmHome;
		}

		public static bool CelestialBodyDistancetoSun(CelestialBody cb, out Vector3d sun_dir, out double sun_dist)
		{
			// bodies traced against
			CelestialBody sun = FlightGlobals.Bodies[0];
			if (cb == sun) //If we have passed in the sun as the cb we default to a distance of 700000Km
			{
				sun_dir = Vector3d.forward;
				sun_dist = sun.Radius + 700000000;
				sun_dir /= sun_dist;
				return true;
			}
			sun_dir = sun.position - cb.position;
			sun_dist = sun_dir.magnitude;
			sun_dir /= sun_dist;
			sun_dist -= sun.Radius;
			return true;
		}

		// return sun luminosity
		public static double SolarLuminosity
		{
			get
			{
				// note: it is 0 before loading first vessel in a game session, we compute it in that case
				if (PhysicsGlobals.SolarLuminosity <= double.Epsilon)
				{
					double A = FlightGlobals.GetHomeBody().orbit.semiMajorAxis;
					return A * A * 12.566370614359172 * PhysicsGlobals.SolarLuminosityAtHome;
				}
				return PhysicsGlobals.SolarLuminosity;
			}
		}

		#endregion GeometryandSpace  

		#region ObjectsandTransforms
		public static void PrintTransform(Transform t, string title = "")
		{
			Log_Debug("------" + title + "------");
			Log_Debug("Position: " + t.localPosition);
			Log_Debug("Rotation: " + t.localRotation);
			Log_Debug("Scale: " + t.localScale);
			Log_Debug("------------------");
		}

		public static void DumpObjectProperties(object o, string title = "---------")
		{
			// Iterate through all of the properties
			Log_Debug("--------- " + title + " ------------");
			foreach (PropertyInfo property in o.GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public))
			{
				if (property.CanRead)
					Log_Debug(property.Name + " = " + property.GetValue(o, null));
			}
			Log_Debug("--------------------------------------");
		}
		
		// Dump an object by reflection
		internal static void DumpObjectFields(object o, string title = "---------")
		{
			// Dump (by reflection)
			Debug.Log("---------" + title + "------------");
			foreach (FieldInfo field in o.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public))
			{
				if (!field.IsStatic)
				{
					Debug.Log(field.Name + " = " + field.GetValue(o));
				}
			}
			Debug.Log("--------------------------------------");
		}

		// Use Reflection to get a field from an object
		internal static object GetObjectField(object o, string fieldName)
		{
			object outputObj = new object();
			bool foundObj = false;
			foreach (FieldInfo field in o.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public))
			{
				if (!field.IsStatic)
				{
					if (field.Name == fieldName)
					{
						foundObj = true;
						outputObj = field.GetValue(o);
						break;
					}
				}
			}
			if (foundObj)
			{
				return outputObj;
			}
			return null;
		}

		/**
		  * Recursively searches for a named transform in the Transform heirarchy.  The requirement of
		  * such a function is sad.  This should really be in the Unity3D API.  Transform.Find() only
		  * searches in the immediate children.
		  *
		  * @param transform Transform in which is search for named child
		  * @param name Name of child to find
		  *
		  * @return Desired transform or null if it could not be found
		  */

		internal static Transform FindInChildren(Transform transform, string name)
		{
			// Is this null?
			if (transform == null)
			{
				return null;
			}

			// Are the names equivalent
			if (transform.name == name)
			{
				return transform;
			}

			// If we did not find a transform, search through the children
			return (from Transform child in transform select FindInChildren(child, name)).FirstOrDefault(t => t != null);

			// Return the transform (will be null if it was not found)
		}

		public static Transform FindChildRecursive(Transform parent, string name)
		{
			return parent.gameObject.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name == name);
		}

		public static Animation FindAnimChildRecursive(Transform parent, string name)
		{
			return parent.gameObject.GetComponentsInChildren<Animation>().FirstOrDefault(t => t.name == name);
		}

		internal static void dmpKerbalRefs(Kerbal kerbal, Kerbal seatkerbalref, Kerbal protocrewkerbalref)
		{
			if (kerbal != null)
			{
				Log_Debug("kerbal " + kerbal.name + " " + kerbal.GetInstanceID());
				Log_Debug(kerbal.GetComponent("TRIvaModule") != null
					? "kerbal has TRIvaModule attached"
					: "kerbal DOES NOT have TRIvaModule attached");
			}

			if (seatkerbalref != null)
			{
				Log_Debug("seatkerbalref " + seatkerbalref.name + " " + seatkerbalref.GetInstanceID());
				Log_Debug(seatkerbalref.GetComponent("TRIvaModule") != null
					? "seatkerbalref has TRIvaModule attached"
					: "seatkerbalref DOES NOT have TRIvaModule attached");
			}
			if (protocrewkerbalref != null)
			{
				Log_Debug("protocrewkerbalref " + protocrewkerbalref.name + " " + protocrewkerbalref.GetInstanceID());
				Log_Debug(protocrewkerbalref.GetComponent("TRIvaModule") != null
					? "protocrewkerbalref has TRIvaModule attached"
					: "protocrewkerbalref DOES NOT have TRIvaModule attached");
			}
		}

		internal static void dmpAllKerbals()
		{
			foreach (Kerbal kerbal in Resources.FindObjectsOfTypeAll<Kerbal>())
			{
				Log_Debug("Kerbal " + kerbal.name + " " + kerbal.crewMemberName + " instance " + kerbal.GetInstanceID() + " rosterstatus " + kerbal.rosterStatus);
				Log_Debug(kerbal.protoCrewMember == null ? "ProtoCrewmember is null " : "ProtoCrewmember exists " + kerbal.protoCrewMember.name);
			}
		}

		internal static void dmpAnimationNames(Animation anim)
		{
			List<AnimationState> states = new List<AnimationState>(anim.Cast<AnimationState>());
			Log_Debug("Animation " + anim.name);
			foreach (AnimationState state in states)
			{
				Log_Debug("Animation clip " + state.name);
			}
		}

		// The following method is modified from RasterPropMonitor as-is. Which is covered by GNU GENERAL PUBLIC LICENSE Version 3, 29 June 2007
		internal static void setTransparentTransforms(this Part thisPart, string transparentTransforms)
		{
			string transparentShaderName = "Transparent/Specular";
			var transparentShader = Shader.Find(transparentShaderName);
			foreach (string transformName in transparentTransforms.Split('|'))
			{
				Log_Debug("setTransparentTransforms " + transformName);
				try
				{
					Transform tr = thisPart.FindModelTransform(transformName.Trim());
					if (tr != null)
					{
						// We both change the shader and backup the original shader so we can undo it later.
						Shader backupShader = tr.GetComponent<Renderer>().material.shader;
						tr.GetComponent<Renderer>().material.shader = transparentShader;
					}
				}
				catch (Exception e)
				{
					Debug.Log("Unable to set transparent shader transform " + transformName);
					Debug.LogException(e);
				}
			}
		}
		
		#endregion ObjectsandTransforms

		#region Cameras

		internal static Camera FindCamera(string name)
		{
			return Camera.allCameras.FirstOrDefault(c => c.name == name);
		}

		// Dump all Unity Cameras
		internal static void DumpCameras()
		{
			// Dump (by reflection)
			Debug.Log("--------- Dump Unity Cameras ------------");
			foreach (Camera c in Camera.allCameras)
			{
				Debug.Log("Camera " + c.name + " cullingmask " + c.cullingMask + " depth " + c.depth + " farClipPlane " + c.farClipPlane + " nearClipPlane " + c.nearClipPlane);
			}

			Debug.Log("--------------------------------------");
		}
		
		public static Camera findCameraByName(string camera)
		{
			return Camera.allCameras.FirstOrDefault(cam => cam.name == camera);
		}

		private static Camera StockOverlayCamera;
		/// <summary>
		/// Returns True if the Stock Overlay Camera Mode is on, otherwise will return false.
		/// </summary>
		public static bool StockOverlayCamIsOn
		{
			get
			{
				StockOverlayCamera = findCameraByName("InternalSpaceOverlay Host");
				if (StockOverlayCamera != null) return true;
				return false;
			}
		}

		private static Shader DepthMaskShader;
		private static string DepthMaskShaderName = "DepthMask";
		/// <summary>
		/// Will search for and change the Mesh (and all it's children) supplied in MeshName Field on the part supplied to Enabled or NotEnabled based on the SetVisible parm.
		/// </summary>
		/// <param name="part">The part to look for the mesh on</param>
		/// <param name="SetVisible">True will Enable the mesh, False will disable the mesh</param>
		/// <param name="MeshName">String containing the Mesh name to look for on the part</param>
		internal static void SetInternalDepthMask(Part part, bool SetVisible, string MeshName = "")
		{
			if (DepthMaskShader == null) DepthMaskShader = Shader.Find(DepthMaskShaderName);
			if (part.internalModel != null)
			{
				if (MeshName != "")
				{
					Transform parentTransform = FindInChildren(part.internalModel.transform, MeshName);
					if (parentTransform != null)
					{
						parentTransform.gameObject.SetActive(SetVisible);
					}
				}
			}
		}

		#endregion Cameras

		#region Animations
		public static IEnumerator WaitForAnimation(Animation animation, string name)
		{
			do
			{
				yield return null;
			} while (animation.IsPlaying(name));
		}

		public static IEnumerator WaitForAnimationNoClip(Animation animation)
		{
			do
			{
				yield return null;
			} while (animation.isPlaying);
		}

		#endregion Animations

		#region Kerbals

		// The following method is derived from TextureReplacer mod. Which is licensed as:
		//Copyright © 2013-2015 Davorin Učakar, Ryan Bray
		//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files(the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
		//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
		private static double atmSuitPressure = 50.0;

		internal static bool isAtmBreathable()
		{
			bool value = !HighLogic.LoadedSceneIsFlight
						 || (FlightGlobals.getStaticPressure() >= atmSuitPressure);
			Log_Debug("isATMBreathable Inflight? " + value + " InFlight " + HighLogic.LoadedSceneIsFlight + " StaticPressure " + FlightGlobals.getStaticPressure());
			return value;
		}

		// The following method is derived from TextureReplacer mod. Which is licensed as:
		//Copyright © 2013-2015 Davorin Učakar, Ryan Bray
		//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files(the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
		//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
		private static Mesh[] helmetMesh = { null, null };

		private static Mesh[] visorMesh = { null, null };
		private static bool helmetMeshstored;

		internal static void storeHelmetMesh()
		{
			Log_Debug("StoreHelmetMesh");
			foreach (Kerbal kerbal in Resources.FindObjectsOfTypeAll<Kerbal>())
			{
				int gender = kerbal.transform.name == "kerbalFemale" ? 1 : 0;
				// Save pointer to helmet & visor meshes so helmet removal can restore them.
				foreach (SkinnedMeshRenderer smr in kerbal.GetComponentsInChildren<SkinnedMeshRenderer>(true))
				{
					if (smr.name.EndsWith("helmet", StringComparison.Ordinal))
						helmetMesh[gender] = smr.sharedMesh;
					else if (smr.name.EndsWith("visor", StringComparison.Ordinal))
						visorMesh[gender] = smr.sharedMesh;
				}
			}
			helmetMeshstored = true;
		}

		// The following method is derived from TextureReplacer mod.Which is licensed as:
		//Copyright © 2013-2015 Davorin Učakar, Ryan Bray
		//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files(the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
		//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
		internal static void setHelmetshaders(Kerbal thatKerbal, bool helmetOn)
		{
			if (!helmetMeshstored)
				storeHelmetMesh();

			//This will check if Atmospher is breathable then we always remove our hetmets regardless.
			if (helmetOn && isAtmBreathable())
			{
				helmetOn = false;
				Log_Debug("setHelmetShaders to put on helmet but in breathable atmosphere");
			}

			try
			{
				foreach (SkinnedMeshRenderer smr in thatKerbal.helmetTransform.GetComponentsInChildren<SkinnedMeshRenderer>())
				{
					if (smr.name.EndsWith("helmet", StringComparison.Ordinal))
						smr.sharedMesh = helmetOn ? helmetMesh[(int)thatKerbal.protoCrewMember.gender] : null;
					else if (smr.name.EndsWith("visor", StringComparison.Ordinal))
						smr.sharedMesh = helmetOn ? visorMesh[(int)thatKerbal.protoCrewMember.gender] : null;
				}
			}
			catch (Exception ex)
			{
				Log("Error attempting to setHelmetshaders for " + thatKerbal.name + " to " + helmetOn);
				Log(ex.Message);
			}
		}

		// The following method is derived from TextureReplacer mod. Which is licensed as:
		//Copyright © 2013-2015 Davorin Učakar, Ryan Bray
		//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files(the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
		//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
		internal static void setHelmets(this Part thisPart, bool helmetOn)
		{
			if (thisPart.internalModel == null)
			{
				Log_Debug("setHelmets but no internalModel");
				return;
			}

			if (!helmetMeshstored)
				storeHelmetMesh();

			Log_Debug("setHelmets helmetOn=" + helmetOn);
			//Kerbal thatKerbal = null;
			foreach (InternalSeat thatSeat in thisPart.internalModel.seats)
			{
				if (thatSeat.crew != null)
				{
					Kerbal thatKerbal = thatSeat.kerbalRef;
					if (thatKerbal != null)
					{
						thatSeat.allowCrewHelmet = helmetOn;
						Log_Debug("Setting helmet=" + helmetOn + " for kerbal " + thatSeat.crew.name);
						// `Kerbal.ShowHelmet(false)` irreversibly removes a helmet while
						// `Kerbal.ShowHelmet(true)` has no effect at all. We need the following workaround.
						// I think this can be done using a coroutine to despawn and spawn the internalseat crewmember kerbalref.
						// But I found this workaround in TextureReplacer so easier to use that.
						//if (thatKerbal.showHelmet)
						//{
						setHelmetshaders(thatKerbal, helmetOn);
						//}
						//else
						//    Log_Debug("Showhelmet is OFF so the helmettransform does not exist");
					}
					else
						Log_Debug("kerbalref = null?");
				}
			}
		}

		// Sets the kerbal layers to make them visible (Thawed) or not (Frozen), setVisible = true sets layers to visible, false turns them off.
		internal static void setFrznKerbalLayer(Part part, ProtoCrewMember kerbal, bool setVisible)
		{
			if (!setVisible)
			{
				kerbal.KerbalRef.SetVisibleInPortrait(setVisible);
				kerbal.KerbalRef.InPart = null;
			}
				
			kerbal.KerbalRef.gameObject.SetActive(setVisible);
			if (setVisible)
			{
				kerbal.KerbalRef.SetVisibleInPortrait(setVisible);
				kerbal.KerbalRef.InPart = part;
			}
				
		}

		private static RuntimeAnimatorController kerbalIVAController, myController;
		private static AnimatorOverrideController myOverrideController;

		internal static void subdueIVAKerbalAnimations(Kerbal kerbal)
		{
			try
			{
				foreach (Animator anim in kerbal.gameObject.GetComponentsInChildren<Animator>())
				{
					if (anim.name == kerbal.name)
					{
						kerbalIVAController = anim.runtimeAnimatorController;
						myController = anim.runtimeAnimatorController;
						myOverrideController = new AnimatorOverrideController();
						myOverrideController.runtimeAnimatorController = myController;
						myOverrideController["idle_animA_upWord"] = myOverrideController["idle_animH_notDoingAnything"];
						myOverrideController["idle_animB"] = myOverrideController["idle_animH_notDoingAnything"];
						myOverrideController["idle_animC"] = myOverrideController["idle_animH_notDoingAnything"];
						myOverrideController["idle_animD_dance"] = myOverrideController["idle_animH_notDoingAnything"];
						myOverrideController["idle_animE_drummingHelmet"] = myOverrideController["idle_animH_notDoingAnything"];
						myOverrideController["idle_animI_drummingControls"] = myOverrideController["idle_animH_notDoingAnything"];
						myOverrideController["idle_animJ_yo"] = myOverrideController["idle_animH_notDoingAnything"];
						myOverrideController["idle_animJ_IdleLoopShort"] = myOverrideController["idle_animH_notDoingAnything"];
						myOverrideController["idle_animK_footStretch"] = myOverrideController["idle_animH_notDoingAnything"];
						myOverrideController["head_rotation_staringUp"] = myOverrideController["idle_animH_notDoingAnything"];
						myOverrideController["head_rotation_longLookUp"] = myOverrideController["idle_animH_notDoingAnything"];
						myOverrideController["head_faceExp_fun_ohAh"] = myOverrideController["idle_animH_notDoingAnything"];
						// Put this line at the end because when you assign a controller on an Animator, unity rebinds all the animated properties
						anim.runtimeAnimatorController = myOverrideController;
						Log_Debug("Animator " + anim.name + " for " + kerbal.name + " subdued");
					}
				}
			}
			catch (Exception ex)
			{
				Log(" failed to subdue IVA animations for " + kerbal.name);
				Debug.LogException(ex);
			}
		}

		internal static void reinvigerateIVAKerbalAnimations(Kerbal kerbal)
		{
			foreach (Animator anim in kerbal.gameObject.GetComponentsInChildren<Animator>())
			{
				if (anim.name == kerbal.name)
				{
					myController = kerbalIVAController;
					myOverrideController = new AnimatorOverrideController();
					myOverrideController.runtimeAnimatorController = myController;
					// Put this line at the end because when you assign a controller on an Animator, unity rebinds all the animated properties
					anim.runtimeAnimatorController = myOverrideController;
					Log_Debug("Animator " + anim.name + " for " + kerbal.name + " reinvigerated");
				}
			}
		}

		#endregion Kerbals

		#region Vessels
		// The following method is taken from RasterPropMonitor as-is. Which is covered by GNU GENERAL PUBLIC LICENSE Version 3, 29 June 2007
		/// <summary>
		/// Returns True if thatVessel is the activevessel and the camera is in IVA mode, otherwise returns false.
		/// </summary>
		/// <param name="thatVessel"></param>
		/// <returns></returns>
		internal static bool VesselIsInIVA(Vessel thatVessel)
		{
			// Inactive IVAs are renderer.enabled = false, this can and should be used...
			// ... but now it can't because we're doing transparent pods, so we need a more complicated way to find which pod the player is in.
			return HighLogic.LoadedSceneIsFlight && IsActiveVessel(thatVessel) && IsInIVA;
		}

		// The following method is taken from RasterPropMonitor as-is. Which is covered by GNU GENERAL PUBLIC LICENSE Version 3, 29 June 2007
		/// <summary>
		/// Returns True if thatVessel is the ActiveVessel, otherwise returns false.
		/// </summary>
		/// <param name="thatVessel"></param>
		/// <returns></returns>
		internal static bool IsActiveVessel(Vessel thatVessel)
		{
			return HighLogic.LoadedSceneIsFlight && thatVessel != null && thatVessel.isActiveVessel;
		}
		
		// The following method is taken from RasterPropMonitor as-is. Which is covered by GNU GENERAL PUBLIC LICENSE Version 3, 29 June 2007
		public static bool UserIsInPod(Part thisPart)
		{

			// Just in case, check for whether we're not in flight.
			if (!HighLogic.LoadedSceneIsFlight)
				return false;

			// If we're not in IVA, or the part does not have an instantiated IVA, the user can't be in it.
			if (!VesselIsInIVA(thisPart.vessel) || thisPart.internalModel == null)
				return false;

			// Now that we got that out of the way, we know that the user is in SOME pod on our ship. We just don't know which.
			// Let's see if he's controlling a kerbal in our pod.
			if (ActiveKerbalIsLocal(thisPart))
				return true;

			// There still remains an option of InternalCamera which we will now sort out.
			if (CameraManager.Instance.currentCameraMode == CameraManager.CameraMode.Internal)
			{
				// So we're watching through an InternalCamera. Which doesn't record which pod we're in anywhere, like with kerbals.
				// But we know that if the camera's transform parent is somewhere in our pod, it's us.
				// InternalCamera.Instance.transform.parent is the transform the camera is attached to that is on either a prop or the internal itself.
				// The problem is figuring out if it's in our pod, or in an identical other pod.
				// Unfortunately I don't have anything smarter right now than get a list of all transforms in the internal and cycle through it.
				// This is a more annoying computation than looking through every kerbal in a pod (there's only a few of those,
				// but potentially hundreds of transforms) and might not even be working as I expect. It needs testing.
				return thisPart.internalModel.GetComponentsInChildren<Transform>().Any(thisTransform => thisTransform == InternalCamera.Instance.transform.parent);
			}

			return false;
		}

		// The following method is taken from RasterPropMonitor as-is. Which is covered by GNU GENERAL PUBLIC LICENSE Version 3, 29 June 2007
		public static bool ActiveKerbalIsLocal(this Part thisPart)
		{
			return FindCurrentKerbal(thisPart) != null;
		}

		// The following method is taken from RasterPropMonitor as-is. Which is covered by GNU GENERAL PUBLIC LICENSE Version 3, 29 June 2007
		public static Kerbal FindCurrentKerbal(this Part thisPart)
		{
			if (thisPart.internalModel == null || !VesselIsInIVA(thisPart.vessel))
				return null;
			// InternalCamera instance does not contain a reference to the kerbal it's looking from.
			// So we have to search through all of them...
			return (from thatSeat in thisPart.internalModel.seats
					where thatSeat.kerbalRef != null
					where thatSeat.kerbalRef.eyeTransform == InternalCamera.Instance.transform.parent
					select thatSeat.kerbalRef).FirstOrDefault();
		}

		// The following method is taken from RasterPropMonitor as-is. Which is covered by GNU GENERAL PUBLIC LICENSE Version 3, 29 June 2007
		/// <summary>
		/// True if Camera is in IVA mode, otherwise false.
		/// </summary>
		internal static bool IsInIVA
		{
			get { return CameraManager.Instance.currentCameraMode == CameraManager.CameraMode.IVA; }
		}
		/// <summary>
		/// True if Camera is in Internal mode, otherwise false.
		/// </summary>
		internal static bool IsInInternal
		{
			get { return CameraManager.Instance.currentCameraMode == CameraManager.CameraMode.Internal; }
		}

		internal static bool ValidVslType(Vessel v)
		{
			switch (v.vesselType)
			{
				case VesselType.Base:
				case VesselType.Lander:
				case VesselType.Probe:
				case VesselType.Rover:
				case VesselType.Ship:
				case VesselType.Station:
					return true;

				default:
					return false;
			}
		}
	   
		// The following method is taken from Kerbal Alarm Clock as-is. Which is covered by MIT license.
		internal static int getVesselIdx(Vessel vtarget)
		{
			for (int i = 0; i < FlightGlobals.Vessels.Count; i++)
			{
				if (FlightGlobals.Vessels[i].id == vtarget.id)
				{
					Log_Debug("Found Target idx=" + i + " (" + vtarget.id + ")");
					return i;
				}
			}
			return -1;
		}

		/// <summary>
		/// Will Spawn the Internal Model for a part, we do this for DeepFreeze Mod because it doesn't work if the crew capacity is zero, which may be
		/// the case sometimes for DeepFreeze parts.
		/// </summary>
		/// <param name="part">The Part to spawn the internal model for</param>
		/// <returns>True if successful or False if not</returns>
		internal static bool spawnInternal(Part part)
		{
			try
			{
				if (part.internalModel != null) return true;
				part.CreateInternalModel();
				if (part.internalModel != null)
				{
					part.internalModel.Initialize(part);
					part.internalModel.SpawnCrew();
				}
				else
				{
					return false;
				}

				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}


		public static void PartHighlight(Part part, bool on)
		{
			if (on)
			{
				if (part.highlighter == null)
				{
					var color = XKCDColors.Yellow;
					var model = part.FindModelTransform("model");
					part.highlighter = model.gameObject.AddComponent<Highlighter>();
					part.highlighter.ConstantOn(color);
					part.SetHighlightColor(color);
					part.SetHighlight(true, false);
				}
			}
			else
			{
				if (part.highlighter != null)
				{
					part.SetHighlightDefault();
					part.highlighter.gameObject.DestroyGameObjectImmediate();
					part.highlighter = null;
				}
			}
		}

		#endregion Vessels

		#region Temperature
		//Temperature
		internal static float KelvintoCelsius(float kelvin)
		{
			return kelvin - 273.15f;
		}

		internal static float CelsiustoKelvin(float celsius)
		{
			return celsius + 273.15f;
		}

		#endregion Temperature

		#region Resources

		private static List<PartResource> resources;
		//Resources
		public static double GetAvailableResource(Part part, String resourceName)
		{
			resources = new List<PartResource>();
			part.GetConnectedResources(PartResourceLibrary.Instance.GetDefinition(resourceName).id, ResourceFlowMode.ALL_VESSEL, resources);
			return resources.Sum(pr => pr.amount);
		}

		public const int MAX_TRANSFER_ATTEMPTS = 4;

		private static double totalReceived;
		private static double requestAmount;
		private static double received;
		public static double RequestResource(Part cvp, String name, double amount)
		{
			if (amount <= 0.0)
				return 0.0;
			totalReceived = 0.0;
			requestAmount = amount;
			for (int attempts = 0; (attempts < MAX_TRANSFER_ATTEMPTS) && (amount > 0.000000000001); attempts++)
			{
				received = cvp.RequestResource(name, requestAmount, ResourceFlowMode.ALL_VESSEL);
				//Log_Debug("requestResource attempt " + attempts);
				//Log_Debug("requested power = " + requestAmount.ToString("0.0000000000000000000000"));
				//Log_Debug("received power = " + received.ToString("0.0000000000000000000000"));
				totalReceived += received;
				amount -= received;
				//Log_Debug("amount = " + amount.ToString("0.0000000000000000000000"));
				if (received <= 0.0)
					requestAmount = amount * 0.5;
				else
					requestAmount = amount;
			}
			return totalReceived;
		}

		#endregion Resources

		#region GUI&Window
		// GUI & Window Methods

		public static int scaledScreenHeight = 1;
		public static int scaledScreenWidth = 1;
		private static bool scaledScreenset;

		internal static void setScaledScreen()
		{
			scaledScreenHeight = Mathf.RoundToInt(Screen.height / 1);
			scaledScreenWidth = Mathf.RoundToInt(Screen.width / 1);
			scaledScreenset = true;
		}
		
		internal static RectOffset SetRectOffset(RectOffset tmpRectOffset, int intValue)
		{
			return SetRectOffset(tmpRectOffset, intValue, intValue, intValue, intValue);
		}

		internal static RectOffset SetRectOffset(RectOffset tmpRectOffset, int Left, int Right, int Top, int Bottom)
		{
			tmpRectOffset.left = Left;
			tmpRectOffset.top = Top;
			tmpRectOffset.right = Right;
			tmpRectOffset.bottom = Bottom;
			return tmpRectOffset;
		}

		//Tooltip variables
		//Store the tooltip text from throughout the code
		internal static String strToolTipText = "";
		internal static String strLastTooltipText = "";
		//is it displayed and where
		internal static Boolean blnToolTipDisplayed;
		internal static Rect rectToolTipPosition;
		internal static Int32 intTooltipVertOffset = 12;
		internal static Int32 intTooltipMaxWidth = 250;
		//timer so it only displays for a period of time
		internal static float fltTooltipTime;
		internal static float fltMaxToolTipTime = 15f;
		internal static GUIStyle _TooltipStyle;
		
		// The following two methods are derived from Kerbal Alarm Clock mod. Which is licensed under:
		//The MIT License(MIT) Copyright(c) 2014, David Tregoning
		// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files(the "Software"), to deal
		// in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
		// copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
		// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
		// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
		// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
		// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
		// THE SOFTWARE.
		internal static void DrawToolTip()
		{
			if (strToolTipText != "" && (fltTooltipTime < fltMaxToolTipTime))
			{
				GUIContent contTooltip = new GUIContent(strToolTipText);
				if (!blnToolTipDisplayed || (strToolTipText != strLastTooltipText))
				{
					//reset display time if text changed
					fltTooltipTime = 0f;
					//Calc the size of the Tooltip
					rectToolTipPosition = new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y + intTooltipVertOffset, 0, 0);
					float minwidth, maxwidth;
					if (_TooltipStyle == null)
					{
						Log("Missing _TooltipStyle definition, cannot draw tooltips");
						return;
					}
					_TooltipStyle.CalcMinMaxWidth(contTooltip, out minwidth, out maxwidth); // figure out how wide one line would be
					rectToolTipPosition.width = Math.Min(intTooltipMaxWidth - _TooltipStyle.padding.horizontal, maxwidth); //then work out the height with a max width
					rectToolTipPosition.height = _TooltipStyle.CalcHeight(contTooltip, rectToolTipPosition.width); // heers the result
					//Make sure its not off the right of the screen
					if (rectToolTipPosition.x + rectToolTipPosition.width > Screen.width) rectToolTipPosition.x = Screen.width - rectToolTipPosition.width;
				}
				//Draw the Tooltip
				GUI.Label(rectToolTipPosition, contTooltip, _TooltipStyle);
				//On top of everything
				GUI.depth = 0;

				//update how long the tip has been on the screen and reset the flags
				fltTooltipTime += Time.deltaTime;
				blnToolTipDisplayed = true;
			}
			else
			{
				//clear the flags
				blnToolTipDisplayed = false;
			}
			if (strToolTipText != strLastTooltipText) fltTooltipTime = 0f;
			strLastTooltipText = strToolTipText;
		}

		internal static void SetTooltipText()
		{
			if (Event.current.type == EventType.Repaint)
			{
				strToolTipText = GUI.tooltip;
			}
		}
		
		// The following method is taken from RasterPropMonitor as-is. Which is covered by GNU GENERAL PUBLIC LICENSE Version 3, 29 June 2007
		public static string WordWrap(string text, int maxLineLength)
		{
			var sb = new StringBuilder();
			char[] prc = { ' ', ',', '.', '?', '!', ':', ';', '-' };
			char[] ws = { ' ' };

			foreach (string line in text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
			{
				int currentIndex;
				int lastWrap = 0;
				do
				{
					currentIndex = lastWrap + maxLineLength > line.Length ? line.Length : line.LastIndexOfAny(prc, Math.Min(line.Length - 1, lastWrap + maxLineLength)) + 1;
					if (currentIndex <= lastWrap)
						currentIndex = Math.Min(lastWrap + maxLineLength, line.Length);
					sb.AppendLine(line.Substring(lastWrap, currentIndex - lastWrap).Trim(ws));
					lastWrap = currentIndex;
				} while (currentIndex < line.Length);
			}
			return sb.ToString();
		}

		/// <summary>
		/// Displays a horizontal list of toggles and returns the index of the selected item.
		/// all you have to do is check items[selected] to see what is selected.
		/// </summary>
		public static int ToggleList(int selected, GUIContent[] items, GUIStyle[] styles, float width)
		{
			// Keep the selected index within the bounds of the items array
			selected = selected < 0 ? 0 : selected >= items.Length ? items.Length - 1 : selected;

			GUILayout.BeginHorizontal();
			for (int i = 0; i < items.Length; i++)
			{
				// Display toggle. Get if toggle changed.
				bool change = GUILayout.Toggle(selected == i, items[i], styles[i], GUILayout.Width(width));
				// If changed, set selected to current index.
				if (change)
					selected = i;
			}
			GUILayout.EndHorizontal();

			// Return the currently selected item's index
			return selected;
		}

		//Returns True if the PauseMenu is open. Because the GameEvent callbacks don't work on the mainmenu.
		internal static bool isPauseMenuOpen 
		{
			get
			{
				try
				{
					
					return PauseMenu.isOpen;
				}
				catch(Exception)
				{
					return false;
				}
			}
		}
		/// <summary>
		///Will delete Screen Messages. If you pass in messagetext it will only delete messages that contain that text string.
		///If you pass in a messagearea it will only delete messages in that area. Values are: UC,UL,UR,LC,ALL
		/// </summary>
		/// <param name="messagetext">Specify a string that is part of a message that you want to remove, or pass in empty string to delete all messages</param>
		/// <param name="messagearea">Specify a string representing the message area of the screen that you want messages removed from, 
		/// or pass in "ALL" string to delete from all message areas. 
		/// messagearea accepts the values of "UC" - UpperCenter, "UL" - UpperLeft, "UR" - UpperRight, "LC" - LowerCenter, "ALL" - All Message Areas</param>
		internal static void DeleteScreenMessages(string messagetext, string messagearea)
		{
			//Get the ScreenMessages Instance
			var messages = ScreenMessages.Instance;
			List<ScreenMessagesText> messagetexts = new List<ScreenMessagesText>();
			//Get the message Area messages based on the value of messagearea parameter.
			switch (messagearea)
			{
				case "UC":
					messagetexts = messages.upperCenter.gameObject.GetComponentsInChildren<ScreenMessagesText>().ToList();
					break;
				case "UL":
					messagetexts = messages.upperLeft.gameObject.GetComponentsInChildren<ScreenMessagesText>().ToList();
					break;
				case "UR":
					messagetexts = messages.upperRight.gameObject.GetComponentsInChildren<ScreenMessagesText>().ToList();
					break;
				case "LC":
					messagetexts = messages.lowerCenter.gameObject.GetComponentsInChildren<ScreenMessagesText>().ToList();
					break;
				case "ALL":
					messagetexts = messages.gameObject.GetComponentsInChildren<ScreenMessagesText>().ToList();
					break;
			}
			//Loop through all the mesages we found.
			List<ScreenMessage> activemessagelist = ScreenMessages.Instance.ActiveMessages;
			foreach (var msgtext in messagetexts)
			{
				//If the user specified text to search for only delete messages that contain that text.
				if (messagetext != "")
				{
					if (msgtext != null && msgtext.text.text.Contains(messagetext))
					{
						Object.Destroy(msgtext.gameObject);
					}
				}
				else  //If the user did not specific a message text to search for we DELETE ALL messages!!
				{
					Object.Destroy(msgtext.gameObject);
				}
			}
		}

		#endregion GUI&Window

		#region ConfigNodes
		// Get Config Node Values out of a config node Methods

		internal static Guid GetNodeValue(ConfigNode confignode, string fieldname)
		{
			if (confignode.HasValue(fieldname))
			{
				try
				{
					Guid id = new Guid(confignode.GetValue(fieldname));
					return id;
				}
				catch (Exception ex)
				{
					Debug.Log("Unable to getNodeValue " + fieldname + " from " + confignode);
					Debug.Log("Err: " + ex);
					return Guid.Empty;
				}
			}
			return Guid.Empty;
		}

		internal static T GetNodeValue<T>(ConfigNode confignode, string fieldname, T defaultValue) where T : IComparable, IFormattable, IConvertible
		{
			if (confignode.HasValue(fieldname))
			{
				string stringValue = confignode.GetValue(fieldname);
				if (Enum.IsDefined(typeof(T), stringValue))
				{
					return (T)Enum.Parse(typeof(T), stringValue);
				}
			}
			return defaultValue;
		}
		#endregion ConfigNodes

		#region Time
		//Formatting time functions

		private static int y, d, h, m;
		private static List<string> parts = new List<string>();
		//Format a Time double variable into format "xxxx:year xxxx:days xxxx:hours xxxx:mins x:xx:secs"
		//Future expansion required to format to different formats.
		public static String formatTime(double seconds)
		{
			y = (int)(seconds / (6.0 * 60.0 * 60.0 * 426.08));
			seconds = seconds % (6.0 * 60.0 * 60.0 * 426.08);
			d = (int)(seconds / (6.0 * 60.0 * 60.0));
			seconds = seconds % (6.0 * 60.0 * 60.0);
			h = (int)(seconds / (60.0 * 60.0));
			seconds = seconds % (60.0 * 60.0);
			m = (int)(seconds / 60.0);
			seconds = seconds % 60.0;

			//List<string> parts = new List<string>();
			parts.Clear();

			if (y > 0)
			{
				parts.Add(String.Format("{0}:year ", y));
			}

			if (d > 0)
			{
				parts.Add(String.Format("{0}:days ", d));
			}

			if (h > 0)
			{
				parts.Add(String.Format("{0}:hours ", h));
			}

			if (m > 0)
			{
				parts.Add(String.Format("{0}:mins ", m));
			}

			if (seconds > 0)
			{
				parts.Add(String.Format("{0:00}:secs ", seconds));
			}

			if (parts.Count > 0)
			{
				return String.Join(" ", parts.ToArray());
			}
			return "0s";
		}

		private static string outputstring;
		private static int[] datestructure = new int[5];
		//Format a Time double variable into format "YxxxxDxxxhh:mm:ss"
		//Future expansion required to format to different formats.
		internal static string FormatDateString(double time)
		{
			outputstring = String.Empty;
			//int[] datestructure = new int[5];
			if (GameSettings.KERBIN_TIME)
			{
				datestructure[0] = (int)time / 60 / 60 / 6 / 426; // Years
				datestructure[1] = (int)time / 60 / 60 / 6 % 426; // Days
				datestructure[2] = (int)time / 60 / 60 % 6;    // Hours
				datestructure[3] = (int)time / 60 % 60;    // Minutes
				datestructure[4] = (int)time % 60; //seconds
			}
			else
			{
				datestructure[0] = (int)time / 60 / 60 / 24 / 365; // Years
				datestructure[1] = (int)time / 60 / 60 / 24 % 365; // Days
				datestructure[2] = (int)time / 60 / 60 % 24;    // Hours
				datestructure[3] = (int)time / 60 % 60;    // Minutes
				datestructure[4] = (int)time % 60; //seconds
			}
			if (datestructure[0] > 0)
				outputstring += "Y" + datestructure[0].ToString("####") + ":";
			if (datestructure[1] > 0)
				outputstring += "D" + datestructure[1].ToString("###") + ":";
			outputstring += datestructure[2].ToString("00:");
			outputstring += datestructure[3].ToString("00:");
			outputstring += datestructure[4].ToString("00");
			return outputstring;
		}

		// Electricity and temperature functions are only valid if timewarp factor is < 5.
		internal static bool timewarpIsValid(int max)
		{
			return TimeWarp.CurrentRateIndex < max;
		}

		internal static void stopWarp()
		{
			TimeWarp.SetRate(0, false);
		}

		#endregion Time

		#region Strings
		/// <summary>
		/// Removes a String A from String B.
		/// </summary>
		internal static string RemoveSubStr(string B, string A)
		{
			StringBuilder b = new StringBuilder(B);
			b.Replace(A, String.Empty);
			return b.ToString();
		}

		public enum ISRUStatus
		{
			Inactive,
			Active,
			MissingResource,
			OutputFull
		}

		private static ISRUStatus returnStatus;
		/// <summary>
		/// Returns a Status Indicating the Status of a ISRU ModuleResourceConverter, given that it's actual status can be active, but not actually doing anything.
		/// </summary>
		internal static ISRUStatus GetModResConverterStatus(ModuleResourceConverter tmpRegRc)
		{
			returnStatus = ISRUStatus.Inactive;
			if (!tmpRegRc.IsActivated) return ISRUStatus.Inactive; //If it's not Activated, it must be inactive.
			// Otherwise it's Activated, but is it really working and using EC? Get it's real status.
			if (tmpRegRc.status.ToLower().Contains("inactive")) returnStatus = ISRUStatus.Inactive; //Status is inactive, it's inactive.. Not sure how but sometimes this remains on load even when it's inactive? Hence the test above.
			if (tmpRegRc.status.ToLower().Contains("missing")) returnStatus = ISRUStatus.MissingResource; //Missing an Input resource makes this appear in the status.
			if (tmpRegRc.status.ToLower().Contains("full")) returnStatus = ISRUStatus.OutputFull; //If the vessel has nowhere to store the output, full appears in the status.
			if (tmpRegRc.status.ToLower().Contains("load")) returnStatus = ISRUStatus.Active; //a Percentage Load indicates it is active and actually processing... except when it gets stuck on this.
			return returnStatus;
		}

		#endregion Strings

		#region ModsInstalled

		private static Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

		internal static bool IsRTInstalled
		{
			get
			{
				return IsModInstalled("RemoteTech");
			}
		}

		internal static bool IsKopInstalled
		{
			get
			{
				return IsModInstalled("Kopernicus");
			}
		}

		internal static bool IsRSSInstalled
		{
			get
			{
				return IsModInstalled("RealSolarSystem");
			}
		}

		internal static bool IsResearchBodiesInstalled
		{
			get
			{
				return IsModInstalled("ResearchBodies");
			}
		}

		internal static bool IsTSTInstalled
		{
			get
			{
				return IsModInstalled("TarsierSpaceTech");
			}
		}

	    internal static bool IsEVEInstalled
	    {
	        get
	        {
	            return IsModInstalled("EVEManager");
	            
	        }
	    }

		internal static bool IsOPMInstalled
		{
			get
			{
				CelestialBody sarnus = FlightGlobals.Bodies.FirstOrDefault(a => a.name == "Sarnus");
				if (sarnus != null)
				{
					return true;
				}
				return false;
			}
		}

		internal static bool IsNHInstalled
		{
			get
			{
				CelestialBody sonnah = FlightGlobals.Bodies.FirstOrDefault(a => a.name == "Sonnah");
				if (sonnah != null)
				{
					return true;
				}
				return false;
			}
		}

		internal static bool IsModInstalled(string assemblyName)
		{
			Assembly assembly = (from a in assemblies
								 where a.FullName.Contains(assemblyName)
								 select a).FirstOrDefault();
			return assembly != null;
		}

		#endregion ModsInstalled

		#region Logging
		// Logging Functions
		// Name of the Assembly that is running this MonoBehaviour
		internal static String _AssemblyName
		{ get { return Assembly.GetExecutingAssembly().GetName().Name; } }
		
		internal static bool debuggingOn = false;
		
		/// <summary>
		/// Logging to the debug file
		/// </summary>
		/// <param name="Message">Text to be printed - can be formatted as per String.format</param>
		/// <param name="strParams">Objects to feed into a String.format</param>			
				
		internal static void Log_Debug(String Message, params object[] strParams)
		{
			if (debuggingOn)
			{
				Log("DEBUG: " + Message, strParams);
			}
		}
		
		/// <summary>
		/// Logging to the log file
		/// </summary>
		/// <param name="Message">Text to be printed - can be formatted as per String.format</param>
		/// <param name="strParams">Objects to feed into a String.format</param>
							
		internal static void Log(String Message, params object[] strParams)
		{
			Message = String.Format(Message, strParams);                  // This fills the params into the message
			String strMessageLine = String.Format("{0},{2},{1}",
				DateTime.Now, Message,
				_AssemblyName);                                           // This adds our standardised wrapper to each line
			Debug.Log(strMessageLine);                        // And this puts it in the log
		}
		#endregion Logging
	}
}