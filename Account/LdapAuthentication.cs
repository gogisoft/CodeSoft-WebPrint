using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Web;
using System.Text;
using System.Collections;

namespace CodeSoftPrinterApp.Account
{
    public class LdapAuthentication
    {
        private string _path;
        private string _path2;
        private string _filterAttribute;
        private Hashtable searchedGroups;
        private DirectoryEntry entry;
        private DirectoryEntry entry2;

        public LdapAuthentication(string username, string pwd)
        {
            string ldapconn = "GC://corp/DC=corp";
            _path = ldapconn;
            entry = new DirectoryEntry(_path, username, pwd, AuthenticationTypes.Secure);
        }

        public bool IsAuthenticated()
        {

            try
            {
                // Bind to the native AdsObject to force authentication.
                object obj = entry.NativeObject;
                DirectorySearcher search = new DirectorySearcher(entry);
                search.SearchScope = SearchScope.Subtree;
                search.Filter = "(userPrincipalName=" + entry.Username + ")";
                search.PropertiesToLoad.Add("cn");
                SearchResult result = search.FindOne();
                if (result == null)
                {
                    throw new Exception("Cannot find Windows user " + entry.Username);
                }
                // Update the new path to the user in the directory
                _path = result.Path;
                _filterAttribute = result.Properties["cn"][0].ToString();

            }
            catch
            {
                return false;
            }
            return true;
        }

        public string GetUserGroups()
        {

            DirectorySearcher search = new DirectorySearcher(entry);
            search.Filter = "(cn=" + _filterAttribute + ")";
            search.PropertiesToLoad.Add("memberOf");
            StringBuilder groupNames = new StringBuilder();
            try
            {

                SearchResult result = search.FindOne();
                Int32 propertyCount = result.Properties["memberOf"].Count;
                string dn;
                Int32 equalsIndex;
                Int32 commaIndex;

                for (int propertyCounter = 0; propertyCounter < propertyCount; propertyCounter++)
                {

                    dn = (string)result.Properties["memberOf"][propertyCounter];
                    equalsIndex = dn.IndexOf("=", 1);
                    commaIndex = dn.IndexOf(",", 1);
                    if (equalsIndex == -1)
                    {
                        return null;
                    }
                    groupNames.Append(dn.Substring((equalsIndex + 1), (commaIndex - equalsIndex) - 1));
                    groupNames.Append("|");
                }
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex);
                throw new Exception("Error obtaining group names. " + ex.Message);
            }
            return groupNames.ToString();

        }
        public List<string> GetUsers(string strGroupName)
        {
            DirectorySearcher search = new DirectorySearcher(entry);

            searchedGroups = new Hashtable();
            List<string> groupMembers = new List<string>();

            // find group
            search.Filter = String.Format
                            ("(&(objectCategory=group)(cn={0}))", strGroupName);
            search.PropertiesToLoad.Add("distinguishedName");
            SearchResult sru = null;
            DirectoryEntry group;

            try
            {
                sru = search.FindOne();
                if(sru == null)
                    throw new Exception("Unable to find Windows group \"" + strGroupName + "\"");
            }
            catch (Exception ex)
            {
                throw ex;
            }
            group = sru.GetDirectoryEntry();

            groupMembers = GetUsersInGroup(group.Properties["distinguishedName"].Value.ToString());

            return groupMembers;
        }
        private List<string> GetUsersInGroup(string strGroupDN)
        {
            List<string> groupMembers = new List<string>();
            searchedGroups.Add(strGroupDN, strGroupDN);

            // find all users in this group
            DirectorySearcher ds = new DirectorySearcher(entry);
            ds.Filter = String.Format
                        ("(&(memberOf={0})(objectClass=person))", strGroupDN);

            ds.PropertiesToLoad.Add("distinguishedName");
            ds.PropertiesToLoad.Add("givenname");
            ds.PropertiesToLoad.Add("samaccountname");
            ds.PropertiesToLoad.Add("sn");

            foreach (SearchResult sr in ds.FindAll())
            {
                groupMembers.Add(sr.Properties["samaccountname"][0].ToString());
            }

            // get nested groups
            List<string> al = GetNestedGroups(strGroupDN);
            foreach (object g in al)
            {
                // only if we haven't searched this group before - avoid endless loops
                if (!searchedGroups.ContainsKey(g))
                {
                    // get members in nested group
                    List<string> ml = GetUsersInGroup(g as string);
                    // add them to result list
                    foreach (object s in ml)
                    {
                        groupMembers.Add(s as string);
                    }
                }
            }

            return groupMembers;
        }
        private List<string> GetNestedGroups(string strGroupDN)
        {
            List<string> groupMembers = new List<string>();

            // find all nested groups in this group
            DirectorySearcher ds = new DirectorySearcher(entry);
            ds.Filter = String.Format
                        ("(&(memberOf={0})(objectClass=group))", strGroupDN);

            ds.PropertiesToLoad.Add("distinguishedName");

            foreach (SearchResult sr in ds.FindAll())
            {
                groupMembers.Add(sr.Properties["distinguishedName"][0].ToString());
            }

            return groupMembers;
        }

    }
}