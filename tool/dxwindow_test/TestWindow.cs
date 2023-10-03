using DirectN;
using libdraw;
using libdraw.Graphics;
using System.Drawing;

namespace dxwindow_test
{
    internal class TestWindow : DXWindow
    {
        private Rectangle rectText = new(40, 40, 300, 200);

        public TestWindow() 
        {
            CheckFPS = true;
            UseHardwareAcceleration = true;
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();

            BaseDpi = 96f;
        }

        protected override void OnFrame(IGraphics graphics)
        {
            var g = graphics;

            var p1 = new Point(0, 0);
            var p2 = new Point(ClientSize.Width, ClientSize.Height);

            // draw X
            g.DrawLine(p1, p2, Color.Blue, 10.0f);
            g.DrawLine(new(ClientSize.Width, 0), new(0, ClientSize.Height), Color.Red, 3.0f);


            // draw rectangle border
            g.DrawRectangle(10f, 10f, ClientSize.Width - 20, ClientSize.Height - 20, 0,
                Color.GreenYellow, null, 5f);


            // draw and fill rounded rectangle
            g.DrawRectangle(ClientSize.Width / 1.5f, ClientSize.Height / 1.5f, 300, 100,
                20f, Color.LightCyan, Color.FromArgb(180, Color.Cyan), 3f);

            // draw and fill ellipse
            g.DrawEllipse(200, 200, 300, 200, Color.FromArgb(120, Color.Magenta), Color.Purple, 5);


            // draw geometry D2D only
            if (UseHardwareAcceleration && g is D2DGraphics dg)
            {
                using var geo = dg.GetCombinedRectanglesGeometry(new RectangleF(200, 300, 300, 300),
                    new Rectangle(250, 250, 300, 100), 0, 0, D2D1_COMBINE_MODE.D2D1_COMBINE_MODE_INTERSECT);
                dg.DrawGeometry(geo, Color.Transparent, Color.Yellow, 2);


                using var geo2 = dg.GetCombinedEllipsesGeometry(new Rectangle(450, 450, 300, 100), new RectangleF(400, 400, 300, 300), D2D1_COMBINE_MODE.D2D1_COMBINE_MODE_EXCLUDE);
                dg.DrawGeometry(geo2, Color.Transparent, Color.Green, 2f);
            }


            // draw and fill rectangle
            g.DrawRectangle(rectText, 0, Color.Green, Color.FromArgb(100, Color.Yellow));


            // draw text
            var text = "Dương\r\nDiệu\r\nPháp\r\n😵🪺🐷😶‍🌫️🤯🫶🏿";
            var textSize = g.MeasureText(text, SystemFonts.DefaultFont.FontFamily.Name, 12, textDpi: DeviceDpi, isBold: true, isItalic: true);
            g.DrawText($"{text}\r\n{textSize}", SystemFonts.DefaultFont.FontFamily.Name, 12, rectText,
                Color.Red, DeviceDpi, StringAlignment.Near, isBold: true, isItalic: true);
            g.DrawRectangle(new RectangleF(rectText.Location, textSize), 0, Color.Red);


            // draw FPD info
            var engine = UseHardwareAcceleration ? "GPU" : "GDI+";
            g.DrawText($"FPS: {FPS} - {engine}", SystemFonts.DefaultFont.FontFamily.Name, 18, 0, 0, Color.Purple, DeviceDpi);
        }
    }
}
