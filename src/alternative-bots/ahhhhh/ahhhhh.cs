using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

// ------------------------------------------------------------------
// Ahhhhh
// ------------------------------------------------------------------
// Targeting: Linear Targeting
// Movement: Ngejar
// ------------------------------------------------------------------
public class Ahhhhh : Bot
{
    static int MOVE_WALL_MARGIN = 25;

    private const double MAX_SHOOT_RANGE_THRESH = 600;
    private double scannedEnemyX;
    private double scannedEnemyY;
    private double scannedEnemySpeed;
    private double scannedEnemyDirection;
    private bool enemyDetected;

    static int moveDir = 1;

    static double gradient;
    
    static void Main()
    {
        new Ahhhhh().Start();
    }

    Ahhhhh() : base(BotInfo.FromFile("ahhhhh.json")) { }

    public override void Run()
    {
        Console.WriteLine("Hello! I'm Ahhhhh!");
        
        BodyColor = Color.Pink;
        TurretColor = Color.Red;
        RadarColor = Color.Pink;
        BulletColor = Color.Red;
        ScanColor = Color.Red;

        AdjustGunForBodyTurn = true;
        gradient = (double) ArenaHeight / ArenaWidth;
        SetTurnRadarLeft(double.PositiveInfinity);

        QuackDriverV1();
    } 

    public void QuackDriverV1() {
        while (IsRunning)
        {
            if (enemyDetected) {
                TrackScanAt(scannedEnemyX, scannedEnemyY);
                ShootPredict(scannedEnemyX, scannedEnemyY, scannedEnemySpeed, scannedEnemyDirection, (Math.Sqrt(ArenaHeight * ArenaHeight + ArenaWidth * ArenaWidth)) / DistanceTo(scannedEnemyX, scannedEnemyY) * 0.15);
                enemyDetected = false;
            } else {
                TurnRadarLeft(20);
            }
        }
    }

    public override void OnScannedBot(ScannedBotEvent e) {
        // Console.WriteLine("\nI see a bot!");
        // Console.WriteLine("Id       : " + e.ScannedBotId);
        // Console.WriteLine("Energy   : " + e.Energy);
        // Console.WriteLine("Direction: " + e.Direction);
        // Console.WriteLine("Speed    : " + e.Speed);
        // Console.WriteLine("X        : " + e.X);
        // Console.WriteLine("Y        : " + e.Y);
        // Console.WriteLine("Distance : " + DistanceTo(e.X, e.Y));

        SetTurnLeft(NormalizeRelativeAngle(BearingTo(e.X, e.Y)));
        SetForward(DistanceTo(e.X, e.Y));

        scannedEnemyX = e.X;
        scannedEnemyY = e.Y;
        scannedEnemySpeed = e.Speed;
        scannedEnemyDirection = e.Direction;
        enemyDetected = true;
    }
    
    private void TrackScanAt(double x, double y) {
        var bearingFromRadar = RadarBearingTo(x, y);
        TurnRadarLeft(bearingFromRadar + 10);
        bearingFromRadar = RadarBearingTo(x, y);
        TurnRadarLeft(bearingFromRadar - 10);
    }

    private void ShootPredict(double targetX, double targetY, double targetSpeed, double targetDirection, double firePower) {
        double bulletSpeed = CalcBulletSpeed(firePower);
        
        double dx = targetX - X;
        double dy = targetY - Y;
        
        double enemyDir = targetDirection * Math.PI / 180.0;
        
        double distance = Math.Sqrt(dx * dx + dy * dy);
        double time = distance / bulletSpeed;
        
        double predictedX = targetX + targetSpeed * time * Math.Cos(enemyDir);
        double predictedY = targetY + targetSpeed * time * Math.Sin(enemyDir);
        
        double bearingFromGun = GunBearingTo(predictedX, predictedY);
        
        TurnGunLeft(bearingFromGun);
        Fire(firePower);
    }
}