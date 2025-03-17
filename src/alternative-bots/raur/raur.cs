using System;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

// ------------------------------------------------------------------
// Raur
// ------------------------------------------------------------------
// Targeting: Circular
// Movement: Corner 
// ------------------------------------------------------------------
/*

*/
// ------------------------------------------------------------------
public class Raur : Bot
{
    static int MOVE_WALL_MARGIN = 25;

    static int moveDir = 1;

    static int targetId;
    static double targetDistance = double.PositiveInfinity;
    static double enemyDistance = double.PositiveInfinity;

    List<double> directionHistory = new List<double>();


    static void Main()
    {
        new Raur().Start();
    }

    Raur() : base(BotInfo.FromFile("raur.json")) { }

    public override void Run()
    {
        Console.WriteLine("Hello! I'm raur!");
        
        BodyColor = Color.Red;
        TurretColor = Color.Yellow;
        RadarColor = Color.Red;
        BulletColor = Color.Red;
        ScanColor = Color.Red;

        SetTurnRadarRight(double.PositiveInfinity);
        AdjustGunForBodyTurn = true;
    } 

    public override void OnTick(TickEvent e) {
        // Corner Movement
        int x = MOVE_WALL_MARGIN + (int) (enemyDistance / 3);
        int y = MOVE_WALL_MARGIN;

        if (DistanceRemaining == 0) 
        {
            moveDir = -moveDir;
        }

        if (moveDir > 0) 
        {
            y = x;
            x = MOVE_WALL_MARGIN;
        }

        if (X > ArenaWidth / 2)
        {
            x = (int) (ArenaWidth - x);
        }

        if (Y > ArenaHeight / 2)
        {
            y = (int) (ArenaHeight - y);
        }

        double turn = BearingTo(x, y) * Math.PI / 180;
        SetTurnLeft(180 / Math.PI * Math.Tan(turn));
        SetForward(DistanceTo(x, y) * Math.Cos(turn));
    }

    public override void OnScannedBot(ScannedBotEvent e) {
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

        if (!double.IsNaN(radarAngle) && (GunHeat < 1 || EnemyCount == 1)) 
        {
            SetTurnRadarLeft(radarAngle);
        }

        // Aim
        double firePower = Energy / DistanceTo(e.X, e.Y) * 10;

        if (GunTurnRemaining == 0)
        {
            SetFire(firePower);
        }

        double bulletSpeed = CalcBulletSpeed(firePower);
        double enemyDir = e.Direction * Math.PI / 180.0;

        directionHistory.Add(enemyDir);

        if (directionHistory.Count > 5)
        {
            directionHistory.RemoveAt(0);
        }

        double angularVelocity = 0;
        if (directionHistory.Count >= 2)
        {
            double totalChange = 0;
            for (int i = 1; i < directionHistory.Count; i++)
            {
                double delta = directionHistory[i] - directionHistory[i - 1];
                delta = (delta + Math.PI) % (2 * Math.PI) - Math.PI;
                totalChange += delta;
            }
            angularVelocity = totalChange / (directionHistory.Count - 1);
        }

        double predictedX = e.X;
        double predictedY = e.Y;

        int time = 0;
        while (time++ * bulletSpeed < DistanceTo(predictedX, predictedY))
        {
            enemyDir += angularVelocity;
            predictedX += e.Speed * Math.Cos(enemyDir);
            predictedY += e.Speed * Math.Sin(enemyDir);
        }

        // Handle wall targeting
        if (predictedX < 0)
        {
            predictedX -= 1;
        } else if (predictedX > ArenaWidth)
        {
            predictedX = 2 * ArenaWidth - predictedX;
        }

        if (predictedY < 0)
        {
            predictedY -= 1;
        } else if (predictedY > ArenaHeight)
        {
            predictedY = 2 * ArenaHeight - predictedY;
        }

        double bearingFromGun = GunBearingTo(predictedX, predictedY);

        SetTurnGunLeft(bearingFromGun);
    }

    public override void OnBotDeath(BotDeathEvent e) {
        if (e.VictimId == targetId)
        {
            targetDistance = double.PositiveInfinity;
        }
    }
}