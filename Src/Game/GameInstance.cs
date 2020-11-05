﻿using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;
using RabbetGameEngine.Debugging;
using RabbetGameEngine.GUI;
using RabbetGameEngine.Sound;
using RabbetGameEngine.Src.Game;
using RabbetGameEngine.Text;
using RabbetGameEngine.VisualEffects;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace RabbetGameEngine
{

    /*This class is the main game class. It contains all the execution code for rendering, logic loops and loading.*/
    public class GameInstance : GameWindow
    {
        public static int temp = 0;
        public static readonly string entityLabelName = "entLabel";
        private static GameInstance instance;
        private static Random privateRand;
        private static int windowWidth;
        private static int windowHeight;
        private static int screenWidth;
        private static int screenHeight;
        private static Vector2 windowCenter;
        private static float dpiY;
        public EntityPlayer thePlayer;
        public Planet currentPlanet;

        /// <summary>
        /// Will be true if there has been atleast one onTick() call since last frame.
        /// </summary>
        private bool doneOneTick = false; 

        public GameInstance(GameWindowSettings gameWindowSettings, NativeWindowSettings windowSettings) : base(gameWindowSettings, windowSettings)
        {

            GameInstance.instance = this;
            GameInstance.windowWidth = this.ClientRectangle.Size.X;
            GameInstance.windowHeight = this.ClientRectangle.Size.Y;
            Title = Application.applicationName;
            int iconWidth, iconHeight;
            byte[] data;
            IconLoader.getIcon("icon", out iconWidth, out iconHeight, out data);
            Icon = new WindowIcon(new OpenTK.Windowing.Common.Input.Image[] { new OpenTK.Windowing.Common.Input.Image(iconWidth, iconHeight, data) });
          
            //TODO: Implement more elegent technique of centering and sizing window, as well as obtaining monitor resolution. (Will most likely require OpenTK future development updates). Test next update of OpenTK.

            this.WindowState = OpenTK.Windowing.Common.WindowState.Maximized;
            screenWidth = ClientRectangle.Size.X;
            screenHeight = ClientRectangle.Size.Y;
            int hw = ClientRectangle.HalfSize.X;
            int hh = ClientRectangle.HalfSize.Y;
            this.WindowState = OpenTK.Windowing.Common.WindowState.Normal;
            ClientRectangle = new Box2i(hw - hw / 2, hh - hh / 2, hw + hw / 2, hh + hh / 2);
            Context.MakeCurrent();
        }
        
        protected override void OnLoad()
        {
            try
            {
                GameInstance.privateRand = new Random();
                GameSettings.loadSettings();
                TextUtil.loadAllFoundTextFiles();
                SoundManager.init();
                windowCenter = new Vector2(this.Location.X / this.Bounds.Size.X + this.Bounds.Size.X / 2, this.Location.Y / this.Bounds.Size.Y + this.Bounds.Size.Y / 2);
                setDPIScale();
                Renderer.init();
                TicksAndFrames.init(30);
                MainGUI.init();
                DebugInfo.init();
                currentPlanet = new Planet(0xdeadbeef);
                //create and spawn player in new world
                thePlayer = new EntityPlayer(currentPlanet, "Steve", new Vector3(0, 3, 2));
                for (int i = 0; i < 2; i++)
                {
                    currentPlanet.spawnEntityInWorld(new EntityCactus(currentPlanet, new Vector3(0, 10, 0)));
                }
                currentPlanet.spawnEntityInWorld(thePlayer);

                //temp sound examples
                SoundManager.playSoundLoopingAt("waterroll", new Vector3(16, 1, 16), 0.1F);
                currentPlanet.spawnVFXInWorld(new VFXStaticText3D("waterroll", GameSettings.defaultFont, "waterroll.ogg, 10% volume", new Vector3(16,2.5F,16), 5.0F, CustomColor.white));
                SoundManager.playSoundLoopingAt("waterroll_large", new Vector3(-16, 1, -16), 0.5F);
                currentPlanet.spawnVFXInWorld(new VFXStaticText3D("waterroll_large", GameSettings.defaultFont, "waterroll_large.ogg, 50% volume", new Vector3(-16,2.5F,-16), 5.0F, CustomColor.white));

                Input.setCursorHiddenAndGrabbed(true);
            }
            catch(Exception e)
            {
                Application.error("Failed load game, Exception: " + e.Message + "\nStack Trace: " + e.StackTrace);
            }
            base.OnLoad();
        }

        public Size getGameWindowSize()
        {
            return new Size(ClientRectangle.Size.X, ClientRectangle.Size.Y);
        }

        /// <summary>
        /// Should be called after toggling gamesettings.debugscreen
        /// </summary>
        public void onToggleEntityLabels()
        {
            if(GameSettings.entityLabels)
            {
                foreach(KeyValuePair<int, Entity> e in currentPlanet.entities)
                {
                    currentPlanet.addDebugLabel(new VFXMovingText3D(e.Value, entityLabelName, GameSettings.defaultFont, "Entity: " +  e.Key.ToString(), new Vector3(0,1,0), 2.0F, CustomColor.white));
                }
            }
            else
            {
                foreach(VFX v in currentPlanet.vfxList)
                {
                    if(v.vfxName == entityLabelName)
                    {
                        v.ceaseToExist();
                    }
                }
            }
        }

        /*overriding OpenTk render update function, called every frame.*/
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            
            Input.updateInput();
            try
            {
                doneOneTick = false;
                TicksAndFrames.doOnTickUntillRealtimeSync(onTick);
            }
            catch(Exception e)
            {
                Application.error("Failed to run game tick, Exception: " + e.Message + "\nStack Trace: " + e.StackTrace);
            }
            TicksAndFrames.updateFPS();
            SoundManager.onFrame();
            thePlayer.onCameraUpdate();//do this before calling on tick to prepare camera variables
            currentPlanet.onFrame();//should be called before rendering world since this may prepare certain elements for a frame perfect render
            Renderer.onFrame();
            Renderer.renderAll();
        }

        /*Overriding OpenTK resize function, called every time the game window is resized*/
        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            windowWidth = this.ClientRectangle.Size.X;
            windowHeight = this.ClientRectangle.Size.Y;
            Renderer.onResize();
        }

        protected override void OnUnload()
        {
            if (currentPlanet != null)
                currentPlanet.onLeavingPlanet();
            Renderer.onClosing();
            SoundManager.onClosing();
        }

        protected override void OnFocusedChanged(FocusedChangedEventArgs e)
        {
            if (thePlayer != null && !thePlayer.paused)
            {
                //pausing the game if the window focus changes
                pauseGame();
            }
            base.OnFocusedChanged(e);
        }

        /*Each itteration of game logic is done here*/
        private void onTick()
        {
            Profiler.beginEndProfile("Loop");
            if(!doneOneTick)
            {
                Renderer.beforeTick();
            }
            windowCenter = new Vector2(this.Location.X / this.Bounds.Size.X + this.Bounds.Size.X / 2, this.Location.Y / this.Bounds.Size.Y + this.Bounds.Size.Y / 2);
            GUIManager.onTick();
            MainGUI.onTick();
            //for(int i = 0; i < 10; i++)
            //currentPlanet.spawnVFXInWorld(new VFXSnowParticle(new Vector3(0,10,0)));
            currentPlanet.onTick();
            SoundManager.onTick();
            Profiler.updateAverages();
            Renderer.onTickEnd();
            Profiler.beginEndProfile("Loop");

            doneOneTick = true;
        }
        
        /*Called when player lands direct hit on a cactus, TEMPORARY!*/
        public static void onDirectHit()
        {
            MainGUI.onDirectHit();
        }

        /*Called when player lands air shot on a cactus, TEMPORARY!*/
        public static void onAirShot()
        {
            MainGUI.onAirShot();
        }

        public void pauseGame()
        {
            thePlayer.togglePause();
            Input.setCursorHiddenAndGrabbed(!thePlayer.paused);
        }

        private void setDPIScale()
        {
            TryGetCurrentMonitorDpi(out _, out dpiY);
        }

        public static int gameWindowWidth { get => windowWidth; }
        public static int gameWindowHeight { get => windowHeight; }
        public static Vector2 gameWindowCenter { get => windowCenter; }
        public static float aspectRatio { get => (float)windowWidth / (float)windowHeight; }
        public static float dpiScale { get => (float)windowHeight / dpiY; }
        public static Random rand { get => privateRand; }
        public static GameInstance get { get => instance; }
    }
}
