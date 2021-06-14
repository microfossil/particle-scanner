using System;
using WPFHelpers.Helpers;

namespace MicroscopeImaging.Models
{
    public class EnderStage : StageBase
    {
        public override int LargeStepXY { get; set; } = 10000;
        public override int MediumStepXY { get; set; } = 1000;
        public override int SmallStepXY { get; set; } = 100;

        public override int LargeStepZ { get; set; } = 1000;
        public override int MediumStepZ { get; set; } = 100;
        public override int SmallStepZ { get; set; } = 20;
        public override int LimitPosX { get; set; } = 200000;
        public override int LimitPosY { get; set; } = 200000;
        public override int LimitNegX { get; set; } = 0;
        public override int LimitNegY { get; set; } = 0;

        public override int micrometersPerUnit { get; set; } = 1;


        public override int BaudRate { get; set; } = 115200;

        public EnderStage() : base()
        {
        }

        public override void Home()
        {
            SendCommand("G28", "R", "X", "Y", "Z");
            base.Home();
        }

        public override void MoveX(int x)
        {
            double pos = VirtualX;
            pos /= 1000;
            SendCommand("G1", "X" + pos.ToString("F3"), "F3000");
        }

        public override void MoveY(int y)
        {
            double pos = VirtualY;
            pos /= 1000;
            SendCommand("G1", "Y" + pos.ToString("F3"), "F3000");
        }

        public override void MoveZ(int y)
        {
            double pos = VirtualZ;
            pos /= 1000;
            SendCommand("G1", "Z" + pos.ToString("F3"), "F100");
        }

        public override void OnConnection()
        {
            SendCommand("G90");
            SendCommand("M106 S0");
        }

        public override void OnPollForUpdates()
        {
            if (IsConnected)
            {
                SendCommand("M114");
            }
        }

        public override void ParseMessageReceived(string reply)
        {
            DebugHelper.MethodInfo(reply);
            //if (reply.Length < 6) return;

            String[] parts = reply.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0) return;

            if (parts[0].StartsWith("X:"))
            {
                X = double.TryParse(parts[0].Substring(2), out var result) ? (int) (result * 1000) : 0;
                Y = double.TryParse(parts[1].Substring(2), out result) ? (int)(result * 1000) : 0;
                Z = double.TryParse(parts[2].Substring(2), out result) ? (int)(result * 1000) : 0;

                if (VirtualX == -Int32.MaxValue) VirtualX = X;
                if (VirtualY == -Int32.MaxValue) VirtualY = Y;
                if (VirtualZ == -Int32.MaxValue) VirtualZ = Z;
            }
            //if (parts.Length < 3) return;
            //if (parts[1] != "32") return;

            //int value;
            //var didParse = int.TryParse(parts[2], out value);
            //if (didParse)
            //{
            //    switch (parts[0])
            //    {
            //        case "70":
            //            Z = value;
            //            break;
            //        case "73":
            //            X = value;
            //            break;
            //        case "74":
            //            Y = value;
            //            break;
            //        default:
            //            break;
            //    }
            //}
        }
    }
}