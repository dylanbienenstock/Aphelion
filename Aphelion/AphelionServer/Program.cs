using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Microsoft.Xna.Framework;
using Lidgren.Network;
using Aphelion;
using Aphelion.Entities;

namespace AphelionServer
{
    class Program
    {
        static bool quitting = false;
        static NetServer server;
        static ConcurrentDictionary<NetConnection, Player> connections = new ConcurrentDictionary<NetConnection, Player>(); // TO DO: Make concurrency better
        static BinaryFormatter binaryFormatter = new BinaryFormatter();

        static void Main(string[] args)
        {
            Console.Title = "Aphelion Server";

            try
            {
                int port = 25656;

                if (args.Length > 0)
                {
                    port = int.Parse(args[0]);
                }

                Console.WriteLine("Starting server on port " + port + "...");

                NetPeerConfiguration config = new NetPeerConfiguration("Aphelion");
                config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
                config.MaximumConnections = 32;
                config.Port = port;
                config.AcceptIncomingConnections = true;

                server = new NetServer(config);
                server.Start();

                Console.WriteLine("Server started successfully.");

                (new Thread(new ThreadStart(() =>
                {
                    while (!quitting)
                    {
                        Synchronize();
                        Thread.Sleep(125);  //   8Hz
                        //Thread.Sleep(62); // ~16Hz
                        //Thread.Sleep(31); // ~32Hz
                    }
                }))).Start();

                (new Thread(new ThreadStart(() =>
                {
                    while (!quitting)
                    {
                        Update();
                        Thread.Sleep(15); // ~64Hz
                    }
                }))).Start();

                while (!quitting)
                {
                    NetIncomingMessage message;

                    while ((message = server.ReadMessage()) != null)
                    {
                        switch (message.MessageType)
                        {
                            case NetIncomingMessageType.VerboseDebugMessage:
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("VERBOSE DEBUG: " + message.ReadString());
                                Console.ResetColor();
                                break;
                            case NetIncomingMessageType.DebugMessage:
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("DEBUG: " + message.ReadString());
                                Console.ResetColor();
                                break;
                            case NetIncomingMessageType.WarningMessage:
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("WARNING: " + message.ReadString());
                                Console.ResetColor();
                                break;
                            case NetIncomingMessageType.ErrorMessage:
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("ERROR: " + message.ReadString());
                                Console.ResetColor();
                                break;
                            case NetIncomingMessageType.ConnectionApproval:
                                JudgeConnection(message);
                                break;
                            case NetIncomingMessageType.StatusChanged:
                                switch ((NetConnectionStatus)message.ReadByte())
                                {
                                    case NetConnectionStatus.Connected:
                                        StampTime();
                                        Console.Write("Player ");
                                        Console.ForegroundColor = ConsoleColor.White;
                                        Console.Write(connections[message.SenderConnection].Name);
                                        Console.ResetColor();
                                        Console.Write(" connected from ");
                                        Console.ForegroundColor = ConsoleColor.White;
                                        Console.Write(message.SenderConnection.RemoteEndpoint.ToString());
                                        Console.ResetColor();
                                        Console.WriteLine('.');
                                        break;
                                    case NetConnectionStatus.Disconnected:
                                        StampTime();
                                        Console.Write("Player ");
                                        Console.ForegroundColor = ConsoleColor.White;
                                        Console.Write(connections[message.SenderConnection].Name);
                                        Console.ResetColor();
                                        Console.WriteLine(" disconnected.");

                                        #region TO DO: Encapsulate this in a method

                                        // Tells the connected clients that a player disconnected
                                        NetOutgoingMessage disconnectMessage = server.CreateMessage();
                                        disconnectMessage.Write((byte)NetworkCommand.RemovePlayer);
                                        disconnectMessage.Write(connections[message.SenderConnection].Name);
                                        server.SendToAll(disconnectMessage, NetDeliveryMethod.ReliableUnordered);

                                        Player outPlayer;
                                        if (!connections.TryRemove(message.SenderConnection, out outPlayer))
                                        {
                                            ConcurrentDictionary<NetConnection, Player> newConnections = new ConcurrentDictionary<NetConnection, Player>();

                                            foreach (KeyValuePair<NetConnection, Player> keyValuePair in connections)
                                            {
                                                if (keyValuePair.Key != message.SenderConnection)
                                                {
                                                    newConnections[message.SenderConnection] = keyValuePair.Value;
                                                }
                                            }
                                        }

                                        #endregion

                                        break;
                                }

                                break;
                            case NetIncomingMessageType.Data:
                                NetworkCommand command = (NetworkCommand)message.ReadByte();

                                switch (command)
                                {
                                    case NetworkCommand.SetPlayerPositionVelocity: // TO DO: Add security in this
                                        connections[message.SenderConnection].Position = new Vector2(message.ReadFloat(), message.ReadFloat());
                                        connections[message.SenderConnection].Velocity = new Vector2(message.ReadFloat(), message.ReadFloat());
                                        break;
                                    case NetworkCommand.SetPlayerAngle:
                                        connections[message.SenderConnection].Angle = message.ReadFloat();
                                        break;
                                    case NetworkCommand.PlayerMoveForward:
                                        connections[message.SenderConnection].MovingForward = message.ReadBoolean();
                                        break;
                                    case NetworkCommand.PlayerMoveBackward:
                                        connections[message.SenderConnection].MovingBackward = message.ReadBoolean();
                                        break;
                                    case NetworkCommand.PlayerTurnLeft:
                                        connections[message.SenderConnection].TurningLeft = message.ReadBoolean();
                                        break;
                                    case NetworkCommand.PlayerTurnRight:
                                        connections[message.SenderConnection].TurningRight = message.ReadBoolean();
                                        break;
                                    case NetworkCommand.PlayerBoost:
                                        connections[message.SenderConnection].Boosting = message.ReadBoolean();
                                        break;
                                }
                                break;
                            default:
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("ERROR: Unhandled message type (" + message.MessageType + ')');
                                Console.ResetColor();
                                break;
                        }

                        server.Recycle(message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("INTERNAL ERROR: " + ex.Message);
                Console.WriteLine("The server will now shut down.");
                Console.ResetColor();
                Thread.Sleep(2500);

                quitting = true;
            }
        }

        private static void JudgeConnection(NetIncomingMessage message)
        {
            string name = message.ReadString();
            bool taken = false;

            foreach (Player player in connections.Values)
            {
                if (player.Name == name)
                {
                    taken = true;
                }
            }

            if (!taken)
            {
                Random random = new Random();

                // TO DO: Encapsulate the alert command
                NetOutgoingMessage userConnectMessage = server.CreateMessage();
                userConnectMessage.Write((byte)NetworkCommand.Alert);
                userConnectMessage.Write("User " + name + " has connected.");
                server.SendToAll(userConnectMessage, NetDeliveryMethod.ReliableUnordered);

                message.SenderConnection.Approve();
                connections[message.SenderConnection] = new Player(false, name, new Vector2(random.Next(-128, 128), random.Next(-128, 128)), Vector2.Zero, 0);
            }
            else
            {
                message.SenderConnection.Deny("Someone with the name \"" + name + "\" is already connected to the server.");
            }
        }

        private static void StampTime()
        {
            Console.Write('[' + DateTime.Now.ToString("h:mm tt") + "] ");
        }

        private static void Synchronize()
        {
            foreach (Player player in connections.Values)
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    NetOutgoingMessage message = server.CreateMessage();

                    binaryFormatter.Serialize(memoryStream, player);
                    message.Write((byte)NetworkCommand.FullyUpdatePlayer);
                    message.Write(Convert.ToBase64String(memoryStream.ToArray()));
                    server.SendToAll(message, NetDeliveryMethod.ReliableSequenced);
                }
            }
        }

        private static void Update()
        {
            EntityManager.Update();

            foreach (Player player in connections.Values)
            {
                player.Position += player.Velocity;
                player.ServerPosition = player.Position;

                if (Vector2.Distance(player.Position, Vector2.Zero) > Math.Pow(2, 16))
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(player.Name);
                    Console.ResetColor();
                    Console.WriteLine(" was moved back inside the bounds of the system.");

                    player.Position = Vector2.Zero;
                    player.Velocity = Vector2.Zero;

                    // TO DO: Encapsulate this all
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        NetOutgoingMessage message = server.CreateMessage();

                        binaryFormatter.Serialize(memoryStream, player);
                        message.Write((byte)NetworkCommand.SetPlayerPositionVelocity);
                        message.Write(Convert.ToBase64String(memoryStream.ToArray()));
                        server.SendToAll(message, NetDeliveryMethod.ReliableSequenced);
                    }
                }
            }
        }
    }
}
