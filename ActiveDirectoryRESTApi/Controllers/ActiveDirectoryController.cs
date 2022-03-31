using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.DirectoryServices;
using System.Collections;
using Newtonsoft.Json;
using System.DirectoryServices.AccountManagement;
using System.Web.Script.Serialization;

namespace ActiveDirectoryRESTApi.Controllers
{
    //[RoutePrefix("api/ActiveDirectory")]
    public class ActiveDirectoryController : ApiController
    {

        public string domainpath = "LDAP://DC=domain,DC=local";

        public class Loginparam
        {
            public object username { get; set; }
            public object password { get; set; }
        }


        [HttpGet]
        [ActionName("AllEmailWithDisplayname")]
        public string AllEmailWithDisplayname()
        {
            List<Models.UserEmailInfo> displayNameEmailAddress = new List<Models.UserEmailInfo>();

           
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

                        displayNameEmailAddress.Add(new Models.UserEmailInfo()
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


 

        [HttpPost]
        [ActionName("Authenticate")]
        // POST: api/Authenticate

        public object Authenticate([FromBody()] object Loginvalues)
        {

            var sessionresponse = new Loginparam();
            string rawrequest = Loginvalues.ToString();
            var jsonResulttodict = JsonConvert.DeserializeObject<Dictionary<string, object>>(rawrequest);
            string rsusername = jsonResulttodict["username"].ToString();
            string rspassword = jsonResulttodict["password"].ToString();

            string domainName = System.Environment.UserDomainName;
            string domainUserName = System.Environment.UserName;
            PrincipalContext pc = new PrincipalContext(ContextType.Domain, domainName, domainUserName, ContextOptions.SimpleBind.ToString());
            bool isValid = pc.ValidateCredentials(rsusername.ToUpper(), rspassword);
            if (isValid)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        [HttpPost]
        [ActionName("ShowCredential")]
        // POST:  api/Showcredential

        public object Showcredential([FromBody()] object Loginvalues)
        {

            var sessionresponse = new Loginparam();
            string rawrequest = Loginvalues.ToString();
            var jsonResulttodict = JsonConvert.DeserializeObject<Dictionary<string, object>>(rawrequest);
            string rsusername = jsonResulttodict["username"].ToString();
            string rspassword = jsonResulttodict["password"].ToString();


            return "Username: " + rsusername + " " + "Password: " + rspassword;


        }



    }
}
