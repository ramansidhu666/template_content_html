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

namespace Friendlier.Controllers.WebApi
{
    [RoutePrefix("Favourite")]
    public class FavoriteController : ApiController
    {
        public IFavoriteService _FavoriteService { get; set; }
        public ILocationService _LocationService { get; set; }
        public ILocationImagesService _LocationImagesService { get; set; }
        public ICustomerService _CustomerService { get; set; }
        public ILocationTagService _LocationTagService { get; set; }
        public ILocationPurchaseService _LocationPurchaseService { get; set; }
        public INotificationService _NotificationService { get; set; }
        public ICategoryService _CategoryService { get; set; }
        public FavoriteController(IFavoriteService FavoriteService, ICategoryService CategoryService, ILocationService LocationService, ILocationImagesService LocationImagesService, ILocationTagService LocationTagService, ICustomerService CustomerService, ILocationPurchaseService LocationPurchaseService, INotificationService NotificationService)
        {
            this._FavoriteService = FavoriteService;
            this._LocationService = LocationService;
            this._LocationImagesService = LocationImagesService;
            this._CustomerService = CustomerService;
            this._LocationPurchaseService = LocationPurchaseService;
            this._LocationTagService = LocationTagService;
            this._NotificationService = NotificationService;
            this._CategoryService = CategoryService;
        }


        [Route("SaveFavourite")]
        [HttpPost]
        public HttpResponseMessage SaveFavourite([FromBody]FavoriteModel FavouriteModel)
        {
            try
            {
                if (FavouriteModel.CustomerId == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer Id is blank"), Configuration.Formatters.JsonFormatter);
                }

                if (FavouriteModel.LocationId == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Location Id is blank"), Configuration.Formatters.JsonFormatter);
                }
                var customer = _CustomerService.GetCustomers().Where(c => c.CustomerId == FavouriteModel.CustomerId && c.IsActive==true).FirstOrDefault();
                if (customer != null)
                {
                    var favorite = _FavoriteService.GetFavorites().Where(f => f.CustomerId == FavouriteModel.CustomerId && f.LocationId == FavouriteModel.LocationId).FirstOrDefault();
                    if (favorite != null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Already exists."), Configuration.Formatters.JsonFormatter);

                    }
                    Mapper.CreateMap<FavoriteModel, HobbiesAndInterests>();
                    HobbiesAndInterests favorites = Mapper.Map<FavoriteModel, HobbiesAndInterests>(FavouriteModel);
                    _FavoriteService.InsertFavorite(favorites);
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", "Successfully added."), Configuration.Formatters.JsonFormatter);
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

        //[Route("UpdateFavourite")]
        //[HttpPost]
        //public HttpResponseMessage UpdateFavourite([FromBody]FavouriteModel FavouriteModel)
        //{
        //    try
        //    {
        //        if (FavouriteModel.CustomerId == 0)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer Id is blank"), Configuration.Formatters.JsonFormatter);
        //        }
        //        if (FavouriteModel.FavouriteId == 0)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Favourite Id is blank"), Configuration.Formatters.JsonFormatter);
        //        }
        //        var Favourite = _FavouriteService.GetFavourite(FavouriteModel.FavouriteId);
        //        if (Favourite == null)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Favourite not found."), Configuration.Formatters.JsonFormatter);
        //        }
        //        else
        //        {
        //            Favourite.Title = ((FavouriteModel.Title != null && FavouriteModel.Title != "") ? FavouriteModel.Title : Favourite.Title);
        //            Favourite.Description = ((FavouriteModel.Description != null && FavouriteModel.Description != "") ? FavouriteModel.Description : Favourite.Description);
        //            if (FavouriteModel.ContactInfo != null && FavouriteModel.ContactInfo != "")
        //            {
        //                string[] contactDetails = FavouriteModel.ContactInfo.Split('|');
        //                Favourite.EmailId = contactDetails[1];
        //                Favourite.MobileNo = contactDetails[0];
        //            }

        //            Favourite.Address = ((FavouriteModel.Address != null && FavouriteModel.Address != "") ? FavouriteModel.Address : Favourite.Address);
        //            Favourite.Ratings = (FavouriteModel.Ratings != null ? FavouriteModel.Ratings : Favourite.Ratings);
        //            if (FavouriteModel.CategoryIds != null && FavouriteModel.CategoryIds != "")
        //            {
        //                string[] categoryIds = FavouriteModel.CategoryIds.Split(',');

        //                List<int> CatIds = _CategoryService.GetCategories().Select(c => c.CategoryId).ToList();
        //                // var CatIds = Categories.Select(c => c.CategoryId);
        //                List<string> Ids = CatIds.ConvertAll<string>(x => x.ToString());
        //                bool isMatched = true;
        //                foreach (var item in categoryIds)
        //                {
        //                    if (!Ids.Contains(item))
        //                    {
        //                        isMatched = false;
        //                    }
        //                }
        //                IEnumerable<string> difference = Ids.Except(categoryIds);
        //                if (isMatched)
        //                {
        //                    Favourite.CategoryIds = FavouriteModel.CategoryIds;
        //                }
        //                else
        //                {
        //                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Category id is wrong."), Configuration.Formatters.JsonFormatter);
        //                }
        //            }
        //                //Favourite.CategoryIds = ((FavouriteModel.CategoryIds != null && FavouriteModel.CategoryIds != "") ? FavouriteModel.CategoryIds : Favourite.CategoryIds);

        //                _FavouriteService.UpdateFavourite(Favourite);
        //                string tags = "";
        //                var FavouriteTag = _FavouriteTagService.GetFavouriteTags().Where(t => t.FavouriteId == Favourite.FavouriteId).ToList();
        //                if (FavouriteModel.Tags != null || FavouriteModel.Tags != "")
        //                {
        //                    string[] tagList = FavouriteModel.Tags.Split(',');


        //                    if (FavouriteTag.Count() > 0)
        //                    {
        //                        foreach (var tag in FavouriteTag)
        //                        {
        //                            _FavouriteTagService.DeleteFavouriteTag(tag);
        //                        }


        //                    }
        //                    foreach (var tag in tagList)
        //                    {

        //                        FavouriteTag FavouriteTag = new FavouriteTag();
        //                        FavouriteTag.FavouriteId = Favourite.FavouriteId;
        //                        FavouriteTag.Tag = tag;
        //                        _FavouriteTagService.InsertFavouriteTag(FavouriteTag);
        //                        tags = tags + "," +tag;
        //                    }


        //                }

        //                Mapper.CreateMap<Favourite, FavouriteResponseModel>();
        //                FavouriteResponseModel model = Mapper.Map<Favourite, FavouriteResponseModel>(Favourite);

        //                model.Tags = tags.TrimStart(',').TrimEnd(',');
        //                model.ContactInfo = Favourite.MobileNo + "|" + Favourite.EmailId;
        //                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", model), Configuration.Formatters.JsonFormatter);
        //            }


        //    }
        //    catch (Exception ex)
        //    {
        //        string ErrorMsg = ex.Message.ToString();
        //        ErrorLogging.LogError(ex);
        //        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Please try again."), Configuration.Formatters.JsonFormatter);
        //    }
        //}


        [Route("DeleteFavorite")]
        [HttpPost]
        public HttpResponseMessage DeleteFavorite([FromBody] deleteFavoriteModel deleteFavoriteModel)
        {
            try
            {
                if (deleteFavoriteModel.CustomerId == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer Id is blank."), Configuration.Formatters.JsonFormatter);
                }
                if (deleteFavoriteModel.FavoriteIds == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Favorite Ids are blank."), Configuration.Formatters.JsonFormatter);
                }
                var models = new List<FavoriteResponseModel>();
                var customer = _CustomerService.GetCustomers().Where(c=>c.CustomerId==deleteFavoriteModel.CustomerId && c.IsActive==true);
                if (customer == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer not found."), Configuration.Formatters.JsonFormatter);
                }


                var favoriteList = _FavoriteService.GetFavorites().Where(f => f.CustomerId == deleteFavoriteModel.CustomerId).ToList();
                if (favoriteList != null)
                {
                    string[] favIds = deleteFavoriteModel.FavoriteIds.Split(',');
                   // List<int> favoriteListIds = favoriteList.Select(f => f.FavoriteId).ToList();
                   // List<int> common = favoriteListIds.Intersect(deleteFavoriteModel.FavoriteIds).ToList();
                   // if (common.Count == deleteFavoriteModel.FavoriteIds.Count())
                   //// if (favoriteListIds.Intersect(deleteFavoriteModel.FavoriteIds).Any())
                   // {
                    foreach (var id in favIds)
                        {
                            var deleteFav = _FavoriteService.GetFavorite(Convert.ToInt32(id));
                            if(deleteFav!=null)
                            {
                                _FavoriteService.DeleteFavorite(deleteFav);
                            }
                            

                        }
                        return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", "Successfully deleted."), Configuration.Formatters.JsonFormatter);
                    //}
                    //else
                    //{
                    //    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Wrong favorite id."), Configuration.Formatters.JsonFormatter);
                    //}

                   

                }


                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Favourite not found."), Configuration.Formatters.JsonFormatter);
                }
            }
            catch (Exception ex)
            {
                string ErrorMsg = ex.Message.ToString();
                ErrorLogging.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "User not found."), Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("GetFavouriteByCustomerId")]
        [HttpGet]
        public HttpResponseMessage GetFavouriteByCustomerId([FromUri] int CustomerId)
        {
            try
            {
                var models = new List<FavoriteResponseModel>();
                var customer = _CustomerService.GetCustomers().Where(c => c.CustomerId == CustomerId && c.IsActive == true);
                if (customer == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Customer not found."), Configuration.Formatters.JsonFormatter);
                }
                var Favourites = _FavoriteService.GetFavorites().Where(l => l.CustomerId == CustomerId).ToList();
                if (Favourites.Count() > 0)
                {
                    foreach (var Favourite in Favourites)
                    {
                        List<string> FavouriteImages = new List<string>();
                        Mapper.CreateMap<Friendlier.Entity.HobbiesAndInterests, Friendlier.Models.FavoriteResponseModel>();
                        FavoriteResponseModel FavouriteResponseModel = Mapper.Map<Friendlier.Entity.HobbiesAndInterests, Friendlier.Models.FavoriteResponseModel>(Favourite);
                        var location = _LocationService.GetLocations().Where(l => l.LocationId == FavouriteResponseModel.LocationId && l.IsApproved==true).FirstOrDefault();
                        
                        if (location==null)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "No location found."), Configuration.Formatters.JsonFormatter);
                        }
                        FavouriteResponseModel.Title = location.Title;
                        FavouriteResponseModel.State = location.State;
                        FavouriteResponseModel.Street = location.Street;
                        FavouriteResponseModel.Country = location.Country;
                        FavouriteResponseModel.City = location.City;
                        FavouriteResponseModel.CategoryIds = location.CategoryIds;
                        FavouriteResponseModel.EmailId = location.EmailId;
                        FavouriteResponseModel.MobileNo = location.MobileNo;
                       // FavouriteResponseModel.ContactInfo = location.MobileNo + "|" + location.EmailId;
                        FavouriteResponseModel.Description = location.Description;
                        FavouriteResponseModel.Ratings = location.Ratings;
                        var images = _LocationImagesService.GetLocationImages().Where(l => l.LocationId == location.LocationId).ToList();
                        List<string> locationImages = new List<string>();
                        if (images.Count() > 0)
                        {

                            foreach (var image in images)
                            {
                                locationImages.Add(image.ImagePath);
                            }
                        }
                        //FavouriteResponseModel.ContactInfo = location.MobileNo + "|" + location.EmailId;
                        FavouriteResponseModel.LocationImages = locationImages;

                        var tags = _LocationTagService.GetLocationTags().Where(t => t.LocationId == location.LocationId).Select(t => t.Tag).ToList();
                        if (tags.Count() > 0)
                        {
                            string tagList = "";
                            foreach (var tag in tags)
                            {
                                tagList = tagList + "," + tag;
                            }
                            FavouriteResponseModel.Tags = tagList.TrimEnd(',').TrimStart(',');
                        }
                        models.Add(FavouriteResponseModel);

                    }
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("success", models), Configuration.Formatters.JsonFormatter);

                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CommonCls.CreateMessage("error", "Favourite not found."), Configuration.Formatters.JsonFormatter);
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

