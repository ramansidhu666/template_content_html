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
using System.Text.RegularExpressions;
using System.Globalization;
using System.Threading.Tasks;
using System.Text;
using System.Net.Http.Headers;
using System.Reflection;

namespace HomeHelp.Web.Controllers.WebApi
{
    public class CustomerProfileApiController : ApiController
    {
        public ICustomerService _CustomerService { get; set; }
       
        public CustomerProfileApiController(ICustomerService CustomerService)
        {
            this._CustomerService = CustomerService;
        }
        public async Task<HttpResponseMessage> UploadCustomerProfile()
        {

            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }
            string root = HttpContext.Current.Server.MapPath("~/CustomerPhoto/temp");

            CustomMultipartFormDataStreamProvider1 provider = new CustomMultipartFormDataStreamProvider1(root);

            try
            {
                StringBuilder sb = new StringBuilder(); // Holds the response body

                Type type = typeof(CustomerModel); // Get type pointer
                CustomerModel locationImagesModel = new CustomerModel();

                // Read the form data and return an async task.
                CustomMultipartFormDataStreamProvider1 x = await Request.Content.ReadAsMultipartAsync(provider);

                int CustomerId = 0;
                string Bio = "";
                string Privacy = "";
                int Age = 0;
                string Gender = "";
                string Music = "";
                string Photography = "";
                string Camping = "";
                string Hiking = "";
                // This illustrates how to get the form data.
                foreach (var key in provider.FormData.AllKeys)
                {
                    if (key == "CustomerId")
                    {
                        string propertyValue = provider.FormData.GetValues(key).FirstOrDefault();
                        if (propertyValue != null)
                        {
                            CustomerId = Convert.ToInt32(propertyValue);
                            if (CustomerId == 0)
                            {
                                return ErrorMessage("error", "Customer Id is blank.");
                            }
                        }
                    }
                    if (key == "Age")
                    {
                        string propertyValue = provider.FormData.GetValues(key).FirstOrDefault();
                        if (propertyValue != null)
                        {
                            Age = Convert.ToInt32(propertyValue);
                            if (Age == 0)
                            {
                                return ErrorMessage("error", "Age is blank.");
                            }
                        }
                    }
                    if (key == "Bio")
                    {
                        string propertyValue = provider.FormData.GetValues(key).FirstOrDefault();
                        if (propertyValue != null)
                        {
                            Bio = propertyValue;
                            if (Bio == "" || Bio == null)
                            {
                                return ErrorMessage("error", "Bio is blank.");
                            }
                        }
                    }
                    if (key == "Privacy")
                    {
                        string propertyValue = provider.FormData.GetValues(key).FirstOrDefault();
                        if (propertyValue != null)
                        {
                            Privacy = propertyValue;
                            if (Privacy != EnumValue.GetEnumDescription(EnumValue.Privacy.Public) && Privacy != EnumValue.GetEnumDescription(EnumValue.Privacy.Private))
                            {
                                return ErrorMessage("error", "Privacy is wrong.");
                            }
                            if (Privacy == "" || Privacy == null)
                            {
                                return ErrorMessage("error", "Privacy is blank.");
                            }
                        }
                    }
                    if (key == "Gender")
                    {
                        string propertyValue = provider.FormData.GetValues(key).FirstOrDefault();
                        if (propertyValue != null)
                        {
                            Gender = propertyValue;
                            if (Gender != EnumValue.GetEnumDescription(EnumValue.Gender.Female) && Gender != EnumValue.GetEnumDescription(EnumValue.Gender.Male))
                            {
                                return ErrorMessage("error", "Gender is wrong.");
                            }
                            if (Gender == "" || Gender == null)
                            {
                                return ErrorMessage("error", "Gender is blank.");
                            }
                        }
                    }
                    if (key == "Music")
                    {
                        string propertyValue = provider.FormData.GetValues(key).FirstOrDefault();
                        if (propertyValue != null)
                        {
                            Music =propertyValue;
                            if (Music == "" || Music == null)
                            {
                                return ErrorMessage("error", "Music is blank.");
                            }
                        }
                    }
                    if (key == "Photography")
                    {
                        string propertyValue = provider.FormData.GetValues(key).FirstOrDefault();
                        if (Photography != null)
                        {
                            Photography = propertyValue;
                            if (Photography == "" || Photography == null)
                            {
                                return ErrorMessage("error", "Photography is blank.");
                            }
                        }
                    }
                    if (key == "Camping")
                    {
                        string propertyValue = provider.FormData.GetValues(key).FirstOrDefault();
                        if (propertyValue != null)
                        {
                            Camping = propertyValue;
                            if (Camping == "" || Camping == null)
                            {
                                return ErrorMessage("error", "Camping is blank.");
                            }
                        }
                    }

                    if (key == "Hiking")
                    {
                        string propertyValue = provider.FormData.GetValues(key).FirstOrDefault();
                        if (propertyValue != null)
                        {
                            Hiking = propertyValue;
                            if (Hiking == "" || Hiking == null)
                            {
                                return ErrorMessage("error", "Hiking is blank.");
                            }
                        }
                    }

                 if(CustomerId!=0&& Bio!=""&&Privacy!=""&&Age!=0&&Gender!=""&&Music!=""&&Photography!=""&&Camping!=""&&Hiking!="")
                 {
                     break;
                 }
                }

                //Delete all already exist files
                HomeHelp.Entity.Customer Customer = _CustomerService.GetCustomers().Where(c => c.CustomerId == CustomerId && c.IsActive == true).FirstOrDefault();
                if (Customer != null)
                {
                    if (Customer.PhotoPath != "" && Customer.PhotoPath != null)
                    {
                        DeleteImage(Customer.BackgroundImage);
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "No user found."), Configuration.Formatters.JsonFormatter);
                }

                // Process the list of files found in the directory.
                string[] fileEntries = Directory.GetFiles(root);
                foreach (string fileName in fileEntries)
                {
                    var fileFound = provider.FileData.Where(c => c.Headers.ContentDisposition.FileName.Replace("\"", string.Empty) == Path.GetFileName(fileName)).FirstOrDefault();
                    if (fileFound != null)
                    {
                        //string NewFileName = Guid.NewGuid() + "_Event" + EventId.ToString() + Path.GetExtension(fileName);
                        string NewFileName = Guid.NewGuid() + Path.GetExtension(fileName);

                        string NewRoot = HttpContext.Current.Server.MapPath("~/CustomerPhoto") + "\\" + NewFileName;

                        System.IO.File.Move(fileName, NewRoot);

                        string URL = CommonCls.GetURL() + "/CustomerPhoto/" + NewFileName;
                       
                        Customer.PhotoPath = URL;
                        
                        sb.Append(URL);

                    }
                }
                Customer.Bio = Bio;
                Customer.Privacy = Privacy;
                Customer.Age = Age;
                Customer.Gender = Gender;
                _CustomerService.UpdateCustomer(Customer);
                HobbiesAndInterests hobbies = _HobbiesAndInterestsService.GetHobbiesAndInterestss().Where(c => c.CustomerId == CustomerId).FirstOrDefault();
                if (hobbies != null)
                {
                    hobbies.Music = Convert.ToInt32(Music);
                    hobbies.Hiking = Convert.ToInt32(Hiking);
                    hobbies.Camping = Convert.ToInt32(Camping);
                    hobbies.Photography = Convert.ToInt32(Photography);
                    hobbies.CustomerId = CustomerId;
                    _HobbiesAndInterestsService.UpdateHobbiesAndInterests(hobbies);
                }
                else
                {
                    HobbiesAndInterests objHobbies = new HobbiesAndInterests();
                    objHobbies.Music = Convert.ToInt32(Music);
                    objHobbies.Hiking = Convert.ToInt32(Hiking);
                    objHobbies.Camping = Convert.ToInt32(Camping);
                    objHobbies.Photography = Convert.ToInt32(Photography);
                    objHobbies.CustomerId = CustomerId;
                    _HobbiesAndInterestsService.InsertHobbiesAndInterests(objHobbies);
                }
                Mapper.CreateMap<Customer, UserCustomerModel>();
                UserCustomerModel userCustomerModel = Mapper.Map<Customer, UserCustomerModel>(Customer);
                userCustomerModel.Music = Music;
                userCustomerModel.Hiking = Hiking;
                userCustomerModel.Camping = Camping;
                userCustomerModel.Photography = Photography;
                return ErrorMessage("success", userCustomerModel);
            }
            catch (System.Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
            }
        }

        public void DeleteImage(string filePath)
        {
            try
            {
                var uri = new Uri(filePath);
                var fileName = Path.GetFileName(uri.AbsolutePath);
                var subPath = HttpContext.Current.Server.MapPath("~/CustomerPhoto");
                var path = Path.Combine(subPath, fileName);

                FileInfo file = new FileInfo(path);
                if (file.Exists)
                {
                    file.Delete();
                }
            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();
                ErrorLogging.LogError(ex);
            }
        }

        public HttpResponseMessage ErrorMessage(string status, object message)
        {
            return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage(status, message), Configuration.Formatters.JsonFormatter);
        }
    }
}
public class CustomMultipartFormDataStreamProvider1 : MultipartFormDataStreamProvider
{
    public CustomMultipartFormDataStreamProvider1(string path) : base(path) { }

    public override string GetLocalFileName(HttpContentHeaders headers)
    {
        return headers.ContentDisposition.FileName.Replace("\"", string.Empty);
    }
}