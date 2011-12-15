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
    private Universe universe;
    private float rotation_speed = 180.0f;
    private float angle;


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

        if(Keyboard[OpenTK.Input.Key.W])
//            universe.bro.x[1]+=0.1;
            universe.bro.v = CoordinateEngine.velocitySum(universe.bro.v,new double[3] {0,.1,0});
        if(Keyboard[OpenTK.Input.Key.A])
//            universe.bro.x[0]-=0.1;
            universe.bro.v = CoordinateEngine.velocitySum(universe.bro.v,new double[3] {-.1,0,0});
        if(Keyboard[OpenTK.Input.Key.S])
//            universe.bro.x[1]-=0.1;
            universe.bro.v = CoordinateEngine.velocitySum(universe.bro.v,new double[3] {0,-.1,0});
        if(Keyboard[OpenTK.Input.Key.D])
//            universe.bro.x[0]+=0.1;
            universe.bro.v = CoordinateEngine.velocitySum(universe.bro.v,new double[3] {+.1,0,0});
        universe.bro.updateGamma();
        if (Keyboard[OpenTK.Input.Key.Escape]) {
            this.Exit();
            return;
        }


    }


    protected override void OnRenderFrame (FrameEventArgs e)
    {
        base.OnRenderFrame (e);

        OpenGL::GL.Clear (OpenGL::ClearBufferMask.ColorBufferBit | OpenGL::ClearBufferMask.DepthBufferBit);

        Matrix4 lookat = Matrix4.LookAt (0, 5, 5, 0, 0, 0, 0, 1, 0);
        OpenGL::GL.MatrixMode (OpenGL::MatrixMode.Modelview);
        OpenGL::GL.LoadMatrix (ref lookat);

        angle += rotation_speed * (float)e.Time;
        //OpenGL::GL.Rotate(angle, 0.0f, 1.0f, 0.0f);

        List<CoordinateEngine.RelativisticObject> ros = universe.GetNPCs ();
        lock (ros) {
            foreach (CoordinateEngine.RelativisticObject ro in ros) {
                DrawRelativisticObject (ro);
            }
        }
        DrawRelativisticObject (universe.bro, false);

        this.SwapBuffers ();
        Thread.Sleep (1);
    }


    /// <summary>
    /// Draw a single RelativisticObject to the screen.
    /// </summary>
    /// <param name="ro">The RelativisticObject to draw.</param>
    /// <param name="issilver">Whether that object should be silver I guess</param>
    /// <remarks>
    /// What? You want to choose a color? Get off my lawn!
    /// </remarks>
    private void DrawRelativisticObject (CoordinateEngine.RelativisticObject ro, bool issilver = true)
    {
        double x = ro.x [0];
        double y = ro.x [1];
        double z = ro.x [2];

        double size = .01;

        OpenGL::GL.Begin (OpenGL::BeginMode.Quads);
        if (issilver)
            OpenGL::GL.Color3 (Color.Silver);
        else
            OpenGL::GL.Color3 (Color.Blue);
        OpenGL::GL.Vertex3 (x - size, y - size, 0);
        OpenGL::GL.Vertex3 (x - size, y + size, 0);
        OpenGL::GL.Vertex3 (x + size, y + size, 0);
        OpenGL::GL.Vertex3 (x + size, y - size, 0);
        OpenGL::GL.End ();
    }
}

