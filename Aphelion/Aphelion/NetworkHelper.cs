//#define PREDICTION

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Lidgren.Network;
using Aphelion.Entities;

namespace Aphelion
{
    public static class NetworkHelper // TO DO: Add a commands manager / binding system WAIT NO DO IT IN THE UPDATE LOOP
                                      // TO DO: Implement a base entity and a manager
    {
        public static NetClient Client;
        public static List<Player> Players = new List<Player>();
        public static Player LocalPlayer;
        private static Aphelion game;
        private static KeyboardState lastKeyboardState;
        private static BinaryFormatter binaryFormatter = new BinaryFormatter();

        public static void Initialize(Aphelion game)
        {
            NetworkHelper.game = game;
        }

        public static void Reset()
        {
            if (Client != null)
            {
                Client.Shutdown("Reset");
            }

            Client = null;
        }

        public static void Connect(string ip, int port)
        {
            if (Client == null)
            {
                NetPeerConfiguration config = new NetPeerConfiguration("Aphelion");

                Client = new NetClient(config);
                Client.Start();
                Client.RegisterReceivedCallback(new SendOrPostCallback(MessageReceived));
                Client.Connect(new IPEndPoint(IPAddress.Parse(ip), port), Client.CreateMessage(Settings.GetValueAsString("Profile.Name", "Unknown")));

                Timer.Create("UpdateLocalPlayerPosition", 125, 0, () =>
                {
                    if (LocalPlayer != null)
                    {
                        NetOutgoingMessage message = Client.CreateMessage();
                        message.Write((byte)NetworkCommand.SetPlayerPositionVelocity);
                        message.Write(LocalPlayer.Position.X);
                        message.Write(LocalPlayer.Position.Y);
                        message.Write(LocalPlayer.Velocity.X);
                        message.Write(LocalPlayer.Velocity.Y);
                        Client.SendMessage(message, NetDeliveryMethod.ReliableSequenced);
                    }
                });
            }
            else
            {
                Reset();
                Connect(ip, port);
            }
        }

        public static void Disconnect()
        {
            try
            {
                Client.Disconnect("Client disconnect");
                Timer.Remove("UpdateLocalPlayerPosition");
            }
            catch
            {
                
            }
        }

        public static void Synchronize()
        {
            if (LocalPlayer != null)
            {
                NetOutgoingMessage message = Client.CreateMessage();
                message.Write((byte)NetworkCommand.SetPlayerAngle);
                message.Write(LocalPlayer.Angle);
                Client.SendMessage(message, NetDeliveryMethod.ReliableSequenced);
            }

            KeyboardState keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.W) && !lastKeyboardState.IsKeyDown(Keys.W))
            {
                NetOutgoingMessage message = Client.CreateMessage();
                message.Write((byte)NetworkCommand.PlayerMoveForward);
                message.Write(true);
                Client.SendMessage(message, NetDeliveryMethod.ReliableSequenced);
            }

            if (keyboardState.IsKeyDown(Keys.S) && !lastKeyboardState.IsKeyDown(Keys.S))
            {
                NetOutgoingMessage message = Client.CreateMessage();
                message.Write((byte)NetworkCommand.PlayerMoveBackward);
                message.Write(true);
                Client.SendMessage(message, NetDeliveryMethod.ReliableSequenced);
            }

            if (keyboardState.IsKeyDown(Keys.A) && !lastKeyboardState.IsKeyDown(Keys.A))
            {
                NetOutgoingMessage message = Client.CreateMessage();
                message.Write((byte)NetworkCommand.PlayerTurnLeft);
                message.Write(true);
                Client.SendMessage(message, NetDeliveryMethod.ReliableSequenced);
            }

            if (keyboardState.IsKeyDown(Keys.D) && !lastKeyboardState.IsKeyDown(Keys.D))
            {
                NetOutgoingMessage message = Client.CreateMessage();
                message.Write((byte)NetworkCommand.PlayerTurnRight);
                message.Write(true);
                Client.SendMessage(message, NetDeliveryMethod.ReliableSequenced);
            }

            if (keyboardState.IsKeyDown(Keys.LeftShift) && !lastKeyboardState.IsKeyDown(Keys.LeftShift))
            {
                NetOutgoingMessage message = Client.CreateMessage();
                message.Write((byte)NetworkCommand.PlayerBoost);
                message.Write(true);
                Client.SendMessage(message, NetDeliveryMethod.ReliableSequenced);
            }

            ////////////////////////////////////////////////////////////////////////////

            if (!keyboardState.IsKeyDown(Keys.W) && lastKeyboardState.IsKeyDown(Keys.W))
            {
                NetOutgoingMessage message = Client.CreateMessage();
                message.Write((byte)NetworkCommand.PlayerMoveForward);
                message.Write(false);
                Client.SendMessage(message, NetDeliveryMethod.ReliableSequenced);
            }

            if (!keyboardState.IsKeyDown(Keys.S) && lastKeyboardState.IsKeyDown(Keys.S))
            {
                NetOutgoingMessage message = Client.CreateMessage();
                message.Write((byte)NetworkCommand.PlayerMoveBackward);
                message.Write(false);
                Client.SendMessage(message, NetDeliveryMethod.ReliableSequenced);
            }

            if (!keyboardState.IsKeyDown(Keys.A) && lastKeyboardState.IsKeyDown(Keys.A))
            {
                NetOutgoingMessage message = Client.CreateMessage();
                message.Write((byte)NetworkCommand.PlayerTurnLeft);
                message.Write(false);
                Client.SendMessage(message, NetDeliveryMethod.ReliableSequenced);
            }

            if (!keyboardState.IsKeyDown(Keys.D) && lastKeyboardState.IsKeyDown(Keys.D))
            {
                NetOutgoingMessage message = Client.CreateMessage();
                message.Write((byte)NetworkCommand.PlayerTurnRight);
                message.Write(false);
                Client.SendMessage(message, NetDeliveryMethod.ReliableSequenced);
            }

            if (!keyboardState.IsKeyDown(Keys.LeftShift) && lastKeyboardState.IsKeyDown(Keys.LeftShift))
            {
                NetOutgoingMessage message = Client.CreateMessage();
                message.Write((byte)NetworkCommand.PlayerBoost);
                message.Write(false);
                Client.SendMessage(message, NetDeliveryMethod.ReliableSequenced);
            }

            lastKeyboardState = keyboardState;
        }

        public static void Predict()
        {
            if (LocalPlayer != null)
            {
                KeyboardState keyboardState = Keyboard.GetState();
                Vector2 direction = new Vector2((float)Math.Cos(LocalPlayer.Angle), (float)Math.Sin(LocalPlayer.Angle));

                if (keyboardState.IsKeyDown(Keys.W) && !keyboardState.IsKeyDown(Keys.S))
                {
                    LocalPlayer.Velocity += direction * 0.05f;

                    if (keyboardState.IsKeyDown(Keys.LeftShift))
                    {
                        LocalPlayer.Velocity += direction * 0.1f;
                    }
                }
                else if (!keyboardState.IsKeyDown(Keys.W) && keyboardState.IsKeyDown(Keys.S))
                {
                    LocalPlayer.Velocity -= direction * 0.05f;
                }

                LocalPlayer.Position += LocalPlayer.Velocity;

                if (keyboardState.IsKeyDown(Keys.A) && !keyboardState.IsKeyDown(Keys.D))
                {
                    LocalPlayer.Angle -= 0.05f;
                }
                else if (!keyboardState.IsKeyDown(Keys.A) && keyboardState.IsKeyDown(Keys.D))
                {
                    LocalPlayer.Angle += 0.05f;
                }
            }

            foreach (Player player in Players)
            {
                player.Position += player.Velocity;
            }
        }

        private static void MessageReceived(object serverObject)
        {
            NetClient server = (NetClient)serverObject;
            NetIncomingMessage message = server.ReadMessage();

            switch (message.MessageType)
            {
                case NetIncomingMessageType.VerboseDebugMessage:
                    break;
                case NetIncomingMessageType.DebugMessage:
                    break;
                case NetIncomingMessageType.WarningMessage:
                    break;
                case NetIncomingMessageType.ErrorMessage:
                    break;
                case NetIncomingMessageType.ConnectionApproval:
                    break;
                case NetIncomingMessageType.StatusChanged:
                    switch ((NetConnectionStatus)message.ReadByte())
                    {
                        case NetConnectionStatus.Connected:
                            game.SuccessfullyConnected();
                            break;
                    }
                    break;
                case NetIncomingMessageType.Data:
                    NetworkCommand command = (NetworkCommand)message.ReadByte();

                    switch (command)
                    {
                        case NetworkCommand.CreatePlayer:
                            CreatePlayer(message);
                            break;
                        case NetworkCommand.RemovePlayer:
                            RemovePlayer(message);
                            break;
                        case NetworkCommand.FullyUpdatePlayer:
                            FullyUpdatePlayer(message);
                            break;
                        case NetworkCommand.SetPlayerPositionVelocity:
                            SetPlayerPositionVelocity(message);
                            break;
                        case NetworkCommand.Alert:
                            Alert.Create(message.ReadString());
                            // TO DO: Implement duration
                            break;
                    }
                    break;
                default:
                    break;
            }

            Client.Recycle(message);
        }

        private static void CreatePlayer(NetIncomingMessage message) // TO DO: Implement duplicate protection
        {
            using (MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(message.ReadString())))
            {
                Player newPlayer = (Player)binaryFormatter.Deserialize(memoryStream);

                if (newPlayer.Name == Settings.GetValueAsString("Profile.Name", "Dylan"))
                {
                    LocalPlayer = new Player(true, newPlayer.Name, newPlayer.Position, newPlayer.Velocity, newPlayer.Angle);
                    EntityManager.Add(LocalPlayer);
                }
                else
                {
                    // TO DO: Fix this, never called
                    //Alert.Create("User " + newPlayer.Name + " has connected.");
                    //Players.Add(newPlayer);
                    //EntityManager.Add(newPlayer);
                }
            }
        }

        private static void RemovePlayer(NetIncomingMessage message)
        {
            string name = message.ReadString();

            if (name == Settings.GetValueAsString("Profile.Name", "Dylan"))
            {
                // This shouldn't happen
                // TO DO: Figure out what we should do here
            }
            else
            {
                foreach (Player player in Players)
                {
                    if (name == player.Name)
                    {
                        Alert.Create("User " + player.Name + " has disconnected.");
                        Players.Remove(player);
                        EntityManager.Remove(player);

                        break;
                    }
                }
            }
        }

        private static void FullyUpdatePlayer(NetIncomingMessage message)
        {
            using (MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(message.ReadString())))
            {
                Player updatedPlayer = (Player)binaryFormatter.Deserialize(memoryStream);

                if (updatedPlayer.Name == Settings.GetValueAsString("Profile.Name", "Dylan"))
                {
                    if (LocalPlayer == null)
                    {
                        LocalPlayer = new Player(true, updatedPlayer.Name, updatedPlayer.Position, updatedPlayer.Velocity, updatedPlayer.Angle);
                        EntityManager.Add(LocalPlayer);
                    }
                    else
                    {
                        //System.Diagnostics.Debug.Print(Vector2.Distance(LocalPlayer.Position, updatedPlayer.Position).ToString());
//#if PREDICTION
//                        if (Vector2.Distance(LocalPlayer.Position, updatedPlayer.Position) > 150)
//                        {
//                            LocalPlayer.Position = updatedPlayer.Position;
//                        }
//                        if (Vector2.Distance(LocalPlayer.Velocity, updatedPlayer.Velocity) > 2)
//                        {
//                            LocalPlayer.Velocity = updatedPlayer.Velocity;
//                        }
//#else
//                        LocalPlayer.Position = updatedPlayer.Position;
//                        LocalPlayer.Velocity = updatedPlayer.Velocity;
//#endif
                          LocalPlayer.ServerPosition = updatedPlayer.Position;
//                        LocalPlayer.Angle = updatedPlayer.Angle;
//                        LocalPlayer.MovingForward = updatedPlayer.MovingForward;
//                        LocalPlayer.MovingBackward = updatedPlayer.MovingBackward;
//                        LocalPlayer.TurningLeft = updatedPlayer.TurningLeft;
//                        LocalPlayer.TurningRight = updatedPlayer.TurningRight;
//                        LocalPlayer.Boosting = updatedPlayer.Boosting;
                    }
                }
                else
                {
                    bool found = false;

                    foreach (Player player in Players)
                    {
                        if (updatedPlayer.Name == player.Name)
                        {
                            found = true;

                            player.Position = updatedPlayer.Position;
                            player.Velocity = updatedPlayer.Velocity;
                            player.ServerPosition = updatedPlayer.ServerPosition;
                            player.Angle = updatedPlayer.Angle;
                            player.MovingForward = updatedPlayer.MovingForward;
                            player.MovingBackward = updatedPlayer.MovingBackward;
                            player.TurningLeft = updatedPlayer.TurningLeft;
                            player.TurningRight = updatedPlayer.TurningRight;
                            player.Boosting = updatedPlayer.Boosting;

                            break;
                        }
                    }

                    if (!found)
                    {
                        Players.Add(updatedPlayer);
                        EntityManager.Add(updatedPlayer);
                    }
                }
            }
        }

        private static void SetPlayerPositionVelocity(NetIncomingMessage message) // TO DO: Use a different command specifically for this
        {
            using (MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(message.ReadString())))
            {
                Player updatedPlayer = (Player)binaryFormatter.Deserialize(memoryStream);

                if (updatedPlayer.Name == Settings.GetValueAsString("Profile.Name", "Dylan"))
                {
                    LocalPlayer.Position = updatedPlayer.Position;
                    LocalPlayer.Velocity = updatedPlayer.Velocity;
                    LocalPlayer.ServerPosition = updatedPlayer.ServerPosition;
                }
                else
                {
                    bool found = false;

                    foreach (Player player in Players)
                    {
                        if (updatedPlayer.Name == player.Name)
                        {
                            found = true;

                            player.Position = updatedPlayer.Position;
                            player.Velocity = updatedPlayer.Velocity;
                            player.ServerPosition = updatedPlayer.ServerPosition;

                            break;
                        }
                    }

                    if (!found)
                    {
                        Players.Add(updatedPlayer);
                        EntityManager.Add(updatedPlayer);
                    }
                }
            }
        }
    }
}
