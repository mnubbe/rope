/*
 * A representation of a universe.
 */

using System;
using System.Collections.Generic;

public class Universe
{
    // Actors.
    public CoordinateEngine.RelativisticObject bro = new
        CoordinateEngine.RelativisticObject(0, 0, 0);
    List<CoordinateEngine.RelativisticObject> dudes = new
        List<CoordinateEngine.RelativisticObject>();

    // Accounting.
    private DateTime start_time;
    private DateTime current_frame;
    private TimeSpan elapsed_time;

    public double universe_time;

    public Universe ()
    {
        universe_time = 0;
        InitDemo ();
    }

    public List<CoordinateEngine.RelativisticObject> GetNPCs()
    {
        return dudes;
    }


    //Populates the dudes, as a hard-coded demo
    public void InitDemo ()
    {
        for (int i=0; i<4; i++) {
            for (int j=0; j<4; j++) {
                dudes.Add (new CoordinateEngine.RelativisticObject (i * 4*.100 + 4*.050, 4*j * .1, 0));
            }
        }
    }

    //Updates the dudes (based on the bro's position for time) and a hard-coded movement
    //Also should be a good starting example for how to simulate things
    public void UpdateDudes ()
    {
        CoordinateEngine.RelativisticObject dude;
        double mytime;
        for (int i=0; i<4; i++) {
            for (int j=0; j<4; j++) {
                dude = dudes [j + 4 * i];
                mytime = dude.observedUniverseTime (universe_time, bro);
                dude.x [0] = 1 * Math.Cos (mytime * 1.2) + 4*i * .1;
                dude.x [1] = 1 * Math.Sin (mytime * 1.2) + 4*j * .1;
                //dude.t_last_update = dude.observedUniverseTime (universe_time, bro);
                //dudes[j+4*i].x[0] = 100*Math.Sin(
                dude.t_last_update = mytime;
            }
        }
        //Console.WriteLine ("{0},{1},{2}", dudes [1].x [0], dudes [1].x [1], dudes [1].t_last_update);
    }
}

