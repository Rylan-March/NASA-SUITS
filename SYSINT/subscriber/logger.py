import zmq
import json
from datetime import datetime

LOG_FILE = "oxygen_log.txt"

def main():
    # ---------------------
    # ZeroMQ SUB SETUP
    # ---------------------
    context = zmq.Context()
    sub = context.socket(zmq.SUB)
    sub.connect("tcp://127.0.0.1:5555")  # Connect to your broadcaster
    sub.setsockopt_string(zmq.SUBSCRIBE, "")  # receive all topics

    print("[LOGGER] Connected to ZeroMQ stream...")
    print(f"[LOGGER] Logging to {LOG_FILE}")

    # ---------------------
    # Main Receive Loop
    # ---------------------
    while True:
        try:
            raw_msg = sub.recv_string()
            data = json.loads(raw_msg)

            timestamp   = data.get("timestamp")
            oxygenLevel = data.get("oxygenLevel")
            status      = data.get("status")

            # Fallback timestamp if JS did not send one
            if not timestamp:
                timestamp = datetime.now().isoformat()

            # Format log line
            line = f"{timestamp}, Oxygen={oxygenLevel}%, Status={status}\n"

            # Append to log file
            with open(LOG_FILE, "a") as f:
                f.write(line)

            print(f"[LOGGER] Logged: {line.strip()}")

        except json.JSONDecodeError:
            print("[LOGGER] Received invalid JSON!")
        except Exception as e:
            print("[LOGGER] Error:", e)

if __name__ == "__main__":
    main()
