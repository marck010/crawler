using System;
using HtmlAgilityPack;
using Newtonsoft.Json;
using RestSharp;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;

namespace Crawler {
    public class Scraping {
        public void Run () {

            var host = "https://www.eslfast.com/robot/";

            var doc = HtmlDocument (host);
            var elements = FindNodes (doc, "//blockquote//a");
            var threads = new List<Thread> ();
            foreach (var element in elements) {

                Thread workerThread = new Thread (() => Execute (element, host));

                workerThread.Start ();
                Console.Write ("\r\nThread started for " + element.InnerText);
                
                Thread.Sleep(5000);
                
                threads.Add (workerThread);

            }

            int countThreads = threads.Count (x => x.IsAlive);
            Console.WriteLine ("\r\nTotal Run Threads: " + countThreads);

            while (threads.Any (x => x.IsAlive)) {
                var currentCountThreads = threads.Count (x => x.IsAlive);

                if (currentCountThreads != countThreads) {
                    Console.WriteLine ("\r\nRun Threads: " + currentCountThreads);
                }

                countThreads = currentCountThreads;

            }

        }

        private HtmlDocument HtmlDocument (string url) {

            var response = Request.Get (url);
            var html = response.Content;

            var doc = new HtmlAgilityPack.HtmlDocument ();
            doc.LoadHtml (html);
            return doc;
        }

        private List<HtmlAttribute> FindAttributesValues (HtmlDocument doc, string selector, string attrName) {
            var nodes = doc.DocumentNode.SelectNodes (selector);

            if (nodes == null) {
                throw new ArgumentException ("Nodes not found for selector " + selector);
            }

            return nodes.SelectMany (element => element.Attributes.Where (attr => attr.Name == attrName)).ToList ();
        }

        private List<HtmlNode> FindNodes (HtmlDocument doc, string selector) {

            var nodes = doc.DocumentNode.SelectNodes (selector);

            if (nodes == null) {
                throw new ArgumentException ("Nodes not found for selector " + selector);
            }

            return nodes.ToList ();
        }

        private void Execute (HtmlNode element, string host) {
            var title = "";

            try {
                var topic = element.Attributes.FirstOrDefault (attr => attr.Name == "href");

                if (topic == null) {
                    throw new ArgumentException ("Attribute not found in " + element.InnerText);
                }

                var topicValue = topic?.Value.Replace ("../", "");
                var urlTopic = host + topicValue;
                var docTopic = HtmlDocument (urlTopic);

                title = element.InnerText;

                var topics2 = FindAttributesValues (docTopic, "//blockquote//a", "href");

                var directoryAudiosRoot = "Storage/" + title;

                foreach (var topic2 in topics2) {
                    var titleTopic2 = "";
                    try {
                        var fileName = topic.Value.Split ("/") [2];
                        urlTopic = urlTopic.Replace (fileName, "");
                        var urTopic2 = urlTopic + topic2.Value.Replace ("../", "");

                        var docTopic2 = HtmlDocument (urTopic2);
                        var nodeTopic2 = FindNodes (docTopic2, "//center//b").FirstOrDefault ();
                        titleTopic2 = nodeTopic2 != null ? nodeTopic2.InnerText : topic2.Value.Replace ("../", "");

                        var audios = FindAttributesValues (docTopic2, "//audio", "src");
                        var directoryAudios = directoryAudiosRoot + "/" + titleTopic2;
                        Directory.CreateDirectory (directoryAudios);

                        foreach (var audio in audios) {
                            var urlAudio = "";
                            try {
                                urlAudio = audio.Value.Replace ("../", "");
                                var responseAudio = Request.Get (host + urlAudio);

                                var fileNameAudios = urlAudio.Split ("/") [2];
                                var directoryFile = directoryAudios + "/" + fileNameAudios;
                                if (responseAudio.RawBytes != null) {
                                    File.WriteAllBytes (directoryFile, responseAudio.RawBytes);
                                }
                            } catch (Exception ex) {

                                Console.WriteLine ("Error in " + urlAudio + " \r\n" + ex.Message);
                            }

                        }
                    } catch (Exception ex) {

                        Console.WriteLine ("Error in " + titleTopic2 + " \r\n" + ex.Message);
                    }

                }

            } catch (Exception ex) {

                Console.WriteLine ("Error in " + title + " \r\n" + ex.Message);

            }
        }
    }
}