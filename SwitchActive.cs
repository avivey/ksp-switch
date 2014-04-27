using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace SwitchActiveVessel
{
[KSPAddon(KSPAddon.Startup.Flight, false)]
public class SwitchActiveVessel : MonoBehaviour
{
    // private SortedSet<Vessel> activeVessels = new SortedSet<Vessel>(new VesselSorter()); need newer vm?
    private HashSet<Vessel> activeVessels = new HashSet<Vessel>();
    private bool pluginActive = true;

    private void ShipOffline(Vessel vessel) {
        var removed = activeVessels.Remove(vessel);
        if (removed != true) {
            print("tried to remove " + vessel.GetName() + ", but failed");
        }
    }
    private void ShipOnline(Vessel vessel) {
        activeVessels.Add(vessel);
    }

    private Vessel highlightedVessel = null;
    private void highlight(Vessel vessel) {
        if (highlightedVessel != null && highlightedVessel != vessel) {
            highlightedVessel.Parts.ForEach(p => p.SetHighlightDefault());
        }

        highlightedVessel = vessel;

        if (highlightedVessel != null) {
            highlightedVessel.Parts.ForEach(p => {
                p.SetHighlightColor(Color.green);
                p.SetHighlight(true);
            });
        }
    }

    private void jumpToVessel(Vessel vessel) {
        FlightGlobals.ForceSetActiveVessel(vessel); // This is more like hittting `]`.
    }


    public static Dictionary<Vessel, List<GameObject>> meshListLookup = new Dictionary<Vessel, List<GameObject>>();
    public static Dictionary<GameObject, ProtoPartSnapshot> referencePart = new Dictionary<GameObject, ProtoPartSnapshot>();
    public static List<Vessel> watchList = new List<Vessel>();


    Rect windowRect = new Rect(200, 30, 250, 0);
    VesselFilterUi vesselFilter = VesselFilterUi.CreateSaneDefault(); // move to ui class
    private void WindowGUI(int windowID)
    {
        GUILayout.BeginVertical();

        vesselFilter.AddFilterToActiveGUI();

        Vessel clickedVessel = null;
        Vessel hoverVessel = null;
        foreach (var vessel in activeVessels) {
            if (!vesselFilter.isVesselTypeEnabled(vessel.vesselType))
                continue;

            if (GUILayout.Button(vessel.GetName()))
                clickedVessel = vessel;
            if (Event.current.type == EventType.Repaint
                    && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                hoverVessel = vessel;
        }
        if (clickedVessel != null)
            jumpToVessel(clickedVessel);

        highlight(hoverVessel);

        GUILayout.EndVertical();
        GUI.DragWindow();
    }
    private void drawGUI()
    {
        if (!this.pluginActive) return;
        var keepSkin = GUI.skin;
        GUI.skin = null;
        windowRect.height = 40;
        windowRect = GUILayout.Window(1, windowRect, WindowGUI, "");
        GUI.skin = keepSkin;
    }

    void Start()
    {
        RenderingManager.AddToPostDrawQueue(3, drawGUI);

        GameEvents.onVesselGoOffRails.Add(ShipOnline);
        GameEvents.onVesselCreate.Add(ShipOnline);

        GameEvents.onVesselGoOnRails.Add(ShipOffline);
        GameEvents.onVesselDestroy.Add(ShipOffline);
    }

    void OnDestroy()
    {
        RenderingManager.RemoveFromPostDrawQueue(3, drawGUI);
        GameEvents.onVesselGoOnRails.Remove(ShipOffline);
        GameEvents.onVesselGoOffRails.Remove(ShipOnline);
    }

    private static double doubleValue(ConfigNode node, string key) {
        double v = 0d;
        Double.TryParse(node.GetValue(key), out v);
        return v;
    }

    void print(string text) {
        MonoBehaviour.print("SwitchActive: " + text);
    }
}

}
