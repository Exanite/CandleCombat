import os
import json
import time
import asyncio
import multiprocessing
import tensorflow as tf
import pandas as pd
import numpy as np
from train import get_prediction

model_id = "232259b"
pipe_name = "ViewPipe1"
num_instances = 1
exe_path = os.path.join("Builds", "UI", "Project.exe")


model_path = os.path.join(os.path.dirname(__file__), "..", "models", f"model_{model_id}.h5")
model =  tf.keras.models.load_model(model_path)

def start_pipe():
    print("starting game")
    os.system(
        f"{exe_path} --instance-count {num_instances} --pipe-name {pipe_name} --respawn-players True")
    print("finished")

def main():

    pipe_path = f"\\\\.\\pipe\\{pipe_name}"

    base_player = {
        "MovementDirection": {
            "x": 0,  # between -1 and 1 [PRESS A/D]
            "y": 0  # between -1 and 1 [PRESS W/S]
        },
        "TargetDirection": {
            "x": 0,  # between -1 and 1 [JOYSTICK X]
            "y": 0  # between -1 and 1 [JOYSTICK Y]
        },
        "IsBurningShotPressed": True,
        "IsSoulTransferPressed": True,
        "IsDodgePressed": True,
        "IsShootPressed": True,
        "IsReloadPressed": True,
    }

    count = 0
    times_run = 1000000
    f = open(pipe_path, "r+")

    while True:
        line = f.readline()
        player_arr = json.loads(line)
        if len(player_arr) == 0:
            break

        output = []

        for player in player_arr:
            tmp = base_player.copy()
            prediction = asyncio.run(get_prediction(model, player))
            prediction = prediction[0]

            tmp["MovementDirection"]["x"] = float(prediction[0])
            tmp["MovementDirection"]["y"] = float(prediction[1])
            tmp["TargetDirection"]["x"] = float(prediction[2])
            tmp["TargetDirection"]["y"] = float(prediction[3])
            tmp["IsBurningShotPressed"] = float(prediction[4]) > 0.5
            tmp["IsSoulTransferPressed"] = float(prediction[5]) > 0.5
            tmp["IsDodgePressed"] = float(prediction[6]) > 0.5
            tmp["IsShootPressed"] = float(prediction[7]) > 0.5
            tmp["IsReloadPressed"] = float(prediction[8]) > 0.5

            output.append(tmp)

        if count == times_run:
            break

        out = json.dumps(output).replace(" ", "") + "\n"
        f.write(out)
        f.flush()
        count += 1

    f.close()


if __name__ == "__main__":
    process = multiprocessing.Process(target=start_pipe)
    process.start()
    time.sleep(10)  # 10 seconds to start the game
    main()
    process.join()