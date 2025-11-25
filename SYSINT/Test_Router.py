import asyncio
import websockets
import json
import zmq

SENDER_IP = '192.168.0.55'   # WebSocket source (your JS program)
PORT = 8080
URI = f"ws://{SENDER_IP}:{PORT}"

# -------------------------
# SET UP ZERO MQ PUBLISHER
# -------------------------
zmq_context = zmq.Context()
pub_socket = zmq_context.socket(zmq.PUB)

# This is where OTHER PROGRAMS (subscribers) will connect
pub_socket.bind("tcp://0.0.0.0:5555")
print("ZeroMQ Publisher online at tcp://0.0.0.0:5555")

async def receive_and_broadcast():
    print(f"Connecting to WebSocket sender at {URI}...")

    try:
        async with websockets.connect(URI) as websocket:
            print("WebSocket connected! Broadcasting over ZeroMQ...")

            while True:
                # Receive JSON string from WebSocket
                json_string = await websocket.recv()

                # Try to decode it for terminal logging
                try:
                    oxygen_data = json.loads(json_string)
                    print("\n--- RECEIVED OXYGEN DATA ---")
                    print(f"Time:   {oxygen_data.get('timestamp')}")
                    print(f"Level:  {oxygen_data.get('oxygenLevel')}%")
                    print(f"Status: {oxygen_data.get('status')}")
                    print("----------------------------------")

                except json.JSONDecodeError:
                    print("Received non-JSON data:", json_string)

                # -------------------------
                # ZERO MQ BROADCAST
                # -------------------------
                pub_socket.send_string(json_string)
                print("[ZMQ] Broadcasted oxygen packet.")

    except Exception as e:
        print("Error:", e)

if __name__ == "__main__":
    asyncio.run(receive_and_broadcast())
