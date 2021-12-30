using Astrum.AstralCore.UI.Attributes;
using MelonLoader;
using System;
using UnityEngine;
using VRC.SDKBase;

[assembly: MelonInfo(typeof(Astrum.AstralXUtils), "AstralXUtils", "0.1.0", downloadLink: "github.com/Astrum-Project/AstralXUtils")]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonColor(ConsoleColor.DarkMagenta)]

namespace Astrum
{
    public class AstralXUtils : MelonMod
    {
        public static bool validHit = false;
        public static RaycastHit hit;
        private static GameObject prev;

        private static bool state = false;
        [UIProperty<bool>("XUtils", "Enabled")]
        public static bool State { 
            get => state;
            set
            {
                lr.enabled = state = value;
                if (value) validHit = false;
            }
        }

        private static LineRenderer lr;

        public override void OnSceneWasLoaded(int index, string _)
        {
            validHit = false;

            if (index != 0) return;

            GameObject puppet = new("AstralXUtils-LineRenderer");
            UnityEngine.Object.DontDestroyOnLoad(puppet);

            lr = puppet.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Legacy Shaders/Particles/Additive"));
            lr.startWidth = 0.02f;
            lr.endWidth = 0.02f;
            lr.useWorldSpace = false;
            lr.SetPosition(0, Vector3.zero);
            lr.SetPosition(1, Vector3.forward * 1000);
            lr.endColor = lr.startColor = new Color32(0x56, 0x00, 0xA5, 0xFF);
            lr.enabled = false;
        }

        // todo: support wrong handed people
        public override void OnApplicationLateStart() => targetBone = UnityEngine.XR.XRDevice.isPresent ? VRCPlayerApi.TrackingDataType.LeftHand : VRCPlayerApi.TrackingDataType.Head;
        private static VRCPlayerApi.TrackingDataType targetBone;

        // todo: optimize this
        public override void OnUpdate()
        {
            if (!state) return;

            // todo: cache and read directly from the hand
            VRCPlayerApi.TrackingData tt = Networking.LocalPlayer.GetTrackingData(targetBone);

            lr.transform.position = tt.position;
            lr.transform.rotation = tt.rotation;

            bool hasHit;
            if (hasHit = Physics.Raycast(lr.transform.position, lr.transform.forward, out hit, 1000f, -1, QueryTriggerInteraction.Collide))
            {
                if (prev != hit.collider.gameObject)
                {
                    Highlight(prev, false);
                    prev = hit.collider.gameObject;
                    Highlight(prev, true);
                }
            } 
            else
            {
                if (prev != null)
                {
                    Highlight(prev, false);
                    prev = null;
                }
            }

            if (Input.GetAxis("Oculus_CrossPlatform_PrimaryIndexTrigger") > 0.75f || Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (hasHit)
                {
                    validHit = true;
                    AstralCore.Logger.Notif("Selected: " + hit.collider.gameObject.name);
                    Highlight(prev, false);
                    prev = null;
                }

                State = false;
            }
        }

        private static void Highlight(GameObject go, bool state) => HighlightsFX.prop_HighlightsFX_0.Method_Public_Void_Renderer_Boolean_0(go?.GetComponent<Renderer>(), state);

        [UIButton("XUtils", "Destroy")]
        public static void Destroy()
        {
            if (!validHit)
            {
                AstralCore.Logger.Notif("You do not have a valid hit");
                return;
            }

            validHit = false;
            UnityEngine.Object.Destroy(hit.collider.gameObject);
        }

        [UIButton("XUtils", "ToggleActive")]
        public static void ToggleActive()
        {
            if (!validHit)
            {
                AstralCore.Logger.Notif("You do not have a valid hit");
                return;
            }

            hit.collider.gameObject.active ^= true;
        }

        [UIButton("XUtils", "ToggleCollision")]
        public static void ToggleCollision()
        {
            if (!validHit)
            {
                AstralCore.Logger.Notif("You do not have a valid hit");
                return;
            }

            hit.collider.enabled ^= true;
        }

        [UIButton("XUtils", "Teleport")]
        public static void Teleport()
        {
            if (!validHit)
            {
                AstralCore.Logger.Notif("You do not have a valid hit");
                return;
            }

            Networking.LocalPlayer.gameObject.transform.position = hit.point;
        }
    }
}
