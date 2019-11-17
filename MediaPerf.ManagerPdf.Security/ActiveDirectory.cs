using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaPerf.ManagerPdf.Security
{
    public class ActiveDirectory
    {
        #region Variables
        /// <summary>
        /// Nom le contenu du paramètre Acitve Directory par défaut ou définit
        /// </summary>
        private static string _ParametreActiveDirectory_Login = "";
        /// <summary>
        /// Nom le contenu du paramètre Acitve Directory par défaut ou définit
        /// </summary>
        public static string ParametreActiveDirectory_Login
        {
            get
            {
                //_ParametreActiveDirectory_Login = System.Configuration.ConfigurationSettings.AppSettings["ActiveDirectoryLoginWindows"];
                _ParametreActiveDirectory_Login = System.Configuration.ConfigurationManager.AppSettings["ActiveDirectoryLoginWindows"];
                if (_ParametreActiveDirectory_Login == null)
                { _ParametreActiveDirectory_Login = "SAMAccountName"; }
                return _ParametreActiveDirectory_Login;
            }
        }
        /// <summary>
        /// Nom le contenu du paramètre Acitve Directory par défaut ou définit
        /// </summary>
        private static string _ParametreActiveDirectory_Mail = "";
        /// <summary>
        /// Nom le contenu du paramètre Acitve Directory par défaut ou définit
        /// </summary>
        public static string ParametreActiveDirectory_Mail
        {
            get
            {
                //_ParametreActiveDirectory_Mail = System.Configuration.ConfigurationSettings.AppSettings["ActiveDirectoryMailWindows"];
                _ParametreActiveDirectory_Mail = System.Configuration.ConfigurationManager.AppSettings["ActiveDirectoryMailWindows"];
                if (_ParametreActiveDirectory_Mail == null)
                { _ParametreActiveDirectory_Mail = "mail"; }
                return _ParametreActiveDirectory_Mail;
            }
        }
        /// <summary>
        /// Nom le contenu du paramètre Active Directory par défaut ou définit
        /// </summary>
        private static string _ParametreActiveDirectory_Phone = "";
        //Rajout OJU - Edition DBC - 02.03.2009
        public static string ParametreActiveDirectory_Phone
        {
            get
            {
                //_ParametreActiveDirectory_Phone = System.Configuration.ConfigurationSettings.AppSettings["ActiveDirectorytelephoneNumberWindows"];
                _ParametreActiveDirectory_Phone = System.Configuration.ConfigurationManager.AppSettings["ActiveDirectorytelephoneNumberWindows"];
                if (_ParametreActiveDirectory_Phone == null)
                { _ParametreActiveDirectory_Phone = "telephoneNumber"; }
                return _ParametreActiveDirectory_Phone;
            }
        }

        private static string _ParametreActiveDirectory_Fax = "";
        //Rajout AG evolution le 04 01 2010
        public static string ParametreActiveDirectory_Fax
        {
            get
            {
                //_ParametreActiveDirectory_Fax = System.Configuration.ConfigurationSettings.AppSettings["ActiveDirectoryfaxWindows"];
                _ParametreActiveDirectory_Fax = System.Configuration.ConfigurationManager.AppSettings["ActiveDirectoryfaxWindows"];
                if (_ParametreActiveDirectory_Fax == null)
                { _ParametreActiveDirectory_Fax = "facsimileTelephoneNumber"; }
                return _ParametreActiveDirectory_Fax;
            }
        }
        private static string _ParametreActiveDirectory_DisplayName = "";
        public static string ParametreActiveDirectory_DisplayName
        {
            get
            {
                //_ParametreActiveDirectory_DisplayName = System.Configuration.ConfigurationSettings.AppSettings["ActiveDirectorydisplayNameWindows"];
                _ParametreActiveDirectory_DisplayName = System.Configuration.ConfigurationManager.AppSettings["ActiveDirectorydisplayNameWindows"];
                if (_ParametreActiveDirectory_DisplayName == null)
                { _ParametreActiveDirectory_DisplayName = "cn"; }
                return _ParametreActiveDirectory_DisplayName;
            }
        }
        #endregion
                
        #region MyRegion
        /// <summary>
        /// Retourne true si l'utilisateur à un profil dans Active Directory. Attention la connexion necessite l'existance du compte standard de connexion à la 
        /// base de données "SQLUser/SQLUser" défini dans le fichier de ressources.
        /// </summary>
        public static bool IsActiveDirectoryAutorised(string aDomaine, string aLogin)
        {
            return IsDomaineDirectoryAutorised(aDomaine) && IsLoginActiveDirectoryAutorised(aLogin);
        }

        /// <summary>
        /// retourne true si le domaine correspond
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static bool IsDomaineDirectoryAutorised(string aDomaine)
        {
            bool _Return = false;
            if (ResourceActiveDirectory.DomaineActiveDirectory.ToLower() == aDomaine.ToLower())
            { _Return = true; }
            return _Return;
        }

        /// <summary>
        /// Retourne l'annuaire ActiveDirectory courant
        /// </summary>
        /// <param name="AnnuaireAD"></param>
        /// <returns></returns>
        private static DirectoryEntry GetAnnuaireAD()
        {
            DirectoryEntry AnnuaireAD = null;
            AnnuaireAD = new DirectoryEntry(ResourceActiveDirectory.TypeAnnuaireActiveDirectory +
                ResourceActiveDirectory.DomaineActiveDirectory, 
                ResourceActiveDirectory.LoginUtilisateurBaseDeDonnees,
                ResourceActiveDirectory.PswUtilisateurBaseDeDonnees);

            return AnnuaireAD;
        }

        /// <summary>
        /// Retourne true si l'utilisateur à un profil dans Active Directory. Attention la connexion necessite l'existance du compte standard de connexion à la 
        /// base de données "SQLUser/SQLUser" défini dans le fichier de ressources.
        /// </summary>
        public static bool IsLoginActiveDirectoryAutorised(string aLogin)
        {
            try
            {
                DirectoryEntry _DirectoryEntry = GetUserActiveDirectoryProfil(aLogin);
                if (_DirectoryEntry != null)
                { return true; }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
            }

            return false;
        }

        /// <summary>
        /// retourne l'adresse mail correspondant au login passé en paramètre
        /// </summary>
        /// <param name="aLogin"></param>
        /// <returns></returns>
        public static System.Net.Mail.MailAddress GetMail(string aLogin)
        {
            System.Net.Mail.MailAddress _MailAddress = null;

            try
            {
                DirectoryEntry _DirectoryEntry = GetUserActiveDirectoryProfil(aLogin);
                if ((_DirectoryEntry != null) && (_DirectoryEntry.Properties[ParametreActiveDirectory_Mail].Value != null))
                {
                    if (_DirectoryEntry.Properties[ParametreActiveDirectory_Mail].Value != null)
                    {
                        _MailAddress = new System.Net.Mail.MailAddress(
                            _DirectoryEntry.Properties[ParametreActiveDirectory_Mail].Value.ToString());
                    }
                    else
                    {
                        _MailAddress = null;
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
            }

            return _MailAddress;
        }

        //Rajout OJU - Edition DBC - 02.03.2009
        public static string GetPhone(string aLogin)
        {
            string _Phone = "";
            try
            {
                DirectoryEntry _DirectoryEntry = GetUserActiveDirectoryProfil(aLogin);
                if ((_DirectoryEntry != null) && (_DirectoryEntry.Properties[ParametreActiveDirectory_Phone].Value != null))
                {
                    _Phone = _DirectoryEntry.Properties[ParametreActiveDirectory_Phone].Value.ToString();
                }
            }
            catch (Exception ex)
            {
                //ClsException.AfficheException(exception);
            }
            return _Phone;
        }
        //Rajout OJU - Edition DBC - 02.03.2009
        public static string GetFax(string aLogin)
        {
            string _Fax = "";
            try
            {
                DirectoryEntry _DirectoryEntry = GetUserActiveDirectoryProfil(aLogin);
                if ((_DirectoryEntry != null) && (_DirectoryEntry.Properties[ParametreActiveDirectory_Fax].Value != null))
                {
                    _Fax = _DirectoryEntry.Properties[ParametreActiveDirectory_Fax].Value.ToString();
                }
            }
            catch (Exception ex)
            {
                //ClsException.AfficheException(exception);
            }
            return _Fax;
        }

        //Assane le 25 octobre lenteur edition dbc
        public static ArrayList GetNomCommercial_MailTelFax(string aLogin)
        {
            ArrayList _List = new ArrayList();
            string _DisplayName = "";
            string _Phone = "";
            string _Mail = "";
            string _Fax = "";
            try
            {
                if (GetUserActiveDirectoryProfil(aLogin) != null)
                {
                    DirectoryEntry _DirectoryEntry = GetUserActiveDirectoryProfil(aLogin);

                    _DisplayName = _DirectoryEntry.Properties[ParametreActiveDirectory_DisplayName].Value.ToString();
                    _Phone = _DirectoryEntry.Properties[ParametreActiveDirectory_Phone].Value.ToString();
                    _Mail = _DirectoryEntry.Properties[ParametreActiveDirectory_Mail].Value.ToString();

                    if (_DirectoryEntry.Properties[ParametreActiveDirectory_Fax].Value != null)
                    {
                        _Fax = _DirectoryEntry.Properties[ParametreActiveDirectory_Fax].Value.ToString();
                    }
                    else
                    {
                        _Fax = "";
                    }

                    if ((_DirectoryEntry != null) && (_DisplayName != null) && (_Phone != null) && (_Mail != null))// && (_Fax != null )
                    {

                        _List.Add(_DisplayName + " au " + _Phone);
                        _List.Add(_Mail + "  " + _Fax);

                        //_List.Add(_DirectoryEntry.Properties[ClsActiveDirectory.ParametreActiveDirectory_DisplayName].Value.ToString() + " au " + _DirectoryEntry.Properties[ClsActiveDirectory._ParametreActiveDirectory_Phone].Value.ToString());
                        //_List.Add(_DirectoryEntry.Properties[ClsActiveDirectory._ParametreActiveDirectory_Mail].Value.ToString() + " - " + _DirectoryEntry.Properties[ClsActiveDirectory._ParametreActiveDirectory_Fax].Value.ToString());
                    }
                }
                else
                {
                    //MessageBox.Show("L'utilisateur " + aLogin + " n'existe pas dans Active directory: il ne sera pas afficher dans l'édition.");
                }

            }
            catch (Exception ex)
            {
                //ClsException.AfficheException(exception);
            }
            return _List;
        }
        public static string GetDisplayName(string aLogin)
        {
            string _DisplayName = null;
            try
            {
                DirectoryEntry _DirectoryEntry = GetUserActiveDirectoryProfil(aLogin);
                if ((_DirectoryEntry != null) && (_DirectoryEntry.Properties[ParametreActiveDirectory_DisplayName].Value != null))
                {
                    _DisplayName = _DirectoryEntry.Properties[ParametreActiveDirectory_DisplayName].Value.ToString();
                }
            }
            catch (Exception ex)
            {
                //ClsException.AfficheException(exception);
            }
            return _DisplayName;
        }

        public static void GetDisplayNamePhoneMail(string aLogin, 
            ref string displayName, 
            ref string phone, 
            ref System.Net.Mail.MailAddress mail)
        {
            displayName = phone = null;
            mail = null;
            try
            {
                DirectoryEntry _DirectoryEntry = GetUserActiveDirectoryProfil(aLogin);
                if ((_DirectoryEntry != null) && (_DirectoryEntry.
                    Properties[ParametreActiveDirectory_DisplayName].
                    Value != null))
                {
                    displayName = _DirectoryEntry.
                        Properties[ParametreActiveDirectory_DisplayName].
                        Value.ToString();
                }
                if ((_DirectoryEntry != null) && (_DirectoryEntry.
                    Properties[ParametreActiveDirectory_Phone].
                    Value != null))
                {
                    phone = _DirectoryEntry.
                        Properties[ParametreActiveDirectory_Phone].
                        Value.ToString();
                }
                if ((_DirectoryEntry != null) && (_DirectoryEntry.
                    Properties[ParametreActiveDirectory_Mail].
                    Value != null))
                {
                    mail = new System.Net.Mail.MailAddress(_DirectoryEntry.
                        Properties[ParametreActiveDirectory_Mail].
                        Value.ToString());
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
            }
        }


        private static DirectoryEntry GetUserActiveDirectoryProfil(string aLogin)
        {
            try
            {
                DirectoryEntry AnnuaireAD = GetAnnuaireAD();
                DirectorySearcher searcher = new DirectorySearcher(AnnuaireAD);
                searcher.Filter = "(&(objectClass=user)(objectCategory=person)(sn=*))";
                //searcher.PageSize = 2000;
                foreach (SearchResult result in searcher.FindAll())
                {
                    // On récupère l'entrée trouvée lors de la recherche
                    DirectoryEntry DirEntry = result.GetDirectoryEntry();

                    //On peut maintenant afficher les informations désirées
                    string login = DirEntry.Properties[ParametreActiveDirectory_Login].Value.ToString();
                    if ((aLogin.ToLower()) == login.ToLower())
                    {
                        return DirEntry;
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
            }

            return null;
        }
        #endregion
    }
}
