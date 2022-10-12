import os
import random
import json
import time
import multiprocessing
import pandas as pd
import numpy as np

pipe_name = "Pipe1"
num_instances = 10
exe_path = os.path.join("Builds", "Server", "Project.exe")


def start_pipe():
    print("starting game")
    os.system(
        f"{exe_path} --instance-count {num_instances} --pipe-name {pipe_name}")
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
    times_run = 1000
    f = open(pipe_path, "r+")

    while True:
        line = f.readline()
        # print(f"--- INPUT {count} ---")
        # print(line)
        player_arr = json.loads(line)
        if len(player_arr) == 0:
            break

        output = []
        sum_time = 0

        for player in player_arr:
            sum_time += player["Player"]["TimeAlive"]
            tmp = base_player.copy()

            tmp["MovementDirection"]["x"] = (random.random() - 0.5) * 2
            tmp["MovementDirection"]["y"] = (random.random() - 0.5) * 2
            tmp["TargetDirection"]["x"] = (random.random() - 0.5) * 2
            tmp["TargetDirection"]["y"] = (random.random() - 0.5) * 2

            output.append(tmp)

        print(f"AVG TIME - {sum_time/len(player_arr)}")

        if count == times_run:
            break

        time.sleep(0.2)

        out = json.dumps(output).replace(" ", "") + "\n"
        # print(f"--- OUTPUT {count} ---")
        # print(out)
        f.write(out)
        f.flush()
        count += 1

    f.close()


if __name__ == "__main__":
    process = multiprocessing.Process(target=start_pipe)
    process.start()
    time.sleep(5)  # 5 seconds to start the game
    main()
    process.join()
