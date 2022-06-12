using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Speech.Synthesis;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AMNHAC.Models;
using Aspose.Html.DataScraping.MultimediaScraping;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Http;
using Google.Apis.Services;

using Google.Apis.YouTube.v3;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;


//

namespace AMNHAC.Controllers
{
    ///////////////////////////////////////////////////////

    public class SearchYouTube
    {


        public int ID { get; set; }

        public async Task<List<Video>> RunYouTube(string timkiem)
        {
            List<Video> vk = new List<Video>();

            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = "AIzaSyBHCjF4D7gZXz7vjh6XMKJpfNLmSGgZvV8",
                ApplicationName = this.GetType().ToString()

            });

            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.Q = timkiem; // Replace with your search term.
            searchListRequest.MaxResults = 6;


            // Call the search.list method to retrieve results matching the specified query term.
            var searchListResponse = await searchListRequest.ExecuteAsync();


            List<string> videos = new List<string>();
            List<string> channels = new List<string>();
            List<string> playlists = new List<string>();



            // Add each result to the appropriate list, and then display the lists of
            // matching videos, channels, and playlists.


            foreach (var searchResult in searchListResponse.Items)
            {
                switch (searchResult.Id.Kind)
                {
                    case "youtube#video":
                        var VideoListRequest = youtubeService.Videos.List("snippet, contentDetails, statistics");
                        VideoListRequest.Id = searchResult.Id.VideoId;
                        var VideoListResponse = await VideoListRequest.ExecuteAsync();


                        videos.Add(String.Format("{0} ({1})", searchResult.Snippet.Title, searchResult.Id.VideoId));
                        var vs = new Video();
                        {
                            vs.title = searchResult.Snippet.Title;
                            vs.id = searchResult.Id.VideoId;
                            vs.HinhNguonVideo = searchResult.Id.VideoId;
                            vs.link = searchResult.Snippet.Description;
                            vs.author = searchResult.Snippet.ChannelTitle;
                            foreach (var video in VideoListResponse.Items)
                            {
                                vs.duration = video.ContentDetails.Duration;

                            }
                        }

                        vk.Add(vs);

                        break;

                    case "youtube#channel":
                        channels.Add(String.Format("{0} ({1})", searchResult.Snippet.Title, searchResult.Id.ChannelId));
                        break;

                    case "youtube#playlist":
                        playlists.Add(String.Format("{0} ({1})", searchResult.Snippet.Title, searchResult.Id.PlaylistId));
                        break;
                }
            }


            Console.WriteLine(String.Format("Videos:\n{0}\n", string.Join("\n", videos)));
            Console.WriteLine(String.Format("Channels:\n{0}\n", string.Join("\n", channels)));
            Console.WriteLine(String.Format("Playlists:\n{0}\n", string.Join("\n", playlists)));

            return vk;
        }
        /////////////////


    }


    public class HomeController : Controller
    {
        DataClasses1DataContext data = new DataClasses1DataContext();
        SearchYouTube searchObject = new SearchYouTube();
        List<Video> test = new List<Video>();


        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        private List<Person> GetPerson()
        {
            List<Person> person = new List<Person>();
            person = data.Persons.ToList();
            return person;
        }

        //
        [HttpGet]

        public ActionResult Index()
        {

            dynamic mymodel = new ExpandoObject();
            mymodel.personTQ = from ss in data.Persons where ss.idTheloai == 1 select ss;
            mymodel.personVN = from ss in data.Persons where ss.idTheloai == 2 select ss;

            mymodel.videoTQ = from ss in data.Videos where ss.loaivideo != "user" && ss.idTheloai == 1 select ss;

            mymodel.videoVN = from ss in data.Videos where ss.loaivideo != "user" && ss.idTheloai == 2 select ss;
            return View(mymodel);
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View("~/Views/Home/Index.cshtml");
        }


        [HttpPost]
        public async Task<ActionResult> Create(FormCollection form)
        {
            var vk = new Video();
            vk.title = form["Id"];

            //Youtube API
            test = await searchObject.RunYouTube(vk.title);

            if (vk.title != "")
            {
                if (test.Count == 0)
                {
                    ViewBag.Message = "Can't find!!!!!";
                    return View(test);
                }
                else
                {
                    ViewBag.Message = "Your Search Results!!";
                    return View(test);
                }

            }
            else
            {
                dynamic mymodel = new ExpandoObject();
                mymodel.person = GetPerson();


                mymodel.videoTQ = from ss in data.Videos where ss.loaivideo != "user" && ss.idTheloai == 1 select ss;

                mymodel.videoVN = from ss in data.Videos where ss.loaivideo != "user" && ss.idTheloai == 2 select ss;
                return View("~/Views/Home/Index.cshtml", mymodel);
            }

        }
        [HttpPost]
        public async Task<ActionResult> CreateAdmin(FormCollection form)
        {
            var vk = new Video();
            vk.title = form["Id"];
            //Youtube API
            test = await searchObject.RunYouTube(vk.title);

            if (vk.title != "")
            {
                if (test.Count == 0)
                {
                    ViewBag.Message = "Can't find!!!!!";
                    return View(test);
                }
                else
                {
                    ViewBag.Message = "Your Search Results!!";
                    return View(test);
                }

            }
            else
            {

                return View("~/Views/Home/TrangChu.cshtml");
            }

        }
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        /*[HttpPost]
        public async Task<ActionResult> Test(FormCollection form)
        {

            List<Video> vs = new List<Video>();
            var vk = new Video();
            //Get tên bài hát muốn thêm vào
            vk.title = form["Id"];
            //Get UserName
            string getUser = form["GetUserName"];
            var getDataUser = data.AspNetUsers.ToList();


            //Get id bài hát kiểm tra có bị trùng trong data ko
            vk.id = form["CheckId"];
            var checkId = data.Videos.Where(m => m.id == vk.id).FirstOrDefault();


            //Youtube API
            test = await searchObject.RunYouTube(vk.title);
            vs = new List<Video>(test);

            for (var item = 0; item < test.Count; item++)
            {
                vs[item].title = test[item].title;
                vs[item].HinhNguonVideo = test[item].id;
                vs[item].author = test[item].author;
                vs[item].link = test[item].link;


                vs[item].loaivideo = "user";
                vs[item].id = test[item].id + "/user/" + getUser;



                vs[item].duration = test[item].duration;

                for (var itemuser = 0; itemuser < getDataUser.Count; itemuser++)
                {
                    if (getDataUser[itemuser].Email == getUser)
                    {
                        vs[item].UserId = getDataUser[itemuser].Id;
                    }
                }


                if (checkId == default)
                {
                    if (vk.id == vs[item].id)
                    {
                        data.Videos.InsertOnSubmit(vs[item]);
                    }
                }
                else
                {
                    ViewBag.Check = "This Have In Your PlayList !!";
                    return View("~/Views/Home/Create.cshtml", test);
                }
            }
            data.SubmitChanges();
            //Sổ danh sách 
            var userId = User.Identity.GetUserId();
            var all_list = data.Videos.ToList();
            var all_check = from ss in data.Videos where ss.loaivideo == "user" && ss.UserId == userId select ss;
            if (all_list.Count == 0)
            {
                ViewBag.Message = "You Not Have Anything In Playlist";
                return View(all_check);
            }
            else
            {
                ViewBag.Message = "Your Playlist";
                return View("~/Views/MyMusicProfile/Index.cshtml", all_check);
            }
        }*/

        [HttpGet]
        public async Task<ActionResult> DetelePlaylist()
        {
            var userId = User.Identity.GetUserId();
            var currentClaims = await UserManager.GetClaimsAsync(HttpContext.User.Identity.GetUserId());

            var accesstoken = currentClaims.FirstOrDefault(x => x.Type == "urn:tokens:facebook");  //currentClaims.FirstOrDefault(x => x.Type == "urn:tokens:facebook")

            if (accesstoken == null)
            {

                var checkuser = from ss in data.AspNetUsers where ss.Id == userId select ss;

                var videoProfile = data.Videos.FirstOrDefault(m => m.UserId == userId);
                var check = from ss in data.Videos where ss.loaivideo == "user" && ss.UserId == userId select ss;
                dynamic mymodel = new ExpandoObject();
                mymodel.user = checkuser;
                mymodel.video = check;
                if (videoProfile == null)
                {
                    ViewBag.Message = "You Not Have Anything In Playlist";
                    return View("~/Views/MyMusicProfile/Index.cshtml", mymodel);
                }
                else
                {
                    ViewBag.Message = "Your Playlist";
                    return View("~/Views/MyMusicProfile/Index.cshtml", mymodel);
                }
                 /*(new HttpStatusCodeResult(HttpStatusCode.NotFound, "Token not found"))*/;
            }
            string url = String.Format("https://graph.facebook.com/v14.0/me?fields=id,name,picture,email&access_token={0}", accesstoken.Value);
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;

            request.Method = "GET";

            using (HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse)
            {
                StreamReader reader = new StreamReader(response.GetResponseStream());

                string result = await reader.ReadToEndAsync();

                dynamic jsonObj = System.Web.Helpers.Json.Decode(result);
                Models.Facebook facebook = new Models.Facebook(jsonObj);
                ViewBag.JSON = result;



                var getUserdata = data.AspNetUsers.ToList();
                for (var item = 0; item < getUserdata.Count; item++)
                {
                    if (getUserdata[item].Id == userId)
                    {
                        getUserdata[item].UserName = facebook.name;
                        getUserdata[item].Name = facebook.picture.data.url;
                    }
                }

                ///

                var checkuser = from ss in data.AspNetUsers where ss.Id == userId select ss;

                var videoProfile = data.Videos.FirstOrDefault(m => m.UserId == userId);
                var check = from ss in data.Videos where ss.loaivideo == "user" && ss.UserId == userId select ss;
                dynamic mymodel = new ExpandoObject();
                mymodel.user = checkuser;
                mymodel.video = check;
                if (videoProfile == null)
                {
                    ViewBag.Message = "You Not Have Anything In Playlist";
                    return View("~/Views/MyMusicProfile/Index.cshtml", mymodel);
                }
                else
                {
                    ViewBag.Message = "Your Playlist";
                    return View("~/Views/MyMusicProfile/Index.cshtml", mymodel);
                }

            }
        }
        [HttpPost]
        public async Task<ActionResult> DetelePlaylist(string id, FormCollection collection)
        {
            var userId = User.Identity.GetUserId();
            var D_playlist = data.Videos.Where(m => m.id == id).First();
            data.Videos.DeleteOnSubmit(D_playlist);
            data.SubmitChanges();



            var currentClaims = await UserManager.GetClaimsAsync(HttpContext.User.Identity.GetUserId());

            var accesstoken = currentClaims.FirstOrDefault(x => x.Type == "urn:tokens:facebook");  //currentClaims.FirstOrDefault(x => x.Type == "urn:tokens:facebook")

            if (accesstoken == null)
            {

                var checkuser = from ss in data.AspNetUsers where ss.Id == userId select ss;

                var videoProfile = data.Videos.FirstOrDefault(m => m.UserId == userId);
                var check = from ss in data.Videos where ss.loaivideo == "user" && ss.UserId == userId select ss;
                dynamic mymodel = new ExpandoObject();
                mymodel.user = checkuser;
                mymodel.video = check;
                if (videoProfile == null)
                {
                    ViewBag.Message = "You Not Have Anything In Playlist";
                    return View("~/Views/MyMusicProfile/Index.cshtml", mymodel);
                }
                else
                {
                    ViewBag.Message = "Your Playlist";
                    return View("~/Views/MyMusicProfile/Index.cshtml", mymodel);
                }
                 /*(new HttpStatusCodeResult(HttpStatusCode.NotFound, "Token not found"))*/;
            }
            string url = String.Format("https://graph.facebook.com/v14.0/me?fields=id,name,picture,email&access_token={0}", accesstoken.Value);
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;

            request.Method = "GET";

            using (HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse)
            {
                StreamReader reader = new StreamReader(response.GetResponseStream());

                string result = await reader.ReadToEndAsync();

                dynamic jsonObj = System.Web.Helpers.Json.Decode(result);
                Models.Facebook facebook = new Models.Facebook(jsonObj);
                ViewBag.JSON = result;



                var getUserdata = data.AspNetUsers.ToList();
                for (var item = 0; item < getUserdata.Count; item++)
                {
                    if (getUserdata[item].Id == userId)
                    {
                        getUserdata[item].UserName = facebook.name;
                        getUserdata[item].Name = facebook.picture.data.url;
                    }
                }

                ///

                var checkuser = from ss in data.AspNetUsers where ss.Id == userId select ss;

                var videoProfile = data.Videos.FirstOrDefault(m => m.UserId == userId);
                var check = from ss in data.Videos where ss.loaivideo == "user" && ss.UserId == userId select ss;
                dynamic mymodel = new ExpandoObject();
                mymodel.user = checkuser;
                mymodel.video = check;
                if (videoProfile == null)
                {
                    ViewBag.Message = "You Not Have Anything In Playlist";
                    return View("~/Views/MyMusicProfile/Index.cshtml", mymodel);
                }
                else
                {
                    ViewBag.Message = "Your Playlist";
                    return View("~/Views/MyMusicProfile/Index.cshtml", mymodel);
                }

            }

        }
        //

        //
        [HttpPost]
        public async Task<ActionResult> AddPlaylist(FormCollection form)
        {
            List<Video> vs = new List<Video>();
            var vk = new Video();
            //Get tên bài hát muốn thêm vào
            vk.title = form["Id"];
            //Get UserName
            string getUser = form["GetUserName"];
            var getDataUser = data.AspNetUsers.ToList();
            //Get id bài hát kiểm tra có bị trùng trong data ko
            vk.id = form["CheckId"];
            var checkId = data.Videos.Where(m => m.id == vk.id).FirstOrDefault();

            test = await searchObject.RunYouTube(vk.title);
            vs = new List<Video>(test);


            for (var item = 0; item < test.Count; item++)
            {
                vs[item].title = test[item].title;
                vs[item].HinhNguonVideo = test[item].id;
                vs[item].author = test[item].author;
                vs[item].link = test[item].link;


                vs[item].loaivideo = "user";
                vs[item].id = test[item].id + "/user/" + getUser;



                vs[item].duration = test[item].duration;

                for (var itemuser = 0; itemuser < getDataUser.Count; itemuser++)
                {
                    if (getDataUser[itemuser].Email == getUser)
                    {
                        vs[item].UserId = getDataUser[itemuser].Id;
                    }
                }


                if (checkId == default)
                {
                    if (vk.id == vs[item].id)
                    {
                        data.Videos.InsertOnSubmit(vs[item]);
                    }
                }
                else
                {
                    var a = await UserManager.GetClaimsAsync(HttpContext.User.Identity.GetUserId());

                    var b = a.FirstOrDefault(x => x.Type == "urn:tokens:facebook");  //currentClaims.FirstOrDefault(x => x.Type == "urn:tokens:facebook")

                    if (b == null)
                    {
                        var userId = User.Identity.GetUserId();
                        var checkuser = from ss in data.AspNetUsers where ss.Id == userId select ss;


                        var check = from ss in data.Videos where ss.loaivideo == "user" && ss.UserId == userId select ss;
                        dynamic mymodel = new ExpandoObject();
                        mymodel.user = checkuser;
                        mymodel.video = check;

                        ViewBag.Message = "This Have In Your Playlist";
                        return View("~/Views/MyMusicProfile/Index.cshtml", mymodel);

                    }
                    string c = String.Format("https://graph.facebook.com/v14.0/me?fields=id,name,picture,email&access_token={0}", b.Value);
                    HttpWebRequest d = WebRequest.Create(c) as HttpWebRequest;

                    d.Method = "GET";

                    using (HttpWebResponse response = await d.GetResponseAsync() as HttpWebResponse)
                    {
                        StreamReader reader = new StreamReader(response.GetResponseStream());

                        string result = await reader.ReadToEndAsync();

                        dynamic jsonObj = System.Web.Helpers.Json.Decode(result);
                        Models.Facebook facebook = new Models.Facebook(jsonObj);
                        ViewBag.JSON = result;

                        var userId = User.Identity.GetUserId();

                        var getUserdata = data.AspNetUsers.ToList();
                        for (var items = 0; items < getUserdata.Count; items++)
                        {
                            if (getUserdata[items].Id == userId)
                            {
                                getUserdata[items].UserName = facebook.name;
                                getUserdata[items].Name = facebook.picture.data.url;
                            }
                        }

                        ///

                        var checkuser = from ss in data.AspNetUsers where ss.Id == userId select ss;


                        var check = from ss in data.Videos where ss.loaivideo == "user" && ss.UserId == userId select ss;
                        dynamic mymodel = new ExpandoObject();
                        mymodel.user = checkuser;
                        mymodel.video = check;

                        ViewBag.Message = "This Have In Your Playlist!!";
                        return View("~/Views/MyMusicProfile/Index.cshtml", mymodel);


                    }
                }
            }
            data.SubmitChanges();
            //Sổ danh sách 
            /*var userId = User.Identity.GetUserId();

            var all_check = from ss in data.Videos where ss.loaivideo == "user" && ss.UserId == userId select ss;
            ViewBag.Message = "Your Playlist";
            return View("~/Views/MyMusicProfile/Index.cshtml", all_check);*/
            var currentClaims = await UserManager.GetClaimsAsync(HttpContext.User.Identity.GetUserId());

            var accesstoken = currentClaims.FirstOrDefault(x => x.Type == "urn:tokens:facebook");  //currentClaims.FirstOrDefault(x => x.Type == "urn:tokens:facebook")

            if (accesstoken == null)
            {
                var userId = User.Identity.GetUserId();
                var checkuser = from ss in data.AspNetUsers where ss.Id == userId select ss;

                var videoProfile = data.Videos.FirstOrDefault(m => m.UserId == userId);
                var check = from ss in data.Videos where ss.loaivideo == "user" && ss.UserId == userId select ss;
                dynamic mymodel = new ExpandoObject();
                mymodel.user = checkuser;
                mymodel.video = check;
                if (videoProfile == null)
                {
                    ViewBag.Message = "You Not Have Anything In Playlist";
                    return View("~/Views/MyMusicProfile/Index.cshtml", mymodel);
                }
                else
                {
                    ViewBag.Message = "Your Playlist";
                    return View("~/Views/MyMusicProfile/Index.cshtml", mymodel);
                }
                 /*(new HttpStatusCodeResult(HttpStatusCode.NotFound, "Token not found"))*/;
            }
            string url = String.Format("https://graph.facebook.com/v14.0/me?fields=id,name,picture,email&access_token={0}", accesstoken.Value);
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;

            request.Method = "GET";

            using (HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse)
            {
                StreamReader reader = new StreamReader(response.GetResponseStream());

                string result = await reader.ReadToEndAsync();

                dynamic jsonObj = System.Web.Helpers.Json.Decode(result);
                Models.Facebook facebook = new Models.Facebook(jsonObj);
                ViewBag.JSON = result;

                var userId = User.Identity.GetUserId();

                var getUserdata = data.AspNetUsers.ToList();
                for (var item = 0; item < getUserdata.Count; item++)
                {
                    if (getUserdata[item].Id == userId)
                    {
                        getUserdata[item].UserName = facebook.name;
                        getUserdata[item].Name = facebook.picture.data.url;
                    }
                }

                ///

                var checkuser = from ss in data.AspNetUsers where ss.Id == userId select ss;

                var videoProfile = data.Videos.FirstOrDefault(m => m.UserId == userId);
                var check = from ss in data.Videos where ss.loaivideo == "user" && ss.UserId == userId select ss;
                dynamic mymodel = new ExpandoObject();
                mymodel.user = checkuser;
                mymodel.video = check;
                if (videoProfile == null)
                {
                    ViewBag.Message = "You Not Have Anything In Playlist";
                    return View("~/Views/MyMusicProfile/Index.cshtml", mymodel);
                }
                else
                {
                    ViewBag.Message = "Your Playlist";
                    return View("~/Views/MyMusicProfile/Index.cshtml", mymodel);
                }

            }

        }


        public bool checkUserorAdmin()
        {
            var userId = User.Identity.GetUserId();
            var getDataUser = data.AspNetUserLogins.ToList();
            for (var item = 0; item < getDataUser.Count; item++)
            {
                if (getDataUser[item].UserId == userId)
                {
                    if (getDataUser[item].LoginProvider == "Facebook")
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// 
        [Authorize(Users = "xincaiten2001@gmail.com")]
        public ActionResult TrangAdmin()
        {

            return View();
        }

        [Authorize(Users = "xincaiten2001@gmail.com")]
        public ActionResult TrangChu()
        {

            ViewBag.Message = "You Can Search AnyThing Add Your Playlist And Controll Trang Chủ!!";
            /*var trangchu = from ss in data.Videos where ss.loaivideo != "user" select ss;*/
            dynamic mymodel = new ExpandoObject();
            mymodel.person = GetPerson();
            mymodel.personTQ = from ss in data.Persons where ss.idTheloai == 1 select ss;
            mymodel.personVN = from ss in data.Persons where ss.idTheloai == 2 select ss;
            mymodel.trangchu = from ss in data.Videos where ss.loaivideo != "user" select ss;
            mymodel.videoTQ = from ss in data.Videos where ss.loaivideo != "user" && ss.idTheloai == 1 select ss;

            mymodel.videoVN = from ss in data.Videos where ss.loaivideo != "user" && ss.idTheloai == 2 select ss;
            return View(mymodel);
            /*return View(trangchu);*/

        }


        [HttpPost]
        public async Task<ActionResult> AddPlaylistAdmin(FormCollection form)
        {
            int a = 0;
            List<Video> vs = new List<Video>();
            var vk = new Video();
            //Get tên bài hát muốn thêm vào
            vk.title = form["Id"];
            //Get UserName
            string getUser = form["GetUserName"];
            var getDataUser = data.AspNetUsers.ToList();


            //Get id bài hát kiểm tra có bị trùng trong data ko
            vk.id = form["CheckId"];
            var checkId = data.Videos.Where(m => m.id == vk.id).FirstOrDefault();

            //Youtube API
            test = await searchObject.RunYouTube(vk.title);
            vs = new List<Video>(test);

            //Check thể loại video
            var check = new Theloai();
            check.nameTheloai = form["theloai"];

            if (check.nameTheloai == "")
            {

                ViewBag.Message = "Check Your Opton!!";
                return View("~/Views/Home/CreateAdmin.cshtml", test);
            }

            for (var item = 0; item < test.Count; item++)
            {

                vs[item].title = test[item].title;
                vs[item].HinhNguonVideo = test[item].id;
                vs[item].author = test[item].author;
                vs[item].link = test[item].link;
                vs[item].vitrivideo = vs[item].id + a;
                vs[item].id = test[item].id + "/admin/" + getUser;
                vs[item].loaivideo = "admin" + vs[item].id;
                vs[item].duration = test[item].duration;
                for (var itemuser = 0; itemuser < getDataUser.Count; itemuser++)
                {
                    if (getDataUser[itemuser].Email == getUser)
                    {
                        vs[item].UserId = getDataUser[itemuser].Id;
                    }
                }
                if (check.nameTheloai == "hoa")
                {
                    vs[item].idTheloai = check.idTheloai = 1;

                }
                if (check.nameTheloai == "viet")
                {
                    vs[item].idTheloai = check.idTheloai = 2;

                }
                if (check.nameTheloai == "khac")
                {
                    vs[item].idTheloai = check.idTheloai = 4;

                }
                a++;
                if (checkId == default)
                {
                    if (vk.id == vs[item].id)
                    {
                        data.Videos.InsertOnSubmit(vs[item]);
                    }
                }
                else
                {
                    ViewBag.Message = "This Have In Your PlayList !!";
                    dynamic mymodel = new ExpandoObject();
                    mymodel.person = GetPerson();

                    mymodel.trangchu = from ss in data.Videos where ss.loaivideo != "user" select ss;
                    mymodel.videoTQ = from ss in data.Videos where ss.loaivideo != "user" && ss.idTheloai == 1 select ss;

                    mymodel.videoVN = from ss in data.Videos where ss.loaivideo != "user" && ss.idTheloai == 2 select ss;
                    return View("~/Views/Home/TrangChu.cshtml", mymodel);
                    /* ViewBag.Message = "This Have In Your PlayList !!";
                     return View("~/Views/Home/TrangChu.cshtml", test);*/
                }
            }
            data.SubmitChanges();
            //Sổ danh sách 

            var all_list = data.Videos.ToList();

            /*var all_sach = from ss in data.Videos where ss.loaivideo != "user" select ss;*/

            if (all_list.Count == 0)
            {
                ViewBag.Message = "You Not Have Anything In Playlist";
                dynamic mymodel = new ExpandoObject();
                mymodel.person = GetPerson();

                mymodel.trangchu = from ss in data.Videos where ss.loaivideo != "user" select ss;
                mymodel.videoTQ = from ss in data.Videos where ss.loaivideo != "user" && ss.idTheloai == 1 select ss;

                mymodel.videoVN = from ss in data.Videos where ss.loaivideo != "user" && ss.idTheloai == 2 select ss;
                return View("~/Views/Home/TrangChu.cshtml", mymodel);
                /*ViewBag.Message = "You Not Have Anything In Playlist";
                return View("~/Views/Home/TrangChu.cshtml", all_sach);*/
            }
            else
            {
                ViewBag.Message = "Your Playlist";
                dynamic mymodel = new ExpandoObject();
                mymodel.person = GetPerson();

                mymodel.trangchu = from ss in data.Videos where ss.loaivideo != "user" select ss;
                mymodel.videoTQ = from ss in data.Videos where ss.loaivideo != "user" && ss.idTheloai == 1 select ss;

                mymodel.videoVN = from ss in data.Videos where ss.loaivideo != "user" && ss.idTheloai == 2 select ss;
                return View("~/Views/Home/TrangChu.cshtml", mymodel);
                /*ViewBag.Message = "Your Playlist";
                return View("~/Views/Home/TrangChu.cshtml", all_sach);*/
            }
        }



        /// <summary>
        /// //////////
        /// </summary>
        /// <returns></returns>
        /*private async Task<List<Video>> GetVideoAsyncTQ()
        {
            int a = 0;

            test = await searchObject.RunYouTube("Nhạc Hot Trung Quốc TikTokS 2022");
            List<Video> video = new List<Video>();
            video = test;
            for (var item = 0; item < video.Count; item++)
            {
                video[item].vitrivideo = video[item].id + a;
                video[item].loaivideo = "1" + a;
                a++;
            }
            return video;
        }*/
        /*private async Task<List<Video>> GetVideoAsyncVN()
        {
            int a = 0;

            test = await searchObject.RunYouTube("Nhạc Hot Việt Nam 2022");
            List<Video> video = new List<Video>();
            video = test;
            for (var item = 0; item < video.Count; item++)
            {
                video[item].vitrivideo = video[item].id + a;
                video[item].loaivideo = "2" + a;
                a++;
            }
            return video;
        }*/
        [HttpPost]
        public ActionResult DetelePlaylistAdmin(string id, FormCollection collection)
        {
            var D_playlist = data.Videos.Where(m => m.id == id).First();
            data.Videos.DeleteOnSubmit(D_playlist);
            data.SubmitChanges();
            var AfterD = data.Videos.ToList();
            /*var all_sach = from ss in data.Videos where ss.loaivideo != "user" select ss;*/


            dynamic mymodel = new ExpandoObject();
            mymodel.person = GetPerson();

            mymodel.trangchu = from ss in data.Videos where ss.loaivideo != "user" select ss;
            mymodel.videoTQ = from ss in data.Videos where ss.loaivideo != "user" && ss.idTheloai == 1 select ss;

            mymodel.videoVN = from ss in data.Videos where ss.loaivideo != "user" && ss.idTheloai == 2 select ss;

            for (var item = 0; item < AfterD.Count; item++)
            {
                if (AfterD[item].loaivideo != "user")
                {
                    ViewBag.Message = "Your Playlist";
                    return View("~/Views/Home/TrangChu.cshtml", mymodel);
                }

            }
            ViewBag.Message = "You Not Have Anything In Playlist";
            return View("~/Views/Home/TrangChu.cshtml", mymodel);
            /*if (AfterD.Count == 0)
            {
                ViewBag.Message = "You Not Have Anything In Playlist";
                return View("~/Views/Home/TrangChu.cshtml", mymodel);
                *//* return View("~/Views/Home/TrangChu.cshtml", all_sach);*//*
            }
            else
            {
                ViewBag.Message = "Your Playlist";
                return View("~/Views/Home/TrangChu.cshtml", mymodel);
                *//*return View("~/Views/Home/TrangChu.cshtml", all_sach);*//*
            }*/
        }
        [HttpPost]
        public ActionResult SearchInPlaylist(FormCollection collection)
        {

            string timkiem = collection["SearchInPlaylist"];
            var D_playlist = data.Videos.ToList();
            /*var list = new List<Video>(D_playlist.Count);*/
            for (var item = 0; item < D_playlist.Count; item++)
            {
                if (D_playlist[item].title.Contains(timkiem))
                {
                    D_playlist[item].vitrivideo = "1";
                    /*list[item].id = D_playlist[item].id;
                    list[item].title = D_playlist[item].title;
                    list[item].author = D_playlist[item].author;
                    list[item].link = D_playlist[item].link;
                    list[item].vitrivideo = D_playlist[item].vitrivideo;
                    list[item].loaivideo = D_playlist[item].loaivideo;
                    list[item].idTheloai = D_playlist[item].idTheloai;*/
                }
            }
            var list = from ss in D_playlist where ss.vitrivideo == "1" && ss.loaivideo != "user" select ss;
            ViewBag.Message = "Your Search In PlayList!!";
            dynamic mymodel = new ExpandoObject();
            mymodel.person = GetPerson();

            mymodel.trangchu = list;
            mymodel.videoTQ = from ss in data.Videos where ss.loaivideo != "user" && ss.idTheloai == 1 select ss;

            mymodel.videoVN = from ss in data.Videos where ss.loaivideo != "user" && ss.idTheloai == 2 select ss;
            return View("~/Views/Home/TrangChu.cshtml", mymodel);
            /*return View("~/Views/Home/TrangChu.cshtml", list);*/
        }


        [HttpPost]
        public ActionResult Post()
        {
            if (checkUserorAdmin() == false)
            {
                ViewBag.Message = "Bạn Không Có Quyền Hạn Này cho tài khoản";
                return View("~/Views/MyMusicProfile/Post.cshtml");
            }
            else
            {
                ViewBag.Message = "Xin Chào Admin";
                return View("~/Views/Home/TrangAdmin.cshtml");
            }

        }

        [Authorize(Users = "xincaiten2001@gmail.com")]
        [HttpGet]
        public ActionResult TacGia()
        {
            dynamic mymodel = new ExpandoObject();
            mymodel.personTQ = from ss in data.Persons where ss.idTheloai == 1 select ss;
            mymodel.personVN = from ss in data.Persons where ss.idTheloai == 2 select ss;
            mymodel.videoTQ = from ss in data.Videos where ss.loaivideo != "user" && ss.idTheloai == 1 select ss;

            mymodel.videoVN = from ss in data.Videos where ss.loaivideo != "user" && ss.idTheloai == 2 select ss;
            return View(mymodel);

        }

        [Authorize]
        [HttpGet]
        public ActionResult CreateTacGia()
        {

            return View();
        }
        [HttpPost]
        public ActionResult CreateTacGia(FormCollection fr,Person s)
        {


            var getname = fr["name"];
            var getmota = fr["mota"];
            var gethinh = fr["hinh"];
            var getnguongoc = fr["nguongoc"];




            dynamic mymodel = new ExpandoObject();
            mymodel.personTQ = from ss in data.Persons where ss.idTheloai == 1 select ss;
            mymodel.personVN = from ss in data.Persons where ss.idTheloai == 2 select ss;
            mymodel.videoTQ = from ss in data.Videos where ss.loaivideo != "user" && ss.idTheloai == 1 select ss;

            mymodel.videoVN = from ss in data.Videos where ss.loaivideo != "user" && ss.idTheloai == 2 select ss;
            var dataperson = data.Persons.ToList();
            s.namePerson = getname;
            s.Mota = getmota;
            s.hinhPerson = gethinh;
            if(getnguongoc == "hoa")
            {
                s.idTheloai = 1;
            }
            if (getnguongoc == "viet")
            {
                s.idTheloai = 2;
            }
            if (getnguongoc == "")
            {
                ViewBag.Message = "Check Your Option!!";
                return View("~/Views/Home/TacGia.cshtml", mymodel);
            }
            data.Persons.InsertOnSubmit(s);
            data.SubmitChanges();

            return View("~/Views/Home/TacGia.cshtml", mymodel);
        }
        [HttpPost]
        public ActionResult DetailsTacGia(int id)
        {
            var datatacgia = from ss in data.Persons where ss.idPerson == id select ss;
            return View(datatacgia);

        }

        [HttpPost]
        public ActionResult EditTacGia(FormCollection id)
        {
            var getid = Convert.ToInt32(id["Id"]);
            var getuser = from ss in data.Persons where ss.idPerson == getid select ss;
            return View(getuser);
        }
        [HttpPost]
        public ActionResult EditTG(FormCollection fr)
        {
            var getid = Convert.ToInt32(fr["Id"]);
            var getname = fr["name"];
            var getmota = fr["mota"];
            var gethinh = fr["hinh"];
            var getnguongoc = fr["nguongoc"];

            dynamic mymodel = new ExpandoObject();
            mymodel.personTQ = from ss in data.Persons where ss.idTheloai == 1 select ss;
            mymodel.personVN = from ss in data.Persons where ss.idTheloai == 2 select ss;
            mymodel.videoTQ = from ss in data.Videos where ss.loaivideo != "user" && ss.idTheloai == 1 select ss;

            mymodel.videoVN = from ss in data.Videos where ss.loaivideo != "user" && ss.idTheloai == 2 select ss;
            var dataperson = data.Persons.ToList();
            for (var item = 0; item < dataperson.Count; item++)
            {
                if (dataperson[item].idPerson == getid)
                {
                    dataperson[item].namePerson = getname;
                    dataperson[item].Mota = getmota;
                    dataperson[item].hinhPerson = gethinh;
                    if (getnguongoc == "hoa")
                    {
                        dataperson[item].idTheloai = 1;
                    }
                    if (getnguongoc == "viet")
                    {
                        dataperson[item].idTheloai = 2;
                    }
                    if (getnguongoc == "")
                    {
                        ViewBag.Message = "Check your option Nguồn Gốc!!";
                        var getuser = from ss in data.Persons where ss.idPerson == getid select ss;
                        return View("~/Views/Home/EditTacGia.cshtml", getuser);
                    }
                    UpdateModel(dataperson[item]);
                }
            }
            data.SubmitChanges();
            return View("~/Views/Home/TacGia.cshtml", mymodel);

        }
        [HttpPost]
        public ActionResult DeleteTacGia(FormCollection id)
        {
            var getid = Convert.ToInt32(id["Id"]);
            var D_playlist = data.Persons.Where(m => m.idPerson == getid).First();
            data.Persons.DeleteOnSubmit(D_playlist);
            data.SubmitChanges();

            dynamic mymodel = new ExpandoObject();
            mymodel.personTQ = from ss in data.Persons where ss.idTheloai == 1 select ss;
            mymodel.personVN = from ss in data.Persons where ss.idTheloai == 2 select ss;
            mymodel.videoTQ = from ss in data.Videos where ss.loaivideo != "user" && ss.idTheloai == 1 select ss;

            mymodel.videoVN = from ss in data.Videos where ss.loaivideo != "user" && ss.idTheloai == 2 select ss;

            return View("~/Views/Home/TacGia.cshtml", mymodel);
        }
        [HttpPost]
        public string ProcessUpload(HttpPostedFileBase file)
        {
            if (file == null)
            {
                return "";
            }
            file.SaveAs(Server.MapPath("~/Content/images/" + file.FileName));
            return "/Content/images/" + file.FileName;
        }
    }
}
