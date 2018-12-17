const connection = new signalR.HubConnectionBuilder()
    .withUrl("/streams/flare")
    .configureLogging(signalR.LogLevel.Information)
    .build();

let _debug = false;
let state = "unknown"; 
_printState()

connection.on("output", (output) => {
    console.log(output);
    let outputElement = document.getElementById("output");
    outputElement.textContent = output.toString();
});

connection.on("error", (errMsg) => {
    console.log(errMsg);
    let errMsgElement = document.getElementById("errMsg");
    errMsgElement.textContent = errMsg.toString();
});

connection.on("exitCode", (exitCode) => {
    console.log(exitCode);
    let exitCodeElement = document.getElementById("exitCode");
    exitCodeElement.textContent = exitCode.toString();
});

function Start() {
    
    if (state != "connected" && state != "connecting") {
        state = "connecting";
        _printState()
        connection
            .start()
            .then(function (s) {
                state = "connected";
                _printState()
            })
            .catch(function (err) {
                state = "error";
                _printState()
                return console.error(err.toString());
            });
    }
};

function Stop() {
    if (state === "connected" || state === "connecting") {
        state = "disconnecting";
        _printState()  
        connection
            .stop()
            .then(function() {
                state = "disconnected"; 
                _printState()
            })
            .catch(function(err) {
                state = "error";
                _printState()
                return console.error(err.toString());
            });
    }
}
/*
async function Reconnect() {
    try {
        await Start();
    } catch (err) {
        console.log(err);
    }

    // if still not connected, try again in a bit.
    if (state ==="error") {
        setTimeout(() => Reconnect(), 1000);
    }
};*/

function _printState() {
    if (_debug) {
        console.debug(state);
    }
    else {
        console.log(state);
    }
}
/*
connection.onclose(async () => {
    if ( state === "error") {
        await reconnect();
    }
});*/