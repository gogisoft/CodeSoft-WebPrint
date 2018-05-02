using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CodeSoftPrinterApp.Account
{
    public partial class Login : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        protected void LogIn(object sender, LoginCancelEventArgs e)
        {
            e.Cancel = true;
            LdapAuthentication adAuth = new LdapAuthentication(Login1.UserName, Login1.Password);
            try
            {
                if (adAuth.IsAuthenticated())
                {

                    string groups = adAuth.GetUserGroups();
                    List<String> users = adAuth.GetUsers("AZ-Sec-CSPrint-Admin");
                    string[] groupColl = groups.Split('|');
                    if (users.Exists(p=> Login1.UserName.ToLower().Contains(p.ToLower()))) 
                    {

                        FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(1,
                                      Login1.UserName,
                                      DateTime.Now,
                                      DateTime.Now.AddMinutes(60),
                                      false, groups);
                        // Now encrypt the ticket.
                        string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
                        // Create a cookie and add the encrypted ticket to the
                        // cookie as data.
                        HttpCookie authCookie = new HttpCookie(FormsAuthentication.FormsCookieName,
                                                    encryptedTicket);
                        // Add the cookie to the outgoing cookies collection.
                        Response.Cookies.Add(authCookie);

                        log4net.LogManager.GetLogger("DEBUG").Info("User \"" + Login1.UserName +"\" is now logged in.");

                        //await SignInAsync(user, model.RememberMe);
                        Response.Redirect(HttpContext.Current.Request.QueryString["ReturnUrl"], true);
                    }
                    else
                    {
                        ErrorMessage.Visible = true;
                        ErrorMessage.Text = "You do not have \"AZ-Sec-CSPrint-Admin\" group membership to access this interface!";
                        Login1.FailureText = "You do not have \"AZ-Sec-CSPrint-Admin\" group membership to access this interface!";
                        ExceptionUtility.LogException(new Exception(Login1.UserName + " does not have \"AZ-Sec-CSPrint-Admin\" group membership to access this interface!"));
                    }
                }
                else
                {
                    ErrorMessage.Visible = true;
                    ErrorMessage.Text = "Invalid username or password.";
                    Login1.FailureText = "Invalid username or password.";
                    log4net.LogManager.GetLogger("DEBUG").Info(Login1.UserName + " had invalid username or password.");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Visible = true;
                ErrorMessage.Text = ex.Message;
                Login1.FailureText = ex.Message;
                ExceptionUtility.LogException(ex);
            }
        }
    }
}