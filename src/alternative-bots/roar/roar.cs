using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

// ------------------------------------------------------------------
// Roar
// ------------------------------------------------------------------
// Targetting: Play It Forward
// Movement: Corner 
// ------------------------------------------------------------------
public class Roar : Bot
{
    static int MOVE_WALL_MARGIN = 25;

    static int moveDir = 1;

    static double gradient;
    
    static void Main()
    {
        new Roar().Start();
    }

    Roar() : base(BotInfo.FromFile("roar.json")) { }

    public override void Run()
    {
        Console.WriteLine("Hello! I'm Roar!");
        
        BodyColor = Color.Red;
        TurretColor = Color.Red;
        RadarColor = Color.Black;
        BulletColor = Color.Black;
        ScanColor = Color.Black;

        SetTurnRadarRight(double.PositiveInfinity);
        AdjustGunForBodyTurn = true;
        gradient = (double) ArenaHeight / ArenaWidth;
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

        // Corner Movement
        int x, y;
        
        if (X * 2 < ArenaWidth && Y * 2 < ArenaHeight)
        {
            if (Y > gradient * X)
            {
                Console.WriteLine("\n 1");
                moveDir = 1;
                x = ArenaWidth / 4;
                y = MOVE_WALL_MARGIN;
            }
            else
            {
                Console.WriteLine("\n 2");
                moveDir = -1;
                x = MOVE_WALL_MARGIN;
                y = ArenaHeight / 4;
            }
        }
        else if (X * 2 < ArenaWidth && Y * 2 > ArenaHeight)
        {
            if (Y > ArenaHeight - gradient * X)
            {
                Console.WriteLine("\n 3");
                moveDir = -1;
                x = MOVE_WALL_MARGIN;
                y = ArenaHeight * 3 / 4;
            }
            else
            {
                Console.WriteLine("\n 4");
                moveDir = 1;
                x = ArenaWidth / 4;
                y = ArenaHeight - MOVE_WALL_MARGIN;
            }
        }
        else if (X * 2 > ArenaWidth && Y * 2 < ArenaHeight)
        {
            if (Y > ArenaHeight - gradient * X)
            {
                Console.WriteLine("\n 5");
                moveDir = 1;
                x = ArenaWidth * 3 / 4;
                y = MOVE_WALL_MARGIN;
            }
            else
            {
                Console.WriteLine("\n 6");
                moveDir = -1;
                x = ArenaWidth - MOVE_WALL_MARGIN;
                y = ArenaHeight / 4;
            }
        }
        else
        {
            if (Y > gradient * X)
            {
                Console.WriteLine("\n 7");
                moveDir = 1;
                x = ArenaWidth - MOVE_WALL_MARGIN;
                y = ArenaHeight * 3 / 4;
            }
            else
            {
                Console.WriteLine("\n 8");
                moveDir = -1;
                x = ArenaWidth * 3 / 4;
                y = ArenaHeight - MOVE_WALL_MARGIN;
            }
        }

        Console.WriteLine("myX: " + X + " myY: " + Y);
        Console.WriteLine("target x: " + x + " target y: " + y);
        Console.WriteLine("DistanceRemaining: " + DistanceRemaining);

        SetTurnLeft(moveDir * NormalizeRelativeAngle(BearingTo(x, y)));
        SetForward(moveDir * DistanceTo(x, y));


        // Targetting
        double dx = e.X - X;
        double dy = e.Y - Y;
        double firePower = 1;
        double bulletSpeed = CalcBulletSpeed(firePower);
        
        double absBearing = Math.Atan2(dy, dx);
        
        double enemyDir = e.Direction * Math.PI / 180.0;
        
        double ratio = Math.Max(-1, Math.Min(1, (e.Speed * Math.Sin(enemyDir - absBearing)) / bulletSpeed));
        
        double gunDirection = absBearing + Math.Asin(ratio);
        
        double time = DistanceTo(e.X, e.Y) / bulletSpeed;
        
        double predictedX = e.X + e.Speed * time * Math.Cos(enemyDir);
        double predictedY = e.Y + e.Speed * time * Math.Sin(enemyDir);
        
        double bearingFromGun = GunBearingTo(predictedX, predictedY);

        TurnGunLeft(bearingFromGun);
        Fire(firePower);
    }
}