﻿let isInceptionTrained = false;
let raspberryPiUrl;
let visionUrl;
let cameraHub;
let sensorsHub;
let capture = {};
let sensors = {};

document.addEventListener('DOMContentLoaded', async (event) => {

    // initialize
    document.querySelector("#stop").style.display = "none";
    document.querySelector("#start").style.display = "block";
    document.querySelector("#live").style.display = "none";

    await getSettings().then((response) => response.json())
        .then(function (data) {
            raspberryPiUrl = data.raspberryPiUrl;
            cameraHub = data.cameraHub;
            sensorsHub = data.sensorsHub;
            visionUrl = data.visionUrl;

            console.log(`camera hub ${raspberryPiUrl}${cameraHub}`);
            console.log(`sensors hub ${raspberryPiUrl}${sensorsHub}`);
        })
        .catch(function (err) {
            console.log(err.message);
        });

    ////////////////////// CAMERA /////////////////////////////
    const cameraConnection = new signalR.HubConnectionBuilder()
        .configureLogging(signalR.LogLevel.Information)
        .withUrl(raspberryPiUrl + cameraHub)
        .build();

    cameraConnection.on("cameraStreamingStarted", function () {
        console.log("CAMERA STREAMING STARTED");
        document.querySelector("#live").style.display = "block";
        cameraConnection.stream("CameraCaptureLoop").subscribe({
            close: false,
            next: data => {
                populateCameraData(data);
            },
            err: err => {
                console.log(err);
            },
            complete: () => {
                console.log("finished camera streaming");
            }
        });
    });

    cameraConnection.on("cameraStreamingStopped", function () {
        document.querySelector("#live").style.display = "none";
        console.log("CAMERA STREAMING STOPPED");
    });

    cameraConnection.on("cameraImageCaptured", function (data) {
        console.log(`image captured, size: ${data}`);
    });

    cameraConnection.start();

    ////////////////////// SENSORS /////////////////////////////
    const sensorsConnection = new signalR.HubConnectionBuilder()
        .configureLogging(signalR.LogLevel.Information)
        .withUrl(raspberryPiUrl + sensorsHub)
        .build();

    sensorsConnection.on("sensorsStreamingStarted", function () {
        console.log("SENSORS STREAMING STARTED");
        sensorsConnection.stream("SensorsCaptureLoop").subscribe({
            close: false,
            next: data => {
                populateSensorsData(data);
            },
            err: err => {
                console.log(err);
            },
            complete: () => {
                console.log("finished sensors streaming");
            }
        });
    });

    sensorsConnection.on("sensorsStreamingStopped", function () {
        console.log("SENSORS STREAMING STOPPED");
    });

    sensorsConnection.on("sensorsDataCaptured", function (data) {
        console.log(`sensors data captured (is alarm: ${data.isAlarm})`);
    });

    sensorsConnection.on("sensorsDataNotCaptured", function () {
        console.log(`sensors data not captured!`);
    });

    sensorsConnection.start();

    ////// EVENTS ///////////////////////////////////////////////

    document.querySelector("#start").onclick = function () {
        if (cameraConnection.connection.connectionState == 1) {
            cameraConnection.invoke("StartCameraStreaming");
        } else {
            console.log("device (camera) is not connected.");
        }

        if (sensorsConnection.connection.connectionState == 1) {
            sensorsConnection.invoke("StartSensorsStreaming");
        } else {
            console.log("device (sensors) is not connected.");
        }

        if (sensorsConnection.connection.connectionState == 1 && cameraConnection.connection.connectionState == 1)
        {
            document.querySelector("#start").style.display = "none";
            document.querySelector("#stop").style.display = "block";
        }
    }

    document.querySelector("#stop").onclick = function () {
        if (cameraConnection.connection.connectionState == 0) {
            console.log("device (camera) is not connected.");
        } else {
            cameraConnection.invoke("StopCameraStreaming");
        }

        if (sensorsConnection.connection.connectionState == 0) {
            console.log("device (sensors) is not connected.");
        } else {
            sensorsConnection.invoke("StopSensorsStreaming");
        }

        if (sensorsConnection.connection.connectionState == 1 && cameraConnection.connection.connectionState == 1) {
            document.querySelector("#start").style.display = "block";
            document.querySelector("#stop").style.display = "none";
        }
    }

    document.querySelector("#inception_train").onclick = function () {
        startInceptionTraining();
        document.querySelector("#inception_train").style.display = "none";
    }
})

function populateCameraData(data) {
    if (data !== undefined) {
        if (data.image !== undefined) {
            document.querySelector("#camera").setAttribute("src", `data:image/jpg;base64,${data.image}`);
            capture.createdAt = data.createdAt;
            getPredictionByImage(data.image);
        }
    }
}

function populateSensorsData(data) {
    if (data !== undefined) {
        if (data.luminosity !== undefined) {
            sensors.luminosity = data.luminosity;
            document.querySelector("#lux").innerHTML = `${data.luminosity} lux`;
        }
        if (data.humidity !== undefined) {
            sensors.humidity = data.humidity;
            document.querySelector("#humid").innerHTML = `${data.humidity} %`;
        }
        if (data.temperature !== undefined) {
            sensors.temperature = data.temperature;
            document.querySelector("#temp").innerHTML = `${data.temperature} &deg;C`;
        }
        if (data.infrared !== undefined) {
            sensors.infrared = data.infrared;
            document.querySelector("#infra").innerHTML = `${data.infrared}`;
        }
        if (data.distance !== undefined) {
            sensors.distance = data.distance;
            document.querySelector("#dist").innerHTML = `${data.distance} cm`;
        }
        if (data.createdAt !== undefined) {
            sensors.createdAt = data.createdAt;
        }
        if (data.isAlarm !== undefined) {
            sensors.isAlarm = data.isAlarm;
            document.querySelector("#sensors_prediction").innerHTML = `${data.isAlarm === true ? "ALARM!" : "Not Alarm"}`;
        }

        console.log(`Is alarm: ${data.isAlarm}`);
    }
}

async function getSettings() {

    let url = "api/main/settings";

    let result = fetch(url, {
        method: 'GET',
        mode: 'cors'
    })

    return result;
}

function startInceptionTraining() {

    let url = `${visionUrl}/api/images/train_inception`;

    fetch(url, {
        method: 'GET',
        mode: 'cors'
    })
        .then((response) => {
            isInceptionTrained = true;
        })
        .catch(function (err) {
            console.log(err.message);
        });
}

function getPredictionByImage(image) {

    let url = `${visionUrl}/api/images/predict_image`;

    document.querySelector("#prediction").innerHTML = "";

    fetch(url, {
        method: 'POST',
        mode: 'cors',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/octet-stream'
        },
        body: image
    })
        .then((response) => response.json())
        .then(function (data) {
            if (data !== undefined) {
                console.log(`Class: ${data}`);
                capture.source = data;
                document.querySelector("#prediction").innerHTML = data;
            }
        })
        .catch(function (err) {
            console.log(err.message);
        });
}
