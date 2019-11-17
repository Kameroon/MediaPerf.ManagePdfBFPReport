using MediaPerf.ManagerPdf.Model.Contracts;
using MediaPerf.ManagerPdf.Repository.Contracts;
using MediaPerf.ManagerPdf.Repository.Helpers.Contracts;
using MediaPerf.ManagerPdf.Security;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace MediaPerf.ManagerPdf.Repository.Helpers.Implementations
{
    public class ConsolidateHelper : IConsolidateHelper
    {
        #region -- Fields --

        #endregion

        #region -- Constructor --
        public ConsolidateHelper()
        {

        }
        #endregion

        #region -- Methods --
        /// <summary>
        /// --   --
        /// </summary>
        /// <param name="contactDataSet"></param>
        /// <param name="contactRedevance"></param>
        /// <returns></returns>
        public IDictionary<string, string> ConsolidateRoyaltyFeeContact(DataSet contactDataSet,
            IContactRedevance contactRedevance)
        {
            var contactDictionary = new Dictionary<string, string>();

            contactDictionary.Clear();

            for (int c = 0; c < contactDataSet.Tables[0].Rows.Count; c++)
            {
                DataRow dr = contactDataSet.Tables[0].Rows[c];

                contactRedevance.Contact = dr[nameof(contactRedevance.Contact)] as string;
                contactRedevance.EMail = dr[nameof(contactRedevance.EMail)] as string;
                contactRedevance.Titre = dr[nameof(contactRedevance.Titre)] as string;

                if (!contactDictionary.ContainsKey(contactRedevance.Contact))
                {
                    contactDictionary.Add(contactRedevance.Contact, contactRedevance.EMail);
                }
            }
            
            return contactDictionary;
        }

        public IContactRedevance ConsolidateContactPvInfo(DataSet contactPvInfoDataSet,
            IContactRedevance contactPv)
        {
            var contactDictionary = new Dictionary<string, string>();

            contactDictionary.Clear();

            for (int c = 0; c < contactPvInfoDataSet.Tables[0].Rows.Count; c++)
            {
                DataRow dr = contactPvInfoDataSet.Tables[0].Rows[c];
                
                contactPv.IsRdvcMailPv = (bool)dr[nameof(contactPv.IsRdvcMailPv)];
                contactPv.Fk_Crm = Convert.ToInt64(dr[nameof(contactPv.Fk_Crm)].ToString());
            }

            return contactPv;
        }

        /// <summary>
        /// --  --
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="bfpReportHistoric"></param>
        /// <returns></returns>
        public IBfpReportHistoric ConsolidateBfpReportHistoric(DataRow dr,
           IBfpReportHistoric bfpReportHistoric)
        {
            bfpReportHistoric.Id = Int32.Parse(dr[nameof(bfpReportHistoric.Id)].ToString());
            bfpReportHistoric.Fk_Bfp = Int64.Parse(dr[nameof(bfpReportHistoric.Fk_Bfp)].ToString());
            bfpReportHistoric.Fk_S_StTarif = Int64.Parse(dr[nameof(bfpReportHistoric.Fk_S_StTarif)].ToString());
            bfpReportHistoric.Fk_S_ModeleEditionBfp = Convert.ToInt32(dr[nameof(bfpReportHistoric.Fk_S_ModeleEditionBfp)].ToString());
            bfpReportHistoric.IdPv = dr[nameof(bfpReportHistoric.IdPv)] != null ? Int64.Parse(dr[nameof(bfpReportHistoric.IdPv)].ToString()) : 0;
            bfpReportHistoric.DtSessionRfr = dr[nameof(bfpReportHistoric.DtSessionRfr)] as string;
            bfpReportHistoric.UserCreation = Convert.ToInt32(dr[nameof(bfpReportHistoric.UserCreation)] as string);
            bfpReportHistoric.DateCreation = dr[nameof(bfpReportHistoric.DateCreation)] as string;

            bfpReportHistoric.SpReportBFPPdfTemplate = dr[nameof(bfpReportHistoric.SpReportBFPPdfTemplate)] as string;
            bfpReportHistoric.SpReportBFPRdvcPv = dr[nameof(bfpReportHistoric.SpReportBFPRdvcPv)] as string;

            return bfpReportHistoric;
        }
        
        /// <summary>
        /// -- Consolidate Pdf header page and send it to _repository --
        /// </summary>
        /// <param name="headerDataTable"></param>
        /// <param name="adressTemple"></param>
        /// <param name="aModeleEdition"></param>
        /// <param name="aStTarif"></param>
        /// <returns></returns>
        public IHeaderPage ConsolidateHeader(DataSet headerDataTable, 
            DataSet adressTemple, 
            long aModeleEdition, 
            long aStTarif, 
            IHeaderPage _headerPage)
        {
            string duplicataText = "Duplicata";

            // --  Loading the pdf header data  --
            for (int i = 0; i < headerDataTable.Tables[0].Rows.Count; i++)
            {
                DataRow dr = headerDataTable.Tables[0].Rows[i];

                try
                {
                    _headerPage.IdBFP = Int64.Parse(dr[nameof(_headerPage.IdBFP)].ToString());
                    var img = dr[nameof(_headerPage.PathLogoEntete)];
                    //_headerPage.PathLogoEntete = Convert.ToByte(dr["PathLogoEntete"]);
                    _headerPage.AdresseMpf = dr[nameof(_headerPage.AdresseMpf)] as string;
                    _headerPage.Prestataire = dr[nameof(_headerPage.Prestataire)] as string;
                    _headerPage.AdressePrestataire = dr[nameof(_headerPage.AdressePrestataire)] as string;
                    _headerPage.DateLivraison = dr[nameof(_headerPage.DateLivraison)] as string;
                    _headerPage.DateBfp = dr[nameof(_headerPage.DateBfp)] as string;
                    _headerPage.BfpParam1 = dr[nameof(_headerPage.BfpParam1)] as string;
                    _headerPage.BfpParam2 = dr[nameof(_headerPage.BfpParam2)] as string;
                    _headerPage.BfpParam7 = dr[nameof(_headerPage.BfpParam7)] as string;
                    _headerPage.NbPv = Int64.Parse(dr[nameof(_headerPage.NbPv)].ToString());
                    _headerPage.NbCmp = Int64.Parse(dr[nameof(_headerPage.NbCmp)].ToString());
                }
                catch (Exception ex)
                {

                    throw;
                }

                // --   --
                if (aModeleEdition == 3 || aModeleEdition == 5 || aModeleEdition == 9)
                {
                    string displayName = null;
                    string phone = null;
                    System.Net.Mail.MailAddress mail = null;
                    ActiveDirectory.GetDisplayNamePhoneMail(dr[nameof(_headerPage.BfpParam3)].ToString(), ref displayName, ref phone, ref mail);
                }
                else // -- Seul cette étape nous intéresse --
                {
                    _headerPage.Destinataire = dr[nameof(_headerPage.Destinataire)] as string;
                    _headerPage.BfpParam3 = dr[nameof(_headerPage.BfpParam3)] as string;
                    _headerPage.BfpParam4 = dr[nameof(_headerPage.BfpParam4)] as string;
                }

                if (aModeleEdition == 5)
                {
                    _headerPage.NomComplet = dr[nameof(_headerPage.NomComplet)] as string;
                    _headerPage.Telephone = dr[nameof(_headerPage.Telephone)] as string;
                    _headerPage.Mail = dr[nameof(_headerPage.Mail)] as string;
                }

                if (aStTarif == 6 && adressTemple.Tables[0].Rows.Count != 0)
                {
                    _headerPage.Prestataire = adressTemple.Tables[0].Rows[0][nameof(_headerPage.Prestataire)] as string;
                    _headerPage.AdressePrestataire = adressTemple.Tables[0].Rows[0][nameof(_headerPage.AdressePrestataire)] as string;
                }

                _headerPage.Duplicata = duplicataText;  /*dr["Duplicata"] as string;*/
            }

            return _headerPage;
        }

        /// <summary>
        /// -- Consolidate Pdf footer page and send it to _repository --
        /// </summary>
        /// <param name="footerDataTable"></param>
        /// <param name="_footerPage"></param>
        /// <returns></returns>
        public IFooterPage ConsolidateFooter(DataSet footerDataSet,
            IFooterPage _footerPage)
        {
            for (int i = 0; i < footerDataSet.Tables[0].Rows.Count; i++)
            {
                DataRow dr = footerDataSet.Tables[0].Rows[i];
                _footerPage.TotalHT = Int64.Parse(dr[nameof(_footerPage.TotalHT)].ToString());
                _footerPage.TotalTVA = Int64.Parse(dr[nameof(_footerPage.TotalTVA)].ToString());
                _footerPage.TotalTTC = Int64.Parse(dr[nameof(_footerPage.TotalTTC)].ToString());
                _footerPage.BfpParam4 = dr[nameof(_footerPage.BfpParam4)] as string;
                _footerPage.BfpParam5 = dr[nameof(_footerPage.BfpParam5)] as string;
                _footerPage.BfpParam6 = dr[nameof(_footerPage.BfpParam6)] as string;
                _footerPage.BfpParam7 = dr[nameof(_footerPage.BfpParam7)] as string;
                _footerPage.IdBFP = Int64.Parse(dr[nameof(_footerPage.IdBFP)].ToString());
                _footerPage.TxTva = Int64.Parse(dr[nameof(_footerPage.TxTva)].ToString());
            }

            return _footerPage;
        }

        /// <summary>
        /// -- Build mail template --
        /// </summary>
        /// <param name="mailTemplateDataSet"></param>
        /// <param name="mailTemplate"></param>
        /// <returns></returns>
        public IMailTemplate BuildMailTemplate(DataSet mailTemplateDataSet, IMailTemplate mailTemplate)
        {
            IList<IMailTemplate> mailTemplates = new List<IMailTemplate>();
            foreach (DataRow row in mailTemplateDataSet.Tables[0].Rows)
            {
                mailTemplate.Nom = row[nameof(mailTemplate.Nom)] as string;
                var corpsDuMail = row[nameof(mailTemplate.Texte)];
                mailTemplate.Texte = corpsDuMail != null ? corpsDuMail as string : null;
                var destinataire = row[nameof(mailTemplate.Destinataires)];
                mailTemplate.Destinataires = destinataire != null ? destinataire as string : null;
                mailTemplate.Objet = row[nameof(mailTemplate.Objet)] as string;
            }

            return mailTemplate;
        }
        #endregion
    }
}