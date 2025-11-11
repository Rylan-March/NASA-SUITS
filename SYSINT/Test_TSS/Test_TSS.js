import { WebSocketServer } from 'ws';

//the desired port that it will listen on
const PORT = 8080; 
const wss = new WebSocketServer({ port: PORT });

console.log(`TSS running on ws://0.0.0.0:${PORT}`);

// Function to generate a random oxygen level
function generateOxygenLevel() {
    // Generates a float between 90.0 and 100.0
    return (Math.random() * 10 + 90).toFixed(2);
}

// Event handler for a receiving server connecting
wss.on('connection', function connection(ws) {
    console.log('Receiver connected.');

    // Function to send data...it creates an object with data that will then be turned to a JSON formatted string.
    const sendOxygenData = () => {
        const oxygenLevel = generateOxygenLevel();
        const data = {
            timestamp: new Date().toISOString(),
            oxygenLevel: oxygenLevel,
            unit: '%',
            status: oxygenLevel > 94 ? 'Normal' : 'Low'
        };

        // Convert the JavaScript object to a JSON string
        const jsonString = JSON.stringify(data);

        // Send the JSON string to the connected receiver
        if (ws.readyState === ws.OPEN) {
            ws.send(jsonString);
            console.log(`[SENT] ${jsonString}`);
        }
    };

    //set an interval to send data every 2 seconds
    const interval = setInterval(sendOxygenData, 2000);

    //handle receiver closing the connection
    ws.on('close', () => {
        console.log('Receiver disconnected. Stopping data transmission.');
        //clear the interval when the client disconnects
        clearInterval(interval); 
    });

    ws.on('error', console.error);
});
