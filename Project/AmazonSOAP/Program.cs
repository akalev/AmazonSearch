using System;
using System.ComponentModel;
using System.Configuration;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Text;



namespace Amazon.PAAPI
{
    class Program
    {
        static readonly AWSECommerceServicePortTypeClient AmazonClient = new AWSECommerceServicePortTypeClient();
        private static readonly List<string> Items = new List<string>();
        private const int PerPage = 13;
        private static int _itemPage = 1;
        private static ItemSearchResponse _itemSearchResponse;
        private static ItemLookupResponse _itemlookupresponse;
        private static readonly string AwsAccessKeyId = ConfigurationManager.AppSettings["accessKeyId"];
        private const string AssociateTag = "0696-3573-3781";

        static void Main(string[] args)
        {
            ItemSearchFunc();
        }

        private static void ItemSearchFunc()
        {
            var request = new ItemSearchRequest
            {
                Keywords = "tv",
                SearchIndex = "All",
                ItemPage = _itemPage.ToString(CultureInfo.InvariantCulture)
            };
            var itemSearch = new ItemSearch
            {
                Request = new ItemSearchRequest[] {request},
                AWSAccessKeyId = AwsAccessKeyId,
                AssociateTag = AssociateTag
            };
            if (_itemSearchResponse == null)
            {
                try
                {
                    _itemSearchResponse = AmazonClient.ItemSearch(itemSearch);
                    Console.WriteLine("TotalResults: " + _itemSearchResponse.Items[0].TotalResults);
                    Console.WriteLine("TotalPages: " + _itemSearchResponse.Items[0].TotalPages);
                }
                catch (ServerTooBusyException)
                {
                    ItemSearchFunc();
                }
            }
            if (_itemSearchResponse == null) return;
            ItemLookupFunc(_itemSearchResponse.Items[0].Item.ToList());
        }

        private static void ItemLookupFunc(IEnumerable<Item> items)
        {
            foreach (var item in items)
            {
                var itemlookuprequest = new ItemLookupRequest
                {
                    ResponseGroup = new string[] { "Offers" },
                    IdType = ItemLookupRequestIdType.ASIN,
                    ItemId = new string[] { item.ASIN }
                };
                var itemlookup = new ItemLookup
                {
                    Request = new ItemLookupRequest[] {itemlookuprequest},
                    AWSAccessKeyId = AwsAccessKeyId,
                    AssociateTag = AssociateTag
                };
                if (_itemlookupresponse == null)
                {
                    try
                    {
                        _itemlookupresponse = AmazonClient.ItemLookup(itemlookup);
                    }
                    catch (ServerTooBusyException)
                    {
                        Items.Clear();
                        ItemSearchFunc();
                        break;
                    }
                }
                Items.Add(item.ItemAttributes.Title + " " + _itemlookupresponse.Items[0].Item[0].OfferSummary.LowestUsedPrice.FormattedPrice);
                if (Items.Count != PerPage) continue;
                Display(Items);
                break;
            }
            if (Items.Count >= PerPage) return;
            _itemPage++;
            ItemSearchFunc();
        }

        private static void Display(IEnumerable<string> items)
        {
            foreach (var item in items)
            {
                Console.WriteLine(item);
            }
        }
    }
}
