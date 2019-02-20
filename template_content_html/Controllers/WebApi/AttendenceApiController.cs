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
using System.Net.Mail;
using System.Web.Configuration;
using Onlo.Data;

namespace Onlo.Controllers.WebApi
{
    [RoutePrefix("Attendence")]
    public class AttendenceApiController : ApiController
    {

        public IAttendenceService _AttendenceService { get; set; }
        public ICustomerService _CustomerService { get; set; }
        public INotificationService _NotificationService { get; set; }
        public AttendenceApiController(ICustomerService CustomerService, IAttendenceService AttendenceService, INotificationService NotificationService)
        {
            this._CustomerService = CustomerService;
            this._AttendenceService = AttendenceService;
            this._NotificationService = NotificationService;
        }

        [Route("SaveAttendence")]
        [HttpPost]
        public HttpResponseMessage SaveAttendence([FromBody]AttendenceModel AttendenceModel)
        {
            try
            {
                List<Attendence> model = new List<Attendence>();
                if (AttendenceModel.AbsentCustomerIds == null && AttendenceModel.AbsentCustomerIds == "" || AttendenceModel.PresentCustomerIds == null && AttendenceModel.PresentCustomerIds == "")
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Absent or PresentCustomer Ids is blank."), Configuration.Formatters.JsonFormatter);
                }

                if (AttendenceModel.AttendenceDate == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Attendence Date is blank."), Configuration.Formatters.JsonFormatter);
                }

                var customerTeacher = _CustomerService.GetCustomer(AttendenceModel.TeacherId);
                if (customerTeacher == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "No teacher found."), Configuration.Formatters.JsonFormatter);
                }

                var CustomerList = _CustomerService.GetCustomers().Where(c => c.ParentId == AttendenceModel.TeacherId).ToList();
                if (CustomerList.Count() > 0)
                {
                    var CustomerIds = CustomerList.Select(s => s.CustomerId).ToList();
                    if (AttendenceModel.PresentCustomerIds != null && AttendenceModel.PresentCustomerIds != "")
                    {
                        var PresentCustomerIds = AttendenceModel.PresentCustomerIds.Split(',');
                        var PresentIds = PresentCustomerIds.Select(int.Parse).ToList();
                        foreach (var item in PresentIds)
                        {
                            
                            var attendenceFound = _AttendenceService.GetAttendences().Where(t => t.AttendenceDate == AttendenceModel.AttendenceDate && t.CustomerId == item && t.TeacherId == AttendenceModel.TeacherId).FirstOrDefault();
                            if (attendenceFound == null)
                            {
                                Mapper.CreateMap<Onlo.Models.AttendenceModel, Onlo.Entity.Attendence>();
                                Onlo.Entity.Attendence Attendence = Mapper.Map<Onlo.Models.AttendenceModel, Onlo.Entity.Attendence>(AttendenceModel);
                                Attendence.Status = EnumValue.GetEnumDescription(EnumValue.AttendenceStatus.Present);
                                Attendence.CustomerId = item;
                                _AttendenceService.InsertAttendence(Attendence);
                                model.Add(Attendence);

                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Attendence already marked."), Configuration.Formatters.JsonFormatter);
                            }
                           
                        }
                    }

                    if (AttendenceModel.AbsentCustomerIds != null && AttendenceModel.AbsentCustomerIds != "")
                    {
                        var AbsentCustomerIds = AttendenceModel.AbsentCustomerIds.Split(',');
                        var AbsentIds = AbsentCustomerIds.Select(int.Parse).ToList();
                        if (AbsentIds.Count() > 0)
                        {
                            foreach (var item in AbsentIds)
                            {
                                var attendenceFound = _AttendenceService.GetAttendences().Where(t => t.AttendenceDate == AttendenceModel.AttendenceDate && t.CustomerId == item && t.TeacherId == AttendenceModel.TeacherId).FirstOrDefault();
                                if (attendenceFound == null)
                                {
                                    Mapper.CreateMap<Onlo.Models.AttendenceModel, Onlo.Entity.Attendence>();
                                    Onlo.Entity.Attendence Attendence = Mapper.Map<Onlo.Models.AttendenceModel, Onlo.Entity.Attendence>(AttendenceModel);
                                    Attendence.Status = EnumValue.GetEnumDescription(EnumValue.AttendenceStatus.Absent);
                                    Attendence.CustomerId = item;
                                    _AttendenceService.InsertAttendence(Attendence);
                                    model.Add(Attendence);
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Attendence already marked."), Configuration.Formatters.JsonFormatter);
                                }
                            }
                        }
                    }


                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", model), Configuration.Formatters.JsonFormatter);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "No teacher found."), Configuration.Formatters.JsonFormatter);
                }

            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();
                ErrorLogging.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "please try again."), Configuration.Formatters.JsonFormatter);
            }

        }


        [Route("GetAttendenceByTeacher")]
        [HttpPost]
        public HttpResponseMessage GetAttendenceByTeacher([FromBody]StudentAttendence StudentAttendence)
        {
            JsonReturnModel result = new JsonReturnModel();
            try
            {
                result = getAttendence(StudentAttendence);
                return Request.CreateResponse(HttpStatusCode.OK, result, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();
                ErrorLogging.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.OK, result, Configuration.Formatters.JsonFormatter);
            }

        }

        [Route("GetAttendenceByStudent")]
        [HttpPost]
        public HttpResponseMessage GetAttendenceByStudent([FromBody]StudentAttendence StudentAttendence)
        {
            JsonReturnModel result = new JsonReturnModel();
            try
            {
                result = getAttendence(StudentAttendence);
                return Request.CreateResponse(HttpStatusCode.OK, result, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();
                ErrorLogging.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.OK, result, Configuration.Formatters.JsonFormatter);
            }

        }
        [Route("GetAttendenceSummary")]
        [HttpGet]
        public HttpResponseMessage GetAttendenceSummary([FromUri]int CustomerId)
        {
            try
            {
                if(CustomerId==0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer Id is blank."), Configuration.Formatters.JsonFormatter);
                }
                var attendences = _AttendenceService.GetAttendences().Where(a => a.CustomerId == CustomerId).ToList();
                if(attendences.Count()>0)
                {
                    var models = new List<AttendenceModel>();
                    Mapper.CreateMap<Onlo.Entity.Attendence, Onlo.Models.AttendenceModel>();
                    foreach (var attendence in attendences)
                    {
                        Onlo.Models.AttendenceModel Attendence = Mapper.Map<Onlo.Entity.Attendence, Onlo.Models.AttendenceModel>(attendence);
                        models.Add(Attendence);
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", models), Configuration.Formatters.JsonFormatter);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", "No attendence found."), Configuration.Formatters.JsonFormatter);
                }
               
            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();
                ErrorLogging.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Please try again."), Configuration.Formatters.JsonFormatter);
            }

        }
        public JsonReturnModel getAttendence(StudentAttendence StudentAttendence)
        {
            List<AttendenceModel> attendenceList = new List<AttendenceModel>();
            var Customer = _CustomerService.GetCustomer(StudentAttendence.CustomerId).CustomerType;
            if (Customer != null)
            {
                if (Customer == StudentAttendence.CustomerType)
                {
                    if (Customer == EnumValue.GetEnumDescription(EnumValue.CustomerType.Student) || Customer == EnumValue.GetEnumDescription(EnumValue.CustomerType.Parent))
                    {

                        var attendences = _AttendenceService.GetAttendences().Where(a => (a.AttendenceDate >= StudentAttendence.StartDate && a.AttendenceDate <= StudentAttendence.EndDate) && a.CustomerId == StudentAttendence.CustomerId).ToList();
                        if (attendences.Count() > 0)
                        {
                            foreach (var item in attendences)
                            {
                                if (item.Status == EnumValue.GetEnumDescription(EnumValue.AttendenceStatus.Present))
                                {
                                    Mapper.CreateMap<Onlo.Entity.Attendence, Onlo.Models.AttendenceModel>();
                                    Onlo.Models.AttendenceModel Attendence = Mapper.Map<Onlo.Entity.Attendence, Onlo.Models.AttendenceModel>(item);
                                    attendenceList.Add(Attendence);
                                }
                            }
                        }
                    }
                    else
                    {
                        var CustomerList = _CustomerService.GetCustomers().Where(c => c.ParentId == StudentAttendence.CustomerId).ToList();
                        if (CustomerList.Count() > 0)
                        {
                            var CustomerIds = CustomerList.Select(s => s.CustomerId).ToList();
                            foreach (var CustomerId in CustomerIds)
                            {
                                var studentAttendence = _AttendenceService.GetAttendences().Where(a => a.CustomerId == CustomerId && a.AttendenceDate == StudentAttendence.StartDate).FirstOrDefault();
                                if (studentAttendence != null)
                                {
                                    Mapper.CreateMap<Onlo.Entity.Attendence, Onlo.Models.AttendenceModel>();
                                    Onlo.Models.AttendenceModel Attendence = Mapper.Map<Onlo.Entity.Attendence, Onlo.Models.AttendenceModel>(studentAttendence);
                                    var studentDetail = _CustomerService.GetCustomer(studentAttendence.CustomerId);
                                    Attendence.StudentName = studentDetail.Name;
                                    Attendence.StudentCode = studentDetail.StudentCode;
                                    Attendence.ProfilePath = studentDetail.PhotoPath;
                                    Attendence.EmailId = studentDetail.EmailId;
                                    attendenceList.Add(Attendence);
                                }

                            }

                        }
                    }
                    return CommonCls.CreateMessage("success", attendenceList);
                }
                else
                {
                    return CommonCls.CreateMessage("error", "This Customer doesnot exist in this customer type.");
                }
            }
            else
            {
                return CommonCls.CreateMessage("success", "No attendence found.");
            }
        }


    }
}