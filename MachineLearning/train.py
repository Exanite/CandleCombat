import os
import math
import random
import json
import time
import multiprocessing
import pandas as pd
import numpy as np
# import src.neural_network as nn

pipe_name = "Pipe1"
num_instances = 1
exe_path = os.path.join("Builds", "UI", "Project.exe")


def get_distances_from_player(player, enemy):
    return math.sqrt((player["Position"]["x"] - enemy["OffsetFromPlayer"]["x"])**2 + (player["Position"]["y"] - enemy["OffsetFromPlayer"]["y"])**2)


def get_row_from_obj(obj):
    player = obj["Player"]
    reward = player["TimeAlive"]
    input_obj = {}
    input_obj["health"] = player["CurrentHealth"] / player["MaxHealth"]
    input_obj["position_x"] = player["Position"]["x"]
    input_obj["position_y"] = player["Position"]["y"]
    input_obj["velocity_x"] = player["Velocity"]["x"]
    input_obj["velocity_y"] = player["Velocity"]["y"]
    input_obj["speed"] = player["MovementSpeed"]
    input_obj["has_burning_shot"] = player["BurningShotCooldown"] == 0
    input_obj["has_soul_transfer"] = player["SoulTransferCooldown"] == 0
    input_obj["has_dodge"] = player["DodgeCooldown"] == 0
    input_obj["has_ammo"] = player["CurrentAmmo"] > 0
    input_obj["ammo"] = player["CurrentAmmo"] / player["MaxAmmo"]
    input_obj["is_reloading"] = player["IsReloading"]
    for i in range(len(player['NavigationRaycasts'])):
        cast = player['NavigationRaycasts'][i]
        input_obj[f"cast_{i}"] = cast / player['NavigationRaycastMaxDistance']

    input_obj["has_enemy"] = 0
    input_obj["enemy_distance"] = 0
    input_obj["enemy_offset_x"] = 0
    input_obj["enemy_offset_y"] = 0
    if len(obj["Enemies"]) > 0:
        nearest_enemy = {}
        nearest_enemy_distance = np.finfo(np.float64).max
        for i in range(len(obj["Enemies"])):
            enemy = obj["Enemies"][i]
            print("enemy", enemy)
            if enemy['CanSeeFromPlayer'] or True:
                input_obj["has_enemy"] = 1
                enemy_dist = get_distances_from_player(player, enemy)

                if enemy_dist < nearest_enemy_distance:
                    nearest_enemy = enemy
                    nearest_enemy_distance = enemy_dist

        if nearest_enemy != {}:
            input_obj["enemy_distance"] = nearest_enemy_distance
            input_obj["enemy_offset_x"] = nearest_enemy["OffsetFromPlayer"]["x"]
            input_obj["enemy_offset_y"] = nearest_enemy["OffsetFromPlayer"]["y"]

    return input_obj, reward


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

        inputs = []
        rewards = []

        for i in range(len(player_arr)):
            player = player_arr[i]
            input_obj, reward = get_row_from_obj(player)
            inputs.append(input_obj)
            rewards.append(reward)

        print(pd.DataFrame(inputs))
        print(f"TIME SURVIVED: {rewards}")

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
    time.sleep(7)
    main()
    process.join()
