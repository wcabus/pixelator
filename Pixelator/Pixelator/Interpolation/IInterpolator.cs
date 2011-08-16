using System;
using System.Drawing;

namespace Pixelator.Interpolation {
    /// <summary>
    /// The IInterpolator interface is used for every interpolation technique.
    /// </summary>
    public interface IInterpolator {
        /// <summary>
        /// Determine the color in the given image and area.
        /// </summary>
        /// <param name="image">The target image</param>
        /// <param name="area">The <see cref="Rectangle"/> of the image where we want to retrieve a color.</param>
        /// <returns>The color in the given area, according to the used interpolation algorithm.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="image"/> or <paramref name="area"/> is a null reference.</exception>
        Color DetermineColor(Bitmap image, Rectangle area);

        /// <summary>
        /// Determine the color in the given image and area.
        /// </summary>
        /// <param name="image">The target image</param>
        /// <param name="area">The <see cref="RectangleF"/> of the image where we want to retrieve a color.</param>
        /// <returns>The color in the given area, according to the used interpolation algorithm.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="image"/> or <paramref name="area"/> is a null reference.</exception>
        Color DetermineColor(Bitmap image, RectangleF area);
    }
}