using System;

using OpenTK;
//using OpenTK.Input;
//using Graphics = OpenTK.Graphics;
using OpenGL = OpenTK.Graphics.OpenGL;

namespace rope
{
    public class camera
        //SUGGESTION: USE QUARTERNIONS
    {
        //Constants
        const float default_rotation_speed = 1.0f;
        const float default_movement_speed = 1.0f;

        //Variables
        //Position of the camera in space
        public Vector3 camera_position = new Vector3 (0,0,5);//We can set this if we want
        //not 100% sure on this variable, but it is fed into the OpenGL call for the camera
        Matrix4 local_look_at;

        //Camera vectors
        //Properties: Should be unit vectors and all orthoganol
        //lookat_vector is the direction of the camera view
        private Vector3 lookat_vector =  new Vector3 (0,0,-1);
        //orientation_vector is the direction up for the camera
        private Vector3 orientation_vector = new Vector3 (0,1,0);
        //left_vector is the direction directly to the left
        private Vector3 left_vector = new Vector3 (1,0,0);//Points left by orientation cross lookat

        public camera ()
        {
            UpdateCameraView();
        }

        public void UpdateCameraView()
        {
            local_look_at = Matrix4.LookAt(
                camera_position,
                Vector3.Add(camera_position,lookat_vector),
                //Vector3.Add(camera_position,orientation_vector)
                orientation_vector
                );
            OpenGL::GL.LoadMatrix(ref local_look_at);
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
                orientation_vector = Vector3.Add(orientation_vector,Vector3.Multiply(left_vector,rotation_speed*roll_left));
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

