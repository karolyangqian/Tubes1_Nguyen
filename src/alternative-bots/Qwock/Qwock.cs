using System;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

// ------------------------------------------------------------------
// Qwock
// ------------------------------------------------------------------
// Targeting: Play It Forward
// Movement: Risk function experiment
// ------------------------------------------------------------------
/*



*/
// ------------------------------------------------------------------
public class Qwock : Bot
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
    static double goalX;
    static double goalY;
    static bool goalReached;
    static bool enemyDetected;

    Random rand = new Random();

    static Dictionary<int, EnemyData> enemyData = new Dictionary<int, EnemyData>();

    static void Main()
    {
        new Qwock().Start();
    }

    Qwock() : base(BotInfo.FromFile("Qwock.json")) { }

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

        targetId = -1;
        goalReached = true;
        ram = false;
        targetDistance = double.PositiveInfinity;
        enemyDistance = double.PositiveInfinity;
        // SetGoal(ArenaWidth / 7, ArenaHeight / 7);
    }

    public override void OnTick(TickEvent e)
    {
        MinimumRiskMovement();
        // Oscillate();
        // Shoot at sniper
        if (targetId != -1 && enemyData[targetId].IsAlive)
        {
            // Approach(enemyData[targetId].LastX, enemyData[targetId].LastY);
            // if (DistanceTo(enemyData[targetId].LastX, enemyData[targetId].LastY) > 100 && goalReached)
            // {
            //     double newGoalX = enemyData[targetId].LastX + 100 * Math.Cos(Direction+180 * Math.PI / 180);
            //     double newGoalY = enemyData[targetId].LastY + 100 * Math.Sin(Direction+180 * Math.PI / 180);
            //     SetGoal(newGoalX, newGoalY);
            // }
            if (enemyDetected) TrackScanAt(enemyData[targetId].LastX, enemyData[targetId].LastY);
            else SetTurnRadarRight(20);
            double gunTurn = NormalizeRelativeAngle(GunBearingTo(enemyData[targetId].LastX, enemyData[targetId].LastY));
            double power = CalcFirePower(enemyData[targetId].LastX, enemyData[targetId].LastY);
            ShootPredict(enemyData[targetId].LastX, enemyData[targetId].LastY, enemyData[targetId].LastSpeed, enemyData[targetId].LastDirection, power);
        } else {
            SetTurnRadarRight(double.PositiveInfinity);
            targetId = findSniper();
        }


        // if (!goalReached)
        // {
        //     Approach(goalX, goalY);
        //     if (DistanceTo(goalX, goalY) < 50)
        //     {
        //         goalReached = true;
        //     }
        // } else {
        //     Oscillate();
        // }

        // Console.WriteLine(targetId);
        Console.WriteLine(enemyData.Count);
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        if (!enemyData.ContainsKey(e.ScannedBotId))
        {
            enemyData[e.ScannedBotId] = new EnemyData();
        }

        enemyData[e.ScannedBotId].LastDirection = e.Direction;
        enemyData[e.ScannedBotId].LastX = e.X;
        enemyData[e.ScannedBotId].LastY = e.Y;
        enemyData[e.ScannedBotId].LastEnergy = e.Energy;
        enemyData[e.ScannedBotId].LastSpeed = e.Speed;
        enemyDetected = true;
    }

    public override void OnBotDeath(BotDeathEvent e)
    {
        enemyData[e.VictimId].IsAlive = false;
        
        if (e.VictimId == targetId)
        {
            targetDistance = double.PositiveInfinity;
        }
    }

    private void TrackScanAt(double x, double y) {
        var bearingFromRadar = NormalizeRelativeAngle(RadarBearingTo(x, y));
        SetTurnRadarLeft(bearingFromRadar + (bearingFromRadar > 0 ? 20 : -20));
        enemyDetected = false;
    }

    private void SetGoal(double x, double y)
    {
        goalX = x;
        goalY = y;
        goalReached = false;
    }
    private int findSniper()
    {
        int sniper = -1;
        double sniperDistance = 0;

        double meanX = 0;
        double meanY = 0;
        double count = 0;

        foreach (var enemy in enemyData)
        {
            if (enemy.Value.IsAlive)
            {
                meanX += enemy.Value.LastX;
                meanY += enemy.Value.LastY;
            }
            count++;
        }

        meanX /= count > 0 ? count : 1;
        meanY /= count > 0 ? count : 1;

        foreach (var enemy in enemyData)
        {
            if (enemy.Value.IsAlive)
            {
                double distance = Math.Sqrt(Math.Pow(enemy.Value.LastX - meanX, 2) + Math.Pow(enemy.Value.LastY - meanY, 2));

                if (distance > sniperDistance)
                {
                    sniper = enemy.Key;
                    sniperDistance = distance;
                }
            }
        }


        return sniper;
    }

    private double CalcFirePower(double targetX, double targetY)
    {
        return Energy / DistanceTo(targetX, targetY) * GUN_FACTOR;
    }

    private void ShootPredict(double targetX, double targetY, double targetSpeed, double targetDirection, double firePower) {
        double bulletSpeed = CalcBulletSpeed(firePower);
        
        double enemyDir = targetDirection * Math.PI / 180.0;

        double time = DistanceTo(targetX, targetY) / bulletSpeed;
        
        double predictedX = targetX + targetSpeed * time * Math.Cos(enemyDir);
        double predictedY = targetY + targetSpeed * time * Math.Sin(enemyDir);
        
        double angleToPredicted = GunBearingTo(predictedX, predictedY);
        double angleToEnemy = GunBearingTo(targetX, targetY);
        double turn = angleToPredicted > angleToEnemy ? angleToPredicted - 5 : angleToPredicted + 5;
        
        SetFire(firePower);
        SetTurnGunLeft(turn);
    }

    private void Approach(double targetX, double targetY)
    {
        // Minimum Risk Movement
        if (DistanceRemaining < MOVE_PADDING) 
        {
            double bestX = X;
            double bestY = Y;
            double minDist = double.PositiveInfinity;

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

                double dist = Math.Sqrt(Math.Pow(targetX - x, 2) + Math.Pow(targetY - y, 2));
                if (dist < minDist)
                {
                    minDist = dist;
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
        Console.WriteLine("Approaching " + destX + " " + destY);
    }

    private void MinimumRiskMovement()
    {
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
        Console.WriteLine("Approaching " + destX + " " + destY);
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

    private void Oscillate()
    {
        TargetSpeed = 5 + 2 * Math.Sin(DateTime.UtcNow.Millisecond/1000);
        TurnRate = 10 +  2 * Math.Sin(DateTime.UtcNow.Millisecond/1000);
    }
}


public class EnemyData
{
    public double LastDirection { get; set; }
    public double LastX { get; set; }
    public double LastY { get; set; }
    public double LastEnergy { get; set; }
    public double LastSpeed { get; set; }
    public bool IsAlive { get; set; } = true;
}
