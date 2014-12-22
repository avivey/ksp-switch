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
    private IEnumerable<Vessel> activeVessels = new Vessel[0];
    private bool pluginActive = true;
    private Rect windowRect = new Rect();

    private bool updateNeeded = true;
    private void scheduleUpdate(object __) {
        this.updateNeeded = true;
    }

    void Update() {
        if (!pluginActive) return;
        if (updateNeeded) {
            windowRect.height = 40;
            activeVessels = FlightGlobals.Vessels.Where(
                                v => v.loaded &&
                                vesselFilter.isVesselTypeEnabled(v.vesselType));
            updateNeeded = false;
        }
    }

    private Vessel highlightedVessel = null;
    private void highlight(Vessel vessel) {
        if (highlightedVessel != null && highlightedVessel != vessel) {
            highlightedVessel.rootPart.SetHighlightDefault();
        }

        highlightedVessel = vessel;

        if (highlightedVessel != null) {
            var p = highlightedVessel.rootPart;
            p.SetHighlightColor(Color.green);
            p.SetHighlight(true, true);
        }
    }

    private void jumpToVessel(Vessel vessel) {
        FlightGlobals.ForceSetActiveVessel(vessel);
    }

    private VesselFilterUi vesselFilter  = VesselFilterUi.CreateSaneDefault();

    private void WindowGUI(int windowID)
    {
        GUILayout.BeginVertical();

        vesselFilter.AddFilterToActiveGUI();

        Vessel clickedVessel = null;
        Vessel hoverVessel = null;
        foreach (var vessel in activeVessels) {

            if (GUILayout.Button(vessel.GetName()))
                clickedVessel = vessel;
            if (Event.current.type == EventType.Repaint
                    && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                hoverVessel = vessel;
        }
        if (clickedVessel != null)
            jumpToVessel(clickedVessel);

        highlight(hoverVessel);

#if DEBUG
        if (GUILayout.Button("clear log"))
            debugLog .Clear();
        GUILayout.Label(string.Join("\n", debugLog.ToArray()));
#endif
        GUILayout.EndVertical();
        GUI.DragWindow(new Rect(0, 0, 1000, 20));
    }
    private void drawGUI()
    {
        if (!this.pluginActive) return;
        var keepSkin = GUI.skin;
        GUI.skin = null;
        windowRect = GUILayout.Window(1, windowRect, WindowGUI, "Switch To");
        GUI.skin = keepSkin;
    }

    void Start()
    {
        RenderingManager.AddToPostDrawQueue(3, drawGUI);

        vesselFilter.interaction += () => scheduleUpdate(null);

        var config = KSP.IO.PluginConfiguration.CreateForType<SwitchActiveVessel>();
        config.load();
        windowRect.x = config.GetValue("window_x", 200);
        windowRect.y = config.GetValue("window_y", 30);
        pluginActive = config.GetValue("plugin_active", true);

        windowRect.width = 250;

        setupToolbar();

        GameEvents.onVesselGoOffRails.Add(scheduleUpdate);
        GameEvents.onVesselCreate.Add(scheduleUpdate);
        GameEvents.onVesselLoaded.Add(scheduleUpdate);
        GameEvents.onVesselWasModified.Add(scheduleUpdate);
        GameEvents.onVesselChange.Add(scheduleUpdate);
        GameEvents.onVesselGoOnRails.Add(scheduleUpdate);
        GameEvents.onVesselDestroy.Add(scheduleUpdate);
    }

    EventData<Vessel>.OnEvent debug(String text, Action<Vessel> func) {
        return (v) => {
            print(text);
            func(v);
        };
    }

    EventData<T>.OnEvent debug<T>(String text) {
        return (v) => {
            print(text);
        };
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
        GameEvents.onVesselGoOffRails.Remove(scheduleUpdate);
        GameEvents.onVesselCreate.Remove(scheduleUpdate);
        GameEvents.onVesselLoaded.Remove(scheduleUpdate);
        GameEvents.onVesselWasModified.Remove(scheduleUpdate);
        GameEvents.onVesselChange.Remove(scheduleUpdate);
        GameEvents.onVesselGoOnRails.Remove(scheduleUpdate);
        GameEvents.onVesselDestroy.Remove(scheduleUpdate);
    }

    private Toolbar.IButton toolbarButton;
    private void setupToolbar() {
        this.toolbarButton = Toolbar.ToolbarManager.Instance.add("switchvessel", "show");
        toolbarButton.TexturePath = "SwitchVessel/SwitchVessel";
        toolbarButton.ToolTip = "Toggle Quick Vessel Switching";
        toolbarButton.OnClick += e => pluginActive = !pluginActive;
    }

    private void teardownToolbar() {
        toolbarButton.Destroy();
    }

    void print(string text) {
        MonoBehaviour.print("SwitchActive: " + text);
#if DEBUG
        debugLog.Add(text);
        if (debugLog.Count > 40) {
            debugLog.RemoveRange(0, 35);
        }
#endif
    }
#if DEBUG
    private List<string> debugLog = new List<string>();
#endif

}

}
