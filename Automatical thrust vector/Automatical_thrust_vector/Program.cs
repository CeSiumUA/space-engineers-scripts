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
        private Vector3D lastPosition;
        IMyThrust upThruster;
        IMyTimerBlock triggerTimer;
        List<IMyGyro> gyroValues = new List<IMyGyro>();
        private float yawValue = 0;
        private float pitchValue = 0;
        private float rollValue = 0;
        double velocity = 0;
        ThrustStrategy currentStrategy = ThrustStrategy.Yaw;
        List<IMyTextSurfaceProvider> surfaceProviders = new List<IMyTextSurfaceProvider>();
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
            lastPosition = this.Me.GetPosition();
            upThruster = GridTerminalSystem.GetBlockWithName("Main thruster") as IMyThrust;
            triggerTimer = GridTerminalSystem.GetBlockWithName("Main Timer") as IMyTimerBlock;
            GridTerminalSystem.GetBlocksOfType<IMyGyro>(gyroValues);
            GridTerminalSystem.GetBlocksOfType<IMyTextSurfaceProvider>(surfaceProviders);
        }

        public void Save()
        {
        }

        public void Main(string argument, UpdateType updateSource)
        {
            var firstGyro = gyroValues.First();
            foreach (var gyroScope in gyroValues)
            {
                gyroScope.GyroOverride = true;
            }
            var currentPosition = this.Me.GetPosition();

            var diferentialVector = currentPosition - this.lastPosition;

            var stepVelocity = (diferentialVector * 60).Length();

            foreach (var surface in surfaceProviders)
            {
                var srf = surface.GetSurface(0);
                if(srf != null)
                {
                    srf.WriteText($"X={diferentialVector.X} \n Y={diferentialVector.Y} \n Z={diferentialVector.Z} \n Vel={velocity} \n Yaw={firstGyro.Yaw} \n Pitch={firstGyro.Pitch} \n Roll={firstGyro.Roll}");
                }
            }

            if(currentStrategy == ThrustStrategy.Yaw)
            {
                foreach (var gyroScope in gyroValues)
                {
                    gyroScope.Yaw = 5;
                }
            }

            velocity = stepVelocity;
            lastPosition = currentPosition;
        }
        public enum ThrustStrategy
        {
            Yaw,
            Pitch,
            Roll
        }
    }
}
