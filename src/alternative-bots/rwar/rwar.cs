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
public class rwar : Bot
{
    // Knobs
    private readonly static double  MOVE_WALL_MARGIN = 25;
    private readonly static double  GUN_FACTOR = 10;
    private readonly static double  MIN_ENERGY = 10;
    private readonly static double  ENEMY_ENERGY_THRESHOLD = 1;
    private readonly static double  RADAR_LOCK = 0.7;
    private readonly static double  MAX_RADIUS = 100;
    private readonly static int     MAX_DATA = 100;

    // Global variables
    static int moveDir;
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
        new rwar().Start();
    }

    rwar() : base(BotInfo.FromFile("rwar.json")) { }

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
        moveDir = 1;
        targetDistance = double.PositiveInfinity;
        enemyDistance = double.PositiveInfinity;
    }

    public override void OnTick(TickEvent e)
    {
        // GOOO!!!
        if (ram)
        {
            SetTurnLeft(NormalizeRelativeAngle(BearingTo(ramX, ramY)));
            SetForward(DistanceTo(ramX, ramY));
            return;
        }

        // Minimum Risk Movement
        if (DistanceRemaining < 15) 
        {
            moveDir = -moveDir;

            double bestX = X;
            double bestY = Y;
            double minRisk = double.PositiveInfinity;

            for (int i = 0; i < 200; i++)
            {
                double r = MAX_RADIUS * Math.Sqrt(rand.NextDouble());
                double theta = rand.NextDouble() * 2 * Math.PI;
                double x = X + r * Math.Cos(theta);
                double y = Y + r * Math.Sin(theta);

                if (x < MOVE_WALL_MARGIN || x > ArenaWidth - MOVE_WALL_MARGIN || y < MOVE_WALL_MARGIN || y > ArenaHeight - MOVE_WALL_MARGIN)
                {
                    continue;
                }

                double risk = CalcRick(x, y);
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

        // Ram toggle
        if (ram || (e.Energy < ENEMY_ENERGY_THRESHOLD && EnemyCount == 1))
        {
            ram = true;
            ramX = e.X;
            ramY = e.Y;
        }

        // Radar 
        double radarAngle = NormalizeRelativeAngle(RadarBearingTo(e.X, e.Y));
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
        double currentDirection = e.Direction * Math.PI / 180.0;  // arah musuh dalam radian
        double enemySpeed = e.Speed;

        // Input EnemyData
        if (!enemyData.ContainsKey(e.ScannedBotId))
        {
            enemyData[e.ScannedBotId] = new EnemyData();
        }
        EnemyData data = enemyData[e.ScannedBotId];

        double angularVelocity = 0;
        if (data.HasPrevious)
        {
            angularVelocity = (currentDirection - data.LastDirection + Math.PI) % (2 * Math.PI) - Math.PI;

        }
        data.LastDirection = currentDirection;
        data.HasPrevious = true;

        MarkovState currentState = new MarkovState(angularVelocity);
        data.StateHistory.Add(currentState);
        if (data.StateHistory.Count > MAX_DATA)
        {
            data.StateHistory.RemoveAt(0);
        }
        if (data.StateHistory.Count >= 2)
        {
            MarkovState previousState = data.StateHistory[data.StateHistory.Count - 2];
            if (!data.MarkovChain.ContainsKey(previousState))
            {
                data.MarkovChain[previousState] = new List<MarkovState>();
            }
            data.MarkovChain[previousState].Add(currentState);
        }

        data.LastX = e.X;
        data.LastY = e.Y;
        data.LastEnergy = e.Energy;

        // --- Play It Forward ---
        double predictedX = e.X;
        double predictedY = e.Y;
        double predictedDirection = currentDirection;
        double simAngularVelocity = angularVelocity;
        MarkovState simCurrentState = currentState;
        int time = 0;
        while (time * bulletSpeed < DistanceTo(predictedX, predictedY))
        {
            if (data.MarkovChain.ContainsKey(simCurrentState) && data.MarkovChain[simCurrentState].Count > 0)
            {
                MarkovState nextState = GetMostFrequentTransition(data.MarkovChain[simCurrentState]);
                simAngularVelocity = nextState.AngularVelocity / 1000.0;
                simCurrentState = nextState;
            }
            predictedDirection += simAngularVelocity;
            predictedX += enemySpeed * Math.Cos(predictedDirection);
            predictedY += enemySpeed * Math.Sin(predictedDirection);
            time++;
        }

        // Wall smoothing
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
    private MarkovState GetMostFrequentTransition(List<MarkovState> transitions)
    {
        Dictionary<MarkovState, int> frequency = new Dictionary<MarkovState, int>();
        foreach (var state in transitions)
        {
            if (frequency.ContainsKey(state))
                frequency[state]++;
            else
                frequency[state] = 1;
        }
        MarkovState bestState = transitions[0];
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

    private double CalcRick(double x, double y)
    {
        double risk = 0;

        foreach (var enemy in enemyData.Values)
        {
            if (enemy.IsAlive) 
            {
                risk += Math.Min(2, enemy.LastEnergy / Energy)
                        * (1 + Math.Abs(Math.Cos((BearingTo(enemy.LastX, enemy.LastY) - BearingTo(x, y)) * Math.PI / 180)))
                        / DistanceTo(enemy.LastX, enemy.LastY);
            }
        }

        return risk;
    }
}

public struct MarkovState
{
    public int AngularVelocity; // quantized: radian * 1000

    public MarkovState(double angularVelocity)
    {
        AngularVelocity = (int)(angularVelocity * 1000);
    }

    public override bool Equals(object obj)
    {
        if (obj is MarkovState state)
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
    public List<MarkovState> StateHistory { get; } = new List<MarkovState>();
    public Dictionary<MarkovState, List<MarkovState>> MarkovChain { get; } = new Dictionary<MarkovState, List<MarkovState>>();
    public double LastDirection { get; set; }
    public bool HasPrevious { get; set; } = false;

    public double LastX { get; set; }
    public double LastY { get; set; }
    public double LastEnergy { get; set; }
    public bool IsAlive { get; set; } = true;
}
