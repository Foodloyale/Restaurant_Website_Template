﻿using restaurant_demo_website.Models;
using Microsoft.AspNetCore.Mvc;
using restaurant_demo_website.Services;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using restaurant_demo_website.ViewModels;
using Microsoft.Extensions.Caching.Memory;
using FoodloyaleApi.Models;
using System.Globalization;
using System.Collections;

namespace restaurant_demo_website.Controllers
{
    public class MenuController : Controller
    {
        private readonly IEntitiesRequest _entitiesRequest;
        private IMemoryCache _memoryCache;
       

        private IEnumerable<Stocks> stocks { get; set; }
        public List<Product> products {get;set;} = new List<Product>();

        public MenuController(IEntitiesRequest entitiesRequest, IMemoryCache memoryCache)
        {
            _entitiesRequest = entitiesRequest;
            _memoryCache = memoryCache;
            
        }

        
        /// <summary>
        /// Get the list of Produce by the restaurant and all Categories
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> IndexAsync()
        {
            ApplicationUser restaurantinfo = await GetCache();
            bool opened = false;
            ViewData["RestaurantName"] = restaurantinfo.BusinessName;
            
            var pr = await _entitiesRequest.GetProductsAsync();
            stocks = pr.Where(p=>!p.IsExpired);

            var dayopentimes = restaurantinfo.OpeningTimes.FirstOrDefault(o => o.Day == DateTime.Now.DayOfWeek.ToString());
            if (dayopentimes != null)
            {
                opened = AffirmOnlineShopping(dayopentimes.StartTime, dayopentimes.EndTime);
            }
            
           

            List<string> categories = new List<string>();

            if (stocks.Any())
            {
                foreach (var stk in stocks)
                {
                        products.Add(stk.Product);
                        stk.Product.imgUrl = GetImagesFromByteArray(stk.Product.photosUrl);
                        categories.Add(stk.Product.Category);

                }
                categories = categories.Distinct().ToList();
            }

            var ProductCategoryPage = new ProductsCategoryViewModel
            {
                Products = products,
                Categories = categories,
                CurrentCategory = "All",
                ShopOpened = opened
            };

            ViewData["Currency"] = restaurantinfo.Currency;
            return View(ProductCategoryPage);
        }

        private async Task<ApplicationUser> GetCache()
        {
            ApplicationUser restaurantinfo = new ApplicationUser();
            if (ShoppingCart.CartSessionKey != null)
            {
                if (!_memoryCache.TryGetValue(ShoppingCart.CartSessionKey, out ApplicationUser u))
                {
                    restaurantinfo = await _entitiesRequest.GetRestaurantInfo();
                    _memoryCache.Set(ShoppingCart.CartSessionKey, restaurantinfo);
                }
                else
                {
                    restaurantinfo = u;
                }

            }

            return restaurantinfo;
        }

        //images are stored in the database as byte. Convert them to base64 string.
        private string GetImagesFromByteArray(byte[]? photosUrl)
        {
            var dataString = Convert.ToBase64String(photosUrl);
            var imgString = string.Format("data:image/png;base64,{0}", dataString);
            return imgString;
        }


        /// <summary>
        /// Displays the details of a chosen product and also shows recommended products on a side view.
        /// Recommedations are based on machine learning SRA algorithm.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> DetailsAsync(int id)
        {
            var stk = await _entitiesRequest.GetProductsAsync();
            Product product = stk.FirstOrDefault(p => p.ProductID == id).Product;
            product.imgUrl = GetImagesFromByteArray(product.photosUrl);
            if (!string.IsNullOrEmpty(product.Allergens))
            {
                product.DiffAllergen = product.Allergens.Split(",");
            }


            if (product == null)
                return View("Error");
            IEnumerable<Product> r = await GetRecommendations(id);

            var menuDetailView = new MenuDetailsViewModel { ProductInView = product, Recommendations = r.ToList() };
            return View(menuDetailView);
        }

        private bool AffirmOnlineShopping(string starttime, string endtime)
        {
            var nowTime = DateTime.Now.TimeOfDay;
            var start = DateTime.ParseExact(starttime, "hh:mm", CultureInfo.InvariantCulture);
            var end = DateTime.ParseExact(endtime, "HH:mm", CultureInfo.InvariantCulture);

            //if current time is greater than start time, returns 1
            //if current time is less than end time, returns -1
            var r1 = TimeSpan.Compare(nowTime, start.TimeOfDay);
            var r2 = TimeSpan.Compare(nowTime, end.TimeOfDay);
            bool possible = r1 == 1  || r2 == -1 ? false : true;
            return possible;
        }

        private async Task<IEnumerable<Product>> GetRecommendations(int productid)
        {

            var recommendations = await _entitiesRequest.GetCoProductRecommendation(productid);
            foreach (var product in recommendations)
            {
                product.imgUrl = GetImagesFromByteArray(product.photosUrl);
            }
            return recommendations;
        }



        /// <summary>
        /// Gets the list of products for a category
        /// </summary>
        /// <param name="categoryName"></param>
        /// <returns>Products for a Category</returns>
        public async Task<IActionResult> Category(string categoryName)
        {
            IEnumerable<Product> CategoryProducts = null;
            var pr = await _entitiesRequest.GetProductsAsync();
            List<string> categories = new List<string>();
            var CategoryStocks = pr.Where(p => p.Product.Category.Equals(categoryName)&& p.Product.IsDeleted == false).ToList();
            if (CategoryStocks.Any())
            {
                
                foreach (var p in CategoryStocks)
                {
                    p.Product.imgUrl = GetImagesFromByteArray(p.Product.photosUrl);
                    
                    categories.Add(p.Product.Category);
                    CategoryProducts.Append(p.Product);
                }
                categories = categories.Distinct().ToList();
            }

            var ProductCategoryPage = new ProductsCategoryViewModel
            {
                Products = CategoryProducts,
                Categories = categories,
                CurrentCategory = categoryName
            };


            return View(ProductCategoryPage);

        }

        public IActionResult MenuSearch(string menuName)
        {
            var delicacy = GetDelicacy(menuName);
            return Json(delicacy);

        }

        public ActionResult QuickSearch(string term)
        {
            var quickSearch = products.Where(p=>p.Name.Contains(term)).Select(p=> new {value = p.Name});
            return Json(quickSearch);
        }

        public IActionResult Filter(string filter)
        {
            var results = new List<Product>();
            if (filter.Contains("Price"))
            {
                if (filter.Contains("Low - High"))
                {
                    results = products.OrderBy(p => p.Price).ToList();
                }
                else
                    results = products.OrderByDescending(p => p.Price).ToList();
                
            }
            if (filter.Contains("Points"))
            {
                if (filter.Equals("Low - High"))
                {
                    results = products.OrderBy(p => p.LoyaltyPoints).ToList();
                }
                else
                    results = products.OrderByDescending(p => p.LoyaltyPoints).ToList();
            }
            var categories = new List<string>();
            if (results.Any())
            {

                foreach (var p in results)
                {
                    p.imgUrl = GetImagesFromByteArray(p.photosUrl);

                    categories.Add(p.Category);
                }
                categories = categories.Distinct().ToList();
            }

            var ProductCategoryPage = new ProductsCategoryViewModel
            {
                Products = results,
                Categories = categories,
                CurrentCategory = "All"
            };
            return View("Index", ProductCategoryPage);
        }


        private IEnumerable<Product> GetDelicacy(string menuName)
        {
            var ps = products.Where(p => p.Name.Contains(menuName)).ToList();
            foreach (var p in ps)
            {
                p.imgUrl = GetImagesFromByteArray(p.photosUrl);
            }

            return ps;
        }
    }
}
