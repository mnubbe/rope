/*
 * Entry point for the program
 */

using System;
using System.Collections.Generic;
using System.Threading;


public class Rope
{
    private Thread engineer;
    private Universe universe;
    private Canvas canvas;
    private List<CoordinateEngine.RelativisticObject> objs;

    public void Start()
    {
        engineer = new Thread(EngineThread);
        universe = new Universe(); // Let there be light.
        objs = universe.GetNPCs();
        canvas = new Canvas(this, universe);
        //Utilities.SetWindowTitle("rope rope rope");

        engineer.Start();
        canvas.Run(30.0, 0.0);
        Shutdown();
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
            Console.WriteLine(String.Format("PHYS: {0:f} ms", ms_taken));
            tick++;
            Thread.Sleep(20);
        }
    }

    public void Shutdown()
    {
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

