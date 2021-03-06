/*
 * Canvas implements the GUI.
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;

using ClassLibrary1.Collections.Generic;
using Statistics;


public class Canvas : GameWindow
{
    //Constants
    private const float bro_acceleration = 0.9f;
    private const int FPS_WINDOW = 30;
    private const int LINE_SPACING = 50;
    public const string FPS_TAG = "Render";
    private const bool SHOULD_LORENTZ_TRANSFORM_IMAGE=true;

    // Indices.
    private const int _x = 0;
    private const int _y = 1;
    private const int _z = 2;
    private Universe universe;
    private rope.camera m_camera;

    //HUD related
    private int linenumber = 0;
    private OpenTK.Graphics.TextPrinter printer;
    private Font font = new Font(FontFamily.GenericSerif, 12);
    private Stats stats;
    private const bool SHOULD_PRINT_RAPIDITY=false;

    //Reticle related

    private const bool AM_USING_RETICLE = true;
    float reticle_size = 30.0f;
    float reticle_thickness = 2.0f;
    System.Drawing.Color reticle_color = Color.White;


    /// <param name="u">A Universe to display.</param>
    public Canvas(Universe u) : base(1920, 1080, new OpenTK.Graphics.GraphicsMode(16, 16))
    {
        universe = u;
        m_camera = new rope.camera (CoordinateEngine.toVector3(universe.bro.x), new Vector3(0,0,-1), new Vector3(0,1,0));
        printer = new OpenTK.Graphics.TextPrinter();
        stats = Stats.Instance();
        stats.AddValue(FPS_TAG, -1);
    }


    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        GL.ClearColor(Color.Black);
        GL.Enable(EnableCap.DepthTest);
    }


    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);

        GL.Viewport(0, 0, Width, Height);

        double aspect_ratio = Width / (double)Height;

        OpenTK.Matrix4 perspective = OpenTK.Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, (float)aspect_ratio, 1, 64);
        GL.MatrixMode(MatrixMode.Projection);
        GL.LoadMatrix(ref perspective);
    }


    protected override void OnUpdateFrame (FrameEventArgs e)
    {
        base.OnUpdateFrame(e);

        // Handle input.

        //Movement modifier
        float modifier = 1.0f;
        if (Keyboard[Key.LShift]){//for precise positioning
            modifier /=5;
        }
        if (Keyboard[Key.LControl]){//ENGAGE WARP DRIVE
            modifier *=5;
        }

        // Camera lateral movement
        if (Keyboard[Key.W]) {
            //m_camera.Fly((float)e.Time,0,0);
            universe.bro.v = CoordinateEngine.velocitySum(universe.bro.v,
                CoordinateEngine.toDoubleArray(Vector3.Multiply(
                m_camera.lookat_vector,(float)e.Time*bro_acceleration*modifier)));
        }
        if (Keyboard[Key.A]) {
            //m_camera.Fly(0,0,(float)e.Time);
            universe.bro.v = CoordinateEngine.velocitySum(universe.bro.v,
                CoordinateEngine.toDoubleArray(Vector3.Multiply(
                m_camera.left_vector,(float)e.Time*bro_acceleration*modifier)));
        }
        if (Keyboard[Key.S]) {
            //m_camera.Fly(-(float)e.Time,0,0);
            universe.bro.v = CoordinateEngine.velocitySum(universe.bro.v,
                CoordinateEngine.toDoubleArray(Vector3.Multiply(
                m_camera.lookat_vector,(float)e.Time*(-bro_acceleration*modifier))));
        }
        if (Keyboard[Key.D]) {
            //m_camera.Fly(0,0,-(float)e.Time);
            universe.bro.v = CoordinateEngine.velocitySum(universe.bro.v,
                CoordinateEngine.toDoubleArray(Vector3.Multiply(
                m_camera.left_vector,(float)e.Time*(-bro_acceleration*modifier))));
        }
        if (Keyboard[Key.E]) {
            //m_camera.Fly(0,(float)e.Time,0);
            universe.bro.v = CoordinateEngine.velocitySum(universe.bro.v,
                CoordinateEngine.toDoubleArray(Vector3.Multiply(
                m_camera.orientation_vector,(float)e.Time*bro_acceleration*modifier)));
        }
        if (Keyboard[Key.C]) {
            //m_camera.Fly(0,-(float)e.Time,0);
            universe.bro.v = CoordinateEngine.velocitySum(universe.bro.v,
                CoordinateEngine.toDoubleArray(Vector3.Multiply(
                m_camera.orientation_vector,(float)e.Time*(-bro_acceleration*modifier))));
        }

        //Brakes, for convenience
        //Note, this is an instant stop
        if (Keyboard[Key.BackSpace]){
            for(int i=0;i<universe.bro.v.Length;i++){
                universe.bro.v[i] = 0;
            }
        }

        //Camera rotation
        //Will work for small angles, deviating at larger ones
        if (Keyboard[Key.Right]||Keyboard[Key.Keypad6]) {
            m_camera.ShiftDirection(0,-(float)e.Time*modifier,0);
        }
        if (Keyboard[Key.Left]||Keyboard[Key.Keypad4]) {
            m_camera.ShiftDirection(0,(float)e.Time*modifier,0);
        }
        if (Keyboard[Key.Down]||Keyboard[Key.Keypad2]) {//up, I wanted inverted controls for testing
            m_camera.ShiftDirection((float)e.Time*modifier,0,0);
        }
        if (Keyboard[Key.Up]||Keyboard[Key.Keypad8]) {//down, I wanted inverted controls for testing
            m_camera.ShiftDirection(-(float)e.Time*modifier,0,0);
        }
        if (Keyboard[Key.Home]||Keyboard[Key.Keypad7]) {//Roll left
            m_camera.ShiftDirection(0,0,(float)e.Time*modifier);
        }
        if (Keyboard[Key.PageUp]||Keyboard[Key.Keypad9]) {//Roll right
            m_camera.ShiftDirection(0,0,-(float)e.Time*modifier);
        }

        m_camera.NormalizeDirection();//Should be called every time direction is messed with

        lock(universe.bro){
            universe.bro.updateGamma();  //Drifting astronaut mode if used alone
            //universe.bro.v = CoordinateEngine.toDoubleArray(Vector3.Multiply(m_camera.lookat_vector,(float)universe.bro.vrms));  //Airplane with no brake mode
        }
        //Console.WriteLine("{0}, ({1},{2},{3})",universe.bro.gamma, universe.bro.v[0],universe.bro.v[1],universe.bro.v[2]);
        if (Keyboard[Key.Escape]) {
            this.Exit();
            return;
        }
    }


    protected override void OnRenderFrame (FrameEventArgs e)
    {
        Tick tick = new Tick();
        base.OnRenderFrame(e);

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        GL.MatrixMode(MatrixMode.Modelview);

        m_camera.UpdateCameraView();
        List<CoordinateEngine.RelativisticObject> ros = universe.GetNPCs();
        lock (ros) {
            foreach (CoordinateEngine.RelativisticObject ro in ros) {
                DrawRelativisticObject (ro);
            }
        }
        lock(universe.bro){
            m_camera.camera_position = CoordinateEngine.toVector3(universe.bro.x);//Loses accuracy in this...
        }
        //DrawRelativisticObject(universe.bro, false);//Don't draw bro if the camera is at bro.

        DrawHUD();

        stats.AddValue(FPS_TAG, tick.Tock());

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
        double x;
        double y;
        double z;
        if(SHOULD_LORENTZ_TRANSFORM_IMAGE){
            double[] apparent_position = CoordinateEngine.ApparentPosition(ro.x,universe.bro.x,universe.bro.v);
            x = apparent_position[_x];
            y = apparent_position[_y];
            z = apparent_position[_z];
        }else{
            x = ro.x[_x];
            y = ro.x[_y];
            z = ro.x[_z];
        }

        //double size = .01*Math.Sqrt (CoordinateEngine.RMS(universe.bro.x));
        double size = 0.042;//Diameter of the Earth if t=seconds
        size/=2;//Turns size into the side length of the cube

        GL.Begin(BeginMode.Quads);
        if (issilver)
            GL.Color3(ArbitraryRedshiftBasedColor(ro,universe.bro));
        else
            GL.Color3(Color.Blue);

        // front
        GL.Vertex3(x - size, y - size, z + size);
        GL.Vertex3(x - size, y + size, z + size);
        GL.Vertex3(x + size, y + size, z + size);
        GL.Vertex3(x + size, y - size, z + size);

        // right
        GL.Vertex3(x + size, y - size, z + size);
        GL.Vertex3(x + size, y + size, z + size);
        GL.Vertex3(x + size, y + size, z - size);
        GL.Vertex3(x + size, y - size, z - size);

        // bottom
        GL.Vertex3(x + size, y - size, z - size);
        GL.Vertex3(x + size, y - size, z + size);
        GL.Vertex3(x - size, y - size, z + size);
        GL.Vertex3(x - size, y - size, z - size);

        // left
        GL.Vertex3(x - size, y - size, z - size);
        GL.Vertex3(x - size, y - size, z + size);
        GL.Vertex3(x - size, y + size, z + size);
        GL.Vertex3(x - size, y + size, z - size);

        // top
        GL.Vertex3(x - size, y + size, z - size);
        GL.Vertex3(x - size, y + size, z + size);
        GL.Vertex3(x + size, y + size, z + size);
        GL.Vertex3(x + size, y + size, z - size);

        // back
        GL.Vertex3(x + size, y + size, z - size);
        GL.Vertex3(x + size, y - size, z - size);
        GL.Vertex3(x - size, y - size, z - size);
        GL.Vertex3(x - size, y + size, z - size);

        GL.End();
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


    private void DrawHUD()
    {
        // Switch to orthographic.
        GL.Disable(EnableCap.DepthTest);
        GL.MatrixMode(MatrixMode.Projection);
        GL.PushMatrix();
        GL.LoadIdentity();
        GL.Ortho(0, Width, Height, 0, -5, 1);
        GL.MatrixMode(MatrixMode.Modelview);
        GL.LoadIdentity();

        string gfps_str = String.Format("graphics: {0} FPS",
            ((int)stats.GetFPS(FPS_TAG)));
        string pfps_str = String.Format("physics: {0} FPS",
            ((int)stats.GetFPS("physics")));

        //Note about printer: mono seems to have a memory leak from this unless it is manually .Dispose() 'd of
        //Or only a finite number of instances are made (1 from constructor seems fine)
        linenumber=0;
        HUDprintLine(gfps_str);
        HUDprintLine(pfps_str);
        HUDprintLine(String.Format("Gamma: {0}",universe.bro.gamma));
        HUDprintLine(String.Format("Gamma*Vrms: {0}",universe.bro.gamma*universe.bro.vrms));
        HUDprintLine(String.Format("Vrms: {0}",universe.bro.vrms));
        HUDprintLine(String.Format("Your watch: {0:F3}",universe.bro.t_object));
        HUDprintLine(String.Format("Wall clock: {0:F3}",universe.universe_time));
        HUDprintLine(String.Format("Position: \n{0:F4}\n{1:F4}\n{2:F4}",universe.bro.x[0],universe.bro.x[1],universe.bro.x[2]));
        linenumber+=3;

        if(SHOULD_PRINT_RAPIDITY){
            double[] rapidity = CoordinateEngine.Rapidity(universe.bro.v);
            //Print next to the position statement
            RectangleF m_rect = new RectangleF(50+100,50+25*(linenumber-1),500,50);
            printer.Print(String.Format("Rapidity: \n{0:F4}\n{1:F4}\n{2:F4}",rapidity[0],rapidity[1],rapidity[2]),
                          font,Color.White,m_rect);
        }


        if(AM_USING_RETICLE){
            HUDdrawReticle();
        }
        
        // Switch back.
        GL.Enable(EnableCap.DepthTest);
        GL.MatrixMode(MatrixMode.Projection);
        GL.PopMatrix();
        GL.MatrixMode(MatrixMode.Modelview);
    }

    private void HUDprintLine(string text)
    {
        {
            RectangleF m_rect = new RectangleF(50,50+25*linenumber,500,50);
            printer.Print(text,font,Color.White,m_rect);
        }
        linenumber++;
    }
    private void HUDdrawReticle()
    {
        GL.Color3(reticle_color);
        GL.Begin(BeginMode.Quads);

        //Go upper left, ur, lr,ll

        //Horizontal "line"
        GL.Vertex3((Width-reticle_size)/2,(Height-reticle_thickness)/2,0);
        GL.Vertex3((Width+reticle_size)/2,(Height-reticle_thickness)/2,0);
        GL.Vertex3((Width+reticle_size)/2,(Height+reticle_thickness)/2,0);
        GL.Vertex3((Width-reticle_size)/2,(Height+reticle_thickness)/2,0);

        //Vertical "line"
        GL.Vertex3((Width-reticle_thickness)/2,(Height-reticle_size)/2,0);
        GL.Vertex3((Width+reticle_thickness)/2,(Height-reticle_size)/2,0);
        GL.Vertex3((Width+reticle_thickness)/2,(Height+reticle_size)/2,0);
        GL.Vertex3((Width-reticle_thickness)/2,(Height+reticle_size)/2,0);

        GL.End();
    }
}

