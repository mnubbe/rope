using System;
//using System.Diagnostics; //Useful for Debug.Assert
//using NUnit.Framework;

public static class CoordinateEngine
//namespace CoordinateEngine
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
        public double t_last_update = 0;
                                //The time coordinate at which the object was most recently updated to.
                                //This is NOT the same as the time that the object percieves.
                                //TODO: t_last_update should be either universe creation time (ostensibly 0) or
                                //given to it in some way.  If an object is created after t=0 the first step will be
                                //MUCH bigger.
                                
        //Derived Physical State Variables: should be updated every frame based on the class's update methods
        public double vrms;     //sqrt(sumsq(vx,vy,vz))
        public double gamma;    //Lorentz factor.  gamma=1/sqrt(1-vrms^2).  Should ONLY be updated with this in mind
        public double t_object; //Time elapsed as observed by the object.
                                //If it wore a watch, this is what it would say.

        
        //Ideas for later consideration:
        //public double mass;
        //public double elasticity; //1=fully elastic, 0=absorbs all impact energy
        //public string name;    //In case we wish to name our object
        
        
        //Constructors: We need to define at minimum the position, velocity, and proper time
        //If proper time is not given, it will assume 0 (clock starts at 0, representing the object's proper "age"
        //If no velocity is given, it will assume v=0 (vector sense)
        
        //TODO: must initialize t_last_update
        public RelativisticObject(double[] xin, double[]vin, double t_object_in = 0)
        {

            //Debug.Assert(xin.Length==vin.Length);
            x = new double[xin.Length];
            v = new double[vin.Length];
            for(int i=0;i<xin.Length;i++){
                x[i]=xin[i];
                v[i]=vin[i];
            }
            //x=xin.Clone; //I am sad the following do not work
            //v=vin.Clone;
            t_object = t_object_in;
            updateGamma();
        }
        public RelativisticObject(double xin,double yin,double zin,double vxin,double vyin,double vzin,double t_object_in = 0){                
            x = new double[3];
            v = new double[3];
            x[0]=xin;
            x[1]=yin;
            x[2]=zin;
            v[0]=vxin;
            v[1]=vyin;
            v[2]=vzin;
            t_object = t_object_in;
            updateGamma();
        }
        public RelativisticObject(double[] xin, double t_object_in = 0){//Assumes v=0
            //Should probably have this as an override or something
            //x = xin.Clone();
            x = new double[xin.Length];
            for(int i=0;i<xin.Length;i++){
                x[i]=xin[i];
            }
            v = new double[3];
            for(int i=0;i<3;i++)
                v[i]=0.0;
            t_object=t_object_in;
            updateGamma();
        }
        public RelativisticObject(double xin, double yin, double zin, double t_object_in = 0){//Assumes v=0
            //Should probably have this as an override or something
            x = new double[3];
            x[0]=xin;
            x[1]=yin;
            x[2]=zin;
            v = new double[3];
            for(int i=0;i<3;i++)
                v[i]=0.0;
            t_object=t_object_in;
            updateGamma();
        }
        
        //Methods
        
        //Will initialize t_last_update to be consistent with the camera's (bro's) perception 
        //Right now is no different than accessing it directly
        void initializeLastUpdateTime(double init_t_update)
        {
            t_last_update = init_t_update;
        }
        
        //Update gamma based on v[].  Also updates vrms.
        public void updateGamma()
        {
            vrms = RMS (v);
            gamma = computeGamma(vrms);
        }
        //Update position to new time based on moving to requested_time assuming constant v[]
        //requested_target_time is by the universe's clock
        //Suggested way to call this is is objectname.updatePositionByDrifting(universe_time-RMS(referencePositionDifference(camera, thisobject)))
        //Or object.updatePositionByDrifting(observedUniverseTime(universe_time, camera))
        public void updatePositionByDrifting(double requested_target_time)
        {
            double interval = requested_target_time - t_last_update;
            //Debug.Assert(requested_target_time>t_last_update || interval<0,
            //"It seems we are seeing something move back in time unexpectedly.  Did you initializeLastUpdateTime(double init_t_update) ?");
            //update position
            for(int i=0;i<x.Length;i++)
            {
                x[i] += v[i]*interval;//Yes for this calculation it is that simple, (up to perspective changing, a 2nd order effect)
            }
            //update t
            t_last_update = requested_target_time;
            //update proper t (t_object)
            t_object += interval/gamma;
        }
        public double observedUniverseTime (double universe_time, RelativisticObject camera)
        {
            double t;
            t = (universe_time - RMS (referencePositionDifference (camera, this)));
            return t;
        }
    }

    //Functions that operate on at least one RelativisticObject
    //Returns the relative velocity of the actor from the observer's perspective
    public static double[] velocityDifference(RelativisticObject observer, RelativisticObject actor)
    {
        //Debug.Assert(observer.v.Length == actor.v.Length, "Coordinate dimensionality mismatch");
        //stupidly short way to implement:
        double[] negative_actor_v = new double[actor.v.Length];
        for(int i=0;i<actor.v.Length;i++){
            negative_actor_v[i] = -actor.v[i];
        }
        //Debug.Assert(RMS(answer)<=1, "Speed of light exceeded");//This should never exceed c unless we have tacheons, which can break other parts of the simulation
        return velocitySum(observer.v,negative_actor_v);
    }
    
    //Returns the relativistic sum of two velocities (adding v2 onto v1).  Does not require a RelativisticObject but likely to use RelativisticObject.v.
    //Note that the addition is NOT straightforward, which is why this is provided
    //Also, it is not unless they are parallel/antiparallel...
    //Reference: http://en.wikipedia.org/wiki/Velocity-addition_formula
    public static double[] velocitySum(double[] v1, double[] v2)
    {
        if(v1.Length!=v2.Length){
            throw new IndexOutOfRangeException("Array inputs do not match in dimension");
        }
        //Debug.Assert(v1.Length == v2.Length, "Coordinate dimensionality mismatch");
        double[] answer =  new double[v1.Length];
        double dot_product = dotProduct(v1,v2);
        double v1_gamma = computeGamma(v1);
        for(int i=0;i<v1.Length;i++){
            answer[i] = v1[i]+v2[i]/v1_gamma+dot_product*v1[i]*v1_gamma/(1+v1_gamma);
            answer[i] /= (1+dot_product);
        }
        //Debug.Assert(RMS(answer)<=1, "Speed of light exceeded");//This should never exceed c unless we have tacheons, which will break other parts of the simulation
        return answer;
    }

    //Returns the dot product of a and b (think vectors)
    public static double dotProduct(double[] a, double[] b){
        if(a.Length!=b.Length)
            throw new IndexOutOfRangeException("Array inputs do not match in dimension");
        double answer =0;
        for(int i=0;i<a.Length;i++)
            answer+=a[i]*b[i];
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
    
    //RMS=Root Mean Squared.  Also known by the distance formula or vector magnitude
    public static double RMS(double a,double b,double c)
    {
        return Math.Sqrt(a*a+b*b+c*c);
    }
    public static double RMS(double[] a)
    {
        double sum=0;
        foreach(double x in a)
        {
            sum+=x*x;
        }
        return System.Math.Sqrt(sum);
    }
    
    //Returns 1/sqrt(RMS(v))
    public static double computeGamma(double[] v)
    {
        double answer = 1.0-RMS(v);
        //Debug.Assert(RMS(v)<1.0, "Speed of light is exceeded on gamma calculation");
        answer = 1.0/Math.Sqrt(answer);//This line also will complain about vrms>1.0
        return answer;
    }
    public static double computeGamma(double vrms)
    {
        double answer = 1.0-vrms;
        //Debug.Assert(vrms<1.0, "Speed of light is exceeded on gamma calculation");
        answer = 1.0/Math.Sqrt(answer);//This line also will complain about vrms>1.0
        return answer;
    }

}
