using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Amazon.PAAPI;
using AmazonWebSearch.Models;

namespace AmazonWebSearch.Controllers
{
    public class PagesController : ApiController
    {
        static readonly AWSECommerceServicePortTypeClient AmazonClient = new AWSECommerceServicePortTypeClient();
        private static readonly List<string> Items = new List<string>();
        private static readonly string AwsAccessKeyId = ConfigurationManager.AppSettings["accessKeyId"];
        private const string AssociateTag = "0696-3573-3781";

        [HttpGet]
        public List<Product> Page(string id)
        {
            return ItemSearchFunc(id);
        }

        private static List<Product> ItemSearchFunc(string key)
        {
            var result = new List<Product>();
            var request = new ItemSearchRequest
            {
                Keywords = key,
                SearchIndex = "All",
                ItemPage = "1"
            };
            var itemSearch = new ItemSearch
            {
                Request = new ItemSearchRequest[] { request },
                AWSAccessKeyId = AwsAccessKeyId,
                AssociateTag = AssociateTag
            };
            var _itemSearchResponse = AmazonClient.ItemSearch(itemSearch);
            if (_itemSearchResponse == null) return null;

            foreach (var item in _itemSearchResponse.Items[0].Item.ToList())
            {
                var itemlookuprequest = new ItemLookupRequest
                {
                    ResponseGroup = new string[] { "Offers" },
                    IdType = ItemLookupRequestIdType.ASIN,
                    ItemId = new string[] { item.ASIN }
                };
                var itemlookup = new ItemLookup
                {
                    Request = new ItemLookupRequest[] { itemlookuprequest },
                    AWSAccessKeyId = AwsAccessKeyId,
                    AssociateTag = AssociateTag
                };

                var _itemlookupresponse = AmazonClient.ItemLookup(itemlookup);
                if (_itemlookupresponse == null)
                    return result;

                var price = _itemlookupresponse.Items[0].Item[0].OfferSummary.LowestUsedPrice != null ?
                    _itemlookupresponse.Items[0].Item[0].OfferSummary.LowestUsedPrice.FormattedPrice : "?";
                var product = new Product()
                {
                    Title = item.ItemAttributes.Title,
                    Price = price
                };
                result.Add(product);
            }
            return result;
        }
    }
}