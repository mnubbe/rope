/*
 * Entry point for the program
 */

using Gtk;
using System.Threading;

public class Rope
{
    private Thread renderer;
    private Thread engineer;
    private Universe universe;
    private Canvas canvas;

    public void Start()
    {
        Application.Init();
        renderer = new Thread(RenderThread);
        engineer = new Thread(EngineThread);
        universe = new Universe(); // Let there be light.
        canvas = new Canvas(this, universe);

        renderer.Start();
        engineer.Start();
        Application.Run();
    }

    public void RenderThread()
    {
        while (true) {
            canvas.Draw();
            Thread.Sleep(20);
        }
    }

    public void EngineThread()
    {
        while (true) {
            // engine.Tick();
            Thread.Sleep(20);
        }
    }

    public void Shutdown()
    {
        renderer.Abort();
        engineer.Abort();
    }
}


public class EntryPoint
{
    public static void Main()
    {
        new Rope().Start();
    }
}

