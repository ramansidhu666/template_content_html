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
using Onlo.Entity;
using Onlo.Services;
using Newtonsoft.Json;
using AutoMapper;
using Onlo.Models;
using Onlo.Infrastructure;
using Onlo.Core.Infrastructure;
using System.IO;
using System.Drawing;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Threading.Tasks;
using System.Text;
using System.Net.Http.Headers;
using System.Reflection;

namespace Onlo.Web.Controllers.WebApi
{
   
    public class TaskCustomerFileApiController : ApiController
    {
        public ICustomerService _CustomerService { get; set; }
        public ITasksService _TasksService { get; set; }
        public ITaskCustomerService _TaskCustomerService { get; set; }
        public ITaskFileService _TaskFileService { get; set; }
        public ITaskCustomerFileService _TaskCustomerFileService { get; set; }
        public TaskCustomerFileApiController(ITasksService TasksService, ICustomerService CustomerService, ITaskFileService TaskFileService, ITaskCustomerFileService TaskCustomerFileService, ITaskCustomerService TaskCustomerService)
        {
            this._TasksService = TasksService;
            this._CustomerService = CustomerService;
            this._TaskFileService = TaskFileService;
            this._TaskCustomerFileService = TaskCustomerFileService;
            this._TaskCustomerService = TaskCustomerService;
        }
        public async Task<HttpResponseMessage> UploadTaskCustomerFiles()
        {
           
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }
            string root = HttpContext.Current.Server.MapPath("~/TaskCustomerFile/temp");
          
            CustomMultipartFormDataStreamProvider1 provider = new CustomMultipartFormDataStreamProvider1(root);

            try
            {
                StringBuilder sb = new StringBuilder(); // Holds the response body

                Type type = typeof(TaskCustomerModel); // Get type pointer
                TaskCustomerModel taskModel = new TaskCustomerModel();

                // Read the form data and return an async task.
                CustomMultipartFormDataStreamProvider1 x = await Request.Content.ReadAsMultipartAsync(provider);

                int TaskId = 0;
                int CustomerId = 0;
                // This illustrates how to get the form data.
                foreach (var key in provider.FormData.AllKeys)
                {
                    if (key == "CustomerId")
                    {
                        string propertyValue = provider.FormData.GetValues(key).FirstOrDefault();
                        if (propertyValue != null)
                        {
                            CustomerId = Convert.ToInt32(propertyValue);
                            //break;
                        }
                    }
                    if (key == "TaskId")
                    {
                        string propertyValue = provider.FormData.GetValues(key).FirstOrDefault();
                        if (propertyValue != null)
                        {
                            TaskId = Convert.ToInt32(propertyValue);
                            //break;
                        }
                    }
                    if (CustomerId != 0 && TaskId != 0)
                    {
                        break;
                    }


                }
                //Delete all already exist files

                var taskCustomer = _TaskCustomerService.GetTaskCustomers().Where(t => t.TaskID == TaskId && t.CustomerId == CustomerId).FirstOrDefault();
                if (taskCustomer != null)
                {
                    var TaskFiles = _TaskCustomerFileService.GetTaskCustomerFiles().Where(t => t.TaskCustomerId == taskCustomer.TaskCustomerId).ToList();
                    if (TaskFiles.Count() > 0)
                    {
                        foreach (var files in TaskFiles)
                        {
                            DeleteImage(files.FilePath);
                            _TaskCustomerFileService.DeleteTaskCustomerFile(files);
                        }
                    }
                }
                // Process the list of files found in the directory.
                string[] fileEntries = Directory.GetFiles(root);
                foreach (string fileName in fileEntries)
                {
                    var fileFound = provider.FileData.Where(c => c.Headers.ContentDisposition.FileName.Replace("\"", string.Empty) == Path.GetFileName(fileName)).FirstOrDefault();
                    if (fileFound != null)
                    {
                        string NewFileName = Guid.NewGuid() + Path.GetExtension(fileName);

                        string NewRoot = HttpContext.Current.Server.MapPath("~/TaskCustomerFile") + "\\" + NewFileName;

                        System.IO.File.Move(fileName, NewRoot);

                        string URL = CommonCls.GetURL() + "/TaskCustomerFile/" + NewFileName;
                        
                        TaskCustomerFile taskFile = new TaskCustomerFile();
                        taskFile.FilePath = URL;
                        taskFile.TaskCustomerId = taskCustomer.TaskCustomerId;
                        _TaskCustomerFileService.InsertTaskCustomerFile(taskFile);
                        sb.Append(URL);
                        
                    }
                }
               
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", "Successfully saved."), Configuration.Formatters.JsonFormatter);
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
                var subPath = HttpContext.Current.Server.MapPath("~/TaskFile");
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
public class CustomMultipartFormDataStreamProvider1 : MultipartFormDataStreamProvider
{
    public CustomMultipartFormDataStreamProvider1(string path) : base(path) { }

    public override string GetLocalFileName(HttpContentHeaders headers)
    {
        return headers.ContentDisposition.FileName.Replace("\"", string.Empty);
    }
}