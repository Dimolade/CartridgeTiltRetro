using Eto.Drawing;
using Eto.Forms;
using System;
using System.Collections.Generic;

public class DisplayPreview : Form
{
    private Color backgroundColor;
    private List<(Image img, int x, int y, CTR.Color tint, CTR.Vector3 s, CTR.Vector3 r)> images = new List<(Image, int, int, CTR.Color,CTR.Vector3,CTR.Vector3)>();
    private Drawable canvas;

    public DisplayPreview(int xSize, int ySize, Color backgroundColor, CTR.Platform p)
    {
        Title = "Scene Preview for " + p.id;
        ClientSize = new Size(xSize, ySize);
        
        this.backgroundColor = backgroundColor;

        canvas = new Drawable();
        canvas.Paint += Canvas_Paint;

        Content = canvas;
    }

    private void Canvas_Paint(object sender, PaintEventArgs e)
    {
        e.Graphics.FillRectangle(backgroundColor, e.ClipRectangle);

        foreach (var (img, x, y, tint, s, r) in images)
        {
            DrawImage(e.Graphics, img, x, y, tint, s ,r);
        }
    }

    public Image LoadImage(string imagePath)
    {
        return new Bitmap(imagePath);
    }

    public void DrawImage(Image img, int x, int y, CTR.Color tint,CTR.Vector3 s,CTR.Vector3 r)
    {
        images.Add((img, x, y, tint,s,r));
        canvas.Invalidate(); // Refresh UI
    }

    private void DrawImage(Graphics graphics, Image img, int x, int y, CTR.Color tint, CTR.Vector3 size, CTR.Vector3 rotation)
    {
        float angle = (float)rotation.z; // Rotation around the Z-axis

        // Create a new bitmap to apply transformations
        Bitmap bitmap = new Bitmap(img.Size, PixelFormat.Format32bppRgba);

        using (Graphics g2 = new Graphics(bitmap))
        {
            g2.DrawImage(img, 0, 0);

            // Create a color that will overlay on the image (tint)
            var overlayColor = Color.FromArgb(tint.r, tint.g, tint.b, tint.blend);

            // Create a brush with the overlay color
            using (var brush = new SolidBrush(overlayColor))
            {
                // Fill the image area with the tint color
                g2.FillRectangle(brush, 0, 0, img.Width, img.Height);
            }
        }

        // Save original transformation state
        graphics.SaveTransform();

        // Move to the desired position (accounting for center origin)
        float scaledWidth = bitmap.Width * (float)size.x;
        float scaledHeight = bitmap.Height * (float)size.y;

        graphics.TranslateTransform(x, y);  // Move to the specified (x, y) position
        graphics.RotateTransform(angle);   // Rotate around the new origin (center)
        graphics.ScaleTransform((float)size.x, (float)size.y); // Apply scaling

        // Move back to center the image properly
        graphics.TranslateTransform(-bitmap.Width / 2, -bitmap.Height / 2);

        // Draw the transformed image
        graphics.DrawImage(bitmap, 0, 0);

        // Restore the original transformation state
        graphics.RestoreTransform();
        bitmap.Dispose();
    }
}
