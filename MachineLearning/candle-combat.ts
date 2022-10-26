import net from "net";
import readline from "readline";

interface Vector2 {
  x: number;
  y: number;
}

interface MlEnemyData {
  OffsetFromPlayer: Vector2;
  CanSeeFromPlayer: boolean;
}

interface MlProjectileData {
  OffsetFromPlayer: Vector2;
  IsOwnedByPlayer: boolean;
}

interface MlGameOutput {
  Id: string;

  Player: {
    TimeAlive: number;

    CurrentHealth: number;
    MaxHealth: number;

    Position: Vector2;
    Velocity: Vector2;
    MovementSpeed: number;

    BurningShotCooldown: number;
    SoulTransferCooldown: number;
    DodgeCooldown: number;

    BurningShotCooldownDuration: number;
    SoulTransferCooldownDuration: number;
    DodgeCooldownDuration: number;

    CurrentAmmo: number;
    MaxAmmo: number;
    IsReloading: boolean;

    NavigationRaycasts: [number, number, number, number, number, number, number, number];
    NavigationRaycastMaxDistance: number;
  };

  Enemies: MlEnemyData[];

  Projectiles: MlProjectileData[];
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

interface MlGameStartedEvent {
  Id: string;
}

interface MlGameClosedEvent {
  Id: string;
  TimeAlive: number;
}

interface MlOutput {
  GameOutputs: MlGameOutput[];

  StartedGames: MlGameStartedEvent[];
  ClosedGames: MlGameClosedEvent[];
}

interface MlInput {
  GameInputs: MlGameInput[];
}

// From http://asserttrue.blogspot.com/2011/12/perlin-noise-in-javascript_31.html
class PerlinNoise {
  public noise(x: number, y: number, z: number): number {
    const p = new Array(512);
    const permutation = [151, 160, 137, 91, 90, 15,
      131, 13, 201, 95, 96, 53, 194, 233, 7, 225, 140, 36, 103, 30, 69, 142, 8, 99, 37, 240, 21, 10, 23,
      190, 6, 148, 247, 120, 234, 75, 0, 26, 197, 62, 94, 252, 219, 203, 117, 35, 11, 32, 57, 177, 33,
      88, 237, 149, 56, 87, 174, 20, 125, 136, 171, 168, 68, 175, 74, 165, 71, 134, 139, 48, 27, 166,
      77, 146, 158, 231, 83, 111, 229, 122, 60, 211, 133, 230, 220, 105, 92, 41, 55, 46, 245, 40, 244,
      102, 143, 54, 65, 25, 63, 161, 1, 216, 80, 73, 209, 76, 132, 187, 208, 89, 18, 169, 200, 196,
      135, 130, 116, 188, 159, 86, 164, 100, 109, 198, 173, 186, 3, 64, 52, 217, 226, 250, 124, 123,
      5, 202, 38, 147, 118, 126, 255, 82, 85, 212, 207, 206, 59, 227, 47, 16, 58, 17, 182, 189, 28, 42,
      223, 183, 170, 213, 119, 248, 152, 2, 44, 154, 163, 70, 221, 153, 101, 155, 167, 43, 172, 9,
      129, 22, 39, 253, 19, 98, 108, 110, 79, 113, 224, 232, 178, 185, 112, 104, 218, 246, 97, 228,
      251, 34, 242, 193, 238, 210, 144, 12, 191, 179, 162, 241, 81, 51, 145, 235, 249, 14, 239, 107,
      49, 192, 214, 31, 181, 199, 106, 157, 184, 84, 204, 176, 115, 121, 50, 45, 127, 4, 150, 254,
      138, 236, 205, 93, 222, 114, 67, 29, 24, 72, 243, 141, 128, 195, 78, 66, 215, 61, 156, 180,
    ];

    for (let i = 0; i < 256; i++) {
      p[256 + i] = p[i] = permutation[i];
    }

    const X = Math.floor(x) & 255;
    const Y = Math.floor(y) & 255;
    const Z = Math.floor(z) & 255;

    x -= Math.floor(x);
    y -= Math.floor(y);
    z -= Math.floor(z);

    const u = this.fade(x);
    const v = this.fade(y);
    const w = this.fade(z);

    const A = p[X] + Y, AA = p[A] + Z, AB = p[A + 1] + Z,
      B = p[X + 1] + Y, BA = p[B] + Z, BB = p[B + 1] + Z;

    return this.scale(
      this.lerp(w,
        this.lerp(v,
          this.lerp(u,
            this.grad(p[AA], x, y, z),
            this.grad(p[BA], x - 1, y, z)),
          this.lerp(u,
            this.grad(p[AB], x, y - 1, z),
            this.grad(p[BB], x - 1, y - 1, z))),
        this.lerp(v,
          this.lerp(u,
            this.grad(p[AA + 1], x, y, z - 1),
            this.grad(p[BA + 1], x - 1, y, z - 1)),
          this.lerp(u,
            this.grad(p[AB + 1], x, y - 1, z - 1),
            this.grad(p[BB + 1], x - 1, y - 1, z - 1)))));
  }

  public fade(t: number): number {
    return t * t * t * (t * (t * 6 - 15) + 10);
  }

  public lerp(t: number, a: number, b: number): number {
    return a + t * (b - a);
  }

  public grad(hash: number, x: number, y: number, z: number): number {
    const h = hash & 15;
    const u = h < 8 ? x : y,
      v = h < 4 ? y : h == 12 || h == 14 ? x : z;
    return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
  }

  public scale(n: number): number {
    return (1 + n) / 2;
  }
}

const perlin = new PerlinNoise();

const getMagnitude = (value: Vector2): number => {
  return Math.sqrt(value.x * value.x + value.y * value.y);
};

const run = async (): Promise<void> => {
  const socket = net.connect("\\\\.\\pipe\\CandleCombatMachineLearning", () => {
    console.log("Connected");
  });

  const lineReader = readline.createInterface({
    input: socket,
    crlfDelay: Infinity,
  });

  for await (const mlOutputJson of lineReader) {
    // console.log("Received data");
    // console.log(mlOutputJson);
    const mlOutput = JSON.parse(mlOutputJson) as MlOutput;
    const gameOutputs = mlOutput.GameOutputs;
    const gameInputs: MlGameInput[] = [];

    for (const startedGame of mlOutput.StartedGames) {
      console.log(`Game started: ${startedGame.Id}`);
    }

    for (const closedGame of mlOutput.ClosedGames) {
      console.log(`Game closed: ${closedGame.Id}`);
      console.log(`Time alive: ${closedGame.TimeAlive}`);
    }

    for (const output of gameOutputs) {
      const input: MlGameInput = {
        MovementDirection: {
          x: 0,
          y: 0,
        },
        TargetDirection: {
          x: 0,
          y: 0,
        },
        IsBurningShotPressed: false,
        IsSoulTransferPressed: false,
        IsDodgePressed: false,
        IsShootPressed: false,
        IsReloadPressed: false,
      };

      gameInputs.push(input);

      let closestEnemy: undefined | MlEnemyData = undefined;
      {
        let closestDistance = Infinity;
        for (let enemy of output.Enemies) {
          if (!enemy.CanSeeFromPlayer || getMagnitude(enemy.OffsetFromPlayer) > 10) {
            continue;
          }

          const distance = getMagnitude(enemy.OffsetFromPlayer);

          if (distance < closestDistance) {
            closestEnemy = enemy;
            closestDistance = distance;
          }
        }
      }

      let closestEnemyProjectile: undefined | MlProjectileData = undefined;
      {
        let closestDistance = Infinity;
        for (let projectile of output.Projectiles) {
          if (projectile.IsOwnedByPlayer) {
            continue;
          }

          const distance = getMagnitude(projectile.OffsetFromPlayer);

          if (distance < closestDistance) {
            closestEnemyProjectile = projectile;
            closestDistance = distance;
          }
        }
      }

      input.MovementDirection.x += Math.cos((perlin.noise(output.Player.TimeAlive, 0, 0) + 1) * Math.PI * 2);
      input.MovementDirection.y += Math.sin((perlin.noise(output.Player.TimeAlive, 0, 0) + 1) * Math.PI * 2);

      for (let i = 0; i < output.Player.NavigationRaycasts.length; i++) {
        const navigationRaycast = output.Player.NavigationRaycasts[i];
        const angle = (Math.PI / 2) - i * ((2 * Math.PI) / output.Player.NavigationRaycasts.length);

        if (navigationRaycast < 5) {
          // 0 to 1
          const weight = (5 - navigationRaycast) / 5;
          const exponentialWeight = weight * weight;

          input.MovementDirection.x -= exponentialWeight * 4 * Math.cos(angle);
          input.MovementDirection.y -= exponentialWeight * 4 * Math.sin(angle);
        }
      }

      if (closestEnemy) {
        input.IsShootPressed = true;
        input.TargetDirection = {
          x: closestEnemy.OffsetFromPlayer.x,
          y: closestEnemy.OffsetFromPlayer.y,
        };
        input.MovementDirection.x -= closestEnemy.OffsetFromPlayer.x;
        input.MovementDirection.y -= closestEnemy.OffsetFromPlayer.y;

        if ((output.Player.CurrentHealth / output.Player.MaxHealth) < 0.4) {
          if (getMagnitude(closestEnemy.OffsetFromPlayer) > 2 && output.Player.SoulTransferCooldown < 0.1) {
            input.MovementDirection.x = closestEnemy.OffsetFromPlayer.x;
            input.MovementDirection.y = closestEnemy.OffsetFromPlayer.y;
          }

          input.IsDodgePressed = Math.random() > 0.5;
          input.IsShootPressed = false;
        }

        if (getMagnitude(closestEnemy.OffsetFromPlayer) < 2) {
          input.IsSoulTransferPressed = Math.random() > 0.5;
        }
        
        if (getMagnitude(closestEnemy.OffsetFromPlayer) < 5 && (output.Player.CurrentHealth / output.Player.MaxHealth) > 0.9) {
          input.IsBurningShotPressed = Math.random() > 0.5;
        }
      } else {
        input.TargetDirection = {
          x: Math.cos(output.Player.TimeAlive * 2 * Math.PI * 5),
          y: Math.sin(output.Player.TimeAlive * 2 * Math.PI * 5),
        };
      }

      if (closestEnemyProjectile) {
        const projectileDistance = getMagnitude(closestEnemyProjectile.OffsetFromPlayer);
        const normalizedOffsetFromPlayer: Vector2 = {
          x: closestEnemyProjectile.OffsetFromPlayer.x / projectileDistance,
          y: closestEnemyProjectile.OffsetFromPlayer.y / projectileDistance,
        };
        
        if (projectileDistance < 5) {
          // Perpendicular
          input.MovementDirection.x = -normalizedOffsetFromPlayer.y;
          input.MovementDirection.y = normalizedOffsetFromPlayer.x;

          if (projectileDistance < 2.5 && output.Player.DodgeCooldown < 0.1) {
            input.MovementDirection = {
              // Perpendicular
              x: -closestEnemyProjectile.OffsetFromPlayer.y,
              y: closestEnemyProjectile.OffsetFromPlayer.x,
            };

            input.IsDodgePressed = Math.random() > 0.5;
          }
        }
      }
    }

    const mlInput: MlInput = {
      GameInputs: gameInputs,
    }
    
    const mlInputJson = JSON.stringify(mlInput);

    // console.log("Sending");
    // console.log(mlInputJson);

    socket.write(mlInputJson);
    socket.write("\n");
  }
};

run().catch((e) => {
  console.error(`An error occurred: ${e}`);
});
