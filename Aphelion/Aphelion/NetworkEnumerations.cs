using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aphelion
{
    public enum NetworkSide
    {
        Client,
        Server
    }

    public enum NetworkCommand
    {
        Disconnect,                 // No arguments (NOT IMPLEMENTED)
        CreatePlayer,               // [string:SerializedPlayer]
        RemovePlayer,               // [string:Name]
        FullyUpdatePlayer,          // [string:SerializedPlayer]
        SetEntityName,              // [string:Name] (NOT IMPLEMENTED)
        SetEntityPosition,          // [float:PositionX] [float:PositionY] (NOT IMPLEMENTED)
        SetEntityVelocity,          // [float:VelocityX] [float:VelocityY] (NOT IMPLEMENTED)
        SetEntityAngle,             // [float:Angle] (NOT IMPLEMENTED)
        PlayerMoveForward,          // [bool:Enabled]
        PlayerMoveBackward,         // [bool:Enabled]
        PlayerTurnLeft,             // [bool:Enabled]
        PlayerTurnRight,            // [bool:Enabled]
        PlayerBoost,                // [bool:Enabled]
        PlayerFire,                 // [bool:Enabled] (NOT IMPLEMENTED)
        SetPlayerPositionVelocity,  // [float:PositionX] [float:PositionY] [float:VelocityX] [float:VelocityY]
        SetPlayerAngle,             // [float:Angle]
        Alert,                      // [string:Text] (PLANNED) [float:Duration]
    }
}
