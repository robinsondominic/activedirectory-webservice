using System;
using System.Collections.Generic;
using System.Web.Services;
using System.Web.Script.Serialization;
using System.DirectoryServices;
using System.Collections;
using System.DirectoryServices.AccountManagement;
using PropertyCollection = System.DirectoryServices.PropertyCollection;

/// <summary>
/// Summary description for ADSOAPWebService
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
// [System.Web.Script.Services.ScriptService]
public class ADSOAPWebService : System.Web.Services.WebService
{
    public string domainpath = "LDAP://DC=domain,DC=local";
    public ADSOAPWebService()
    {

        //Uncomment the following line if using designed components 
        //InitializeComponent(); 
    }


    public string ExtractUserName(string path)
    {
        string[] userPath = path.Split(new char[] { '\\' });
        return userPath[userPath.Length - 1];
    }


    public class UserEmailInfo
    {
        public UserEmailInfo()
        {
        }
        public string DisplayName { get; set; }
        public string DisplayEmail { get; set; }
    }




    [WebMethod]

    public bool AuthenticateADCredentials(string userName,string passWord)
    {
        string domainName = System.Environment.UserDomainName;
        string domainUserName = System.Environment.UserName;
        PrincipalContext pc = new PrincipalContext(ContextType.Domain, domainName, domainUserName, ContextOptions.SimpleBind.ToString());
        bool isValid = pc.ValidateCredentials(userName.ToUpper(), passWord);
        if (isValid)
        {
            return true;
        }
        else
        {
            return false;
        }
    }



    [WebMethod]

    public bool UserExistInActiveDirectory(string loginName)
    {
        string userName = ExtractUserName(loginName);
        DirectorySearcher search = new DirectorySearcher();
        search.Filter = String.Format("(SAMAccountName={0})", userName);
        search.PropertiesToLoad.Add("cn");
        SearchResult result = search.FindOne();

        if (result == null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    [WebMethod]

    public ArrayList ListAllUsersInActiveDirectory()
    {
        ArrayList allUsers = new ArrayList();

        // parameter for domainpath = "LDAP://DC=domain,DC=local"

       
        DirectoryEntry searchRoot = new DirectoryEntry(domainpath);
        DirectorySearcher search = new DirectorySearcher(searchRoot);
        search.Filter = "(&(objectClass=user)(objectCategory=person))";
        search.PropertiesToLoad.Add("displayname");

        SearchResult result;
        SearchResultCollection resultCol = search.FindAll();
        if (resultCol != null)
        {
            for (int counter = 0; counter < resultCol.Count; counter++)
            {
                result = resultCol[counter];
                if (result.Properties.Contains("displayname"))
                {
                    allUsers.Add((String)result.Properties["displayname"][0]);
                }
            }
        }
        return allUsers;
    }


    [WebMethod]

    public ArrayList ListAllUserEmailsAndDisplayNamesInActiveDirectory()
    {
        ArrayList allUsers = new ArrayList();

        
        DirectoryEntry searchRoot = new DirectoryEntry(domainpath);
        DirectorySearcher search = new DirectorySearcher(searchRoot);
        search.Filter = "(&(objectClass=user)(objectCategory=person))";
        search.PropertiesToLoad.Add("mail");
        search.PropertiesToLoad.Add("displayname");       

        SearchResult result;
        SearchResultCollection resultCol = search.FindAll();
        if (resultCol != null)
        {
            for (int counter = 0; counter < resultCol.Count; counter++)
            {
                result = resultCol[counter];
                if (result.Properties.Contains("mail"))
                {
                    allUsers.Add((String)result.Properties["mail"][0]);
                    allUsers.Add((String)result.Properties["displayname"][0]);
                   
                }
            }
        }
        return allUsers;
    }

    [WebMethod]
    public ArrayList ListAllUserEmailsAndUserNamesInActiveDirectory()
    {
        ArrayList allUsers = new ArrayList();

        
        DirectoryEntry searchRoot = new DirectoryEntry(domainpath);
        DirectorySearcher search = new DirectorySearcher(searchRoot);
        search.Filter = "(&(objectClass=user)(objectCategory=person))";
        search.PropertiesToLoad.Add("mail");
        search.PropertiesToLoad.Add("samaccountname");

        SearchResult result;
        SearchResultCollection resultCol = search.FindAll();
        if (resultCol != null)
        {
            for (int counter = 0; counter < resultCol.Count; counter++)
            {
                result = resultCol[counter];
                if (result.Properties.Contains("mail"))
                {
                    allUsers.Add((String)result.Properties["mail"][0]);
                    allUsers.Add((String)result.Properties["samaccountname"][0]);
                }
            }
        }
        return allUsers;
    }


    [WebMethod]

    public string FindUserEmailFromAD(string userName)
    {
        

        var search = new DirectorySearcher(new DirectoryEntry(domainpath));
        string ldapfilter = "(&(ObjectClass=user)(objectCategory=person)(samaccountname={0}))";

        search.Filter = String.Format(ldapfilter, userName);
        search.PropertiesToLoad.Add("mail");
        search.PropertiesToLoad.Add("samaccountname");

        SearchResult result = search.FindOne();
        string emailAddress = null;

        if (result == null)
        {
            emailAddress = "none";
        }
        else
        {
            if (result.Properties.Contains("mail") && (result != null))
            {
                emailAddress = result.Properties["mail"][0].ToString();
            }
        }
        //return emailAddress;
        string myJsonString = (new JavaScriptSerializer()).Serialize(emailAddress);
        return myJsonString;
    }





    [WebMethod]
    public string FindUserDisplayNameAndEmailAddressFromAD(string displayName)
    {
        

        List<UserEmailInfo> displayNameEmailAddress = new List<UserEmailInfo>();

        var search = new DirectorySearcher(new DirectoryEntry(domainpath));
        string ldapfilter = "(&(ObjectClass=user)(objectCategory=person)(name={0}))";

        search.Filter = String.Format(ldapfilter, displayName);
        search.PropertiesToLoad.Add("displayname");
        search.PropertiesToLoad.Add("mail");

        SearchResult result = search.FindOne();

            if (result == null)
            {
                displayNameEmailAddress.Add(new UserEmailInfo()
                {
                    DisplayName = "none",
                    DisplayEmail = "none"
                });
            }
            else
            {
                if (result.Properties.Contains("mail") && result.Properties["mail"] != null)
                {
                    displayNameEmailAddress.Add(new UserEmailInfo()
                    {
                        DisplayName = result.Properties["displayname"][0].ToString(),
                        DisplayEmail = result.Properties["mail"][0].ToString()
                    });
                }
            }                
        //return emailAddress;
        string myJsonString = (new JavaScriptSerializer()).Serialize(displayNameEmailAddress);
        return myJsonString;
    }

    [WebMethod]
    public string ShowAllADUserEmailsAndDisplayNames()
    {

        List<UserEmailInfo> displayNameEmailAddress = new List<UserEmailInfo>();

    
        DirectoryEntry searchRoot = new DirectoryEntry(domainpath);
        DirectorySearcher search = new DirectorySearcher(searchRoot);
        search.Filter = "(&(objectClass=user)(objectCategory=person))";
        search.PropertiesToLoad.Add("mail");
        search.PropertiesToLoad.Add("displayname");

        SearchResult result;
        SearchResultCollection resultCol = search.FindAll();
        if (resultCol != null)
        {
            for (int counter = 0; counter < resultCol.Count; counter++)
            {
                result = resultCol[counter];
                if (result.Properties.Contains("mail"))
                {

                    displayNameEmailAddress.Add(new UserEmailInfo()
                    {
                        DisplayEmail = result.Properties["mail"][0].ToString(),
                        DisplayName = result.Properties["displayname"][0].ToString()
                    });
                }
            }
        }
        string myJsonString = (new JavaScriptSerializer()).Serialize(displayNameEmailAddress);
        return myJsonString;
    }


}

