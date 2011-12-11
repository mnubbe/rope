using Cairo;
using Gtk;
using System;
using System.Collections.Generic;

public class Canvas : Window
{
    private Universe universe;


    private class Whiteboard : DrawingArea
    {
        protected override bool OnExposeEvent(Gdk.EventExpose args)
        {
            using (Context c = Gdk.CairoHelper.Create(args.Window))
            {
                c.MoveTo(0, 0);
                c.Arc(100, 100, 50, Math.PI, -Math.PI);
                c.Color = new Color(0, 0, 0);
                c.LineWidth = 5;
                c.Stroke();
            }
            return true;
        }
    }


    public Canvas(Universe u) : base("Center")
    {
        universe = u;

        // Configure window.
        SetDefaultSize(400, 400);
        SetPosition(WindowPosition.Center);

        // Add drawable components.
        Box b = new HBox(true, 0);
        b.Add(new Whiteboard());
        Add(b);

        // Add Event handlers.
        DeleteEvent += delegate { Application.Quit(); };

        ShowAll();
    }


    public void Draw()
    {
        List<CoordinateEngine.RelativisticObject> npcs = universe.getNPCs();
        foreach (CoordinateEngine.RelativisticObject npc in npcs) {
            DrawObject(npc);
        }
    }


    public void DrawObject(CoordinateEngine.RelativisticObject obj)
    {

    }
}

