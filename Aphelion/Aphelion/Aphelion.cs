using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.Win32;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.GamerServices;
using Aphelion.Entities;

namespace Aphelion
{
    public enum GameState
    {
        AtMenu,
        Connecting,
        InGame
    }

    public class Aphelion : Game // TO DO: Make it so the boundary's radius is 1,048,576 (2^20) and so 1 pixel = 100 km (EARTH = 128 px)
                                                                                    // DO 2^26 OR 2^30 MAYBE
    {
        public GameState State = GameState.AtMenu;
        public Camera MainCamera = new Camera();
        GraphicsDeviceManager graphics;
        SpriteBatch sprites;
        MouseState lastMouseState = Mouse.GetState();
        public Vector2 ScreenBounds;

        public Aphelion() : base()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.IsFullScreen = false;

            if (graphics.IsFullScreen)
            {
                ScreenBounds = new Vector2(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);
            }
            else
            {
                ScreenBounds = new Vector2(1000, 600);
            }

            graphics.PreferredBackBufferWidth = (int)ScreenBounds.X;
            graphics.PreferredBackBufferHeight = (int)ScreenBounds.Y;

            IsFixedTimeStep = true;
            TargetElapsedTime = TimeSpan.FromMilliseconds(15); // ~64Hz
            IsMouseVisible = true;

            Content.RootDirectory = "Content";

            // To keep you from ruining everything forever
            Form gameForm = (Form)Form.FromHandle(Window.Handle);
            gameForm.MinimumSize = new System.Drawing.Size(640 + 20, 480 + 40);
            gameForm.FormClosing += new System.Windows.Forms.FormClosingEventHandler((sender, e) =>
            {
                NetworkHelper.Disconnect();
            });

            NetworkHelper.Initialize(this);
        }

        protected override void Initialize()
        {
            Settings.SetFile(Environment.CurrentDirectory + "\\settings.xml", true);
            Settings.ReadFromFile();

            Settings.SetDefaultValue<string>("Profile.Name", "Dylan");
            Settings.SetDefaultValue<int>("Profile.Red", 255);
            Settings.SetDefaultValue<int>("Profile.Green", 255);
            Settings.SetDefaultValue<int>("Profile.Blue", 255);
            Settings.SetDefaultValue<string>("LastIP", "127.0.0.1");
            Settings.SetDefaultValue<int>("LastPort", 25656);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            MainCamera.Size = ScreenBounds;

            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += new EventHandler<EventArgs>((sender, e) =>
            {
                float snap = 10.0f;
                ScreenBounds = new Vector2((float)Math.Round(Window.ClientBounds.Width / snap) * snap, (float)Math.Round(Window.ClientBounds.Height / snap) * snap);
                Window.Title = "Aphelion - X:" + ScreenBounds.X + ", Y:" + ScreenBounds.Y;

                graphics.PreferredBackBufferWidth = (int)ScreenBounds.X;
                graphics.PreferredBackBufferHeight = (int)ScreenBounds.Y;
                graphics.ApplyChanges();

                // TO DO: Decide if I should turn this into a switch statement
                if (State == GameState.AtMenu)
                {
                    Interface.InterfaceManager.RemoveAllWithTag("Title Menu");
                    Interface.InterfaceManager.RemoveAllWithTag("Prompt");

                    CreateTitleMenu();
                }
                else
                {
                    Interface.TextButton debugButton = (Interface.TextButton)Interface.InterfaceManager.GetAllElementsWithTag("Debug Button")[0];
                    debugButton.Position = new Vector2(8, ScreenBounds.Y - 8 - debugButton.CalculateDimensions().Y);
                }

                MainCamera.Size = ScreenBounds;
            });

            sprites = new SpriteBatch(GraphicsDevice);
            GameContent.SetContentManager(Content);

            TextRenderer.SetSpriteBatch(sprites);
            TextRenderer.SetFont(GameContent.Texture(@"interface\fonts\default"));

            CreateTitleMenu();
        }

        public void CreateTitleMenu()
        {
            int titlePadding = Utility.ScaleY(36, this);
            int panelPadding = Utility.ScaleY(9, this);
            int topPadding = Utility.ScaleY(64, this);
            int padding = Utility.ScaleY(18, this);
            int titleScale = Utility.ScaleY(15, this);
            int labelScale = Math.Max(Utility.ScaleY(3, this), 2);
            string openPrompt = null;

            Action<string> SetPrompt = new Action<string>((prompt) =>
            {
                Interface.InterfaceManager.RemoveAllWithTag("Prompt");
                openPrompt = prompt;
            });

            Interface.Panel titleMenuPanel = new Interface.Panel();
            titleMenuPanel.Tag = "Title Menu";
            titleMenuPanel.AutoSize = true;
            titleMenuPanel.BorderScale = 2;
            Interface.InterfaceManager.Add(titleMenuPanel);

            Interface.Label titleLabel = new Interface.Label();
            titleLabel.Tag = "Title Menu";
            titleLabel.Text = "Aphelion";
            titleLabel.Position = new Vector2(padding, padding);
            titleLabel.Scale = titleScale;
            titleMenuPanel.Add(titleLabel);

            Interface.TextButton manageYourProfileButton = new Interface.TextButton();
            manageYourProfileButton.Tag = "Title Menu";
            manageYourProfileButton.Text = "Manage your profile";
            manageYourProfileButton.Position = new Vector2(titleLabel.Position.X, titlePadding + titleLabel.Position.Y + titleLabel.CalculateDimensions().Y);
            manageYourProfileButton.Scale = labelScale;
            manageYourProfileButton.HoverMode = Interface.TextButtonHoverMode.PointAt;
            manageYourProfileButton.OnHover += (sender, e) => { GameContent.Sound("titlemenu_buttonhover").Play(0.5f, 1.0f, 0.0f); };
            manageYourProfileButton.OnRelease += (sender, e) => { GameContent.Sound("titlemenu_buttonclick").Play(0.5f, 1.0f, 0.0f); };
            manageYourProfileButton.OnRelease += (sender, e) =>
            {
                if (openPrompt != "Profile")
                {
                    SetPrompt("Profile");

                    Interface.Panel profilePanel = new Interface.Panel();
                    profilePanel.Tag = "Prompt";
                    profilePanel.AutoSize = true;
                    profilePanel.Position = new Vector2(titleMenuPanel.Position.X, titleMenuPanel.Position.Y + titleMenuPanel.CalculateDimensions().Y + panelPadding + titleMenuPanel.BorderScale);
                    profilePanel.BorderScale = titleMenuPanel.BorderScale;
                    Interface.InterfaceManager.Add(profilePanel);

                    Interface.Label nameLabel = new Interface.Label();
                    nameLabel.Tag = "Prompt";
                    nameLabel.Text = "Name";
                    nameLabel.Position = new Vector2(padding, padding);
                    nameLabel.Scale = labelScale;
                    profilePanel.Add(nameLabel);

                    Interface.TextBox nameTextbox = new Interface.TextBox();
                    nameTextbox.Tag = "Prompt";
                    nameTextbox.MaxLength = 15;
                    nameTextbox.Position = new Vector2(padding, padding + nameLabel.Position.Y + nameLabel.CalculateDimensions().Y);
                    nameTextbox.BorderScale = 2;
                    nameTextbox.Scale = labelScale;
                    nameTextbox.Text = Settings.GetValueAsString("Profile.Name", "Dylan");
                    profilePanel.Add(nameTextbox);

                    Interface.Label colorLabel = new Interface.Label();
                    colorLabel.Tag = "Prompt";
                    colorLabel.Text = "Color";
                    colorLabel.Position = new Vector2(padding + nameTextbox.Position.X + nameTextbox.CalculateDimensions().X, padding);
                    colorLabel.Scale = labelScale;
                    profilePanel.Add(colorLabel);

                    //Interface.TextButton colorHelpButton = new Interface.TextButton();
                    //colorHelpButton.Tag = "Prompt";
                    //colorHelpButton.Text = "?";
                    //colorHelpButton.Position = new Vector2(colorLabel.Position.X + colorLabel.CalculateDimensions().X + 4, colorLabel.Position.Y);
                    //colorHelpButton.Scale = labelScale;
                    //profilePanel.Add(colorHelpButton);

                    Interface.TextBox redTextbox = new Interface.TextBox();
                    redTextbox.Tag = "Prompt";
                    redTextbox.Text = Settings.GetValueAsString("Profile.Red", "255");
                    redTextbox.Mode = Interface.TextBoxInputMode.Numerical;
                    redTextbox.MaxLength = 3;
                    redTextbox.Position = new Vector2(padding + nameLabel.Position.X + nameTextbox.CalculateDimensions().X, padding + nameLabel.Position.Y + nameLabel.CalculateDimensions().Y);
                    redTextbox.BorderScale = 2;
                    redTextbox.Scale = labelScale;
                    redTextbox.BorderColor = Color.FromNonPremultiplied(255, 0, 0, 255);
                    profilePanel.Add(redTextbox);

                    Interface.TextBox greenTextbox = new Interface.TextBox();
                    greenTextbox.Tag = "Prompt";
                    greenTextbox.Text = Settings.GetValueAsString("Profile.Green", "255");
                    greenTextbox.Mode = Interface.TextBoxInputMode.Numerical;
                    greenTextbox.MaxLength = 3;
                    greenTextbox.Position = new Vector2(padding + redTextbox.Position.X + redTextbox.CalculateDimensions().X, padding + nameLabel.Position.Y + nameLabel.CalculateDimensions().Y);
                    greenTextbox.BorderScale = 2;
                    greenTextbox.Scale = labelScale;
                    greenTextbox.BorderColor = Color.FromNonPremultiplied(0, 255, 0, 255);
                    profilePanel.Add(greenTextbox);

                    Interface.TextBox blueTextbox = new Interface.TextBox();
                    blueTextbox.Tag = "Prompt";
                    blueTextbox.Text = Settings.GetValueAsString("Profile.Blue", "255");
                    blueTextbox.Mode = Interface.TextBoxInputMode.Numerical;
                    blueTextbox.MaxLength = 3;
                    blueTextbox.Position = new Vector2(padding + greenTextbox.Position.X + greenTextbox.CalculateDimensions().X, padding + nameLabel.Position.Y + nameLabel.CalculateDimensions().Y);
                    blueTextbox.BorderScale = 2;
                    blueTextbox.Scale = labelScale;
                    blueTextbox.BorderColor = Color.FromNonPremultiplied(0, 0, 255, 255);
                    profilePanel.Add(blueTextbox);

                    Interface.TextButton confirmButton = new Interface.TextButton();
                    confirmButton.Tag = "Title Menu";
                    confirmButton.Text = "Confirm";
                    confirmButton.Scale = labelScale;
                    confirmButton.Position = new Vector2(blueTextbox.Position.X + blueTextbox.CalculateDimensions().X + (int)Math.Round(padding * 1.5f), blueTextbox.Position.Y + confirmButton.Scale);
                    confirmButton.OnHover += (sender2, e2) => { GameContent.Sound("titlemenu_buttonhover").Play(0.5f, 1.0f, 0.0f); };
                    confirmButton.OnRelease += (sender2, e2) => { GameContent.Sound("titlemenu_buttonclick").Play(0.5f, 1.0f, 0.0f); };
                    confirmButton.OnRelease += (sender2, e2) =>
                    {
                        Settings.SetValue<string>("Profile.Name", nameTextbox.Text);
                        Settings.SetValue<int>("Profile.Red", int.Parse(redTextbox.Text));
                        Settings.SetValue<int>("Profile.Green", int.Parse(greenTextbox.Text));
                        Settings.SetValue<int>("Profile.Blue", int.Parse(blueTextbox.Text));

                        SetPrompt(null);
                    };
                    profilePanel.Add(confirmButton);
                }
            };
            titleMenuPanel.Add(manageYourProfileButton);

            Interface.TextButton connectToServerButton = new Interface.TextButton();
            connectToServerButton.Tag = "Title Menu";
            connectToServerButton.Text = "Connect to a server";
            connectToServerButton.Position = new Vector2(titleLabel.Position.X, padding + manageYourProfileButton.Position.Y + manageYourProfileButton.CalculateDimensions().Y);
            connectToServerButton.Scale = labelScale;
            connectToServerButton.HoverMode = Interface.TextButtonHoverMode.PointAt;
            connectToServerButton.OnHover += (sender, e) => { GameContent.Sound("titlemenu_buttonhover").Play(0.5f, 1.0f, 0.0f); };
            connectToServerButton.OnRelease += (sender, e) => { GameContent.Sound("titlemenu_buttonclick").Play(0.5f, 1.0f, 0.0f); };
            connectToServerButton.OnRelease += (sender, e) =>
            {
                if (openPrompt != "Connect")
                {
                    SetPrompt("Connect");

                    Interface.Panel connectPanel = new Interface.Panel();
                    connectPanel.Tag = "Prompt";
                    connectPanel.AutoSize = true;
                    connectPanel.Position = new Vector2(titleMenuPanel.Position.X, titleMenuPanel.Position.Y + titleMenuPanel.CalculateDimensions().Y + panelPadding + titleMenuPanel.BorderScale);
                    connectPanel.BorderScale = titleMenuPanel.BorderScale;
                    Interface.InterfaceManager.Add(connectPanel);

                    Interface.Label ipAddressLabel = new Interface.Label();
                    ipAddressLabel.Tag = "Prompt";
                    ipAddressLabel.Text = "IP Address";
                    ipAddressLabel.Position = new Vector2(padding, padding);
                    ipAddressLabel.Scale = labelScale;
                    connectPanel.Add(ipAddressLabel);

                    Interface.TextBox ipAddressTextbox = new Interface.TextBox();
                    ipAddressTextbox.Tag = "Prompt";
                    ipAddressTextbox.Text = Settings.GetValueAsString("LastIP", "127.0.0.1");
                    ipAddressTextbox.Mode = Interface.TextBoxInputMode.Numerical;
                    ipAddressTextbox.MaxLength = 15;
                    ipAddressTextbox.Position = new Vector2(padding, padding + ipAddressLabel.Position.Y + ipAddressLabel.CalculateDimensions().Y);
                    ipAddressTextbox.BorderScale = 2;
                    ipAddressTextbox.Scale = labelScale;
                    connectPanel.Add(ipAddressTextbox);

                    Interface.Label portLabel = new Interface.Label();
                    portLabel.Tag = "Prompt";
                    portLabel.Text = "Port";
                    portLabel.Position = new Vector2(padding + ipAddressTextbox.Position.X + ipAddressTextbox.CalculateDimensions().X, padding);
                    portLabel.Scale = labelScale;
                    connectPanel.Add(portLabel);

                    Interface.TextBox portTextbox = new Interface.TextBox();
                    portTextbox.Tag = "Prompt";
                    portTextbox.Text = Settings.GetValueAsString("LastPort", "25656");
                    portTextbox.Mode = Interface.TextBoxInputMode.Numerical;
                    portTextbox.MaxLength = 6;
                    portTextbox.Position = new Vector2(padding + ipAddressLabel.Position.X + ipAddressTextbox.CalculateDimensions().X, padding + ipAddressLabel.Position.Y + ipAddressLabel.CalculateDimensions().Y);
                    portTextbox.BorderScale = 2;
                    portTextbox.Scale = labelScale;
                    connectPanel.Add(portTextbox);

                    Interface.TextButton connectButton = new Interface.TextButton();
                    connectButton.Tag = "Title Menu";
                    connectButton.Text = "Connect";
                    connectButton.Scale = labelScale;
                    connectButton.Position = new Vector2(portTextbox.Position.X + portTextbox.CalculateDimensions().X + (int)Math.Round(padding * 1.5f), portTextbox.Position.Y + connectButton.Scale);
                    connectButton.OnHover += (sender2, e2) => { GameContent.Sound("titlemenu_buttonhover").Play(0.5f, 1.0f, 0.0f); };
                    connectButton.OnRelease += (sender2, e2) => { GameContent.Sound("titlemenu_buttonclick").Play(0.5f, 1.0f, 0.0f); };
                    connectButton.OnRelease += (sender2, e2) =>
                    {
                        State = GameState.Connecting;
                        ConnectingScreen.Text = "CONNECTING";
                        ConnectingScreen.CurrentColor = Color.White;
                        ConnectingScreen.DestColor = Color.White;

                        Interface.InterfaceManager.RemoveAllWithTag("Title Menu");
                        Interface.InterfaceManager.RemoveAllWithTag("Prompt");

                        Timer.Create("FakeConnectionLatency", 3500, 1, () =>
                        {
                            Settings.SetValue<string>("LastIP", ipAddressTextbox.Text);
                            Settings.SetValue<int>("LastPort", int.Parse(portTextbox.Text));

                            Timer.Create("ConnectionTimeout", 6500, 1, () =>
                            {
                                if (NetworkHelper.Client.ConnectionStatus != Lidgren.Network.NetConnectionStatus.Connected)
                                {
                                    ConnectingScreen.Text = "COULD NOT\n CONNECT";
                                    ConnectingScreen.DestColor = Color.Red;
                                    NetworkHelper.Disconnect();

                                    Timer.Create("ConnectionTimeoutBackToMenu", 3500, 1, () =>
                                    {
                                        State = GameState.AtMenu;
                                        CreateTitleMenu();
                                    });
                                }
                            });

                            NetworkHelper.Connect(ipAddressTextbox.Text, int.Parse(portTextbox.Text));
                        });
                    };
                    connectPanel.Add(connectButton);
                }
            };
            titleMenuPanel.Add(connectToServerButton);

            Interface.TextButton hostNewServerButton = new Interface.TextButton();
            hostNewServerButton.Tag = "Title Menu";
            hostNewServerButton.Text = "Host a new server";
            hostNewServerButton.Position = new Vector2(titleLabel.Position.X, padding + connectToServerButton.Position.Y + connectToServerButton.CalculateDimensions().Y);
            hostNewServerButton.Scale = labelScale;
            hostNewServerButton.HoverMode = Interface.TextButtonHoverMode.PointAt;
            hostNewServerButton.OnHover += (sender, e) => { GameContent.Sound("titlemenu_buttonhover").Play(0.5f, 1.0f, 0.0f); };
            hostNewServerButton.OnRelease += (sender, e) => {GameContent.Sound("titlemenu_buttonclick").Play(0.5f, 1.0f, 0.0f); };
            hostNewServerButton.OnRelease += (sender, e) =>
            {
                if (openPrompt != "Host")
                {
                    SetPrompt("Host");

                    Interface.Panel hostPanel = new Interface.Panel();
                    hostPanel.Tag = "Prompt";
                    hostPanel.AutoSize = true;
                    hostPanel.Position = new Vector2(titleMenuPanel.Position.X, titleMenuPanel.Position.Y + titleMenuPanel.CalculateDimensions().Y + panelPadding + titleMenuPanel.BorderScale);
                    hostPanel.BorderScale = titleMenuPanel.BorderScale;
                    Interface.InterfaceManager.Add(hostPanel);

                    Interface.Label serverFileLabel = new Interface.Label();
                    serverFileLabel.Tag = "Prompt";
                    serverFileLabel.Text = "Server";
                    serverFileLabel.Position = new Vector2(padding, padding);
                    serverFileLabel.Scale = labelScale;
                    hostPanel.Add(serverFileLabel);

                    Interface.TextBox serverFileTextBox = new Interface.TextBox();
                    serverFileTextBox.Tag = "Prompt";
                    serverFileTextBox.Text = "None selected.";
                    serverFileTextBox.MaxLength = 23;
                    serverFileTextBox.Position = new Vector2(padding, padding + serverFileLabel.Position.Y + serverFileLabel.CalculateDimensions().Y);
                    serverFileTextBox.BorderScale = 2;
                    serverFileTextBox.Scale = labelScale;
                    serverFileTextBox.Color = Color.Gray;
                    serverFileTextBox.Enabled = false;
                    hostPanel.Add(serverFileTextBox);

                    Interface.TextButton serverFileBrowseButton = new Interface.TextButton();
                    serverFileBrowseButton.Tag = "Prompt";
                    serverFileBrowseButton.Text = "Browse";
                    serverFileBrowseButton.Scale = 1;
                    serverFileBrowseButton.Position = new Vector2(serverFileLabel.Position.X + serverFileLabel.CalculateDimensions().X + 4, serverFileLabel.Position.Y + serverFileLabel.CalculateDimensions().Y - serverFileBrowseButton.CalculateDimensions().Y);
                    serverFileBrowseButton.OnRelease += (sender2, e2) =>
                    {
                        OpenFileDialog fileDialog = new OpenFileDialog();
                        fileDialog.FileOk += (sender3, e3) =>
                        {
                            if (fileDialog.FileName.Length > 23)
                            {
                                serverFileTextBox.Text = "..." + fileDialog.FileName.Substring(fileDialog.FileName.Length - 20);
                            }
                            else
                            {
                                serverFileTextBox.Text = fileDialog.FileName;
                            }
                        };
                        fileDialog.ShowDialog();
                    };
                    hostPanel.Add(serverFileBrowseButton);

                    Interface.Label portLabel = new Interface.Label();
                    portLabel.Tag = "Prompt";
                    portLabel.Text = "Port";
                    portLabel.Position = new Vector2(padding + serverFileTextBox.Position.X + serverFileTextBox.CalculateDimensions().X, padding);
                    portLabel.Scale = labelScale;
                    hostPanel.Add(portLabel);

                    Interface.TextBox portTextBox = new Interface.TextBox();
                    portTextBox.Tag = "Prompt";
                    portTextBox.Text = "25656";
                    portTextBox.Mode = Interface.TextBoxInputMode.Numerical;
                    portTextBox.MaxLength = 5;
                    portTextBox.Position = new Vector2(padding + serverFileLabel.Position.X + serverFileTextBox.CalculateDimensions().X, padding + serverFileLabel.Position.Y + serverFileLabel.CalculateDimensions().Y);
                    portTextBox.BorderScale = 2;
                    portTextBox.Scale = labelScale;
                    hostPanel.Add(portTextBox);

                    Interface.TextButton confirmButton = new Interface.TextButton();
                    confirmButton.Tag = "Title Menu";
                    confirmButton.Text = "Confirm";
                    confirmButton.Scale = labelScale;
                    confirmButton.Position = new Vector2(portTextBox.Position.X + portTextBox.CalculateDimensions().X + (int)Math.Round(padding * 1.5f), portTextBox.Position.Y + confirmButton.Scale);
                    confirmButton.OnHover += (sender2, e2) => { GameContent.Sound("titlemenu_buttonhover").Play(0.5f, 1.0f, 0.0f); };
                    confirmButton.OnRelease += (sender2, e2) => { GameContent.Sound("titlemenu_buttonclick").Play(0.5f, 1.0f, 0.0f); };
                    confirmButton.OnRelease += (sender2, e2) =>
                    {
                        Interface.Label startingLabel = new Interface.Label();
                        startingLabel.Tag = "Prompt";
                        startingLabel.Text = "Starting server";
                        startingLabel.Position = new Vector2(hostPanel.Position.X, hostPanel.Position.Y + hostPanel.CalculateDimensions().Y + panelPadding + hostPanel.BorderScale);
                        startingLabel.Scale = labelScale;
                        Interface.InterfaceManager.Add(startingLabel);

                        Timer.Create(null, 250, 3, () =>
                        {
                            startingLabel.Text += '.';
                        });

                        Timer.Create(null, 1000, 1, () =>
                        {
                            SetPrompt(null);

                            Process.Start("AphelionServer.exe", portTextBox.Text);
                        });
                    };
                    hostPanel.Add(confirmButton);
                }
            };
            titleMenuPanel.Add(hostNewServerButton);

            Interface.TextButton instructionsButton = new Interface.TextButton();
            instructionsButton.Tag = "Title Menu";
            instructionsButton.Text = "Instructions";
            instructionsButton.Position = new Vector2(titleLabel.Position.X, padding + hostNewServerButton.Position.Y + hostNewServerButton.CalculateDimensions().Y);
            instructionsButton.Scale = labelScale;
            instructionsButton.HoverMode = Interface.TextButtonHoverMode.PointAt;
            instructionsButton.OnHover += (sender, e) => { GameContent.Sound("titlemenu_buttonhover").Play(0.5f, 1.0f, 0.0f); };
            instructionsButton.OnRelease += (sender, e) => { GameContent.Sound("titlemenu_buttonclick").Play(0.5f, 1.0f, 0.0f); };
            instructionsButton.OnRelease += (sender, e) =>
            {
                if (openPrompt != "Instructions")
                {
                    SetPrompt("Instructions");

                    Interface.Label openingLabel = new Interface.Label();
                    openingLabel.Tag = "Prompt";
                    openingLabel.Text = "Opening instructions";
                    openingLabel.Position = new Vector2(titleMenuPanel.Position.X, titleMenuPanel.Position.Y + titleMenuPanel.CalculateDimensions().Y + panelPadding + titleMenuPanel.BorderScale);
                    openingLabel.Scale = labelScale;
                    Interface.InterfaceManager.Add(openingLabel);

                    Timer.Create(null, 250, 3, () =>
                    {
                        openingLabel.Text += '.';
                    });

                    Timer.Create(null, 1000, 1, () =>
                    {
                        SetPrompt(null);

                        using (RegistryKey registryKey = Registry.ClassesRoot.OpenSubKey(@"HTTP\Shell\Open\Command", false))
                        {
                            Process.Start(((string)registryKey.GetValue(null, null)).Split('"')[1], String.Format("file:///{0}/Instructions.html", Uri.EscapeUriString(Environment.CurrentDirectory.Replace('\\', '/'))));
                        }
                    });
                }
            };
            titleMenuPanel.Add(instructionsButton);

            Interface.TextButton optionsButton = new Interface.TextButton();
            optionsButton.Tag = "Title Menu";
            optionsButton.Text = "*Options";
            optionsButton.Position = new Vector2(titleLabel.Position.X, padding + instructionsButton.Position.Y + instructionsButton.CalculateDimensions().Y);
            optionsButton.Scale = labelScale;
            optionsButton.HoverMode = Interface.TextButtonHoverMode.PointAt;
            optionsButton.OnHover += (sender, e) => { GameContent.Sound("titlemenu_buttonhover").Play(0.5f, 1.0f, 0.0f); };
            optionsButton.OnRelease += (sender, e) => { GameContent.Sound("titlemenu_buttonclick").Play(0.5f, 1.0f, 0.0f); };
            optionsButton.OnRelease += (sender, e) =>
            {
                if (openPrompt != "Options")
                {
                    SetPrompt("Options");
                }
            };
            titleMenuPanel.Add(optionsButton);

            Interface.TextButton exitGameButton = new Interface.TextButton();
            exitGameButton.Tag = "Title Menu";
            exitGameButton.Text = "Exit Game";
            exitGameButton.Position = new Vector2(titleLabel.Position.X, padding + optionsButton.Position.Y + optionsButton.CalculateDimensions().Y);
            exitGameButton.Scale = labelScale;
            exitGameButton.HoverMode = Interface.TextButtonHoverMode.PointAt;
            exitGameButton.OnHover += (sender, e) => { GameContent.Sound("titlemenu_buttonhover").Play(0.5f, 1.0f, 0.0f); };
            exitGameButton.OnRelease += (sender, e) => { GameContent.Sound("titlemenu_buttonclick").Play(0.5f, 1.0f, 0.0f); };
            exitGameButton.OnRelease += (sender, e) =>
            {
                if (openPrompt != "Exit")
                {
                    SetPrompt("Exit");

                    Interface.Panel exitPanel = new Interface.Panel();
                    exitPanel.Tag = "Prompt";
                    exitPanel.AutoSize = true;
                    exitPanel.Position = new Vector2(titleMenuPanel.Position.X, titleMenuPanel.Position.Y + titleMenuPanel.CalculateDimensions().Y + panelPadding + titleMenuPanel.BorderScale);
                    exitPanel.BorderScale = titleMenuPanel.BorderScale;
                    Interface.InterfaceManager.Add(exitPanel);

                    Interface.Label confirmLabel = new Interface.Label();
                    confirmLabel.Tag = "Prompt";
                    confirmLabel.Text = "Are you sure?";
                    confirmLabel.Position = new Vector2(padding, padding);
                    confirmLabel.Scale = labelScale;
                    exitPanel.Add(confirmLabel);

                    Interface.TextButton yesButton = new Interface.TextButton();
                    yesButton.Tag = "Prompt";
                    yesButton.Text = "Yes";
                    yesButton.Position = new Vector2(confirmLabel.Position.X, confirmLabel.Position.Y + padding + confirmLabel.CalculateDimensions().Y);
                    yesButton.Scale = labelScale;
                    yesButton.OnHover += (sender2, e2) => { GameContent.Sound("titlemenu_buttonhover").Play(0.5f, 1.0f, 0.0f); };
                    yesButton.OnRelease += (sender2, e2) => { GameContent.Sound("titlemenu_buttonclick").Play(0.5f, 1.0f, 0.0f); Exit(); };
                    exitPanel.Add(yesButton);

                    Interface.TextButton noButton = new Interface.TextButton();
                    noButton.Tag = "Prompt";
                    noButton.Text = "No";
                    noButton.Position = new Vector2(yesButton.Position.X + yesButton.CalculateDimensions().X + topPadding, yesButton.Position.Y);
                    noButton.Scale = labelScale;
                    noButton.OnHover += (sender2, e2) => { GameContent.Sound("titlemenu_buttonhover").Play(0.5f, 1.0f, 0.0f); };
                    noButton.OnRelease += (sender2, e2) =>
                    {
                        GameContent.Sound("titlemenu_buttonclick").Play(0.5f, 1.0f, 0.0f);
                        SetPrompt(null);
                    };
                    exitPanel.Add(noButton);
                }
            };
            titleMenuPanel.Add(exitGameButton);

            titleMenuPanel.Position = new Vector2(ScreenBounds.X / 2 - titleMenuPanel.CalculateDimensions().X / 2, topPadding);

            Interface.Label debugInfoLabel = new Interface.Label();
            debugInfoLabel.Tag = "Title Menu";
            debugInfoLabel.Text = "APHELION (WIP) - DO NOT DISTRIBUTE";
            debugInfoLabel.Position = new Vector2(16 - 7, ScreenBounds.Y - debugInfoLabel.CalculateDimensions().Y - 8);
            Interface.InterfaceManager.Add(debugInfoLabel);
        }

        protected override void UnloadContent()
        {

        }

        public void SuccessfullyConnected()
        {
            ConnectingScreen.Text = "CONNECTED";
            ConnectingScreen.DestColor = Color.FromNonPremultiplied(0, 255, 0, 255);

            Timer.Create("SuccessfullyConnected", 3500, 1, () =>
            {
                    State = GameState.InGame;
                    Interface.InterfaceManager.RemoveAllWithTag("Title Menu");
                    Interface.InterfaceManager.RemoveAllWithTag("Prompt");

                    Interface.TextButton debugButton = new Interface.TextButton();
                    debugButton.Tag = "Debug Button";
                    debugButton.Text = "Toggle debug overlay (Currently OFF)";
                    debugButton.Scale = 1;
                    debugButton.Position = new Vector2(8, ScreenBounds.Y - 8 - debugButton.CalculateDimensions().Y);
                    debugButton.HoverMode = Interface.TextButtonHoverMode.Darken;
                    debugButton.OnRelease += (sender, e) =>
                    {
                        if (debugButton.Text.Contains("ON"))
                        {
                            debugButton.Text = "Toggle debug overlay (Currently OFF)";
                        }
                        else
                        {
                            debugButton.Text = "Toggle debug overlay (Currently ON)";
                        }
                    };
                    Interface.InterfaceManager.Add(debugButton);
            });
        }

        protected override void Update(GameTime time)
        {
            if (IsActive)
            {
                Interface.InterfaceManager.Update(time);
            }

            switch (State)
            {
                case GameState.AtMenu:
                    break;
                case GameState.InGame:
                    //if (IsActive)
                    //{
                        NetworkHelper.Predict();
                        NetworkHelper.Synchronize();
                    //}
                    break;
            }

            Particles.Update(time);
            EntityManager.Update();
            Timer.Update(time);
            base.Update(time);
        }

        protected override void Draw(GameTime time)
        {
            GraphicsDevice.Clear(Color.Black);

            if (State == GameState.AtMenu)
            {
                sprites.Begin();

                for (int i = 0; i < ScreenBounds.X / 100; i++)
                {
                    Utility.DrawDottedCircle(sprites, Color.Gray, ScreenBounds / 2, i * 100, 16);
                }

                for (int x = 0; x < Math.Ceiling(ScreenBounds.X / 1024); x++)
                {
                    for (int y = 0; y < Math.Ceiling(ScreenBounds.Y / 1024); y++)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            sprites.Draw(GameContent.Texture("environment\\background\\stars" + (i + 1)), new Vector2(x * 1024, y * 1024), Color.White);
                        }
                    }
                }

                sprites.End();
            }
            else if (State == GameState.Connecting)
            {
                ConnectingScreen.Draw(this, sprites, time);
            }
            else if (State == GameState.InGame)
            {
                Backdrop.Draw(sprites, MainCamera);

                if (NetworkHelper.LocalPlayer != null)
                {
                    MainCamera.Position = NetworkHelper.LocalPlayer.RenderPosition;
                    //Camera.Position = new Vector2((int)Math.Round(MathHelper.Lerp(Camera.Position.X, NetworkHelper.LocalPlayer.RenderPosition.X, 0.05f)), (int)Math.Round(MathHelper.Lerp(Camera.Position.Y, NetworkHelper.LocalPlayer.RenderPosition.Y, 0.5f)));
                }
                else
                {
                    MainCamera.Position = Vector2.Zero;
                }

                sprites.Begin(SpriteSortMode.Deferred, null, SamplerState.LinearClamp, null, null, null, MainCamera.GetTransformation());

                Particles.Draw(sprites);
                DrawSystem();
                Entities.EntityManager.Draw(sprites);

                sprites.End();

                HUD.Draw(sprites, this);
            }

            Interface.InterfaceManager.Draw(sprites);

            base.Draw(time);
        }

        private void DrawSystem()
        {
            Utility.DrawDottedCircleOptimized(sprites, MainCamera, Color.White, Vector2.Zero, (int)Math.Pow(2, 16), 32);

            int sunRadius = 6958;
            Vector2 sunPos = Vector2.Zero;

            int mercuryRadius = 24;
            int mercuryDistance = 8368;
            Vector2 mercuryPos = new Vector2(sunRadius + mercuryDistance, 0);

            int venusRadius = 60;
            int venusDistance = 8368;
            Vector2 venusPos = new Vector2(mercuryPos.X + venusDistance, 0);

            Utility.DrawCircleOptimized(sprites, MainCamera, Color.Red, sunPos, sunRadius);
            Utility.DrawCircleOptimized(sprites, MainCamera, Color.Orange, sunPos, sunRadius - 16);
            Utility.DrawCircleOptimized(sprites, MainCamera, Color.Gray, mercuryPos, mercuryRadius);
            Utility.DrawCircleOptimized(sprites, MainCamera, Color.DarkOrange, venusPos, venusRadius);
        }
    }
}