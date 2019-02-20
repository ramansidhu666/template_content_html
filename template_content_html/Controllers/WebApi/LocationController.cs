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
using Friendlier.Entity;
using Friendlier.Services;
using Newtonsoft.Json;
using Friendlier.Models;
using AutoMapper;
using Friendlier.Infrastructure;
using System.Globalization;
using Friendlier.Core.Infrastructure;
using System.Net.Mail;
using System.Web.Configuration;
using System.Configuration;

namespace Friendlier.Controllers.WebApi
{
    [RoutePrefix("Location")]
    public class LocationController : ApiController
    {
       
        public ILocationService _LocationService { get; set; }
        public ILocationImagesService _LocationImagesService { get; set; }
        public ICustomerService _CustomerService { get; set; }
        public ILocationTagService _LocationTagService { get; set; }
        public ILocationPurchaseService _LocationPurchaseService { get; set; }
        public INotificationService _NotificationService { get; set; }
        public ICategoryService _CategoryService { get; set; }
        public LocationController(ICategoryService CategoryService, ILocationService LocationService, ILocationImagesService LocationImagesService, ILocationTagService LocationTagService, ICustomerService CustomerService, ILocationPurchaseService LocationPurchaseService, INotificationService NotificationService)
        {
            this._LocationService = LocationService;
            this._LocationImagesService = LocationImagesService;
            this._CustomerService = CustomerService;
            this._LocationPurchaseService = LocationPurchaseService;
            this._LocationTagService = LocationTagService;
            this._NotificationService = NotificationService;
            this._CategoryService = CategoryService;
        }


        [Route("SaveLocation")]
        [HttpPost]
        public HttpResponseMessage SaveLocation([FromBody]LocationModel LocationModel)
        {
            int locationId = 0;
            try
            {
                if (LocationModel.CustomerId == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer Id is blank"), Configuration.Formatters.JsonFormatter);
                }
                if (LocationModel.Title == null || LocationModel.Title == "")
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Title is blank"), Configuration.Formatters.JsonFormatter);
                }
                if (LocationModel.Description == null || LocationModel.Description == "")
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Description is blank"), Configuration.Formatters.JsonFormatter);
                }
                //if (LocationModel.ContactInfo == null && LocationModel.ContactInfo == "")
                //{
                //    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "ContactInfo is blank"), Configuration.Formatters.JsonFormatter);
                //}
                if (LocationModel.Street == null || LocationModel.Street == "")
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Street is blank"), Configuration.Formatters.JsonFormatter);
                }
                if (LocationModel.City == null || LocationModel.City == "")
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "City is blank"), Configuration.Formatters.JsonFormatter);
                }
                if (LocationModel.State == null || LocationModel.State == "")
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "State is blank"), Configuration.Formatters.JsonFormatter);
                }
                if (LocationModel.Country == null || LocationModel.Country == "")
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Country is blank"), Configuration.Formatters.JsonFormatter);
                }
                if (LocationModel.CategoryIds == null || LocationModel.CategoryIds == "")
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "CategoryIds is blank"), Configuration.Formatters.JsonFormatter);
                }
                if (LocationModel.Ratings == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Ratings is blank"), Configuration.Formatters.JsonFormatter);
                }
                
                var customer = _CustomerService.GetCustomers().Where(c => c.CustomerId == LocationModel.CustomerId && c.IsActive == true).FirstOrDefault();
                if (customer != null)
                {
                    var street = _LocationService.GetLocations().Where(l => l.Street.ToLower()==LocationModel.Street.ToLower()).Distinct().ToList();
                    if (street.Count() > 0)
                    {

                        var entity1 = street.Where(l => l.City.ToLower() == LocationModel.City.ToLower()).Distinct().ToList();
                        if (entity1.Count() > 0)
                        {
                            var entity2 = street.Where(l => l.State.ToLower() == LocationModel.State.ToLower()).Distinct().ToList();
                            if (entity2.Count() > 0)
                            {

                                var entity3 = street.Where(l => l.Country.ToLower() == LocationModel.Country.ToLower()).Distinct().ToList();
                                if (entity3.Count() > 0)
                                {
                                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "this location already exists."), Configuration.Formatters.JsonFormatter);
                                }

                            }

                        }

                    }
                   

                        string[] categoryIds = LocationModel.CategoryIds.Split(',');

                        List<int> CatIds = _CategoryService.GetCategories().Select(c => c.CategoryId).ToList();
                        // var CatIds = Categories.Select(c => c.CategoryId);
                        List<string> Ids = CatIds.ConvertAll<string>(x => x.ToString());
                        bool isMatched = true;
                        foreach (var item in categoryIds)
                        {
                            if (!Ids.Contains(item))
                            {
                                isMatched = false;
                            }
                        }
                        IEnumerable<string> difference = Ids.Except(categoryIds);
                        if (isMatched)
                        {
                            Mapper.CreateMap<Friendlier.Models.LocationModel, Friendlier.Entity.Location>();
                            Friendlier.Entity.Location location = Mapper.Map<Friendlier.Models.LocationModel, Friendlier.Entity.Location>(LocationModel);
                            //string[] contactDetails = LocationModel.ContactInfo.Split('|');
                            //location.EmailId = contactDetails[1];
                            //location.MobileNo = contactDetails[0];
                            location.EmailId = LocationModel.EmailId;
                            location.MobileNo = LocationModel.MobileNo;
                            location.CategoryIds = LocationModel.CategoryIds;
                            location.IsApproved = false;
                            location.Status = EnumValue.GetEnumDescription(EnumValue.LocationStatus.New);
                            _LocationService.InsertLocation(location);
                            locationId = location.LocationId;
                            if (LocationModel.Tags != null && LocationModel.Tags != "")
                            {
                                string[] tagList = LocationModel.Tags.Split(',');
                                foreach (var tag in tagList)
                                {

                                    LocationTag locationTag = new LocationTag();
                                    locationTag.LocationId = locationId;
                                    locationTag.Tag = tag;
                                    _LocationTagService.InsertLocationTag(locationTag);

                                }
                            }


                            Mapper.CreateMap<Location, LocationResponseModel>();
                            LocationResponseModel model = Mapper.Map<Location, LocationResponseModel>(location);
                            model.Tags = LocationModel.Tags;
                            SendMailToAdmin(customer.FirstName, model.Title, model.Street + " " + model.City + " " + model.State + " " + model.Country);
                            //model.ContactInfo = location.MobileNo + "|" + location.EmailId;
                            return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", model), Configuration.Formatters.JsonFormatter);

                        }

                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Category id is wrong."), Configuration.Formatters.JsonFormatter);
                        }
                    }
             
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer id is wrong."), Configuration.Formatters.JsonFormatter);
                }


            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();
                ErrorLogging.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Please try again."), Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("UpdateLocation")]
        [HttpPost]
        public HttpResponseMessage UpdateLocation([FromBody]LocationModel LocationModel)
        {
            try
            {
                if (LocationModel.CustomerId == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer Id is blank"), Configuration.Formatters.JsonFormatter);
                }
                var customer = _CustomerService.GetCustomers().Where(c => c.CustomerId == LocationModel.CustomerId && c.IsActive == true).FirstOrDefault();
                if (customer == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer not found."), Configuration.Formatters.JsonFormatter);
                }
                if (LocationModel.LocationId == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Location Id is blank"), Configuration.Formatters.JsonFormatter);
                }
                var location = _LocationService.GetLocations().Where(l => l.LocationId == LocationModel.LocationId && l.CustomerId == LocationModel.CustomerId).FirstOrDefault();
                if (location == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Location not found."), Configuration.Formatters.JsonFormatter);
                }
                else
                {
                    location.Title = ((LocationModel.Title != null && LocationModel.Title != "") ? LocationModel.Title : location.Title);
                    location.Description = ((LocationModel.Description != null && LocationModel.Description != "") ? LocationModel.Description : location.Description);
                    //if (LocationModel.ContactInfo != null && LocationModel.ContactInfo != "")
                    //{
                    //    string[] contactDetails = LocationModel.ContactInfo.Split('|');
                    //    location.EmailId = contactDetails[1];
                    //    location.MobileNo = contactDetails[0];
                    //}
                    location.EmailId = ((LocationModel.EmailId != null && LocationModel.EmailId != "") ? LocationModel.EmailId : location.EmailId);
                    location.MobileNo = ((LocationModel.MobileNo != null && LocationModel.MobileNo != "") ? LocationModel.MobileNo : location.MobileNo);
                    location.State = ((LocationModel.State != null && LocationModel.State != "") ? LocationModel.State : location.State);
                    location.City = ((LocationModel.City != null && LocationModel.City != "") ? LocationModel.City : location.City);
                    location.Country = ((LocationModel.Country != null && LocationModel.Country != "") ? LocationModel.Country : location.Country);
                    location.Street = ((LocationModel.Street != null && LocationModel.Street != "") ? LocationModel.Street : location.Street);
                    location.Ratings = (LocationModel.Ratings != null ? LocationModel.Ratings : location.Ratings);
                    if (LocationModel.CategoryIds != null && LocationModel.CategoryIds != "")
                    {
                        string[] categoryIds = LocationModel.CategoryIds.Split(',');

                        List<int> CatIds = _CategoryService.GetCategories().Select(c => c.CategoryId).ToList();
                        // var CatIds = Categories.Select(c => c.CategoryId);
                        List<string> Ids = CatIds.ConvertAll<string>(x => x.ToString());
                        bool isMatched = true;
                        foreach (var item in categoryIds)
                        {
                            if (!Ids.Contains(item))
                            {
                                isMatched = false;
                            }
                        }
                        IEnumerable<string> difference = Ids.Except(categoryIds);
                        if (isMatched)
                        {
                            location.CategoryIds = LocationModel.CategoryIds;
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Category id is wrong."), Configuration.Formatters.JsonFormatter);
                        }
                    }
                    //location.CategoryIds = ((LocationModel.CategoryIds != null && LocationModel.CategoryIds != "") ? LocationModel.CategoryIds : location.CategoryIds);

                    _LocationService.UpdateLocation(location);
                    string tags = "";
                    var LocationTag = _LocationTagService.GetLocationTags().Where(t => t.LocationId == location.LocationId).ToList();
                    if (LocationModel.Tags != null || LocationModel.Tags != "")
                    {
                        string[] tagList = LocationModel.Tags.Split(',');


                        if (LocationTag.Count() > 0)
                        {
                            foreach (var tag in LocationTag)
                            {
                                _LocationTagService.DeleteLocationTag(tag);
                            }


                        }
                        foreach (var tag in tagList)
                        {

                            LocationTag locationTag = new LocationTag();
                            locationTag.LocationId = location.LocationId;
                            locationTag.Tag = tag;
                            _LocationTagService.InsertLocationTag(locationTag);
                            tags = tags + "," + tag;
                        }


                    }

                    Mapper.CreateMap<Location, LocationResponseModel>();
                    LocationResponseModel model = Mapper.Map<Location, LocationResponseModel>(location);
                    string[] categoryId = model.CategoryIds.Split(',');
                    string categoryNames = "";
                    foreach (var item in categoryId)
                    {
                        var categoryName = _CategoryService.GetCategories().Where(c => c.CategoryId == Convert.ToInt32(item)).Select(c => c.Name).FirstOrDefault();
                        categoryNames = categoryNames + ',' + categoryName;
                    }
                    model.CategoryNames = categoryNames.TrimStart(',').TrimEnd(',');
                    List<string> LocationImages = new List<string>();
                    var images = _LocationImagesService.GetLocationImages().Where(l => l.LocationId == location.LocationId).ToList();
                    if (images.Count() > 0)
                    {

                        foreach (var image in images)
                        {
                            LocationImages.Add(image.ImagePath);
                        }
                    }
                    model.LocationImages = LocationImages;

                    model.Tags = tags.TrimStart(',').TrimEnd(',');
                    // model.ContactInfo = location.MobileNo + "|" + location.EmailId;
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", model), Configuration.Formatters.JsonFormatter);
                }


            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();
                ErrorLogging.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Please try again."), Configuration.Formatters.JsonFormatter);
            }
        }

        //[Route("GetAllLocations")]
        //[HttpGet]
        //public HttpResponseMessage GetAllLocations()
        //{
        //    try
        //    {
        //        var entity1 = _LocationService.GetLocations().Where(l => l.IsApproved == true);
        //        var entity2 = _CustomerService.GetCustomers().Where(c => c.IsActive == true);

        //        var Locations = (from x in entity1
        //                         join y in entity2
        //                         on x.CustomerId equals y.CustomerId
        //                         select x).ToList();
        //        //var Locations = _LocationService.GetLocations().Where(l=>l.IsApproved==true);

        //        var models = new List<LocationResponseModel>();
        //        Mapper.CreateMap<Friendlier.Entity.Location, Friendlier.Models.LocationResponseModel>();
        //        foreach (var Location in Locations)
        //        {
        //            List<string> LocationImages = new List<string>();
        //            var LocationModel = Mapper.Map<Friendlier.Entity.Location, Friendlier.Models.LocationResponseModel>(Location);
        //            //LocationModel.ContactInfo = Location.MobileNo + "|" + Location.EmailId;
        //            var images = _LocationImagesService.GetLocationImages().Where(l => l.LocationId == Location.LocationId).ToList();
        //            if (images.Count() > 0)
        //            {
        //                foreach (var image in images)
        //                {
        //                    LocationImages.Add(image.ImagePath);
        //                }
        //            }
        //            // LocationResponseModel.ContactInfo = Location.MobileNo + "|" + Location.EmailId;
        //            LocationModel.LocationImages = LocationImages;
        //            LocationModel.CategoryNames = "";
        //            models.Add(LocationModel);
        //        }

        //        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", models), Configuration.Formatters.JsonFormatter);
        //    }
        //    catch (Exception ex)
        //    {
        //        string ErrorMsg = ex.Message.ToString();
        //        ErrorLogging.LogError(ex);
        //        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Please try again."), Configuration.Formatters.JsonFormatter);
        //    }

        //}



        [Route("GetAllLocations")]
        [HttpPost]
        public HttpResponseMessage GetAllLocations(LocationModel LocationModel)
        {
            try
            {
                if (LocationModel.City == "" || LocationModel.City == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "City is blank."), Configuration.Formatters.JsonFormatter);
                }
                if (LocationModel.Country == "" || LocationModel.City == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Country is blank."), Configuration.Formatters.JsonFormatter);
                }
                if (LocationModel.State == "" || LocationModel.City == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "State is blank."), Configuration.Formatters.JsonFormatter);
                }
                List<Location> entity4 = new List<Location>();
                var entity1 = _LocationService.GetLocations().Where(l => l.City.ToLower().Contains(LocationModel.City.ToLower()) && l.IsApproved == true).Distinct().ToList();
                if (entity1.Count() > 0)
                {
                    var entity2 = entity1.Where(l => l.State.ToLower().Contains(LocationModel.State.ToLower())).Distinct().ToList();
                    if (entity2.Count() > 0)
                    {
                        var entity3 = entity2.Where(l => l.Country.ToLower().Contains(LocationModel.Country.ToLower())).Distinct().ToList();
                        if (entity3.Count() > 0)
                        {
                            entity4 = entity3;
                        }
                        else
                        {
                            entity4 = entity2;
                        }
                    }
                    else
                    {
                        entity4 = entity1;
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "No locations found."), Configuration.Formatters.JsonFormatter);
                }

                //var entity1 = _LocationService.GetLocations().Where(l => l.IsApproved == true);
                var entity5 = _CustomerService.GetCustomers().Where(c => c.IsActive == true);

                var Locations = (from x in entity4
                                 join y in entity5
                                 on x.CustomerId equals y.CustomerId
                                 select x).ToList();
                //var Locations = _LocationService.GetLocations().Where(l=>l.IsApproved==true);

                var models = new List<LocationResponseModel>();
                Mapper.CreateMap<Friendlier.Entity.Location, Friendlier.Models.LocationResponseModel>();
                foreach (var Location in Locations)
                {
                    List<string> LocationImages = new List<string>();
                    var LocationModl = Mapper.Map<Friendlier.Entity.Location, Friendlier.Models.LocationResponseModel>(Location);
                    //LocationModel.ContactInfo = Location.MobileNo + "|" + Location.EmailId;
                    var images = _LocationImagesService.GetLocationImages().Where(l => l.LocationId == Location.LocationId).ToList();
                    if (images.Count() > 0)
                    {
                        foreach (var image in images)
                        {
                            LocationImages.Add(image.ImagePath);
                        }
                    }
                    // LocationResponseModel.ContactInfo = Location.MobileNo + "|" + Location.EmailId;
                    LocationModl.LocationImages = LocationImages;
                    LocationModl.CategoryNames = "";
                    models.Add(LocationModl);
                }

                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", models), Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();
                ErrorLogging.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Please try again."), Configuration.Formatters.JsonFormatter);
            }

        }


        [Route("GetLocationByID")]
        [HttpGet]
        public HttpResponseMessage GetLocationByID([FromUri] int LocationId, [FromUri] int CustomerId)
        {
            try
            {
                var customer = _CustomerService.GetCustomers().Where(c => c.CustomerId == CustomerId && c.IsActive == true).FirstOrDefault();
                if (customer == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer not found."), Configuration.Formatters.JsonFormatter);
                }
                List<string> LocationImages = new List<string>();
                var Location = _LocationService.GetLocation(LocationId);
                if (Location != null)
                {
                    Mapper.CreateMap<Friendlier.Entity.Location, Friendlier.Models.LocationResponseModel>();
                    LocationResponseModel LocationResponseModel = Mapper.Map<Friendlier.Entity.Location, Friendlier.Models.LocationResponseModel>(Location);
                    //LocationResponseModel.ContactInfo = Location.MobileNo + "|" + Location.EmailId;
                    var images = _LocationImagesService.GetLocationImages().Where(l => l.LocationId == LocationId).ToList();
                    if (images.Count() > 0)
                    {
                        foreach (var image in images)
                        {
                            LocationImages.Add(image.ImagePath);
                        }
                    }
                    // LocationResponseModel.ContactInfo = Location.MobileNo + "|" + Location.EmailId;
                    LocationResponseModel.LocationImages = LocationImages;
                    var tags = _LocationTagService.GetLocationTags().Where(t => t.LocationId == LocationId).Select(t => t.Tag).ToList();
                    if (tags.Count() > 0)
                    {
                        string tagList = "";
                        foreach (var tag in tags)
                        {
                            tagList = tagList + "," + tag;
                        }
                        LocationResponseModel.Tags = tagList.TrimEnd(',').TrimStart(',');
                    }
                    var isFavourite = _FavoriteService.GetFavorites().Where(f => f.LocationId == LocationId && f.CustomerId == CustomerId).FirstOrDefault();
                    if (isFavourite == null)
                    {
                        LocationResponseModel.IsFavourite = false;
                    }
                    else
                    {
                        LocationResponseModel.IsFavourite = true;
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", LocationResponseModel), Configuration.Formatters.JsonFormatter);

                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Location not found."), Configuration.Formatters.JsonFormatter);
                }
            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();
                ErrorLogging.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "User not found."), Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("GetLocationByCustomerId")]
        [HttpGet]
        public HttpResponseMessage GetLocationByCustomerId([FromUri] int CustomerId)
        {
            try
            {
                var models = new List<LocationResponseModel>();
                var customer = _CustomerService.GetCustomers().Where(c => c.CustomerId == CustomerId && c.IsActive == true).FirstOrDefault();
                if (customer == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer not found."), Configuration.Formatters.JsonFormatter);
                }
                var Locations = _LocationService.GetLocations().Where(l => l.CustomerId == CustomerId && l.IsApproved == true).ToList();
                if (Locations.Count() > 0)
                {
                    foreach (var Location in Locations)
                    {

                        List<string> LocationImages = new List<string>();
                        Mapper.CreateMap<Friendlier.Entity.Location, Friendlier.Models.LocationResponseModel>();
                        LocationResponseModel LocationResponseModel = Mapper.Map<Friendlier.Entity.Location, Friendlier.Models.LocationResponseModel>(Location);
                        var images = _LocationImagesService.GetLocationImages().Where(l => l.LocationId == Location.LocationId).ToList();
                        if (images.Count() > 0)
                        {

                            foreach (var image in images)
                            {
                                LocationImages.Add(image.ImagePath);
                            }
                        }
                        //LocationResponseModel.ContactInfo = Location.MobileNo + "|" + Location.EmailId;
                        LocationResponseModel.LocationImages = LocationImages;
                        var isFavourite = _FavoriteService.GetFavorites().Where(f => f.LocationId == Location.LocationId && f.CustomerId == CustomerId).FirstOrDefault();
                        if (isFavourite == null)
                        {
                            LocationResponseModel.IsFavourite = false;
                        }
                        else
                        {
                            LocationResponseModel.IsFavourite = true;
                        }

                        var tags = _LocationTagService.GetLocationTags().Where(t => t.LocationId == Location.LocationId).Select(t => t.Tag).ToList();
                        if (tags.Count() > 0)
                        {
                            string tagList = "";
                            foreach (var tag in tags)
                            {
                                tagList = tagList + "," + tag;
                            }
                            LocationResponseModel.Tags = tagList.TrimEnd(',').TrimStart(',');
                        }
                        models.Add(LocationResponseModel);

                    }
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", models), Configuration.Formatters.JsonFormatter);

                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Location not found."), Configuration.Formatters.JsonFormatter);
                }
            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();
                ErrorLogging.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "User not found."), Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("GetAllCategories")]
        [HttpGet]
        public HttpResponseMessage GetAllCategories()
        {
            try
            {
                var categories = _CategoryService.GetCategories().ToList();

                var models = new List<categoryResponseModule>();
                Mapper.CreateMap<Friendlier.Entity.Category, Friendlier.Models.categoryResponseModule>();
                foreach (var category in categories)
                {
                    var categoryModel = Mapper.Map<Friendlier.Entity.Category, Friendlier.Models.categoryResponseModule>(category);

                    models.Add(categoryModel);
                }

                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", models), Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();
                ErrorLogging.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Please try again."), Configuration.Formatters.JsonFormatter);
            }

        }

        [Route("SearchLocation")]
        [HttpGet]
        public HttpResponseMessage SearchLocation([FromUri]string searchTerm)
        {
            var models = new List<LocationResponseModel>();
            try
            {
                if (searchTerm == null || searchTerm == "")
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "searchTerm is blank"), Configuration.Formatters.JsonFormatter);
                }
                string[] searchList = searchTerm.ToLower().Split(' ');
                List<int> locationIdList = _LocationTagService.GetLocationTags().Where(x => x.Tag.ToLower().Contains(searchTerm.ToLower())).Select(x => x.LocationId).Distinct().ToList();

                //  var locationsList = _LocationService.GetLocations().Where(l => l.Title.ToLower().Contains(searchTerm.ToLower()) || l.Address.ToLower().Contains(searchTerm.ToLower()) || locationIdList.Contains(l.LocationId) && l.IsApproved==true).ToList();
                var locationsList = _LocationService.GetLocations().Where(l => l.Title.ToLower().Contains(searchTerm.ToLower()) || searchList.Contains(l.City.ToLower()) || searchList.Contains(l.Street.ToLower()) || searchList.Contains(l.Country.ToLower()) || searchList.Contains(l.State.ToLower()) || locationIdList.Contains(l.LocationId) && l.IsApproved == true).ToList();
                var entity1 = locationsList;
                var entity2 = _CustomerService.GetCustomers().Where(c => c.IsActive == true);

                var locations = (from x in entity1
                                 join y in entity2
                                 on x.CustomerId equals y.CustomerId
                                 select x).ToList();


                if (locations.Count > 0)
                {
                    Mapper.CreateMap<Location, LocationResponseModel>();
                    //var tags = _LocationTagService.GetLocationTags();
                    //if (tags != null)
                    //{
                    //    tags = tags.Where(t => t.Tag.ToLower().Contains(searchTerm.ToLower())).ToList();

                    //    foreach (var tag in tags)
                    //    {
                    //        var locationList = _LocationService.GetLocations().Where(l => l.LocationId == tag.LocationId).FirstOrDefault();
                    //        LocationResponseModel loc = Mapper.Map<Location, LocationResponseModel>(locationList);
                    //        models.Add(loc);
                    //    }

                    //}
                    //locations = locations.Where(l => l.Title.ToLower().Contains(searchTerm.ToLower())).ToList();
                    foreach (var location in locations)
                    {
                        LocationResponseModel loc = Mapper.Map<Location, LocationResponseModel>(location);
                        var tags = _LocationTagService.GetLocationTags().Where(t => t.LocationId == loc.LocationId).Select(t => t.Tag).ToList();
                        if (tags.Count() > 0)
                        {
                            string tagList = "";
                            foreach (var tag in tags)
                            {
                                tagList = tagList + "," + tag;
                            }
                            loc.Tags = tagList.TrimEnd(',').TrimStart(',');
                        }
                        models.Add(loc);
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", models), Configuration.Formatters.JsonFormatter);


                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "No location found."), Configuration.Formatters.JsonFormatter);
                }
            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();
                ErrorLogging.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Please try again."), Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("SavePurchasedLocation")]
        [HttpPost]
        public HttpResponseMessage SavePurchasedLocation([FromBody]LocationModel LocationModel)
        {
            try
            {
                if (LocationModel.CustomerId == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer Id is blank"), Configuration.Formatters.JsonFormatter);
                }
                if (LocationModel.LocationId == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Location Id is blank"), Configuration.Formatters.JsonFormatter);
                }

                var customer = _CustomerService.GetCustomers().Where(c => c.CustomerId == LocationModel.CustomerId && c.IsActive == true).FirstOrDefault();
                if (customer != null)
                {
                    var location = _LocationService.GetLocations().Where(l => l.LocationId == LocationModel.LocationId).FirstOrDefault();
                    if (location != null)
                    {
                        location.IsPurchased = true;
                        location.PurchasedById = LocationModel.CustomerId;
                        _LocationService.UpdateLocation(location);
                        //Mapper.CreateMap<Location, LocationResponseModel>();
                        //LocationResponseModel LocationResponseModel = Mapper.Map<Location, LocationResponseModel>(location);

                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", "Successfully purchased."), Configuration.Formatters.JsonFormatter);
                    }
                    else
                    {

                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "No location found."), Configuration.Formatters.JsonFormatter);
                    }


                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer not found."), Configuration.Formatters.JsonFormatter);
                }


            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();
                ErrorLogging.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Please try again."), Configuration.Formatters.JsonFormatter);
            }
        }


        [Route("GetPurchasedLocationsByCustomerId")]
        [HttpGet]
        public HttpResponseMessage GetPurchasedLocationsByCustomerId([FromUri] int CustomerId)
        {
            try
            {
                var models = new List<LocationResponseModel>();
                var customer = _CustomerService.GetCustomers().Where(c => c.CustomerId == CustomerId && c.IsActive == true).FirstOrDefault();
                if (customer == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer not found."), Configuration.Formatters.JsonFormatter);
                }
                var locations = _LocationService.GetLocations().Where(l => l.PurchasedById == CustomerId).ToList();
                if (locations.Count() > 0)
                {
                    foreach (var Location in locations)
                    {
                        List<string> LocationImages = new List<string>();
                        Mapper.CreateMap<Friendlier.Entity.Location, Friendlier.Models.LocationResponseModel>();
                        LocationResponseModel LocationResponseModel = Mapper.Map<Friendlier.Entity.Location, Friendlier.Models.LocationResponseModel>(Location);
                        var images = _LocationImagesService.GetLocationImages().Where(l => l.LocationId == Location.LocationId).ToList();
                        if (images.Count() > 0)
                        {

                            foreach (var image in images)
                            {
                                LocationImages.Add(image.ImagePath);
                            }
                        }
                        //LocationResponseModel.ContactInfo = Location.MobileNo + "|" + Location.EmailId;
                        LocationResponseModel.LocationImages = LocationImages;

                        var tags = _LocationTagService.GetLocationTags().Where(t => t.LocationId == Location.LocationId).Select(t => t.Tag).ToList();
                        if (tags.Count() > 0)
                        {
                            string tagList = "";
                            foreach (var tag in tags)
                            {
                                tagList = tagList + "," + tag;
                            }
                            LocationResponseModel.Tags = tagList.TrimEnd(',').TrimStart(',');
                        }
                        models.Add(LocationResponseModel);

                    }
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", models), Configuration.Formatters.JsonFormatter);

                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Location not found."), Configuration.Formatters.JsonFormatter);
                }
            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();
                ErrorLogging.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "User not found."), Configuration.Formatters.JsonFormatter);
            }
        }


        public void SendMailToAdmin(string UserName, string title, string address)
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

                mail.To.Add(new MailAddress(ToEmailID));

                mail.From = new MailAddress(FromEmailID);
                mail.Subject = "New location posted";
                string msgbody = "";
                msgbody = msgbody + "<br />";
                msgbody = msgbody + "<table style='width:80%'>";
                msgbody = msgbody + "<tr>";

                msgbody = msgbody + "<td align='left' style=' font-family:Arial; font-weight:bold; font-size:15px;'>New location have recently posted on Friendlier App by " + UserName + ".<br /></td></tr>";
                msgbody = msgbody + "<td align='left' style=' font-family:Arial; font-weight:bold; font-size:15px;'>Please Find location details below:<br /></td></tr>";
                msgbody = msgbody + "<tr><td align='left'>";
                msgbody = msgbody + "<br /><font style=' font-family:Arial; font-size:13px;'><b>Location Title: </b>" + title + "</font><br /><br />";
                msgbody = msgbody + "<font style=' font-family:Arial; font-size:13px;'><b>Location Address: </b>" + address + "</font><br /><br />";

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

