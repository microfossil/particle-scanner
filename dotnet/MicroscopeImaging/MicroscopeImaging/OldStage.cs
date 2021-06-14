using System;
using MicroscopeImaging.Models;

namespace MicroscopeImaging
{
    public class OldStage : StageBase
    {
        public override int LargeStepXY { get; set; } = 10000;
        public override int MediumStepXY { get; set; } = 1000;
        public override int SmallStepXY { get; set; } = 100;

        public override int LargeStepZ { get; set; } = 100;
        public override int MediumStepZ { get; set; } = 20;
        public override int SmallStepZ { get; set; } = 5;
        public override int LimitPosX { get; set; } = 75000;
        public override int LimitPosY { get; set; } = 50000;
        public override int LimitNegX { get; set; } = 75000;
        public override int LimitNegY { get; set; } = 50000;
        public override int micrometersPerUnit { get; set; } = 100;
        public override int BaudRate { get; set; } = 9600;
        
        
        public OldStage() : base()
        {

        }
        public override void Home()
        {
            SendCommand("73", "011");
            SendCommand("74", "010");
        }

        public override void MoveX(int x)
        {
            SendCommand("73", "030", VirtualX.ToString());
        }

        public override void MoveY(int y)
        {
            SendCommand("74", "030", VirtualY.ToString());
        }

        public override void MoveZ(int y)
        {
            SendCommand("70", "030", VirtualZ.ToString());
        }

        public override void OnConnection()
        {
            SendCommand("70", "032");
            SendCommand("73", "032");
            SendCommand("74", "032");
            SendCommand("70", "033", "10"); //Slow down Z speed
            SendCommand("70", "034");
        }

        public override void OnPollForUpdates()
        {
            if (IsConnected)
            {
                SendCommand("70", "032");
            }
        }

        public override void ParseMessageReceived(string reply)
        {
            if (reply.Length < 6) return;

            String[] parts = reply.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3) return;
            if (parts[1] != "32") return;

            int value;
            var didParse = int.TryParse(parts[2], out value);
            if (didParse)
            {
                switch (parts[0])
                {
                    case "70":
                        Z = value;
                        break;
                    case "73":
                        X = value;
                        break;
                    case "74":
                        Y = value;
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
