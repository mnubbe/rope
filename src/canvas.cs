/*
 * Canvas implements the GUI.
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

using OpenTK;
using Graphics = OpenTK.Graphics;
using OpenGL = OpenTK.Graphics.OpenGL;


public class Canvas : GameWindow
{
    // Indices.
    private const int _x = 0;
    private const int _y = 1;
    private const int _z = 2;

    private Universe universe;
    private float rotation_speed = 90.0f;
    private float angle = 0.0f;
    private Vector3 camera_position = new Vector3 (0,0,5);
    private Vector3 lookat_vector =  new Vector3 (0,0,-1);
    private Vector3 orientation_vector = new Vector3 (0,1,0);
    private Vector3 left_vector = new Vector3 (1,0,0);//Points left by orientation cross lookat
    private float movement_speed = 0.1f;


    /// <param name="u">A Universe to display.</param>
    public Canvas(Universe u) : base(1920, 1080, new Graphics::GraphicsMode(16, 16))
    {
        universe = u;
    }


    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        OpenGL::GL.ClearColor(Color.Black);
        OpenGL::GL.Enable(OpenGL::EnableCap.DepthTest);
    }


    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);

        OpenGL::GL.Viewport(0, 0, Width, Height);

        double aspect_ratio = Width / (double)Height;

        OpenTK.Matrix4 perspective = OpenTK.Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, (float)aspect_ratio, 1, 64);
        OpenGL::GL.MatrixMode(OpenGL::MatrixMode.Projection);
        OpenGL::GL.LoadMatrix(ref perspective);
    }


    protected override void OnUpdateFrame (FrameEventArgs e)
    {
        base.OnUpdateFrame(e);

        // Handle input.
        /*// Bro movement.
        if (Keyboard[OpenTK.Input.Key.W]) {
            universe.bro.v = CoordinateEngine.velocitySum(universe.bro.v,new double[3]{0,.1,0});
        }
        if (Keyboard[OpenTK.Input.Key.A]) {
            universe.bro.v = CoordinateEngine.velocitySum(universe.bro.v,new double[3]{-.1,0,0});
        }
        if (Keyboard[OpenTK.Input.Key.S]) {
            universe.bro.v = CoordinateEngine.velocitySum(universe.bro.v,new double[3]{0,-.1,0});
        }
        if (Keyboard[OpenTK.Input.Key.D]) {
            universe.bro.v = CoordinateEngine.velocitySum(universe.bro.v,new double[3]{.1,0,0});
        }*/

        // Camera movement.
        if (Keyboard[OpenTK.Input.Key.W]) {
            camera_position = Vector3.Add(camera_position,Vector3.Multiply(lookat_vector,movement_speed));
        }
        if (Keyboard[OpenTK.Input.Key.A]) {
            camera_position = Vector3.Add(camera_position,Vector3.Multiply(left_vector,movement_speed));
        }
        if (Keyboard[OpenTK.Input.Key.S]) {
            camera_position = Vector3.Add(camera_position,Vector3.Multiply(lookat_vector,-movement_speed));
        }
        if (Keyboard[OpenTK.Input.Key.D]) {
            camera_position = Vector3.Add(camera_position,Vector3.Multiply(left_vector,-movement_speed));
        }
        if (Keyboard[OpenTK.Input.Key.E]) {
            camera_position = Vector3.Add(camera_position,Vector3.Multiply(orientation_vector,movement_speed));
        }
        if (Keyboard[OpenTK.Input.Key.C]) {
            camera_position = Vector3.Add(camera_position,Vector3.Multiply(orientation_vector,-movement_speed));
        }



        angle = rotation_speed * (float)e.Time*(float)Math.PI/180.0f;
        //Will work for small angles, deviating at larger ones
        if (Keyboard[OpenTK.Input.Key.Right]) {
            lookat_vector = Vector3.Add(lookat_vector,Vector3.Multiply(left_vector,-angle));
            //orientation_vector = Vector3.sum(orientation_vector,Vector3.Multiply(left_vector,-angle));
        }
        if (Keyboard[OpenTK.Input.Key.Left]) {
            lookat_vector = Vector3.Add(lookat_vector,Vector3.Multiply(left_vector,angle));
            //orientation_vector = Vector3.sum(orientation_vector,Vector3.Multiply(left_vector,-angle));
        }
        if (Keyboard[OpenTK.Input.Key.Down]) {//up, I wanted inverted controls for testing
            lookat_vector = Vector3.Add(lookat_vector,Vector3.Multiply(orientation_vector,angle));
            orientation_vector = Vector3.Add(orientation_vector,Vector3.Multiply(lookat_vector,-angle));
        }
        if (Keyboard[OpenTK.Input.Key.Up]) {//down, I wanted inverted controls for testing
            lookat_vector = Vector3.Add(lookat_vector,Vector3.Multiply(orientation_vector,-angle));
            orientation_vector = Vector3.Add(orientation_vector,Vector3.Multiply(lookat_vector,angle));
        }
        //Could also include a roll which only changes orientation vector by left_vector
//        if(rotating){
//            angle = rotation_speed * (float)e.Time*Math.PI/180.0f;
//        }else{
//            angle = 0.0f;
//        }
        lookat_vector.Normalize();
        orientation_vector.Normalize();
        left_vector = Vector3.Cross(orientation_vector,lookat_vector);
        orientation_vector = Vector3.Cross(lookat_vector,left_vector);

        universe.bro.updateGamma();
        if (Keyboard[OpenTK.Input.Key.Escape]) {
            this.Exit();
            return;
        }
    }


    protected override void OnRenderFrame (FrameEventArgs e)
    {
        Matrix4 local_look_at = Matrix4.LookAt(
            camera_position,
            Vector3.Add(camera_position,lookat_vector),
            //Vector3.Add(camera_position,orientation_vector)
            orientation_vector
            );
        base.OnRenderFrame(e);

        OpenGL::GL.Clear(OpenGL::ClearBufferMask.ColorBufferBit | OpenGL::ClearBufferMask.DepthBufferBit);

        //Matrix4 lookat = Matrix4.LookAt (0, 5, 5, 0, 0, 0, 0, 1, 0);
        OpenGL::GL.MatrixMode(OpenGL::MatrixMode.Modelview);

        //lookat.Rotate(Quaternion.FromAxisAngle(new Vector3(rotate[_x], rotate[_y], rotate[_z]),angle));

        OpenGL::GL.LoadMatrix(ref local_look_at);
        //OpenGL::GL.Rotate(angle, rotate[_x], rotate[_y], rotate[_z]);
        //Console.WriteLine(String.Format("{0} @ {1}, {2}, {3}", angle, rotate[_x], rotate[_y], rotate[_z]));
        List<CoordinateEngine.RelativisticObject> ros = universe.GetNPCs ();
        lock (ros) {
            foreach (CoordinateEngine.RelativisticObject ro in ros) {
                DrawRelativisticObject (ro);
            }
        }
        DrawRelativisticObject(universe.bro, false);

        this.SwapBuffers();
        Thread.Sleep(1);
    }


    /// <summary>
    /// Draw a single RelativisticObject to the screen.
    /// </summary>
    /// <param name="ro">The RelativisticObject to draw.</param>
    /// <param name="issilver">Whether that object should be silver I guess</param>
    /// <remarks>
    /// What? You want to choose a color? Get off my lawn!
    /// </remarks>
    private void DrawRelativisticObject(CoordinateEngine.RelativisticObject ro, bool issilver = true)
    {
        double x = ro.x[_x];
        double y = ro.x[_y];
        double z = ro.x[_z];

        double size = .1;

        OpenGL::GL.Begin(OpenGL::BeginMode.Quads);
        if (issilver)
            OpenGL::GL.Color3(Color.Silver);
        else
            OpenGL::GL.Color3(Color.Blue);

        // front
        OpenGL::GL.Vertex3(x - size, y - size, z + size);
        OpenGL::GL.Vertex3(x - size, y + size, z + size);
        OpenGL::GL.Vertex3(x + size, y + size, z + size);
        OpenGL::GL.Vertex3(x + size, y - size, z + size);

        // right
        OpenGL::GL.Vertex3(x + size, y - size, z + size);
        OpenGL::GL.Vertex3(x + size, y + size, z + size);
        OpenGL::GL.Vertex3(x + size, y + size, z - size);
        OpenGL::GL.Vertex3(x + size, y - size, z - size);

        // bottom
        OpenGL::GL.Vertex3(x + size, y - size, z - size);
        OpenGL::GL.Vertex3(x + size, y - size, z + size);
        OpenGL::GL.Vertex3(x - size, y - size, z + size);
        OpenGL::GL.Vertex3(x - size, y - size, z - size);

        // left
        OpenGL::GL.Vertex3(x - size, y - size, z - size);
        OpenGL::GL.Vertex3(x - size, y - size, z + size);
        OpenGL::GL.Vertex3(x - size, y + size, z + size);
        OpenGL::GL.Vertex3(x - size, y + size, z - size);

        // top
        OpenGL::GL.Vertex3(x - size, y + size, z - size);
        OpenGL::GL.Vertex3(x - size, y + size, z + size);
        OpenGL::GL.Vertex3(x + size, y + size, z + size);
        OpenGL::GL.Vertex3(x + size, y + size, z - size);

        // back
        OpenGL::GL.Vertex3(x + size, y + size, z - size);
        OpenGL::GL.Vertex3(x + size, y - size, z - size);
        OpenGL::GL.Vertex3(x - size, y - size, z - size);
        OpenGL::GL.Vertex3(x - size, y + size, z - size);

        OpenGL::GL.End();
    }
}

