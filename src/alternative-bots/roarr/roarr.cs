using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

// ------------------------------------------------------------------
// Roarr
// ------------------------------------------------------------------
// Targeting: Play It Forward
// Movement: Corner 
// ------------------------------------------------------------------
/*


*/
// ------------------------------------------------------------------
public class Roarr : Bot
{
    // Constants
    static double AIM_WALL_MARGIN = 17.5;
    static int MOVE_WALL_MARGIN = 25;
    static int FIRE_ANGLES = 1000;
    static int TABLE_SIZE = 126;
    static int OPPONENT_HASHES = 256;

    // Aim data
    static int[,,] markovTransitionTable = new int[OPPONENT_HASHES,579,TABLE_SIZE + 1];

    // Opponent data
    static int targetId;
    static double targetDistance = double.PositiveInfinity;
    static double targetVelocity;
    static double targetHeading;
    static int targetMarkovState;

    // Our data
    static double myX;
    static double myY;
    static int moveDir = 1;

    static void Main()
    {
        new Roarr().Start();
    }

    Roarr() : base(BotInfo.FromFile("roarr.json")) { }

    public override void Run()
    {
        Console.WriteLine("Hello! I'm Roarr!");
        
        BodyColor = Color.Red;
        TurretColor = Color.Yellow;
        RadarColor = Color.Red;
        BulletColor = Color.Red;
        ScanColor = Color.Black;

        SetTurnRadarRight(double.PositiveInfinity);
        AdjustGunForBodyTurn = true;

    public override void OnTick(TickEvent e) {
        // Console.WriteLine("GunHeat: " + GunHeat + " Energy: " + Energy);

        // Corner Movement
        int x = MOVE_WALL_MARGIN + (int) (targetDistance / 2.7);
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

        targetDistance = DistanceTo(e.X, e.Y);
        SetTurnLeft(180 / Math.PI * Math.Tan(turn));
        SetForward(DistanceTo(x, y) * Math.Cos(turn));
    }

    public override void OnScannedBot(ScannedBotEvent e) {

        if (GunHeat < 1) 
        {
            SetTurnRadarLeft(double.PositiveInfinity * NormalizeRelativeAngle(RadarBearingTo(e.X, e.Y)));
        }

        // Targeting
        double firePower = (Math.Sqrt(ArenaHeight * ArenaHeight + ArenaWidth * ArenaWidth)) / DistanceTo(e.X, e.Y) * 0.3;

        if (GunTurnRemaining == 0)
        {
            SetFire(firePower);
        }

        double bulletSpeed = CalcBulletSpeed(firePower);
        
        double absBearing = Math.Atan2(e.Y - Y, e.X - X);
        
        double enemyDir = e.Direction * Math.PI / 180.0;
        
        double ratio = Math.Max(-1, Math.Min(1, (e.Speed * Math.Sin(enemyDir - absBearing)) / bulletSpeed));
        
        double gunDirection = absBearing + Math.Asin(ratio);
        
        double time = DistanceTo(e.X, e.Y) / bulletSpeed;
        
        double predictedX = e.X + e.Speed * time * Math.Cos(enemyDir);
        double predictedY = e.Y + e.Speed * time * Math.Sin(enemyDir);
        
        double bearingFromGun = GunBearingTo(predictedX, predictedY);

        SetTurnGunLeft(bearingFromGun);
    }

    public override void OnBotDeath(BotDeathEvent e)
    {
        SetTurnRadarRight(targetDistance = double.PositiveInfinity);
    }
}