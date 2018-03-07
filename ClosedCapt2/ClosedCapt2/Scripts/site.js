/*
*  Copyright (c) 2015 The WebRTC project authors. All Rights Reserved.
*
*  Use of this source code is governed by a BSD-style license
*  that can be found in the LICENSE file in the root of the source
*  tree.
*/

// This code is adapted from
// https://rawgit.com/Miguelao/demos/master/mediarecorder.html

'use strict';

/* globals MediaRecorder */
var identifiedUser = false;

var mediaSource = new MediaSource();
mediaSource.addEventListener('sourceopen', handleSourceOpen, false);
var mediaRecorder, audioRecorder;
var recordedBlobs;
var sourceBuffer;
var speaker, transcript;

var gumVideo = document.querySelector('video#gum');
var recordedAudio = document.querySelector('audio#recorded');

var recordButton = document.querySelector('button#record');
var playButton = document.querySelector('button#play');
var downloadButton = document.querySelector('button#download');
var identifyButton = document.querySelector('button#identify');
var voicerecog = document.querySelector('button#voicerecog');

recordButton.onclick = toggleRecording;
playButton.onclick = play;
downloadButton.onclick = download;
identifyButton.onclick = identifySpeaker;
voicerecog.onclick = voiceRecog;

// window.isSecureContext could be used for Chrome
var isSecureOrigin = location.protocol === 'https:' ||
    location.hostname === 'localhost';
if (!isSecureOrigin) {
    alert('getUserMedia() must be run from a secure origin: HTTPS or localhost.' +
        '\n\nChanging protocol to HTTPS');
    location.protocol = 'HTTPS';
}

var constraints = {
    audio: true,
    video: true
};

function handleSuccess(stream) {

    console.log('getUserMedia() got stream: ', stream);
    window.stream = stream;
    gumVideo.srcObject = stream;
}

function handleError(error) {
    console.log('navigator.getUserMedia error: ', error);
}

navigator.mediaDevices.getUserMedia(constraints).
    then(handleSuccess).catch(handleError);

function handleSourceOpen(event) {
    console.log('MediaSource opened');
    sourceBuffer = mediaSource.addSourceBuffer('video/webm; codecs="vp8"');
    console.log('Source buffer: ', sourceBuffer);
}

recordedAudio.addEventListener('error', function (ev) {
    console.error('MediaRecording.recordedMedia.error()');
    alert('Your browser can not play\n\n' + recordedAudio.src
        + '\n\n media clip. event: ' + JSON.stringify(ev));
}, true);

function handleDataAvailable(event) {
    if (event.data && event.data.size > 0) {
        recordedBlobs.push(event.data);

        //TODO split and send blob to places
    }
}

function handleAudioDataAvailable(event) {
    if (event.data && event.data.size > 0) {
        recordedBlobs.push(event.data);

        //TODO split and send blob to places
    }
}

function handleStop(event) {
    console.log('Recorder stopped: ', event);
}

function toggleRecording() {
    if (recordButton.textContent === 'Start Translating') {
        startRecording();
    } else {
        stopRecording();
        recordButton.textContent = 'Start Translating';
        playButton.disabled = false;
        downloadButton.disabled = false;
        identifyButton.disabled = false;
    }
}
function identifySpeaker() {
    recordButton.disabled = false;

    $.get("Home/Identify", function (data) {
        speaker = data;
        console.log(speaker)
    })

}

function startRecording() {
    recordedBlobs = [];
    var options = { mimeType: 'video/webm;codecs=vp9' };
    var audioOptions = { mimeType: 'audio/webm' };
    if (!MediaRecorder.isTypeSupported(options.mimeType)) {
        console.log(options.mimeType + ' is not Supported');
        options = { mimeType: 'video/webm;codecs=vp8' };
        if (!MediaRecorder.isTypeSupported(options.mimeType)) {
            console.log(options.mimeType + ' is not Supported');
            options = { mimeType: 'video/webm' };
            if (!MediaRecorder.isTypeSupported(options.mimeType)) {
                console.log(options.mimeType + ' is not Supported');
                options = { mimeType: '' };
            }
        }
    }
    try {
        mediaRecorder = new MediaRecorder(window.stream, options);
        audioRecorder = new MediaRecorder(window.stream, audioOptions);
    } catch (e) {
        console.error('Exception while creating MediaRecorder: ' + e);
        alert('Exception while creating MediaRecorder: '
            + e + '. mimeType: ' + options.mimeType);
        return;
    }
    console.log('Created MediaRecorder', mediaRecorder, 'with options', options);
    recordButton.textContent = 'Stop Recording';
    playButton.disabled = true;
    downloadButton.disabled = true;
    identifyButton.disable = true;
    mediaRecorder.onstop = handleStop;
    //mediaRecorder.ondataavailable = handleDataAvailable;
    mediaRecorder.start(10); // collect 10ms of data

    audioRecorder.onstop = handleStop;
    audioRecorder.ondataavailable = handleAudioDataAvailable;
    audioRecorder.start(10); // collect 10ms of data
    console.log('MediaRecorder started', mediaRecorder);

    //Initialize a transcript
    $.get('Home/StartStreaming', {session : 20}, function (data) {
        transcript = data
    })
}

function stopRecording() {
    mediaRecorder.stop();
    console.log('Recorded Blobs: ', recordedBlobs);
    recordedAudio.controls = true;
}

function play() {
    var superBuffer = new Blob(recordedBlobs, { type: 'audio/webm' });
    recordedAudio.src = window.URL.createObjectURL(superBuffer);
    // workaround for non-seekable video taken from
    // https://bugs.chromium.org/p/chromium/issues/detail?id=642012#c23
    recordedAudio.addEventListener('loadedmetadata', function () {
        if (recordedAudio.duration === Infinity) {
            recordedAudio.currentTime = 1e101;
            recordedAudio.ontimeupdate = function () {
                recordedAudio.currentTime = 0;
                recordedAudio.ontimeupdate = function () {
                    delete recordedAudio.ontimeupdate;
                    recordedAudio.play();
                };
            };
        }
    });
}

function download() {
    var blob = new Blob(recordedBlobs, { type: 'video/webm' });
    var url = window.URL.createObjectURL(blob);
    var a = document.createElement('a');
    a.style.display = 'none';
    a.href = url;
    a.download = 'test.webm';
    document.body.appendChild(a);
    a.click();
    setTimeout(function () {
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
    }, 100);
}

function voiceRecog() {
    $.ajax({
        type: "POST",
        url: "/Home/SpeechRecogAsync"
    });
}
