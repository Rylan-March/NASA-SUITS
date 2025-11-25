from multiprocessing import Process
import subprocess

def run(script):
    print(f"Starting {script}...")
    subprocess.run(["python3", script])

if __name__ == "__main__":
    scripts = [
        "Test_Router.py",  # WS → ZMQ bridge
        "logger.py",     # Logging service
        "sub.py"              # Example subscriber
    ]

    processes = []

    for s in scripts:
        p = Process(target=run, args=(s,))
        p.start()                             #where it runs all the scripts as processes
        processes.append(p)

    # Keep everything running
    for p in processes:
        p.join()
