using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Friendlier.Services;
using Friendlier.Controllers;
using Friendlier.Infrastructure;
using Friendlier.Entity;
using AutoMapper;
using Friendlier.Models;
using System.Data.SqlClient;
using System.Data.Entity.Infrastructure;
using Friendlier.Core.Infrastructure;
//using Friendlier.Core.UtilityManager;
using System.Data;
using System.Net.Mail;
using System.Web.Configuration;
using System.Configuration;


namespace Friendlier.Controllers
{
    public class LocationController : BaseController
    {
        #region Dependancy injection
        public IUserRoleService _UserRoleService { get; set; }
        public IUserService _UserService { get; set; }
        public ILocationService _LocationService { get; set; }
        public ILocationImagesService _LocationImagesService { get; set; }
        public ILocationTagService _LocationTagService { get; set; }
        public ICustomerService _CustomerService { get; set; }
        public ICategoryService _CategoryService { get; set; }
        public LocationController(ICategoryService CategoryService, ILocationTagService LocationTagService, ICustomerService CustomerService, ILocationImagesService LocationImagesService, IUserService UserService, IRoleService RoleService, IUserRoleService UserRoleService, IFormService FormService, IRoleDetailService RoleDetailService, ILocationService LocationService)
            : base(UserService, RoleService, FormService, RoleDetailService, UserRoleService)
        {
            this._LocationImagesService = LocationImagesService;
            this._LocationTagService = LocationTagService;
            this._UserRoleService = UserRoleService;
            this._LocationService = LocationService;
            this._CustomerService = CustomerService;
            this._CategoryService = CategoryService;
        }
        #endregion

        #region Security fuctions
        private void CheckPermission()
        {
            RoleDetailModel roleDetail = UserPermission("Location");
            TempData["View"] = roleDetail.IsView;
            TempData["Create"] = roleDetail.IsCreate;
            TempData["Edit"] = roleDetail.IsEdit;
            TempData["Delete"] = roleDetail.IsDelete;
            TempData["Detail"] = roleDetail.IsDetail;
        }
        #endregion

        #region Basic crude oprations of Location
        public ActionResult Index(string Name, string operation, string ShowMessage, string MessageBody, string CurrentPageNo)
        {
            UserPermissionAction("Location", RoleAction.view.ToString(), operation, ShowMessage, MessageBody);
            CheckPermission();

            var models = new List<LocationModel>();
            if (CurrentPageNo != null && CurrentPageNo != "")
            {
                ViewBag.CurrentPageNo = CurrentPageNo;
            }
            else
            {
                ViewBag.CurrentPageNo = 1;
            }
            var entity1 = _LocationService.GetLocations();
            var entity2 = _CustomerService.GetCustomers().Where(c => c.IsActive == true);

            var Locations = (from x in entity1
                             join y in entity2
                             on x.CustomerId equals y.CustomerId
                             select x).ToList();



            // var Locations = _LocationService.GetLocations();
            if (!string.IsNullOrEmpty(Name))
            {
                Locations = Locations.Where(x => x.Title.Contains(Name.Trim())).ToList();
            }
            Mapper.CreateMap<Friendlier.Entity.Location, Friendlier.Models.LocationModel>();
            foreach (var Location in Locations)
            {
                var _Location = Mapper.Map<Friendlier.Entity.Location, Friendlier.Models.LocationModel>(Location);


                models.Add(_Location);

            }
            if (models.ToList().Count == 0)
            {
                TempData["ShowMessage"] = "error";
                TempData["MessageBody"] = "No Record found !";
            }

            return View(models);
        }

        [HttpGet]
        public ActionResult Create()
        {
            UserPermissionAction("Location", RoleAction.view.ToString());
            CheckPermission();
            //List<Location> objService = _LocationService.GetLocations();
            //objService.Add(new Location { LocationId = -1, Name = "Parent" });

            //ViewBag.Location = new SelectList(objService, "LocationId", "Name");
            //ViewBag.LocationList = new SelectList(_CompanyService.GetCompanies(), "CompanyID", "CompanyName", Session["CompanyID"].ToString());
            return View();
        }
        [HttpPost]
        public ActionResult Create([Bind(Include = "LocationId,Name,ParentId,IsActive")]LocationModel LocationModel, HttpPostedFileBase file)
        {

            UserPermissionAction("Location", RoleAction.view.ToString());
            CheckPermission();
            try
            {
                TempData["ShowMessage"] = "error";
                TempData["MessageBody"] = "Please fill the required field with valid data";
                if (file != null && file.ContentLength > 0)
                {
                    try
                    {

                        string path = Path.Combine(Server.MapPath("~/LocationImage"), Path.GetFileName(file.FileName));
                        file.SaveAs(path);
                        // ViewBag.Message = "File uploaded successfully";  
                    }
                    catch (Exception ex)
                    {
                        ViewBag.Message = "ERROR:" + ex.Message.ToString();
                    }

                }

                //var IsParentAllReadyContainLocationName = _LocationService.GetLocations().Where(c => c.ParentId == LocationModel.ParentId).Select(c => c.Name);
                //if (IsParentAllReadyContainLocationName.Contains(LocationModel.Name.Trim()))
                //{
                //    TempData["ShowMessage"] = "error";
                //    TempData["MessageBody"] = " Location name is already exist.";
                //    return RedirectToAction("Create");
                //}
                var IsLocationDuplicate = _LocationService.GetLocations().Where(c => c.Title == LocationModel.Title.Trim()).FirstOrDefault();
                if (IsLocationDuplicate != null)
                {
                    TempData["ShowMessage"] = "error";
                    TempData["MessageBody"] = " Location name is already exist.";
                    return RedirectToAction("Create");
                }
                //int? parent;
                //if (LocationModel.ParentId == -1)
                //{
                //    parent = null;
                //}
                //else
                //{
                //    parent = Convert.ToInt32(LocationModel.ParentId);
                //}
                Mapper.CreateMap<LocationModel, Location>();
                var Location = Mapper.Map<LocationModel, Location>(LocationModel);
                string URL = CommonCls.GetURL() + "/LocationImage/" + file.FileName;
                //Location.PhotoPath = URL;
                //Location.IsActive = true;
                _LocationService.InsertLocation(Location);
                //new LocationModel
                //{
                //    LocationId = Convert.ToInt32(LocationModel.LocationId),
                //    Name = LocationModel.Name,
                //    Description=LocationModel.Description// LocationModel.ParentId == -1?null:Convert.ToInt32(LocationModel.ParentId)
                //}.InsertUpdate();

                TempData["ShowMessage"] = "success";
                TempData["MessageBody"] = " Location save  successfully.";
                return RedirectToAction("Index");

            }


            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();//
                ErrorLogging.LogError(ex);

            }
            var errors = ModelState.Where(x => x.Value.Errors.Count > 0).Select(x => new { x.Key, x.Value.Errors }).ToArray();
            var modelStateErrors = this.ModelState.Keys.SelectMany(key => this.ModelState[key].Errors);
            //ViewBag.Location = new SelectList(_LocationService.GetLocations(), "LocationId", "Name");
            return View(LocationModel);
        }
        [HttpGet]
        public ActionResult Edit(int id, string CurrentPageNo)
        {
            UserPermissionAction("Location", RoleAction.detail.ToString());
            CheckPermission();
            if (id < 0)
            {

                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Location objLocation = _LocationService.GetLocation(id);
            var models = new List<LocationModel>();
            Mapper.CreateMap<Friendlier.Entity.Location, Friendlier.Models.LocationModel>();
            Friendlier.Models.LocationModel LocationModel = Mapper.Map<Friendlier.Entity.Location, Friendlier.Models.LocationModel>(objLocation);
            LocationModel.CurrentPageNo = CurrentPageNo;
            if (objLocation == null)
            {
                return HttpNotFound();

            }
            //List<Location> objService = _LocationService.GetLocations();
            //objService.Add(new Location { LocationId = -1, Name = "Parent" });

            //ViewBag.Location = new SelectList(objService, "LocationId", "Name", objLocation.LocationId);

            return View(LocationModel);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit([Bind(Include = "LocationId,Name,Description,CurrentPageNo")]LocationModel LocationModel)
        {
            UserPermissionAction("Location", RoleAction.view.ToString());
            CheckPermission();
            try
            {
                TempData["ShowMessage"] = "error";
                TempData["MessageBody"] = "Please fill the required field with valid data";
                if (ModelState.IsValid)
                {
                    //var Location = form["LocationLists"].ToString();
                    var IsParentAllReadyContainLocationName = _LocationService.GetLocations().Where(c => c.LocationId != LocationModel.LocationId).Select(c => c.Title);
                    if (IsParentAllReadyContainLocationName.Contains(LocationModel.Title.Trim()))
                    {
                        TempData["ShowMessage"] = "error";
                        TempData["MessageBody"] = " Location name is already exist.";
                        return RedirectToAction("Edit");
                    }
                    Location LocationFound = _LocationService.GetLocations().Where(x => x.Title == LocationModel.Title && x.LocationId != LocationModel.LocationId).FirstOrDefault();
                    Mapper.CreateMap<Friendlier.Models.LocationModel, Friendlier.Entity.Location>();
                    Friendlier.Entity.Location Locations = Mapper.Map<Friendlier.Models.LocationModel, Friendlier.Entity.Location>(LocationModel);
                    if (LocationFound == null)
                    {

                        //Locations.IsActive = true;
                        _LocationService.UpdateLocation(Locations);

                        TempData["ShowMessage"] = "success";
                        TempData["MessageBody"] = " Location update  successfully.";
                        return RedirectToAction("Index", new { CurrentPageNo = LocationModel.CurrentPageNo });
                        // end  Update CompanyEquipments
                    }

                    else
                    {
                        TempData["ShowMessage"] = "error";
                        if (LocationFound.Title.Trim().ToLower() == LocationModel.Title.Trim().ToLower()) //Check User Name
                        {
                            TempData["MessageBody"] = LocationFound.Title + " already exist.";
                        }

                        else
                        {
                            TempData["MessageBody"] = "Some unknown problem occured while proccessing save operation on ";
                        }


                    }
                }


            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();//
                ErrorLogging.LogError(ex);

            }
            ViewBag.Location = new SelectList(_LocationService.GetLocations(), "LocationId", "Name", LocationModel.LocationId);
            return View(LocationModel);

        }
        [HttpGet]
        public ActionResult Delete(int id)
        {
            UserPermissionAction("Location", RoleAction.detail.ToString());
            CheckPermission();
            if (id < 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Location objLocation = _LocationService.GetLocation(id);
            try
            {
                if (objLocation != null)
                {
                    //var IsParent = _LocationService.GetLocations().Where(c => c.LocationId == objLocation.LocationId).FirstOrDefault();
                    //if (IsParent == null)
                    //{
                    _LocationService.DeleteLocation(objLocation);
                    TempData["ShowMessage"] = "success";
                    TempData["MessageBody"] = "Location successfully deleted.";
                    RedirectToAction("Index");
                    //}
                    //else
                    //{
                    //    TempData["ShowMessage"] = "error";
                    //    TempData["MessageBody"] = "First delete the childs of this Location.";
                    //    RedirectToAction("Index");
                    //}
                    return RedirectToAction("Index");
                }

            }
            catch (Exception ex)
            {
                ErrorLogging.LogError(ex);
                RedirectToAction("Index");
            }
            return RedirectToAction("Index");

        }
        #endregion

        public ActionResult Details(int Id, string CurrentPageNo)
        {

            UserPermissionAction("User", RoleAction.view.ToString());
            CheckPermission();
            var location = _LocationService.GetLocations().Where(l => l.LocationId == Id).FirstOrDefault();
            var model = new LocationResponseAdminModel();

            if (location != null)
            {
                List<string> LocationImages = new List<string>();
                Mapper.CreateMap<Friendlier.Entity.Location, Friendlier.Models.LocationResponseAdminModel>();
                LocationResponseAdminModel LocationResponseModel = Mapper.Map<Friendlier.Entity.Location, Friendlier.Models.LocationResponseAdminModel>(location);
                string[] categoryId = LocationResponseModel.CategoryIds.Split(',');
                string categoryNames = "";
                foreach (var item in categoryId)
                {
                    var categoryName = _CategoryService.GetCategories().Where(c => c.CategoryId == Convert.ToInt32(item)).Select(c => c.Name).FirstOrDefault();
                    categoryNames = categoryNames + ',' + categoryName;
                }
                LocationResponseModel.CategoryNames = categoryNames.TrimStart(',').TrimEnd(',');
                var images = _LocationImagesService.GetLocationImages().Where(l => l.LocationId == Id).ToList();
                if (images.Count() > 0)
                {
                    foreach (var image in images)
                    {
                        LocationImages.Add(image.ImagePath);
                    }
                }
                // LocationResponseModel.ContactInfo = Location.MobileNo + "|" + Location.EmailId;
                LocationResponseModel.LocationImages = LocationImages;
                var tags = _LocationTagService.GetLocationTags().Where(t => t.LocationId == Id).Select(t => t.Tag).ToList();
                if (tags.Count() > 0)
                {
                    string tagList = "";
                    foreach (var tag in tags)
                    {
                        tagList = tagList + "," + tag;
                    }
                    LocationResponseModel.Tags = tagList.TrimEnd(',').TrimStart(',');
                }

                var customer = _CustomerService.GetCustomer(location.CustomerId);
                LocationResponseModel.Name = customer.FirstName + " " + customer.LastName;
                return View(LocationResponseModel);
            }
            else
            {
                TempData["ShowMessage"] = "error";
                TempData["MessageBody"] = " Location has no details.";
                return RedirectToAction("Index");
            }

        }


        public ActionResult ApproveEvents(int? Id)
        {
            if (Id != 0)
            {
                var location = _LocationService.GetLocations().Where(l => l.LocationId == Id).FirstOrDefault();

                if (location != null)
                {
                    location.IsApproved = true;
                    location.Status = EnumValue.GetEnumDescription(EnumValue.LocationStatus.Approved);
                    _LocationService.UpdateLocation(location);
                    CommonCls.SendMailToUser("", "", "");
                }
                TempData["ShowMessage"] = "success";
                TempData["MessageBody"] = " Location approved  successfully.";
                return RedirectToAction("Index");
            }



            else
            {
                TempData["ShowMessage"] = "error";
                TempData["MessageBody"] = " Location not found. ";
                return RedirectToAction("Details", new { Id = Id });
            }

        }

         [HttpPost]
        public ActionResult DisapproveEvents(int? Id, string ReasonToDisapprove)
        {
            try
            {


                if (Id != 0)
                {
                    var location = _LocationService.GetLocations().Where(l => l.LocationId == Id).FirstOrDefault();

                    if (location != null)
                    {
                        location.IsApproved = false;
                        location.Status = EnumValue.GetEnumDescription(EnumValue.LocationStatus.Disapproved);
                        _LocationService.UpdateLocation(location);
                    }
                    var customer = _CustomerService.GetCustomers().Where(l => l.CustomerId == location.CustomerId).FirstOrDefault();
                    SendMailToUser(customer.FirstName, location.Title, location.EmailId, ReasonToDisapprove);
                    TempData["ShowMessage"] = "success";
                    TempData["MessageBody"] = " Location disapproved  successfully.";
                    return Json(new { success = true, responseText = "Location disapproved  successfully." }, JsonRequestBehavior.AllowGet);
                }



                else
                {
                    TempData["ShowMessage"] = "error";
                    TempData["MessageBody"] = " Location not found. ";
                    return Json(new { success = false, responseText = "error." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {

                string ErrorMsg = ex.Message.ToString();
                ErrorLogging.LogError(ex);
                return Json(new { success = false, responseText = "error." }, JsonRequestBehavior.AllowGet);
            }
        }
        public void SendMailToUser(string UserName, string title, string EmailAddress, string ReasonToDisapprove)
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
                mail.Subject = "Location disapproved";
                string msgbody = "";
                msgbody = msgbody + "<br />";
                msgbody = msgbody + "<table style='width:80%'>";
                msgbody = msgbody + "<tr>";

                msgbody = msgbody + "<td align='left' style=' font-family:Arial; font-weight:bold; font-size:15px;'>The location named " + title + "  you have posted on Friendlier App has been disapproved for the below reason:<br /></td></tr>";
                msgbody = msgbody + "<tr><td align='left'>";
                msgbody = msgbody + "<br /><font style=' font-family:Arial; font-size:13px;'><b>Reason: </b>" + ReasonToDisapprove + "</font><br /><br />";
                //msgbody = msgbody + "<font style=' font-family:Arial; font-size:13px;'><b>EmailAddress: </b>" + EmailAddress + "</font><br /><br />";

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