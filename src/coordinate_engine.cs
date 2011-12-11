using System;
using System.Diagnostics; //Useful for Debug.Assert

public static class CoordinateEngine
{
    //An object, be it the camera, a rock, or otherwise, in the simulation
    public class RelativisticObject
    {
        //Big note: we assume units are done such that c=1.0.
        //Suggested units are seconds (though this can be whatever) with distance defined based on c
        //The value of c should not be needed by this class, so feel free to implement things abusing this fact
        
        
        //Explicit Physical State Variables: should be updated every frame
        public double[] x;      //Spatial coordinates of the object.
                                //Ideally this should be relative to a common,
                                //inertial frame of reference (no spinning, velocity is constant)
        public double[] v;      //Velocity of the object.  To start with c(light speed) is 1.0
                                //(the magnitude of these should never exceed it...)
        public double t_last_update;
                                //The time coordinate at which the object was most recently updated to.
                                //This is NOT the same as the time that the object percieves.
                                
        //Derived Physical State Variables: should be updated every frame based on the class's update methods
        //public double vrms;   //sqrt(sumsq(vx,vy,vz))
        public double gamma;    //Lorentz factor.  gamma=1/sqrt(1-vrms^2).  Should ONLY be updated with this in mind
        public double t_object; //Time elapsed as observed by the object.
                                //If it wore a watch, this is what it would say.

     
        

        
        //Ideas for later consideration:
        //public double mass;
        //public double elasticity; //1=fully elastic, 0=absorbs all impact energy
        //public string name;    //In case we wish to name our object
        
        
        //Constructors
        
        
        //Methods
        
        //Update gamma based on v[]
        public void updateGamma()
        {
            throw new NotImplementedException();
        }
        //Update position to new time based on moving to requested_time assuming constant v[]
        public void updatePosition(double requested_time)
        {
            Debug.Assert(requested_time>t_last_update,
            "It seems we are seeing something move back in time unexpectedly");
            throw new NotImplementedException();
        }
        
    }

    //Functions that operate on at least one RelativisticObject
    //Returns the relative velocity of the actor from the observer's perspective
    public static double[] velocityDifference(RelativisticObject observer, RelativisticObject actor)
    {
        Debug.Assert(observer.v.Length == actor.v.Length, "Coordinate dimensionality mismatch");
        double[] answer =  new double[observer.v.Length];
        for(int i=0;i<observer.v.Length;i++){
            answer[i]= (actor.v[i] - observer.v[i])/(1.0 -  actor.v[i] * observer.v[i]);
        }
        Debug.Assert(RMS(answer)<=1, "Speed of light exceeded");//This should never exceed c unless we have tacheons, which can break other parts of the simulation
        return answer;
    }
    
    //Returns the relative position difference as seen by the observer RelativisticObject
    //public static double[] observedPositionDifference(RelativisticObject observer, RelativisticObject actor)

    //Returns the position difference based on the reference frame (ignores Lorentz contraction)
    public static double[] referencePositionDifference(RelativisticObject observer, RelativisticObject actor)
    {
        double[] answer =  new double[observer.x.Length];
        for(int i=0;i<observer.x.Length;i++)
        {
            answer[i] = actor.x[i] - observer.x[i];
        }
        return answer;
    }
    
    //Functions that may be useful to have
    
    //RMS=Root Mean Squared.  Also known as the distance formula
    public static double RMS(double a,double b,double c)
    {
        return 1.0/Math.Sqrt(a*a+b*b+c*c);
    }
    public static double RMS(double[] a)
    {
        double sum=0;
        foreach(double x in a)
        {
            sum+=x*x;
        }
        return 1.0/System.Math.Sqrt(sum);
    }
    
    //Returns 1/sqrt(RMS(v))
    //public static double gamma(double[] v){}

}
