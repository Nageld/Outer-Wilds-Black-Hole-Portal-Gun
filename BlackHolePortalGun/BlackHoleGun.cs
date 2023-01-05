using OWML.ModHelper;
using OWML.Common;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;

namespace BlackHolePortalGun
{
    public class BlackHolePortalGun : ModBehaviour
    {
        public bool BlackPortalButton;
        public bool WhitePortalButton;
        public PortalGun gun = new PortalGun();

        public float range = 10000f;
        public Key Black;
        public Key White;
        public Key Clear;
        private void Awake()
        {
            // You won't be able to access OWML's mod helper in Awake.
            // So you probably don't want to do anything here.
            // Use Start() instead.
        }

        private void Start()
        {
            // Starting here, you'll have access to OWML's mod helper.
            ModHelper.Console.WriteLine($"Mod {nameof(BlackHolePortalGun)} is loaded!", MessageType.Success);
            ModHelper.HarmonyHelper.AddPrefix<WhiteHoleVolume>("Awake", typeof(BlackHolePortalGun), "OnWhiteHoleVolumeAwake");
            LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
            {
                if (loadScene != OWScene.SolarSystem) return;
                gun.setLogger(ModHelper);
            };
        }
        private void Update()
        {
            if (!OWInput.IsInputMode(InputMode.Menu))
            {
                BlackPortalButton = Keyboard.current[Black].wasPressedThisFrame;;
                WhitePortalButton = Keyboard.current[White].wasReleasedThisFrame;
            }

            if (BlackPortalButton || WhitePortalButton)
            {

                Vector3 fwd = transform.TransformDirection(Vector3.forward);


                if (Physics.Raycast(Locator.GetActiveCamera().transform.position, fwd, range))
                {
                    PlaceObjectRaycast(BlackPortalButton);
                }
            }
            if (Keyboard.current[Clear].wasReleasedThisFrame)
                gun.clear_portals();
        }


        void PlaceObjectRaycast(bool color)
        {
            if (IsPlaceable(out Vector3 placeNormal, out Vector3 placePoint, out OWRigidbody targetRigidbody))
            {
                if (color)
                {
                    gun.shoot_blackHole(placeNormal, placePoint, targetRigidbody);

                } else
                {
                    gun.shoot_whiteHole(placeNormal, placePoint, targetRigidbody);

                }
            }
        }

        bool IsPlaceable(out Vector3 placeNormal, out Vector3 placePoint, out OWRigidbody targetRigidbody)
        {
            placeNormal = Vector3.zero;
            placePoint = Vector3.zero;
            targetRigidbody = null;

            Vector3 forward = Locator.GetPlayerCamera().transform.forward;

            if (Physics.Raycast(Locator.GetPlayerCamera().transform.position, forward, out RaycastHit hit, range, OWLayerMask.physicalMask | OWLayerMask.interactMask))
            {
                placeNormal = hit.normal;
                placePoint = hit.point - forward;
                targetRigidbody = hit.collider.GetAttachedOWRigidbody(false);
                return true;
            }
            return false;
        }

        public override void Configure(IModConfig config)
        {
            this.range = config.GetSettingsValue<float>("Black Hole Gun Range");
            Black = (Key) System.Enum.Parse(typeof(Key), config.GetSettingsValue<string>("Black Hole")) ;
            White = (Key) System.Enum.Parse(typeof(Key), config.GetSettingsValue<string>("White Hole")) ;
            Clear = (Key) System.Enum.Parse(typeof(Key), config.GetSettingsValue<string>("Remove Holes")) ;
        }

        public static bool OnWhiteHoleVolumeAwake(WhiteHoleVolume __instance)
        {
            __instance._growQueue = new List<OWRigidbody>(8);
            __instance._growQueueLocationData = new List<RelativeLocationData>(8);
            __instance._ejectedBodyList = new List<OWRigidbody>(64);
            try
            {
                __instance._whiteHoleBody = __instance.gameObject.GetAttachedOWRigidbody(false);
                __instance._whiteHoleProxyShadowSuperGroup = __instance._whiteHoleBody.GetComponentInChildren<ProxyShadowCasterSuperGroup>();
                __instance._fluidVolume = __instance.gameObject.GetRequiredComponent<WhiteHoleFluidVolume>();
            }
            catch (Exception) { }
            return false;
        }

    }
   }

