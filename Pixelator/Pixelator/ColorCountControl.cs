using System.Drawing;
using System.Windows.Forms;

namespace Pixelator {
    /// <summary>
    /// Displays a color and its usage count.
    /// </summary>
    public partial class ColorCountControl : UserControl {
        public ColorCountControl() {
            InitializeComponent();
        }

        private Color _color;
        /// <summary>
        /// The color. When setting it, a small image will be generated to show the color.
        /// </summary>
        public Color Color {
            get {
                return _color;
            }
            set {
                if (_color == value)
                    return;

                _color = value;
                Bitmap bmp = new Bitmap(32, 32);
                using (Graphics g = Graphics.FromImage(bmp)) {
                    g.Clear(value);
                }

                picColor.Image = bmp;
                UpdateText();
            }
        }

        private int _colorCount;
        /// <summary>
        /// The color usage count.
        /// </summary>
        public int ColorCount {
            get {
                return _colorCount;
            }
            set { 
                _colorCount = value;
                UpdateText();
            }
        }

        /// <summary>
        /// Updates lblCount with a hexadecimal version of the color and the usage count.
        /// </summary>
        private void UpdateText() {
            lblCount.Text = string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}, {4} time(s)", Color.A, Color.R, Color.G, Color.B, ColorCount);
        }
    }
}
