import tensorflow as tf

class NeuralNetwork:
    def __init__(self, input_size, output_size, hidden_layers, learning_rate):
        self.input_size = input_size
        self.output_size = output_size
        self.hidden_layers = hidden_layers
        self.learning_rate = learning_rate

        self._build_model()

    def _build_model(self):
        self.model = tf.keras.Sequential()
        self.model.add(tf.keras.layers.Dense(self.input_size, input_dim=self.input_size, activation='relu'))
        for layer in self.hidden_layers:
            self.model.add(tf.keras.layers.Dense(layer, activation='relu'))
        self.model.add(tf.keras.layers.Dense(self.output_size, activation='linear'))
        self.model.compile(loss='mse', optimizer=tf.keras.optimizers.Adam(lr=self.learning_rate))

    def predict(self, inputs):
        return self.model.predict(inputs)

    def train(self, inputs, targets):
        self.model.fit(inputs, targets, epochs=1, verbose=0)