using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Pixelator.Interpolation {
    /// <summary>
    /// The DominantPixelInterpolator returns the color that dominates in the specified area of the image.
    /// </summary>
    public class DominantPixelInterpolator : IInterpolator {
        public Color DetermineColor(Bitmap image, Rectangle area) {
            return DetermineColor(image, new RectangleF(area.Location, area.Size));
        }

        public Color DetermineColor(Bitmap image, RectangleF area) {
            Dictionary<Color, int> colorCount = new Dictionary<Color, int>();
            for (int x = (int)Math.Ceiling(area.Left); x <= (int)Math.Floor(area.Right); x++) {
                for (int y = (int)Math.Ceiling(area.Top); y <= (int)Math.Floor(area.Bottom); y++) {
                    Color c = image.GetPixel(x, y);
                    if (colorCount.ContainsKey(c) == false)
                        colorCount[c] = 1;
                    else
                        colorCount[c]++;
                }
            }

            return colorCount.OrderByDescending(pair => pair.Value).First().Key;
        }
    }
}