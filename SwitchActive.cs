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
    string debugLabel = "";
    private bool pluginActive = true;

    void say(Vessel target, string verb) {
        string s = verb + " Vessel: " +
                   (target == null ? "NULL" : target.GetName())
                   + "\n";
        debugLabel += s;
        print(s);
    }
    private void ShipOnRails(Vessel vessel) {
        var removed = activeVessels.Remove(vessel);
        if (removed != true) {
            print("tried to remove " + vessel.GetName() + ", but failed");
        }
        say(vessel, "on rails");
    }
    private void ShipOffRails(Vessel vessel) {
        activeVessels.Add(vessel);
        say(vessel, "off rails");
    }

    private void ShipLoaded(Vessel vessel) {
        say(vessel, "loaded");
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


    public static Dictionary<Vessel, List<GameObject>> meshListLookup = new Dictionary<Vessel, List<GameObject>>();
    public static Dictionary<GameObject, ProtoPartSnapshot> referencePart = new Dictionary<GameObject, ProtoPartSnapshot>();
    public static List<Vessel> watchList = new List<Vessel>();


    Rect windowRect = new Rect(200, 30, 250, 0);
    private void WindowGUI(int windowID)
    {
        GUILayout.BeginVertical();


        Vessel clickedVessel = null;
        Vessel hoverVessel = null;
        foreach (var vessel in activeVessels) {
            if (GUILayout.Button(vessel.GetName()))
                clickedVessel = vessel;
            if (Event.current.type == EventType.Repaint
                    && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                hoverVessel = vessel;

        }
        if (clickedVessel != null) {
            say(clickedVessel, "click");
        }

        highlight(hoverVessel);

        GUILayout.Label(this.debugLabel);
        GUILayout.EndVertical();
        GUI.DragWindow();
    }
    private void drawGUI()
    {
        if (this.pluginActive)
            windowRect = GUILayout.Window(1, windowRect, WindowGUI, "");
    }

    void Awake()
    {
        print("am awake");
    }

    void Start()
    {
        print("pre start");
        RenderingManager.AddToPostDrawQueue(3, drawGUI);
        GameEvents.onVesselLoaded.Add(ShipLoaded);
        GameEvents.onVesselGoOnRails.Add(ShipOnRails);
        GameEvents.onVesselGoOffRails.Add(ShipOffRails);
    }

    void OnDestroy()
    {
        RenderingManager.RemoveFromPostDrawQueue(3, drawGUI);
        GameEvents.onVesselGoOnRails.Remove(ShipOnRails);
        GameEvents.onVesselGoOffRails.Remove(ShipOffRails);
        GameEvents.onVesselLoaded.Remove(ShipLoaded);
        print("post destroy");
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

class VesselSorter : IComparer<Vessel>
{
    public int Compare(Vessel x, Vessel y) {
        return x.id.CompareTo(y);
    }
}

}
