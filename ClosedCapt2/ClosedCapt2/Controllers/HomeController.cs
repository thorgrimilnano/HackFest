using ClosedCapt2.Models;
using ClosedCapt2.Service;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.Bing.Speech;

namespace ClosedCapt2.Controllers
{
    public class HomeController : Controller
    {
        private readonly IFaceServiceClient faceServiceClient = new FaceServiceClient("41fa2860658b4bc3b7c467f58f2a6cdf", "https://eastus.api.cognitive.microsoft.com/face/v1.0");

        const string personGroupId = "1";

        public ActionResult Index()
        {
            //var transcript = new Transcipt();
            
            //transcript.AppendToTranscript(10, "This is a another test.");

            return View();
        }

        [HttpPost]
        public async Task<JsonResult> SpeechRecogAsync()
        {
            var filePath = Server.MapPath("~/Audio/1.wav");
            var converter = new Speech();
            await new Speech().Run(filePath, "it-IT");
            return Json(true,JsonRequestBehavior.AllowGet);
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

        public async Task<JsonResult> IdentifyAsync(string imageData)
        {
            byte[] bytes = Convert.FromBase64String(imageData);

            Image image;
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                image = Image.FromStream(ms);
            }

            var fileName = "~/Images/Test/IdentifyThisPerson.png";
            var filePath = Server.MapPath(fileName);
            image.Save(filePath);

            var faces = await faceServiceClient.DetectAsync(Request.Url.Scheme + "://" + Request.Url.Authority + Url.Content(fileName)); ;
            var faceIds = faces.Select(face => face.FaceId).ToArray();

            var results = await faceServiceClient.IdentifyAsync(personGroupId, faceIds);
            Person person = new Person();
            foreach (var identifyResult in results)
            {
                Console.WriteLine("Result of face: {0}", identifyResult.FaceId);
                if (identifyResult.Candidates.Length != 0)
                {
                    // Get top 1 among all candidates returned
                    var candidateId = identifyResult.Candidates[0].PersonId;
                    person = await faceServiceClient.GetPersonAsync(personGroupId, candidateId);
                }
            }

            return Json(new { Data = person }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task LoadData()
        {

            await faceServiceClient.CreatePersonGroupAsync(personGroupId, "Speakers");

            await TrainFacialRecognizitionAsync(personGroupId, "Darrell", Server.MapPath("~/Images/Darrell/"));
            await TrainFacialRecognizitionAsync(personGroupId, "Ben", Server.MapPath("~/Images/Ben/"));
            await TrainFacialRecognizitionAsync(personGroupId, "Ciro", Server.MapPath("~/Images/Ciro/"));
            await TrainFacialRecognizitionAsync(personGroupId, "Eugene", Server.MapPath("~/Images/Eugene/"));

        }

        [HttpGet]
        public async Task TrainData()
        {
            await faceServiceClient.TrainPersonGroupAsync(personGroupId);
}
        [HttpGet]
        public JsonResult Identify(string userId)
        {
            var speaker = new Speaker(userId);
            speaker.GetSpeaker();


            return Json(speaker, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult StartStreaming(int session)
        {
            var transcript = new Transcipt();
            transcript.CreateNewTranscript(session);

            return Json(transcript, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public void SendAudioData(object data)
        {
            //Send to translater

            //Send to speech to text


        }


        public async Task TrainFacialRecognizitionAsync(string personGroupId, string personGroupName, string filePath)
        {
            CreatePersonResult person = await faceServiceClient.CreatePersonAsync(personGroupId, personGroupName);
            foreach (string imagePath in Directory.GetFiles(filePath))
            {
                using (Stream s = System.IO.File.OpenRead(imagePath))
                {
                    await faceServiceClient.AddPersonFaceAsync(personGroupId, person.PersonId, s);
                }
            }
        }


    }
}