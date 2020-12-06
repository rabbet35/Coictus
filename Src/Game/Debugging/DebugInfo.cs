﻿using RabbetGameEngine.GUI;
using RabbetGameEngine.Sound;
using RabbetGameEngine.SubRendering;

namespace RabbetGameEngine.Debugging
{
    /*A class for abstracting the process of displaying debug information on the screen when active.*/
    public static class DebugInfo
    {
        public static readonly string debugInfoTextPanelName = "debugInfo";
        /*Initialize the text panel for the debug info, can only be done if the mainGUI panel is created first*/
        public static void init()
        {
            GUIManager.addTextPanelToGUI(GUIHud.guiHudName, debugInfoTextPanelName, new GUITextPanel(new TextFormat(0.0F, 0.05F)
                .setLines(new string[]
                        {
                        ("press F3 to hide debug screen.")
                        }
                       ).setPanelColor(CustomColor.lightGrey)));
        }

        /*Shows and updates the debug info on the screen, Can be called every tick (Do not call every frame, too expensive)*/
        public static void displayOrClearDebugInfo()
        {
            if (GameSettings.debugScreen && GameInstance.get.thePlayer != null)
            {
                float entColAverage = Profiler.getAverageForProfile("EntCollisions");
                float colAverage = Profiler.getAverageForProfile("Collisions");
                float batchAverage = Profiler.getAverageForProfile("batching");
                float textBuildAverage = Profiler.getAverageForProfile("textBuild");
                float soundsAverage = Profiler.getAverageForProfile("sounds");
                float gameLoopAverage = Profiler.getAverageForProfile("Loop");
                GUIManager.unHideTextPanelInGUI(GUIHud.guiHudName, debugInfoTextPanelName);
                GUIManager.getTextPanelFormatFromGUI(GUIHud.guiHudName, debugInfoTextPanelName).setLines(
                       new string[]
                       {
                       /* ("X: " + GameInstance.get.thePlayer.getPosition().X.ToString("0.00")),
                        ("Y: " + GameInstance.get.thePlayer.getPosition().Y.ToString("0.00")),
                        ("Z: " + GameInstance.get.thePlayer.getPosition().Z.ToString("0.00")),
                        ("Velocity X: " + GameInstance.get.thePlayer.getVelocity().X.ToString("0.00")),
                        ("Velocity Y: " + GameInstance.get.thePlayer.getVelocity().Y.ToString("0.00 ")),
                        ("Velocity Z: " + GameInstance.get.thePlayer.getVelocity().Z.ToString("0.00")),*/
                       ("Profiler Averages (MS)" ),
                       ("   GameLoop: " + gameLoopAverage.ToString("0.00") + "ms." ),
                       ("   {" ),
                       ("       Entity Collisions: " + entColAverage.ToString("0.00") + "ms." ),
                       ("       World Collisions: " + colAverage.ToString("0.00") + "ms." ),
                       ("       Text building: " + textBuildAverage.ToString("0.00") + "ms." ),
                       ("       Batching: " + batchAverage.ToString("0.00") + "ms." ),
                       ("   }Residual: " + (gameLoopAverage - (entColAverage + colAverage + soundsAverage + textBuildAverage + batchAverage)).ToString("0.00") + "ms." ),

                       ("aux profile: " + Profiler.getAverageForProfile("aux").ToString("0.00") + "ms." ),
                       ("Entities: " + GameInstance.get.currentPlanet.getEntityCount()),
                       ("Projectiles: " + GameInstance.get.currentPlanet.getProjectileCount()),
                        ("VFX: " + GameInstance.get.currentPlanet.getVFXCount()),
                        ("Sounds: " + SoundManager.getPlayingSoundsCount()),
                        ("Draw Calls: " + Renderer.totalDraws),
                        ("Batches: " + BatchManager.batchCount),
                        ("Used memory: " + (Application.getMemoryUsageBytes()/1000000L).ToString() + "MB")
                       });
            }
            else
            {
                GUIManager.hideTextPanelInGUI(GUIHud.guiHudName, debugInfoTextPanelName);
            }
        }
    }
}
