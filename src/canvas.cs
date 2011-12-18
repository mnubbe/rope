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
    //Constants
    private const float bro_acceleration = 0.9f;

    // Indices.
    private const int _x = 0;
    private const int _y = 1;
    private const int _z = 2;

    private Universe universe;
    private rope.camera m_camera;


    /// <param name="u">A Universe to display.</param>
    public Canvas(Universe u) : base(1920, 1080, new Graphics::GraphicsMode(16, 16))
    {
        universe = u;
        m_camera = new rope.camera (CoordinateEngine.toVector3(universe.bro.x), new Vector3(0,0,-1), new Vector3(0,1,0));
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

        // Camera lateral movement
        if (Keyboard[OpenTK.Input.Key.W]) {
            //m_camera.Fly((float)e.Time,0,0);
            universe.bro.v = CoordinateEngine.velocitySum(universe.bro.v,
                CoordinateEngine.toDoubleArray(Vector3.Multiply(m_camera.lookat_vector,(float)e.Time*bro_acceleration)));
        }
        if (Keyboard[OpenTK.Input.Key.A]) {
            //m_camera.Fly(0,0,(float)e.Time);
            universe.bro.v = CoordinateEngine.velocitySum(universe.bro.v,
                CoordinateEngine.toDoubleArray(Vector3.Multiply(m_camera.left_vector,(float)e.Time*bro_acceleration)));
        }
        if (Keyboard[OpenTK.Input.Key.S]) {
            //m_camera.Fly(-(float)e.Time,0,0);
            universe.bro.v = CoordinateEngine.velocitySum(universe.bro.v,
                CoordinateEngine.toDoubleArray(Vector3.Multiply(m_camera.lookat_vector,(float)e.Time*(-bro_acceleration))));
        }
        if (Keyboard[OpenTK.Input.Key.D]) {
            //m_camera.Fly(0,0,-(float)e.Time);
            universe.bro.v = CoordinateEngine.velocitySum(universe.bro.v,
                CoordinateEngine.toDoubleArray(Vector3.Multiply(m_camera.left_vector,(float)e.Time*(-bro_acceleration))));
        }
        if (Keyboard[OpenTK.Input.Key.E]) {
            //m_camera.Fly(0,(float)e.Time,0);
            universe.bro.v = CoordinateEngine.velocitySum(universe.bro.v,
                CoordinateEngine.toDoubleArray(Vector3.Multiply(m_camera.orientation_vector,(float)e.Time*bro_acceleration)));
        }
        if (Keyboard[OpenTK.Input.Key.C]) {
            //m_camera.Fly(0,-(float)e.Time,0);
            universe.bro.v = CoordinateEngine.velocitySum(universe.bro.v,
                CoordinateEngine.toDoubleArray(Vector3.Multiply(m_camera.orientation_vector,(float)e.Time*(-bro_acceleration))));
        }


        //Camera rotation
        //Will work for small angles, deviating at larger ones
        if (Keyboard[OpenTK.Input.Key.Right]||Keyboard[OpenTK.Input.Key.Keypad6]) {
            m_camera.ShiftDirection(0,-(float)e.Time,0);
        }
        if (Keyboard[OpenTK.Input.Key.Left]||Keyboard[OpenTK.Input.Key.Keypad4]) {
            m_camera.ShiftDirection(0,(float)e.Time,0);
        }
        if (Keyboard[OpenTK.Input.Key.Down]||Keyboard[OpenTK.Input.Key.Keypad2]) {//up, I wanted inverted controls for testing
            m_camera.ShiftDirection((float)e.Time,0,0);
        }
        if (Keyboard[OpenTK.Input.Key.Up]||Keyboard[OpenTK.Input.Key.Keypad8]) {//down, I wanted inverted controls for testing
            m_camera.ShiftDirection(-(float)e.Time,0,0);
        }
        if (Keyboard[OpenTK.Input.Key.Home]||Keyboard[OpenTK.Input.Key.Keypad7]) {//Roll left
            m_camera.ShiftDirection(0,0,(float)e.Time);
        }
        if (Keyboard[OpenTK.Input.Key.PageUp]||Keyboard[OpenTK.Input.Key.Keypad9]) {//Roll right
            m_camera.ShiftDirection(0,0,-(float)e.Time);
        }

        m_camera.NormalizeDirection();//Should be called every time direction is messed with

        lock(universe.bro){
            universe.bro.updateGamma();  //Drifting astronaut mode
            //universe.bro.v = CoordinateEngine.toDoubleArray(Vector3.Multiply(m_camera.lookat_vector,(float)universe.bro.vrms));  //Airplane with no brake mode
        }
        //Console.WriteLine("{0}, ({1},{2},{3})",universe.bro.gamma, universe.bro.v[0],universe.bro.v[1],universe.bro.v[2]);
        if (Keyboard[OpenTK.Input.Key.Escape]) {
            this.Exit();
            return;
        }
    }


    protected override void OnRenderFrame (FrameEventArgs e)
    {
        base.OnRenderFrame(e);

        OpenGL::GL.Clear(OpenGL::ClearBufferMask.ColorBufferBit | OpenGL::ClearBufferMask.DepthBufferBit);

        OpenGL::GL.MatrixMode(OpenGL::MatrixMode.Modelview);

        m_camera.UpdateCameraView();
        List<CoordinateEngine.RelativisticObject> ros = universe.GetNPCs ();
        lock (ros) {
            foreach (CoordinateEngine.RelativisticObject ro in ros) {
                DrawRelativisticObject (ro);
            }
        }
        lock(universe.bro){
            m_camera.camera_position = CoordinateEngine.toVector3(universe.bro.x);//Loses accuracy in this...
        }
        //DrawRelativisticObject(universe.bro, false);//Don't draw bro if the camera is at bro.

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

        //double size = .01*Math.Sqrt (CoordinateEngine.RMS(universe.bro.x));
        double size = 0.01;

        OpenGL::GL.Begin(OpenGL::BeginMode.Quads);
        if (issilver)
            OpenGL::GL.Color3(ArbitraryRedshiftBasedColor(ro,universe.bro));
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

    private System.Drawing.Color ArbitraryRedshiftBasedColor(CoordinateEngine.RelativisticObject dude, CoordinateEngine.RelativisticObject reference)
    {
        //Get z from the two objects
        //for information about z, see http://en.wikipedia.org/wiki/Redshift
        //z==0 means no shift
        //1+z = wavelength(observed)/wavelength(emitted)
        //so multiply the object's wavelength by (1+z)
        //the shift from pure green to pure red or pure blue to pure green is about z=0.17, so this would not be the most dynamic scale for large z
        double z = CoordinateEngine.calculateRedshiftZ(dude,universe.bro);

        //"Accurate" values
        const double upperlimit = 0.17;
        const double lowerlimit = -0.146;

        //Way to "soften" the quick and hard transition between colors.  The 1/exponent roughly multiplies the visible spectrum window...
        z = -1.0+Math.Pow(1.0+z,1.0/1.0);

        if(z>upperlimit){//receding fast
            return System.Drawing.Color.FromArgb(127,5,10);
        }else if(z<lowerlimit){//approaching fast
            return System.Drawing.Color.FromArgb(127,0,255);
        }else if(z>0.0){
            return System.Drawing.Color.FromArgb((int)(z/upperlimit*255),(int)(255*(1-z/upperlimit)),0);
        }else{//z<0.0
            return System.Drawing.Color.FromArgb(0,(int)(255*(1-z/lowerlimit)),(int)(z/lowerlimit*255));
        }
    }
}

