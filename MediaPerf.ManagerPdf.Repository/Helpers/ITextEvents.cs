using iTextSharp.text;
using iTextSharp.text.pdf;
using MediaPerf.ManagerPdf.Model.Contracts;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace MediaPerf.ManagerPdf.Repository.Helpers
{
    public class ITextEvents : PdfPageEventHelper
    {
        #region Fields
        private string _header;
        #endregion

        #region Properties
        // This is the contentbyte object of the writer
        PdfContentByte cb;

        // we will put the final number of pages in a template
        PdfTemplate headerTemplate, footerTemplate;

        // this is the BaseFont we are going to use for the header / footer
        BaseFont bf = null;

        // This keeps track of the creation time
        DateTime PrintTime = DateTime.Now;

        public string Header
        {
            get { return _header; }
            set { _header = value; }
        }
        #endregion

        private int _countPv = 0;
        private IHeaderPage _headerPage = null;
        private IFooterPage _footerPage = null;

        #region -- Constructor --
        public ITextEvents(IHeaderPage headerPage,
          IFooterPage footerPage, int countPv)
        {
            _countPv = countPv;
            _headerPage = headerPage;
            _footerPage = footerPage;
        }
        #endregion

        #region -- Methods --
        public override void OnOpenDocument(PdfWriter writer, Document document)
        {
            try
            {
                PrintTime = DateTime.Now;
                bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                cb = writer.DirectContent;
                headerTemplate = cb.CreateTemplate(100, 100);
                footerTemplate = cb.CreateTemplate(50, 50);
            }
            catch (DocumentException documentException)
            {
                Console.WriteLine(documentException.ToString());
            }
            catch (IOException iOException)
            {
                Console.WriteLine(iOException.ToString());
            }
        }

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            base.OnEndPage(writer, document);

            #region -- Define fonts --
            BaseFont baseFont = BaseFont.CreateFont(BaseFont.COURIER, BaseFont.CP1252, false);
            var noneFontOneWhite = new Font(baseFont, 1, Font.NORMAL, BaseColor.WHITE);
            var boldFontElevenBlack = new Font(baseFont, 11, Font.BOLD, BaseColor.BLACK);
            var normalFontElevenBlack = new Font(baseFont, 11, Font.NORMAL, BaseColor.BLACK);
            var boldFontNineBlack = new Font(baseFont, 9, Font.BOLD, BaseColor.BLACK);
            var normalFontNimeBlack = new Font(baseFont, 9, Font.NORMAL, BaseColor.BLACK);
            var boldFontTwelveBlack = new Font(baseFont, 12, Font.BOLD, BaseColor.BLACK);
            var normalFontTwelveBlack = new Font(baseFont, 12, Font.NORMAL, BaseColor.BLACK);
            var boldFontTenBlack = new Font(baseFont, 10, Font.BOLD, BaseColor.BLACK);
            var normalFontTenBlack = new Font(baseFont, 10, Font.NORMAL, BaseColor.BLACK);
            var normalFontEightBlack = new Font(baseFont, 8, Font.NORMAL, BaseColor.BLACK);
            var boldFontTwentyBlack = new Font(Font.FontFamily.TIMES_ROMAN, 20, Font.BOLD, BaseColor.BLACK);
            var boldFontFortyTwoLightGray = new Font(Font.FontFamily.TIMES_ROMAN, 39, Font.BOLD, BaseColor.LIGHT_GRAY);
            #endregion

            //Create PdfTable object
            PdfPTable pdfTab = new PdfPTable(3);

            #region --- First header logo(left) and docTitle and page counter(rigth)  ---
            PdfPTable table = new PdfPTable(2);
            table.TotalWidth = document.PageSize.Width - document.LeftMargin - document.RightMargin;
            table.DefaultCell.Border = Rectangle.NO_BORDER;

            string imgPath = "https://ftp.mediaperf.com/img/logo.gif";
            //Image imagePath = Image.GetInstance("imgPath");
            //imagePath.ScalePercent(80f);
            PdfPCell cell = new PdfPCell(new Phrase("Image.GetInstance(imagePath)"));
            cell.Border = 0;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            //---------------------------------------------
            PdfPTable subTable = new PdfPTable(4);
            subTable.WidthPercentage = 70;
            subTable.DefaultCell.Border = Rectangle.NO_BORDER;
            cell = new PdfPCell(new Phrase("Relevé de redévances", boldFontTwentyBlack))
            {
                Colspan = 3,
                PaddingBottom = 10,
                MinimumHeight = 50,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_CENTER
            };
            subTable.AddCell(cell);


            Paragraph paragraph = new Paragraph("Page\r\n ", boldFontTwelveBlack);
            cell = new PdfPCell(paragraph);
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            subTable.AddCell(cell);

            cell = new PdfPCell(new Phrase("Ceci n'est pas une facture", boldFontTwelveBlack));
            cell.Colspan = 4;
            cell.PaddingBottom = 8;
            cell.MinimumHeight = 20;
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            subTable.AddCell(cell);
            table.AddCell(subTable);
            #endregion

            // -- Row 1 We will have to create separate cells to include image logo and 2 separate strings
            PdfPCell firstCell = new PdfPCell(table);
            firstCell.HorizontalAlignment = Element.ALIGN_CENTER;
            firstCell.Colspan = 3;
            firstCell.Border = 0;
            pdfTab.AddCell(firstCell);

            string text = "Page " + writer.PageNumber + " / ";

            #region -- No need off this --
            //Add paging to header
            {
                //cb.BeginText();
                //cb.SetFontAndSize(bf, 12);
                ////cb.SetTextMatrix(document.PageSize.GetRight(200), document.PageSize.GetTop(45));
                //cb.SetTextMatrix(document.PageSize.GetRight(90), document.PageSize.GetTop(45));
                //cb.ShowText(text);
                //cb.EndText();
                //float len = bf.GetWidthPoint(text, 12);
                ////Adds "12" in Page 1 of 12
                ////cb.AddTemplate(headerTemplate, document.PageSize.GetRight(200) + len, document.PageSize.GetTop(45));
                //cb.AddTemplate(headerTemplate, document.PageSize.GetRight(80) + len, document.PageSize.GetTop(45));
            }
            //Add paging to footer
            {
                //cb.BeginText();
                //cb.SetFontAndSize(bf, 12);
                ////cb.SetTextMatrix(document.PageSize.GetRight(180), document.PageSize.GetBottom(30));
                //cb.SetTextMatrix(document.PageSize.GetRight(80), document.PageSize.GetBottom(30));
                //cb.ShowText(text);
                //cb.EndText();
                //float len = bf.GetWidthPoint(text, 12);
                ////cb.AddTemplate(footerTemplate, document.PageSize.GetRight(180) + len, document.PageSize.GetBottom(30));
                //cb.AddTemplate(footerTemplate, document.PageSize.GetRight(80) + len, document.PageSize.GetBottom(30));
            }
            #endregion

            #region -- Row 2 Header Duplicata-- DUPLICATA
            Phrase phrase = new Phrase(_headerPage.Duplicata.ToUpper(), boldFontFortyTwoLightGray);
            PdfPCell duplicataCell = new PdfPCell(phrase);
            duplicataCell.HorizontalAlignment = Element.ALIGN_RIGHT;
            duplicataCell.VerticalAlignment = Element.ALIGN_TOP;
            duplicataCell.PaddingBottom = 15;
            duplicataCell.PaddingTop = 8;
            duplicataCell.Colspan = 3;
            duplicataCell.Border = 0;
            pdfTab.AddCell(duplicataCell);
            #endregion

            #region -- Row 3 OK OK OK Header first line OK OK OK --
            subTable = new PdfPTable(3);
            subTable.DefaultCell.Border = Rectangle.NO_BORDER;

            #region -- Left --
            PdfPTable firstLineTable = new PdfPTable(1);
            cell = new PdfPCell();
            cell.MinimumHeight = 15;
            paragraph = new Paragraph();
            paragraph.Add(new Phrase("      N° RdR", boldFontElevenBlack));
            paragraph.Add(new Chunk($"    { _footerPage.IdBFP }", normalFontElevenBlack));
            cell.AddElement(paragraph);
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.PaddingTop = -8;
            cell.PaddingRight = 2;
            cell.PaddingLeft = -2;
            firstLineTable.AddCell(cell);

            subTable.AddCell(firstLineTable);
            #endregion

            #region -- Middle --
            firstLineTable = new PdfPTable(1);
            cell = new PdfPCell();
            paragraph = new Paragraph();
            paragraph.Add(new Phrase("   Date", boldFontElevenBlack));
            paragraph.Add(new Chunk($"       {PrintTime.ToShortDateString()}  ", normalFontElevenBlack));
            cell.AddElement(paragraph);
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.PaddingTop = -8;
            cell.PaddingRight = 2;
            cell.PaddingLeft = 2;
            firstLineTable.AddCell(cell);

            subTable.AddCell(firstLineTable);
            #endregion

            #region -- Right --
            firstLineTable = new PdfPTable(1);
            cell = new PdfPCell();
            paragraph = new Paragraph(new Phrase(new Phrase("        Destinataire\r\n", boldFontElevenBlack)));
            cell.AddElement(paragraph);
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.PaddingTop = -8;
            cell.PaddingLeft = 2;
            cell.PaddingRight = -2;
            firstLineTable.AddCell(cell);

            subTable.AddCell(firstLineTable);
            #endregion

            PdfPCell pdfCell5 = new PdfPCell(subTable);
            pdfCell5.Colspan = 3;
            pdfCell5.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfCell5.VerticalAlignment = Element.ALIGN_MIDDLE;
            pdfCell5.Padding = 2;
            pdfCell5.Border = 0;
            pdfTab.AddCell(pdfCell5);
            #endregion

            #region -- Header SECOND line --
            subTable = new PdfPTable(3);
            cell = new PdfPCell();
            paragraph = new Paragraph();
            paragraph.Add(new Phrase($" {_headerPage.BfpParam1}  \r\n", normalFontElevenBlack));
            paragraph.Add(new Phrase($" {_headerPage.BfpParam2}  \r\n", normalFontElevenBlack));
            cell = new PdfPCell(paragraph);
            cell.Padding = 2;
            cell.Colspan = 2;
            cell.MinimumHeight = 110;
            subTable.AddCell(cell);

            // --------------------------------------------------------------
            cell = new PdfPCell();
            paragraph = new Paragraph();
            paragraph.Add(new Phrase($"\n {_headerPage.Destinataire}  \r\n", normalFontElevenBlack));
            paragraph.Add(new Phrase($" {_headerPage.Prestataire}  \r\n", normalFontElevenBlack));
            paragraph.Add(new Phrase($" {_headerPage.AdressePrestataire}  \r\n\n", normalFontElevenBlack));
            cell.AddElement(paragraph);
            cell.Padding = 2;
            cell.Rowspan = 2;
            subTable.AddCell(cell);

            // --------------------------------------------------------------
            paragraph = new Paragraph();
            paragraph.Add(new Phrase($" Nombre de Pv                {_headerPage.NbPv}  \n", boldFontElevenBlack));
            paragraph.Add(new Phrase($" Nombre de Campagnes         {_countPv.ToString()}  \n\n", boldFontElevenBlack));
            cell = new PdfPCell(paragraph);
            cell.Padding = 2;
            cell.Colspan = 2;
            subTable.AddCell(cell);

            PdfPCell pdfCell6 = new PdfPCell(subTable);
            pdfCell6.Colspan = 3;
            pdfCell6.Border = 0;
            pdfCell6.Padding = 2;
            pdfTab.AddCell(pdfCell6);
            #endregion

            #region -- Footer -- 
            PdfPTable footerPageTable = new PdfPTable(1);
            footerPageTable.SpacingBefore = 100;

            footerPageTable.TotalWidth = 70;
            footerPageTable.TotalWidth = document.PageSize.Width - document.LeftMargin - document.RightMargin;
            PdfPTable table2 = new PdfPTable(1);

            string footer = _footerPage.BfpParam6;
            string[] footerTextArray = Regex.Split(footer, "\r\n");

            PdfPCell cell2 = null;
            for (int i = 0; i < footerTextArray.Length; i++)
            {
                cell2 = new PdfPCell(new Phrase(footerTextArray[i], normalFontEightBlack));
                cell2.Border = Rectangle.NO_BORDER;
                table2.AddCell(cell2);
            }

            table2.DefaultCell.Border = Rectangle.NO_BORDER;
            cell = new PdfPCell(table2);
            cell.BorderColor = BaseColor.WHITE;
            footerPageTable.AddCell(cell);

            footerPageTable.WriteSelectedRows(0, -1, 20, 70, writer.DirectContent);
            #endregion

            pdfTab.TotalWidth = document.PageSize.Width - 39f;
            pdfTab.WidthPercentage = 96;

            ////first param is start row. -1 indicates there is no end row and all the rows to be included to write
            var heigth = document.PageSize.Height - 19;   //842  -  20 = 822
            var width = document.PageSize.Width;    // 595
            pdfTab.WriteSelectedRows(0, -1, 19, heigth, writer.DirectContent);
            //set pdfContent value

            //////Move the pointer and draw line to separate header section from rest of page
            //// -- Tracer une ligne horizontale --
            //cb.MoveTo(40, document.PageSize.Height - 100);
            //cb.LineTo(document.PageSize.Width - 40, document.PageSize.Height - 100);
            //cb.Stroke();

            //Move the pointer and draw line to separate footer section from rest of page
            //cb.MoveTo(40, document.PageSize.GetBottom(76));
            cb.MoveTo(20, document.PageSize.GetBottom(71));
            //cb.LineTo(document.PageSize.Width - 50, document.PageSize.GetBottom(76));
            //cb.LineTo(document.PageSize.Width - 26, document.PageSize.GetBottom(76));
            cb.LineTo(document.PageSize.Width - 20, document.PageSize.GetBottom(71));
            cb.Stroke();
        }

        /// <summary>
        /// --  --
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="document"></param>
        public override void OnCloseDocument(PdfWriter writer, Document document)
        {
            base.OnCloseDocument(writer, document);

            headerTemplate.BeginText();
            headerTemplate.SetFontAndSize(bf, 12);
            headerTemplate.SetTextMatrix(0, 0);
            headerTemplate.ShowText((writer.PageNumber - 1).ToString());
            headerTemplate.EndText();

            footerTemplate.BeginText();
            footerTemplate.SetFontAndSize(bf, 12);
            footerTemplate.SetTextMatrix(0, 0);
            footerTemplate.ShowText((writer.PageNumber - 1).ToString());
            footerTemplate.EndText();
        } 
        #endregion
    }
}
