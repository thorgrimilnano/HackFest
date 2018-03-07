using ClosedCapt2.Models;
using Microsoft.Bing.Speech;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace ClosedCapt2.Service
{
    public class Speech
    {
        public Speech() {
            transcript = new Transcipt();
            transcript.CreateNewTranscript(99);
        }
        private Transcipt transcript;
        /// <summary>
        /// The long dictation URL
        /// </summary>
        private static readonly Uri LongDictationUrl = new Uri(@"wss://speech.platform.bing.com/api/service/recognition/continuous");

        /// <summary>
        /// A completed task
        /// </summary>
        private static readonly Task CompletedTask = Task.FromResult(true);

        private static readonly string key = "a981cc903f61446c861d5ef64c40a74f";

        /// <summary>
        /// Cancellation token used to stop sending the audio.
        /// </summary>
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        

        /// <summary>
        /// Invoked when the speech client receives a partial recognition hypothesis from the server.
        /// </summary>
        /// <param name="args">The partial response recognition result.</param>
        /// <returns>
        /// A task
        /// </returns>
        public Task OnPartialResult(RecognitionPartialResult args)
        {
            transcript.AppendToTranscript(99, args.DisplayText);

            return CompletedTask;
        }

        /// <summary>
        /// Invoked when the speech client receives a phrase recognition result(s) from the server.
        /// </summary>
        /// <param name="args">The recognition result.</param>
        /// <returns>
        /// A task
        /// </returns>
        public Task OnRecognitionResult(RecognitionResult args)
        {
            var response = args;
            
            if (response.Phrases != null)
            {                
                transcript.AppendToTranscript(99, response.Phrases[0].DisplayText);
            }

            return CompletedTask;
        }

        /// <summary>
        /// Sends a speech recognition request to the speech service
        /// </summary>
        /// <param name="audioFile">The audio file.</param>
        /// <param name="locale">The locale.</param>
        /// <param name="serviceUrl">The service URL.</param>
        /// <param name="subscriptionKey">The subscription key.</param>
        /// <returns>
        /// A task
        /// </returns>
        public async Task Run(string audioFile, string locale)
        {
            // create the preferences object
            var preferences = new Preferences(locale, LongDictationUrl, new CognitiveServicesAuthorizationProvider(key));

            // Create a a speech client
            using (var speechClient = new SpeechClient(preferences))
            {
                speechClient.SubscribeToPartialResult(this.OnPartialResult);
                speechClient.SubscribeToRecognitionResult(this.OnRecognitionResult);

                // create an audio content and pass it a stream.
                using (var audio = new FileStream(audioFile, FileMode.Open, FileAccess.Read))
                {
                    var deviceMetadata = new DeviceMetadata(DeviceType.Near, DeviceFamily.Desktop, NetworkType.Ethernet, OsName.Windows, "1607", "Dell", "T3600");
                    var applicationMetadata = new ApplicationMetadata("SpeechRecog", "1.0.0");
                    var requestMetadata = new RequestMetadata(Guid.NewGuid(), deviceMetadata, applicationMetadata, "SpeechRecog");

                    await speechClient.RecognizeAsync(new SpeechInput(audio, requestMetadata), this.cts.Token).ConfigureAwait(false);
                }
            }
        }
    }
}