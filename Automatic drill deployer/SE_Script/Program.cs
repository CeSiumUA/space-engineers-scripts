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

        List<IMyMotorStator> rotors = new List<IMyMotorStator>();
        List<IMyShipDrill> drills = new List<IMyShipDrill>();
        List<IMyPistonBase> drillPistons = new List<IMyPistonBase>();
        public Program()
        {
            GridTerminalSystem.GetBlocksOfType<IMyMotorStator>(rotors);
            GridTerminalSystem.GetBlocksOfType<IMyShipDrill>(drills);
            GridTerminalSystem.GetBlocksOfType<IMyPistonBase>(drillPistons);
            foreach(var rotor in rotors)
            {
                var direction = rotor.CustomData.Split('-');
                if (direction[1] == "i")
                {
                    rotor.LowerLimitDeg = -90;
                    rotor.UpperLimitDeg = 0;
                    rotor.TargetVelocityRPM = 0;
                }
                if(direction[1] == "s")
                {
                    rotor.LowerLimitDeg = 0;
                    rotor.UpperLimitDeg = 90;
                    rotor.TargetVelocityRPM = 0;
                }
                if(direction[1] == "f")
                {
                    rotor.LowerLimitDeg = 0;
                    rotor.UpperLimitDeg = 180;
                    rotor.TargetVelocityRPM = 0;
                }
            }
        }

        public void Save()
        {
            
        }

        public void Main(string argument, UpdateType updateSource)
        {
            bool enabled = argument == "deploy" ? true : false;

            if (enabled)
            {
                Deploy();
            }
            else
            {
                Retract();
            }

            if(argument == "stop")
            {
                Stop();
            }
        }
        private void Deploy()
        {
            var orderedRotors = new IMyMotorStator[3];
            foreach (var rotor in rotors)
            {
                var customDataORder = Int32.Parse(rotor.CustomData.Split('-')[0]);
                orderedRotors[customDataORder - 1] = rotor;
            }
            foreach(var rotor in orderedRotors)
            {
                var direction = rotor.CustomData.Split('-')[1];
                if(direction == "i")
                {
                    rotor.TargetVelocityRPM = -1;
                }
                else
                {
                    rotor.TargetVelocityRPM = 1;
                }
            }
            //foreach (var rotor in orderedRotors)
            //{
            //    var direction = rotor.CustomData.Split('-')[1];
            //    if (direction == "i")
            //    {
            //        while (rotor.Angle != rotor.LowerLimitDeg) { }
            //    }
            //    else
            //    {
            //        while (rotor.Angle != rotor.UpperLimitDeg) { }
            //    }
            //    rotor.TargetVelocityRPM = 0;
            //}
            foreach (var drill in drills)
            {
                drill.Enabled = true;
            }
            foreach(var piston in drillPistons)
            {
                piston.Extend();
            }
        }
        private void Retract()
        {
            var orderedRotors = new IMyMotorStator[3];
            foreach (var rotor in rotors)
            {
                var customDataORder = Int32.Parse(rotor.CustomData.Split('-')[0]);
                orderedRotors[orderedRotors.Length - customDataORder] = rotor;
            }
            foreach (var rotor in orderedRotors)
            {
                var direction = rotor.CustomData.Split('-')[1];
                if (direction == "i")
                {
                    rotor.TargetVelocityRPM = 1;
                }
                else
                {
                    rotor.TargetVelocityRPM = -1;
                }
            }
            foreach (var drill in drills)
            {
                drill.Enabled = false;
            }
            foreach (var piston in drillPistons)
            {
                piston.Retract();
            }
        }
        private void Stop()
        {
            foreach (var rotor in rotors)
            {
                var direction = rotor.CustomData.Split('-');
                if (direction[1] == "i")
                {
                    rotor.LowerLimitDeg = -90;
                    rotor.UpperLimitDeg = 0;
                    rotor.TargetVelocityRPM = 0;
                }
                if (direction[1] == "s")
                {
                    rotor.LowerLimitDeg = 0;
                    rotor.UpperLimitDeg = 90;
                    rotor.TargetVelocityRPM = 0;
                }
                if (direction[1] == "f")
                {
                    rotor.LowerLimitDeg = 0;
                    rotor.UpperLimitDeg = 180;
                    rotor.TargetVelocityRPM = 0;
                }
            }
        }
    }
}
