import os
import math
import random
import json
import time
import multiprocessing
import asyncio
import pandas as pd
import numpy as np
import src.neural_network as nn

pipe_name = "Pipe16"
num_instances = 16
respawn_behaviour = 'waves'
exe_path = os.path.join("Builds", "Server", "Project.exe")


def get_distances_from_player(player, enemy):
    return math.sqrt((player["Position"]["x"] - enemy["OffsetFromPlayer"]["x"])**2 + (player["Position"]["y"] - enemy["OffsetFromPlayer"]["y"])**2)


async def get_prediction(model, player):
    in_arr, time_alive = get_row_from_obj(player)
    prediction = model.predict([list(in_arr.values())])
    return prediction

models = {}


async def get_predictions(players):
    predictions = []
    for player in players:
        predictions.append(get_prediction(models[player['Id']], player))
    predictions = await asyncio.gather(*predictions)
    return predictions


def get_row_from_obj(obj):
    player = obj["Player"]
    reward = player["TimeAlive"]
    input_obj = {}
    input_obj["health"] = player["CurrentHealth"] / player["MaxHealth"]
    # input_obj["position_x"] = player["Position"]["x"]
    # input_obj["position_y"] = player["Position"]["y"]
    input_obj["velocity_x"] = player["Velocity"]["x"]
    input_obj["velocity_y"] = player["Velocity"]["y"]
    input_obj["speed"] = player["MovementSpeed"]
    input_obj["has_burning_shot"] = float(player["BurningShotCooldown"] == 0)
    input_obj["has_soul_transfer"] = float(player["SoulTransferCooldown"] == 0)
    input_obj["has_dodge"] = float(player["DodgeCooldown"] == 0)
    input_obj["has_ammo"] = float(player["CurrentAmmo"] > 0)
    input_obj["ammo"] = player["CurrentAmmo"] / player["MaxAmmo"]
    input_obj["is_reloading"] = float(player["IsReloading"])
    for i in range(len(player['NavigationRaycasts'])):
        cast = player['NavigationRaycasts'][i]
        input_obj[f"cast_{i}"] = cast / player['NavigationRaycastMaxDistance']

    input_obj["has_enemy"] = 0
    input_obj["enemy_distance"] = 0
    input_obj["enemy_offset_x"] = 0
    input_obj["enemy_offset_y"] = 0
    if len(obj["Enemies"]) > 0:
        nearest_enemy = {}
        nearest_enemy_distance = 100000000
        for i in range(len(obj["Enemies"])):
            enemy = obj["Enemies"][i]
            if enemy['CanSeeFromPlayer'] or True:
                input_obj["has_enemy"] = 1
                enemy_dist = get_distances_from_player(player, enemy)

                if enemy_dist < nearest_enemy_distance:
                    nearest_enemy = enemy
                    nearest_enemy_distance = enemy_dist

        if nearest_enemy != {}:
            input_obj["enemy_distance"] = 8.0
            input_obj["enemy_offset_x"] = nearest_enemy["OffsetFromPlayer"]["x"]
            input_obj["enemy_offset_y"] = nearest_enemy["OffsetFromPlayer"]["y"]

    return input_obj, reward


def start_pipe():
    print("starting game")
    os.system(
        f"{exe_path} --instance-count {num_instances} --pipe-name {pipe_name} --respawn-behavior {respawn_behaviour}")
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
    gen_count = 0
    times_run = 100000
    max_results = -1
    count_last_max = 0
    f = open(pipe_path, "r+")
    results = {}
    history = []

    while count < times_run:
        line = f.readline()
        player_data = json.loads(line)
        player_arr = player_data["GameOutputs"]
        if len(player_arr) == 0:
            gen_count += 1
            print(f"\n\n--- New Generation {gen_count} ---\n\n")

        for i in range(len(player_arr)):
            if not player_arr[i]["Id"] in models:
                player = player_arr[i]
                if len(models) < num_instances:
                    print(f"Initializing model for player {player['Id']}")
                    models[player['Id']] = nn.NeuralNetwork(
                        22, [256], 9, 0.3, init_std=5.0)
                    models[player['Id']]._build_model()
                    results[player['Id']] = 0.0
                else:
                    max_id = max(results, key=results.get)
                    max_results = results[max_id]
                    del results[max_id]
                    second_max_id = max(results, key=results.get)
                    second_max_results = results[second_max_id]
                    results[max_id] = max_results

                    # save best model
                    file_name = f"model_{max_id[0:7]}.h5"
                    models[max_id].save(os.path.join("models", file_name))

                    file_name = f"model_{second_max_id[0:7]}.h5"
                    models[second_max_id].save(
                        os.path.join("models", file_name))

                    second_best = models[second_max_id]
                    print(
                        f"Generating new model for player {player['Id']} from players: \n\tbest={max_id} ({max_results})\n\tsecond_best={second_max_id} ({second_max_results}) \nSaved Best: {file_name}\n{'-'*16}")
                    models[player['Id']] = models[max_id].new_generation(
                        second_best=second_best)
                    results[player['Id']] = 0.0
                    history.append(results[max_id])
            results[player_arr[i]['Id']] = player_arr[i]["Player"]["TimeAlive"]

        output = []
        predictions = asyncio.run(get_predictions(player_arr))

        for pred in predictions:
            tmp = base_player.copy()
            prediction = pred[0]

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

        timed_runs = list(results.values())
        tmp_max_results = max(timed_runs)
        if tmp_max_results > max_results:
            max_results = tmp_max_results
            print(
                f"New max results: {max_results} took {count - count_last_max} runs")
            count_last_max = count

        out = {}
        out["GameInputs"] = output
        out["NewGamesToStart"] = 0

        out = json.dumps(out).replace(" ", "") + "\n"
        # print(f"--- OUTPUT {count} ---")
        # print(out)
        f.write(out)
        f.flush()
        count += 1

    f.close()


if __name__ == "__main__":
    process = multiprocessing.Process(target=start_pipe)
    process.start()
    time.sleep(10)
    main()
    process.join()
