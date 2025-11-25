FROM python:3.11-slim
WORKDIR /home

COPY . .

RUN pip install websockets zmq

CMD ["python3", "run.py"]
