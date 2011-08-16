using System.Drawing;

namespace Pixelator.Interpolation {
    /// <summary>
    /// This IInterpolator simply returns the color in the middle of the specified area of the image, as it was originally in the previous version of Pixelator.
    /// </summary>
    public class MiddlePixelInterpolator : IInterpolator {
        public Color DetermineColor(Bitmap image, Rectangle area) {
            return DetermineColor(image, new RectangleF(area.Location, area.Size));
        }

        public Color DetermineColor(Bitmap image, RectangleF area) {
            return image.GetPixel((int)(area.Left + area.Width / 2), (int)(area.Top + area.Height / 2));
        }
    }
}