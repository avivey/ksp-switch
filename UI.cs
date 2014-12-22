using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace SwitchActiveVessel
{
class VesselFilterUi
{
    public SortedDictionary<VesselType, bool> filteredTypes ;
    public event Action interaction;

    public VesselFilterUi(SortedDictionary<VesselType, bool> filteredTypes) {
        this.filteredTypes = filteredTypes;
    }

    public static VesselFilterUi CreateSaneDefault() {
        var filteredTypes = new SortedDictionary<VesselType, bool>();
        filteredTypes[VesselType.Debris] = false;
        filteredTypes[VesselType.Probe] = true;
        filteredTypes[VesselType.Rover] = true;
        filteredTypes[VesselType.Lander] = true;
        filteredTypes[VesselType.Ship] = true;
        filteredTypes[VesselType.Station] = true;
        filteredTypes[VesselType.Base] = true;
        filteredTypes[VesselType.EVA] = true;
        filteredTypes[VesselType.Flag] = true;
        return new VesselFilterUi(filteredTypes);
    }

    public bool isVesselTypeEnabled(VesselType type) {
        var enabled = false;
        filteredTypes.TryGetValue(type, out enabled);
        return enabled;
    }

    void toggle(VesselType type) {
        bool oldv = filteredTypes[type];
        filteredTypes[type] = !oldv;
    }

    public void AddFilterToActiveGUI() {
        var originalColor = GUI.color;
        GUILayout.BeginHorizontal();

        foreach (var entry in filteredTypes) {
            var type = entry.Key;
            var enabled = entry.Value;
            GUI.color = enabled ? originalColor : Color.black;
            if (GUILayout.Button("  ")) {
                toggle(type);
                interaction();
            }
            var button = GUILayoutUtility.GetLastRect();
            UiUtils.DrawOrbitIcon(button, type);
        }

        GUILayout.EndHorizontal();
        GUI.color = originalColor;
    }
}

class UiUtils
{
    static Dictionary<VesselType, Rect> OrbitIconLocation;

    static UiUtils() {
        OrbitIconLocation = new  Dictionary<VesselType, Rect> ();
        // new Rect(left, top, width, height). Count from bottom left point.
        // List was created in 0.23.5.
        OrbitIconLocation[VesselType.Debris] = new Rect(0.2f, 0.6f, 0.2f, 0.2f);
        OrbitIconLocation[VesselType.SpaceObject] = new Rect(0.8f, 0.2f, 0.2f, 0.2f);
        OrbitIconLocation[VesselType.Unknown] = new Rect(0.6f, 0.6f, 0.2f, 0.2f);
        OrbitIconLocation[VesselType.Probe] = new Rect(0.2f, 0f, 0.2f, 0.2f);
        OrbitIconLocation[VesselType.Rover] = new Rect(0f, 0f, 0.2f, 0.2f);
        OrbitIconLocation[VesselType.Lander] = new Rect(0.6f, 0f, 0.2f, 0.2f);
        OrbitIconLocation[VesselType.Ship] = new Rect(0f, 0.6f, 0.2f, 0.2f);
        OrbitIconLocation[VesselType.Station] = new Rect(0.6f, 0.2f, 0.2f, 0.2f);
        OrbitIconLocation[VesselType.Base] = new Rect(0.4f, 0.0f, 0.2f, 0.2f);
        OrbitIconLocation[VesselType.EVA] = new Rect(0.4f, 0.4f, 0.2f, 0.2f);
        OrbitIconLocation[VesselType.Flag] = new Rect(0.8f, 0.0f, 0.2f, 0.2f);
    }
    public static void DrawOrbitIcon(Rect target, VesselType type) {
        GUI.DrawTextureWithTexCoords(target,
                                     MapView.OrbitIconsMap,
                                     OrbitIconLocation[type]);
    }
}
}
