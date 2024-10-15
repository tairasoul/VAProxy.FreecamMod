using System.Collections;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
//using HarmonyLib;
//using System.Linq;

/*
	additions:
	[Info   :   Freecam] OnTrigger Cube (2) (OnTrigger) on Cube (2) (UnityEngine.GameObject) (World/Areas/ERI Grave/LOD1/tunnel/Cube (1)/Cube (2)) entered.
	[Info   :   Freecam] OnTrigger Cube (1) (OnTrigger) on Cube (1) (UnityEngine.GameObject) (World/Areas/ERI Grave/LOD1/tunnel/Cube (1)/Cube (1)) entered.
	[Info   :   Freecam] OnTrigger Cube (2) (OnTrigger) on Cube (2) (UnityEngine.GameObject) (World/Areas/ERI Grave/LOD1/tunnel/Cube/Cube (2)) entered.
	[Info   :   Freecam] OnTrigger Cube (1) (OnTrigger) on Cube (1) (UnityEngine.GameObject) (World/Areas/ERI Grave/LOD1/tunnel/Cube/Cube (1)) entered.
	[Info   :   Freecam] OnTrigger Cube (1) (OnTrigger) on Cube (1) (UnityEngine.GameObject) (World/Areas/ERI Grave/LOD1/tunnel (1)/Cube (1)/Cube (1)) entered.
	[Info   :   Freecam] OnTrigger Cube (6) (OnTrigger) on Cube (6) (UnityEngine.GameObject) (World/Areas/SewerGateway/Cube (6)) entered.
	[Info   :   Freecam] OnTrigger Cube (5) (OnTrigger) on Cube (5) (UnityEngine.GameObject) (World/Areas/SewerGateway/Cube (5)) entered.
	[Info   :   Freecam] OnTrigger Cube (3) (OnTrigger) on Cube (3) (UnityEngine.GameObject) (World/Areas/ERI Grave/LOD1/Bridge_Double_Straight/Cube (3)) entered.
	[Info   :   Freecam] OnTrigger Cube (3) (OnTrigger) on Cube (3) (UnityEngine.GameObject) (World/Areas/ERI Grave/LOD1/Bridge_Double_Straight/Cube (3)) entered.
	[Info   :   Freecam] OnTrigger Cube (3) (OnTrigger) on Cube (3) (UnityEngine.GameObject) (World/Areas/ERI Grave/LOD1/Bridge_Double_Straight/Cube (3)) entered.
*/

namespace FreecamMod 
{
	[BepInPlugin("tairasoul.vaproxy.freecam", "Freecam", "1.0.2")]
	class Plugin : BaseUnityPlugin
	{
		Rect windowRect = new(20, 20, 200, 100);
		Rect infoRect = new(2, 979, 200, 48);
		ConfigEntry<KeyCode> activate;
		ConfigEntry<KeyCode> activateInfo;
		internal static ManualLogSource Log;
		bool active = false;
		bool infoActive = false;
		bool infoHeld = false;
		bool activeHeld = false;
		Freecam? freecam;
		Scene currentScene;
		void Awake() 
		{
			Log = Logger;
			activate = Config.Bind("Keybinds", "Activate Freecam Window", KeyCode.F9, "The keycode that activates the freecam window.");
			activateInfo = Config.Bind("Keybinds", "Activate Info Window", KeyCode.F10, "The keycode that activates the info window.");
			SceneManager.activeSceneChanged += SceneLoaded;
			//Harmony a = new("g");
			//a.PatchAll();
		}
		void OnGUI()
		{
			if (active) 
				windowRect = GUILayout.Window(1, windowRect, DrawWindow, "Freecam");
			if (fcActive && infoActive)
				infoRect = GUI.Window(2, infoRect, DrawInfo, "FreecamInfo");
		}
		
		void SceneLoaded(Scene old, Scene newS) 
		{
			currentScene = newS;
			if (newS.name != "Intro" && newS.name != "Menu") 
			{
				StartCoroutine(SetupFreecamElement());
			}
		}
		
		IEnumerator SetupFreecamElement() 
		{
			while (true)
			{
				GameObject TPC = GameObject.Find("TPC");
				if (TPC) 
				{
					freecam = TPC.GetComponent<Freecam>() ?? TPC.AddComponent<Freecam>();
					break;
				}
				yield return null;
			}
			yield return null;
		}
		
		bool fcActive = false;
		
		void DrawInfo(int windowId) 
		{
			if (freecam != null) {
				GUILayout.Label($"Freecam Speed: {freecam.desiredMoveSpeed}");
			}
			GUI.DragWindow(new Rect(0, 0, 5000000, 50000));
		}
		
		void DrawWindow(int windowId) 
		{
			fcActive = GUILayout.Toggle(fcActive, "Toggle Freecam");
			GUI.DragWindow(new Rect(0, 0, 5000000, 50000));
		}
		
		CursorLockMode oldLock;
		bool oldVis = true;
		
		void Update() 
		{
			if (freecam != null) 
			{
				if (fcActive && !freecam.freecamActive) 
					freecam.setActive(true);
				else if (!fcActive && freecam.freecamActive)
					freecam.setActive(false);
			}
		}
		
		void LateUpdate() 
		{
			if (UnityInput.Current.GetKeyDown(activate.Value)) 
			{
				if (!activeHeld) 
				{
					activeHeld = true;
					active = !active;
					if (active || currentScene.name == "Menu")
					{
						Log.LogInfo("Unlocking cursor.");
						oldLock = Cursor.lockState;
						oldVis = Cursor.visible;
						Cursor.lockState = CursorLockMode.None;
						Cursor.visible = true;
					}
					else
					{
						Log.LogInfo("Locking cursor.");
						Cursor.lockState = oldLock;
						Cursor.visible = oldVis;
					}
				}
			}
			else 
			{
				activeHeld = false;
			}
			if (UnityInput.Current.GetKeyDown(activateInfo.Value)) 
			{
				if (!infoHeld) 
				{
					infoHeld = true;
					infoActive = !infoActive;
				}
			}
			else 
			{
				infoHeld = false;
			}
		}
	}
	
	/*[HarmonyPatch(typeof(OnTrigger))]
	static class OnTriggerPatch
	{
		[HarmonyPrefix]
		[HarmonyPatch("OnTriggerEnter")]
		static void g(OnTrigger __instance) 
		{
			Plugin.Log.LogInfo($"OnTrigger {__instance} on {__instance.gameObject} ({ConstructParentPath(__instance)}) entered.");
		}
		
		static string ConstructParentPath(MonoBehaviour instance) 
		{
			Transform[] path = {};
			Transform current = instance.transform;
			path = path.AddItem(current).ToArray();
			while (true) 
			{
				current = current.parent;
				if (current) 
				{
					path = path.AddItem(current).ToArray();
				}
				else 
				{
					break;
				}
			}
			Transform[] reversed = path.Reverse().ToArray();
			string pathstr = "";
			foreach (Transform transform in reversed) 
			{
				pathstr += $"/{transform.name}";
			}
			pathstr = pathstr.Substring(1);
			return pathstr;
		}
	}*/
}