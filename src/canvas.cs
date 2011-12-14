using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

using OpenTK;
using Graphics = OpenTK.Graphics;
using OpenGL = OpenTK.Graphics.OpenGL;

public class Canvas : GameWindow
{
    private Rope rope;
    private Universe universe;
    private float rotation_speed = 180.0f;
    private float angle;


    public Canvas(Rope r, Universe u) : base(500, 500, new Graphics::GraphicsMode(16, 16))
    {
        rope = r;
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


    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        base.OnUpdateFrame(e);

        if (Keyboard[OpenTK.Input.Key.Escape]) {
            this.Exit();
            return;
        }
    }


    protected override void OnRenderFrame(FrameEventArgs e)
    {
        base.OnRenderFrame(e);

        OpenGL::GL.Clear(OpenGL::ClearBufferMask.ColorBufferBit | OpenGL::ClearBufferMask.DepthBufferBit);

        Matrix4 lookat = Matrix4.LookAt(0, 5, 5, 0, 0, 0, 0, 1, 0);
        OpenGL::GL.MatrixMode(OpenGL::MatrixMode.Modelview);
        OpenGL::GL.LoadMatrix(ref lookat);

        angle += rotation_speed * (float)e.Time;
        //OpenGL::GL.Rotate(angle, 0.0f, 1.0f, 0.0f);

        List<CoordinateEngine.RelativisticObject> ros = universe.GetNPCs();
        lock (ros) {
            foreach (CoordinateEngine.RelativisticObject ro in ros) {
                DrawRelativisticObject(ro);
            }
        }

        this.SwapBuffers();
        Thread.Sleep(1);
    }


    private void DrawRelativisticObject(CoordinateEngine.RelativisticObject ro)
    {
        double x = ro.x[0];
        double y = ro.x[1];
        double z = ro.x[2];

        double size = .2;

        OpenGL::GL.Begin(OpenGL::BeginMode.Quads);
        OpenGL::GL.Color3(Color.Silver);
        OpenGL::GL.Vertex3(x - size, y - size, 0);
        OpenGL::GL.Vertex3(x - size, y + size, 0);
        OpenGL::GL.Vertex3(x + size, y + size, 0);
        OpenGL::GL.Vertex3(x + size, y - size, 0);
        OpenGL::GL.End();
    }


    private void DrawCube()
    {
            OpenGL::GL.Begin(OpenGL::BeginMode.Quads);

            OpenGL::GL.Color3(Color.Silver);
            OpenGL::GL.Vertex3(-1.0f, -1.0f, -1.0f);
            OpenGL::GL.Vertex3(-1.0f, 1.0f, -1.0f);
            OpenGL::GL.Vertex3(1.0f, 1.0f, -1.0f);
            OpenGL::GL.Vertex3(1.0f, -1.0f, -1.0f);

            OpenGL::GL.Color3(Color.Honeydew);
            OpenGL::GL.Vertex3(-1.0f, -1.0f, -1.0f);
            OpenGL::GL.Vertex3(1.0f, -1.0f, -1.0f);
            OpenGL::GL.Vertex3(1.0f, -1.0f, 1.0f);
            OpenGL::GL.Vertex3(-1.0f, -1.0f, 1.0f);

            OpenGL::GL.Color3(Color.Moccasin);
            OpenGL::GL.Vertex3(-1.0f, -1.0f, -1.0f);
            OpenGL::GL.Vertex3(-1.0f, -1.0f, 1.0f);
            OpenGL::GL.Vertex3(-1.0f, 1.0f, 1.0f);
            OpenGL::GL.Vertex3(-1.0f, 1.0f, -1.0f);

            OpenGL::GL.Color3(Color.IndianRed);
            OpenGL::GL.Vertex3(-1.0f, -1.0f, 1.0f);
            OpenGL::GL.Vertex3(1.0f, -1.0f, 1.0f);
            OpenGL::GL.Vertex3(1.0f, 1.0f, 1.0f);
            OpenGL::GL.Vertex3(-1.0f, 1.0f, 1.0f);

            OpenGL::GL.Color3(Color.PaleVioletRed);
            OpenGL::GL.Vertex3(-1.0f, 1.0f, -1.0f);
            OpenGL::GL.Vertex3(-1.0f, 1.0f, 1.0f);
            OpenGL::GL.Vertex3(1.0f, 1.0f, 1.0f);
            OpenGL::GL.Vertex3(1.0f, 1.0f, -1.0f);

            OpenGL::GL.Color3(Color.ForestGreen);
            OpenGL::GL.Vertex3(1.0f, -1.0f, -1.0f);
            OpenGL::GL.Vertex3(1.0f, 1.0f, -1.0f);
            OpenGL::GL.Vertex3(1.0f, 1.0f, 1.0f);
            OpenGL::GL.Vertex3(1.0f, -1.0f, 1.0f);

            OpenGL::GL.End();
    }
}

