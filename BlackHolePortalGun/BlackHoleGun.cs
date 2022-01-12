using OWML.ModHelper;
using OWML.Common;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackHolePortalGun
{
    public class BlackHolePortalGun : ModBehaviour
    {
        public bool BlackPortalButton = false;
        public bool WhitePortalButton = false;
        PlayerBody player;
        public PortalGun gun = new PortalGun();

        private void Awake()
        {
            // You won't be able to access OWML's mod helper in Awake.
            // So you probably don't want to do anything here.
            // Use Start() instead.
        }

        private void Start()
        {
            // Starting here, you'll have access to OWML's mod helper.
            ModHelper.Console.WriteLine($"My mod {nameof(BlackHolePortalGun)} is loaded!", MessageType.Success);

            LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
            {
                if (loadScene != OWScene.SolarSystem) return;
                player = FindObjectOfType<PlayerBody>();
                gun.setLogger(ModHelper);
                ModHelper.Console.WriteLine($"Found player body, and it's called {player.name}!", MessageType.Success);
            };
        }
        private void Update()
        {
            if (!OWInput.IsInputMode(InputMode.Menu))
            {
                BlackPortalButton = Keyboard.current[Key.B].wasReleasedThisFrame;
                WhitePortalButton = Keyboard.current[Key.G].wasReleasedThisFrame;
                // Don't check this if we are in the signalscope with multiple frequencies available or if we have the probe launcher equiped but not in the ship (same button as change freq/photo mode)
                var flag1 = (Locator.GetToolModeSwapper().GetToolMode() == ToolMode.SignalScope) && PlayerData.KnowsMultipleFrequencies();
                var flag2 = (Locator.GetToolModeSwapper().GetToolMode() == ToolMode.Probe && !PlayerState.AtFlightConsole());
                if (!flag1 && !flag2)
                    BlackPortalButton |= OWInput.IsNewlyReleased(InputLibrary.toolOptionRight);
            }

            if (BlackPortalButton || WhitePortalButton)
            {
                ModHelper.Console.WriteLine($"TEST {player.name}!", MessageType.Success);

                Vector3 fwd = transform.TransformDirection(Vector3.forward);

                ModHelper.Console.WriteLine($"Player Position {player.GetPosition()}");

                if (Physics.Raycast(Locator.GetActiveCamera().transform.position, fwd, 10000f))
                {
                    PlaceObjectRaycast(BlackPortalButton);
                }

            }
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

            if (Physics.Raycast(Locator.GetPlayerCamera().transform.position, forward, out RaycastHit hit, 10000f, OWLayerMask.physicalMask | OWLayerMask.interactMask))
            {
                placeNormal = hit.normal;
                placePoint = hit.point - forward;
                targetRigidbody = hit.collider.GetAttachedOWRigidbody(false);
                return true;
            }
            return false;
        }

    }
   }

