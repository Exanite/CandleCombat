import net from "net";
import readline from "readline";

interface Vector2 {
  x: number;
  y: number;
}

interface MlGameOutput {
  Id: string;

  Player: {
    TimeAlive: number;

    CurrentHealth: number;
    MaxHealth: number;

    Position: Vector2;
    Velocity: Vector2;

    BurningShotCooldown: number;
    SoulTransferCooldown: number;
    DodgeCooldown: number;

    CurrentAmmo: number;
    MaxAmmo: number;
    IsReloading: boolean;
  };

  Enemies: {
    OffsetFromPlayer: Vector2;
    CanSeeFromPlayer: boolean;
  }[];
}

interface MlGameInput {
  MovementDirection: Vector2;
  TargetDirection: Vector2;
  IsBurningShotPressed: boolean;
  IsSoulTransferPressed: boolean;
  IsDodgePressed: boolean;
  IsShootPressed: boolean;
  IsReloadPressed: boolean;
}

const run = async (): Promise<void> => {
  const socket = net.connect("\\\\.\\pipe\\CandleCombatMachineLearning", () => {
    console.log("Connected");
  });

  const lineReader = readline.createInterface({
    input: socket,
    crlfDelay: Infinity,
  });

  for await (const inputJson of lineReader) {
    console.log("Received data");
    console.log(inputJson);
    const outputs = JSON.parse(inputJson) as MlGameOutput[];
    const inputs: MlGameInput[] = [];

    for (const output of outputs) {
      inputs.push({
        MovementDirection: {
          x: (Math.random() - 0.5) * 2,
          y: (Math.random() - 0.5) * 2,
        },
        TargetDirection: {
          x: (Math.random() - 0.5) * 2,
          y: (Math.random() - 0.5) * 2,
        },
        IsBurningShotPressed: true,
        IsSoulTransferPressed: true,
        IsDodgePressed: true,
        IsShootPressed: true,
        IsReloadPressed: true,
      });
    }

    const outputJson = JSON.stringify(inputs);

    console.log("Sending");
    console.log(outputJson);

    socket.write(outputJson);
    socket.write("\n");
  }
};

run().catch((e) => {
  console.error(`An error occurred: ${e}`);
});
