// oxygen_sender.js (Run this on the sending machine using Node.js)
import { WebSocketServer } from 'ws';

// The port the sender will listen on for the receiver to connect to
const PORT = 8080; 
const wss = new WebSocketServer({ port: PORT });

console.log(`🚀 JavaScript Sender running on ws://0.0.0.0:${PORT}`);

// Function to generate a random oxygen level
function generateOxygenLevel() {
    // Generates a float between 90.0 and 100.0
    return (Math.random() * 10 + 90).toFixed(2);
}

// Event handler for a receiving server connecting
wss.on('connection', function connection(ws) {
    console.log('✅ Python Receiver connected.');

    // Function to send data
    const sendOxygenData = () => {
        const oxygenLevel = generateOxygenLevel();
        const data = {
            timestamp: new Date().toISOString(),
            oxygenLevel: oxygenLevel,
            unit: '%',
            status: oxygenLevel > 94 ? 'Normal' : 'Low'
        };

        // Convert the JavaScript object to a **JSON string**
        const jsonString = JSON.stringify(data);

        // Send the JSON string to the connected receiver
        if (ws.readyState === ws.OPEN) {
            ws.send(jsonString);
            console.log(`[SENT] ${jsonString}`);
        }
    };

    // Set an interval to send data every 2 seconds
    const interval = setInterval(sendOxygenData, 2000);

    // Handle receiver closing the connection
    ws.on('close', () => {
        console.log('❌ Receiver disconnected. Stopping data transmission.');
        // Clear the interval when the client disconnects
        clearInterval(interval); 
    });

    ws.on('error', console.error);
});
