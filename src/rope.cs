/*
 * Entry point for the program
 */

using Gtk;
using System.Threading;

public class rope
{
    private static Thread renderer;
    private static Thread engineer;
    private static Universe universe;
    private static Canvas canvas;

    public static void Main()
    {
        Application.Init();
        renderer = new Thread(RenderThread);
        engineer = new Thread(EngineThread);
        universe = new Universe(); // Let there be light.
        canvas = new Canvas(universe);

        renderer.Start();
        engineer.Start();
        Application.Run();
    }

    public static void RenderThread()
    {
        while (true) {
            canvas.Draw();
            Thread.Sleep(20);
        }
    }

    public static void EngineThread()
    {
        while (true) {
            // engine.Tick();
            Thread.Sleep(20);
        }
    }
}

