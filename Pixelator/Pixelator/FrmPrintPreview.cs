using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace Pixelator {
    /// <summary>
    /// This window is used for showing a print preview of the image and for printing the image as well.
    /// </summary>
    public partial class FrmPrintPreview : Form {
        private int currentPage;
        private int page;
        private int horizontalPageCount;
        private int verticalPageCount;
        private int pageCount;

        /// <summary>
        /// The image to print
        /// </summary>
        public Image PrintImage { get; set; }

        public FrmPrintPreview() {
            InitializeComponent();
        }

        /// <summary>
        /// PrintDocument.PrintPage: each time this event is called, a new page is being requested.
        /// By setting <see cref="PrintPageEventArgs.HasMorePages"/> to false, we indicate that we've reached the last page.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void printDocument_PrintPage(object sender, PrintPageEventArgs e) {
            Image img = PrintImage;

            //Retrieve the margins of the printable area.
            int left = printDocument.DefaultPageSettings.Margins.Left;
            int top = printDocument.DefaultPageSettings.Margins.Top;
            int pageWidth = printDocument.DefaultPageSettings.PaperSize.Width - printDocument.DefaultPageSettings.Margins.Left - printDocument.DefaultPageSettings.Margins.Right;
            int pageHeight = printDocument.DefaultPageSettings.PaperSize.Height - printDocument.DefaultPageSettings.Margins.Top - printDocument.DefaultPageSettings.Margins.Bottom;

            if (printDocument.DefaultPageSettings.Landscape) { //Switch width and height if landscape
                int temp = pageWidth;
                pageWidth = pageHeight;
                pageHeight = temp;
            }

            if (pageCount == 0) { //If pageCount is zero, we must start all over again. This happens if the user changes margins or page layout, or decides to actually print the image.
                //Determine the number of pages
                horizontalPageCount = (int)Math.Ceiling(img.Width * 1.0 / pageWidth);
                verticalPageCount = (int)Math.Ceiling(img.Height * 1.0 / pageHeight);
                pageCount = horizontalPageCount * verticalPageCount;
                lblPageCount.Text = pageCount.ToString();
            }

            int xIndex = page % horizontalPageCount; //x and y indices of the part of the image that we're about to print (preview)
            int yIndex = page / horizontalPageCount;

            //Map the image over multiple pages by keeping track which part is needed.
            /*
             * In example (landscape layout):
             * 
             *                                  horizontalPageCount = 3
             *    *------------------------------------*------------------------------------*------*
             *    *                                    *                                    *      *
             *    *                                    *                                    *      *
             *    *                Page 1              *         Page 2                     *  P.3 *
             *    *                                    *                                    *      *
             *    *                                    *                                    *      *
             *    *                                    *                                    *      *
             *    *------------------------------------*------------------------------------*------*
             *    *                                    *                                    *      *
             *    *                                    *                                    *      *
             *    *                Page 4              *         Page 5                     *  P.6 *
             *    *                                    *                                    *      *
             *    *                                    *                                    *      *
             *    *                                    *                                    *      *
             *    *------------------------------------*------------------------------------*------*
             * verticalPageCount = 2
             * 
             */
            float partWidth = pageWidth;
            if (xIndex == horizontalPageCount - 1)
                partWidth = img.Width - xIndex * pageWidth;

            float partHeight = pageHeight;
            if (yIndex == verticalPageCount - 1)
                partHeight = img.Height - yIndex * pageHeight;

            RectangleF imagePart = new RectangleF(xIndex * pageWidth, yIndex * pageHeight, partWidth, partHeight);
            RectangleF target = new RectangleF(left, top, partWidth, partHeight);

            e.Graphics.DrawImage(img, target, imagePart, GraphicsUnit.Pixel);
            page++;

            //If the current page does not equal pageCount, there are more pages to render.
            e.HasMorePages = page < pageCount;
        }

        private void FrmPrintPreview_Load(object sender, EventArgs e) {
            pageSetupDialog.EnableMetric = true;
        }
        
        private void cboZoom_SelectedIndexChanged(object sender, EventArgs e) {
            if (cboZoom.SelectedIndex == -1 && string.IsNullOrWhiteSpace(cboZoom.Text))
                return;

            string zoom = cboZoom.Text.Trim(' ', '\t', '%');
            int zoomLevel;
            if (int.TryParse(zoom, out zoomLevel) == false || zoomLevel < 10 || zoomLevel > 800) {
                MessageBox.Show(this, "Invalid zoom value. Please enter a value between 10% and 800%.", "Pixelator", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            printPreviewControl.Zoom = zoomLevel / 100.0;
        }

        private void cboZoom_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
                cboZoom_SelectedIndexChanged(sender, EventArgs.Empty);
        }

        private void txtPage_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode != Keys.Enter && e.KeyCode != Keys.Return)
                return;


            int nPage;
            if (int.TryParse(txtPage.Text, out nPage) == false || nPage < 1 || nPage > pageCount) {
                MessageBox.Show(this, "Invalid page number.", "Pixelator", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            NavigateToPage(nPage - 1);
        }

        private void btnPreviousPage_Click(object sender, EventArgs e) {
            NavigateToPage(currentPage - 1);
        }

        private void btnNextPage_Click(object sender, EventArgs e) {
            NavigateToPage(currentPage + 1);
        }

        /// <summary>
        /// Navigates to page <paramref name="nPage"/> of the print preview.
        /// </summary>
        /// <param name="nPage"></param>
        private void NavigateToPage(int nPage) {
            if (nPage < 0 || nPage >= pageCount)
                return;

            currentPage = nPage;
            btnPreviousPage.Enabled = nPage != 0;
            btnNextPage.Enabled = nPage != pageCount - 1;

            txtPage.Text = (currentPage + 1).ToString();
            printPreviewControl.StartPage = currentPage;
        }

        private void btnPageSetup_Click(object sender, EventArgs e) {
            pageSetupDialog.PageSettings = printDocument.DefaultPageSettings;

            if (pageSetupDialog.ShowDialog(this) != DialogResult.OK)
                return;

            pageCount = 0;
            page = 0;
            printDocument.DefaultPageSettings = pageSetupDialog.PageSettings;
            printPreviewControl.InvalidatePreview();
            NavigateToPage(0);
        }

        private void btnPrint_Click(object sender, EventArgs e) {
            if (printDialog.ShowDialog(this) != DialogResult.OK)
                return;

            printDocument.PrinterSettings = printDialog.PrinterSettings;
            page = 0;
            pageCount = 0;
            printDocument.Print();
            Close();
        }
    }
}
