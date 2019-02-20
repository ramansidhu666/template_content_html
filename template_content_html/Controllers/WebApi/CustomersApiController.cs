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
using AutoMapper;
using HomeHelp.Models;
using HomeHelp.Infrastructure;
using HomeHelp.Core.Infrastructure;
using System.IO;
using System.Drawing;
using System.Configuration;
using System.Net.Mail;
using System.Web.Configuration;
using HomeHelp.Data;
using OilAndGas.Core.UtilityManager;

namespace HomeHelp.Controllers.WebApi
{
    [RoutePrefix("Customer")]
    public class CustomersApiController : ApiController
    {
        public IUserService _UserService { get; set; }
        public IUserRoleService _UserRoleService { get; set; }
        public ICustomerService _CustomerService { get; set; }
        public ICategoryService _CategoryService { get; set; }
        public ICustomerLocationService _CustomerLocationService { get; set; }
        public IAgencyIndividualService _AgencyIndividualService { get; set; }
        // public INotificationService _NotificationService { get; set; }
        public CustomersApiController(ICustomerLocationService CustomerLocationService, ICategoryService CategoryService, ICustomerService CustomerService, IUserService UserService,
            IUserRoleService UserRoleService, IAgencyIndividualService AgencyIndividualService)
        {
            this._CustomerLocationService = CustomerLocationService;
            this._CategoryService = CategoryService;
            this._CustomerService = CustomerService;
            this._UserService = UserService;
            this._UserRoleService = UserRoleService;
            this._AgencyIndividualService = AgencyIndividualService;
            //this._NotificationService = NotificationService;
        }

        //[Route("GetAllCustomers")]
        //[HttpGet]
        //public HttpResponseMessage GetAllCustomers()
        //{
        //    try
        //    {
        //        int guestId =Convert.ToInt32( WebConfigurationManager.AppSettings["GuestCustomerId"]);
        //        var Customers = _CustomerService.GetCustomers().Where(c => c.IsActive == true && c.CustomerId != guestId);

        //        var models = new List<CustomerModel>();
        //        Mapper.CreateMap<HomeHelp.Entity.Customer, HomeHelp.Models.CustomerModel>();
        //        foreach (var Customer in Customers)
        //        {
        //            var CustomerModel = Mapper.Map<HomeHelp.Entity.Customer, HomeHelp.Models.CustomerModel>(Customer);
        //            models.Add(CustomerModel);
        //        }

        //        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", models), Configuration.Formatters.JsonFormatter);
        //    }
        //    catch (Exception ex)
        //    {
        //        string ErrorMsg = ex.Message.ToString();
        //        ErrorLogging.LogError(ex);
        //        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "User not found."), Configuration.Formatters.JsonFormatter);
        //    }

        //}

        [Route("GetCustomerByID")]
        [HttpGet]
        public HttpResponseMessage GetCustomerByID([FromUri] Guid CustomerId)
        {
            try
            {
                var Customer = _CustomerService.GetCustomer(CustomerId);
                if (Customer != null)
                {
                    if (Customer.IsActive)
                    {
                        Mapper.CreateMap<HomeHelp.Entity.Customer, HomeHelp.Models.UserCustomerModel>();
                        UserCustomerModel CustomerResponseModel = Mapper.Map<HomeHelp.Entity.Customer, HomeHelp.Models.UserCustomerModel>(Customer);
                        if (Customer.CustomerType == EnumValue.GetEnumDescription(EnumValue.CustomerType.ServiceProvider))
                        {
                            CustomerResponseModel.CategoryName = _CategoryService.GetCategory(Convert.ToInt32(Customer.CategoryId)).Name;
                        }
                        else
                        {
                            CustomerResponseModel.CategoryName = "";
                        }

                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", CustomerResponseModel), Configuration.Formatters.JsonFormatter);

                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "User not found."), Configuration.Formatters.JsonFormatter);
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "User not found."), Configuration.Formatters.JsonFormatter);
                }

            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();
                ErrorLogging.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "User not found."), Configuration.Formatters.JsonFormatter);
            }
        }


        //[Route("GetGuestCustomer")]
        //[HttpGet]
        //public HttpResponseMessage GetCustomerByID()
        //{
        //    try
        //    {
        //        int guestId = Convert.ToInt32(WebConfigurationManager.AppSettings["GuestCustomerId"]);
        //        var Customer = _CustomerService.GetCustomer(guestId);
        //        if (Customer != null)
        //        {
        //            if (Customer.IsActive)
        //            {
        //                Mapper.CreateMap<HomeHelp.Entity.Customer, HomeHelp.Models.NewResponseModel>();
        //                NewResponseModel CustomerResponseModel = Mapper.Map<HomeHelp.Entity.Customer, HomeHelp.Models.NewResponseModel>(Customer);
        //                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", CustomerResponseModel), Configuration.Formatters.JsonFormatter);

        //            }
        //            else
        //            {
        //                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "User not found."), Configuration.Formatters.JsonFormatter);
        //            }
        //        }
        //        else
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "User not found."), Configuration.Formatters.JsonFormatter);
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        string ErrorMsg = ex.Message.ToString();
        //        ErrorLogging.LogError(ex);
        //        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "User not found."), Configuration.Formatters.JsonFormatter);
        //    }
        //}

        public HttpResponseMessage ErrorMessage(string status, object message)
        {
            return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage(status, message), Configuration.Formatters.JsonFormatter);
        }
        [Route("SaveCustomer")]
        [HttpPost]
        public HttpResponseMessage SaveCustomer([FromBody]CustomerModel CustomerModel)
        {
            string CustomerID = "-1";
            int UserID = 0;
            try
            {
                if (CustomerModel == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Model is blank."), Configuration.Formatters.JsonFormatter);
                }

                if (CustomerModel.ApplicationId == null || CustomerModel.ApplicationId == "")
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Application Id is blank."), Configuration.Formatters.JsonFormatter);
                }
                if (CustomerModel.DeviceType == null || CustomerModel.DeviceType == "")
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Device Type is blank."), Configuration.Formatters.JsonFormatter);
                }
                else if ((CustomerModel.DeviceType != EnumValue.GetEnumDescription(EnumValue.DeviceType.Android)) && (CustomerModel.DeviceType != EnumValue.GetEnumDescription(EnumValue.DeviceType.Ios)))
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Device Type is incorrect."), Configuration.Formatters.JsonFormatter);
                }
                if (CustomerModel.IsSocial == null || CustomerModel.IsSocial == "")
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Mention IsSocial."), Configuration.Formatters.JsonFormatter);
                }
                if (CustomerModel.MobileNumber == null || CustomerModel.MobileNumber == "")
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Mobile Number is blank."), Configuration.Formatters.JsonFormatter);
                }
                if (CustomerModel.Address == null || CustomerModel.Address == "")
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Address is blank."), Configuration.Formatters.JsonFormatter);
                }
                //if (CustomerModel.AccountType == null && CustomerModel.AccountType == "")
                //{
                //    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Mention Account type."), Configuration.Formatters.JsonFormatter);
                //}
                if (CustomerModel.IsSocial != EnumValue.GetEnumDescription(EnumValue.SignUp.Social) && CustomerModel.IsSocial != EnumValue.GetEnumDescription(EnumValue.SignUp.Manual))
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Wrong IsSocial."), Configuration.Formatters.JsonFormatter);
                }
                if (CustomerModel.CustomerType == null || CustomerModel.CustomerType == "")
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer Type is blank."), Configuration.Formatters.JsonFormatter);
                }
                if (CustomerModel.CustomerType != EnumValue.GetEnumDescription(EnumValue.CustomerType.Customer) && CustomerModel.CustomerType != EnumValue.GetEnumDescription(EnumValue.CustomerType.ServiceProvider))
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Wrong Customer Type."), Configuration.Formatters.JsonFormatter);
                }
                if (CustomerModel.IsSocial == EnumValue.GetEnumDescription(EnumValue.SignUp.Manual))
                {
                    if (CustomerModel.Password == null || CustomerModel.Password == "")
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Password is blank."), Configuration.Formatters.JsonFormatter);
                    }
                    if (CustomerModel.EmailId == null || CustomerModel.EmailId == "")
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Email Id is blank."), Configuration.Formatters.JsonFormatter);
                    }
                }
                else
                {
                    if (CustomerModel.FacebookId == null || CustomerModel.FacebookId == "")
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Facebook Id is blank."), Configuration.Formatters.JsonFormatter);
                    }
                }
                if (CustomerModel.CustomerType == EnumValue.GetEnumDescription(EnumValue.CustomerType.ServiceProvider))
                {
                    if (CustomerModel.CategoryId == 0 || CustomerModel.CategoryId == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Category Id is blank."), Configuration.Formatters.JsonFormatter);
                    }
                    if (CustomerModel.WorkRate == 0)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Work Rate is blank."), Configuration.Formatters.JsonFormatter);
                    }

                }
                if (CustomerModel.Latitude == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Latitude is blank."), Configuration.Formatters.JsonFormatter);
                }
                if (CustomerModel.Longitude == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Longitude is blank."), Configuration.Formatters.JsonFormatter);
                }
                Customer userFound = new Entity.Customer();
                Mapper.CreateMap<HomeHelp.Models.CustomerModel, HomeHelp.Entity.Customer>();
                HomeHelp.Entity.Customer Customer = Mapper.Map<HomeHelp.Models.CustomerModel, HomeHelp.Entity.Customer>(CustomerModel);
                if (CustomerModel.IsSocial == EnumValue.GetEnumDescription(EnumValue.SignUp.Manual))
                {
                    //userFound = _UserService.ValidateUserByEmail(Customer.EmailId, Customer.Password);
                    userFound = _CustomerService.GetCustomers().Where(e => e.EmailId.ToLower() == Customer.EmailId.ToLower() && e.CustomerType.ToLower() == Customer.CustomerType.ToLower()).FirstOrDefault();
                }
                else
                {
                    userFound = _CustomerService.GetCustomers().Where(e => e.FacebookId.ToLower() == Customer.FacebookId.ToLower() && e.CustomerType.ToLower() == Customer.CustomerType.ToLower()).FirstOrDefault();
                }

                if (userFound == null)
                {
                    Customer.CompanyId = 2;
                    HomeHelp.Entity.User user = new HomeHelp.Entity.User();
                    user.CompanyId = Customer.CompanyId;
                    user.FirstName = CustomerModel.FirstName;
                    user.LastName = CustomerModel.LastName;

                    if (CustomerModel.IsSocial == EnumValue.GetEnumDescription(EnumValue.SignUp.Manual))
                    {
                        user.EmailId = CustomerModel.EmailId;
                        user.Password = SecurityFunction.EncryptString(CustomerModel.Password);
                        user.FacebookId = "";
                    }
                    else
                    {
                        user.FacebookId = CustomerModel.FacebookId;
                        user.EmailId = CustomerModel.EmailId;
                        user.Password = "";
                    }
                    _UserService.InsertUser(user);

                    UserID = user.UserId;
                    if (user.UserId > 0)
                    {
                        HomeHelp.Entity.UserRole userRole = new HomeHelp.Entity.UserRole();
                        userRole.UserId = user.UserId;
                        userRole.RoleId = 3;
                        _UserRoleService.InsertUserRole(userRole);

                        //Insert the Customer
                        Customer.FirstName = CustomerModel.FirstName;
                        Customer.LastName = CustomerModel.LastName;
                        Customer.UserId = user.UserId;

                        if (CustomerModel.IsSocial == EnumValue.GetEnumDescription(EnumValue.SignUp.Social))
                        {
                            Customer.FacebookId = CustomerModel.FacebookId;
                            Customer.EmailId = "";
                            Customer.Password = "";
                            Customer.EmailVerifyCode = "";
                            Customer.IsEmailVerified = true;
                            Customer.IsActive = true;
                        }
                        else
                        {
                            Customer.EmailId = CustomerModel.EmailId;
                            Customer.Password = SecurityFunction.EncryptString(CustomerModel.Password);
                            Customer.FacebookId = "";
                            Customer.EmailVerifyCode = CommonCls.GetNumericCode();
                            Customer.IsEmailVerified = false;
                            Customer.IsActive = false;
                        }

                        Customer.CustomerType = CustomerModel.CustomerType;
                        Customer.ApplicationId = CustomerModel.ApplicationId;
                        Customer.DeviceSerialNo = CustomerModel.DeviceSerialNo;
                        Customer.DeviceType = CustomerModel.DeviceType;
                        Customer.MobileNumber = CustomerModel.MobileNumber;
                        Customer.Address = CustomerModel.Address;
                        Customer.Latitude = CustomerModel.Latitude;
                        Customer.Longitude = CustomerModel.Longitude;

                        if (Customer.CustomerType == EnumValue.GetEnumDescription(EnumValue.CustomerType.ServiceProvider))
                        {
                            Customer.CategoryId = CustomerModel.CategoryId;
                            Customer.WorkRate = CustomerModel.WorkRate;

                        }
                        _CustomerService.InsertCustomer(Customer);
                        CustomerLocation location = new CustomerLocation();
                        location.Latitude = CustomerModel.Latitude;
                        location.Longitude = CustomerModel.Longitude;
                        location.CustomerId = Customer.CustomerId;
                        _CustomerLocationService.InsertCustomerLocation(location);
                        if (CustomerModel.IsSocial != EnumValue.GetEnumDescription(EnumValue.SignUp.Social))
                        {
                            SendMailToUser(Customer.FirstName + " " + Customer.LastName, CustomerModel.EmailId, Customer.EmailVerifyCode);
                        }
                        CustomerID = Customer.CustomerId.ToString();

                        var customers = _CustomerService.GetBy(x => new
                        {
                            x.CustomerId,
                            x.ApplicationId,
                            x.DeviceSerialNo

                        }, x => x.CustomerId != Customer.CustomerId && x.ApplicationId == CustomerModel.ApplicationId);
                        foreach (var custormer in customers)
                        {
                            var customer = _CustomerService.GetCustomer(custormer.CustomerId);
                            customer.ApplicationId = "";
                            customer.DeviceSerialNo = "";
                            customer.DeviceType = "";
                            _CustomerService.UpdateCustomer(customer);
                        }
                        //End : Insert the Customer

                        //START: INSERT AGENCY-INDIVIDUAL


                        if (Customer.CustomerType == EnumValue.GetEnumDescription(EnumValue.CustomerType.ServiceProvider))
                        {
                            AgencyIndividual agencyIndividual = new AgencyIndividual();
                            agencyIndividual.FullName = CustomerModel.FirstName;
                            agencyIndividual.Address = CustomerModel.Address;
                            agencyIndividual.CategoryId = CustomerModel.CategoryId;
                            agencyIndividual.ContactNumber = CustomerModel.MobileNumber;
                            agencyIndividual.EmailId = CustomerModel.EmailId;
                            agencyIndividual.IsActive = true;
                            agencyIndividual.IsAgency = false;
                            agencyIndividual.ParentId = new Guid();
                            agencyIndividual.Password = SecurityFunction.EncryptString(CustomerModel.Password);
                            agencyIndividual.UserId = user.UserId;
                            agencyIndividual.WorkRate = CustomerModel.WorkRate;
                            agencyIndividual.CreatedOn = DateTime.Now;
                            agencyIndividual.LastUpdatedOn = DateTime.Now;
                            agencyIndividual.Latitude = CustomerModel.Latitude;
                            agencyIndividual.Longitude = CustomerModel.Longitude;
                            agencyIndividual.IsInvited = false;
                            agencyIndividual.PhotoPath = CustomerModel.PhotoPath;
                            _AgencyIndividualService.InsertAgencyIndividual(agencyIndividual);

                        }
                        //END: INSERT AGENCY-INDIVIDUAL

                    }
                    //Send Verify Code to Customer
                    //if (Customer.IsEmailVerified != null && CustomerModel.CustomerType != "facebook")
                    //{
                    //    if (Customer.IsEmailVerified == false)
                    //    {
                    //SendMailToAdmin(Customer.FirstName, Customer.EmailId);
                    //    }
                    //}
                    //End : Send Verify Code to Customer
                    Mapper.CreateMap<HomeHelp.Entity.Customer, HomeHelp.Models.NewResponseModel>();
                    NewResponseModel customerResponseModel = Mapper.Map<HomeHelp.Entity.Customer, HomeHelp.Models.NewResponseModel>(Customer);
                    customerResponseModel.flag = "New user.";
                    customerResponseModel.CustomerId = new Guid(CustomerID);
                    //customerResponseModel.flag = "New user.";
                    customerResponseModel.IsSocial = CustomerModel.IsSocial;
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", customerResponseModel), Configuration.Formatters.JsonFormatter);


                }
                else
                {
                    //var customerUpdate = _CustomerService.GetCustomers().Where(x => x.UserId == userFound.UserId).FirstOrDefault();
                    //Mapper.CreateMap<HomeHelp.Entity.Customer, HomeHelp.Models.NewResponseModel>();
                    //NewResponseModel customerResponseModel = Mapper.Map<HomeHelp.Entity.Customer, HomeHelp.Models.NewResponseModel>(customerUpdate);

                    //customerUpdate.ApplicationId = customerResponseModel.ApplicationId;
                    //customerUpdate.DeviceSerialNo = customerResponseModel.DeviceSerialNo;
                    //customerUpdate.DeviceType = customerResponseModel.DeviceType;
                    //customerUpdate.Latitude = (customerResponseModel.Latitude == 0 ? customerUpdate.Latitude : CustomerModel.Latitude);
                    //customerUpdate.Longitude = (customerResponseModel.Longitude == 0 ? customerUpdate.Longitude : CustomerModel.Longitude);

                    //_CustomerService.UpdateCustomer(customerUpdate);

                    //customerResponseModel.ApplicationId = CustomerModel.ApplicationId;
                    //customerResponseModel.flag = "Existed user.";

                    //customerResponseModel.IsSocial = CustomerModel.IsSocial;
                    Mapper.CreateMap<HomeHelp.Entity.Customer, HomeHelp.Models.NewResponseModel>();
                    NewResponseModel customerResponseModel = Mapper.Map<HomeHelp.Entity.Customer, HomeHelp.Models.NewResponseModel>(userFound);
                    customerResponseModel.IsSocial = CustomerModel.IsSocial;
                    customerResponseModel.flag = "Existed user.";
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", customerResponseModel), Configuration.Formatters.JsonFormatter);
                }
            }
            catch (Exception ex)
            {
                var UserRole = _UserRoleService.GetUserRoles().Where(x => x.UserId == UserID).FirstOrDefault();
                if (UserRole != null)
                {
                    _UserRoleService.DeleteUserRole(UserRole); // delete user role
                }
                var User = _UserService.GetUsers().Where(x => x.UserId == UserID).FirstOrDefault();
                if (User != null)
                {
                    _UserService.DeleteUser(User); // delete user 
                }
                string ErrorMsg = ex.Message.ToString();
                ErrorLogging.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Please try later."), Configuration.Formatters.JsonFormatter);
            }
        }

        public void SendMailToUser(string UserName, string EmailAddress, string Code)
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
                mail.Subject = "HomeHelp App-Verification Code";

                string msgbody = "";
                msgbody = msgbody + "<br />";
                msgbody = msgbody + "<table style='width:80%'>";
                msgbody = msgbody + "<tr>";
                msgbody = msgbody + "<td align='left' style=' font-family:Arial; font-size:15px;'>Hi " + UserName + ", <br /></td></tr>";
                msgbody = msgbody + "<br /><font style=' font-family:Arial; font-size:13px;'>Thanks for signing up!Your verification code is " + Code + " Please verify your account.</font><br /><br />";
                //msgbody = msgbody + "<font style=' font-family:Arial; font-size:13px;'>Please find your password below:</font>";

                //msgbody = msgbody + "<tr><td align='left'>";
                //msgbody = msgbody + "<br /><font style=' font-family:Arial; font-size:13px;'>Email Address: " + EmailAddress + "</font><br /><br />";
                //msgbody = msgbody + "<font style=' font-family:Arial; font-size:13px;'>Password: " + Password + "</font>";
                msgbody = msgbody + "<br /><br />";
                msgbody = msgbody + "<br /><font style=' font-family:Arial; font-size:13px;'>Thanks,</font><br /><br />";
                msgbody = msgbody + "<font style=' font-family:Arial; font-size:13px;'>Home help team.</font>";

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
        [Route("ValidateUserCustomer")]
        [HttpPost]
        public HttpResponseMessage ValidateUserCustomer([FromBody]CustomerModel userCustomer)
        {
            string UserID = "-1";
            try
            {
                if (userCustomer.IsSocial == null || userCustomer.IsSocial == "")
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Mention IsSocial."), Configuration.Formatters.JsonFormatter);
                }
                if (userCustomer.IsSocial != EnumValue.GetEnumDescription(EnumValue.SignUp.Social) && userCustomer.IsSocial != EnumValue.GetEnumDescription(EnumValue.SignUp.Manual))
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Wrong IsSocial."), Configuration.Formatters.JsonFormatter);
                }
                if (userCustomer.IsSocial == EnumValue.GetEnumDescription(EnumValue.SignUp.Manual))
                {
                    if (userCustomer.Password == null || userCustomer.Password == "")
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Password is blank."), Configuration.Formatters.JsonFormatter);
                    }
                    if (userCustomer.EmailId == null || userCustomer.EmailId == "")
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Email Id is blank."), Configuration.Formatters.JsonFormatter);
                    }
                }
                else
                {
                    if (userCustomer.FacebookId == null || userCustomer.FacebookId == "")
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Facebook Id is blank."), Configuration.Formatters.JsonFormatter);
                    }
                }
                HomeHelp.Entity.Customer users = null;
                if (userCustomer.IsSocial == EnumValue.GetEnumDescription(EnumValue.SignUp.Social))
                {
                    users = _CustomerService.GetCustomers().Where(c => c.FacebookId.ToLower() == userCustomer.FacebookId.ToLower() && c.CustomerType.ToLower() == userCustomer.CustomerType.ToLower()).FirstOrDefault();

                }
                else
                {
                    string password = SecurityFunction.EncryptString(userCustomer.Password);
                    users = _CustomerService.GetCustomers().Where(e => e.EmailId.ToLower() == userCustomer.EmailId.ToLower() && e.Password == password && e.CustomerType.ToLower() == userCustomer.CustomerType.ToLower()).FirstOrDefault();

                }


                if (users != null)
                {
                    UserID = users.UserId.ToString();
                    HomeHelp.Entity.Customer customer = _CustomerService.GetCustomers().Where(x => x.UserId == users.UserId).FirstOrDefault();
                    if (customer == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "User is not found."), Configuration.Formatters.JsonFormatter);
                    }

                    //if ((userCustomer.DeviceSerialNo != null) && (userCustomer.DeviceSerialNo != ""))
                    //{
                    //    customer.DeviceSerialNo = userCustomer.DeviceSerialNo; //Reset the Value
                    //}
                    //else
                    //{
                    //    userCustomer.DeviceSerialNo = customer.DeviceSerialNo; //Fetch from DB
                    //}

                    //if (userCustomer.ApplicationId != null && userCustomer.ApplicationId != "")
                    //{
                    //    customer.ApplicationId = userCustomer.ApplicationId;
                    //}

                    ////if (userCustomer.DeviceType != null && userCustomer.DeviceType != "")
                    ////{
                    ////    customer.DeviceType = userCustomer.DeviceType;
                    ////}
                    //_CustomerService.UpdateCustomer(customer);

                    //Mapper.CreateMap<HomeHelp.Entity.Customer, HomeHelp.Models.CustomerModel>();
                    //CustomerModel CustomerModel = Mapper.Map<HomeHelp.Entity.Customer, HomeHelp.Models.CustomerModel>(Customer);
                    if (customer.IsEmailVerified)
                    {
                        Mapper.CreateMap<HomeHelp.Entity.Customer, HomeHelp.Models.NewResponseModel>();
                        NewResponseModel CustomerResponseModel = Mapper.Map<HomeHelp.Entity.Customer, HomeHelp.Models.NewResponseModel>(customer);
                        CustomerResponseModel.IsSocial = userCustomer.IsSocial;

                        //Update all applicationId
                        var CustomersList = _CustomerService.GetCustomers().Where(c => c.CustomerId != customer.CustomerId && c.ApplicationId == customer.ApplicationId).ToList();
                        foreach (var Customers in CustomersList)
                        {
                            Customers.ApplicationId = "";
                            Customers.DeviceSerialNo = "";
                            Customers.DeviceType = "";
                            _CustomerService.UpdateCustomer(Customers);
                        }
                        //End

                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", CustomerResponseModel), Configuration.Formatters.JsonFormatter);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Please verify your account."), Configuration.Formatters.JsonFormatter);
                    }


                }
                else
                {
                    if ((users == null) && (userCustomer.IsSocial == EnumValue.GetEnumDescription(EnumValue.SignUp.Manual)))
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Incorrect emailid or password."), Configuration.Formatters.JsonFormatter);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Facebook Id not found."), Configuration.Formatters.JsonFormatter);
                    }
                }
            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();
                ErrorLogging.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Please try later."), Configuration.Formatters.JsonFormatter);
            }
        }


        [Route("ValidateEmailCode")]
        [HttpPost]
        public HttpResponseMessage ValidateEmailCode([FromBody]ValidateEmailCodeModel ValidateEmailCodeModel)
        {
            try
            {
                var Customer = _CustomerService.GetCustomer(ValidateEmailCodeModel.CustomerId);
                if (Customer != null)
                {
                    if (Customer.EmailVerifyCode == ValidateEmailCodeModel.EmailVerifyCode)
                    {
                        Customer.IsEmailVerified = true;
                        Customer.IsActive = true;
                        _CustomerService.UpdateCustomer(Customer); //Update IsMobileVerified & IsActive  Operation

                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", "Email is verified."), Configuration.Formatters.JsonFormatter);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Please enter correct code."), Configuration.Formatters.JsonFormatter);
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Please try later."), Configuration.Formatters.JsonFormatter);
                }
            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();
                ErrorLogging.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Please try later."), Configuration.Formatters.JsonFormatter);
            }
        }


        [Route("UpdateProfile")]
        [HttpPost]
        public HttpResponseMessage UpdateProfile([FromBody]CustomerModel userCustomer)
        {
            try
            {
                HomeHelp.Entity.Customer Customer = _CustomerService.GetCustomers().Where(x => x.CustomerId == userCustomer.CustomerId).FirstOrDefault();
                if (Customer != null)
                {
                    //Update the User Table

                    HomeHelp.Entity.User user = _UserService.GetUsers().Where(c => c.UserId == Customer.UserId).FirstOrDefault();
                    user.FirstName = ((userCustomer.FirstName != null && userCustomer.FirstName != "") ? userCustomer.FirstName : user.FirstName);
                    user.LastName = ((userCustomer.LastName != null && userCustomer.LastName != "") ? userCustomer.LastName : user.LastName);
                    _UserService.UpdateUser(user);

                    //End : Update the User Table

                    //Update the Customer Table
                    Customer.FirstName = ((userCustomer.FirstName != null && userCustomer.FirstName != "") ? userCustomer.FirstName : Customer.FirstName);
                    Customer.LastName = ((userCustomer.LastName != null && userCustomer.LastName != "") ? userCustomer.LastName : Customer.LastName);
                    Customer.Address = ((userCustomer.Address != null && userCustomer.Address != "") ? userCustomer.Address : Customer.Address);
                    Customer.MobileNumber = ((userCustomer.MobileNumber != null && userCustomer.MobileNumber != "") ? userCustomer.MobileNumber : Customer.MobileNumber);
                    Customer.Latitude = ((userCustomer.Latitude != 0) ? userCustomer.Latitude : Customer.Latitude);
                    Customer.Longitude = ((userCustomer.Longitude != 0) ? userCustomer.Longitude : Customer.Longitude);

                    if (Customer.CustomerType == EnumValue.GetEnumDescription(EnumValue.CustomerType.ServiceProvider))
                    {
                        Customer.CategoryId = ((userCustomer.CategoryId != 0) ? userCustomer.CategoryId : Customer.CategoryId);

                        Customer.WorkRate = ((userCustomer.WorkRate != 0) ? userCustomer.WorkRate : Customer.WorkRate);

                    }

                    _CustomerService.UpdateCustomer(Customer);

                    var agencyIndividual = _AgencyIndividualService.GetAgencyIndividualByUserId(Convert.ToInt32( Customer.UserId));
                    if(agencyIndividual!=null)
                    {
                        agencyIndividual.FullName = ((userCustomer.FirstName != null && userCustomer.FirstName != "") ? userCustomer.FirstName : agencyIndividual.FullName);
                        agencyIndividual.Address = ((userCustomer.Address != null && userCustomer.Address != "") ? userCustomer.Address : agencyIndividual.Address);
                        agencyIndividual.ContactNumber = ((userCustomer.MobileNumber != null && userCustomer.MobileNumber != "") ? userCustomer.MobileNumber : agencyIndividual.ContactNumber);
                        agencyIndividual.Latitude = ((userCustomer.Latitude != 0) ? userCustomer.Latitude : agencyIndividual.Latitude);
                        agencyIndividual.Longitude = ((userCustomer.Longitude != 0) ? userCustomer.Longitude : agencyIndividual.Longitude);

                        if (Customer.CustomerType == EnumValue.GetEnumDescription(EnumValue.CustomerType.ServiceProvider))
                        {
                            agencyIndividual.CategoryId = ((userCustomer.CategoryId != 0) ? userCustomer.CategoryId : agencyIndividual.CategoryId);

                            agencyIndividual.WorkRate = ((userCustomer.WorkRate != 0) ? userCustomer.WorkRate : agencyIndividual.WorkRate);

                        }
                        _AgencyIndividualService.UpdateAgencyIndividual(agencyIndividual);
                    }
                    //End : Update the Customer Table

                    Mapper.CreateMap<HomeHelp.Entity.Customer, HomeHelp.Models.UserCustomerModel>();
                    UserCustomerModel CustomerResponseModel = Mapper.Map<HomeHelp.Entity.Customer, HomeHelp.Models.UserCustomerModel>(Customer);
                    if (Customer.CustomerType == EnumValue.GetEnumDescription(EnumValue.CustomerType.ServiceProvider))
                    {
                        CustomerResponseModel.CategoryName = _CategoryService.GetCategory(Convert.ToInt32(Customer.CategoryId)).Name;
                    }
                    else
                    {
                        CustomerResponseModel.CategoryName = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", CustomerResponseModel), Configuration.Formatters.JsonFormatter);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "User is not found."), Configuration.Formatters.JsonFormatter);
                }
            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();
                ErrorLogging.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Please try later."), Configuration.Formatters.JsonFormatter);
            }
        }
        //public string SaveImage(string Base64String)
        //{
        //    string fileName = Guid.NewGuid() + ".png";
        //    Image image = CommonCls.Base64ToImage(Base64String);
        //    var subPath = HttpContext.Current.Server.MapPath("~/CustomerPhoto");
        //    var path = Path.Combine(subPath, fileName);
        //    image.Save(path, System.Drawing.Imaging.ImageFormat.Png);

        //    string URL = CommonCls.GetURL() + "/CustomerPhoto/" + fileName;
        //    return URL;
        //}
        //public void DeleteImage(string filePath)
        //{
        //    try
        //    {
        //        var uri = new Uri(filePath);
        //        var fileName = Path.GetFileName(uri.AbsolutePath);
        //        var subPath = HttpContext.Current.Server.MapPath("~/CustomerPhoto");
        //        var path = Path.Combine(subPath, fileName);

        //        FileInfo file = new FileInfo(path);
        //        if (file.Exists)
        //        {
        //            file.Delete();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        string ErrorMsg = ex.Message.ToString();
        //        ErrorLogging.LogError(ex);
        //    }
        //}

        //public void SendMailToAdmin(string UserName, string EmailAddress)
        //{
        //    try
        //    {
        //        // Send mail.
        //        MailMessage mail = new MailMessage();

        //        string FromEmailID = WebConfigurationManager.AppSettings["FromEmailID"];
        //        string FromEmailPassword = WebConfigurationManager.AppSettings["FromEmailPassword"];
        //        string ToEmailID = WebConfigurationManager.AppSettings["ToEmailID"];

        //        SmtpClient smtpClient = new SmtpClient(ConfigurationManager.AppSettings["SmtpServer"]);
        //        int _Port = Convert.ToInt32(WebConfigurationManager.AppSettings["Port"].ToString());
        //        Boolean _UseDefaultCredentials = Convert.ToBoolean(WebConfigurationManager.AppSettings["UseDefaultCredentials"].ToString());
        //        Boolean _EnableSsl = Convert.ToBoolean(WebConfigurationManager.AppSettings["EnableSsl"].ToString());

        //        mail.To.Add(new MailAddress(ToEmailID));

        //        mail.From = new MailAddress(FromEmailID);
        //        mail.Subject = "New registeration";
        //        string msgbody = "";
        //        msgbody = msgbody + "<br />";
        //        msgbody = msgbody + "<table style='width:80%'>";
        //        msgbody = msgbody + "<tr>";

        //        msgbody = msgbody + "<td align='left' style=' font-family:Arial; font-weight:bold; font-size:15px;'>New user have recently registered on HomeHelp App. Please Find Your user details below:<br /></td></tr>";
        //        msgbody = msgbody + "<tr><td align='left'>";
        //        msgbody = msgbody + "<br /><font style=' font-family:Arial; font-size:13px;'><b>User Name: </b>" + UserName + "</font><br /><br />";
        //        msgbody = msgbody + "<font style=' font-family:Arial; font-size:13px;'><b>EmailAddress: </b>" + EmailAddress + "</font><br /><br />";

        //        msgbody = msgbody + "<br />";
        //        mail.Body = msgbody;
        //        mail.IsBodyHtml = true;

        //        SmtpClient smtp = new SmtpClient();
        //        smtp.Host = "smtp.gmail.com"; //_Host;
        //        smtp.Port = _Port;

        //        smtp.Credentials = new System.Net.NetworkCredential(FromEmailID, FromEmailPassword);// Enter senders User name and password
        //        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
        //        smtp.EnableSsl = _EnableSsl;
        //        smtp.Send(mail);
        //    }
        //    catch (Exception ex)
        //    {
        //        string ErrorMsg = ex.ToString();
        //    }
        //}

        //[Route("Logout")]
        //[HttpPost]
        //public HttpResponseMessage Logout([FromBody]Logout Logout)
        //{
        //    try
        //    {
        //        HomeHelp.Entity.Customer Customer = _CustomerService.GetCustomer(Logout.CustomerId);
        //        if (Customer != null)
        //        {
        //            Customer.DeviceSerialNo = "";
        //            Customer.ApplicationId = "";
        //            _CustomerService.UpdateCustomer(Customer);
        //            return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", "Logout successfully"), Configuration.Formatters.JsonFormatter);
        //        }
        //        else
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "No user found."), Configuration.Formatters.JsonFormatter);
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //        string ErrorMsg = ex.Message.ToString();
        //        ErrorLogging.LogError(ex);
        //        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Please try later."), Configuration.Formatters.JsonFormatter);
        //    }
        //}


        [Route("UpdateApplicationId")]
        [HttpPost]
        public HttpResponseMessage UpdateApplicationId([FromBody]UserCustomerModel userCustomer)
        {
            try
            {
                if (userCustomer.DeviceType == null || userCustomer.DeviceType == "")
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Device Type is blank."), Configuration.Formatters.JsonFormatter);
                }
                if ((userCustomer.DeviceType != EnumValue.GetEnumDescription(EnumValue.DeviceType.Android)) && (userCustomer.DeviceType != EnumValue.GetEnumDescription(EnumValue.DeviceType.Ios)))
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Device Type is incorrect."), Configuration.Formatters.JsonFormatter);
                }
                if (userCustomer.ApplicationId == null || userCustomer.ApplicationId == "")
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Application Id is blank."), Configuration.Formatters.JsonFormatter);
                }
                if (userCustomer.CustomerId == new Guid())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer Id is blank."), Configuration.Formatters.JsonFormatter);
                }
                HomeHelp.Entity.Customer Customer = _CustomerService.GetCustomers().Where(x => x.CustomerId == userCustomer.CustomerId).FirstOrDefault();
                if (Customer != null)
                {
                    if (!Customer.IsActive)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "User is deactivated."), Configuration.Formatters.JsonFormatter);
                    }
                    Customer.ApplicationId = userCustomer.ApplicationId;
                    Customer.DeviceType = userCustomer.DeviceType;
                    _CustomerService.UpdateCustomer(Customer);
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", "Successfully updated."), Configuration.Formatters.JsonFormatter);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "User is not found."), Configuration.Formatters.JsonFormatter);
                }
            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();
                ErrorLogging.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Please try later."), Configuration.Formatters.JsonFormatter);
            }
        }

        //[Route("GetNearByProvider")]
        //[HttpPost]
        //public HttpResponseMessage GetNearByProvider(CustomerModel userCustomer)
        //{
        //    try
        //    {
        //        if (userCustomer.Longitude == 0)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Longitude is blank."), Configuration.Formatters.JsonFormatter);
        //        }
        //        if (userCustomer.Latitude == 0)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Latitude is blank."), Configuration.Formatters.JsonFormatter);
        //        }
        //        var customers = _CustomerService.GetCustomers().Where(c=>c.CustomerType==EnumValue.GetEnumDescription(EnumValue.CustomerType.ServiceProvider)).ToList();
        //        if (customers.Count() > 0)
        //        {

        //            double TotalDistance = userCustomer.DistanceRange;
        //            List<UserCustomerModel> obj = new List<UserCustomerModel>();
        //            foreach (var customer in customers)
        //            {
        //                Mapper.CreateMap<HomeHelp.Entity.Customer, HomeHelp.Models.UserCustomerModel>();
        //                HomeHelp.Models.UserCustomerModel NewResponseModel = Mapper.Map<HomeHelp.Entity.Customer, HomeHelp.Models.UserCustomerModel>(customer);
        //                if (userCustomer.DistanceRange != 0)
        //                {
        //                    double Distance = Math.Round(GoogleDistance.Calc(Convert.ToDouble(userCustomer.Latitude), Convert.ToDouble(userCustomer.Longitude), Convert.ToDouble(customer.Latitude), Convert.ToDouble(customer.Longitude)), 2);
        //                    if (TotalDistance >= Distance)
        //                    {
        //                        if(customer.CustomerType==EnumValue.GetEnumDescription(EnumValue.CustomerType.ServiceProvider))
        //                        {
        //                            NewResponseModel.CategoryName = _CategoryService.GetCategory(Convert.ToInt32(customer.CategoryId)).Name;
        //                        }

        //                        obj.Add(NewResponseModel);

        //                    }
        //                }
        //                else
        //                {
        //                    if (customer.CustomerType == EnumValue.GetEnumDescription(EnumValue.CustomerType.ServiceProvider))
        //                    {
        //                        NewResponseModel.CategoryName = _CategoryService.GetCategory(Convert.ToInt32(customer.CategoryId)).Name;
        //                    }
        //                    obj.Add(NewResponseModel);
        //                }

        //            }

        //            //int numberOfObjectsPerPage = 10;
        //            //var modelsdata = obj.Skip(numberOfObjectsPerPage * userCustomer.PageNumber).Take(numberOfObjectsPerPage);
        //            return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", obj), Configuration.Formatters.JsonFormatter);
        //        }
        //        else
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "User not found."), Configuration.Formatters.JsonFormatter);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        string ErrorMsg = ex.Message.ToString();
        //        ErrorLogging.LogError(ex);
        //        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "User not found."), Configuration.Formatters.JsonFormatter);
        //    }
        //}

        [Route("GetNearByProvider")]
        [HttpPost]
        public HttpResponseMessage GetNearByProvider(CustomerModel userCustomer)
        {
            try
            {
                if (userCustomer.Longitude == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Longitude is blank."), Configuration.Formatters.JsonFormatter);
                }
                if (userCustomer.Latitude == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Latitude is blank."), Configuration.Formatters.JsonFormatter);
                }
                var customers = _AgencyIndividualService.GetAgencyIndividuals().Where(a => a.IsActive == true).ToList();
                if (customers.Count() > 0)
                {

                    double TotalDistance = userCustomer.DistanceRange;
                    List<NearByModel> obj = new List<NearByModel>();
                    foreach (var customer in customers)
                    {
                        Mapper.CreateMap<HomeHelp.Entity.AgencyIndividual, HomeHelp.Models.NearByModel>();
                        HomeHelp.Models.NearByModel NewResponseModel = Mapper.Map<HomeHelp.Entity.AgencyIndividual, HomeHelp.Models.NearByModel>(customer);
                        if (!customer.IsAgency)
                        {
                            NewResponseModel.CategoryName = _CategoryService.GetCategory(Convert.ToInt32(customer.CategoryId)).Name;
                        }
                        else
                        {
                            string categoryId = "";
                            string categoryName = "";

                            var agencyMembers = _AgencyIndividualService.GetAgencyIndividuals().Where(c => c.ParentId == customer.AgencyIndividualId).Select(c => c.CategoryId).ToList();
                            foreach (var item in agencyMembers.Distinct())
                            {
                                categoryId = categoryId + ","+item;
                                var name = _CategoryService.GetCategory(Convert.ToInt32(item)).Name;
                                categoryName = categoryName + "," + name;
                            }
                            NewResponseModel.CategoryId = categoryId.TrimEnd(',').TrimStart(',');
                            NewResponseModel.CategoryName = categoryName.TrimEnd(',').TrimStart(',');
                        }
                        if (userCustomer.DistanceRange != 0)
                        {
                            double Distance = Math.Round(GoogleDistance.Calc(Convert.ToDouble(userCustomer.Latitude), Convert.ToDouble(userCustomer.Longitude), Convert.ToDouble(customer.Latitude), Convert.ToDouble(customer.Longitude)), 2);
                            if (TotalDistance >= Distance)
                            {
                              
                                if (customer.IsAgency || (!customer.IsAgency && customer.ParentId == new Guid()))
                                {
                                    if (customer.IsAgency)
                                    {
                                        NewResponseModel.CustomerId = customer.AgencyIndividualId;
                                        NewResponseModel.CompanyName = customer.FullName;
                                        NewResponseModel.FirstName = customer.FullName;
                                        NewResponseModel.MobileNumber = customer.ContactNumber;
                                        NewResponseModel.CustomerType = EnumValue.GetEnumDescription(EnumValue.CustomerType.Agency);


                                    }
                                    else
                                    {
                                        var cust = _CustomerService.GetCustomers().Where(c => c.UserId == customer.UserId).FirstOrDefault();
                                        if (cust != null)
                                        {
                                            NewResponseModel.PhotoPath = cust.PhotoPath;
                                            NewResponseModel.WorkRate = cust.WorkRate;
                                            NewResponseModel.FirstName = cust.FirstName;
                                            NewResponseModel.LastName = cust.LastName;
                                            NewResponseModel.FacebookId = cust.FacebookId;
                                            NewResponseModel.DeviceSerialNo = cust.DeviceSerialNo;
                                            NewResponseModel.ApplicationId = cust.ApplicationId;
                                            NewResponseModel.DeviceType = cust.DeviceType;
                                            NewResponseModel.CategoryId=cust.CategoryId.ToString();
                                        }
                                        NewResponseModel.CustomerId = customer.AgencyIndividualId;
                                        NewResponseModel.MobileNumber = customer.ContactNumber;
                                        NewResponseModel.CustomerType = EnumValue.GetEnumDescription(EnumValue.CustomerType.ServiceProvider);
                                    }
                                    obj.Add(NewResponseModel);
                                }


                            }
                        }
                        else
                        {
                            if (customer.IsAgency)
                            {
                                NewResponseModel.CustomerId = customer.AgencyIndividualId;
                                NewResponseModel.CompanyName = customer.FullName;
                                NewResponseModel.FirstName = customer.FullName;
                                NewResponseModel.MobileNumber = customer.ContactNumber;
                                NewResponseModel.CustomerType = EnumValue.GetEnumDescription(EnumValue.CustomerType.Agency);


                            }
                            else
                            {
                                var cust = _CustomerService.GetCustomers().Where(c => c.UserId == customer.UserId).FirstOrDefault();
                                if (cust != null)
                                {
                                    NewResponseModel.PhotoPath = cust.PhotoPath;
                                    NewResponseModel.WorkRate = cust.WorkRate;
                                    NewResponseModel.FirstName = cust.FirstName;
                                    NewResponseModel.LastName = cust.LastName;
                                    NewResponseModel.FacebookId = cust.FacebookId;
                                    NewResponseModel.DeviceSerialNo = cust.DeviceSerialNo;
                                    NewResponseModel.ApplicationId = cust.ApplicationId;
                                    NewResponseModel.DeviceType = cust.DeviceType;
                                    NewResponseModel.CategoryId = cust.CategoryId.ToString();
                                }
                                NewResponseModel.CustomerId = customer.AgencyIndividualId;
                                NewResponseModel.MobileNumber = customer.ContactNumber;
                                NewResponseModel.CustomerType = EnumValue.GetEnumDescription(EnumValue.CustomerType.ServiceProvider);
                            }
                            if (customer.IsAgency || (!customer.IsAgency && customer.ParentId == new Guid()))
                            {
                                obj.Add(NewResponseModel);
                            }
                        }

                    }

                    //int numberOfObjectsPerPage = 10;
                    //var modelsdata = obj.Skip(numberOfObjectsPerPage * userCustomer.PageNumber).Take(numberOfObjectsPerPage);
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", obj), Configuration.Formatters.JsonFormatter);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "User not found."), Configuration.Formatters.JsonFormatter);
                }
            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();
                ErrorLogging.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "User not found."), Configuration.Formatters.JsonFormatter);
            }
        }
        [Route("UpdateCustomerLocation")]
        [HttpPost]
        public HttpResponseMessage UpdateCustomerLocation([FromBody]CustomerLocationModel CustomerLocationModel)
        {
            try
            {
                if (CustomerLocationModel.CustomerId == new Guid())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer Id is blank."), Configuration.Formatters.JsonFormatter);
                }
                if (CustomerLocationModel.Latitude == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Latitude is blank."), Configuration.Formatters.JsonFormatter);
                }
                if (CustomerLocationModel.Longitude == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Longitude is blank."), Configuration.Formatters.JsonFormatter);
                }
                HomeHelp.Entity.Customer Customer = _CustomerService.GetCustomers().Where(x => x.CustomerId == CustomerLocationModel.CustomerId).FirstOrDefault();
                if (Customer != null)
                {
                    if (!Customer.IsActive)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "User is deactivated."), Configuration.Formatters.JsonFormatter);
                    }
                    var location = _CustomerLocationService.GetCustomerLocations().Where(c => c.CustomerId == CustomerLocationModel.CustomerId).FirstOrDefault();
                    location.Latitude = CustomerLocationModel.Latitude;
                    location.Longitude = CustomerLocationModel.Longitude;
                    _CustomerLocationService.UpdateCustomerLocation(location);
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", "Successfully updated."), Configuration.Formatters.JsonFormatter);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "User is not found."), Configuration.Formatters.JsonFormatter);
                }
            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();
                ErrorLogging.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Please try later."), Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("GetCustomerLocationByID")]
        [HttpGet]
        public HttpResponseMessage GetCustomerLocationByID([FromUri] Guid CustomerId)
        {
            try
            {
                var Customer = _CustomerService.GetCustomer(CustomerId);
                if (Customer != null)
                {
                    if (Customer.IsActive)
                    {
                        var location = _CustomerLocationService.GetCustomerLocations().Where(c => c.CustomerId == CustomerId).FirstOrDefault();
                        Mapper.CreateMap<HomeHelp.Entity.CustomerLocation, HomeHelp.Models.CustomerLocationResponseModel>();
                        CustomerLocationResponseModel CustomerResponseModel = Mapper.Map<HomeHelp.Entity.CustomerLocation, HomeHelp.Models.CustomerLocationResponseModel>(location);
                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", CustomerResponseModel), Configuration.Formatters.JsonFormatter);

                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "User not found."), Configuration.Formatters.JsonFormatter);
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "User not found."), Configuration.Formatters.JsonFormatter);
                }

            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();
                ErrorLogging.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "User not found."), Configuration.Formatters.JsonFormatter);
            }
        }
    }
}