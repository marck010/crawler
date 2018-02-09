using System;
using HtmlAgilityPack;
using Newtonsoft.Json;
using RestSharp;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Threading;

namespace Crawler {
    class Program {
        static void Main (string[] args) {
            var startDate = DateTime.Now;

            try {
                
                new Scraping().Run();

            } catch (Exception ex) {
                Console.WriteLine (ex.Message);
            }

            var finishDate = DateTime.Now;

            Console.WriteLine ("Time Elapsed: " + (finishDate - startDate));

        }
    }
}