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

        //Returns a RO that is positioned relative to the observer in the observer's rest frame
        //More or less does a lorentz transform of that object in reference to the observer
        public RelativisticObject GetApparentObject(RelativisticObject observer)
        {
            return GetApparentObject(observer.x,observer.v);
        }
        public RelativisticObject GetApparentObject(double[] observer_position, double[] observer_v)
        {
            //Calculate v
            double[] newv = velocityDifference(observer_v,v);
            //Calculate position (aka x)
            double[] newx = ApparentPosition(x,observer_position,observer_position);
            return new RelativisticObject(newx,newv);
        }
    }

    //Functions that operate on at least one RelativisticObject
    //Returns the relative velocity of the actor from the observer's perspective
    public static double[] velocityDifference(RelativisticObject observer, RelativisticObject actor)
    {
        return velocityDifference(observer.v,actor.v);
    }
    public static double[] velocityDifference(double[] observer_v, double[] actor_v)
    {
        double[] negative_actor_v = new double[actor_v.Length];
        for(int i=0;i<actor_v.Length;i++){
            negative_actor_v[i] = -actor_v[i];
        }
        return velocitySum(observer_v,negative_actor_v);
    }
    
    //Returns the relativistic sum of two velocities (adding u onto frame_boost_v).  Does not require a RelativisticObject but likely to use RelativisticObject.v.
    //Note that the addition is NOT straightforward, which is why this is provided
    //Also, it is not commutative unless they are parallel/antiparallel...
    //Reference: http://en.wikipedia.org/wiki/Velocity-addition_formula
    //Best way to think about the operation: take u and boost it by frame_boost_v
    public static double[] velocitySum(double[] frame_boost_v, double[] u)
    {
        if(frame_boost_v.Length!=u.Length){
            throw new IndexOutOfRangeException("Array inputs do not match in dimension");
        }
        double[] answer =  new double[frame_boost_v.Length];
        double dot_product = dotProduct(frame_boost_v,u);
        double frame_gamma = computeGamma(frame_boost_v);
        for(int i=0;i<frame_boost_v.Length;i++){
            answer[i] = frame_boost_v[i]+u[i]/frame_gamma+
                dot_product*frame_boost_v[i]*frame_gamma/(1+frame_gamma);
            answer[i] /= (1+dot_product);
        }
        //Debug.Assert(RMS(answer)<=1, "Speed of light exceeded");//This should never exceed c unless we have tacheons, which will break other parts of the simulation
        return answer;
    }
    public static double[] velocitySum(RelativisticObject observer, double[] u)
    {//Note: u is seen from observer's perspective, sum returns from the universe's perspective
        return velocitySum(observer.v,u);
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

    //Returns the position difference based on the reference frame (includes lorentz transform for observer)
    public static double[] referencePositionDifference(RelativisticObject observer, RelativisticObject actor)
    {
        double[] answer =  new double[observer.x.Length];
        for(int i=0;i<observer.x.Length;i++)
        {
            answer[i] = actor.x[i] - observer.x[i];
        }
        //Assumption that the time is based on distance (true if observer sees actor in terms of a photon path)
        answer = LorenzTransform(answer,observer.v,-RMS(answer));
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
        return Math.Sqrt(sum);
    }
    //Returns a+b
    private static double[] sum(double[] a,double[] b)
    {
        if(a.Length!=b.Length){
            throw new IndexOutOfRangeException("Array inputs do not match in dimension");
        }
        double[] answer = new double[a.Length];
        for(int i=0;i<a.Length;i++){
            answer[i]=a[i]+b[i];
        }
        return answer;
    }
    //Returns a-b
    private static double[] difference(double[] a,double[] b)
    {
        if(a.Length!=b.Length){
            throw new IndexOutOfRangeException("Array inputs do not match in dimension");
        }
        double[] answer = new double[a.Length];
        for(int i=0;i<a.Length;i++){
            answer[i]=a[i]-b[i];
        }
        return answer;
    }
    
    //Returns 1/sqrt(RMS(v))
    public static double computeGamma(double[] v)
    {
        /*double answer = 1.0-RMS(v)*RMS(v);
        //Debug.Assert(RMS(v)<1.0, "Speed of light is exceeded on gamma calculation");
        answer = 1.0/Math.Sqrt(answer);//This line also will complain about vrms>1.0
        return answer;*/
        return computeGamma(RMS (v));//Also should work
    }
    public static double computeGamma(double vrms)
    {
        double answer = 1.0-vrms*vrms;
        //Debug.Assert(vrms<1.0, "Speed of light is exceeded on gamma calculation");
        answer = 1.0/Math.Sqrt(answer);//This line also will complain about vrms>1.0
        return answer;
    }

    //Should this go somewhere else?
    public static OpenTK.Vector3 toVector3(double[] input)
    {
        if(input.Length!=3){
            throw new IndexOutOfRangeException();
        }
        return new OpenTK.Vector3((float)input[0],(float)input[1],(float)input[2]);
    }
    public static double[] toDoubleArray(OpenTK.Vector3 input){
        return new double[3] {(double)input.X,(double)input.Y,(double)input.Z};
    }
    public static double calculateRedshiftZ(RelativisticObject emitter, RelativisticObject observer)
    {
        double[] velocity_difference = velocityDifference(emitter,observer);

        //all velocities below are implicitly from the difference
        //which gamma do we care about?  v parallel or v total?
        //Arguably v total, actually.  Think time dilation
        double velocity_rms = RMS (velocity_difference);
        double mygamma = computeGamma(velocity_rms);
        double[] position_difference = referencePositionDifference(observer,emitter);
        double v_parallel_component = dotProduct(velocity_difference,position_difference)/RMS (position_difference);

        double answer = -1.0 + mygamma * (1+v_parallel_component);
        return answer;
    }
    public static double calculateRedshiftZ(RelativisticObject relative_emitter, double[] observer_position)
    {//Requires the observer to be stationary at a given position
        double[] v = relative_emitter.v;

        double velocity_rms = relative_emitter.vrms;
        double mygamma = relative_emitter.gamma;
        double[] position_difference = difference(observer_position,relative_emitter.x);
        double v_parallel_component = dotProduct(v,position_difference)/RMS (position_difference);

        double answer = -1.0 + mygamma * (1+v_parallel_component);
        return answer;
    }
    //Returns the Lorenz transform of the position vector x_input
    //See http://en.wikipedia.org/wiki/Lorenz_transformation#Boost_in_any_direction
    //In this method we are ignoring the time coordinate, but otherwise emulating the matrix operation
    public static double[] LorenzTransform(double[] x_input, double[] v, double t)
    {
        if(x_input.Length!=v.Length){
            throw new IndexOutOfRangeException();
        }
        int dim = x_input.Length;//dimension, likely 3 but keeps this flexible
        double vrms = RMS (v);
        double gamma = computeGamma(v);
        double[] answer = new double[dim];

        if(vrms!=0){//then compute normally
            for(int i=0;i<dim;i++)
            {
                answer[i]=0;
                //time part
                answer[i]+= - v[i]*gamma*t;
                //position parts
                for(int j=0;j<dim;j++)
                {
                    if(j==i){
                        answer[i]+=x_input[j];
                    }
                    answer[i]+=x_input[j]*(gamma-1)*v[i]*v[j]/vrms/vrms;
                }
            }
            return answer;
        }else{//v==0, so the transformation is the identity matrix.  Aka return the input.
              //(otherwise we get NAN if we compute 1+(1-1)*(0/0), which should approach 1)
            return x_input;
        }
    }
    //Useful to use for calculating where something at x_input should appear based on the observer's x and v
    public static double[] ApparentPosition(double[] x_input, double[] x_observer, double[] v_observer)
    {
        if(x_input.Length!=x_observer.Length||x_input.Length!=v_observer.Length){
            throw new IndexOutOfRangeException();
        }
        int dim = x_input.Length;
        double[] answer = new double[dim];
        for(int i=0;i<dim;i++){
            answer[i]=x_input[i]-x_observer[i];
        }
        //Assumption: object is in time/position for a photon to reach the observer
        // --> thus t = -(dist_to_observer)
        double t = -RMS(answer);

        answer = LorenzTransform(answer, v_observer,t);
        for(int i=0;i<dim;i++){
            answer[i]+=x_observer[i];
        }
        return answer;
    }
    //Rapidity: has the advantage that if accelerating in one direction it will increase steadily
    //See http://en.wikipedia.org/wiki/Rapidity for more info
    //Note, several assumptions were made about it in more than 1 dimension... may be invalid
    public static double[] Rapidity(double[] v)
    {
        int dim = v.Length;
        double[] answer = new double[dim];
        double vrms = RMS(v);
        if(vrms!=0){//Then compute it normally
            double r_rms = ATanH(vrms);
            for(int i=0;i<dim;i++){
                answer[i]=r_rms*v[i]/vrms;
            }
            return answer;
        }else{//v==0, so rapidity will be 0
            for(int i=0;i<dim;i++){
                answer[i]=0;
            }
            return answer;
        }
    }
    //Inverse hyperbolic tangent
    private static double ATanH(double input)
    {
        double answer = (1+input)/(1-input);
        answer = Math.Log(answer);
        answer /=2;
        return answer;
    }
}
