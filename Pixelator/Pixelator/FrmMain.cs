using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Pixelator.Interpolation;

namespace Pixelator {
    public partial class FrmMain : Form {
        private Dictionary<Color, List<Rectangle>> drawList;
        private Image originalImage;
        private IInterpolator interpolator = new MiddlePixelInterpolator();

        public FrmMain() {
            InitializeComponent();
        }
        
        private void txtImage_DragOver(object sender, DragEventArgs e) {
            //Allow dropping files on the TextBox
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                e.Effect = DragDropEffects.Move;
                return;
            }

            e.Effect = DragDropEffects.None;
        }

        private void txtImage_DragDrop(object sender, DragEventArgs e) {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            string[] files = e.Data.GetData(DataFormats.FileDrop, true) as string[];
            if (files == null || files.Length == 0)
                return;

            txtImage.Text = files[0];
            ReadImage(txtImage.Text);
        }

        /// <summary>
        /// Browse for an image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBrowse_Click(object sender, EventArgs e) {
            using (OpenFileDialog ofd = new OpenFileDialog()) {
                ofd.Filter = "Images (*.bmp; *.jpg; *.jpeg; *.gif; *.png)|*.bmp;*.jpg;*.jpeg;*.gif;*.png|All files (*.*)|*.*||";
                ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

                if (ofd.ShowDialog(this) != DialogResult.OK)
                    return;

                txtImage.Text = ofd.FileName;
                ReadImage(txtImage.Text);
            }
        }

        /// <summary>
        /// Opens the image specified by <paramref name="file"/> and triggers the Pixelate method.
        /// </summary>
        /// <param name="file"></param>
        private void ReadImage(string file) {
            Bitmap bmp = new Bitmap(file);
            originalImage = bmp;

            Pixelate(showGridToolStripMenuItem.Checked);
        }

        /// <summary>
        /// Renders a pixelated version of <see cref="originalImage"/>. If <paramref name="renderGrid"/> is true, a grid will be drawn over the image.
        /// </summary>
        /// <param name="renderGrid"></param>
        /// <returns>The pixelated version of the image.</returns>
        private Image RenderImage(bool renderGrid = true) {
            if (originalImage as Bitmap == null)
                return null;

            Bitmap source = originalImage as Bitmap;
            Bitmap rasterized = new Bitmap(originalImage.Width, originalImage.Height);
            int gridSize = tbGridSize.Value;

            //Well keep a list of rectangles per color, so we can show the usage count of each color,
            //but this also enables us to draw all rectangles of the same color in one operation.
            drawList = new Dictionary<Color, List<Rectangle>>();

            for (int x = 0; x < rasterized.Width; x += gridSize) {
                for (int y = 0; y < rasterized.Height; y += gridSize) {
                    int width = gridSize, height = gridSize;

                    //Fix width or height if the image is not divided in squares (near the edges)
                    if (x + gridSize >= rasterized.Width)
                        width = (rasterized.Width) - x - 1;

                    if (y + gridSize >= rasterized.Height)
                        height = (rasterized.Height) - y - 1;

                    Rectangle block = new Rectangle(x, y, width, height);
                    Color pixel = interpolator.DetermineColor(source, block);

                    if (!drawList.ContainsKey(pixel))
                        drawList.Add(pixel, new List<Rectangle>());

                    drawList[pixel].Add(block);
                }
            }

            using (Graphics g = Graphics.FromImage(rasterized)) {
                //Using each color, draw the list of rectangles at once
                foreach (Color key in drawList.Keys) {
                    using (Brush b = new SolidBrush(key))
                        g.FillRectangles(b, drawList[key].ToArray());
                }

                if (renderGrid) {
                    //Draw horizontal lines
                    for (int x = gridSize; x < rasterized.Width; x += gridSize)
                        g.DrawLine(Pens.Black, x, 0, x, rasterized.Height);

                    //Draw vertical lines
                    for (int y = gridSize; y < rasterized.Height; y += gridSize)
                        g.DrawLine(Pens.Black, 0, y, rasterized.Width, y);
                }
            }

            return rasterized;
        }

        /// <summary>
        /// Pixelate will call <see cref="RenderImage"/>. If <see cref="originalImage"/> is null, RenderImage will also return null.
        /// Else, we'll enable the print controls (they can only be used after a first image has been loaded), show the pixelated image and
        /// list up to 64 colors and their usage count.
        /// </summary>
        /// <remarks>
        /// The reasons why we only show 64 colors:
        /// - We don't have that many different post-it colors.
        /// - If the original image contains too many different colors, the list of <see cref="ColorCountControl"/> objects would grow so big that a <see cref="System.ComponentModel.Win32Exception"/> (Error -2147467259: Error creating window handle) occurs.
        /// </remarks>
        private void Pixelate(bool renderGrid = true) {
            Image rasterized = RenderImage(renderGrid);
            if (rasterized == null)
                return;

            SuspendLayout();
            try {
                printToolStripMenuItem.Enabled = true; //Enable the print methods
                print2ToolStripMenuItem.Enabled = true;

                picImage.Image = rasterized; //Set the image
                pnlColors.Controls.Clear(); //Clear the color count panel.

                int i = 0;
                foreach (KeyValuePair<Color, List<Rectangle>> pair in drawList.OrderByDescending(p => p.Value.Count)) { //Order by usage count (most used first)
                    if (pair.Key.A == 0) //ignore full transparency
                        continue;

                    ColorCountControl c = new ColorCountControl();
                    c.Location = new Point(0, i * c.Height);
                    c.Size = new Size(pnlColors.Width, c.Height);
                    c.Color = pair.Key;
                    c.ColorCount = pair.Value.Count;

                    pnlColors.Controls.Add(c);
                    i++;
                    if (i > 63) //Stop when we have reached 64 different colors.
                        break;
                }
            }
            finally {
                ResumeLayout();
            }
        }

        /// <summary>
        /// TrackBar.ValueChanged: update the Label control that shows the pixel dimensions, and adjust the image.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbGridSize_ValueChanged(object sender, EventArgs e) {
            lblGridSize.Text = string.Format("{0}x{0}", tbGridSize.Value); //update the label
            Pixelate(showGridToolStripMenuItem.Checked); //And re-render the pixel version of the image
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
            Close();
        }

        private void openImageToolStripMenuItem_Click(object sender, EventArgs e) {
            btnBrowse_Click(sender, e);
        }

        private void printToolStripMenuItem_Click(object sender, EventArgs e) {
            if (picImage.Image == null)
                return;

            Image img = RenderImage(sender != print2ToolStripMenuItem); //If sender is print2 (print without grid), pass false to Pixelate.

            using (FrmPrintPreview fpp = new FrmPrintPreview()) {
                fpp.PrintImage = img;
                fpp.ShowDialog(this);
            }
        }

        private void showGridToolStripMenuItem_Click(object sender, EventArgs e) {
            Pixelate(showGridToolStripMenuItem.Checked);
        }

        private void InterpolationModeChanged(object sender, EventArgs e) {
            ToolStripMenuItem current = middlePixelToolStripMenuItem;
            if (interpolator is DominantPixelInterpolator)
                current = dominantPixelToolStripMenuItem;
            if (interpolator is WeightedAverageInterpolator)
                current = weightedAverageToolStripMenuItem;


            if (sender == current) {
                //Reapply the checkbox
                current.Checked = true;
                return;
            }

            if (sender == middlePixelToolStripMenuItem) {
                dominantPixelToolStripMenuItem.Checked = false;
                weightedAverageToolStripMenuItem.Checked = false;

                interpolator = new MiddlePixelInterpolator();
                Pixelate(showGridToolStripMenuItem.Checked);
                return;
            }

            if (sender == dominantPixelToolStripMenuItem) {
                middlePixelToolStripMenuItem.Checked = false;
                weightedAverageToolStripMenuItem.Checked = false;

                interpolator = new DominantPixelInterpolator();
                Pixelate(showGridToolStripMenuItem.Checked);
                return;
            }

            middlePixelToolStripMenuItem.Checked = false;
            dominantPixelToolStripMenuItem.Checked = false;

            interpolator = new WeightedAverageInterpolator();
            Pixelate(showGridToolStripMenuItem.Checked);
        }
    }
}
