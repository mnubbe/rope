/*
 * A representation of a universe.
 */

using System;
using System.Collections.Generic;


public class Universe
{
    // Actors.
    CoordinateEngine.RelativisticObject bro = new
        CoordinateEngine.RelativisticObject(0, 0, 0);
    List<CoordinateEngine.RelativisticObject> dudes = new
        List<CoordinateEngine.RelativisticObject>();

    // Accounting.
    private DateTime start_time;
    private DateTime current_frame;
    private TimeSpan elapsed_time;
    

    public List<CoordinateEngine.RelativisticObject> GetNPCs()
    {
        return dudes;
    }
}

