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
    public class ProfileApiController : ApiController
    {
        public ICustomerService _CustomerService { get; set; }
       
        public ProfileApiController(ICustomerService CustomerService)
        {
            this._CustomerService = CustomerService;
        }
        public async Task<HttpResponseMessage> UploadImage()
        {
           
            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }
            string root = HttpContext.Current.Server.MapPath("~/CustomerPhoto/temp");
          
            CustomMultipartFormDataStreamProvider provider = new CustomMultipartFormDataStreamProvider(root);

            try
            {
                StringBuilder sb = new StringBuilder(); // Holds the response body
                
                Type type = typeof(CustomerModel); // Get type pointer
                CustomerModel locationImagesModel = new CustomerModel();
                
                // Read the form data and return an async task.
                CustomMultipartFormDataStreamProvider x = await Request.Content.ReadAsMultipartAsync(provider);

                Guid CustomerId = new Guid();
                // This illustrates how to get the form data.
                foreach (var key in provider.FormData.AllKeys)
                {
                    if (key == "CustomerId")
                    {
                        string propertyValue = provider.FormData.GetValues(key).FirstOrDefault();
                        if (propertyValue != null)
                        {
                            CustomerId = new Guid(propertyValue);
                            break;
                        }
                    }
                 
                    
                }
                
                //Delete all already exist files
                HomeHelp.Entity.Customer Customer = _CustomerService.GetCustomers().Where(c => c.CustomerId == CustomerId && c.IsActive == true).FirstOrDefault();
                if (Customer != null)
                {
                    if (Customer.PhotoPath != "" && Customer.PhotoPath != null)
                    {
                        DeleteImage(Customer.PhotoPath);
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
                             _CustomerService.UpdateCustomer(Customer);
                             sb.Append(URL);
                        
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", Customer.PhotoPath), Configuration.Formatters.JsonFormatter);
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
    }
}
public class CustomMultipartFormDataStreamProvider : MultipartFormDataStreamProvider
{
    public CustomMultipartFormDataStreamProvider(string path) : base(path) { }

    public override string GetLocalFileName(HttpContentHeaders headers)
    {
        return headers.ContentDisposition.FileName.Replace("\"", string.Empty);
    }
}