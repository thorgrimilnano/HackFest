using ClosedCapt2.Models;
using ClosedCapt2.Service;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
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

        public ActionResult Index()
        {
            //var transcript = new Transcipt();
            
            //transcript.AppendToTranscript(10, "This is a another test.");

            return View();
        }

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


        [HttpGet]
        public void Train()
        {
            var personGroupId = "1";
            var personGroupName = "Darrell";
            var filePath = "~/images/Darrell/";
            var task = TrainFacialRecognizitionAsync(personGroupId, personGroupName, filePath);
            task.Start();

        }

        [HttpGet]
        public JsonResult Identify()
        {
            var FRId = "group4Eugene";
            var speaker = new Speaker(FRId);
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