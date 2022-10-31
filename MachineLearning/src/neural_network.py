import tensorflow as tf
import numpy as np
import random


def is_number(val):
    if type(val) == type(0.0) \
            or type(val) == type(0) \
            or type(val) == np.float32 \
            or type(val) == np.float64 \
            or type(val) == np.int32 \
            or type(val) == np.int64:
        return True
    return False


def print_arr_shape(arr, name="test", i=0):
    while not is_number(arr):
        tabs = "\t" * i
        print(f"{tabs}len({name}) = {len(arr)}")
        for j in range(len(arr)):
            print_arr_shape(arr[j], name=f"{name}[{j}]", i=(i+1))
        break


def get_weights_mutated(first, second, mutation_rate):

    if is_number(first) and is_number(second):
        # randomly switch biases between the two
        rand_num = random.random()
        if rand_num > mutation_rate:
            pct = random.random()
            if pct < 0.5:
                return first
            return second
        else:
            pct = random.random()
            if pct < 0.5:
                return np.random.normal(first, 5.0, 1)[0]
            return np.random.normal(second, 5.0, 1)[0]

    if len(first) != len(second):
        raise Exception("Arrays are not the same length")

    rand_num = random.random()
    if rand_num > mutation_rate:
        pct = random.random()
        if pct <= 0.5:
            return first
        return second
    else:
        # get the random average of the weights from the two parents
        pct = random.random()  # 0 - 1 uniform distribution
        weight = first
        if pct > 0.5:
            weight = second
        mutation = np.random.normal(weight, 5.0)
        return mutation


class NeuralNetwork:
    def __init__(self, input_size, hidden_layers, output_size, mutation_rate, init_std=5.0):
        self.input_size = input_size
        self.hidden_layers = hidden_layers
        self.output_size = output_size
        self.mutation_rate = mutation_rate
        self.init_std = init_std

        self._build_model()

    def _build_model(self):
        initializer = tf.keras.initializers.RandomNormal(stddev=self.init_std)
        # bias_initializer = tf.keras.initializers.RandomNormal(stddev=3.0)

        self.model = tf.keras.Sequential()
        # input layer
        self.model.add(
            tf.keras.layers.Dense(
                self.input_size,
                input_dim=self.input_size,
                activation='tanh', # scale from -1 - 1
                kernel_initializer=initializer
            ))
        # hidden layers
        for layer in self.hidden_layers:
            self.model.add(
                tf.keras.layers.Dense(
                    layer,
                    activation='relu', # max(0, wx + b)
                    kernel_initializer=initializer
                ))

        # output layer
        self.model.add(
            tf.keras.layers.Dense(
                self.output_size,
                activation='tanh', # scale from -1 - 1
                kernel_initializer=initializer
            ))

        self.model.compile(
            loss='mse', optimizer=tf.keras.optimizers.Adam(lr=self.mutation_rate))

    def predict(self, inputs):
        return self.model.predict(inputs)

    def new_generation(self, second_best=None, mutation_rate=None):
        if mutation_rate == None:
            mutation_rate = self.mutation_rate
        # get the generation of the current model
        weights_best = self.model.get_weights()
        weights_second_best = second_best.model.get_weights()
        output_weights = []

        # print_arr_shape(weights_best, name='weights_best')

        # for each layer
        for k in range(len(weights_best)):
            node = weights_best[k]
            out = []
            # for each node
            for i in range(len(node)):
                new_weights = get_weights_mutated(
                    weights_best[k][i], weights_second_best[k][i], self.mutation_rate)
                out.append(new_weights)
            output_weights.append(out)
        # print_arr_shape(output_weights, name='output_weights')
        output = NeuralNetwork(
            self.input_size, self.hidden_layers, self.output_size, self.mutation_rate)
        output.model.set_weights([np.array(a) for a in output_weights])
        output.model.compile(
            loss='mse', optimizer=tf.keras.optimizers.Adam(lr=self.mutation_rate))
        return output

    def save(self, path):
        self.model.save(path)

    def load(self, path):
        self.model = tf.keras.models.load_model(path)
