import zmq
import json

ctx = zmq.Context()
sub = ctx.socket(zmq.SUB)

# Connect to your Python broadcaster
sub.connect("tcp://127.0.0.1:5555")

# Subscribe to all messages (empty filter)
sub.setsockopt_string(zmq.SUBSCRIBE, "")

print("Subscriber connected to ZeroMQ broadcaster...")

while True:
    msg = sub.recv_string()
    data = json.loads(msg)
    print("[SUBSCRIBER] Oxygen:", data)
