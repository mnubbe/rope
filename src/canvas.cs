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


    public Canvas(Rope r, Universe u) : base("rope rope rope")
    {
        rope = r;
        universe = u;

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

        ShowAll();
    }


    public void Draw()
    {
        using (Context context = Gdk.CairoHelper.Create(drawing_area.GdkWindow))
        {
            DrawObject(context, new CoordinateEngine.RelativisticObject(150,
                        150, 150));
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
}

