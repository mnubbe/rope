using Cairo;
using Gtk;
using System;
using System.Collections.Generic;

public class Canvas : Window
{
    private Rope rope;
    private Universe universe;
    private Pango.Layout layout;
    private Gtk.DrawingArea drawing_area;
    private int width = 500;
    private int height = 500;
    public bool[] keyflags = new bool[255];


    public Canvas(Rope r, Universe u) : base("rope rope rope")
    {
        rope = r;
        universe = u;
        initKeyFlags();

        // Configure window.
        layout = new Pango.Layout(this.PangoContext);
        layout.Width = Pango.Units.FromPixels(width);

        // Add drawable components.
        drawing_area = new Gtk.DrawingArea();
        drawing_area.SetSizeRequest(width, height);
        Add(drawing_area);

        // Add Event handlers.
        DeleteEvent += delegate { r.Shutdown(); };
        DeleteEvent += delegate { Application.Quit(); };
        this.drawing_area.KeyPressEvent += new global::Gtk.KeyPressEventHandler (this.OnDrawingarea1KeyPressEvent);
        this.drawing_area.KeyReleaseEvent += new global::Gtk.KeyReleaseEventHandler (this.OnDrawingarea1KeyReleaseEvent);

        ShowAll();
    }


    public void Draw()
    {
        drawing_area.GdkWindow.Clear();
        using (Context context = Gdk.CairoHelper.Create(drawing_area.GdkWindow))
        {
            List<CoordinateEngine.RelativisticObject> npcs = universe.GetNPCs();
            foreach (CoordinateEngine.RelativisticObject npc in npcs) {
                DrawObject(context, npc);
            }
        }
    }


    public void DrawObject(Context context, CoordinateEngine.RelativisticObject obj)
    {
        context.Color = new Color(255, 255, 255);
        context.Rectangle(new PointD(obj.x[0], obj.x[1]), 5, 5); 
        context.Stroke();
    }
    
    #region keybinding
    //Keybinding functionality
    
    //Please don't forget calling this in a constructor:
    public void initKeyFlags(){
        for(int i=0;i<255;i++)
            keyflags[i]=false;
    }
    
    public void UpdateKeyIsDown (bool IsPressed, char key){
        if(key>=0 && key<255)//Note: sizeof(char)=2 in .NET
            keyflags[key]=IsPressed;
        Console.WriteLine(String.Format("Key {0} is now {1}",key,IsPressed));
    }
    
    protected void OnDrawingarea1KeyPressEvent (object o, Gtk.KeyPressEventArgs args)
    {
        UpdateKeyIsDown(true, args.Event.Key.ToString()[0]);
        Console.Write (args.Event.Key.ToString());
    }

    protected void OnDrawingarea1KeyReleaseEvent (object o, Gtk.KeyReleaseEventArgs args)
    {
        UpdateKeyIsDown(false, args.Event.Key.ToString()[0]);
        Console.Write (args.Event.Key.ToString());
    }
    /*
    Other parts of keybinding functionality:
        this.drawing_area.KeyPressEvent += new global::Gtk.KeyPressEventHandler (this.OnDrawingarea1KeyPressEvent);
        this.drawing_area.KeyReleaseEvent += new global::Gtk.KeyReleaseEventHandler (this.OnDrawingarea1KeyReleaseEvent);
        //Having a drawing_area to begin with
        public bool[] keyflags = new bool[255];
    */
    #endregion
}

