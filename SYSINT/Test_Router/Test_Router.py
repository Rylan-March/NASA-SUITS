import asyncio
import websockets
import json

SENDER_IP = '127.0.0.1' #Replace with the Senders IP
PORT = 8080
URI = f"ws://{SENDER_IP}:{PORT}"

async def receive_oxygen_data():
    print(f"Connecting to JavaScript Sender at {URI}...")
    
    try:
        # Establish the WebSocket connection
        # The 'async with' ensures the connection is closed cleanly when done
        async with websockets.connect(URI) as websocket:
            print("Connection established. Waiting for data...")
            
            # Loop forever, listening for incoming messages
            while True:
                # Wait for the next message (received as a JSON string)
                json_string = await websocket.recv()
                
                try:
                    # Parse the JSON string into a Python dictionary
                    oxygen_data = json.loads(json_string)
                    
                    #Terminal Output
                    print("\n--- RECEIVED OXYGEN DATA (Python) ---")
                    print(f"Time:     {oxygen_data.get('timestamp')}")
                    print(f"Level:    **{oxygen_data.get('oxygenLevel')}%**")
                    print(f"Status:   {oxygen_data.get('status')}")
                    print("-------------------------------------")
                    

                except json.JSONDecodeError:
                    print(f"Error decoding JSON: Received non-JSON data: {json_string}")
                
    except ConnectionRefusedError:
        print(f"Connection Refused: Ensure the sender is running at {SENDER_IP}:{PORT} and firewall is open.")
    except websockets.ConnectionClosedOK:
        print("Connection closed cleanly by the sender.")
    except Exception as e:
        print(f"An unexpected error occurred: {e}")

if __name__ == "__main__":
    # The websockets library requires the asyncio event loop to run
    asyncio.run(receive_oxygen_data())
