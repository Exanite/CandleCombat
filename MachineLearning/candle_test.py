import os
import random
import json
import pandas as pd
import numpy as np
import errno

pipe_path = "\\\\.\\pipe\\CandleCombatMachineLearning3"

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
f = open(pipe_path, "r+")

while True:
    line = f.readline()
    # print(f"--- INPUT {count} ---")
    # print(line)
    player_arr = json.loads(line)

    output = []
    sum_time = 0

    for player in player_arr:
        sum_time += player["TimeAlive"]
        tmp = base_player.copy()

        tmp["MovementDirection"]["x"] = (random.random() - 0.5) * 2
        tmp["MovementDirection"]["y"] = (random.random() - 0.5) * 2
        tmp["TargetDirection"]["x"] = (random.random() - 0.5) * 2
        tmp["TargetDirection"]["y"] = (random.random() - 0.5) * 2

        output.append(tmp)

    print(f"AVG TIME - {sum_time/len(player_arr)}")

    if count == 400:
        break

    out = json.dumps(output).replace(" ", "") + "\n"
    # print(f"--- OUTPUT {count} ---")
    # f.write(out)
    f.flush()
    print(out)
    count += 1

f.close()
