using Cairo;
using Gtk;
using System;

public class Canvas : Window
{
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


    public Canvas() : base("Center")
    {
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
}

