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
        double interval;
        Random r = new Random();
        universe.UpdateTimes();
        while (true) {
            interval = universe.WaitForNextTick(TimeSpan.FromMilliseconds(20))/1000.0;
            //Console.WriteLine(interval);
            lock (objs) {
                universe.UpdateDudes ();
                universe.UpdateBro();
                universe.universe_time += interval*universe.bro.gamma;
                //universe.universe_time += 0.020*universe.bro.gamma;

                if (tick % 60 == 0) {
                    double new_x = r.NextDouble() * 2 - 2;
                    double new_y = r.NextDouble() * 2 - 2;
                    double new_z = r.NextDouble() * 2 - 2;
                    objs.Add(new CoordinateEngine.RelativisticObject(new_x, new_y, new_z));
                }

                List<CoordinateEngine.RelativisticObject> to_remove = new List<CoordinateEngine.RelativisticObject>();
                foreach (CoordinateEngine.RelativisticObject obj in objs) {
                    obj.x[0] += .02;
                    if (obj.x[0] > 40) {
                        to_remove.Add(obj);
                    }
                }

                foreach (CoordinateEngine.RelativisticObject obj in to_remove) {
                    objs.Remove(obj);
                }
            }
            //Thread.Sleep(20);
            tick++;
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

