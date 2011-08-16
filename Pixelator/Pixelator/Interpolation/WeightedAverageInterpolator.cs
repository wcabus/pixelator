using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Pixelator.Interpolation {
    /// <summary>
    /// The WeightedAverageInterpolator returns a color that is the weighted average of all colors in the specified area in the image.
    /// </summary>
    public class WeightedAverageInterpolator : IInterpolator {
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

            int r = 0, g = 0, b = 0;
            var countPerColor = from pair in colorCount
                                group pair by pair.Key into @group
                                select new { Color = @group.Key, Count = @group.Count() };

            int total = 0;
            foreach (var count in countPerColor) {
                total += count.Count;
                r += count.Count * count.Color.R;
                g += count.Count * count.Color.G;
                b += count.Count * count.Color.B;
            }

            r /= total;
            g /= total;
            b /= total;

            return Color.FromArgb(r, g, b);
        }
    }
}