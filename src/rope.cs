//Entry point for the program
using Gtk;
using System;
using System.Threading;

public class dummyrope{
    public static void Main()
    {
        //Strategy: run most of the program as an instance of the rope class.
        //Allows some ease for things like volatile variables to be shared.
        rope ProgramInstance = new rope();
        ProgramInstance.Run();
    }
}

//Primary class for the program.
public class rope
{
    //Common member variables
    DateTime starting_time;
    DateTime current_frame;
    TimeSpan elapsed_time;
    double observer_proper_time;  //We sync this to the computer's clock and run the simulation around this
    
    double universe_time = 0;  //NOT SYNCED with the computer's clock! Relativity dilates this...
    
    //Camera: this is the user
    CoordinateEngine.RelativisticObject camera = new CoordinateEngine.RelativisticObject(0,0,0);
    
    //Threads
    System.Threading.Thread ApplicationThread;
    
    //Constructor (blank)
    public rope(){}
    
    //Effective entry point for the program
    public void Run(){
        starting_time = DateTime.Now;
        StartApplicationThread();
        while(ApplicationThread !=null) //Runs until the window is closed or closes itself
        {
            //Update universal time, must be accomplished in a short period of time for consistency
            current_frame = DateTime.Now;
            elapsed_time = current_frame.Subtract(starting_time);
            observer_proper_time = elapsed_time.TotalSeconds;
            //TODO: universe_time += delta_observer_proper_time/camera.gamma;
        
            //Test code for seeing that it is stepping through the frames.
            //Note: ToString is not very useful for most objects unless overriden
            Console.WriteLine (string.Format("{0:F2} seconds have elapsed, state is {1}",observer_proper_time, camera.ToString()));
        
        
        
        
        
        
        
            //Aim for 50fps?
            Thread.Sleep(20);
        }
    }

    //Starts this thread, then returns on the original thread.  Each thread has access to the same class variables.
    //Note that this could cause issues if both try to use the same one at the same time.  A lock statement can help
    //Alleviate this problem.
    public void StartApplicationThread()
    {
        ApplicationThread = new System.Threading.Thread(ApplicationThreadMethod);
        ApplicationThread.Start();
    }
    public void StopApplicationThread()
    {
        //Ugly way to do this, better ways exist
        ApplicationThread.Abort();
        ApplicationThread = null;
    }
    public void ApplicationThreadMethod()
    {
        Application.Init();
        new Canvas();
        Application.Run();
        ApplicationThread = null; //Upon exit of Application.Run()
    }
}

