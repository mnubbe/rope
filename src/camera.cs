using System;

using OpenTK;
//using OpenTK.Input;
//using Graphics = OpenTK.Graphics;
using OpenGL = OpenTK.Graphics.OpenGL;
using OpenTK.Graphics.OpenGL;

namespace rope
{
    public class camera
        //SUGGESTION: USE QUARTERNIONS
    {
        //Constants
        const float default_rotation_speed = 1.0f;
        const float roll_speedup_factor    = 2.0f; //roll will happen this factor faster for palatability
        const float default_movement_speed = 1.0f;

        //Field of view parameters
        float FOV = 90.0f/180.0f*(float)Math.PI;//First part in degrees, width
        float aspect_ratio = 9.0f/16.0f;
        float znear = 0.01f;//If used like this, it is the minimum displayable distance.
        //...And the switch for using the experimental camera that takes these into account
        bool I_should_use_the_experimental_camera = true;

        //Variables
        //Position of the camera in space
        public Vector3 camera_position = new Vector3 (0,0,5);//We can set this if we want
        //not 100% sure on this variable, but it is fed into the OpenGL call for the camera
        Matrix4 local_look_at;

        //Camera vectors
        //Properties: Should be unit vectors and all orthoganol
        //TODO: these should be private set, public get
        //lookat_vector is the direction of the camera view
        public Vector3 lookat_vector =  new Vector3 (0,0,-1);
        //orientation_vector is the direction up for the camera
        public Vector3 orientation_vector = new Vector3 (0,1,0);
        //left_vector is the direction directly to the left
        public Vector3 left_vector = new Vector3 (1,0,0);//Points left by orientation cross lookat

        public camera ()
        {
            UpdateCameraView();
        }
        public camera (Vector3 initial_position, Vector3 initial_view_vector, Vector3 initial_orientation_vector)
        {
            //See above for what these are
            camera_position    = new Vector3(initial_position);
            lookat_vector      = new Vector3(initial_view_vector);
            orientation_vector = new Vector3(initial_orientation_vector);
            NormalizeDirection();
            UpdateCameraView();
        }

        public void UpdateCameraView()
        {
            local_look_at = Matrix4.LookAt(
                camera_position,
                Vector3.Add(camera_position,lookat_vector),
                orientation_vector
                );


            if(I_should_use_the_experimental_camera){
                //See http://www.opentk.com/node/2083,
                //http://www.opengl.org/resources/faq/technical/viewing.htm at 8.040
                //For how this was all figured out
                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadIdentity();
                OpenTK.Matrix4 perspective = Matrix4.CreatePerspectiveOffCenter(-FOV*znear/2,FOV*znear/2,
                                                                                -FOV*aspect_ratio*znear/2,FOV*aspect_ratio*znear/2,
                                                                                znear,1000);
                GL.MultMatrix(ref perspective);
                GL.MatrixMode(MatrixMode.Modelview);
                GL.LoadIdentity();
            }
            
            GL.MultMatrix(ref local_look_at);
        }

        //See http://en.wikipedia.org/wiki/File:Rollpitchyawplain.png for direction definitions
        //Not actually implementing angular methods, but for small angles the units are approximately radians
//        public void Rotate(float pitch, float yaw, float roll){
//            throw new NotImplementedException();
//        }

        //Units approximately in radians for small angles
        public void ShiftDirection(float up, float left, float roll_left, float rotation_speed = default_rotation_speed)
        {
            if (up!=0.0f) {
                lookat_vector = Vector3.Add(lookat_vector,Vector3.Multiply(orientation_vector,up*rotation_speed));
                orientation_vector = Vector3.Add(orientation_vector,Vector3.Multiply(lookat_vector,-up*rotation_speed));
            }
            if (left!=0.0f) {
                lookat_vector = Vector3.Add(lookat_vector,Vector3.Multiply(left_vector,rotation_speed*left));
                //left_vector = Vector3.sum(left_vector,Vector3.Multiply(orientation_vector,-angle));
            }
            if (roll_left!=0.0f) {
                //lookat_vector does not change
                orientation_vector = Vector3.Add(orientation_vector,Vector3.Multiply(left_vector,rotation_speed*roll_left*roll_speedup_factor));
                //left_vector goes downward
            }
        }

        public void Fly(float forward, float strafeup, float strafeleft, float speed = default_movement_speed)
        {
            //Forward
            if(forward!=0.0f)
                camera_position = Vector3.Add(camera_position,Vector3.Multiply(lookat_vector,forward*default_movement_speed));
            //Up
            if(strafeup!=0.0f)
                camera_position = Vector3.Add(camera_position,Vector3.Multiply(orientation_vector,strafeup*default_movement_speed));
            //Left
            if(strafeleft!=0.0f)
                camera_position = Vector3.Add(camera_position,Vector3.Multiply(left_vector,strafeleft*default_movement_speed));
        }

        public void Drift(Vector3 difference)
        {
            camera_position = Vector3.Add(camera_position,difference);
        }

        //Should be called every time direction is messed with
        public void NormalizeDirection(){
            lookat_vector.Normalize();
            orientation_vector.Normalize();
            left_vector = Vector3.Cross(orientation_vector,lookat_vector);
            orientation_vector = Vector3.Cross(lookat_vector,left_vector);
        }


    }
}

