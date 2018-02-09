using System;
using HtmlAgilityPack;
using Newtonsoft.Json;
using RestSharp;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Net;

namespace Crawler {

    public class Request {

        public static IRestResponse Get (string url) {

            var client = new RestClient (url);
            var request = new RestRequest (Method.GET);
            IRestResponse response = client.Execute (request);
            if(response.ResponseStatus == ResponseStatus.Error){
                throw new Exception("Request error in :"+ url + "\r\nStatus Code:" + response.StatusCode+"\r\n"+ response.Content);
            }
            return response;

        }
        
    }
}