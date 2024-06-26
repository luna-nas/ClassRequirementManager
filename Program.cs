﻿using System.Diagnostics;
using System.Numerics;
using ImGuiNET;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace ClassRequirementManager;

class Program
{
    public static Sdl2Window? Window;
    private static GraphicsDevice? _gd;
    private static ImGuiController? _controller;
    private static CommandList? _cl;
    private static string _lastOpened = "";
    private static string _lastClosed = "";

    private static bool _addClass;

    private static Vector3 _clearColor = new(0.45f, 0.55f, 0.6f);

    public static bool SaveDialog = false;
    static void Main()
    {
        DataManager.Load();
        VeldridStartup.CreateWindowAndGraphicsDevice(
            new WindowCreateInfo(50, 50, 800, 800, WindowState.Normal, "Class Requirement Manager"),
            new GraphicsDeviceOptions(false, null, true, ResourceBindingModel.Default, true, true),
            preferredBackend: GraphicsBackend.OpenGL,
            out Window,
            out _gd
        );

        Window.SetCloseRequestedHandler(() =>
        {
            var loadClasses = DataManager.LoadClasses();
            if (DataManager.Classes.Count != loadClasses.Count) goto NotEqual;
            for (var i = 0; i < DataManager.Classes.Count; i++)
            {
                if (loadClasses[i] != DataManager.Classes[i]) goto NotEqual;
            }

            var loadedTracks = DataManager.LoadTracks();
            if (DataManager.Tracks.Count != loadedTracks.Count) goto NotEqual;
            for (var i = 0; i < DataManager.Tracks.Count; i++)
            {
                if (loadedTracks[i] != DataManager.Tracks[i]) goto NotEqual;
            }

            return false;
            
            NotEqual:
            SaveDialog = true;

            return true;
        });

        Window.Resized += () =>
        {
            _gd.MainSwapchain.Resize((uint)Window.Width, (uint)Window.Height);
            _controller.WindowResized(Window.Width, Window.Height);
        };

        _cl = _gd.ResourceFactory.CreateCommandList();
        _controller = new ImGuiController(_gd, _gd.MainSwapchain.Framebuffer.OutputDescription, Window.Width,
            Window.Height);
        
        var stopwatch = Stopwatch.StartNew();
        while (Window.Exists)
        {
            float deltaTime = stopwatch.ElapsedTicks / (float)Stopwatch.Frequency;
            stopwatch.Restart();

            InputSnapshot snapshot = Window.PumpEvents();
            if (!Window.Exists) break;
            _controller.Update(deltaTime, snapshot);

            MainUi.Display();
            
            _cl.Begin();
            _cl.SetFramebuffer(_gd.MainSwapchain.Framebuffer);
            _cl.ClearColorTarget(0, new RgbaFloat(_clearColor.X, _clearColor.Y, _clearColor.Z, 1));
            _controller.Render(_gd, _cl);
            _cl.End();
            _gd.SubmitCommands(_cl);
            _gd.SwapBuffers(_gd.MainSwapchain);
        }
        
        _gd.WaitForIdle();
        _controller.Dispose();
        _cl.Dispose();
        _gd.Dispose();
    }

    
}