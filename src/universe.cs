/*
 * A representation of a universe.
 */

using System;
using System.Collections.Generic;

public class Universe
{
    // Actors.
    public CoordinateEngine.RelativisticObject bro = new
        CoordinateEngine.RelativisticObject(0, 0, 5);
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
        bro.updateGamma();
        start_time = DateTime.Now;
    }

    public List<CoordinateEngine.RelativisticObject> GetNPCs()
    {
        return dudes;
    }

    private static class DemoConsts{
        public static int    array_size = 40;
        public static double speed = 0.1;
        public static double spacing = 0.1;
        public static double radius = 1/Math.PI/2;
        public static double x_center = 0.0;
        public static double y_center = 0.0;
        public static double phase = 0.0;//Adds a phase to the circle progression.  Math.PI/2 per quarter rotation
    }
    //Populates the dudes, as a hard-coded demo
    public void InitDemo ()
    {
        for (int i=0; i<DemoConsts.array_size; i++) {
            for (int j=0; j<DemoConsts.array_size; j++) {
                CoordinateEngine.RelativisticObject new_guy = new CoordinateEngine.RelativisticObject(0,0,0);

                new_guy.vrms = DemoConsts.speed;
                double new_gamma = double.PositiveInfinity;
                if(DemoConsts.speed<1.0){
                    new_gamma = CoordinateEngine.computeGamma(DemoConsts.speed);
                }
                new_guy.gamma = new_gamma;

                dudes.Add(new_guy);//This mess of things at 0,0,0 sorts itself out after a few frames using UpdateDudes(), don't worry
            }
        }
    }

    //Updates the dudes (based on the bro's position for time) and a hard-coded movement
    //Also should be a good starting example for how to simulate things
    public void UpdateDudes ()
    {
        CoordinateEngine.RelativisticObject dude;
        double mytime;
        for (int i=0; i<DemoConsts.array_size; i++) {
            for (int j=0; j<DemoConsts.array_size; j++) {
                dude = dudes [j + DemoConsts.array_size * i];
                mytime = dude.observedUniverseTime (universe_time, bro);
                dude.x[0] = DemoConsts.x_center + i*DemoConsts.spacing + DemoConsts.radius * Math.Cos(
                    DemoConsts.phase + mytime*DemoConsts.speed/DemoConsts.radius);
                dude.x[1] = DemoConsts.x_center + j*DemoConsts.spacing + DemoConsts.radius * Math.Sin(
                    DemoConsts.phase + mytime*DemoConsts.speed/DemoConsts.radius);
                dude.v[0] = (-1) * DemoConsts.speed * Math.Sin (
                    DemoConsts.phase + mytime*DemoConsts.speed/DemoConsts.radius);
                dude.v[1] = (1)  * DemoConsts.speed * Math.Cos (
                    DemoConsts.phase + mytime*DemoConsts.speed/DemoConsts.radius);

                dude.t_last_update = mytime;
            }
        }
        //Console.WriteLine ("{0},{1},{2}", dudes [1].x [0], dudes [1].x [1], dudes [1].t_last_update);
    }

    public void UpdateBro()
    {
        bro.updateGamma();
        bro.updatePositionByDrifting(universe_time);
    }

    //Returns the time (in milliseconds) that has passed since current_frame was last updated
    public double UpdateTimes()
    {
        double answer = (current_frame.Subtract(DateTime.Now)).TotalMilliseconds;
        current_frame = DateTime.Now;
        elapsed_time  = current_frame.Subtract(start_time);
        return answer;
    }

    //Waits for the frame_interval to have elapsed since the last UpdateTimes() call or WaitForNextTick(...) call in system time before returning.
    //Returns the time (in milliseconds) that has passed since current_frame was last updated
    public double WaitForNextTick(TimeSpan frame_interval, int millisec_resolution = 1)
    {
        DateTime target = current_frame.Add(frame_interval);
        bool we_are_not_behind = (target>DateTime.Now);
        while(target>DateTime.Now)
        {
            System.Threading.Thread.Sleep(millisec_resolution);
        }
        if(we_are_not_behind){
            current_frame = current_frame.Add(frame_interval);
            elapsed_time  = current_frame.Subtract(start_time);
            return (double)frame_interval.Milliseconds;
        }else{//we are behind schedule, update times to whatever the current time is
            return UpdateTimes();
        }
    }
}

