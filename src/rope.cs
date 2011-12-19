/*
 * Entry point for the program
 */

using System;
using System.Collections.Generic;
using System.Threading;

using ClassLibrary1.Collections.Generic;
using Statistics;

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
        RingBuffer<int> fps_ticks = Stats.Instance().GetFrameTickBuffer("Physics");
        int ticks = 0;
        Random r = new Random();
        universe.UpdateTimes();
        while (true) {
            universe.WaitForNextTick(TimeSpan.FromMilliseconds(20));
            Tick tick = new Tick();
            lock (objs) {
                universe.UpdateDudes ();
                universe.UpdateBro();
                universe.universe_time += 0.020*universe.bro.gamma;

                if (ticks % 60 == 0) {
                    double new_x = r.NextDouble() * 2 - 2;
                    double new_y = r.NextDouble() * 2 - 2;
                    double new_z = r.NextDouble() * 2 - 2;
                    objs.Add(new CoordinateEngine.RelativisticObject(new_x, new_y, new_z));
                }

                List<CoordinateEngine.RelativisticObject> to_remove = new List<CoordinateEngine.RelativisticObject>();
                foreach (CoordinateEngine.RelativisticObject obj in objs) {
                    obj.x[0] += .02;
                    if (obj.x[0] > 30) {
                        to_remove.Add(obj);
                    }
                }

                foreach (CoordinateEngine.RelativisticObject obj in to_remove) {
                    objs.Remove(obj);
                }
            }
            ticks++;
            fps_ticks.Add(tick.Tock());
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

