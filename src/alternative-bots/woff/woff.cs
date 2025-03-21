using System;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

// ------------------------------------------------------------------
// woff 🐶
// ------------------------------------------------------------------
// Targeting: Play It Forward
// Movement: Minimum Risk
// ------------------------------------------------------------------
/*

🐕🐕🐕

*/
// ------------------------------------------------------------------
public class Woff : Bot
{
    // Knobs
    private readonly static double  ENEMY_ENERGY_THRESHOLD = 1.3;
    private readonly static double  MOVE_WALL_MARGIN = 25;
    private readonly static double  GUN_FACTOR = 10;
    private readonly static double  MIN_ENERGY = 10;
    private readonly static double  RADAR_LOCK = 0.7;
    private readonly static double  MIN_RADIUS = 200;
    private readonly static double  MAX_RADIUS = 300;
    private readonly static double  POINT_COUNT = 36;
    private readonly static int     MAX_DATA = 100;

    // Global variables
    static int targetId;
    static double targetDistance;
    static double enemyDistance;

    static double destX;
    static double destY;

    Random rand = new Random();

    static Dictionary<int, EnemyData> enemyData = new Dictionary<int, EnemyData>();

    static List<Bullet> bullets;

    static void Main()
    {
        new Woff().Start();
    }

    Woff() : base(BotInfo.FromFile("woff.json")) { }

    public override void Run()
    {
        RadarColor = Color.White;

        SetTurnRadarRight(double.PositiveInfinity);
        AdjustGunForBodyTurn = true;
        AdjustRadarForGunTurn = true;
        AdjustRadarForBodyTurn = true;

        targetDistance = double.PositiveInfinity;
        enemyDistance = double.PositiveInfinity;
        bullets = new List<Bullet>();
    }

    public override void OnTick(TickEvent e)
    {
        TurretColor = Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256));
        BulletColor = Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256));
        ScanColor = Color.FromArgb(105, 105, rand.Next(256));
        BodyColor = ScanColor;

        for (int i = bullets.Count - 1; i >= 0; i--)
        {
            Bullet bullet = bullets[i];
            bullet.X += bullet.Speed * Math.Cos(bullet.Direction);
            bullet.Y += bullet.Speed * Math.Sin(bullet.Direction);
            // Console.WriteLine("BulletId: " + i + " X: " + bullet.X + " Y: " + bullet.Y);

            if (bullet.X < 0 - MOVE_WALL_MARGIN * 2 || bullet.X > ArenaWidth + MOVE_WALL_MARGIN * 2 || 
                bullet.Y < 0 - MOVE_WALL_MARGIN * 2 || bullet.Y > ArenaHeight + MOVE_WALL_MARGIN * 2)
            {
                bullets.RemoveAt(i);
            }
            else 
            {
                bullets[i] = bullet;
            }
        }

        // Minimum Risk Movement
        double bestX = X;
        double bestY = Y;
        double minRisk = double.PositiveInfinity;

        for (int i = 0; i < POINT_COUNT; i++)
        {
            double theta = (2 * Math.PI / POINT_COUNT) * i;
            
            for (int u = 0; u <= 1; u++) {
                double r = Math.Sqrt(u * (MAX_RADIUS * MAX_RADIUS - MIN_RADIUS * MIN_RADIUS) + MIN_RADIUS * MIN_RADIUS);
                
                double x = X + r * Math.Cos(theta);
                double y = Y + r * Math.Sin(theta);

                if (x < MOVE_WALL_MARGIN || x > ArenaWidth - MOVE_WALL_MARGIN ||
                    y < MOVE_WALL_MARGIN || y > ArenaHeight - MOVE_WALL_MARGIN)
                {
                    continue;
                }

                double risk = CalcRisk(x, y);
                if (risk < minRisk)
                {
                    minRisk = risk;
                    bestX = x;
                    bestY = y;
                }
            }
        }

        if (minRisk < CalcRisk(destX, destY) * 0.9)
        {
            destX = bestX;
            destY = bestY;
        }

        double turn = BearingTo(destX, destY) * Math.PI / 180;
        SetTurnLeft(180 / Math.PI * Math.Tan(turn));
        SetForward(DistanceTo(destX, destY) * Math.Cos(turn));
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        // Update enemy data
        if (!enemyData.ContainsKey(e.ScannedBotId))
        {
            enemyData[e.ScannedBotId] = new EnemyData();
        }
        EnemyData data = enemyData[e.ScannedBotId];
        data.LastX = e.X;
        data.LastY = e.Y;
        data.IsAlive = true;

        // Lock closest target
        double scannedDistance = enemyDistance = DistanceTo(e.X, e.Y);
        if (scannedDistance < targetDistance)
        {
            targetId = e.ScannedBotId;
        }
        else if (e.ScannedBotId != targetId && GunHeat != 0)
        {
            return;
        }
        targetDistance = scannedDistance;

        // Radar 
        double radarAngle = double.PositiveInfinity * NormalizeRelativeAngle(RadarBearingTo(e.X, e.Y));
        if (!double.IsNaN(radarAngle) && (GunHeat < RADAR_LOCK || EnemyCount == 1))
        {
            SetTurnRadarLeft(radarAngle);
        }

        // Fire control
        double firePower = Energy / DistanceTo(e.X, e.Y) * GUN_FACTOR;
        if (GunTurnRemaining == 0 && (Energy > MIN_ENERGY || DistanceTo(e.X, e.Y) < 50))
        {
            SetFire(firePower);
        }

        double bulletSpeed = CalcBulletSpeed(firePower);
        double currentDirection = e.Direction * Math.PI / 180.0;

        // Input Virtual Bullets
        double energyDrop = data.LastEnergy - e.Energy;
        if (0.11 < energyDrop && energyDrop <= 3)
        {
            AddVirtualBullet(e.X, e.Y, CalcBulletSpeed(energyDrop), energyDrop);
            // Console.WriteLine("Bullet Speed: " + CalcBulletSpeed(energyDrop) + " Power: " + energyDrop);
        }
        data.LastEnergy = e.Energy;

        // Input Markov Chain
        double currentSpeed = e.Speed;
        double acceleration = 0;
        if (data.HasPrevious)
        {
            acceleration = currentSpeed - data.LastSpeed;
        }
        data.LastSpeed = currentSpeed;
        
        double angularVelocity = 0;
        if (data.HasPrevious)
        {
            angularVelocity = (currentDirection - data.LastDirection + Math.PI) % (2 * Math.PI) - Math.PI;
        }
        data.LastDirection = currentDirection;
        data.HasPrevious = true;

        State currentState = new State(angularVelocity, currentSpeed, acceleration);
        data.StateHistory.Add(currentState);
        if (data.StateHistory.Count > MAX_DATA)
        {
            data.StateHistory.RemoveAt(0);
        }
        if (data.StateHistory.Count >= 2)
        {
            State previousState = data.StateHistory[data.StateHistory.Count - 2];
            if (!data.MarkovChain.ContainsKey(previousState))
            {
                data.MarkovChain[previousState] = new List<State>();
            }
            data.MarkovChain[previousState].Add(currentState);
        }

        // --- Play It Forward ---
        double predictedX = e.X;
        double predictedY = e.Y;
        double predictedDirection = currentDirection;
        double predictedSpeed = currentSpeed;
        double simAngularVelocity = angularVelocity;
        State simCurrentState = currentState;
        int time = 0;
        while (time * bulletSpeed < DistanceTo(predictedX, predictedY))
        {
            if (data.MarkovChain.ContainsKey(simCurrentState) && data.MarkovChain[simCurrentState].Count > 0)
            {
                State nextState = GetMostFrequentTransition(data.MarkovChain[simCurrentState]);
                simAngularVelocity = nextState.AngularVelocity / 1000.0;
                predictedSpeed += nextState.Acceleration;
                simCurrentState = nextState;
            }
            predictedDirection += simAngularVelocity;
            predictedX += predictedSpeed * Math.Cos(predictedDirection);
            predictedY += predictedSpeed * Math.Sin(predictedDirection);
            time++;
        }

        // Bullet's Wall Avoidance
        predictedX = Math.Max(MOVE_WALL_MARGIN, Math.Min(ArenaWidth - MOVE_WALL_MARGIN, predictedX));
        predictedY = Math.Max(MOVE_WALL_MARGIN, Math.Min(ArenaHeight - MOVE_WALL_MARGIN, predictedY));

        double bearingFromGun = GunBearingTo(predictedX, predictedY);
        SetTurnGunLeft(bearingFromGun);
    }

    public override void OnBotDeath(BotDeathEvent e)
    {
        enemyData[e.VictimId].IsAlive = false;
        
        if (e.VictimId == targetId)
        {
            targetDistance = double.PositiveInfinity;
        }
    }

    public override void OnBulletFired(BulletFiredEvent e)
    {
        // Console.WriteLine("BulletId: " + e.Bullet.BulletId + " X: " + e.Bullet.X + " Y: " + e.Bullet.Y);
        // bullets[e.Bullet.BulletId] = new Bullet
        // {
        //     X = e.Bullet.X,
        //     Y = e.Bullet.Y,
        //     Speed = CalcBulletSpeed(e.Bullet.Power),
        //     Direction = e.Bullet.Direction * Math.PI / 180
        // };
    }

    public override void OnBulletHit(BulletHitBotEvent e)
    {
        // Console.WriteLine("Bullet Hit Bullet " + e.Bullet.BulletId + " Owner: " + e.Bullet.OwnerId);
        // if (e.Bullet.OwnerId == MyId)
        // {
        //     bullets.Remove(e.Bullet.BulletId);
        // }
    }

    public override void OnBulletHitBullet(BulletHitBulletEvent e)
    {
        // Console.WriteLine("Bullet Hit Bullet " + e.Bullet.BulletId + " Owner: " + e.Bullet.OwnerId);
        // if (e.Bullet.OwnerId == MyId)
        // {
        //     bullets.Remove(e.Bullet.BulletId);
        // }
    }

    public override void OnBulletHitWall(BulletHitWallEvent e)
    {
        // Console.WriteLine("Bullet Hit Wall " + e.Bullet.BulletId + " Owner: " + e.Bullet.OwnerId);
        // if (e.Bullet.OwnerId == MyId)
        // {
        //     bullets.Remove(e.Bullet.BulletId);
        // }
    }

    public override void OnHitByBullet(HitByBulletEvent e)
    {
        // Console.WriteLine("Hit by bullet " + e.Bullet.BulletId + " Owner: " + e.Bullet.OwnerId);
        // if (e.Bullet.OwnerId == MyId)
        // {
        //     bullets.Remove(e.Bullet.BulletId);
        // }
    }

    // --- Helper Functions ---
    private State GetMostFrequentTransition(List<State> transitions)
    {
        List<State> transitionsCopy = new List<State>(transitions);
        
        Dictionary<State, int> frequency = new Dictionary<State, int>();
        foreach (State state in transitionsCopy)
        {
            if (frequency.ContainsKey(state))
                frequency[state]++;
            else
                frequency[state] = 1;
        }
        State bestState = transitionsCopy[0];
        int bestCount = 0;
        foreach (var kvp in frequency)
        {
            if (kvp.Value > bestCount)
            {
                bestCount = kvp.Value;
                bestState = kvp.Key;
            }
        }
        return bestState;
    }

    private double CalcRisk(double candidateX, double candidateY)
    {
        double risk = 0;

        foreach (EnemyData enemy in enemyData.Values)
        {
            if (enemy.IsAlive)
            {
                risk += 300 * (enemy.LastEnergy - ENEMY_ENERGY_THRESHOLD) / (distanceSq(candidateX, candidateY, enemy.LastX, enemy.LastY) + 1e-6);
            }
        }

        risk += 10 * rand.NextDouble() / (Math.Pow(DistanceTo(candidateX, candidateY), 2) + 1e-6);
        // risk += rand.NextDouble() / (2 * distanceSq(ArenaWidth / 2, ArenaHeight / 2, candidateX, candidateY) + 1e-6);

        foreach (Bullet bullet in bullets)
        {
            Line2D bulletLine = new Line2D(
                bullet.X - Math.Cos(bullet.Direction) * 10000, 
                bullet.Y - Math.Sin(bullet.Direction) * 10000, 
                bullet.X + Math.Cos(bullet.Direction) * 10000, 
                bullet.Y + Math.Sin(bullet.Direction) * 10000
            );
            
            double d = bulletLine.DistanceToPoint(candidateX, candidateY);
            risk += 10 * bullet.Power / (d * d + 1e-6);
        }

        return risk;
    }
    
    private void AddVirtualBullet(double x, double y, double speed, double power)
    {
        // Head on
        double headOnDirection = (180 + DirectionTo(x, y)) * Math.PI / 180;
        Bullet bullet = new Bullet
        {
            Speed = speed,
            Direction = headOnDirection,
            X = x,
            Y = y,
            Power = power
        };
        bullets.Add(bullet);
        
        // Linear
        double linearX = x - speed * Math.Cos(Direction);
        double linearY = y - speed * Math.Sin(Direction);
        double linearDirection = 180 + DirectionTo(linearX, linearY);
        Bullet bulletLinear = new Bullet
        {
            Speed = speed,
            Direction = linearDirection,
            X = x,
            Y = y,
            Power = power * 2
        };
        bullets.Add(bulletLinear);
    }
    
    private double distanceSq(double x1, double y1, double x2, double y2)
    {
        return Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2);
    }
}

public struct State
{
    public int AngularVelocity; // quantized: radian * 1000
    public int Speed; // -8 -- 8
    public int Acceleration; // -1 -- 1

    public State(double angularVelocity, double speed, double acceleration)
    {
        AngularVelocity = (int)(angularVelocity * 1000);

        Speed = (int)Math.Round(speed);
        
        double threshold = 0.1; 
        if (acceleration < -threshold)
            Acceleration = -1;
        else if (acceleration > threshold)
            Acceleration = 1;
        else
            Acceleration = 0;
    }

    public override bool Equals(object obj)
    {
        if (obj is State state)
        {
            return state.AngularVelocity == AngularVelocity &&
                   state.Speed == Speed &&
                   state.Acceleration == Acceleration;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return AngularVelocity.GetHashCode() ^ Speed.GetHashCode() ^ Acceleration.GetHashCode();
    }
}

public class EnemyData
{
    public List<State> StateHistory { get; } = new List<State>();
    public Dictionary<State, List<State>> MarkovChain { get; } = new Dictionary<State, List<State>>();
    public double LastDirection { get; set; }
    public bool HasPrevious { get; set; } = false;

    public double LastX { get; set; }
    public double LastY { get; set; }
    public double LastEnergy { get; set; }
    public double LastSpeed { get; set; }
    public bool IsAlive { get; set; } = true;
}

public struct Bullet
{
    public double X;
    public double Y;
    public double Speed;
    public double Direction;
    public double Power;
}

public class Line2D
{
    public double X1 { get; }
    public double Y1 { get; }
    public double X2 { get; }
    public double Y2 { get; }

    public Line2D(double x1, double y1, double x2, double y2)
    {
        X1 = x1;
        Y1 = y1;
        X2 = x2;
        Y2 = y2;
    }

    public double DistanceToPoint(double px, double py)
    {
        return Math.Abs((Y2 - Y1) * px - (X2 - X1) * py + (X2 * Y1 - Y2 * X1)) 
                / Math.Sqrt(Math.Pow(Y2 - Y1, 2) + Math.Pow(X2 - X1, 2));
    }
}