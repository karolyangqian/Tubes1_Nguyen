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

🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕

⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⣤⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣾⣿⣿⣿⣦⣴⣶⣶⣦⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢻⣿⣿⣿⣿⣿⣿⣿⣿⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣀⣀⣤⣤⣤⣤⣤⣤⣤⣤⣀⣀⣀⠀⠀⠀⠀⢻⣿⣿⣿⣿⣿⡿⠋⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣀⣤⣶⠿⠟⠛⠛⠋⠉⠉⠉⠉⠉⠉⠛⠛⠛⠿⢷⣦⣤⣀⡹⠿⠿⠛⠋⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⣠⣤⣴⣶⣶⣾⠟⠋⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠙⠻⣿⣿⣶⣶⣶⣤⣄⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⣴⣿⠟⠉⠀⠀⠙⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠟⠀⠀⠀⠉⠙⢿⣦⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⣠⣿⡟⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢦⣽⣿⡄⠀⠀⠀⠀⠀
⠀⠀⠀⣰⣿⠏⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠙⣿⣷⠀⠀⠀⠀⠀
⠀⠀⢰⣿⡏⣤⠀⠀⠀⠀⠀⢀⡼⠃⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢰⣻⡀⠀⠀⢤⢠⣼⣿⡆⠀⠀⠀⠀
⠀⠀⠀⢿⣿⠁⠀⠀⠀⠀⣴⡾⠁⠀⠀⠀⢀⣀⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⣠⣀⠀⠀⠀⠀⠀⠈⢻⣇⠀⠀⠈⣇⣿⣿⠀⠀⠀⠀⠀
⠀⠀⠀⢸⣿⠀⡀⣀⠀⢠⣿⠃⠀⠀⢀⣾⣿⣿⡿⠆⠀⠀⠀⠀⠀⠀⠀⠀⠀⣼⣿⣿⣿⡷⠀⠀⠀⠀⠀⢸⣿⠀⢠⣠⣿⣿⠇⠀⠀⠀⠀⠀
⠀⠀⠀⠈⢿⣷⣇⣽⠀⢈⡏⠀⠀⠀⠸⣿⣿⣿⣦⣤⠀⠀⠀⠀⠀⠀⠀⠀⠀⢻⣿⣿⣧⣤⠥⠀⠀⠀⠀⣿⣿⣧⣾⣿⠟⠁⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠈⠛⠿⣿⣧⣾⣿⡄⠀⠀⠀⠙⠿⠿⠿⠃⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠛⠛⠛⠋⠀⠀⠀⠀⠀⢸⣿⡿⠋⠁⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠈⠉⣿⡇⣴⠀⠀⠀⠀⠀⠀⠀⠀⠀⣀⣤⣤⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠐⢶⣼⣿⣀⣠⣤⣤⣤⣀⠀⠀⠀⠀⠀
⠀⠀⣠⣶⣾⠿⠛⠛⠻⢷⣿⣿⠁⠀⠀⠀⠀⠀⠀⠀⠀⣼⣿⣿⣿⣿⡆⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⣿⣿⡿⠋⠉⠉⠉⠛⢿⣦⡀⠀⠀
⢀⣾⡿⠋⠀⠀⠀⠀⠀⠀⠙⣿⡆⢀⠀⠀⠀⠀⠀⠀⠀⠘⢿⣿⣿⠟⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢠⣤⣿⡟⠀⠀⠀⠀⠀⠀⠀⠹⣿⡆⠀
⣼⡿⠁⠀⠀⠀⠀⠀⠀⠀⠀⣸⣷⣿⣷⣧⠀⢀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣄⠀⢠⡾⣠⣇⣠⣿⣿⣿⡇⠀⢀⠀⠀⠀⢀⠀⠀⢹⣷⠀
⣿⣷⡀⠀⣷⠀⠀⠀⣼⣦⣴⣿⠏⠙⠻⠿⣷⡿⠷⣶⣶⡾⠿⠿⠷⢶⣶⣦⣤⣾⣿⣷⣿⣿⠿⠿⠛⠛⠙⠻⣿⣤⣾⣇⠀⢀⣸⣇⣀⣼⣿⠃
⠘⢿⣿⣾⣿⣷⣴⣾⡿⠟⠋⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠉⠉⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠙⠛⠻⠿⠿⠿⠟⠛⠛⠁⠀

🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕

*/
// ------------------------------------------------------------------
public class Woff : Bot
{
    // Knobs
    private readonly static double  ENEMY_ENERGY_THRESHOLD = 1.3;
    private readonly static double  MOVE_WALL_MARGIN = 25;
    private readonly static double  GUN_FACTOR = 5;
    private readonly static double  MIN_ENERGY = 12;
    private readonly static double  RADAR_LOCK = 0.7;
    private readonly static double  MIN_RADIUS = 200;
    private readonly static double  MAX_RADIUS = 300;
    private readonly static double  POINT_COUNT = 36;
    private readonly static double  MIN_DIVISOR = 1e-6;
    private readonly static int     NGRAM_ORDER = 4;
    private readonly static int     BULLET_OFFSET_ARENA = 50;
    private readonly static int     ENEMY_GRAVITY_CONSTANT = 300;
    private readonly static int     BULLET_GRAVITY_CONSTANT = 10;
    private readonly static int     LAST_LOC_GRAVITY_CONSTANT = 10;

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
        Console.WriteLine("Woff woff woff 🐶! |---| round: " + RoundNumber);
        RadarColor = Color.White;
        TracksColor = Color.White;
        GunColor = Color.White;

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
        ScanColor = Color.FromArgb(105, 105, rand.Next(256));
        BodyColor = ScanColor;
        BulletColor = ScanColor;

        var g = Graphics;
        for (int i = bullets.Count - 1; i >= 0; i--)
        {
            Bullet bullet = bullets[i];
            bullet.X += bullet.Speed * Math.Cos(bullet.Direction);
            bullet.Y += bullet.Speed * Math.Sin(bullet.Direction);
            g.FillRectangle(Brushes.Black, (float)bullet.X, (float)bullet.Y, (float)(3 * bullet.Power), (float)(3 * bullet.Power));
            // Console.WriteLine("BulletId: " + i + " X: " + bullet.X + " Y: " + bullet.Y);

            if (bullet.X < 0 - BULLET_OFFSET_ARENA || bullet.X > ArenaWidth + BULLET_OFFSET_ARENA || 
                bullet.Y < 0 - BULLET_OFFSET_ARENA || bullet.Y > ArenaHeight + BULLET_OFFSET_ARENA)
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

        // Input State
        double currentSpeed = e.Speed;
        double acceleration = data.HasPrevious ? currentSpeed - data.LastSpeed : 0;
        data.LastSpeed = currentSpeed;
        double angularVelocity = data.HasPrevious ? (currentDirection - data.LastDirection + Math.PI) % (2 * Math.PI) - Math.PI : 0;
        data.LastDirection = currentDirection;
        State currentState = new State(angularVelocity, currentSpeed, acceleration);
        data.StateHistory.Add(currentState);

        if (data.StateHistory.Count >= NGRAM_ORDER)
        {
            List<State> contextStates = data.StateHistory.GetRange(data.StateHistory.Count - (NGRAM_ORDER - 1), NGRAM_ORDER - 1);
            StateSequence contextKey = new StateSequence(contextStates);
            if (!data.NgramTree.ContainsKey(contextKey))
            {
                data.NgramTree[contextKey] = new TransitionSegmentTree();
            }
            data.NgramTree[contextKey].Add(currentState);
        }
        data.HasPrevious = true;

        // --- Play It Forward ---
        double predictedX = e.X;
        double predictedY = e.Y;
        double predictedDirection = currentDirection;
        double predictedSpeed = currentSpeed;
        double simAngularVelocity = angularVelocity;
        State simCurrentState = currentState;
        int time = 0;

        List<State> simContext = null;
        if (data.StateHistory.Count >= NGRAM_ORDER - 1)
        {
            simContext = new List<State>(data.StateHistory.GetRange(data.StateHistory.Count - (NGRAM_ORDER - 1), NGRAM_ORDER - 1));
        }

        while (time * bulletSpeed < DistanceTo(predictedX, predictedY) && time < 100)
        {
            if (simContext != null)
            {
                StateSequence simContextKey = new StateSequence(simContext);
                if (data.NgramTree.ContainsKey(simContextKey))
                {
                    State nextState = data.NgramTree[simContextKey].GetMostFrequent();
                    simAngularVelocity = nextState.AngularVelocity / 1024.0;
                    predictedSpeed += nextState.Acceleration;
                    simContext.RemoveAt(0);
                    simContext.Add(nextState);
                }
            }
            predictedDirection += simAngularVelocity;
            predictedX += predictedSpeed * Math.Cos(predictedDirection);
            predictedY += predictedSpeed * Math.Sin(predictedDirection);
            time++;
        }

        // Bullet's Wall Avoidance
        predictedX = Math.Max(MOVE_WALL_MARGIN, Math.Min(ArenaWidth - MOVE_WALL_MARGIN, predictedX));
        predictedY = Math.Max(MOVE_WALL_MARGIN, Math.Min(ArenaHeight - MOVE_WALL_MARGIN, predictedY));

        var g = Graphics;
        Pen redPen = new Pen(Brushes.Red);
        g.DrawRectangle(redPen, (float)predictedX, (float)predictedY, 20, 20);
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

    // --- Helper Functions ---
    private double CalcRisk(double candidateX, double candidateY)
    {
        double risk = 0;

        foreach (EnemyData enemy in enemyData.Values)
        {
            if (enemy.IsAlive)
            {
                risk += ENEMY_GRAVITY_CONSTANT * (enemy.LastEnergy - ENEMY_ENERGY_THRESHOLD) / 
                        (distanceSq(candidateX, candidateY, enemy.LastX, enemy.LastY) + MIN_DIVISOR);
            }
        }

        foreach (Bullet bullet in bullets)
        {
            Line2D bulletLine = new Line2D(
                bullet.X - Math.Cos(bullet.Direction) * 10000, 
                bullet.Y - Math.Sin(bullet.Direction) * 10000, 
                bullet.X + Math.Cos(bullet.Direction) * 10000, 
                bullet.Y + Math.Sin(bullet.Direction) * 10000
            );
            
            double d = bulletLine.DistanceToPoint(candidateX, candidateY);
            risk += BULLET_GRAVITY_CONSTANT * bullet.Power / (d * d + MIN_DIVISOR);

        }

        risk += LAST_LOC_GRAVITY_CONSTANT * rand.NextDouble() / 
                (Math.Pow(DistanceTo(candidateX, candidateY), 2) + MIN_DIVISOR);

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
        
        // Linear-nya karol
        double vb = CalcBulletSpeed(power);
        double myDir = Direction * Math.PI / 180;
        double vxt = Speed * Math.Cos(myDir);
        double vyt = Speed * Math.Sin(myDir);
        double xt = X;
        double yt = Y;
        double a = Math.Pow(vxt, 2) + Math.Pow(vyt, 2) - Math.Pow(vb, 2);
        double b = 2 * (vxt * (xt - x) + vyt * (yt - y));
        double c = Math.Pow(xt - x, 2) + Math.Pow(yt - y, 2);
        double d = Math.Pow(b, 2) - 4 * a * c;
        double t1 = (-b + Math.Sqrt(d)) / (2 * a);
        double t2 = (-b - Math.Sqrt(d)) / (2 * a);
        double t = Math.Max(t1, t2);
        double predictedX = xt + vxt * t;
        double predictedY = yt + vyt * t;
        double linearDirection = Math.Atan2(predictedY - y, predictedX - x);
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
    public int AngularVelocity; // quantized: radian * 1024
    public int Speed;           // -8 -- 8
    public int Acceleration;    // -1 -- 1

    public State(double angularVelocity, double speed, double acceleration)
    {
        AngularVelocity = (int)(angularVelocity * 1024);

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

public class StateSequence
{
    public List<State> States { get; }
    public StateSequence(IEnumerable<State> states)
    {
        States = new List<State>(states);
    }
    public override bool Equals(object obj)
    {
        if (obj is StateSequence seq)
        {
            if (States.Count != seq.States.Count)
                return false;
            for (int i = 0; i < States.Count; i++)
            {
                if (!States[i].Equals(seq.States[i]))
                    return false;
            }
            return true;
        }
        return false;
    }
    public override int GetHashCode()
    {
        int hash = 17;
        foreach (var s in States)
            hash = hash * 31 + s.GetHashCode();
        return hash;
    }
}

public class EnemyData
{
    public List<State> StateHistory { get; } = new List<State>();
    public Dictionary<StateSequence, TransitionSegmentTree> NgramTree { get; } = new Dictionary<StateSequence, TransitionSegmentTree>();
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

public class TransitionSegmentTree
{
    private List<KeyValuePair<State, int>> data;
    private int size;
    private (State state, int frequency)[] tree;
    private Dictionary<State, int> stateToIndex;

    public TransitionSegmentTree()
    {
        data = new List<KeyValuePair<State, int>>();
        stateToIndex = new Dictionary<State, int>();
        size = 0;
        tree = new (State, int)[0];
    }

    public void Add(State s)
    {
        if (stateToIndex.ContainsKey(s))
        {
            int idx = stateToIndex[s];
            var kvp = data[idx];
            data[idx] = new KeyValuePair<State, int>(s, kvp.Value + 1);
        }
        else
        {
            stateToIndex[s] = data.Count;
            data.Add(new KeyValuePair<State, int>(s, 1));
        }
        RebuildTree();
    }

    private void RebuildTree()
    {
        int n = data.Count;
        if (n == 0)
        {
            tree = new (State, int)[0];
            size = 0;
            return;
        }
        size = 1;
        while (size < n) size *= 2;
        tree = new (State, int)[2 * size];
        for (int i = 0; i < size; i++)
        {
            if (i < n)
            {
                tree[size + i] = (data[i].Key, data[i].Value);
            }
            else
            {
                tree[size + i] = (default(State), 0);
            }
        }
        for (int i = size - 1; i > 0; i--)
        {
            var left = tree[2 * i];
            var right = tree[2 * i + 1];
            tree[i] = left.frequency >= right.frequency ? left : right;
        }
    }

    public State GetMostFrequent()
    {
        if (tree.Length > 0)
        {
            return tree[1].state;
        }
        return default(State);
    }
}