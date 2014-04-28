using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Toolbar;

namespace SwitchActiveVessel
{
[KSPAddon(KSPAddon.Startup.Flight, false)]
public class SwitchActiveVessel : MonoBehaviour
{
    private HashSet<Vessel> activeVessels = new HashSet<Vessel>();
    private bool pluginActive = true;
    private Rect windowRect = new Rect();
    private Toolbar.IButton toolbarButton;

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
        GUI.DragWindow(new Rect(0, 0, 1000, 20));
    }
    private void drawGUI()
    {
        if (!this.pluginActive) return;
        var keepSkin = GUI.skin;
        GUI.skin = null;
        windowRect.height = 40;
        windowRect = GUILayout.Window(1, windowRect, WindowGUI, "Switch To");
        GUI.skin = keepSkin;
    }

    void Start()
    {
        RenderingManager.AddToPostDrawQueue(3, drawGUI);

        var config = KSP.IO.PluginConfiguration.CreateForType<SwitchActiveVessel>();
        config.load();
        windowRect.x = config.GetValue("window_x", 200);
        windowRect.y = config.GetValue("window_y", 30);
        pluginActive = config.GetValue("plugin_active", true);

        windowRect.width = 250;

        setupToolbar();

        GameEvents.onVesselGoOffRails.Add(ShipOnline);
        GameEvents.onVesselCreate.Add(ShipOnline);

        GameEvents.onVesselGoOnRails.Add(ShipOffline);
        GameEvents.onVesselDestroy.Add(ShipOffline);
    }

    void OnDestroy()
    {
        var config = KSP.IO.PluginConfiguration.CreateForType<SwitchActiveVessel>();
        config.SetValue("window_x", (int)windowRect.x);
        config.SetValue("window_y", (int)windowRect.y);
        config.SetValue("plugin_active", pluginActive);
        config.save();

        teardownToolbar();

        RenderingManager.RemoveFromPostDrawQueue(3, drawGUI);
        GameEvents.onVesselGoOffRails.Remove(ShipOnline);
        GameEvents.onVesselCreate.Remove(ShipOnline);

        GameEvents.onVesselGoOnRails.Remove(ShipOffline);
        GameEvents.onVesselDestroy.Remove(ShipOffline);
    }

    private void setupToolbar() {
        this.toolbarButton = Toolbar.ToolbarManager.Instance.add("switchvessel", "show");
        toolbarButton.TexturePath = "SwitchVessel/SwitchVessel";
        toolbarButton.ToolTip = "Toggle Quick Vessel Switching";
        toolbarButton.OnClick += e => pluginActive = !pluginActive;
    }

    private void teardownToolbar() {
        toolbarButton.Destroy();
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
