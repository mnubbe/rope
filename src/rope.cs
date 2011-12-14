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
        canvas = new Canvas(universe);
        //Utilities.SetWindowTitle("rope rope rope");

        engineer.Start();
        canvas.Run(30.0, 0.0);
        Shutdown();
    }

    public void EngineThread ()
    {
        int tick = 0;
        Random r = new Random();
        while (true) {
            lock (objs) {
                universe.UpdateDudes ();
                universe.universe_time += 0.020;

                if (tick % 60 == 0) {
                    double new_x = r.NextDouble() * 2 - 2;
                    double new_y = r.NextDouble() * 2 - 2;
                    double new_z = r.NextDouble() * 2 - 2;
                    objs.Add(new CoordinateEngine.RelativisticObject(new_x, new_y, new_z));
                }

                List<CoordinateEngine.RelativisticObject> to_remove = new List<CoordinateEngine.RelativisticObject>();
                foreach (CoordinateEngine.RelativisticObject obj in objs) {
                    obj.x[0] += .02;
                    if (obj.x[0] > 3) {
                        to_remove.Add(obj);
                    }
                }

                foreach (CoordinateEngine.RelativisticObject obj in to_remove) {
                    objs.Remove(obj);
                }
            }
            tick++;
            Thread.Sleep (20);
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

