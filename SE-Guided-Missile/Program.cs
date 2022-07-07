using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        private List<IMyCameraBlock> cameras = new List<IMyCameraBlock>();
        private List<IMyTextSurfaceProvider> surfaceProviders = new List<IMyTextSurfaceProvider>();
        private List<IMyGyro> gyros = new List<IMyGyro>();
        private List<IMyRadioAntenna> antennas = new List<IMyRadioAntenna>();
        private List<IMyTerminalBlock> terminals = new List<IMyTerminalBlock>();
        private List<IMyThrust> thrusters = new List<IMyThrust>();
        private List<IMyWarhead> warheads = new List<IMyWarhead>();

        private IMyCameraBlock spotter;
        private bool cameraTargetLock = false;
        private MyDetectedEntityInfo cameraTarget;
        private float targetYaw;
        private float targetPitch;
        private bool aimed = false;
        private bool warheadsTriggered = false;

        private float thinPitchAngle;
        private float thinYawAngle;

        private CameraArrayCellAngle[] angles;
        private float coneLimit;
        private float arrayCellLimit;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update1;

            GridTerminalSystem.GetBlocksOfType(cameras);
            GridTerminalSystem.GetBlocksOfType(surfaceProviders);
            GridTerminalSystem.GetBlocksOfType(gyros);
            GridTerminalSystem.GetBlocksOfType(antennas);
            GridTerminalSystem.GetBlocksOfType(terminals);
            GridTerminalSystem.GetBlocksOfType(thrusters);
            GridTerminalSystem.GetBlocksOfType(warheads);

            foreach (var camera in cameras)
            {
                camera.EnableRaycast = true;
            }

            angles = new CameraArrayCellAngle[cameras.Count];

            coneLimit = cameras.First().RaycastConeLimit;

            arrayCellLimit = (2 * coneLimit) / angles.Length;

            for (int x = 0; x < angles.Length; x++)
            {
                var initialValue = -coneLimit + (arrayCellLimit * x);

                angles[x] = new CameraArrayCellAngle()
                {
                    InitialPitch = initialValue,
                    InitialYaw = initialValue,
                    MaximumPitch = initialValue + arrayCellLimit,
                    MaximumYaw = initialValue + arrayCellLimit,
                    MinimumPitch = initialValue,
                    MinimumYaw = initialValue
                };
            }

            foreach (var gyro in gyros)
            {
                gyro.GyroOverride = true;
                gyro.GyroPower = 0.01f;
            }
        }

        public void Save()
        {
            // Called when the program needs to save its state. Use
            // this method to save your state to the Storage field
            // or some other means. 
            // 
            // This method is optional and can be removed if not
            // needed.
        }

        public void Main(string argument, UpdateType updateSource)
        {
            if (cameraTargetLock)
            {
                ThinCameraSeek();
            }
            else
            {
                CameraSeek();
            }
        }

        private void CameraSeek()
        {
            for (int camIndex = 0; camIndex < cameras.Count; camIndex++)
            {
                var camera = cameras[camIndex];
                var angle = angles[camIndex];

                var distanceLimit = camera.AvailableScanRange;

                var result = camera.Raycast(120, angle.InitialPitch, angle.InitialYaw);
                foreach (var surface in surfaceProviders.Where(s => s != null && s.SurfaceCount > 0))
                {
                    var srf = surface.GetSurface(0);
                    if (srf != null)
                    {
                        if (result.Type == MyDetectedEntityType.SmallGrid)
                        {
                            cameraTargetLock = true;

                            cameraTarget = result;

                            spotter = camera;

                            targetPitch = angle.InitialPitch;
                            targetYaw = angle.InitialYaw;

                            srf.WriteText($"Target spotted! Cam: {camIndex}{Environment.NewLine}" +
                                          $"P: {angle.InitialPitch}{Environment.NewLine}" +
                                          $"Y: {angle.InitialYaw}{Environment.NewLine}");

                            //AimAtTarget(targetYaw, targetPitch);

                            thinPitchAngle = targetPitch;
                            thinYawAngle = targetYaw;

                            return;
                        }
                        else
                        {
                            srf.WriteText($"Cam {camIndex} Scanning P: {angle.InitialPitch} Y: {angle.InitialYaw}" +
                                          $"{Environment.NewLine}{result.Type}" +
                                          $"{Environment.NewLine}{result.Relationship}" +
                                          $"{Environment.NewLine}{DateTime.Now}" +
                                          $"{Environment.NewLine}Seeker limit: {distanceLimit}" +
                                          $"{Environment.NewLine}Cone limit: {camera.RaycastConeLimit}");
                        }
                    }
                }

                angle.InitialYaw++;

                if (angle.InitialYaw > angle.MaximumYaw)
                {
                    angle.InitialYaw = angle.MinimumYaw;

                    angle.InitialPitch++;
                    if (angle.InitialPitch > angle.MaximumPitch)
                    {
                        angle.InitialPitch = angle.MinimumPitch;
                    }
                }
            }

        }

        private void ThinCameraSeek()
        {
            var result = spotter.Raycast(120, thinPitchAngle, thinYawAngle);

            foreach (var surface in surfaceProviders.Where(s => s != null && s.SurfaceCount > 0))
            {
                var srf = surface.GetSurface(0);
                if (srf != null)
                {
                    if (result.Type == MyDetectedEntityType.SmallGrid)
                    {
                        srf.WriteText($"Chasing target!" +
                                      $"{Environment.NewLine}P: {thinPitchAngle}" +
                                      $"{Environment.NewLine}Y: {thinYawAngle}" +
                                      $"{Environment.NewLine}{DateTime.Now}" +
                                      $"{Environment.NewLine}Time differential: {result.TimeStamp}");

                        targetYaw = thinYawAngle;
                        targetPitch = thinPitchAngle;

                        AimAtTarget(targetYaw, targetPitch);
                    }
                }
            }

            thinYawAngle++;

            if (thinYawAngle > targetYaw + 1)
            {
                thinYawAngle = targetYaw - 1;

                thinPitchAngle++;
                if (thinPitchAngle > targetPitch + 1)
                {
                    thinPitchAngle = targetPitch - 1;
                }
            }
        }

        private void AimAtTarget(float yaw, float pitch)
        {
            float appliedYaw = (yaw >= -1 && yaw <= 1) ? 0 : (0.01f * yaw / Math.Abs(yaw));
            float appliedPitch = (pitch >= -1 && pitch <= 1) ? 0 : (0.01f * -pitch / Math.Abs(pitch));
            foreach (var gyro in gyros)
            {
                gyro.Yaw = appliedYaw;
                gyro.Pitch = appliedPitch;
            }

            if (appliedYaw == 0 && appliedPitch == 0)
            {
                foreach (var thrust in thrusters)
                {
                    thrust.ThrustOverride = thrust.MaxThrust;
                }

                if (!warheadsTriggered)
                {
                    foreach (var warhead in warheads)
                    {
                        warhead.StartCountdown();
                    }
                }
            }
        }

        private class CameraArrayCellAngle
        {
            public float InitialPitch { get; set; }
            public float InitialYaw { get; set; }
            public float MaximumPitch { get; set; }
            public float MaximumYaw { get; set; }
            public float MinimumPitch { get; set; }
            public float MinimumYaw { get; set; }
        }
    }
}
