//Entry point for the program
using Gtk;
using System;
using System.Threading;

public class dummyrope{
	public static void Main()
	{
		rope ProgramInstance = new rope();
		ProgramInstance.Run();
	}
}

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
	
	public void Run(){
		starting_time = DateTime.Now;
		StartApplicationThread();
		//while(true) or while(!exit)
		//for(int i=0;i<100;i++)
		while(ApplicationThread !=null)
		{
			//Update universal time, must be accomplished quickly
			
			current_frame = DateTime.Now;
			elapsed_time = current_frame.Subtract(starting_time);
			observer_proper_time = elapsed_time.TotalSeconds;
			
		
			Console.WriteLine (string.Format("{0:F2} seconds have elapsed, state is {1}",observer_proper_time, camera.ToString()));
		
		
		
		
		
		
		
			//Aim for 50fps?
			Thread.Sleep(20);
		}
	}

	
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
        ApplicationThread = null; //Upon exit
	}
}

