using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using log4net;
using System.Threading.Tasks;

namespace EmailFinderLib
{
    public class HtmlSearch
    {
        static ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static List<string> joinedLinks = new List<string>();
        private static List<string> linkList = new List<string>();
        private static List<Detail> targetDetails = new List<Detail>();
        private static List<EmailList> emailList = new List<EmailList>();
        const string MatchEmailPattern = @"[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,6}";
        private static string keyword;

        public static void GetEmails(List<Detail> targets, string searchKeyword)
        {
            targetDetails = targets;
            keyword = searchKeyword;
            getEmails();
        }
        public static List<Detail> GetEmails(string lat, string lng, string radius, string searchKeyword, string apiKey)
        {
            Api.SetApiKey(apiKey);
            targetDetails = getLinksFromLocs(lat, lng, radius, searchKeyword);
            keyword = searchKeyword;
            if (targetDetails.Count > 0)
            {
                Task.Factory.StartNew(() =>
                {
                    getEmails();
                });
            }
            return targetDetails;
        }
        static void getEmails()
        {
            for (int j = 0; j < targetDetails.Count; j++)
            {
                string htmlString = getWebSiteString(targetDetails[j].WebSite);
                List<string> subLinks = getLinks(htmlString, targetDetails[j].WebSite, new List<string>());
                getEmail(htmlString, targetDetails[j]);
                Console.WriteLine("{0} scanned.Time:{1}", targetDetails[j].WebSite, DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                for (int i = 0; i < subLinks.Count; i++)
                {
                    htmlString = getWebSiteString(subLinks[i]);
                    getEmail(htmlString, targetDetails[j]);
                    subLinks.AddRange(getLinks(htmlString, targetDetails[j].WebSite, subLinks));
                    Console.WriteLine("{0} scanned.Time:{1}", subLinks[i], DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                }
            }
            for (int j = 0; j < linkList.Count; j++)
            {
                string htmlString = getWebSiteString(linkList[j]);
                List<string> subLinks = getLinks(htmlString, linkList[j], new List<string>());
                getEmail(htmlString, linkList[j]);
                Console.WriteLine("{0} scanned.Time:{1}", linkList[j], DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                for (int i = 0; i < subLinks.Count; i++)
                {
                    htmlString = getWebSiteString(subLinks[i]);
                    getEmail(htmlString, linkList[j]);
                    subLinks.AddRange(getLinks(htmlString, linkList[j], subLinks));
                    Console.WriteLine("{0} scanned.Time:{1}", subLinks[i], DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                }
            }
        }
        static List<Detail> getLinksFromLocs(string lat, string lng, string radius, string searchKeyword)
        {
            var res = Api.GetPlaces(lat, lng, radius, searchKeyword);
            res.Wait();
            if (res.Result != null)
            {
                List<Detail> links = new List<Detail>();
                res.Result.Places.ForEach(f =>
                {
                    var det = f.GetDetails();
                    det.Wait();
                    if (det.Result != null && !string.IsNullOrEmpty(det.Result.WebSite))
                    {
                        links.Add(det.Result);
                    }
                });
                return links;
            }
            else
            {
                return new List<Detail>();
            }
        }
        static string getWebSiteString(string link)
        {
            try
            {
                WebClient client = new WebClient();
                joinedLinks.Add(link);
                string downloadString = client.DownloadString(link);
                return downloadString;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }
        static List<string> getLinks(string htmlString, string domain, List<string> targetLinks)
        {
            if (!string.IsNullOrEmpty(htmlString))
            {
                List<string> returnLinks = new List<string>();
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(htmlString);
                HtmlNodeCollection linkNodes = doc.DocumentNode.SelectNodes("//a/@href");
                if (linkNodes != null)
                {
                    foreach (HtmlNode linkNode in linkNodes)
                    {
                        HtmlAttribute attrib = linkNode.Attributes["href"];
                        if (!string.IsNullOrEmpty(attrib.Value) && !attrib.Value.ToLower().Contains("facebook.") && !attrib.Value.ToLower().Contains("twitter.") && !attrib.Value.ToLower().Contains("google.") && !attrib.Value.ToLower().Contains("linkedin.") && !attrib.Value.ToLower().Contains("youtube.") && !attrib.Value.ToLower().Contains("instagram.") && !attrib.Value.ToLower().Contains("tripadvisor.") && !attrib.Value.ToLower().Contains("foursquare.") && !attrib.Value.ToLower().Contains(".jpg") && !attrib.Value.ToLower().Contains(".jpeg") && !attrib.Value.ToLower().Contains(".png") && !attrib.Value.ToLower().Contains(".gif") && !attrib.Value.ToLower().Contains("@") && !attrib.Value.ToLower().Contains("javascript") && !returnLinks.Contains(attrib.Value.ToLower().Trim()) && !joinedLinks.Contains(attrib.Value.ToLower().Trim()) && !targetLinks.Contains(attrib.Value.ToLower().Trim()) && !linkList.Contains(attrib.Value.ToLower().Trim()) && !targetDetails.Exists(s => s.WebSite == attrib.Value.ToLower().Trim()))
                        {
                            if (Uri.IsWellFormedUriString(attrib.Value.Trim(), UriKind.Absolute))
                            {
                                if (attrib.Value.Contains(keyword) || attrib.Value.Contains(domain))
                                {
                                    linkList.Add(attrib.Value.Trim());
                                }
                            }
                            else
                            {
                                string link = ((domain.EndsWith("/") || attrib.Value.Trim().StartsWith("/")) ? string.Format("{0}{1}", domain, attrib.Value.Trim()) : string.Format("{0}/{1}", domain, attrib.Value.Trim()));
                                if (!returnLinks.Contains(link) && !joinedLinks.Contains(link) && !targetLinks.Contains(link))
                                {
                                    returnLinks.Add(link);
                                }
                            }
                        }
                    }
                }
                return returnLinks;
            }
            return new List<string>();
        }
        static void getEmail(string htmlString, Detail detail)
        {
            if (!string.IsNullOrEmpty(htmlString))
            {
                Regex rx = new Regex(MatchEmailPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                MatchCollection matches = rx.Matches(htmlString);
                foreach (Match match in matches)
                {
                    if (!emailList.Exists(s => s.Email == match.Value.ToString().Trim()))
                    {
                        var email = new EmailList { Email = match.Value.ToString().Trim(), Domain = detail.WebSite, Name = detail.Name, Geo = detail.Geo };
                        ThreadContext.Properties["name"] = email.Name;
                        ThreadContext.Properties["website"] = email.Domain;
                        ThreadContext.Properties["email"] = email.Email;
                        ThreadContext.Properties["lat"] = email.Geo != null ? email.Geo.Location.Latitude.ToString() : string.Empty;
                        ThreadContext.Properties["lng"] = email.Geo != null ? email.Geo.Location.Longitude.ToString() : string.Empty;
                        logger.Info("");
                        emailList.Add(email);
                    }
                }
            }
        }
        static void getEmail(string htmlString, string domain)
        {
            if (!string.IsNullOrEmpty(htmlString))
            {
                Regex rx = new Regex(MatchEmailPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                MatchCollection matches = rx.Matches(htmlString);
                foreach (Match match in matches)
                {
                    if (!emailList.Exists(s => s.Email == match.Value.ToString().Trim()))
                    {
                        var email = new EmailList { Email = match.Value.ToString().Trim(), Domain = domain };
                        ThreadContext.Properties["name"] = string.Empty;
                        ThreadContext.Properties["website"] = email.Domain;
                        ThreadContext.Properties["email"] = email.Email;
                        ThreadContext.Properties["lat"] = string.Empty;
                        ThreadContext.Properties["lng"] = string.Empty;
                        logger.Info("");
                        emailList.Add(email);
                    }
                }
            }
        }
    }
}