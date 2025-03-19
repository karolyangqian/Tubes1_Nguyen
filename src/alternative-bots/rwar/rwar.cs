using System;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

// ------------------------------------------------------------------
// rwar
// ------------------------------------------------------------------
// Targeting: Play It Forward
// Movement: Minimum Risk Movement
// ------------------------------------------------------------------
/*



*/
// ------------------------------------------------------------------
public class Rwar : Bot
{
    // Knobs
    private readonly static double  MOVE_WALL_MARGIN = 25;
    private readonly static double  GUN_FACTOR = 10;
    private readonly static double  MIN_ENERGY = 10;
    private readonly static double  ENEMY_ENERGY_THRESHOLD = 1;
    private readonly static double  RADAR_LOCK = 0.7;
    private readonly static double  MIN_RADIUS = 80;
    private readonly static double  MAX_RADIUS = 200;
    private readonly static double  POINT_COUNT = 36;
    private readonly static double  MOVE_PADDING = 10;
    private readonly static int     MAX_DATA = 100;

    // Global variables
    static bool ram;
    static double ramX;
    static double ramY;
    static int targetId;
    static double targetDistance;
    static double enemyDistance;

    static double destX;
    static double destY;

    Random rand = new Random();

    static Dictionary<int, EnemyData> enemyData = new Dictionary<int, EnemyData>();

    static void Main()
    {
        new Rwar().Start();
    }

    Rwar() : base(BotInfo.FromFile("rwar.json")) { }

    public override void Run()
    {
        BodyColor = Color.Red;
        TurretColor = Color.Yellow;
        RadarColor = Color.Orange;
        BulletColor = Color.Red;
        ScanColor = Color.Orange;

        SetTurnRadarRight(double.PositiveInfinity);
        AdjustGunForBodyTurn = true;
        AdjustRadarForGunTurn = true;
        AdjustRadarForBodyTurn = true;

        ram = false;
        targetDistance = double.PositiveInfinity;
        enemyDistance = double.PositiveInfinity;
    }

    public override void OnTick(TickEvent e)
    {
        TurretColor = Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256));

        // GOOO!!!
        if (ram)
        {
            ScanColor = Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256));
            SetTurnLeft(NormalizeRelativeAngle(BearingTo(ramX, ramY)));
            SetForward(DistanceTo(ramX, ramY));
            return;
        }

        // Minimum Risk Movement
        if (DistanceRemaining < MOVE_PADDING) 
        {
            double bestX = X;
            double bestY = Y;
            double minRisk = double.PositiveInfinity;

            for (int i = 0; i < POINT_COUNT; i++)
            {
                double theta = (2 * Math.PI / POINT_COUNT) * i;
                
                double u = rand.NextDouble();
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

            destX = bestX;
            destY = bestY;
        }

        double turn = BearingTo(destX, destY) * Math.PI / 180;
        SetTurnLeft(180 / Math.PI * Math.Tan(turn));
        SetForward(DistanceTo(destX, destY) * Math.Cos(turn));
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        // Ram toggle
        if (ram || (EnemyCount == 1 && e.Energy < ENEMY_ENERGY_THRESHOLD))
        {
            ram = true;
            ramX = e.X;
            ramY = e.Y;
        }

        // Update enemy data
        if (!enemyData.ContainsKey(e.ScannedBotId))
        {
            enemyData[e.ScannedBotId] = new EnemyData();
        }
        EnemyData data = enemyData[e.ScannedBotId];
        data.LastX = e.X;
        data.LastY = e.Y;
        data.IsAlive = true;
        data.LastEnergy = e.Energy;

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
        double enemySpeed = e.Speed;

        // Input Markov Chain
        double angularVelocity = 0;
        if (data.HasPrevious)
        {
            angularVelocity = (currentDirection - data.LastDirection + Math.PI) % (2 * Math.PI) - Math.PI;
        }
        data.LastDirection = currentDirection;
        data.HasPrevious = true;

        State currentState = new State(angularVelocity);
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
        double simAngularVelocity = angularVelocity;
        State simCurrentState = currentState;
        int time = 0;
        while (time * bulletSpeed < DistanceTo(predictedX, predictedY))
        {
            if (data.MarkovChain.ContainsKey(simCurrentState) && data.MarkovChain[simCurrentState].Count > 0)
            {
                State nextState = GetMostFrequentTransition(data.MarkovChain[simCurrentState]);
                simAngularVelocity = nextState.AngularVelocity / 1000.0;
                simCurrentState = nextState;
            }
            predictedDirection += simAngularVelocity;
            predictedX += enemySpeed * Math.Cos(predictedDirection);
            predictedY += enemySpeed * Math.Sin(predictedDirection);
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

    // --- Helper Functions ---
    private State GetMostFrequentTransition(List<State> transitions)
    {
        Dictionary<State, int> frequency = new Dictionary<State, int>();
        foreach (State state in transitions)
        {
            if (frequency.ContainsKey(state))
                frequency[state]++;
            else
                frequency[state] = 1;
        }
        State bestState = transitions[0];
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
                double energyFactor = enemy.LastEnergy;

                double distanceSq = Math.Pow(candidateX - enemy.LastX, 2) + Math.Pow(candidateY - enemy.LastY, 2);
                if (distanceSq < 1e-6)
                    distanceSq = 1e-6;

                risk += energyFactor / distanceSq;
            }
        }

        return risk;
    }

}

public struct State
{
    public int AngularVelocity; // quantized: radian * 1000

    public State(double angularVelocity)
    {
        AngularVelocity = (int)(angularVelocity * 1000);
    }

    public override bool Equals(object obj)
    {
        if (obj is State state)
        {
            return state.AngularVelocity == AngularVelocity;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return AngularVelocity.GetHashCode();
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
    public bool IsAlive { get; set; } = true;
}
