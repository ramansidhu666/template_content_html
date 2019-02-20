using System;
using System.Web;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using HomeHelp.Entity;
using HomeHelp.Services;
using Newtonsoft.Json;
using HomeHelp.Models;
using AutoMapper;
using HomeHelp.Infrastructure;
using HomeHelp.Core.Infrastructure;
using System.Net.Mail;
using System.Web.Configuration;
using System.Configuration;

namespace HomeHelp.Controllers.WebApi
{
    [RoutePrefix("User")]
    public class UserApiController : ApiController
    {
        public IUserService _UserService { get; set; }
        public ICustomerService _CustomerService { get; set; }
        public INotificationService _NotificationService { get; set; }
        public IAgencyIndividualService _AgencyIndividualService { get; set; }
        public UserApiController(IAgencyIndividualService AgencyIndividualService ,IUserService UserService, ICustomerService CustomerServer, INotificationService NotificationService)
        {
            this._AgencyIndividualService = AgencyIndividualService;
            this._UserService = UserService;
            this._CustomerService = CustomerServer;
            this._NotificationService = NotificationService;
        }
        //[Route("GetUsers")]
        //[HttpGet]
        //public IHttpActionResult GetAllUsers()
        //{
        //    var users = _UserService.GetUsers();
        //    var models = new List<UserModel>();
        //    Mapper.CreateMap<HomeHelp.Entity.User, HomeHelp.Models.UserModel>();
        //    foreach (var user in users)
        //    {
        //        models.Add(Mapper.Map<HomeHelp.Entity.User, HomeHelp.Models.UserModel>(user));
        //    }

        //    return Json(models);
        //}


        [Route("ChangePassword")]
        [HttpPost]
        public HttpResponseMessage ChangePassword([FromBody]ChangePasswordsModel changeModel)
        {
            try
            {
                var customer = _CustomerService.GetCustomers().Where(x => x.CustomerId == changeModel.CustomerId && x.IsActive == true).FirstOrDefault();
                if (customer != null)
                {
                    if(customer.Password=="")
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "only manually registered users can change their password."), Configuration.Formatters.JsonFormatter);
                    }
                    var user = _UserService.GetUserById(Convert.ToInt32(customer.UserId));//(customer.UserId);
                    if (user != null)
                    {
                        var sp = _AgencyIndividualService.GetAgencyIndividuals().Where(a => a.UserId == Convert.ToInt32(customer.UserId)).FirstOrDefault();
                        if (SecurityFunction.DecryptString(user.Password) == changeModel.OldPassword)
                        {
                           sp.Password= customer.Password = user.Password = SecurityFunction.EncryptString(changeModel.NewPassword);
                            _UserService.UpdateUser(user);
                            _CustomerService.UpdateCustomer(customer);
                            _AgencyIndividualService.UpdateAgencyIndividual(sp);
                            return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", "Password changed successfully."), Configuration.Formatters.JsonFormatter);
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Wrong old password."), Configuration.Formatters.JsonFormatter);
                        }

                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "User does not exist."), Configuration.Formatters.JsonFormatter);
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "User does not exist."), Configuration.Formatters.JsonFormatter);
                }
            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();
                ErrorLogging.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Please try later."), Configuration.Formatters.JsonFormatter);
            }
        }
        [Route("ForgotPassword")]
        [HttpPost]
        public HttpResponseMessage ForgotPassword([FromBody]ForgetPasswordsModel usermodel)
        {
            try
            {
                if (usermodel.CustomerType == "" || usermodel.CustomerType == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer Type is blank."), Configuration.Formatters.JsonFormatter);
                }
                if (usermodel.CustomerType != EnumValue.GetEnumDescription(EnumValue.CustomerType.Customer) && usermodel.CustomerType != EnumValue.GetEnumDescription(EnumValue.CustomerType.ServiceProvider))
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Wrong Customer Type."), Configuration.Formatters.JsonFormatter);
                }
                if (usermodel.EmailId == "" || usermodel.CustomerType == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer Type is blank."), Configuration.Formatters.JsonFormatter);
                }
                var customer = _CustomerService.GetCustomers().Where(x => x.EmailId == usermodel.EmailId && x.CustomerType == usermodel.CustomerType).FirstOrDefault();
                if (customer != null)
                {

                    var user = _UserService.GetUserById(Convert.ToInt32(customer.UserId));
                    if (user != null)
                    {

                        if (!customer.IsActive)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "User is deactivated."), Configuration.Formatters.JsonFormatter);
                        }
                        //Send Email to User
                        string Password = SecurityFunction.DecryptString(user.Password);
                        SendMailToUser(customer.FirstName + " " + customer.LastName, usermodel.EmailId, Password);

                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", "Password has been sent to your email. Please check your email."), Configuration.Formatters.JsonFormatter);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "User is not found."), Configuration.Formatters.JsonFormatter);
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Incorrect email id."), Configuration.Formatters.JsonFormatter);
                }
            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();
                ErrorLogging.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Please try later."), Configuration.Formatters.JsonFormatter);
            }
        }

        public void SendMailToUser(string UserName, string EmailAddress, string Password)
        {
            try
            {
                // Send mail.
                MailMessage mail = new MailMessage();

                string FromEmailID = WebConfigurationManager.AppSettings["FromEmailID"];
                string FromEmailPassword = WebConfigurationManager.AppSettings["FromEmailPassword"];
                string ToEmailID = WebConfigurationManager.AppSettings["ToEmailID"];

                SmtpClient smtpClient = new SmtpClient(ConfigurationManager.AppSettings["SmtpServer"]);
                int _Port = Convert.ToInt32(WebConfigurationManager.AppSettings["Port"].ToString());
                Boolean _UseDefaultCredentials = Convert.ToBoolean(WebConfigurationManager.AppSettings["UseDefaultCredentials"].ToString());
                Boolean _EnableSsl = Convert.ToBoolean(WebConfigurationManager.AppSettings["EnableSsl"].ToString());


                mail.To.Add(new MailAddress(EmailAddress));
                mail.From = new MailAddress(FromEmailID);
                mail.Subject = "HomeHelp App-Forgot Password";
                string msgbody = "";
                msgbody = msgbody + "<br />";
                msgbody = msgbody + "<table style='width:80%'>";
                msgbody = msgbody + "<tr>";

                msgbody = msgbody + "<td align='left' style=' font-family:Arial; font-weight:bold; font-size:15px;'>You have recently requested for Password Recovery on HomeHelp Mobile App. Please Find Your Password Below:<br /></td></tr>";
                msgbody = msgbody + "<tr><td align='left'>";
                msgbody = msgbody + "<br /><font style=' font-family:Arial; font-size:13px;'><b>Email Address: </b>" + EmailAddress + "</font><br /><br />";
                msgbody = msgbody + "<font style=' font-family:Arial; font-size:13px;'><b>Password: </b>" + Password + "</font><br /><br />";
                msgbody = msgbody + "<br />";
                mail.Body = msgbody;
                mail.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com"; //_Host;
                smtp.Port = _Port;
                smtp.Credentials = new System.Net.NetworkCredential(FromEmailID, FromEmailPassword);// Enter senders User name and password
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.EnableSsl = _EnableSsl;
                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.ToString();
            }
        }

    }
}