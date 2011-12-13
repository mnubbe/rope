/*
 * Entry point for the program
 */

using Gtk;
using System;
using System.Collections.Generic;
using System.Threading;

public class Rope
{
    private Thread renderer;
    private Thread engineer;
    private Universe universe;
    private Canvas canvas;
    private List<CoordinateEngine.RelativisticObject> objs;

    public void Start()
    {
        Application.Init();
        renderer = new Thread(RenderThread);
        engineer = new Thread(EngineThread);
        universe = new Universe(); // Let there be light.
        objs = universe.GetNPCs();
        canvas = new Canvas(this, universe);

        renderer.Start();
        engineer.Start();
        Application.Run();
    }

    public void RenderThread()
    {
        while (true) {
            DateTime start = DateTime.Now;
            lock (objs) {
                canvas.Draw();
            }
            double ms_taken = (DateTime.Now - start).TotalMilliseconds;
            Console.WriteLine(String.Format("REND: {0:f} ms", ms_taken));
            Thread.Sleep(20);
        }
    }

    public void EngineThread()
    {
        int tick = 0;
        while (true) {
            DateTime start = DateTime.Now;
            lock (objs) {
                objs.Clear();
                objs.Add(new CoordinateEngine.RelativisticObject(tick % 500, tick % 500, tick % 500));
            }
            double ms_taken = (DateTime.Now - start).TotalMilliseconds;
            Console.WriteLine(String.Format("REND: {0:f} ms", ms_taken));
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

