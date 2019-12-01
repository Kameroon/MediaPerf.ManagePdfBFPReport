using Dapper;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using MediaPerf.ManagerPdf.Model.Contracts;
using MediaPerf.ManagerPdf.Model.Implemenations;
using MediaPerf.ManagerPdf.Repository.Contracts;
using MediaPerf.ManagerPdf.Repository.Helpers;
using MediaPerf.ManagerPdf.Repository.Helpers.Contracts;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using NLog;
using MediaPerf.ManagerPdf.Infrastrure.Contracts;
using System.Windows;
using MediaPerf.ManagerPdf.Model.Implementations;
using System.Xml.Linq;
using MediaPerf.ManagerPdf.MailService.Contracts;
using MediaPerf.ManagerPdf.MailService.Implementations;

namespace MediaPerf.ManagerPdf.Repository.Implementations
{
    // -- Pagination --
    // -- https://stackoverflow.com/questions/35821278/itextsharp-table-span-pages-using-stamper --

    public class PdfRepository : IPdfRepository
    {
        #region -- Constantes --
        private const string MAIL_TEMPLATE_PATH = @"C:\Users\mMABOU\Desktop\MailTemplates\BfpRedevenceMailTemplate.htm";
        private const string PDF_REPOSITORY_PATH = @"C:\Users\mMABOU\Desktop\PDFFiles\";
        #endregion

        #region -- Fields -- 
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private IConnectionStringHelper _connectionStringHelper;
        private DataSet _adressTemple = null;
        private DataSet _royaltyFeeDataSet = null;
        private DataSet _headerDataSetTemple = null;
        private DataSet _footerDataSetTemple = null;
        private DataSet _bfpReportHistoricDataSet = null;
        //private DataSet _royaltyFeeDataSetTemplate = null;
        private static object _lockObj = new object();

        private IRoyaltyFee _royaltyFee = null;
        private IHeaderPage _headerPage = null;
        private IFooterPage _footerPage = null;
        private IMailTemplate _mailTemplate = null;
        private IContactRedevance _contactRoyaltyFee = null;
        private IEmailMessageService _emailMessageService = null;
        private IBfpReportHistoric _bfpReportHistoric = null;
        private IConsolidateHelper _consolidateHelper = null;
        private readonly IDialogService _dialogService = null;

        private IEnumerable<BfpReportHistoric> _bfpReportHistoricsEnumerable = null;
        #endregion  

        #region -- Properties --  
        private string ErrorMessage { get; set; }
        private string _fullPdfFilePath { get; set; }
        private int _countPv = 0;
        private StringBuilder ErrorMessages = new StringBuilder();
        #endregion

        #region -- Constructor --
        public PdfRepository(IHeaderPage headerPage,
            IFooterPage footerPage,
            IRoyaltyFee royaltyFee,
            IMailTemplate mailTemplate,
            IContactRedevance contactRedevance,
            IConsolidateHelper consolidateHelper,
            IBfpReportHistoric bfpReportHistoric,
            IConnectionStringHelper connectionString,
            IDialogService dialogService,
            IEmailMessageService emailMessageService)
        {
            _logger.Debug($"==> Début initialisation du Repository");
            _footerPage = footerPage;
            _headerPage = headerPage;
            _royaltyFee = royaltyFee;
            _mailTemplate = mailTemplate;
            _contactRoyaltyFee = contactRedevance;
            _bfpReportHistoric = bfpReportHistoric;
            _dialogService = dialogService;
            _emailMessageService = emailMessageService;
            _consolidateHelper = consolidateHelper;
            _connectionStringHelper = connectionString;
            _logger.Debug($"==> Fin initialisation du Repository");
        }
        #endregion

        #region -- Methods --
        /// <summary>
        /// -- Create SQL Connection --
        /// </summary>
        /// <returns></returns>
        private SqlConnection CreateConnection()
        {
            return new SqlConnection(_connectionStringHelper.GetConnectionString());
        }

        #region -------------------------------------------
        public bool CreateRoyaltyFeePdfFile(string customPdfFileName,
             IHeaderPage headerPage,
             IFooterPage footerPage,
             XDocument xDocument)
        {
            _logger.Debug($"==> Début création du fichier Pdf");

            #region -- Fields --
            bool result = false;

            double total = 0;
            double totalHT = 0;
            int countDetail = 0;
            int totalCountDetail = 0;
            double totalTTC = 0;
            double montantTva = 0;
            double totalHtArrondi = 0;
            string idPv = null;
            string dpName = null;
            string communeName = null;
            string enseigneName = null;
            string productName = null;
            IEnumerable<XElement> _detailXElements = null;
            #endregion

            #region --   --
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            //PdfWriter masterWriter = null;
            DateTime fileCreationDatetime = DateTime.Now;

            var widthPercentage = 96;

            string extension = ".pdf";
            string date = fileCreationDatetime.ToString(@"ddMMyyyy");
            string fileName = $"_{date}{extension}";

            _logger.Debug($"==> Génération du chemin du fichier");
            _fullPdfFilePath = string.Concat(PDF_REPOSITORY_PATH, customPdfFileName, extension);

            if (File.Exists(_fullPdfFilePath))
            {
                File.Delete(_fullPdfFilePath);
            }
            #endregion

            try
            {
                // -- Define royalTies columns widths --
                float[] widths = new float[] { 10f, 60f, 20f, 20f };


                //xDocument = XDocument.Load("..\\..\\Product.xml");

                if (xDocument != null && xDocument.Descendants("TypeProduit").Elements("Produit").Count() > 0)
                {
                    // -- Retrieve count Pv --
                    _countPv = xDocument.Descendants("TypeProduit").Elements("Produit").Elements("Dp").Elements("Details").Count();

                    // -- MMA All Using has changed --
                    using (var masterStream = new FileStream(_fullPdfFilePath, FileMode.Create))
                    using (Document masterDocument = new Document(PageSize.A4, 10f, 10f, 340f, 120f))   //(PageSize.A4, 5, 12, 20, 10)  //new Document(PageSize.A4, 10f, 10f, 350f, 150f))  PageSize.A4, 10f, 10f, 368f, 100f)
                    using (PdfWriter masterWriter = PdfWriter.GetInstance(masterDocument, masterStream))
                    {
                        #region -- Setting Document properties e.g. --
                        masterDocument.AddTitle("Envoi des relevés de redevance par mail");
                        masterDocument.AddSubject("Génération de relevés de redevance en PDF pour envoi par mail");
                        masterDocument.AddKeywords("Metadata, iTextSharp 5.4.13.1");
                        masterDocument.AddCreator("MMA");
                        masterDocument.AddAuthor("M MABOU");
                        masterDocument.AddHeader("Some thing", "Header");
                        #endregion

                        ErrorMessage = null;

                        //// -- Setting Encryption properties --
                        //masterWriter.SetEncryption(PdfWriter.STRENGTH40BITS, "Vers@illes78", "000000", PdfWriter.ALLOW_COPY);

                        //// -- Add Event in all pages --
                        masterWriter.PageEvent = new ITextEvents(headerPage, footerPage, _countPv);

                        masterDocument.Open();

                        PdfContentByte pdfContentByte = masterWriter.DirectContent;
                        var lineSeparator = new LineSeparator(2.0F, 96.0F, BaseColor.RED, Element.ALIGN_CENTER, 1);

                        #region -- Define fonts --
                        BaseFont baseFont = BaseFont.CreateFont(BaseFont.COURIER, BaseFont.CP1252, false);
                        var noneFontOneWhite = new Font(baseFont, 1, Font.NORMAL, BaseColor.WHITE);
                        var normalFontEightBlack = new Font(baseFont, 8, Font.NORMAL, BaseColor.BLACK);
                        var boldFontEightBlack = new Font(baseFont, 8, Font.BOLD, BaseColor.BLACK);
                        var boldFontNineBlack = new Font(baseFont, 9, Font.BOLD, BaseColor.BLACK);
                        var normalFontNimeBlack = new Font(baseFont, 9, Font.NORMAL, BaseColor.BLACK);
                        var boldFontTenBlack = new Font(baseFont, 10, Font.BOLD, BaseColor.BLACK);
                        var normalFontTenBlack = new Font(baseFont, 10, Font.NORMAL, BaseColor.BLACK);
                        var boldFontEleventBlack = new Font(baseFont, 11, Font.BOLD, BaseColor.BLACK);
                        var normalFontEleventBlack = new Font(baseFont, 11, Font.NORMAL, BaseColor.BLACK);
                        var boldFontTwelveBlack = new Font(baseFont, 12, Font.BOLD, BaseColor.BLACK);
                        var normalFontTwelveBlack = new Font(baseFont, 12, Font.NORMAL, BaseColor.BLACK);
                        #endregion

                        #region -- Generate XML Grid --
                        PdfPTable royaltiesTables = new PdfPTable(4);

                        royaltiesTables.WidthPercentage = 96;

                        // -- Add Header row for every page --
                        royaltiesTables.HeaderRows = 1;

                        // -- Set spacing between gridView and  --
                        royaltiesTables.SpacingBefore = 5f;

                        // --   --
                        royaltiesTables.SetWidths(widths);

                        #region -- Table Header --
                        PdfPCell headerCell = new PdfPCell(new Phrase("No", boldFontTenBlack));
                        headerCell.Colspan = 0;
                        headerCell.MinimumHeight = 18;
                        headerCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                        headerCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                        headerCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        royaltiesTables.AddCell(headerCell);

                        headerCell = new PdfPCell(new Phrase("Campagne", boldFontTenBlack));
                        headerCell.Colspan = 0;
                        headerCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                        headerCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        headerCell.HorizontalAlignment = 0;
                        royaltiesTables.AddCell(headerCell);

                        headerCell = new PdfPCell(new Phrase("Du", boldFontTenBlack));
                        headerCell.Colspan = 0;
                        headerCell.HorizontalAlignment = 1;
                        headerCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                        headerCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        royaltiesTables.AddCell(headerCell);

                        headerCell = new PdfPCell(new Phrase("Au", boldFontTenBlack));
                        headerCell.Colspan = 0;
                        headerCell.HorizontalAlignment = 1;
                        headerCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                        headerCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        royaltiesTables.AddCell(headerCell);
                        #endregion

                        #region -- Generate pdf group grid --
                        
                        foreach (XElement communeXElement in xDocument.Descendants(nameof(_royaltyFee.Commune)))
                        {
                            var _communesElement = communeXElement.Elements(nameof(_royaltyFee.TypeProduit));
                            int tmpCountDetail = countDetail;
                            countDetail = 0;

                            foreach (XElement typeproduit in _communesElement)
                            {
                                var _produitsXElement = typeproduit.Elements(nameof(_royaltyFee.Produit));
                                tmpCountDetail = countDetail;
                                double tempTotal = total;
                                total = 0;

                                foreach (XElement element in _produitsXElement)
                                {
                                    productName = element.FirstAttribute?.Value;

                                    var _dpsXElement = element.Elements(nameof(_royaltyFee.Dp));

                                    foreach (XElement childEllement in _dpsXElement)
                                    {
                                        int tmpCount = 0;
                                        totalCountDetail = countDetail;
                                        countDetail = 0;

                                        dpName = childEllement.Attribute("Nom").Value;
                                        PdfPCell productNameCell = new PdfPCell(new Phrase($"{ productName }\n   { dpName }", boldFontNineBlack));
                                        productNameCell.Colspan = 4;
                                        productNameCell.MinimumHeight = 23;
                                        productNameCell.Padding = 3;
                                        productNameCell.HorizontalAlignment = 0;
                                        royaltiesTables.AddCell(productNameCell);

                                        _detailXElements = childEllement.Elements(nameof(_royaltyFee.Details));
                                        double sstotal = 0;
                                        foreach (XElement detailXML in _detailXElements)
                                        {
                                            PdfPCell detailCell = new PdfPCell(
                                                new Phrase((string)(detailXML.Element(nameof(_royaltyFee.IdCmp))),
                                                normalFontNimeBlack));
                                            detailCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                                            detailCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                            royaltiesTables.AddCell(detailCell);

                                            detailCell = new PdfPCell(
                                                new Phrase((string)(detailXML.Element(nameof(_royaltyFee.Campagne))),
                                                normalFontNimeBlack));
                                            detailCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                                            detailCell.HorizontalAlignment = Element.ALIGN_LEFT;
                                            royaltiesTables.AddCell(detailCell);

                                            detailCell = new PdfPCell(
                                                new Phrase((string)(detailXML.Element(nameof(_royaltyFee.DateDebut))),
                                                normalFontNimeBlack));
                                            detailCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                                            detailCell.HorizontalAlignment = Element.ALIGN_CENTER;
                                            royaltiesTables.AddCell(detailCell);

                                            detailCell = new PdfPCell(
                                                new Phrase((string)(detailXML.Element(nameof(_royaltyFee.DateFin))),
                                                normalFontNimeBlack));
                                            detailCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                                            detailCell.HorizontalAlignment = Element.ALIGN_CENTER;
                                            royaltiesTables.AddCell(detailCell);

                                            enseigneName = (string)(detailXML.Element(nameof(_royaltyFee.Enseigne)));
                                            communeName = (string)(detailXML.Element(nameof(_royaltyFee.Commune)));
                                            idPv = (string)(detailXML.Element(nameof(_royaltyFee.IdPv)));

                                            sstotal += double.Parse(detailXML.Element(nameof(_royaltyFee.MontantRdvcHT)).Value.Replace(".", ","));
                                        }

                                        // -- Set Sub Total Name --
                                        PdfPCell subTotalCell = new PdfPCell(new Phrase("Sous Total", boldFontNineBlack));
                                        subTotalCell.Colspan = 2;
                                        subTotalCell.PaddingRight = 10;
                                        subTotalCell.PaddingBottom = 5;
                                        subTotalCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                        subTotalCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                                        royaltiesTables.AddCell(subTotalCell);

                                        // -- Set Sub Total Value --
                                        countDetail = _detailXElements.Count();
                                        totalCountDetail = totalCountDetail + countDetail;
                                        subTotalCell = new PdfPCell(new Phrase(countDetail.ToString(), boldFontNineBlack));
                                        subTotalCell.HorizontalAlignment = Element.ALIGN_CENTER;
                                        subTotalCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                                        royaltiesTables.AddCell(subTotalCell);

                                        sstotal = Math.Round(sstotal, 2);
                                        var ssTotalPhrase = new Phrase(sstotal.ToString(), boldFontNineBlack);
                                        PdfPCell cel = new PdfPCell(ssTotalPhrase);
                                        cel.HorizontalAlignment = Element.ALIGN_CENTER;
                                        cel.VerticalAlignment = Element.ALIGN_MIDDLE;
                                        royaltiesTables.AddCell(cel);

                                        total += sstotal;
                                    }
                                }

                                string productType = typeproduit.Attribute("Nom").Value;

                                totalHT = tempTotal + total;
                                //countElementByProduct = countElementByProductType + countElementByDp;
                                totalCountDetail += totalCountDetail;

                                // -- Set Total Name --
                                #region -- Manage Total by product type --
                                totalHtArrondi = Math.Round(totalHT, 2);
                                Phrase productTypePhrase = new Phrase($"Total  { productType }", boldFontNineBlack);
                                PdfPCell productTypeCell = new PdfPCell(productTypePhrase);
                                productTypeCell.Colspan = 3;
                                productTypeCell.PaddingRight = 10;
                                productTypeCell.PaddingBottom = 5;
                                productTypeCell.MinimumHeight = 15;
                                productTypeCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                productTypeCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                                royaltiesTables.AddCell(productTypeCell);

                                double t = Math.Round(total, 2);
                                productTypePhrase = new Phrase(t.ToString(), boldFontNineBlack);  //totalHtArrondi.ToString()
                                productTypeCell = new PdfPCell(productTypePhrase);
                                productTypeCell.PaddingRight = 10;
                                productTypeCell.PaddingBottom = 5;
                                productTypeCell.MinimumHeight = 15;
                                productTypeCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                                productTypeCell.HorizontalAlignment = Element.ALIGN_CENTER;
                                productTypeCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                                royaltiesTables.AddCell(productTypeCell);
                                #endregion
                            }

                            // -- Set Sub table data --
                            if (countDetail > 0)
                            {
                                PdfPCell footerCell = new PdfPCell(
                                    new Phrase($" { idPv } - { enseigneName }  -  { communeName }", boldFontNineBlack));
                                footerCell.Colspan = 2;
                                footerCell.PaddingRight = 10;
                                footerCell.PaddingBottom = 5;
                                footerCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                                footerCell.HorizontalAlignment = Element.ALIGN_LEFT;
                                footerCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                                royaltiesTables.AddCell(footerCell);

                                footerCell = new PdfPCell(
                                    new Phrase($"Total    { totalCountDetail }         { totalHT.ToString() }", boldFontNineBlack));
                                footerCell.Colspan = 2;
                                footerCell.PaddingRight = 10;
                                footerCell.PaddingBottom = 5;
                                footerCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                                footerCell.HorizontalAlignment = Element.ALIGN_CENTER;
                                footerCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                                royaltiesTables.AddCell(footerCell);
                            }
                        }

                        masterDocument.Add(royaltiesTables);

                        // -- Set spacing between gridView and  --
                        royaltiesTables.SpacingAfter = 5f;
                        #endregion
                        #endregion
                        
                        //// -- Draw interemediate horizontal line. --
                        //var virtualLineSeparator = new LineSeparator(0.0F, 96.0F, BaseColor.BLUE, Element.ALIGN_CENTER, 1);
                        //Paragraph interemediateLineSeparator = new Paragraph(new Chunk(virtualLineSeparator));
                        //interemediateLineSeparator.SpacingBefore = 108f;
                        //masterDocument.Add(interemediateLineSeparator);

                        #region -- Total --
                        PdfPTable royalFeeTotalMasterTable = new PdfPTable(1);
                        royalFeeTotalMasterTable.TotalWidth = 400;
                        PdfPTable royalFeeTotalTable = new PdfPTable(4);

                        PdfPCell firstRoyalFeeTotalCell = new PdfPCell(new Phrase());
                        firstRoyalFeeTotalCell.BorderColor = BaseColor.WHITE;
                        firstRoyalFeeTotalCell.Border = 5;
                        royalFeeTotalTable.AddCell(firstRoyalFeeTotalCell);
                        PdfPCell royalFeeTotalCell = new PdfPCell(new Phrase("HT", boldFontNineBlack));
                        royalFeeTotalCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        royalFeeTotalCell.HorizontalAlignment = Element.ALIGN_CENTER;
                        royalFeeTotalTable.AddCell(royalFeeTotalCell);

                        string tva = "20,00";
                        royalFeeTotalCell = new PdfPCell(new Phrase($"TVA {_footerPage.TxTva}", boldFontNineBlack));
                        royalFeeTotalCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        royalFeeTotalCell.HorizontalAlignment = Element.ALIGN_CENTER;
                        royalFeeTotalTable.AddCell(royalFeeTotalCell);

                        royalFeeTotalCell = new PdfPCell(new Phrase("TTC", boldFontNineBlack));
                        royalFeeTotalCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        royalFeeTotalCell.HorizontalAlignment = Element.ALIGN_CENTER;
                        royalFeeTotalTable.AddCell(royalFeeTotalCell);

                        royalFeeTotalCell = new PdfPCell(new Phrase("Total du relevé", boldFontNineBlack));
                        royalFeeTotalCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        royalFeeTotalCell.HorizontalAlignment = Element.ALIGN_CENTER;
                        royalFeeTotalTable.AddCell(royalFeeTotalCell);

                        var tHt = total.ToString();
                        royalFeeTotalCell = new PdfPCell(new Phrase($"{ _footerPage.TotalHT }", boldFontNineBlack));  //_footerPage.TotalHT //totalHtArrondi.ToString()
                        royalFeeTotalCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        royalFeeTotalCell.HorizontalAlignment = Element.ALIGN_CENTER;
                        royalFeeTotalTable.AddCell(royalFeeTotalCell);

                        montantTva = totalHtArrondi * _footerPage.TxTva / 100;
                        royalFeeTotalCell = new PdfPCell(new Phrase($"{ _footerPage.TotalTVA }", boldFontNineBlack));  //_footerPage.TotalTVA //montantTva
                        royalFeeTotalCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        royalFeeTotalCell.HorizontalAlignment = Element.ALIGN_CENTER;
                        royalFeeTotalTable.AddCell(royalFeeTotalCell);

                        totalTTC = montantTva + totalHtArrondi;
                        royalFeeTotalCell = new PdfPCell(new Phrase($"{ _footerPage.TotalTTC }", boldFontNineBlack));  //_footerPage.TotalTTC  //totalTTC
                        royalFeeTotalCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        royalFeeTotalCell.HorizontalAlignment = Element.ALIGN_CENTER;
                        royalFeeTotalTable.AddCell(royalFeeTotalCell);

                        PdfPCell royalFeeTotalMasterCell = new PdfPCell(royalFeeTotalTable);

                        royalFeeTotalMasterTable.AddCell(royalFeeTotalMasterCell);
                        royalFeeTotalMasterTable.WriteSelectedRows(0, -1, 22, 102, pdfContentByte);
                        #endregion

                        //// -- Add footer to the last page --
                        //OnEndPage(masterWriter, masterDocument, baseFont, _footerPage);

                        masterDocument.Close();
                        masterStream.Close();
                        masterWriter.Close();
                    }
                    #endregion

                    // -- Manage pages number --
                    AddPageNumber(_fullPdfFilePath, _fullPdfFilePath);

                    result = true;

                    _logger.Debug($"==> Fin création du Pdf.");

                    stopwatch.Stop();
                    TimeSpan stopwatchElapsed = stopwatch.Elapsed;
                    Console.WriteLine("Temps mis pour la génération du PDF " + Convert.ToInt32(stopwatchElapsed.TotalMilliseconds));

                    //Process.Start(_fullPdfFilePath);
                }
            }
            #region -- Catch - Finaly Bloc --
            catch (DocumentException docExcexption)
            {
                result = false;
                ErrorMessage = docExcexption.ToString();
                _logger.Debug($" [{ ErrorMessage }]");
            }
            catch (IOException ioException)
            {
                result = false;
                ErrorMessage = ioException.ToString();
                _logger.Debug($" [{ ErrorMessage }]");
            }
            catch (Exception exception)
            {
                result = false;
                ErrorMessage = exception.ToString();
                _logger.Debug($" [{ ErrorMessage }]");
            }
            finally
            {
                List<string> wrongPathList = new List<string>();
                wrongPathList.Clear();

                if (!string.IsNullOrWhiteSpace(ErrorMessage))
                {
                    result = false;
                    StringBuilder stringBuilder = new StringBuilder();

                    if (wrongPathList.Count > 0)
                    {
                        foreach (var item in wrongPathList)
                        {
                            stringBuilder.Append($"{item} " + Environment.NewLine);
                        }
                    }

                    ErrorMessage = stringBuilder.Length > 0 ?
                        string.Format("ErrorMessageLabels.CheckFilesPathMsg", stringBuilder.ToString()) : ErrorMessage;

                    _dialogService.ShowMessage($"[{ErrorMessage}", "ERROR",
                                              MessageBoxButton.OK,
                                              MessageBoxImage.Error,
                                              MessageBoxResult.Yes);
                }
            }
            #endregion
            return result;
        }
        #endregion

        #region MyRegion
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="repositoryPath"></param>
        ///// <param name="headerPage"></param>
        ///// <param name="footerPage"></param>
        ///// <param name="royaltyFeeDataSet"></param>
        ///// <returns>bool</returns>
        //public bool CreateRoyaltyFeePdfFile(string customPdfFileName,
        //    IHeaderPage headerPage,
        //    IFooterPage footerPage,
        //    XDocument xDocument)
        //{
        //    _logger.Debug($"==> Début création du fichier Pdf");

        //    #region -- Declare and Init --
        //    bool result = false;
        //    double total = 0;
        //    double ssTotal = 0;
        //    int countElementByDp = 0;
        //    double totalTTC = 0;
        //    double montantTva = 0;
        //    double totalHtArrondi = 0;
        //    string idPv = null;
        //    string dpName = null;
        //    string commune = null;
        //    string enseigne = null;
        //    string productName = null;
        //    IEnumerable<XElement> _detailXElements = null;

        //    Stopwatch stopwatch = new Stopwatch();
        //    stopwatch.Start();

        //    //PdfWriter masterWriter = null;
        //    DateTime fileCreationDatetime = DateTime.Now;
        //    string imgPath = "https://ftp.mediaperf.com/img/logo.gif";

        //    var widthPercentage = 96;

        //    string extension = ".pdf";
        //    string date = fileCreationDatetime.ToString(@"ddMMyyyy");
        //    string fileName = $"_{date}{extension}";

        //    _logger.Debug($"==> Génération du chemin du fichier");
        //    _fullPdfFilePath = string.Concat(PDF_REPOSITORY_PATH, customPdfFileName, extension);
        //    #endregion

        //    if (File.Exists(_fullPdfFilePath))
        //    {
        //        File.Delete(_fullPdfFilePath);
        //    }

        //    try
        //    {
        //        // -- Define royalTies columns widths --
        //        float[] widths = new float[] { 10f, 60f, 20f, 20f };

        //        if (xDocument != null && xDocument.Descendants("TypeProduit").Elements("Produit").Count() > 0)
        //        {
        //            // -- Retrieve count Pv --
        //            _countPv = xDocument.Descendants("TypeProduit").Elements("Produit").Elements("Dp").Elements("Details").Count();

        //            // -- MMA All Using has changed --
        //            using (var masterStream = new FileStream(_fullPdfFilePath, FileMode.Create))
        //            using (Document masterDocument = new Document(PageSize.A4, 10f, 10f, 340f, 120f))   //(PageSize.A4, 5, 12, 20, 10)  //new Document(PageSize.A4, 10f, 10f, 350f, 150f))  PageSize.A4, 10f, 10f, 368f, 100f)
        //            using (PdfWriter masterWriter = PdfWriter.GetInstance(masterDocument, masterStream))
        //            {
        //                #region -- Setting Document properties e.g. --
        //                masterDocument.AddTitle("Envoi des relevés de redevance par mail");
        //                masterDocument.AddSubject("Génération de relevés de redevance en PDF pour envoi par mail");
        //                masterDocument.AddKeywords("Metadata, iTextSharp 5.4.13.1");
        //                masterDocument.AddCreator("MMA");
        //                masterDocument.AddAuthor("M MABOU");
        //                masterDocument.AddHeader("Some thing", "Header");
        //                #endregion

        //                ErrorMessage = null;

        //                //// -- Setting Encryption properties --
        //                //masterWriter.SetEncryption(PdfWriter.STRENGTH40BITS, "Vers@illes78", "null", PdfWriter.ALLOW_COPY);

        //                //// -- Add Event in all pages --
        //                masterWriter.PageEvent = new ITextEvents(headerPage, footerPage, _countPv);

        //                masterDocument.Open();

        //                PdfContentByte pdfContentByte = masterWriter.DirectContent;
        //                var lineSeparator = new LineSeparator(2.0F, 96.0F, BaseColor.RED, Element.ALIGN_CENTER, 1);

        //                #region -- Define fonts --
        //                BaseFont baseFont = BaseFont.CreateFont(BaseFont.COURIER, BaseFont.CP1252, false);
        //                var noneFontOneWhite = new Font(baseFont, 1, Font.NORMAL, BaseColor.WHITE);
        //                var boldFontEleventBlack = new Font(baseFont, 11, Font.BOLD, BaseColor.BLACK);
        //                var normalFontEleventBlack = new Font(baseFont, 11, Font.NORMAL, BaseColor.BLACK);
        //                var boldFontNineBlack = new Font(baseFont, 9, Font.BOLD, BaseColor.BLACK);
        //                var normalFontNimeBlack = new Font(baseFont, 9, Font.NORMAL, BaseColor.BLACK);
        //                var boldFontTwelveBlack = new Font(baseFont, 12, Font.BOLD, BaseColor.BLACK);
        //                var normalFontTwelveBlack = new Font(baseFont, 12, Font.NORMAL, BaseColor.BLACK);
        //                var boldFontTenBlack = new Font(baseFont, 10, Font.BOLD, BaseColor.BLACK);
        //                var normalFontTenBlack = new Font(baseFont, 10, Font.NORMAL, BaseColor.BLACK);
        //                #endregion

        //                //// -- Draw horizontal line. --
        //                //Paragraph firstLineSeparator = new Paragraph(new Chunk(lineSeparator));
        //                //firstLineSeparator.SpacingBefore = 120f;
        //                //masterDocument.Add(firstLineSeparator);

        //                #region -- Manage XML --
        //                PdfPTable royaltiesTables = new PdfPTable(4);

        //                royaltiesTables.WidthPercentage = 96;

        //                // -- Add Header row for every page --
        //                royaltiesTables.HeaderRows = 1;

        //                // -- Set spacing between gridView and  --
        //                royaltiesTables.SpacingBefore = 5f;

        //                // --   --
        //                royaltiesTables.SetWidths(widths);

        //                #region -- Table Header --
        //                PdfPCell headerCell = new PdfPCell(new Phrase("No", boldFontTwelveBlack));
        //                headerCell.Colspan = 0;
        //                headerCell.MinimumHeight = 20;
        //                headerCell.BackgroundColor = BaseColor.LIGHT_GRAY;
        //                headerCell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //                headerCell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //                royaltiesTables.AddCell(headerCell);

        //                headerCell = new PdfPCell(new Phrase("Campagne", boldFontTwelveBlack));
        //                headerCell.Colspan = 0;
        //                headerCell.BackgroundColor = BaseColor.LIGHT_GRAY;
        //                headerCell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //                headerCell.HorizontalAlignment = 0;
        //                royaltiesTables.AddCell(headerCell);

        //                headerCell = new PdfPCell(new Phrase("Du", boldFontTwelveBlack));
        //                headerCell.Colspan = 0;
        //                headerCell.HorizontalAlignment = 1;
        //                headerCell.BackgroundColor = BaseColor.LIGHT_GRAY;
        //                headerCell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //                royaltiesTables.AddCell(headerCell);

        //                headerCell = new PdfPCell(new Phrase("Au", boldFontTwelveBlack));
        //                headerCell.Colspan = 0;
        //                headerCell.HorizontalAlignment = 1;
        //                headerCell.BackgroundColor = BaseColor.LIGHT_GRAY;
        //                headerCell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //                royaltiesTables.AddCell(headerCell);
        //                #endregion

        //                #region -- Generate pdf group grid --
        //                foreach (XElement typeproduit in xDocument.Descendants(nameof(_royaltyFee.TypeProduit)))
        //                {
        //                    var _produitXElements = typeproduit.Elements(nameof(_royaltyFee.Produit));

        //                    total = 0;

        //                    foreach (XElement element in _produitXElements)
        //                    {
        //                        productName = element.FirstAttribute?.Value;

        //                        var _dpXElements = element.Elements(nameof(_royaltyFee.Dp));

        //                        foreach (XElement childEllement in _dpXElements)
        //                        {
        //                            dpName = childEllement.Attribute("Nom").Value;
        //                            PdfPCell productNameCell = new PdfPCell(new Phrase($"{ productName }\n   { dpName }", boldFontTenBlack));
        //                            productNameCell.Colspan = 4;
        //                            productNameCell.MinimumHeight = 23;
        //                            productNameCell.Padding = 3;
        //                            productNameCell.HorizontalAlignment = 0;
        //                            royaltiesTables.AddCell(productNameCell);

        //                            _detailXElements = childEllement.Elements(nameof(_royaltyFee.Details));
        //                            double sstotal = 0;
        //                            foreach (XElement detailXML in _detailXElements)
        //                            {
        //                                PdfPCell detailCell = new PdfPCell(
        //                                    new Phrase((string)(detailXML.Element(nameof(_royaltyFee.IdCmp))),
        //                                    normalFontTenBlack));
        //                                detailCell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //                                detailCell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //                                royaltiesTables.AddCell(detailCell);

        //                                detailCell = new PdfPCell(
        //                                    new Phrase((string)(detailXML.Element(nameof(_royaltyFee.Campagne))),
        //                                    normalFontTenBlack));
        //                                detailCell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //                                detailCell.HorizontalAlignment = Element.ALIGN_LEFT;
        //                                royaltiesTables.AddCell(detailCell);

        //                                detailCell = new PdfPCell(
        //                                    new Phrase((string)(detailXML.Element(nameof(_royaltyFee.DateDebut))),
        //                                    normalFontTenBlack));
        //                                detailCell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //                                detailCell.HorizontalAlignment = Element.ALIGN_CENTER;
        //                                royaltiesTables.AddCell(detailCell);

        //                                detailCell = new PdfPCell(
        //                                    new Phrase((string)(detailXML.Element(nameof(_royaltyFee.DateFin))),
        //                                    normalFontTenBlack));
        //                                detailCell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //                                detailCell.HorizontalAlignment = Element.ALIGN_CENTER;
        //                                royaltiesTables.AddCell(detailCell);

        //                                enseigne = (string)(detailXML.Element(nameof(_royaltyFee.Enseigne)));
        //                                commune = (string)(detailXML.Element(nameof(_royaltyFee.Commune)));
        //                                idPv = (string)(detailXML.Element(nameof(_royaltyFee.IdPv)));

        //                                sstotal += double.Parse(detailXML.Element(nameof(_royaltyFee.MontantRdvcHT)).Value.Replace(".", ","));
        //                            }

        //                            // -- Set Sub Total Name --
        //                            PdfPCell subTotalCell = new PdfPCell(new Phrase("Sous Total", boldFontTenBlack));
        //                            subTotalCell.Colspan = 2;
        //                            subTotalCell.PaddingRight = 10;
        //                            subTotalCell.PaddingBottom = 5;
        //                            subTotalCell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //                            subTotalCell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //                            royaltiesTables.AddCell(subTotalCell);

        //                            // -- Set Sub Total Value --
        //                            countElementByDp = _detailXElements.Count();
        //                            subTotalCell = new PdfPCell(new Phrase(countElementByDp.ToString(), boldFontTenBlack));
        //                            subTotalCell.HorizontalAlignment = Element.ALIGN_CENTER;
        //                            subTotalCell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //                            royaltiesTables.AddCell(subTotalCell);

        //                            var ssTotalPhrase = new Phrase(sstotal.ToString(), boldFontTenBlack);
        //                            PdfPCell cel = new PdfPCell(ssTotalPhrase);
        //                            cel.HorizontalAlignment = Element.ALIGN_CENTER;
        //                            cel.VerticalAlignment = Element.ALIGN_MIDDLE;
        //                            royaltiesTables.AddCell(cel);

        //                            total += sstotal;
        //                        }
        //                    }

        //                    string productType = typeproduit.Attribute("Nom").Value;

        //                    // -- Set Total Name --
        //                    totalHtArrondi = Math.Round(total, 2);
        //                    Phrase productTypePhrase = new Phrase($"Total  { productType }", boldFontTenBlack);
        //                    PdfPCell productTypeCell = new PdfPCell(productTypePhrase);
        //                    productTypeCell.Colspan = 3;
        //                    productTypeCell.PaddingRight = 10;
        //                    productTypeCell.PaddingBottom = 5;
        //                    productTypeCell.MinimumHeight = 15;
        //                    productTypeCell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //                    productTypeCell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //                    royaltiesTables.AddCell(productTypeCell);

        //                    productTypePhrase = new Phrase(totalHtArrondi.ToString(), boldFontTenBlack);  //totalHtArrondi.ToString()
        //                    productTypeCell = new PdfPCell(productTypePhrase);
        //                    productTypeCell.PaddingRight = 10;
        //                    productTypeCell.PaddingBottom = 5;
        //                    productTypeCell.MinimumHeight = 15;
        //                    productTypeCell.BackgroundColor = BaseColor.LIGHT_GRAY;
        //                    productTypeCell.HorizontalAlignment = Element.ALIGN_CENTER;
        //                    productTypeCell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //                    royaltiesTables.AddCell(productTypeCell);
        //                }

        //                masterDocument.Add(royaltiesTables);

        //                // -- Set spacing between gridView and  --
        //                royaltiesTables.SpacingAfter = 5f;
        //                #endregion
        //                #endregion

        //                #region -- Table Footer --                    
        //                PdfPTable footerTable = new PdfPTable(4);
        //                footerTable.WidthPercentage = widthPercentage;
        //                footerTable.SetWidths(widths);

        //                PdfPCell footerCell = new PdfPCell(new Phrase(idPv, boldFontTenBlack));
        //                footerCell.Colspan = 0;
        //                footerCell.MinimumHeight = 16;
        //                footerCell.PaddingBottom = 5;
        //                footerCell.BackgroundColor = BaseColor.LIGHT_GRAY;
        //                footerCell.HorizontalAlignment = Element.ALIGN_LEFT;
        //                footerCell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //                footerTable.AddCell(footerCell);

        //                footerCell = new PdfPCell(new Phrase($"{ enseigne} - { commune }", boldFontTenBlack));
        //                footerCell.Colspan = 0;
        //                footerCell.PaddingBottom = 5;
        //                footerCell.BackgroundColor = BaseColor.LIGHT_GRAY;
        //                footerCell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //                footerCell.HorizontalAlignment = Element.ALIGN_LEFT;
        //                footerTable.AddCell(footerCell);

        //                footerCell = new PdfPCell(new Phrase($"Total  { _countPv.ToString() }", boldFontTenBlack));
        //                footerCell.Colspan = 0;
        //                footerCell.PaddingBottom = 5;
        //                footerCell.BackgroundColor = BaseColor.LIGHT_GRAY;
        //                footerCell.HorizontalAlignment = Element.ALIGN_CENTER;
        //                footerCell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //                footerTable.AddCell(footerCell);

        //                footerCell = new PdfPCell(new Phrase($"{ _footerPage.TotalHT.ToString() }", boldFontTenBlack));
        //                footerCell.Colspan = 0;
        //                footerCell.PaddingBottom = 5;
        //                footerCell.BackgroundColor = BaseColor.LIGHT_GRAY;
        //                footerCell.HorizontalAlignment = Element.ALIGN_CENTER;
        //                footerCell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //                footerTable.AddCell(footerCell);

        //                masterDocument.Add(footerTable);
        //                #endregion

        //                //// -- Draw interemediate horizontal line. --
        //                //var virtualLineSeparator = new LineSeparator(0.0F, 96.0F, BaseColor.BLUE, Element.ALIGN_CENTER, 1);
        //                //Paragraph interemediateLineSeparator = new Paragraph(new Chunk(virtualLineSeparator));
        //                //interemediateLineSeparator.SpacingBefore = 108f;
        //                //masterDocument.Add(interemediateLineSeparator);

        //                #region -- Total --
        //                PdfPTable royalFeeTotalMasterTable = new PdfPTable(1);
        //                royalFeeTotalMasterTable.TotalWidth = 400;
        //                PdfPTable royalFeeTotalTable = new PdfPTable(4);

        //                PdfPCell firstRoyalFeeTotalCell = new PdfPCell(new Phrase());
        //                firstRoyalFeeTotalCell.BorderColor = BaseColor.WHITE;
        //                firstRoyalFeeTotalCell.Border = 5;
        //                royalFeeTotalTable.AddCell(firstRoyalFeeTotalCell);
        //                PdfPCell royalFeeTotalCell = new PdfPCell(new Phrase("HT", boldFontNineBlack));
        //                royalFeeTotalCell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //                royalFeeTotalCell.HorizontalAlignment = Element.ALIGN_CENTER;
        //                royalFeeTotalTable.AddCell(royalFeeTotalCell);

        //                royalFeeTotalCell = new PdfPCell(new Phrase($"TVA {_footerPage.TxTva}", boldFontNineBlack));
        //                royalFeeTotalCell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //                royalFeeTotalCell.HorizontalAlignment = Element.ALIGN_CENTER;
        //                royalFeeTotalTable.AddCell(royalFeeTotalCell);

        //                royalFeeTotalCell = new PdfPCell(new Phrase("TTC", boldFontNineBlack));
        //                royalFeeTotalCell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //                royalFeeTotalCell.HorizontalAlignment = Element.ALIGN_CENTER;
        //                royalFeeTotalTable.AddCell(royalFeeTotalCell);

        //                royalFeeTotalCell = new PdfPCell(new Phrase("Total du relevé", boldFontNineBlack));
        //                royalFeeTotalCell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //                royalFeeTotalCell.HorizontalAlignment = Element.ALIGN_CENTER;
        //                royalFeeTotalTable.AddCell(royalFeeTotalCell);

        //                royalFeeTotalCell = new PdfPCell(new Phrase($"{ _footerPage.TotalHT }", boldFontNineBlack));  //total.ToString()
        //                royalFeeTotalCell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //                royalFeeTotalCell.HorizontalAlignment = Element.ALIGN_CENTER;
        //                royalFeeTotalTable.AddCell(royalFeeTotalCell);

        //                royalFeeTotalCell = new PdfPCell(new Phrase($"{ _footerPage.TotalTVA }", boldFontNineBlack));  //montantTva.ToString()
        //                royalFeeTotalCell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //                royalFeeTotalCell.HorizontalAlignment = Element.ALIGN_CENTER;
        //                royalFeeTotalTable.AddCell(royalFeeTotalCell);

        //                royalFeeTotalCell = new PdfPCell(new Phrase($"{ _footerPage.TotalTTC }", boldFontNineBlack));   //totalTTC.ToString()
        //                royalFeeTotalCell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //                royalFeeTotalCell.HorizontalAlignment = Element.ALIGN_CENTER;
        //                royalFeeTotalTable.AddCell(royalFeeTotalCell);

        //                PdfPCell royalFeeTotalMasterCell = new PdfPCell(royalFeeTotalTable);

        //                royalFeeTotalMasterTable.AddCell(royalFeeTotalMasterCell);
        //                royalFeeTotalMasterTable.WriteSelectedRows(0, -1, 22, 102, pdfContentByte);
        //                #endregion

        //                //// -- Add footer to the last page --
        //                //OnEndPage(masterWriter, masterDocument, baseFont, _footerPage);

        //                masterDocument.Close();
        //                masterStream.Close();
        //                masterWriter.Close();
        //            }

        //            // -- Manage pages number --
        //            AddPageNumber(_fullPdfFilePath, _fullPdfFilePath);

        //            result = true;

        //            _logger.Debug($"==> Fin création du Pdf.");

        //            stopwatch.Stop();
        //            TimeSpan stopwatchElapsed = stopwatch.Elapsed;
        //            Console.WriteLine("Temps mis pour la génération du PDF " + Convert.ToInt32(stopwatchElapsed.TotalMilliseconds));

        //            //Process.Start(_fullPdfFilePath);
        //        }
        //    }
        //    #region -- Catch - Finaly Bloc --
        //    catch (DocumentException docExcexption)
        //    {
        //        result = false;
        //        ErrorMessage = docExcexption.ToString();
        //        _logger.Debug($" [{ ErrorMessage }]");
        //    }
        //    catch (IOException ioException)
        //    {
        //        result = false;
        //        ErrorMessage = ioException.ToString();
        //        _logger.Debug($" [{ ErrorMessage }]");
        //    }
        //    catch (Exception exception)
        //    {
        //        result = false;
        //        ErrorMessage = exception.ToString();
        //        _logger.Debug($" [{ ErrorMessage }]");
        //    }
        //    finally
        //    {
        //        List<string> wrongPathList = new List<string>();
        //        wrongPathList.Clear();

        //        if (!string.IsNullOrWhiteSpace(ErrorMessage))
        //        {
        //            result = false;
        //            StringBuilder stringBuilder = new StringBuilder();

        //            if (wrongPathList.Count > 0)
        //            {
        //                foreach (var item in wrongPathList)
        //                {
        //                    stringBuilder.Append($"{item} " + Environment.NewLine);
        //                }
        //            }

        //            ErrorMessage = stringBuilder.Length > 0 ?
        //                string.Format("ErrorMessageLabels.CheckFilesPathMsg", stringBuilder.ToString()) : ErrorMessage;

        //            _dialogService.ShowMessage($"[{ErrorMessage}", "ERROR",
        //                                      MessageBoxButton.OK,
        //                                      MessageBoxImage.Error,
        //                                      MessageBoxResult.Yes);
        //        }
        //    }
        //    #endregion
        //    return result;
        //}

        #endregion


        #region -- Generate Pdf Methods --      
        /// <summary>
        /// -- Set page number --
        /// </summary>
        /// <param name="fileIn"></param>
        /// <param name="fileOut"></param>
        private void AddPageNumber(string fileIn, string fileOut)
        {
            byte[] bytes = File.ReadAllBytes(fileIn);
            Font blackFont = FontFactory.GetFont("COURIER", 10, Font.NORMAL, BaseColor.BLACK);
            using (MemoryStream stream = new MemoryStream())
            {
                PdfReader reader = new PdfReader(bytes);
                using (PdfStamper stamper = new PdfStamper(reader, stream))
                {
                    int pages = reader.NumberOfPages;
                    for (int i = 1; i <= pages; i++)
                    {
                        // -- Display page number in all the page header --
                        ColumnText.ShowTextAligned(stamper.GetUnderContent(i), Element.ALIGN_RIGHT,
                                new Phrase(i.ToString() + "/" + pages.ToString(), blackFont), 548f, 780f, 0);

                        // -- Display page number in all the page footer --
                        ColumnText.ShowTextAligned(stamper.GetUnderContent(i), Element.ALIGN_RIGHT,
                            new Phrase(i.ToString(), blackFont), 582f, 12f, 0);
                    }
                }
                bytes = stream.ToArray();
            }
            File.WriteAllBytes(fileOut, bytes);
        }

        /// <summary>
        /// -- Set Footer Plus utiliser depuis la gestion global du HeaderFooter mais peu servir --
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="document"></param>
        /// <param name="baseFont"></param>
        public static void OnEndPage(PdfWriter writer, Document document, BaseFont baseFont, IFooterPage footerPage)
        {
            PdfPTable endPageTable = new PdfPTable(1);
            
            endPageTable.TotalWidth = 70;
            endPageTable.TotalWidth = document.PageSize.Width - document.LeftMargin - document.RightMargin;
            PdfPTable table2 = new PdfPTable(1);

            string footer = footerPage.BfpParam6;
            string[] footerTextArray = Regex.Split(footer, "\r\n");

            PdfPCell cell2 = null;
            for (int i = 0; i < footerTextArray.Length; i++)
            {
                cell2 = new PdfPCell(new Phrase(footerTextArray[i], new Font(baseFont, 8, Font.NORMAL, BaseColor.BLACK)));
                cell2.Border = Rectangle.NO_BORDER;
                table2.AddCell(cell2);
            }

            table2.DefaultCell.Border = Rectangle.NO_BORDER;
            PdfPCell cell = new PdfPCell(table2);
            cell.BorderColor = BaseColor.WHITE;
            endPageTable.AddCell(cell);
            endPageTable.WriteSelectedRows(0, -1, 20, 70, writer.DirectContent);
        }
        #endregion

        #region MyRegion

        /// <summary>
        /// -- Lecture de la base de données ou du fichier à la recherche !!!!!!!!!!!!!!!!! --
        /// </summary>
        /// <returns></returns>
        //public async Task<bool> ManagefpPPV()
        public bool ManagefpPPV()
        {
            bool result = false;

            string currentTime = DateTime.Now.ToString("dd-MM-yyyy");
            string backupDirectoryReports = @"O:\USI\USI_Recette\ReportRedevanceForSendingByMail";
            FileInfo[] allTextFiles = null;
            int counter = 0;

            #region -- Recovery of all royalties to be generated in pdf then send by email --
            try
            {
                #region -- Gestion par la base de données --
                var bfpReportHistoricDataSet = GetBfpReportHistoricSync("SP_PvRedevanceHistorique_Select");

                if ((bfpReportHistoricDataSet != null) &&
                    (bfpReportHistoricDataSet.Tables.Count > 0) &&
                    (bfpReportHistoricDataSet.Tables[0].Rows.Count > 0))
                {
                    #region -- With Multi threading --
                    #region --- OK OK OK OK ---
                    Thread letgoThread = new Thread(Do);

                    // Commencer Thread (start thread).
                    letgoThread.Start();

                    // Dites au thread principal (voici main principal)
                    // Attendez que letgoThread finisse, puis continuez à fonctionner.
                    letgoThread.Join();
                    #endregion


                    //int workerThreadCount;
                    //int ioThreadCount;
                    //ThreadPool.GetMaxThreads(out workerThreadCount, out ioThreadCount);
                    //Console.WriteLine("Current max worker thread: " + workerThreadCount.ToString());

                    //Parallel.ForEach(_bfpReportHistoricsEnumerable, (action) =>
                    //{
                    //    lock (_lockObj)
                    //    {
                    //        result = Process();
                    //    }
                    //});

                    //result = Process();

                    //Task currentThread = new Task(() =>
                    //{
                    //    Parallel.ForEach(_bfpReportHistoricsEnumerable, (action) =>
                    //    {
                    //        result = Process();

                    //        lock (_lockObj)
                    //        {
                    //            result = Process();
                    //        }
                    //    });
                    //});

                    //currentThread.Start();
                    //currentThread.Wait();

                    // --------------------------------------------------------------------
                    //Stopwatch stopwatch0 = new Stopwatch();
                    //stopwatch0.Start();
                    //Task currentThread = new Task(() =>
                    //{
                    //    lock (_lockObj)
                    //    {
                    //        result = Process();

                    //        Console.WriteLine("\r\n Processing {0} on thread {1}", "TaxInvoiceNumber",
                    //                    Thread.CurrentThread.ManagedThreadId);
                    //    }
                    //});

                    //currentThread.Start();
                    //currentThread.Wait();

                    #region -- To TEST --
                    /*
                    List<bool> results = new List<bool>(_bfpReportHistoricDataSet.Tables[0].Rows.Count);
                    Parallel.ForEach(_bfpReportHistoricsEnumerable, t =>
                    {
                        result = Process();
                        lock (results)
                        { // lock the list to avoid race conditions
                            results.Add(result);
                        }
                    });

                    */

                    //Parallel.For(0, _bfpReportHistoricDataSet.Tables[0].Rows.Count, i =>
                    //{

                    //});

                    //// -- To TEST --
                    //var tasks = _bfpReportHistoricsEnumerable.Select(url => Task.Factory.StartNew(() => Process()));
                    #endregion
                    #endregion

                    #region -- Without multithreading --
                    /*
                     bool result = false; 
                    
                    for (int i = 0; i < bfpReportHistoricDataSet.Tables[0].Rows.Count; i++)
                    {
                        var currentDataSet = bfpReportHistoricDataSet.Tables[0].Rows[i];

                        // -- Retrieve BFP royalties historic from database --
                        _bfpReportHistoric = _consolidateHelper.ConsolidateBfpReportHistoric(currentDataSet, _bfpReportHistoric);

                        // -- Retrieve PDF template storeProceture --
                        string spBfpRedevanceTemplate = $"{_bfpReportHistoric.SpReportBFPPdfTemplate} {_bfpReportHistoric.Fk_Bfp}, {_bfpReportHistoric.Fk_S_StTarif}";
                        string spReportBfpRedevance = $"{_bfpReportHistoric.SpReportBFPRdvcPv} {_bfpReportHistoric.Fk_Bfp}";
                        string spBfpContactRoyalty = $"SP_ContactsRecevantRdvc_SelectFromPv  15899"; // {_bfpReportHistoric.IdPv}"; //"15899 @e_Fk_Pv  --  387  3287  1639 --  442  -- ok 16088 ok";

                        // -- Retrieve the Pdf template by BfpPvId and typeTarif from database --
                        GetPdfDataTemplateSync(spBfpRedevanceTemplate);
                                                
                        #region -- Manage Pdf Header page -- 
                        if ((_headerDataSetTemple != null) && (_headerDataSetTemple.Tables.Count > 0) &&
                            (_headerDataSetTemple.Tables[0].Rows.Count > 0))
                        {
                            _headerPage = _consolidateHelper.ConsolidateHeader(_headerDataSetTemple,
                                            _adressTemple,
                                            _bfpReportHistoric.Fk_S_ModeleEditionBfp,
                                            _bfpReportHistoric.Fk_S_StTarif,
                                            _headerPage);
                        }
                        #endregion

                        #region -- Manage Pdf footer page --
                        if ((_footerDataSetTemple != null) && (_footerDataSetTemple.Tables.Count > 0) &&
                            (_footerDataSetTemple.Tables[0].Rows.Count != 0))
                        {
                            // --  Loading the pdf footer data  --
                            _footerPage = _consolidateHelper.ConsolidateFooter(_footerDataSetTemple, _footerPage);
                        }
                        #endregion

                        #region -- Manage RoyaltyFee Pdf body page --
                        // -- Loading the pdf table data  --
                        XDocument xDocument = GetXMLReportBFPRedevance(_bfpReportHistoric.Fk_Bfp);

                        if (xDocument != null)
                        {
                            string dtSessionRfr = Regex.Split(_bfpReportHistoric.DtSessionRfr.Replace('/', '-'), " ")[0];

                            string customFileName = $"{_bfpReportHistoric.IdPv}_Relevé de redevance Médiaperformances au {dtSessionRfr}";

                            bool createPdfResult = CreateRoyaltyFeePdfFile(customFileName, _headerPage, _footerPage, xDocument);

                            #region -- OK OK OK OK OK --  
                            //createPdfResult = false;
                            if (createPdfResult)
                            {
                                #region -- Retrieve contacts redevance PV from PV and Send by mail --       _bfpReportHistoric.IdPv

                                // -- Retrieve all associated contacts --
                                DataSet contactDataSet = GetContactReceivingTheFeeSync(spBfpContactRoyalty);

                                // -- Check if "IsRdvcMailPv" and Get the Fk_Crm --  SP_Pv_RdvcTypeAndCrmId_SelectByPvId  idPv   15899                        
                                DataSet contactPvInfoDataSet = ManageIfRedvanceMailPvChecked("SP_Pv_RdvcTypeAndCrmId_SelectByPvId", _bfpReportHistoric.IdPv);  //10

                                if (contactDataSet != null && contactDataSet.Tables[0].Rows.Count > 0 ||
                                    contactPvInfoDataSet != null && contactPvInfoDataSet.Tables[0].Rows.Count > 0)
                                {
                                    string mailPvAdress = string.Empty;

                                    #region --  --
                                    var contactPv = _consolidateHelper.ConsolidateContactPvInfo(contactPvInfoDataSet, _contactRoyaltyFee);

                                    // -- If mailPv's added to recieving BFP repport contact --
                                    if (contactPv.IsRdvcMailPv)
                                    {
                                        // -- Retrieve the mail Pv --   // SP_Vue_Ext_Comptes_SelectFromId   ==> SP_RdvcGetMailPv_SelectByFk_Crm anIdCrm  9981
                                        mailPvAdress = RetrieveMailPvByFkCrm("SP_RdvcGetMailPv_SelectByFk_Crm", contactPv.Fk_Crm);
                                    }
                                    #endregion

                                    var contactDictionnary = _consolidateHelper.ConsolidateRoyaltyFeeContact(contactDataSet, _contactRoyaltyFee);

                                    var mailTemplateDataSet = GetMailTemplateASync("SP_RedevanceBFPMailTemplate_Select", "Redevance par Mail");

                                    var mailTemplate = _consolidateHelper.BuildMailTemplate(mailTemplateDataSet, _mailTemplate);

                                    if (contactDictionnary != null && contactDictionnary.Count() > 0)
                                    {
                                        if (!string.IsNullOrEmpty(mailPvAdress))
                                        {
                                            contactDictionnary.Add("Mail Pv", mailPvAdress);
                                        }

#if DEBUG
                                        contactDictionnary.Add("Yahoo", "michaelmabou@yahoo.fr");
                                        contactDictionnary.Add("Gmail", "citoyenlamda@gmail.com");
                                        //contactDictionnary.Add("Laurent", "lmaduraud@mediaperf.com");
                                        //contactDictionnary.Add("David", "dzaguedoun@mediaperf.com");
                                        //contactDictionnary.Add("Lucie", "LDUTHEIL@mediaperf.com");

                                        _emailMessageService.IsPreviewMail = true;
                                        _emailMessageService.AdminEmail = $"{Environment.UserName}@mediaperf.com";
#else
                                    //_emailMessageService.Bcc = contactBuilder.ToString();
                                    //_emailMessageService.Cc = "michaelmabou@yahoo.fr";
                                    //_emailMessageService.ToEmail = contactBuilder.ToString();
#endif
                                        _emailMessageService.SenderEmail = "Redevance_Mediaperformance@mediaperf.com";
                                        _emailMessageService.FilePath = _fullPdfFilePath;
                                        _emailMessageService.MailBody = mailTemplate.Texte; //MAIL_TEMPLATE_PATH;
                                        _emailMessageService.Suject = mailTemplate.Objet;

                                        bool sendMailResult = SendEmailHelper.SendEmail(_emailMessageService, contactDictionnary, _bfpReportHistoric.DtSessionRfr);
                                        //sendMailResult = false;
                                        if (sendMailResult)
                                        {
                                            bool updapteResult = UpdateBFPRoyaltiesHistoric(_bfpReportHistoric.Id);

                                            if (updapteResult)
                                            {
                                                // -- Change BFP status to Lancé --
                                                bool updateResult = UpdateBFPChangeStatusEnvoiMail("SP_BFP_Set_SttRdvcMail_Update", _headerPage.IdBFP);
                                                //updapteResult = false;
                                                if (updateResult)
                                                {
                                                    // -- Delete pdf file after sending by mail --   @"C:\Users\mMABOU\Desktop\PDFFiles";  //backupDirectoryReports +"\\"
                                                    // string.Concat(PDF_REPOSITORY_PATH, customPdfFileName, extension);
                                                    string fileTodelete = string.Concat(PDF_REPOSITORY_PATH, customFileName, ".pdf");
                                                    //File.SetAttributes(fileTodelete, FileAttributes.Normal);
                                                    //File.Delete(fileTodelete);

                                                    result = true;
                                                    _logger.Debug($"Fin génération, envoi envoi, MAJ de l'historique et du PV en fin Suppression du fichier PDF. [{ _bfpReportHistoric.IdPv }]");
                                                }
                                                else
                                                {
                                                    result = false;
                                                    ErrorMessage = $"Problème survenu lors de la MAJ du statut. [{ _bfpReportHistoric.IdPv }]";
                                                    _logger.Debug(ErrorMessage);
                                                }
                                            }
                                            else
                                            {
                                                ErrorMessage = $"Problème survenu lors de la mise à jour du BFP [{ _headerPage.IdBFP }]";
                                                _logger.Debug(ErrorMessage);
                                                result = false;
                                            }
                                        }
                                        else
                                        {
                                            ErrorMessage = $"Problème survenu lors de l'envoie par mail du BFP [{ _headerPage.IdBFP }]";
                                            _logger.Debug(ErrorMessage);
                                            result = false;
                                        }
                                    } // Pas de contact associé à cette redevance.
                                    #endregion
                                }
                                else
                                {
                                    ErrorMessage = $"Impossible d'envoyer ce mail car il n'y a aucun contact associé";
                                    _logger.Debug(ErrorMessage);
                                    result = false;
                                }
                            }
                            else
                            {
                                //ErrorMessage = $"Erreur lors de la création du PDF pour le BFP [{ _headerPage.IdBFP }]";
                                _logger.Debug(ErrorMessage);
                                result = false;
                            }
                            #endregion
                        }
                        #endregion
                    }
                    */
                    #endregion
                }
                else
                {
                    ErrorMessage = $"Aucune redevace non envoyée n'a été trouvée.";
                    _logger.Debug($" [{ ErrorMessage }]");
                }
                #endregion

                #region -- Gestion par récûpération du fichier de fichier TEXT --
                /*
                DirectoryInfo d = new DirectoryInfo(backupDirectoryReports);
                allTextFiles = d.GetFiles("*.txt");
                string str = "";
                foreach (FileInfo file in allTextFiles)
                {
                    counter++;
                    string fileName = Path.GetFileNameWithoutExtension(file.Name);

                    string[] elements = Regex.Split(fileName, "_");
                    string fileDate = elements[1].ToString();

                    if (elements.Count() > 1 && (fileDate == currentTime))
                    {
                        string completeFileName = string.Concat(@"\", file.Name);
                        string searchedFile = string.Concat(backupDirectoryReports, completeFileName);
                        string[] lines = File.ReadAllLines(searchedFile);

                        // -- Retrieve the file contents --
                        foreach (string line in lines)
                        {
                            string[] data = Regex.Split(line, "  ");

                            long anIdBFP = Convert.ToInt64(data[0].ToString());
                            long aStTarif = Convert.ToInt64(data[1].ToString());
                            long aModeleEdition = Convert.ToInt64(data[2].ToString());

                            string spBfpRedevanceTemplate = $"{data[5]} {data[0]}, {data[1]}";
                            string spReportBfpRedevance = $"{data[6]} {data[0]}";

                            string idPv = $"{data[4]}";

                            // -- Retrieve the Pdf template by BfpPvId and typeTarif from database --
                            await GetPdfDataTemplateSync(spBfpRedevanceTemplate);

                            // -- Retrieve the BfpRedevance by BfpPvId from database --
                            await GetReportBFPRedevanceSync(spReportBfpRedevance);
                            string imgPath = "https://ftp.mediaperf.com/img/logo.gif";

                            // --  --
                            string idBfp = null;

                            #region -- Manage Pdf Header page -- 
                            if ((_headerDataSetTemple != null) && (_headerDataSetTemple.Tables.Count > 0) &&
                                (_headerDataSetTemple.Tables[0].Rows.Count > 0))
                            {
                                _headerPage = _consolidateHelper.ConsolidateHeader(_headerDataSetTemple,
                                                _adressTemple,
                                                aModeleEdition,
                                                aStTarif,
                                                _headerPage);
                            }
                            #endregion

                            #region -- Manage Pdf footer page --
                            if ((_footerDataSetTemple != null) && (_footerDataSetTemple.Tables.Count > 0) &&
                                (_footerDataSetTemple.Tables[0].Rows.Count != 0))
                            {
                                // --  Loading the pdf footer data  --
                                _footerPage = _consolidateHelper.ConsolidateFooter(_footerDataSetTemple, _footerPage);
                            }
                            #endregion

                            #region -- Manage RoyaltyFee Pdf body page --
                            if ((_royaltyFeeDataSetTemplate != null) && (_royaltyFeeDataSetTemplate.Tables.Count > 0) &&
                                (_royaltyFeeDataSetTemplate.Tables[0].Rows.Count != 0))
                            {
                                // --  Loading the pdf table data  --
                                _royaltyFee = _consolidateHelper.ConsolidateRoyaltyFee(_royaltyFeeDataSetTemplate, _royaltyFee, _headerPage);
                            }
                            #endregion
                            ;
                            string customFileName = @"\" + anIdBFP;

                            bool createPdfResult = await CreateRoyaltyFeePdfFile(customFileName, _headerPage, _footerPage, _royaltyFeeDataSetTemplate);

                            if (createPdfResult)
                            {

                            }
                            else
                            {
                                ErrorMessage = $"Erreur lors de la création du PDF pour la ligne [{ idBfp }]";
                            }

                            //// -- Retrieve the report royalty fee by BfpPvId from database --
                            //Task rdvc = GetReportBFPRedevanceSync(spReportBfpRedevance);

                            //GetTemplateDataSync(spBfpRedevanceTemplate, null, null);

                            Console.WriteLine("\t" + line);
                        }

                        result = true;
                    }
                }
                */
                #endregion
                //return result;
            }
            #region -- Catch Finaly Blocs --
            catch (ArgumentException argumentException)
            {
                ErrorMessage = argumentException.ToString();
                _logger.Debug($" [{ ErrorMessage }]");
            }
            catch (DirectoryNotFoundException directoryNotFoundException)
            {
                ErrorMessage = directoryNotFoundException.ToString();
                _logger.Debug($" [{ ErrorMessage }]");
            }
            catch (IOException iOException)
            {
                ErrorMessage = iOException.Message.ToString();
                _logger.Debug($" [{ ErrorMessage }]");
            }
            catch (Exception exception)
            {
                ErrorMessage = exception.Message.ToString();

                result = false;
                exception.Data.Add("user", Thread.CurrentPrincipal.Identity.Name);
                Console.WriteLine(exception.ToString());

                var supportId = Guid.NewGuid();
                exception.Data.Add("Support id", supportId);
                //_logger.Error(e);
                throw;
            }
            finally
            {
                List<string> wrongPathList = new List<string>();
                wrongPathList.Clear();

                //if (allTextFiles.Length - counter == 0 && result == false)
                //{
                //    ErrorMessage = "Aucun fichier ne correspond à votre recherche.";
                //}

                if (!string.IsNullOrWhiteSpace(ErrorMessage))
                {
                    result = false;
                    StringBuilder messageBuilder = new StringBuilder();

                    if (wrongPathList.Count > 0)
                    {
                        foreach (var item in wrongPathList)
                        {
                            messageBuilder.Append($"{item} " + Environment.NewLine);
                        }
                    }

                    ErrorMessage = messageBuilder.Length > 0 ?
                        string.Format("ErrorMessageLabels.CheckFilesPathMsg\r\n", messageBuilder.ToString()) : ErrorMessage;

                    _dialogService.ShowMessage($"{ErrorMessage}", "ERROR",
                                              MessageBoxButton.OK,
                                              MessageBoxImage.Error,
                                              MessageBoxResult.Yes);
                }
            }

            return result;
            #endregion
            #endregion
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        private void Do()
        {
            ProcessV0();
        }

        /// <summary>
        /// --   --
        /// </summary>
        /// <param name="bfpReportHistoricDataSet"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private bool ProcessV0()
        {
            bool result = false;

            try
            {
                #region -----------------------
                _bfpReportHistoricDataSet = DataTableHelper.ConvertToDataSet<BfpReportHistoric>(_bfpReportHistoricsEnumerable.ToList());

                Parallel.ForEach(_bfpReportHistoricsEnumerable, t =>
                //Parallel.For(0, _bfpReportHistoricDataSet.Tables[0].Rows.Count, t =>
                {
                    for (int i = 0; i < _bfpReportHistoricDataSet.Tables[0].Rows.Count; i++)
                    {
                        var currentDataSet = _bfpReportHistoricDataSet.Tables[0].Rows[i];

                        // -- Retrieve BFP royalties historic from database --
                        _bfpReportHistoric = _consolidateHelper.ConsolidateBfpReportHistoric(currentDataSet, _bfpReportHistoric);

                        // -- Retrieve PDF template storeProceture --
                        string spBfpRedevanceTemplate = $"{_bfpReportHistoric.SpReportBFPPdfTemplate} {_bfpReportHistoric.Fk_Bfp}, {_bfpReportHistoric.Fk_S_StTarif}";
                        string spReportBfpRedevance = $"{_bfpReportHistoric.SpReportBFPRdvcPv} {_bfpReportHistoric.Fk_Bfp}";
                        string spBfpContactRoyalty = $"SP_ContactsRecevantRdvc_SelectFromPv  15899"; // {_bfpReportHistoric.IdPv}"; //"15899 @e_Fk_Pv  --  387  3287  1639 --  442  -- ok 16088 ok";

                        // -- Retrieve the Pdf template by BfpPvId and typeTarif from database --
                        GetPdfDataTemplateSync(spBfpRedevanceTemplate);

                        // --  --
                        string idBfp = null;

                        #region -- Manage Pdf Header page -- 
                        if ((_headerDataSetTemple != null) && (_headerDataSetTemple.Tables.Count > 0) &&
                            (_headerDataSetTemple.Tables[0].Rows.Count > 0))
                        {
                            _headerPage = _consolidateHelper.ConsolidateHeader(_headerDataSetTemple,
                                            _adressTemple,
                                            _bfpReportHistoric.Fk_S_ModeleEditionBfp,
                                            _bfpReportHistoric.Fk_S_StTarif,
                                            _headerPage);
                        }
                        #endregion

                        #region -- Manage Pdf footer page --
                        if ((_footerDataSetTemple != null) && (_footerDataSetTemple.Tables.Count > 0) &&
                            (_footerDataSetTemple.Tables[0].Rows.Count != 0))
                        {
                            // --  Loading the pdf footer data  --
                            _footerPage = _consolidateHelper.ConsolidateFooter(_footerDataSetTemple, _footerPage);
                        }
                        #endregion

                        #region -- Manage RoyaltyFee Pdf body page --
                        // -- Loading the pdf table data  --
                        XDocument xDocument = GetXMLReportBFPRedevance(_bfpReportHistoric.Fk_Bfp);

                        if (xDocument != null)
                        {
                            string dtSessionRfr = Regex.Split(_bfpReportHistoric.DtSessionRfr.Replace('/', '-'), " ")[0];

                            string customFileName = $"{_bfpReportHistoric.IdPv}_Relevé de redevance Médiaperformances au {dtSessionRfr}";

                            bool createPdfResult = CreateRoyaltyFeePdfFile(customFileName, _headerPage, _footerPage, xDocument);

                            #region -- OK OK OK OK OK --  
                            //createPdfResult = false;
                            if (createPdfResult)
                            {
                                #region -- Retrieve contacts redevance PV from PV and Send by mail --       _bfpReportHistoric.IdPv

                                // -- Retrieve all associated contacts --
                                DataSet contactDataSet = GetContactReceivingTheFeeSync(spBfpContactRoyalty);

                                // -- Check if "IsRdvcMailPv" and Get the Fk_Crm --  SP_Pv_RdvcTypeAndCrmId_SelectByPvId  idPv   15899                        
                                DataSet contactPvInfoDataSet = ManageIfRedvanceMailPvChecked("SP_Pv_RdvcTypeAndCrmId_SelectByPvId", _bfpReportHistoric.IdPv);  //10

                                if (contactDataSet != null && contactDataSet.Tables[0].Rows.Count > 0 ||
                                    contactPvInfoDataSet != null && contactPvInfoDataSet.Tables[0].Rows.Count > 0)
                                {
                                    string mailPvAdress = string.Empty;

                                    #region --  --
                                    var contactPv = _consolidateHelper.ConsolidateContactPvInfo(contactPvInfoDataSet, _contactRoyaltyFee);

                                    // -- If mailPv's added to recieving BFP repport contact --
                                    if (contactPv.IsRdvcMailPv)
                                    {
                                        // -- Retrieve the mail Pv --   // SP_Vue_Ext_Comptes_SelectFromId   ==> SP_RdvcGetMailPv_SelectByFk_Crm anIdCrm  9981
                                        mailPvAdress = RetrieveMailPvByFkCrm("SP_RdvcGetMailPv_SelectByFk_Crm", contactPv.Fk_Crm);
                                    }
                                    #endregion

                                    var contactDictionnary = _consolidateHelper.ConsolidateRoyaltyFeeContact(contactDataSet, _contactRoyaltyFee);

                                    var mailTemplateDataSet = GetMailTemplateASync("SP_RedevanceBFPMailTemplate_Select", "Redevance par Mail");

                                    var mailTemplate = _consolidateHelper.BuildMailTemplate(mailTemplateDataSet, _mailTemplate);

                                    if (contactDictionnary != null && contactDictionnary.Count() > 0)
                                    {
                                        if (!string.IsNullOrEmpty(mailPvAdress))
                                        {
                                            contactDictionnary.Add("Mail Pv", mailPvAdress);
                                        }

#if DEBUG
                                        contactDictionnary.Add("Yahoo", "michaelmabou@yahoo.fr");
                                        contactDictionnary.Add("Gmail", "citoyenlamda@gmail.com");
                                        //contactDictionnary.Add("Laurent", "lmaduraud@mediaperf.com");
                                        //contactDictionnary.Add("David", "dzaguedoun@mediaperf.com");
                                        //contactDictionnary.Add("Lucie", "LDUTHEIL@mediaperf.com");

                                        _emailMessageService.IsPreviewMail = true;
                                        _emailMessageService.AdminEmail = $"{Environment.UserName}@mediaperf.com";
#else
                                    //_emailMessageService.Bcc = contactBuilder.ToString();
                                    //_emailMessageService.Cc = "michaelmabou@yahoo.fr";
                                    //_emailMessageService.ToEmail = contactBuilder.ToString();
#endif
                                        _emailMessageService.SenderEmail = "Redevance_Mediaperformance@mediaperf.com";
                                        _emailMessageService.FilePath = _fullPdfFilePath;
                                        _emailMessageService.MailBody = mailTemplate.Texte; //MAIL_TEMPLATE_PATH;
                                        _emailMessageService.Suject = mailTemplate.Objet;

                                        bool sendMailResult = SendEmailHelper.SendEmail(_emailMessageService, contactDictionnary, _bfpReportHistoric.DtSessionRfr);
                                        //sendMailResult = false;
                                        if (sendMailResult)
                                        {
                                            bool updapteResult = UpdateBFPRoyaltiesHistoric(_bfpReportHistoric.Id);
                                            updapteResult = false;
                                            if (updapteResult)
                                            {
                                                // -- Change BFP status to Lancé --
                                                bool updateResult = UpdateBFPChangeStatusEnvoiMail("SP_BFP_Set_SttRdvcMail_Update", _headerPage.IdBFP);
                                                //updapteResult = false;
                                                if (updateResult)
                                                {
                                                    // -- Delete pdf file after sending by mail --   @"C:\Users\mMABOU\Desktop\PDFFiles";  //backupDirectoryReports +"\\"
                                                    // string.Concat(PDF_REPOSITORY_PATH, customPdfFileName, extension);
                                                    string fileTodelete = string.Concat(PDF_REPOSITORY_PATH, customFileName, ".pdf");
                                                    //File.SetAttributes(fileTodelete, FileAttributes.Normal);
                                                    //File.Delete(fileTodelete);

                                                    result = true;
                                                    _logger.Debug($"Fin génération, envoi envoi, MAJ de l'historique et du PV en fin Suppression du fichier PDF. [{ _bfpReportHistoric.IdPv }]");
                                                }
                                                else
                                                {
                                                    result = false;
                                                    ErrorMessage = $"Problème survenu lors de la MAJ du statut. [{ _bfpReportHistoric.IdPv }]";
                                                    _logger.Debug(ErrorMessage);
                                                }
                                            }
                                            else
                                            {
                                                ErrorMessage = $"Problème survenu lors de la mise à jour du BFP [{ _headerPage.IdBFP }]";
                                                _logger.Debug(ErrorMessage);
                                                result = false;
                                            }
                                        }
                                        else
                                        {
                                            ErrorMessage = $"Problème survenu lors de l'envoie par mail du BFP [{ _headerPage.IdBFP }]";
                                            _logger.Debug(ErrorMessage);
                                            result = false;
                                        }
                                    } // Pas de contact associé à cette redevance.
                                    #endregion
                                }
                                else
                                {
                                    ErrorMessage = $"Impossible d'envoyer ce mail car il n'y a aucun contact associé";
                                    _logger.Debug(ErrorMessage);
                                    result = false;
                                }
                            }
                            else
                            {
                                //ErrorMessage = $"Erreur lors de la création du PDF pour le BFP [{ _headerPage.IdBFP }]";
                                _logger.Debug(ErrorMessage);
                                result = false;
                            }
                            #endregion
                        }
                        #endregion
                    }

                });
                #endregion
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
                throw;
            }

            return result;
        }

        #region -- *********************************************************************** --

        #region -------------------------------------------
        public bool CreateRoyaltyFeePdfFile(
             XDocument xDocument)
        {
            //_logger.Debug($"==> Début création du fichier Pdf");

            #region MyRegion

            string ErrorMessage = null;
            int _countPv = 0;
            #endregion


            #region -- Fields --
            bool result = false;

            double total = 0;
            double totalHT = 0;
            int countElementByDp = 0;
            int countElementByProduct = 0;
            double totalTTC = 0;
            double montantTva = 0;
            double totalHtArrondi = 0;
            string idPv = null;
            string dpName = null;
            string communeName = null;
            string enseigneName = null;
            string productName = null;
            IEnumerable<XElement> _detailXElements = null;
            #endregion

            #region --   --
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            //PdfWriter masterWriter = null;
            DateTime fileCreationDatetime = DateTime.Now;

            var widthPercentage = 96;

            string extension = ".pdf";
            string date = fileCreationDatetime.ToString(@"ddMMyyyy");
            string fileName = $"_{date}{extension}";

            string pdfPath = @"C:\Users\Sweet Family\Desktop\PdfFilesPath\TestPDF";


            //_logger.Debug($"==> Génération du chemin du fichier");
            string _fullPdfFilePath = string.Concat(pdfPath, "PDF_TEST", extension);

            if (File.Exists(_fullPdfFilePath))
            {
                File.Delete(_fullPdfFilePath);
            }
            #endregion

            try
            {
                // -- Define royalTies columns widths --
                float[] widths = new float[] { 10f, 60f, 20f, 20f };

                string xmlPath = @"C:\Users\Sweet Family\source\repos\MediaPerf.ManagerBfpPdfReport\MediaPerf.ManagerPdf.Repository\Product.xml";

                xDocument = XDocument.Load(xmlPath);

                if (xDocument != null && xDocument.Descendants("TypeProduit").Elements("Produit").Count() > 0)
                {
                    // -- Retrieve count Pv --
                    _countPv = xDocument.Descendants("TypeProduit").Elements("Produit").Elements("Dp").Elements("Details").Count();


                    var de = xDocument.Descendants("TypeProduit").Elements("Produit").Count();

                    // -- MMA All Using has changed --
                    using (var masterStream = new FileStream(_fullPdfFilePath, FileMode.Create))
                    using (Document masterDocument = new Document(PageSize.A4, 10f, 10f, 340f, 120f))   //(PageSize.A4, 5, 12, 20, 10)  //new Document(PageSize.A4, 10f, 10f, 350f, 150f))  PageSize.A4, 10f, 10f, 368f, 100f)
                    using (PdfWriter masterWriter = PdfWriter.GetInstance(masterDocument, masterStream))
                    {
                        #region -- Setting Document properties e.g. --
                        masterDocument.AddTitle("Envoi des relevés de redevance par mail");
                        masterDocument.AddSubject("Génération de relevés de redevance en PDF pour envoi par mail");
                        masterDocument.AddKeywords("Metadata, iTextSharp 5.4.13.1");
                        masterDocument.AddCreator("MMA");
                        masterDocument.AddAuthor("M MABOU");
                        masterDocument.AddHeader("Some thing", "Header");
                        #endregion

                        ErrorMessage = null;

                        //// -- Add Event in all pages --
                        //masterWriter.PageEvent = new ITextEvents(_headerPage, _footerPage, _countPv);

                        masterDocument.Open();

                        PdfContentByte pdfContentByte = masterWriter.DirectContent;
                        var lineSeparator = new LineSeparator(2.0F, 96.0F, BaseColor.RED, Element.ALIGN_CENTER, 1);

                        #region -- Define fonts --
                        BaseFont baseFont = BaseFont.CreateFont(BaseFont.COURIER, BaseFont.CP1252, false);
                        var noneFontOneWhite = new Font(baseFont, 1, Font.NORMAL, BaseColor.WHITE);
                        var normalFontEightBlack = new Font(baseFont, 8, Font.NORMAL, BaseColor.BLACK);
                        var boldFontEightBlack = new Font(baseFont, 8, Font.BOLD, BaseColor.BLACK);
                        var boldFontNineBlack = new Font(baseFont, 9, Font.BOLD, BaseColor.BLACK);
                        var normalFontNimeBlack = new Font(baseFont, 9, Font.NORMAL, BaseColor.BLACK);
                        var boldFontTenBlack = new Font(baseFont, 10, Font.BOLD, BaseColor.BLACK);
                        var normalFontTenBlack = new Font(baseFont, 10, Font.NORMAL, BaseColor.BLACK);
                        var boldFontEleventBlack = new Font(baseFont, 11, Font.BOLD, BaseColor.BLACK);
                        var normalFontEleventBlack = new Font(baseFont, 11, Font.NORMAL, BaseColor.BLACK);
                        var boldFontTwelveBlack = new Font(baseFont, 12, Font.BOLD, BaseColor.BLACK);
                        var normalFontTwelveBlack = new Font(baseFont, 12, Font.NORMAL, BaseColor.BLACK);
                        #endregion

                        #region -- Generate XML Grid --
                        PdfPTable royaltiesTables = new PdfPTable(4);

                        royaltiesTables.WidthPercentage = 96;

                        // -- Add Header row for every page --
                        royaltiesTables.HeaderRows = 1;

                        // -- Set spacing between gridView and  --
                        royaltiesTables.SpacingBefore = 5f;

                        // --   --
                        royaltiesTables.SetWidths(widths);

                        #region -- Table Header --
                        PdfPCell headerCell = new PdfPCell(new Phrase("No", boldFontTenBlack));
                        headerCell.Colspan = 0;
                        headerCell.MinimumHeight = 18;
                        headerCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                        headerCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                        headerCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        royaltiesTables.AddCell(headerCell);

                        headerCell = new PdfPCell(new Phrase("Campagne", boldFontTenBlack));
                        headerCell.Colspan = 0;
                        headerCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                        headerCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        headerCell.HorizontalAlignment = 0;
                        royaltiesTables.AddCell(headerCell);

                        headerCell = new PdfPCell(new Phrase("Du", boldFontTenBlack));
                        headerCell.Colspan = 0;
                        headerCell.HorizontalAlignment = 1;
                        headerCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                        headerCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        royaltiesTables.AddCell(headerCell);

                        headerCell = new PdfPCell(new Phrase("Au", boldFontTenBlack));
                        headerCell.Colspan = 0;
                        headerCell.HorizontalAlignment = 1;
                        headerCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                        headerCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        royaltiesTables.AddCell(headerCell);
                        #endregion


                        var totalDetails0 = xDocument.Descendants(nameof(_royaltyFee.Commune))
                                                    .Descendants(nameof(_royaltyFee.TypeProduit))
                                                    .Descendants(nameof(_royaltyFee.Produit))
                                                    .Descendants(nameof(_royaltyFee.Dp))
                                                    .Elements(nameof(_royaltyFee.Details))
                                                    .Count();

                        var totalTypeProduit0 = xDocument.Descendants(nameof(_royaltyFee.Commune))
                                                        .Descendants(nameof(_royaltyFee.TypeProduit))
                                                        .Count();

                        var totalTypeProduit10 = xDocument.Descendants(nameof(_royaltyFee.Commune))
                                                        .Elements(nameof(_royaltyFee.TypeProduit))
                                                        .Count();
                        
                        var totalProduit0 = xDocument.Descendants(nameof(_royaltyFee.Commune))
                                                       .Descendants(nameof(_royaltyFee.TypeProduit))
                                                       .Elements(nameof(_royaltyFee.Produit))
                                                       .Count();

                        #region -- Generate pdf group grid --
                        foreach (XElement communeXElement in xDocument.Descendants(nameof(_royaltyFee.Commune)))
                        {
                            var _communesElement = communeXElement.Elements(nameof(_royaltyFee.TypeProduit));
                            int countElementByProductType = countElementByDp;
                            countElementByDp = 0;
                                                        
                            foreach (XElement typeproduit in _communesElement)
                            {
                                var _produitsXElement = typeproduit.Elements(nameof(_royaltyFee.Produit));
                                countElementByProductType = countElementByDp;
                                double tempTotal = total;
                                total = 0;

                                foreach (XElement element in _produitsXElement)
                                {
                                    productName = element.FirstAttribute?.Value;

                                    var _dpsXElement = element.Elements(nameof(_royaltyFee.Dp));

                                    foreach (XElement childEllement in _dpsXElement)
                                    {
                                        int tmpCount = 0;
                                        countElementByProduct = countElementByDp;
                                        countElementByDp = 0;

                                        dpName = childEllement.Attribute("Nom").Value;
                                        PdfPCell productNameCell = new PdfPCell(new Phrase($"{ productName }\n   { dpName }", boldFontNineBlack));
                                        productNameCell.Colspan = 4;
                                        productNameCell.MinimumHeight = 23;
                                        productNameCell.Padding = 3;
                                        productNameCell.HorizontalAlignment = 0;
                                        royaltiesTables.AddCell(productNameCell);

                                        _detailXElements = childEllement.Elements(nameof(_royaltyFee.Details));
                                        double sstotal = 0;
                                        foreach (XElement detailXML in _detailXElements)
                                        {
                                            PdfPCell detailCell = new PdfPCell(
                                                new Phrase((string)(detailXML.Element(nameof(_royaltyFee.IdCmp))),
                                                normalFontNimeBlack));
                                            detailCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                                            detailCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                            royaltiesTables.AddCell(detailCell);

                                            detailCell = new PdfPCell(
                                                new Phrase((string)(detailXML.Element(nameof(_royaltyFee.Campagne))),
                                                normalFontNimeBlack));
                                            detailCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                                            detailCell.HorizontalAlignment = Element.ALIGN_LEFT;
                                            royaltiesTables.AddCell(detailCell);

                                            detailCell = new PdfPCell(
                                                new Phrase((string)(detailXML.Element(nameof(_royaltyFee.DateDebut))),
                                                normalFontNimeBlack));
                                            detailCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                                            detailCell.HorizontalAlignment = Element.ALIGN_CENTER;
                                            royaltiesTables.AddCell(detailCell);

                                            detailCell = new PdfPCell(
                                                new Phrase((string)(detailXML.Element(nameof(_royaltyFee.DateFin))),
                                                normalFontNimeBlack));
                                            detailCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                                            detailCell.HorizontalAlignment = Element.ALIGN_CENTER;
                                            royaltiesTables.AddCell(detailCell);

                                            enseigneName = (string)(detailXML.Element(nameof(_royaltyFee.Enseigne)));
                                            communeName = (string)(detailXML.Element(nameof(_royaltyFee.Commune)));
                                            idPv = (string)(detailXML.Element(nameof(_royaltyFee.IdPv)));

                                            sstotal += double.Parse(detailXML.Element(nameof(_royaltyFee.MontantRdvcHT)).Value.Replace(".", ","));
                                        }

                                        // -- Set Sub Total Name --
                                        PdfPCell subTotalCell = new PdfPCell(new Phrase("Sous Total", boldFontNineBlack));
                                        subTotalCell.Colspan = 2;
                                        subTotalCell.PaddingRight = 10;
                                        subTotalCell.PaddingBottom = 5;
                                        subTotalCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                        subTotalCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                                        royaltiesTables.AddCell(subTotalCell);

                                        // -- Set Sub Total Value --
                                        countElementByDp = _detailXElements.Count();
                                        countElementByProduct = countElementByProduct + countElementByDp;
                                        subTotalCell = new PdfPCell(new Phrase(countElementByDp.ToString(), boldFontNineBlack));
                                        subTotalCell.HorizontalAlignment = Element.ALIGN_CENTER;
                                        subTotalCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                                        royaltiesTables.AddCell(subTotalCell);

                                        sstotal = Math.Round(sstotal, 2);
                                        var ssTotalPhrase = new Phrase(sstotal.ToString(), boldFontNineBlack);
                                        PdfPCell cel = new PdfPCell(ssTotalPhrase);
                                        cel.HorizontalAlignment = Element.ALIGN_CENTER;
                                        cel.VerticalAlignment = Element.ALIGN_MIDDLE;
                                        royaltiesTables.AddCell(cel);

                                        total += sstotal;
                                    }
                                }

                                string productType = typeproduit.Attribute("Nom").Value;

                                totalHT = tempTotal + total;
                                //countElementByProduct = countElementByProductType + countElementByDp;
                                countElementByProduct += countElementByProduct;

                                // -- Set Total Name --
                                #region -- Manage Total by product type --
                                totalHtArrondi = Math.Round(totalHT, 2);
                                Phrase productTypePhrase = new Phrase($"Total  { productType }", boldFontNineBlack);
                                PdfPCell productTypeCell = new PdfPCell(productTypePhrase);
                                productTypeCell.Colspan = 3;
                                productTypeCell.PaddingRight = 10;
                                productTypeCell.PaddingBottom = 5;
                                productTypeCell.MinimumHeight = 15;
                                productTypeCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                productTypeCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                                royaltiesTables.AddCell(productTypeCell);

                                double t = Math.Round(total, 2);
                                productTypePhrase = new Phrase(t.ToString(), boldFontNineBlack);  //totalHtArrondi.ToString()
                                productTypeCell = new PdfPCell(productTypePhrase);
                                productTypeCell.PaddingRight = 10;
                                productTypeCell.PaddingBottom = 5;
                                productTypeCell.MinimumHeight = 15;
                                productTypeCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                                productTypeCell.HorizontalAlignment = Element.ALIGN_CENTER;
                                productTypeCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                                royaltiesTables.AddCell(productTypeCell);
                                #endregion
                            }
                                                     

                            var totalDetails = communeXElement
                                                        .Descendants(nameof(_royaltyFee.TypeProduit))
                                                        .Descendants(nameof(_royaltyFee.Produit))
                                                        .Descendants(nameof(_royaltyFee.Dp))
                                                        .Elements(nameof(_royaltyFee.Details))
                                                        .Count();

                            var totalTypeProduit = xDocument.Descendants(nameof(_royaltyFee.Commune))
                                                            .Descendants(nameof(_royaltyFee.TypeProduit))
                                                            .Count();

                            var totalProduit = xDocument.Descendants(nameof(_royaltyFee.Commune))
                                                           .Descendants(nameof(_royaltyFee.TypeProduit))
                                                           .Elements(nameof(_royaltyFee.Produit))
                                                           .Count();



                            // -- Set Sub table data --
                            if (countElementByDp > 0)
                            {
                                PdfPCell footerCell = new PdfPCell(
                                    new Phrase($" { idPv } - { enseigneName }  -  { communeName }", boldFontNineBlack));
                                footerCell.Colspan = 2;
                                footerCell.PaddingRight = 10;
                                footerCell.PaddingBottom = 5;
                                footerCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                                footerCell.HorizontalAlignment = Element.ALIGN_LEFT;
                                footerCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                                royaltiesTables.AddCell(footerCell);

                                footerCell = new PdfPCell(
                                    new Phrase($"Total    { totalDetails }         { totalHT.ToString() }", boldFontNineBlack));
                                footerCell.Colspan = 2;
                                footerCell.PaddingRight = 10;
                                footerCell.PaddingBottom = 5;
                                footerCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                                footerCell.HorizontalAlignment = Element.ALIGN_CENTER;
                                footerCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                                royaltiesTables.AddCell(footerCell);
                            }
                        }

                        masterDocument.Add(royaltiesTables);

                        // -- Set spacing between gridView and  --
                        royaltiesTables.SpacingAfter = 5f;
                        #endregion
                        #endregion


                        #region -- Total --
                        PdfPTable royalFeeTotalMasterTable = new PdfPTable(1);
                        royalFeeTotalMasterTable.TotalWidth = 400;
                        PdfPTable royalFeeTotalTable = new PdfPTable(4);

                        PdfPCell firstRoyalFeeTotalCell = new PdfPCell(new Phrase());
                        firstRoyalFeeTotalCell.BorderColor = BaseColor.WHITE;
                        firstRoyalFeeTotalCell.Border = 5;
                        royalFeeTotalTable.AddCell(firstRoyalFeeTotalCell);
                        PdfPCell royalFeeTotalCell = new PdfPCell(new Phrase("HT", boldFontNineBlack));
                        royalFeeTotalCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        royalFeeTotalCell.HorizontalAlignment = Element.ALIGN_CENTER;
                        royalFeeTotalTable.AddCell(royalFeeTotalCell);

                        string tva = "20,00";
                        royalFeeTotalCell = new PdfPCell(new Phrase($"TVA _footerPage.TxTva", boldFontNineBlack));
                        royalFeeTotalCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        royalFeeTotalCell.HorizontalAlignment = Element.ALIGN_CENTER;
                        royalFeeTotalTable.AddCell(royalFeeTotalCell);

                        royalFeeTotalCell = new PdfPCell(new Phrase("TTC", boldFontNineBlack));
                        royalFeeTotalCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        royalFeeTotalCell.HorizontalAlignment = Element.ALIGN_CENTER;
                        royalFeeTotalTable.AddCell(royalFeeTotalCell);

                        royalFeeTotalCell = new PdfPCell(new Phrase("Total du relevé", boldFontNineBlack));
                        royalFeeTotalCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        royalFeeTotalCell.HorizontalAlignment = Element.ALIGN_CENTER;
                        royalFeeTotalTable.AddCell(royalFeeTotalCell);

                        var tHt = total.ToString();
                        royalFeeTotalCell = new PdfPCell(new Phrase($" _footerPage.TotalHT '", boldFontNineBlack));  //_footerPage.TotalHT //totalHtArrondi.ToString()
                        royalFeeTotalCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        royalFeeTotalCell.HorizontalAlignment = Element.ALIGN_CENTER;
                        royalFeeTotalTable.AddCell(royalFeeTotalCell);

                        //montantTva = totalHtArrondi * _footerPage.TxTva / 100;
                        royalFeeTotalCell = new PdfPCell(new Phrase($" _footerPage.TotalTVA ", boldFontNineBlack));  //_footerPage.TotalTVA //montantTva
                        royalFeeTotalCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        royalFeeTotalCell.HorizontalAlignment = Element.ALIGN_CENTER;
                        royalFeeTotalTable.AddCell(royalFeeTotalCell);

                        totalTTC = montantTva + totalHtArrondi;
                        royalFeeTotalCell = new PdfPCell(new Phrase($" _footerPage.TotalTTC ", boldFontNineBlack));  //_footerPage.TotalTTC  //totalTTC
                        royalFeeTotalCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        royalFeeTotalCell.HorizontalAlignment = Element.ALIGN_CENTER;
                        royalFeeTotalTable.AddCell(royalFeeTotalCell);

                        PdfPCell royalFeeTotalMasterCell = new PdfPCell(royalFeeTotalTable);

                        royalFeeTotalMasterTable.AddCell(royalFeeTotalMasterCell);
                        royalFeeTotalMasterTable.WriteSelectedRows(0, -1, 22, 102, pdfContentByte);
                        #endregion

                        masterDocument.Close();
                        masterStream.Close();
                        masterWriter.Close();
                    }
                    #endregion

                    // -- Manage pages number --
                    //AddPageNumber(_fullPdfFilePath, _fullPdfFilePath);

                    result = true;

                    //_logger.Debug($"==> Fin création du Pdf.");

                    stopwatch.Stop();
                    TimeSpan stopwatchElapsed = stopwatch.Elapsed;
                    Console.WriteLine("Temps mis pour la génération du PDF " + Convert.ToInt32(stopwatchElapsed.TotalMilliseconds));

                    Process.Start(_fullPdfFilePath);
                }
            }
            #region -- Catch - Finaly Bloc --
            catch (DocumentException docExcexption)
            {
                result = false;
                //ErrorMessage = docExcexption.ToString();
                //_logger.Debug($" [{ ErrorMessage }]");
            }
            catch (IOException ioException)
            {
                result = false;
                //ErrorMessage = ioException.ToString();
                //_logger.Debug($" [{ ErrorMessage }]");
            }
            catch (Exception exception)
            {
                result = false;
                //ErrorMessage = exception.ToString();
                //_logger.Debug($" [{ ErrorMessage }]");
            }
            finally
            {
                List<string> wrongPathList = new List<string>();
                wrongPathList.Clear();

            }
            #endregion
            return result;
        }

        #endregion

        #region -- Get Pdf template DataSet --
        /// <summary>
        /// -- Tres bien OK --
        /// </summary>
        /// <param name="customeStoredProcedure"></param>
        /// <returns></returns>
        public void GetPdfDataTemplateSync(string customeStoredProcedure)
        {
            _logger.Debug($"Debut MAJ du model de pdf.");
            using (var conn = CreateConnection())
            {
                //return conn.QueryAsync(storedProcedure);
                using (var data = conn.QueryMultiple(customeStoredProcedure, null))
                {
                    var header = data.Read<HeaderPage>();
                    var footer = data.Read<FooterPage>();
                    var adress = data.Read<Adress>();

                    _headerDataSetTemple = DataTableHelper.ConvertToDataSet<HeaderPage>(header.ToList());
                    _footerDataSetTemple = DataTableHelper.ConvertToDataSet<FooterPage>(footer.ToList());
                    _adressTemple = DataTableHelper.ConvertToDataSet<Adress>(adress.ToList());
                }
            }
            _logger.Debug($"Fin récupération du model de pdf.");
        }

        /// <summary>
        /// -- OK --
        /// </summary>
        /// <param name="customeStoredProcedure"></param>
        private void GetReportBFPRedevanceSync(string customeStoredProcedure)
        {
            try
            {
                _logger.Debug($"Debut récupération du rapport BFP en lui même.");
                using (var connection = CreateConnection())
                {
                    using (var result = connection.QueryMultiple(customeStoredProcedure))
                    {
                        var royaltyFee = result.Read<RoyaltyFee>();

                        _royaltyFeeDataSet = DataTableHelper.ConvertToDataSet<RoyaltyFee>(royaltyFee.ToList());
                    }
                }
                _logger.Debug($"Debut MAJ du rapport BFP en lui même.");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
                throw;
            }
        }

        /// <summary>
        /// -- OK --
        /// </summary>
        /// <param name="customeStoredProcedure"></param>
        private DataSet GetContactReceivingTheFeeSync(string customeStoredProcedure)
        {
            try
            {
                _logger.Debug($"Debut récupération des contacts");

                DataSet contactDataSetTemple = null;

                using (var connection = CreateConnection())
                {
                    using (var result = connection.QueryMultiple(customeStoredProcedure))
                    {
                        var contacts = result.Read<ContactRedevance>();

                        contactDataSetTemple = DataTableHelper.ConvertToDataSet<ContactRedevance>(contacts.ToList());
                    }
                }
                _logger.Debug($"Fin récupération des contacts");
                return contactDataSetTemple;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
                throw;
            }
        }

        /// <summary>
        /// --   --
        /// </summary>
        /// <param name="customeStoredProcedure"></param>
        /// <returns></returns>
        private DataSet GetBfpReportHistoricSync(string customeStoredProcedure)
        {
            try
            {
                _logger.Debug($"Debut récupération du rapport BFP en lui même.");
                using (var connection = CreateConnection())
                {
                    using (var result = connection.QueryMultiple(customeStoredProcedure))
                    {
                        var bfpReportHistoric = result.Read<BfpReportHistoric>();
                        _bfpReportHistoricsEnumerable = bfpReportHistoric;
                        DataSet bfpReportHistoricDataSet = DataTableHelper.ConvertToDataSet<BfpReportHistoric>(bfpReportHistoric.ToList());

                        _logger.Debug($"Fin récupération du rapport BFP en lui même.");

                        return bfpReportHistoricDataSet;
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
                throw;
            }
        }

        /// <summary>
        /// --   --
        /// </summary>
        /// <param name="customeStoredProcedure"></param>
        /// <returns></returns>
        private int GetBfpBFPStatus(string storedProcedure, string parameter)
        {
            try
            {
                using (var connection = CreateConnection())
                {
                    _logger.Debug($"Debut récupération du d'envoi du PV avant MAJ de celui-ci.");

                    var statusId = connection.Query<int>(storedProcedure, new { Libelle = parameter },
                              commandType: CommandType.StoredProcedure).SingleOrDefault();

                    _logger.Debug($"Fin récupération du d'envoi du PV avant MAJ de celui-ci.");
                    return statusId;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
                ErrorMessage = exception.ToString();
                _logger.Debug($"Problème lors de la récupération du status envoi du PV avant MAJ de celui-ci.");
                throw;
            }
        }

        /// <summary>
        /// -- Update BFP change Statut envoi mail to 'Lancé' --                                  
        /// </summary>
        /// <param name="customeStoredProcedure"></param>
        /// <param name="idBfp"></param>
        private bool UpdateBFPChangeStatusEnvoiMail(string storedProcedure, long idBfp)
        {
            try
            {
                bool result = false;

                var stautId = GetBfpBFPStatus("SP_S_SttRdvcMail_Id_Select", "Lancé");

                if (stautId > 0)
                {
                    using (var connection = CreateConnection())
                    {
                        _logger.Debug($"Debut MAJ status envoi du PV Bfp [{ idBfp }]..");

                        var executionResult = connection.Execute(storedProcedure, new { e_IdBfp = idBfp, e_StatusId = stautId },
                                  commandType: CommandType.StoredProcedure);

                        if (executionResult == 1)
                        {
                            result = true;
                        }

                        _logger.Debug($"Fin MAJ status envoi du PV Bfp [{ idBfp }].");
                    }
                }

                return result;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
                ErrorMessage = exception.ToString();
                _logger.Debug($"Problème lors de la MAJ du status envoi du PV Bfp [{ idBfp }].");
                throw;
            }
        }

        /// <summary>
        /// -- Get IsRdvcMailPv an Fk_Crm if IsmailPv is Checked --
        /// </summary>
        /// <param name="customeStoredProcedure"></param>
        /// <returns></returns>
        private DataSet ManageIfRedvanceMailPvChecked(string storedProcedure, long idPv)
        {
            try
            {
                using (SqlConnection connnetion = CreateConnection())
                {
                    connnetion.Open();
                    using (var cmd = new SqlCommand(storedProcedure, connnetion))
                    {
                        _logger.Debug($"Debut récupération du fk_Crm si IsRdvcMailPv is true [{ idPv }].");

                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@e_Id", SqlDbType.BigInt).Value = idPv; // 86619; // e_BfpId; 

                        DataSet dataset = new DataSet();

                        using (SqlDataAdapter adp = new SqlDataAdapter(cmd))
                        {
                            adp.Fill(dataset);
                        }

                        _logger.Debug($"Fin MAJ récupération du fk_Crm si IsRdvcMailPv is true [{ idPv }].");
                        return dataset;
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
                ErrorMessage = exception.ToString();
                _logger.Debug($"Problème lors de la récupération du fk_Crm si IsRdvcMailPv is true [{ idPv }].");
                throw;
            }
        }

        /// <summary>
        /// -- Retrieve mail pv adress by fk_Crm --
        /// </summary>
        /// <param name="storedProcedure"></param>
        /// <param name="fkCrm"></param>
        /// <returns></returns>
        private string RetrieveMailPvByFkCrm(string storedProcedure, long fkCrm)
        {
            try
            {
                using (var connection = CreateConnection())
                {
                    _logger.Debug($"Debut récupération de email du Pv fontion du [{ fkCrm }] si coché.");

                    var mailPv = connection.Query<string>(storedProcedure, new { e_Id = fkCrm },
                              commandType: CommandType.StoredProcedure).SingleOrDefault();

                    _logger.Debug($"Fin récupération de email du Pv fontion du [{ fkCrm }] si coché.");
                    return mailPv;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
                ErrorMessage = exception.ToString();
                _logger.Debug($"Problème lors de la récupération de email du Pv fontion du [{ fkCrm }] si coché.");
                throw;
            }
        }

        /// <summary>
        /// --   --
        /// </summary>
        /// <param name="customeStoredProcedure"></param>
        /// <returns></returns>
        private DataSet GetMailTemplateASync(string customeStoredProcedure, string param)
        {
            try
            {
                using (SqlConnection connnetion = CreateConnection())
                {
                    connnetion.Open();
                    using (var cmd = new SqlCommand(customeStoredProcedure, connnetion))
                    {
                        _logger.Debug($"Fin récupération du model de mail en fontion de [{ param }].");

                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@ModeEnvoi", SqlDbType.VarChar).Value = param; // 86619; // e_BfpId; 

                        DataSet dataset = new DataSet();

                        using (SqlDataAdapter adp = new SqlDataAdapter(cmd))
                        {
                            adp.Fill(dataset);
                        }

                        _logger.Debug($"Fin récupération du model de mail en fontion de [{ param }].");
                        return dataset;
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
                ErrorMessage = exception.ToString();
                _logger.Debug($"Problème lors de la récupération du model de mail en fontion de [{ param }].");
                throw;
            }
        }
        #endregion

        #region -- Update historic royaltyFee  ---
        /// <summary>
        /// -- Update BFP royalties sent by email --
        /// </summary>
        /// <param name="e_Id"></param>
        private bool UpdateBFPRoyaltiesHistoric(long e_Id)
        {
            bool result = false;

            try
            {
                _logger.Debug($"Debut MAJ du Pv N° : [{ e_Id }]");

                using (SqlConnection connection = CreateConnection())
                {
                    string storedProcedure = "SP_PvRedevanceHistorique_Update";

                    connection.Open();
                    using (var cmd = new SqlCommand(storedProcedure, connection))
                    {
                        var param = new DynamicParameters();
                        param.Add("@e_Id", e_Id);

                        var updateResult = connection.Execute(storedProcedure, param, commandType: CommandType.StoredProcedure);
                        if (updateResult == 1)
                        {
                            result = true;
                        }
                    }
                }
                _logger.Debug($"Fin MAJ du Pv N° : [{ e_Id }]");
            }
            catch (SqlException ex)
            {
                for (int i = 0; i < ex.Errors.Count; i++)
                {
                    ErrorMessages.Append("Index #" + i + "\n" +
                        "Message: " + ex.Errors[i].Message + "\n" +
                        "LineNumber: " + ex.Errors[i].LineNumber + "\n" +
                        "Source: " + ex.Errors[i].Source + "\n" +
                        "Procedure: " + ex.Errors[i].Procedure + "\n");
                }
                result = false;

                _logger.Debug($"Un problème est survenu pendant la MAJ : [{ e_Id }] \r\n message [{ErrorMessages}]:");
                Console.WriteLine(ErrorMessages.ToString());
            }
            catch (Exception exception)
            {
                result = false;
                Console.WriteLine(exception.ToString());

                ErrorMessage = exception.ToString();
                _logger.Debug($"Un problème est survenu pendant la MAJ : [{ e_Id }] \r\n message [{ErrorMessage}]:");
                throw;
            }

            return result;
        }
        #endregion

        #region -- XML --        
        /// <summary>
        /// -- return XML BFP report --        //87859;
        /// </summary>
        /// <param name="customeStoredProcedure"></param>
        /// <returns></returns>
        private XDocument GetXMLReportBFPRedevance(long e_BfpId)
        {
            string storedProcedure = "[SP_ReportBFP_Redevance_ModelePv_XML_Select]";
            XDocument xdoc = null;
            StringBuilder errorMessages = new StringBuilder();

            try
            {
                using (SqlConnection con = CreateConnection())
                {
                    con.Open();
                    using (var cmd = new SqlCommand(storedProcedure, con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@e_BfpId", SqlDbType.Float).Value = e_BfpId; // 86619; // e_BfpId; 

                        using (var reader = cmd.ExecuteReader())
                        {
                            reader.NextResult();
                            while (reader.Read())
                            {
                                xdoc = XDocument.Parse(reader[0].ToString());
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                for (int i = 0; i < ex.Errors.Count; i++)
                {
                    errorMessages.Append("Index #" + i + "\n" +
                        "Message: " + ex.Errors[i].Message + "\n" +
                        "LineNumber: " + ex.Errors[i].LineNumber + "\n" +
                        "Source: " + ex.Errors[i].Source + "\n" +
                        "Procedure: " + ex.Errors[i].Procedure + "\n");
                }
                Console.WriteLine(errorMessages.ToString());
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
                throw;
            }

            return xdoc;
        }
        #endregion

        #region -- Merge PDF files OK ***  A conserver   *** --
        string outPut = @"C:\Users\mMABOU\Desktop\PDFFiles\Nouveau.pdf";
        string outPutTest = @"E:\Telechargements\iTextDemo\iTextDemo\iTextDemoInConsole\PDFs\Chapter1_Example1.pdf";
        string inputs = @"C:\Users\mMABOU\Desktop\PDFFiles\32261_29102019.pdf";

        /// <summary>
        /// -- A conserver -- MergePDFs(outPut, inputs, outPutTest); --
        /// </summary>
        /// <param name="outPutFilePath"></param>
        /// <param name="filesPath"></param>
        private static void MergePDFs(string outPutFilePath, params string[] filesPath)
        {
            List<PdfReader> readerList = new List<PdfReader>();
            foreach (string filePath in filesPath)
            {
                PdfReader pdfReader = new PdfReader(filePath);
                readerList.Add(pdfReader);
            }

            //Define a new output document and its size, type
            Document document = new Document(PageSize.A4, 0, 0, 0, 0);
            //Create blank output pdf file and get the stream to write on it.
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(outPutFilePath, FileMode.Create));
            document.Open();

            foreach (PdfReader reader in readerList)
            {
                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    PdfImportedPage page = writer.GetImportedPage(reader, i);
                    document.Add(Image.GetInstance(page));
                }
            }
            document.Close();
        }
        #endregion 
    }
}
