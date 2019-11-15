using MediaPerf.ManagerPdf.Model.Contracts;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace MediaPerf.ManagerPdf.Repository.Helpers.Contracts
{
    public interface IConsolidateHelper
    {
        IHeaderPage ConsolidateHeader(DataSet headerDataTable, DataSet adressTemple, 
            long aModeleEdition, long aStTarif, IHeaderPage _headerPage);

        IFooterPage ConsolidateFooter(DataSet footerDataTable, IFooterPage _footerPage);

        IBfpReportHistoric ConsolidateBfpReportHistoric(DataRow bfpReportHistoricDataRow,
           IBfpReportHistoric bfpReportHistoric);

        IDictionary<string, string> ConsolidateRoyaltyFeeContact(DataSet contactDataSet,
            IContactRedevance contactRedevance);

        IContactRedevance ConsolidateContactPvInfo(DataSet contactPvInfoDataSet,
            IContactRedevance contactPv);

        IMailTemplate BuildMailTemplate(DataSet mailTemplateDataSet, IMailTemplate mailTemplate);
    }
}