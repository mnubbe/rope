/*
 * Entry point for the program
 */

using Gtk;
using System.Collections.Generic;
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
            lock (universe) {
                canvas.Draw();
            }
            Thread.Sleep(20);
        }
    }

    public void EngineThread()
    {
        int tick = 0;
        while (true) {
            lock (universe) {
                List<CoordinateEngine.RelativisticObject> objs = universe.GetNPCs();
                objs.Clear();
                objs.Add(new CoordinateEngine.RelativisticObject(tick % 500, tick % 500, tick % 500));
            }
            tick++;
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

